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
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace UCode.Mongo
{

    /// <summary>
    /// Represents the base context class for managing resources and providing a common
    /// interface for derived context classes. This class implements the IDisposable interface,
    /// allowing derived classes to free resources when no longer needed.
    /// </summary>
    /// <remarks>
    /// Derived classes should implement their own Dispose pattern to ensure proper
    /// resource management and cleanup. This class may contain common implementation details
    /// that are shared among different context types.
    /// </remarks>
    /// <example>
    /// The following is an example of a derived class:
    /// <code>
    public abstract class ContextBase : IDisposable
    {
        // Lock for synchronizing access to _collectionNames
        private static readonly object _collectionNamesLock = new();

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<MongoContextImplementation, IEnumerable<string>> _dictionaryConstructed = new();

        internal readonly ILoggerFactory LoggerFactory;

        internal readonly MongoClient Client;

        internal readonly IMongoDatabase Database;

        internal IClientSessionHandle? ContextSession;

        private readonly object _transactionContextLock = new();

        public readonly string DatabaseName;

        private readonly Lazy<ILogger<ContextBase>> _logger;

        /// <summary>
        /// Gets the logger instance for the current context.
        /// </summary>
        /// <value>
        /// An instance of <see cref="ILogger{ContextBase}"/> that provides the logging functionality.
        /// </value>
        protected ILogger<ContextBase> Logger => this._logger.Value;

        /// <summary>
        /// Gets a value indicating whether the current context is a transactional context.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current context is transactional; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property can only be set 
        public bool TransactionalContext
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the transaction context has been started.
        /// </summary>
        /// <value>
        /// <c>true</c> if the transaction context has started; otherwise, <c>false</c>.
        /// </value>
        private bool TransactionContextStarted
        {
            get; set;
        }

        private bool _disposedValue;
        private MongoContextImplementation _instanceMongoContextImplementation;
        private IEnumerable<string> _instanceCollectionNames;

        /// <summary>
        /// Represents a delegate for handling events with a specific event type.
        /// </summary>
        /// <typeparam name="TEvent">
        /// The type of the event associated with the event handler.
        /// </typeparam>
        /// <param name="sender">
        /// The source of the event, typically the object that raises the event.
        /// </param>
        /// <param name="args">
        /// An instance of <see cref="MongoEventArgs{TEvent}"/> containing data related to the event.
        /// </param>
        public delegate void EventHandler<TEvent>(object sender, MongoEventArgs<TEvent> args);

        public event EventHandler Event;

        /// <summary>
        /// Invokes the event with the specified event data.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event data.</typeparam>
        /// <param name="ev">The event data to pass to the event handler.</param>
        /// <remarks>
        /// This method is virtual, allowing derived classes to override its behavior.
        /// If the <c>Event</c> is not null, it raises the event using an instance of
        /// <see cref="MongoEventArgs{TEvent}"/> to wrap the event data.
        /// </remarks>
        public virtual void OnEvent<TEvent>(TEvent ev) => Event?.Invoke(this, new MongoEventArgs<TEvent>(ev));

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextBase"/> class.
        /// </summary>
        /// <param name="loggerFactory">The factory used to create loggers.</param>
        /// <param name="connectionString">The connection string to the MongoDB database.</param>
        /// <param name="applicationName">Optional. The name of the application using this context.</param>
        /// <param name="transactionalContext">Optional. Indicates whether the context should support transactions.</param>
        /// <returns>
        /// A new instance of the <see cref="ContextBase"/> class.
        /// </returns>
        protected ContextBase([NotNull] ILoggerFactory loggerFactory, [NotNull] string connectionString,
                    string? applicationName = null, bool transactionalContext = false)
        {
            // Initialize LoggerFactory
            this.LoggerFactory = loggerFactory;

            this._logger = new Lazy<ILogger<ContextBase>>(() => loggerFactory.CreateLogger<ContextBase>());

            // Initialize Client
            var mongoClientSettings = MongoClientSettings.FromConnectionString(connectionString);

            // Set application name if provided
            if (!string.IsNullOrWhiteSpace(applicationName))
            {
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
            this.Database = (IMongoDatabase)this.Client.GetDatabase(this.DatabaseName);

            this.TransactionalContext = transactionalContext;

            if (this.TransactionalContext)
            {
                this.ContextSession = this.Client.StartSession();
                this.StartTransaction();
            }

            this.InternalConstructor(connectionString);
        }


        /// <summary>
        /// Initializes a new instance of the class, using the provided connection string.
        /// It retrieves the underlying system type and the full name of the type.
        /// The connection string is hashed and used to instantiate a Mongo context implementation.
        /// Collection names are collected and indexed for further operations.
        /// </summary>
        /// <param name="connectionString">The connection string to connect to the MongoDB database.</param>
        /// <remarks>
        /// This method is intended to be used 
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
        /// <summary>
        /// Processes a document before insertion by invoking a pre-insertion method.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document being handled, which must implement IObjectId with a specific ID type.</typeparam>
        /// <typeparam name="TObjectId">The type of the object's ID, which must be comparable and equatable.</typeparam>
        /// <param name="original">The original document that is being processed.</param>
        /// <returns>
        /// Returns the processed document after pre-insertion logic has been applied.
        /// Throws an exception if the processed document is null.
        /// </returns>
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

        /// <summary>
        /// Executes pre-update operations on the given update options for a document of type TDocument.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document that is being updated. It must implement the IObjectId interface.</typeparam>
        /// <typeparam name="TObjectId">The type of the identifier for the document. It must implement both IComparable and IEquatable interfaces.</typeparam>
        /// <param name="updateOptions">The update options to apply before executing the update operation.</param>
        /// <returns>
        /// Returns the modified update options if the operation is successful.
        /// </returns>
        /// <exception cref="System.Exception">
        /// Thrown when the destination update options are null, indicating that the update cannot proceed.
        /// </exception>
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

        /// <summary>
        /// Internal method that processes a document before replacing it. 
        /// It calls the BeforeReplace method and checks if the returned 
        /// object is null, throwing an exception if that is the case.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document being processed. 
        /// It must implement the IObjectId interface.</typeparam>
        /// <typeparam name="TObjectId">The type of the object ID. 
        /// It must implement IComparable and IEquatable interfaces.</typeparam>
        /// <param name="original">The original document that is being replaced.</param>
        /// <returns>
        /// The processed document that is to replace the original document.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when the destination document to replace the original is null.
        /// </exception>
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

        /// <summary>
        /// Processes the specified array of BsonDocuments before aggregation.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document which must implement the IObjectId interface.</typeparam>
        /// <typeparam name="TObjectId">The type of the object ID which must implement IComparable and IEquatable interfaces.</typeparam>
        /// <typeparam name="TProjection">The type representing the projection for the aggregation.</typeparam>
        /// <param name="original">An array of BsonDocuments representing the original data to be processed.</param>
        /// <returns>
        /// An array of BsonDocuments after being processed by the BeforeAggregate method.
        /// </returns>
        /// <exception cref="Exception">Thrown when the resulting array of BsonDocuments is null or empty.</exception>
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
        /// This method is called before inserting a document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document to be inserted, which must implement the IObjectId interface.</typeparam>
        /// <typeparam name="TObjectId">The type of the ObjectId used by the document, which must be comparable and equatable.</typeparam>
        /// <param name="original">The original document that is being processed before insertion.</param>
        /// <returns>The original document unmodified.</returns>
        /// <remarks>
        /// This method is marked as virtual, allowing derived classes to override its behavior if desired.
        /// It takes a single parameter 'original', which is of type 'TDocument', and returns the same document.
        /// </remarks>
        protected virtual TDocument? BeforeInsert<TDocument, TObjectId>(TDocument original)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId> => original;

        /// <summary>
        /// This method serves as a virtual hook that can be overridden to perform operations
        /// before an update is executed on a document.
        /// </summary>
        /// <typeparam name="TDocument">
        /// The type of the document being updated. This type must implement the 
        /// <see cref="IObjectId{TObjectId}"/> interface.
        /// </typeparam>
        /// <typeparam name="TObjectId">
        /// The type of the object ID associated with the document. This type must implement
        /// both <see cref="IComparable{T}"/> and <see cref="IEquatable{T}"/> interfaces.
        /// </typeparam>
        /// <param name="updateOptions">
        /// The options for the update operation, encapsulated within an 
        /// <see cref="Update{TDocument}"/> instance.
        /// </param>
        /// <returns>
        /// Returns the provided <paramref name="updateOptions"/> instance, 
        /// which can be modified before being passed to the actual update operation.
        /// </returns>
        protected virtual Update<TDocument>? BeforeUpdate<TDocument, TObjectId>(Update<TDocument> updateOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId> => updateOptions;

        /// <summary>
        /// This method is a virtual hook that can be overridden in derived classes.
        /// It is called before an object is replaced in a data store.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document that is being replaced.</typeparam>
        /// <typeparam name="TObjectId">The type of the object identifier which must implement IComparable and IEquatable interfaces.</typeparam>
        /// <param name="original">The original document that is being replaced.</param>
        /// <returns>
        /// Returns the original document. This could potentially be modified in 
        /// derived classes before replacement takes place.
        /// </returns>
        /// <remarks>
        /// This method provides an opportunity to perform additional logic or validation 
        /// before the replacement of the document occurs, such as logging or transformation.
        /// </remarks>
        protected virtual TDocument? BeforeReplace<TDocument, TObjectId>(TDocument original)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId> => original;

        /// <summary>
        /// This method is invoked before the aggregation operation occurs.
        /// It takes an array of BsonDocuments as input and returns them unchanged.
        /// </summary>
        /// <typeparam name="TDocument">
        /// The type of the document that implements the IObjectId interface.
        /// </typeparam>
        /// <typeparam name="TObjectId">
        /// The type of the object identifier, which must implement IComparable and IEquatable interfaces.
        /// </typeparam>
        /// <typeparam name="TProjection">
        /// The type of the projection to be applied during aggregation.
        /// </typeparam>
        /// <param name="bsonDocuments">
        /// An array of BsonDocument instances that will be processed before aggregation.
        /// </param>
        /// <returns>
        /// Returns an array of BsonDocument, which is the same array passed as input.
        /// </returns>
        protected virtual BsonDocument[]? BeforeAggregate<TDocument, TObjectId, TProjection>(BsonDocument[] bsonDocuments)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId> => bsonDocuments;

        #endregion


        /// <summary>
        /// Gets a DbSet<TEntity, TIdentifier> for the specified document type and identifier type.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document. Must implement the <see cref="IObjectId{TIdentifier}"/> interface.</typeparam>
        /// <typeparam name="TObjectId">The type of the object identifier. Must implement <see cref="IComparable{TObjectId}"/> and <see cref="IEquatable{TObjectId}"/>.</typeparam>
        /// <param name="collectionName">Optional. The name of the collection to use. If not provided, defaults to the convention-based name.</param>
        /// <param name="createCollectionOptionsAction">Optional. An action to configure options for the collection being created.</param>
        /// <param name="mongoCollectionSettingsAction">Optional. An action to configure settings for the MongoDB collection.</param>
        /// <param name="useTransaction">Optional. A boolean indicating whether to force the use of transactions.</param>
        /// <returns>A <see cref="DbSet{TDocument, TObjectId}"/> instance for the specified document type and identifier type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="collectionName"/> is null and the default collection name cannot be determined.</exception>
        /// <remarks>
        /// This method allows for the creation and retrieval of a DbSet for handling operations on the specified document type.
        /// Customization of collection creation and settings can enhance the performance and behavior of interactions with the database.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet<TDocument, TObjectId> GetDbSet<TDocument, TObjectId>(
            string? collectionName = null, Action<CreateCollectionOptions>? createCollectionOptionsAction = null,
            Action<MongoCollectionSettings>? mongoCollectionSettingsAction = null, bool? useTransaction = default)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectId<TObjectId> => new (this, collectionName, createCollectionOptionsAction, mongoCollectionSettingsAction, useTransaction ?? this.TransactionalContext);




        /// <summary>
        /// Retrieves an enumerable collection of string names from the instance 
        /// collection. This method is marked with Aggressive Inlining to optimize 
        /// performance by encouraging the Just-In-Time compiler to inline the method.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="string"/> representing the 
        /// names of collections in the instance.
        /// </returns>
        /// <remarks>
        /// This method does not take any parameters and returns the collection names 
        /// directly from the 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<string> CollectionNames() => _instanceCollectionNames;

        /// <summary>
        /// This abstract method is intended to be implemented by derived classes 
        /// to perform an asynchronous mapping operation. The exact details of 
        /// the mapping process will be defined in the implementing class.
        /// </summary>
        /// <returns>
        /// A Task representing the asynchronous operation. The Task will complete 
        /// when the mapping operation has finished. Implementations should ensure 
        /// that any exceptions are properly handled and propagated.
        /// </returns>
        /// <remarks>
        /// This method is marked with the <see cref="MethodImplAttribute"/> attribute, 
        /// with <see cref="MethodImplOptions.AggressiveInlining"/> indicating 
        /// that the JIT compiler should inline this method if possible, potentially 
        /// optimizing performance when called frequently.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Task MapAsync();

        /// <summary>
        /// An abstract method that allows for asynchronous indexing of data.
        /// This method should be implemented by derived classes to provide
        /// specific functionality for indexing operations.
        /// </summary>
        /// <returns>
        /// A Task representing the asynchronous operation. The task 
        /// will complete when the indexing operation is finished.
        /// </returns>
        /// <remarks>
        /// The method is marked with <see cref="MethodImplOptions.AggressiveInlining"/> 
        /// to suggest the JIT compiler inline this method for performance optimization.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Task IndexAsync();

        /// <summary>
        /// Starts a new client session if one hasn't already been initiated. 
        /// Ensures that the operation is thread-safe by using a lock on the transaction context.
        /// </summary>
        /// <returns>
        /// Returns the current active <see cref="IClientSessionHandle"/> instance.
        /// This instance will either be a newly started session or an existing session
        /// if one is already in progress.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IClientSessionHandle StartSession()
        {
            lock (this._transactionContextLock)
            {
                // Check if a session hasn't already been started
                if (!this.TransactionContextStarted || this.ContextSession == null)
                {
                    // Start a new session
                    this.ContextSession = this.Client.StartSession();
                }
            }

            // Return the session
            return this.ContextSession!;
        }

        /// <summary>
        /// Creates a new client session from the current client instance.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="IClientSessionHandle"/> representing the new client session.
        /// </returns>
        /// <remarks>
        /// This method calls the <see cref="Client.StartSession"/> method to initiate a session.
        /// It is typically used when you need to perform operations that require a session context.
        /// </remarks>
        public IClientSessionHandle CreateSession() => this.Client.StartSession();

        /// <summary>
        /// Initiates a new transaction within the current client session.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="IClientSessionHandle"/> that represents the started transaction session.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IClientSessionHandle StartTransaction()
        {
            IClientSessionHandle? result = StartSession();

            lock (this._transactionContextLock)
            {
                if (!this.TransactionContextStarted)
                {
                    // Start the transaction
                    result.StartTransaction();

                    this.TransactionContextStarted = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Aborts the current transaction, if one is in progress.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if an attempt is made to abort a transaction that has not been started.
        /// </exception>
        /// <remarks>
        /// This method ensures that the transaction is properly aborted and that 
        /// resources are released correctly. It uses a lock to prevent concurrent 
        /// access to the transaction context, ensuring thread safety.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AbortTransaction()
        {
            if (!this.TransactionContextStarted)
                throw new InvalidOperationException("Transaction has not been started.");

            lock (this._transactionContextLock)
            {
                this.ContextSession!.AbortTransaction();

                this.TransactionalContext = false;

                this.ContextSession.Dispose();

                this.ContextSession = default;
            }
        }

        /// <summary>
        /// Commits the current transaction if it has been started.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when an attempt is made to commit a transaction that has not been started.
        /// </exception>
        /// <remarks>
        /// This method uses a lock to ensure that the commit process is thread-safe.
        /// It commits the transaction using the current session, releases the resources by disposing of the session,
        /// and sets the session and transaction context status to indicate that the transaction has been successfully committed.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CommitTransaction()
        {
            if (!this.TransactionContextStarted)
                throw new InvalidOperationException("Transaction has not been started.");

            lock (this._transactionContextLock)
            {
                // Commit the transaction
                this.ContextSession!.CommitTransaction();

                // Dispose of the session to release resources
                this.ContextSession!.Dispose();

                // Set the session to null to indicate that the transaction has been committed
                this.ContextSession = null;

                // Set _onTransaction to false to indicate that the transaction has been committed
                this.TransactionContextStarted = default;
            }
        }

        #region Dispose

        /// <summary>
        /// Releases resources used by the current instance of the class. 
        /// This method is called from both the Dispose() method and the finalizer. 
        /// It is important to ensure that any managed and unmanaged resources are properly disposed of
        /// to prevent memory leaks.
        /// </summary>
        /// <param name="disposing">A boolean indicating whether the method is called 
        /// from the Dispose method (true) or from the finalizer (false).</param>
        /// <remarks>
        /// Call this method when you are done with the object to release 
        /// its resources. When disposing is true, managed resources will be disposed. 
        /// When disposing is false, only unmanaged resources need to be freed.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    this.ContextSession?.Dispose();
                    this.Client?.Dispose();
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
        /// Finalizer for the ContextBase class.
        /// This method is currently commented out as it should only be implemented
        /// if the 'Dispose(bool disposing)' method includes code to release unmanaged resources.
        /// The purpose of a finalizer is to allow the class to clean up resources when
        /// it's no longer in use and not explicitly disposed.
        /// </summary>
        /// <remarks>
        /// Ensure that cleanup code for unmanaged resources is placed in the
        /// 'Dispose(bool disposing)' method, not within the finalizer itself.
        /// </remarks>
        /// <example>
        /// To enable the finalizer, uncomment the method:
        /// ~ContextBase()
        /// {
        ///     Dispose(disposing: false);
        /// }
        /// </example>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Gets the MongoDB client instance associated with this class.
        /// </summary>
        /// <returns>
        /// A <see cref="MongoClient"/> instance that represents the MongoDB client.
        /// </returns>
        public MongoClient GetMongoClient() => this.Client;

        public static implicit operator MongoClient(ContextBase context) => context.Client;

    }
}
