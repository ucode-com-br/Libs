using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Asn1;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using UCode.Extensions;
using UCode.Mongo.Models;
using UCode.Mongo.Options;
using ZstdSharp.Unsafe;

namespace UCode.Mongo
{


    /// <summary>
    /// Represents the base class for context management, providing an interface for disposal.
    /// This class is intended to be inherited by other context classes that require 
    /// resource management and cleanup.
    /// </summary>
    /// <remarks>
    /// This class implements the <see cref="IDisposable"/> interface to ensure 
    /// that any unmanaged resources are properly released when the derived classes 
    /// are no longer needed.
    /// </remarks>
    /// <seealso cref="IDisposable"/>
    public abstract class ContextBase : IDisposable
    {
        internal Dictionary<string, ContextCollectionMetadata> _contextCollectionMetadata = new Dictionary<string, ContextCollectionMetadata>();

        /// <summary>
        /// A static readonly object used for locking purposes to ensure thread safety
        /// when accessing or modifying collection names. This lock prevents race 
        /// conditions and ensures that only one thread can access the critical 
        /// section of the code that modifies shared collection names at any given time.
        /// </summary>
        private static readonly object _collectionNamesLock = new();

        /// <summary>
        /// A static, read-only field that holds a thread-safe collection of constructed dictionary entries. 
        /// This dictionary uses the MongoContextImplementation as its key and associates it with an 
        /// enumerable collection of strings. It is meant for managing concurrent access to a set of 
        /// strings related to different Mongo context implementations.
        /// </summary>
        /// <remarks>
        /// The ConcurrentDictionary is ideal in scenarios where multiple threads might be reading from 
        /// or writing to the dictionary simultaneously, ensuring safe operations without the need for 
        /// explicit locking.
        /// </remarks>
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<MongoContextImplementation, IEnumerable<string>> _dictionaryConstructed = new();

        /// <summary>
        /// Represents a factory for creating instances of <see cref="ILogger"/>.
        /// </summary>
        /// <remarks>
        /// This field is marked as <c>readonly</c>, meaning it can only be assigned during initialization 
        /// (either in its declaration, or within the constructor of the containing class).
        /// The <see cref="ILoggerFactory"/> interface provides methods for creating <see cref="ILogger"/> instances 
        /// which are used for logging messages to various outputs.
        /// </remarks>
        internal readonly ILoggerFactory LoggerFactory;

        /// <summary>
        /// Represents a MongoDB client that is used to connect to the MongoDB server.
        /// </summary>
        /// <remarks>
        /// The <c>MongoClient</c> is designed to be instantiated once and reused throughout the application.
        /// It is thread-safe and will manage connections in an efficient manner.
        /// </remarks>
        /// <value>
        /// The <c>MongoClient</c> instance that provides methods to interact with the MongoDB database.
        /// </value>
        internal readonly MongoClient MongoClient;


        /// <summary>
        /// Represents an instance of a MongoDB database that is used for data operations.
        /// The database instance is read-only and cannot be modified after being initialized.
        /// </summary>
        /// <remarks>
        /// This field is intended to provide access to the MongoDB database implementation,
        /// enabling data management operations such as CRUD (Create, Read, Update, Delete).
        /// Use this instance to interact with collections within the database.
        /// </remarks>
        internal readonly IMongoDatabase Database;

        /// <summary>
        /// Represents the context session handle for the client.
        /// It can be used to manage the state and transactions 
        /// associated with a client session.
        /// </summary>
        /// <remarks>
        /// This field can be null, indicating that the context 
        /// session is not currently established.
        /// </remarks>
        internal IClientSessionHandle? ContextSession;

        /// <summary>
        /// Represents a lock object used to synchronize access to 
        /// the transaction context in a thread-safe manner.
        /// This ensures that only one thread can access the 
        /// shared transaction context at a time, preventing 
        /// potential data corruption or inconsistent state.
        /// </summary>
        private readonly object _transactionContextLock = new();

        /// <summary>
        /// Represents the name of the database.
        /// </summary>
        /// <remarks>
        /// This field is marked as readonly, meaning its value can only be assigned 
        /// during the declaration or within a constructor of the containing class.
        /// </remarks>
        public readonly string DatabaseName;

        /// <summary>
        /// Represents a logger that can be used to log messages for the <see cref="ContextBase"/> class.
        /// The logger is wrapped in a <see cref="Lazy{T}"/> to ensure it is only created when accessed,
        /// allowing for deferred initialization and potentially avoiding unnecessary allocation.
        /// </summary>
        /// <remarks>
        /// This logger is parameterized with the type <see cref="ContextBase"/> to provide contextual
        /// logging information relevant to instances of this class.
        /// </remarks>
        private readonly Lazy<ILogger<ContextBase>> _logger;

        /// <summary>
        /// A flag to indicate whether the object has already been disposed.
        /// This is used to prevent multiple calls to the Dispose method,
        /// ensuring that resources are released only once.
        /// </summary>
        private bool _disposedValue;

        /// <summary>
        /// Represents an instance of the MongoContextImplementation
        /// which is used to manage the connection and operations 
        /// with the MongoDB database.
        /// </summary>
        /// <remarks>
        /// This field is 
        private MongoContextImplementation _instanceMongoContextImplementation;

        /// <summary>
        /// Represents a collection of instance names as a sequence of strings.
        /// </summary>
        /// <remarks>
        /// This 
        private IEnumerable<string> _instanceCollectionNames;

        /// <summary>
        /// Gets the logger instance associated with the current context.
        /// </summary>
        /// <value>
        /// The logger that is used for logging messages and exceptions.
        /// </value>
        protected ILogger<ContextBase> Logger => this._logger.Value;

        /// <summary>
        /// Gets a value indicating whether the current context is transactional.
        /// This property is read-only from outside the class, as it has a 
        public bool TransactionalContext
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the transaction context has been started.
        /// </summary>
        /// <value>
        /// A boolean value that is true if the transaction context is started; otherwise, false.
        /// </value>
        private bool TransactionContextStartedxxxxxxx
        {
            get; set;
        }



        /// <summary>
        /// Represents a method that will handle an event, with a specified event type 
        /// and sender information.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event arguments.</typeparam>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">An instance of <see cref="MongoEventArgs{TEvent}"/> that contains the event data.</param>
        public delegate void EventHandler<TEvent>(object sender, MongoEventArgs<TEvent> args);

        public event EventHandler Event;

        /// <summary>
        /// Invokes the event for the specified event type with the provided event data.
        /// </summary>
        /// <typeparam name="TEvent">
        /// The type of the event data being passed.
        /// </typeparam>
        /// <param name="ev">
        /// The event data to be passed to the event handlers.
        /// </param>
        public virtual void OnEvent<TEvent>(TEvent ev) => Event?.Invoke(this, new MongoEventArgs<TEvent>(ev));

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextBase"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory used to create loggers.</param>
        /// <param name="connectionString">The connection string for the MongoDB database.</param>
        /// <param name="applicationName">Optional name of the application; if provided, it sets the application name in MongoDB client settings.</param>
        /// <param name="transactionalContext">Indicates whether the context should support transactions.</param>
        /// <remarks>
        /// This constructor initializes the MongoDB client and database, sets up logging, and configures
        /// event subscriptions for command and connection events. If transactional support is enabled,
        /// it starts a session and begins a transaction.
        /// </remarks>
        protected ContextBase([NotNull] ILoggerFactory loggerFactory, [NotNull] string connectionString,
                              string? applicationName = null, bool transactionalContext = false)
        {
            //BsonSerializer.TryRegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            // Initialize LoggerFactory
            this.LoggerFactory = loggerFactory;

            this._logger = new Lazy<ILogger<ContextBase>>(() => loggerFactory.CreateLogger<ContextBase>());

            // Initialize Client
            var mongoClientSettings = MongoClientSettings.FromConnectionString(connectionString);


            // 
            mongoClientSettings.TranslationOptions = new ExpressionTranslationOptions()
            {
                EnableClientSideProjections = true,
                CompatibilityLevel = ServerVersion.Server70
            };

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
            this.MongoClient = new MongoClient(mongoClientSettings);

            // Initialize Database
            this.DatabaseName =
                @"^mongodb(\+srv)?\:\/\/(((?<USER>.*)\:(?<PASSWORD>.*)\@(?<CLUSTER>.*))|((?<HOST>.+)\:(?<PORT>.+)))\/(?<DBNAME>.*)\?.*$".MatchNamedCaptures(connectionString)["DBNAME"];

            this.Database = this.MongoClient.GetDatabase(this.DatabaseName);

            this.TransactionalContext = transactionalContext;

            if (this.TransactionalContext)
            {
                this.ContextSession = this.MongoClient.StartSession();
                this.StartTransaction();
            }

            this.InternalConstructor(connectionString);
        }


        /// <summary>
        /// Initializes an instance of the MongoContextImplementation class using the provided connection string.
        /// This method retrieves the underlying system type of the current instance, computes its SHA256 hash,
        /// and maps the asynchronous operations to the MongoDB collections. It also manages the collection names 
        /// in a dictionary for further use.
        /// </summary>
        /// <param name="connectionString">The connection string to the MongoDB database.</param>
        private void InternalConstructor(string connectionString)
        {
            var implimentedUnderlyingSystemType = this.GetType().UnderlyingSystemType;
            var fullname = implimentedUnderlyingSystemType.FullName!;

            _instanceMongoContextImplementation = new MongoContextImplementation(fullname, implimentedUnderlyingSystemType, connectionString.CalculateSha256Hash()!, this.DatabaseName);


            _instanceCollectionNames = _dictionaryConstructed.AddOrUpdate(_instanceMongoContextImplementation, (key) =>
            {
                var collectionName = this.Database.ListCollectionNames().ToEnumerable().ToArray();

                return collectionName;
                //return collectionName.Select(collectionName => $"{fullname}-{this.DatabaseName}.{collectionName}").ToList();
            }, (key, value) => value);



            //this.MapAsync(classMaps).Wait();

            //this.IndexAsync().Wait();
        }







        #region Before

        //
        internal void BeforeInsertInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref TDocument original, ref ReplaceOptions replaceOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);

            OptionObject<TDocument, TProjection> option = replaceOptions;

            this.BeforeInsert<TDocument, TObjectId, TProjection, TUser>(sender, ref original, ref option);

            replaceOptions = option;
        }

        internal void BeforeInsertInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref TDocument original, ref InsertOneOptions insertOneOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);

            OptionObject<TDocument, TProjection> option = insertOneOptions;

            this.BeforeInsert<TDocument, TObjectId, TProjection, TUser>(sender, ref original, ref option);

            insertOneOptions = option;
        }


        //internal void BeforeInsertInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref TDocument original, ref OptionObject<TDocument, TProjection> option)
        //    where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
        //    where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        //{
        //    ArgumentNullException.ThrowIfNull(sender);

        //    this.BeforeInsert<TDocument, TObjectId, TProjection, TUser>(sender, ref original, ref option);
        //}

        //
        internal void BeforeUpdateInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref UpdateDefinition<TDocument> updateDefinition, ref UpdateOptions updateOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);

            OptionObject<TDocument, TProjection> option = updateOptions;

            this.BeforeUpdate<TDocument, TObjectId, TProjection, TUser>(sender, ref filterDefinition, ref updateDefinition, ref option);
        }

        internal void BeforeUpdateInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref UpdateDefinition<TDocument> updateDefinition, ref FindOneAndUpdateOptions<TDocument, TProjection> findOneAndUpdateOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);

            OptionObject<TDocument, TProjection> option = findOneAndUpdateOptions;

            this.BeforeUpdate<TDocument, TObjectId, TProjection, TUser>(sender, ref filterDefinition, ref updateDefinition, ref option);
        }

        //internal void BeforeUpdateInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref UpdateDefinition<TDocument> updateDefinition, ref OptionObject<TDocument, TProjection> option)
        //    where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
        //    where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        //{
        //    ArgumentNullException.ThrowIfNull(sender);

        //    this.BeforeUpdate<TDocument, TObjectId, TProjection, TUser>(sender, ref filterDefinition, ref updateDefinition, ref option);
        //}

        internal void BeforeReplaceInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, TDocument original, ref FilterDefinition<TDocument> filterDefinition, ref ReplaceOptions replaceOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);
            OptionObject<TDocument, TProjection> option = replaceOptions;
            this.BeforeReplace<TDocument, TObjectId, TProjection, TUser>(sender, ref original, ref filterDefinition, ref option);
            replaceOptions = option;
        }

        //AggregateOptions
        internal void BeforeAggregateInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref BsonDocument[] original, ref AggregateOptions aggregateOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);
            OptionObject<TDocument, TProjection> option = aggregateOptions;
            this.BeforeAggregate<TDocument, TObjectId, TProjection, TUser>(sender, ref original, ref option);
            aggregateOptions = option;
        }

        //internal void BeforeAggregateInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref BsonDocument[] original, ref OptionObject<TDocument, TProjection> option)
        //    where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
        //    where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        //{
        //    ArgumentNullException.ThrowIfNull(sender);
        //    this.BeforeAggregate<TDocument, TObjectId, TProjection, TUser>(sender, ref original, ref option);
        //}

        internal void BeforeFindInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref CountOptions countOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);
            OptionObject<TDocument, TProjection> option = countOptions;
            this.BeforeFind<TDocument, TObjectId, TProjection, TUser>(sender, ref filterDefinition, ref option);
            countOptions = option;
        }

        
        internal void BeforeFindInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref FindOptions<TDocument, TProjection> findOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);
            OptionObject<TDocument, TProjection> option = findOptions;
            this.BeforeFind<TDocument, TObjectId, TProjection, TUser>(sender, ref filterDefinition, ref option);
            findOptions = option;
        }

        //internal void BeforeFindInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref OptionObject<TDocument, TProjection> option)
        //    where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
        //    where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        //{
        //    ArgumentNullException.ThrowIfNull(sender);
        //    this.BeforeFind<TDocument, TObjectId, TProjection, TUser>(sender, ref filterDefinition, ref option);
        //}

        
        internal void BeforeQueryableInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref IQueryable<TDocument> queryable, ref AggregateOptions aggregateOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);

            OptionObject<TDocument, TProjection> option = aggregateOptions;
            this.BeforeQueryable<TDocument, TObjectId, TProjection, TUser>(sender, ref queryable, ref option);
            aggregateOptions = option;
        }

        //internal void BeforeQueryableInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref IQueryable<TDocument> queryable, ref OptionObject<TDocument, TProjection> option)
        //    where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
        //    where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        //{
        //    ArgumentNullException.ThrowIfNull(sender);
        //    this.BeforeQueryable<TDocument, TObjectId, TProjection, TUser>(sender, ref queryable, ref option);
        //}


        internal void BeforeDeleteInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref DeleteOptions deleteOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);
            OptionObject<TDocument, TProjection> option = deleteOptions;
            this.BeforeDelete<TDocument, TObjectId, TProjection, TUser>(sender, ref filterDefinition, ref option);
            deleteOptions = option;
        }

        //BulkWriteOptions
        internal void BeforeBulkWriteInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref IEnumerable<WriteModel<TDocument>> writeModels, ref BulkWriteOptions bulkWriteOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);
            OptionObject<TDocument, TProjection> option = bulkWriteOptions;
            this.BeforeBulkWrite<TDocument, TObjectId, TProjection, TUser>(sender, ref writeModels, ref option);
            bulkWriteOptions = option;
        }
        //internal void BeforeBulkWriteInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref IEnumerable<WriteModel<TDocument>> writeModels, ref OptionObject<TDocument, TProjection> option)
        //    where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
        //    where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        //{
        //    ArgumentNullException.ThrowIfNull(sender);
        //    this.BeforeBulkWrite<TDocument, TObjectId, TProjection, TUser>(sender, ref writeModels, ref option);
        //}

        internal void ResultInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref IEnumerable<TProjection> results)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);

            this.Result<TDocument, TObjectId, TProjection, TUser>(sender, ref results);
        }

        internal void ResultInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref TProjection result)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);

            this.Result<TDocument, TObjectId, TProjection, TUser>(sender, ref result);
        }




        protected virtual void BeforeInsert<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref TDocument original, ref OptionObject<TDocument, TProjection> option)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }

        protected virtual void BeforeUpdate<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref UpdateDefinition<TDocument> updateDefinition, ref OptionObject<TDocument, TProjection> option)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }

        protected virtual void BeforeReplace<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref TDocument original, ref FilterDefinition<TDocument> filterDefinition, ref OptionObject<TDocument, TProjection> option)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }

        protected virtual void BeforeAggregate<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref BsonDocument[] bsonDocuments, ref OptionObject<TDocument, TProjection> option)
                    where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
                    where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }

        protected virtual void BeforeDelete<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref OptionObject<TDocument, TProjection> option)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }

        protected virtual void BeforeFind<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref OptionObject<TDocument, TProjection> option)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }

        protected virtual void BeforeQueryable<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref IQueryable<TDocument> queryable, ref OptionObject<TDocument, TProjection> option)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }

        protected virtual void BeforeBulkWrite<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref IEnumerable<WriteModel<TDocument>> writeModels, ref OptionObject<TDocument, TProjection> option)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }


        protected virtual void Result<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref IEnumerable<TProjection> items)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }

        protected virtual void Result<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref TProjection item)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }
        #endregion



        /// <summary>
        /// Gets a DbSet instance for the specified document type and object ID type.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TObjectId">The type of the object ID, which must implement IComparable and IEquatable interfaces.</typeparam>
        /// <param name="collectionName">An optional name for the MongoDB collection. If not provided, a default collection name will be used.</param>
        /// <param name="createCollectionOptionsAction">An optional action to configure the collection options when creating a new collection.</param>
        /// <param name="mongoCollectionSettingsAction">An optional action to configure the MongoDB collection settings.</param>
        /// <param name="useTransaction">An optional boolean that indicates whether to use a transaction. Defaults to the current transactional context if not provided.</param>
        /// <param name="throwIndexExceptions"></param>
        /// <returns>A DbSet instance that can be used for querying and managing documents of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any of the required parameters are null or invalid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet<TDocument, TObjectId> GetDbSet<TDocument, TObjectId>(
            string? collectionName = null,
            Action<CreateCollectionOptions>? createCollectionOptionsAction = null,
            Action<MongoCollectionSettings>? mongoCollectionSettingsAction = null,
            bool? useTransaction = default,
            bool throwIndexExceptions = false)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId>, IObjectBaseTenant => new(this, collectionName, createCollectionOptionsAction, mongoCollectionSettingsAction, useTransaction ?? this.TransactionalContext, throwIndexExceptions);

        /// <summary>
        /// Retrieves a DbSet of the specified document type from the database.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document that represents the collection.</typeparam>
        /// <param name="collectionName">An optional name of the collection. If not provided, a default is used.</param>
        /// <param name="createCollectionOptionsAction">An optional action to configure the creation options for the collection.</param>
        /// <param name="mongoCollectionSettingsAction">An optional action to configure the MongoDB collection settings.</param>
        /// <param name="useTransaction">Optional value indicating whether to use a transaction.</param>
        /// <param name="throwIndexExceptions"></param>
        /// <returns>A DbSet of the specified document type.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the operation cannot be performed.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet<TDocument> GetDbSet<TDocument>(
                    string? collectionName = null,
                    Action<CreateCollectionOptions>? createCollectionOptionsAction = null,
                    Action<MongoCollectionSettings>? mongoCollectionSettingsAction = null,
                    bool? useTransaction = default,
                    bool throwIndexExceptions = false)
                    where TDocument : IObjectBase, IObjectBaseTenant => new(this, collectionName, createCollectionOptionsAction, mongoCollectionSettingsAction, useTransaction ?? this.TransactionalContext, throwIndexExceptions);





        /// <summary>
        /// Gets the collection names from the instance collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of strings representing the names of the collections.
        /// </returns>
        /// <remarks>
        /// The method uses aggressive inlining to improve performance by reducing the overhead of method calls.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<string> CollectionNames() => this._instanceCollectionNames;


        /// <summary>
        /// Initiates a new client session if one is not already active.
        /// </summary>
        /// <returns>
        /// An <see cref="IClientSessionHandle"/> representing the current session.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IClientSessionHandle StartSession()
        {
            lock (this._transactionContextLock)
            {
                // Start a new session
                this.ContextSession ??= this.CreateSession();
            }

            // Return the session
            return this.ContextSession!;
        }

        /// <summary>
        /// Creates a new client session by starting a session 
        /// with the associated client. This method returns an 
        /// instance of <see cref="IClientSessionHandle"/> that 
        /// represents the client session.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="IClientSessionHandle"/> that 
        /// is used to manage the client session.
        /// </returns>
        public IClientSessionHandle CreateSession() => this.MongoClient.StartSession();

        /// <summary>
        /// Starts a new transaction within the current client session.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="IClientSessionHandle"/> representing the client session handle with the new transaction started.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IClientSessionHandle StartTransaction()
        {
            var result = this.StartSession();

            lock (this._transactionContextLock)
            {
                if (!result.IsInTransaction)
                {
                    // Start the transaction
                    result.StartTransaction();
                }
            }

            return result;
        }

        /// <summary>
        /// Aborts the current transaction if it has been started.
        /// Throws an InvalidOperationException if the transaction has not been initiated.
        /// </summary>
        /// <remarks>
        /// This method ensures that the transaction aborts safely, releasing any resources associated
        /// with the transaction context. It uses a lock to prevent concurrent access to the transaction
        /// context, ensuring thread safety.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to abort a transaction that has not been started.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AbortTransaction()
        {
            if (!(this.ContextSession?.IsInTransaction ?? false))
                throw new InvalidOperationException("Transaction has not been started.");

            lock (this._transactionContextLock)
            {
                this.ContextSession!.AbortTransaction();

                //this.TransactionalContext = false;

                this.ContextSession.Dispose();

                this.ContextSession = this.CreateSession();
            }
        }

        /// <summary>
        /// Commits the current transaction if it has been started.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to commit a transaction that has not been started.
        /// </exception>
        /// <remarks>
        /// This method ensures that the transaction is committed, releases resources associated with the session,
        /// and updates the transaction state.
        /// </remarks>
        /// <seealso cref="TransactionContextStarted"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CommitTransaction()
        {
            if (!(this.ContextSession?.IsInTransaction ?? false))
                throw new InvalidOperationException("Transaction has not been started.");

            lock (this._transactionContextLock)
            {
                // Commit the transaction
                this.ContextSession?.CommitTransaction();

                // Dispose of the session to release resources
                this.ContextSession?.Dispose();

                // Set the session to null to indicate that the transaction has been committed
                this.ContextSession = this.CreateSession();
            }
        }

        #region Dispose

        /// <summary>
        /// Releases the resources used by the instance of the class.
        /// This method can be called by derived classes to implement 
        /// disposal of both managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// A boolean indicating whether the method has been called directly 
        /// or indirectly by a user's code (true) or by the runtime (false). 
        /// When disposing is true, the method has been called directly 
        /// by a user code, and managed and unmanaged resources can be disposed. 
        /// When disposing is false, the method has been called by the runtime 
        /// from the finalizer and only unmanaged resources should be disposed.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    this.ContextSession?.Dispose();
                    this.MongoClient?.Dispose();
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
        /// Override this only if the 'Dispose(bool disposing)' method contains 
        /// code to free unmanaged resources.
        /// </summary>
        /// <remarks>
        /// The finalizer should invoke the Dispose method with 'disposing' set to false,
        /// to ensure that resources are properly released when the object is garbage collected.
        /// </remarks>
        /// <example>
        /// <code>
        /// ~ContextBase()
        /// {
        ///     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        ///     Dispose(disposing: false);
        /// }
        /// </code>
        /// </example>
        ~ContextBase()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// Disposes of the resources used by the ContextBase class.
        /// This is the 
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Gets the instance of the MongoClient associated with this class.
        /// </summary>
        /// <returns>A MongoClient instance that represents the connection to the MongoDB database.</returns>
        public MongoClient GetMongoClient() => this.MongoClient;

        public static implicit operator MongoClient(ContextBase context) => context.MongoClient;

    }
}
