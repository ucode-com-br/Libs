using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using UCode.Extensions;
using UCode.Mongo.Options;
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
        /// <summary>
        /// Stores the names of all collections in the database.
        /// </summary>
        private static System.Collections.Concurrent.ConcurrentDictionary<MongoContextImplementation, IEnumerable<string>> _dictionaryConstructed = new();

        /// <summary>
        /// The logger factory used to create loggers.
        /// </summary>
        internal readonly ILoggerFactory LoggerFactory;

        /// <summary>
        /// The MongoDB client used to connect to the database.
        /// </summary>
        internal readonly MongoClient Client;

        /// <summary>
        /// The database associated with this context.
        /// </summary>
        internal readonly MongoDatabaseBase Database;

        /// <summary>
        /// The session handle used for the current transaction.
        /// </summary>
        internal IClientSessionHandle? Session;

        /// <summary>
        /// Indicates whether a transaction is currently in progress.
        /// </summary>
        private bool _onTransaction;

        /// <summary>
        /// Lock object used to ensure thread safety when starting or aborting transactions.
        /// </summary>
        private readonly object _onTransactionLock = new();


        /// <summary>
        /// The name of the database associated with this context.
        /// </summary>
        public readonly string DatabaseName;

        /// <summary>
        /// Indicates whether a session was started by the constructor.
        /// </summary>
        private readonly bool SessionStatedOnConstructor;

        /// <summary>
        /// Lazily creates a logger for this context.
        /// </summary>
        private readonly Lazy<ILogger<ContextBase>> _logger;

        /// <summary>
        /// Gets the logger for this context.
        /// </summary>
        protected ILogger<ContextBase> Logger => this._logger.Value;

        /// <summary>
        /// Indicates whether a transaction is currently in progress.
        /// </summary>
        public bool IsUseTransaction
        {
            get; private set;
        }

        /// <summary>
        /// Indicates whether a transaction was started by the constructor.
        /// </summary>
        private bool SetUseTransactionOnConstructor
        {
            get; set;
        }

        /// <summary>
        /// Indicates whether the object has been disposed.
        /// </summary>
        private bool _disposedValue;
        private MongoContextImplementation _instanceMongoContextImplementation;
        private IEnumerable<string> _instanceCollectionNames;

        /// <summary>
        /// Event handler for MongoDB events.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments.</param>
        public delegate void EventHandler<TEvent>(object sender, MongoEventArgs<TEvent> args);

        /// <summary>
        /// Event raised when a MongoDB event occurs.
        /// </summary>
        public event EventHandler Event;

        /// <summary>
        /// Invokes the event handler for the given event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="ev">The event to invoke the handler for.</param>
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

            // Set up LINQ provider
            //mongoClientSettings.LinqProvider = MongoDB.Driver.Linq.LinqProvider.V3;

            // Set application name if provided
            if (!string.IsNullOrWhiteSpace(applicationName))
            {
                //applicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                mongoClientSettings.ApplicationName = applicationName;
            }

            // Store the previous cluster configurator
            var prevClusterConfigurator = mongoClientSettings.ClusterConfigurator;

            // Set the new cluster configurator
            mongoClientSettings.ClusterConfigurator = clusterConfigurator =>
            {
                // Invoke the previous cluster configurator (if any)
                prevClusterConfigurator?.Invoke(clusterConfigurator);

                // Subscribe to command started events
                clusterConfigurator.Subscribe<CommandStartedEvent>(this.OnEvent);

                // Subscribe to command succeeded events
                clusterConfigurator.Subscribe<CommandSucceededEvent>(this.OnEvent);

                // Subscribe to command failed events
                clusterConfigurator.Subscribe<CommandFailedEvent>(this.OnEvent);

                // Subscribe to connection failed events
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

            if (forceTransaction)
            {
                this.Session = this.Client.StartSession();
                this.StartTransaction();
            }

            this.InternalConstructor(connectionString);
        }

        private void InternalConstructor(string connectionString)
        {
            var implimentedUnderlyingSystemType = this.GetType().UnderlyingSystemType;
            var fullname = implimentedUnderlyingSystemType.FullName!;

            _instanceMongoContextImplementation = new MongoContextImplementation(fullname, implimentedUnderlyingSystemType, connectionString.CalculateSha256Hash()!, this.DatabaseName);

            _instanceCollectionNames = _dictionaryConstructed.AddOrUpdate(_instanceMongoContextImplementation, (key) =>
            {
                this.MapAsync().Wait();
                this.IndexAsync().Wait();
                return this.Database.ListCollectionNames().ToEnumerable().Select(collectionName => $"{fullname}-{this.DatabaseName}.{collectionName}").ToList();
            }, (key, value) => value);
        }


        #region Before
        internal TDocument BeforeInsertInternal<TDocument, TObjectId>(TDocument original)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId>
        {
            var destination = this.BeforeInsert<TDocument, TObjectId>(original);

            if (destination == null)
            {
                throw new Exception("cannot save null object.");
            }

            return destination;
        }

        internal Update<TDocument> BeforeUpdateInternal<TDocument, TObjectId>(Update<TDocument> updateOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId>
        {
            var destination = this.BeforeUpdate<TDocument, TObjectId>(updateOptions);

            if (destination == null)
            {
                throw new Exception("cannot update null object.");
            }

            return destination;
        }

        internal TDocument BeforeReplaceInternal<TDocument, TObjectId>(TDocument original)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId>
        {
            var destination = this.BeforeReplace<TDocument, TObjectId>(original);

            if (destination == null)
            {
                throw new Exception("cannot save null object.");
            }

            return destination;
        }

        internal BsonDocument[] BeforeAggregateInternal<TDocument, TObjectId, TProjection>(BsonDocument[] original)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId>
        {
            var destination = this.BeforeAggregate<TDocument, TObjectId, TProjection>(original);

            if (destination == null || destination.Length == 0)
            {
                throw new Exception("Cannot save null or empty aggregation.");
            }

            return destination;
        }

        /// <summary>
        /// Before send insert to TDocument to MongoDB
        /// </summary>
        /// <typeparam name="TDocument">Document collection type</typeparam>
        /// <typeparam name="TObjectId">Document id type</typeparam>
        /// <param name="original">Document original value</param>
        /// <returns>Document changed</returns>
        protected virtual TDocument? BeforeInsert<TDocument, TObjectId>(TDocument original)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId> => original;

        /// <summary>
        /// Before send update to TDocument to MongoDB
        /// </summary>
        /// <typeparam name="TDocument">Document collection type</typeparam>
        /// <typeparam name="TObjectId">Document id type</typeparam>
        /// <param name="updateOptions">Original update command</param>
        /// <returns>Update command</returns>
        protected virtual Update<TDocument>? BeforeUpdate<TDocument, TObjectId>(Update<TDocument> updateOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId> => updateOptions;

        /// <summary>
        /// Before send replace to TDocument to MongoDB
        /// </summary>
        /// <typeparam name="TDocument">Document collection type</typeparam>
        /// <typeparam name="TObjectId">Document id type</typeparam>
        /// <param name="original">Document original value</param>
        /// <returns>Document changed</returns>
        protected virtual TDocument? BeforeReplace<TDocument, TObjectId>(TDocument original)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId> => original;

        /// <summary>
        /// Before send aggregation to TDocument to MongoDB
        /// </summary>
        /// <typeparam name="TDocument">Document collection type</typeparam>
        /// <typeparam name="TObjectId">Document id type</typeparam>
        /// <typeparam name="TProjection">Document projection type</typeparam>
        /// <param name="query">Original aggregation query</param>
        /// <returns>Aggregation query changed</returns>
        protected virtual BsonDocument[]? BeforeAggregate<TDocument, TObjectId, TProjection>(BsonDocument[] bsonDocuments)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId> => bsonDocuments;
        #endregion


        /// <summary>
        /// Get a database set
        /// </summary>
        /// <typeparam name="TDocument">Document collection type</typeparam>
        /// <typeparam name="TObjectId">Document id type</typeparam>
        /// <param name="collectionName">Collection name.</param>
        /// <param name="timeSeriesOptions">Time series options.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet<TDocument, TObjectId> GetDbSet<TDocument, TObjectId>(string? collectionName = null,
            TimerSeriesOptions? timeSeriesOptions = null)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId> => new(this, collectionName, timeSeriesOptions);

        /// <summary>
        /// Get a raw database set
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="timeSeriesOptions">Time series options.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawDbSet GetDbSet(string collectionName, TimerSeriesOptions? timeSeriesOptions = null) => new(this, collectionName, timeSeriesOptions);

        // Get a database set with a string ID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet<TDocument> GetDbSet<TDocument>(string? collectionName = null, TimerSeriesOptions? timeSeriesOptions = null)
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
        public IEnumerable<string> CollectionNames() => _instanceCollectionNames;

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

            // Lock to ensure thread safety when starting the transaction
            lock (this._onTransactionLock)
            {
                // If a transaction is already in progress, throw an exception
                //if (_onTransaction)
                //    throw new InvalidOperationException("Transaction has already been called.");

                // Set the IsUseTransaction flag to true to indicate that a transaction is in progress
                this.IsUseTransaction = true;

                // Start the transaction
                this.Session.StartTransaction();

                // Set the _onTransaction flag to true to indicate that a transaction is in progress
                this._onTransaction = true;
            }
        }

        /// <summary>
        /// Aborts the current transaction.
        /// </summary>
        /// <remarks>
        /// This method should be called after a transaction has been started using the <see cref="StartTransaction"/> method.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AbortTransaction()
        {
            lock (this._onTransactionLock)
            {
                // If the transaction was not started with the constructor, reset the IsUseTransaction flag to false
                if (!this.SetUseTransactionOnConstructor)
                {
                    this.IsUseTransaction = false;
                }

                // Abort the transaction
                this.Session.AbortTransaction();
                this._onTransaction = false;
            }
        }

        /// <summary>
        /// Commits the current transaction.
        /// This method should be called after a transaction has been started using the <see cref="StartTransaction"/> method.
        /// </summary>
        /// <remarks>
        /// This method should be called within a lock to ensure thread safety.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CommitTransaction()
        {
            if (!this._onTransaction)
                throw new InvalidOperationException("Transaction has not been started.");

            lock (this._onTransactionLock)
            {
                // Commit the transaction
                this.Session.CommitTransaction();

                // Dispose of the session to release resources
                this.Session.Dispose();

                // Set the session to null to indicate that the transaction has been committed
                this.Session = null;

                // Set _onTransaction to false to indicate that the transaction has been committed
                this._onTransaction = false;
            }
        }

        #region Dispose

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        /// <param name="disposing">True if disposing, false otherwise.</param>
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

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Gets the MongoClient instance associated with this context.
        /// </summary>
        /// <returns>The MongoClient instance.</returns>
        public MongoClient GetMongoClient() => this.Client;

        /// <summary>
        /// Implicitly converts a ContextBase instance to a MongoClient instance.
        /// </summary>
        /// <param name="context">The ContextBase instance to convert.</param>
        /// <returns>The MongoClient instance.</returns>
        public static implicit operator MongoClient(ContextBase context) => context.Client;

        /// <summary>
        /// Implicitly converts a ContextBase instance to a MongoDatabaseBase instance.
        /// </summary>
        /// <param name="context">The ContextBase instance to convert.</param>
        /// <returns>The MongoDatabaseBase instance.</returns>
        public static implicit operator MongoDatabaseBase(ContextBase context) => context.Database;
    }
}
