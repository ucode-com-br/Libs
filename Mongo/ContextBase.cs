using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using UCode.Extensions;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace UCode.Mongo
{

    /// <summary>
    /// Base class for MongoDB context. Provides methods for starting sessions, starting transactions, committing transactions, and aborting transactions.
    /// </summary>
    public abstract class ContextBase : IDisposable
    {
        // Lock for synchronizing access to _collectionNames
        private static readonly object _collectionNamesLock = new();

        // Collection names
        private static System.Collections.Concurrent.ConcurrentBag<string> _collectionNames;

        internal readonly ILoggerFactory LoggerFactory;
        internal readonly MongoClient Client;
        internal readonly MongoDatabaseBase Database;
        internal IClientSessionHandle? Session;
        private bool _onTransaction;
        private readonly object _onTransactionLock = new();
        public readonly string DatabaseName;
        private readonly bool SessionStatedOnConstructor;

        private readonly Lazy<ILogger<ContextBase>> _logger;
        protected ILogger<ContextBase> Logger => this._logger.Value;

        public bool IsUseTransaction
        {
            get; private set;
        }
        private bool SetUseTransactionOnConstructor
        {
            get; set;
        }
        private bool _disposedValue;

        public delegate void EventHandler<TEvent>(object sender, MongoEventArgs<TEvent> args);
        public event EventHandler Event;

        public virtual void OnEvent<TEvent>(TEvent ev) => Event?.Invoke(this, new MongoEventArgs<TEvent>(ev));

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextBase"/> class.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="connectionString"></param>
        /// <param name="applicationName"></param>
        /// <param name="forceTransaction"></param>
        protected ContextBase([NotNull] ILoggerFactory loggerFactory, [NotNull] string connectionString, string? applicationName = null, bool forceTransaction = true)
        {
            // Initialize LoggerFactory
            this.LoggerFactory = loggerFactory;

            this._logger = new Lazy<ILogger<ContextBase>>(() => loggerFactory.CreateLogger<ContextBase>());

            // Initialize Client
            var mongoClientSettings = MongoClientSettings.FromConnectionString(connectionString);

            //mongoClientSettings.MaxConnecting = 3000;
            //mongoClientSettings.MinConnectionPoolSize = 100;
            //mongoClientSettings.MaxConnectionPoolSize = 3000;
            //mongoClientSettings.MaxConnectionIdleTime = TimeSpan.FromSeconds(300);
            //mongoClientSettings.WaitQueueSize = 3000;

            // Set up LINQ provider
            mongoClientSettings.LinqProvider = MongoDB.Driver.Linq.LinqProvider.V3;

            // Set application name if provided
            if (!string.IsNullOrWhiteSpace(applicationName))
            {
                //applicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                mongoClientSettings.ApplicationName = applicationName;
            }

            var prevClusterConfigurator = mongoClientSettings.ClusterConfigurator;
            mongoClientSettings.ClusterConfigurator = clusterConfigurator =>
            {
                prevClusterConfigurator?.Invoke(clusterConfigurator);

                clusterConfigurator.Subscribe<CommandStartedEvent>(this.OnEvent);
                clusterConfigurator.Subscribe<CommandSucceededEvent>(this.OnEvent);
                clusterConfigurator.Subscribe<CommandFailedEvent>(this.OnEvent);
                clusterConfigurator.Subscribe<ConnectionFailedEvent>(this.OnEvent);
            };

            // Initialize Client
            this.Client = new MongoClient(mongoClientSettings);

            // Initialize Database
            this.DatabaseName =
                @"^mongodb(\+srv)?\:\/\/(((?<USER>.*)\:(?<PASSWORD>.*)\@(?<CLUSTER>.*))|((?<HOST>.+)\:(?<PORT>.+)))\/(?<DBNAME>.*)\?.*$"
                    .MatchNamedCaptures(connectionString)["DBNAME"];
            this.Database = (MongoDatabaseBase)this.Client.GetDatabase(this.DatabaseName);

            //SessionStatedOnConstructor = startSession;
            this.IsUseTransaction = forceTransaction;
            this.SetUseTransactionOnConstructor = forceTransaction;

            //if (SessionStatedOnConstructor)
            //{
            //Session = Client.StartSession();

            if (forceTransaction)
            {
                this.Session = this.Client.StartSession();
                this.StartTransaction();
            }
            //}
            this.InternalConstructor().Wait();
        }

        private async Task InternalConstructor()
        {
            await this.MapAsync();

            await this.IndexAsync();

            if (_collectionNames == null)
            {
                lock (_collectionNamesLock)
                {
                    _collectionNames ??= new System.Collections.Concurrent.ConcurrentBag<string>(this.Database.ListCollectionNames().ToEnumerable());
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet<TDocument, TObjectId> GetDbSet<TDocument, TObjectId>(string collectionName = null,
            Options.TimerSeriesOptions timeSeriesOptions = null)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId> => new(this, collectionName, timeSeriesOptions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawDbSet GetDbSet(string collectionName,
            Options.TimerSeriesOptions timeSeriesOptions = null) => new(this, collectionName, timeSeriesOptions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet<TDocument> GetDbSet<TDocument>(string collectionName = null, Options.TimerSeriesOptions timeSeriesOptions = null)
            where TDocument : IObjectId<string> => new(this, collectionName, timeSeriesOptions);


        /// <summary>
        /// Gets the collection names.
        /// </summary>
        /// <returns>Collection names.</returns>
        /// <typeparam name="T">Type parameter description template.
        /// <param name="connectionString">Connection string.</param>
        /// <param name="applicationName">Application name.</param>
        /// <param name="forceTransaction">Force transaction.</param>
        /// <returns>Collection names.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> CollectionNames() => _collectionNames.ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Task MapAsync();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Task IndexAsync();

        /// <summary>
        /// Starts a session if one hasn't already been started.
        /// </summary>
        public void StartSessions()
        {
            // If a session hasn't already been started, start a new one.
            if (!this.SessionStatedOnConstructor)
            {
                this.Session = this.Client.StartSession();
            }

            // If a session hasn't been started, start a new one.
            // This is equivalent to the previous if statement.
            this.Session ??= this.Client.StartSession();
        }

        /// <summary>
        /// Starts a session if one hasn't already been started.
        /// </summary>
        /// <returns>A client session handle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IClientSessionHandle StartSession()
        {
            // Check if a session hasn't already been started
            if (!this.SessionStatedOnConstructor)
            {
                // Return the existing session
                return this.Session;
            }

            // Start a new session
            this.Session = this.Client.StartSession();

            // Return the session
            return this.Session;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        ///     Inicia a transação
        /// </summary>
        public void StartTransaction()
        {
            this.Session ??= this.Client.StartSession();
            //if (_onTransaction)
            //    throw new InvalidOperationException("Transaction has already been called.");

            lock (this._onTransactionLock)
            {
                this.IsUseTransaction = true;
                this.Session.StartTransaction();
                this._onTransaction = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        ///     Aborta a transação
        /// </summary>
        public void AbortTransaction()
        {
            lock (this._onTransactionLock)
            {
                if (!this.SetUseTransactionOnConstructor)
                {
                    this.IsUseTransaction = false;
                }

                this.Session.AbortTransaction();
                this._onTransaction = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        ///     Aplica mudanças pendentes
        /// </summary>
        public void CommitTransaction()
        {
            lock (this._onTransactionLock)
            {
                this.Session.CommitTransaction();
                this.Session.Dispose();
                this.Session = null;
                this._onTransaction = false;
            }
        }

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this._disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ContextBase()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public MongoClient GetMongoClient() => this.Client;

        public static implicit operator MongoClient(ContextBase context) => context.Client;

        public static implicit operator MongoDatabaseBase(ContextBase context) => context.Database;
    }
}
