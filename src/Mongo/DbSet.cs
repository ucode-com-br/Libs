using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using UCode.Extensions;
using UCode.Mongo.Models;
using UCode.Mongo.Options;
using UCode.Repositories;

namespace UCode.Mongo
{
    //https://mongodb.github.io/mongo-csharp-driver/2.11/reference/driver/crud/writing/
    /// <summary>
    /// Represents a collection of entities that can be queried from a database and 
    /// saved to the database in a generic manner. This class inherits from 
    /// <see cref="DbSet{TDocument,TKey,TValue}" /> with string as the type for the key 
    /// and value parameters.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document being managed in the set.</typeparam>
    public class DbSet<TDocument> : DbSet<TDocument, string, string>
            where TDocument : IObjectBase<string, string>, IObjectBaseTenant
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DbSet"/> class.
        /// </summary>
        /// <param name="contextBase">The context base that the DbSet operates within. This parameter cannot be null.</param>
        /// <param name="collectionName">The optional name of the collection; if null, the default collection name will be used.</param>
        /// <param name="createCollectionOptionsAction">An optional action to configure the creation options for the collection.</param>
        /// <param name="mongoCollectionSettingsAction">An optional action to configure the MongoDB collection settings.</param>
        /// <param name="useTransaction">A boolean value indicating whether to use transactions for operations on this DbSet.</param>
        /// <param name="throwIndexExceptions">A boolean value indicating whether to throw exceptions related to indexing operations.</param>
        /// <returns>
        /// A <see cref="DbSet"/> object representing the specified collection in the MongoDB context.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet([NotNull] ContextBase contextBase, string? collectionName = null,
            Action<CreateCollectionOptions>? createCollectionOptionsAction = null,
            Action<MongoCollectionSettings>? mongoCollectionSettingsAction = null,
            bool useTransaction = false,
            bool throwIndexExceptions = false) :
                base(contextBase, collectionName, createCollectionOptionsAction, mongoCollectionSettingsAction, useTransaction, throwIndexExceptions)
        {

        }

        /// <summary>
        /// Returns a string representation of the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object, or null if the base 
        /// object does not provide one. This method overrides the 
        /// <see cref="object.ToString"/> method.
        /// </returns>
        public override string? ToString() => base.ToString();
    }



    /// <summary>
    /// Represents a collection of entities of a specified type that can be queried from the database.
    /// This class inherits from the generic DbSet class with a string as the third type parameter.
    /// </summary>
    /// <typeparam name="TDocument">The type of the entity that will be stored in the database.</typeparam>
    /// <typeparam name="TObjectId">The type of the identifier for the entity.</typeparam>
    public class DbSet<TDocument, TObjectId> : DbSet<TDocument, TObjectId, string>
            where TDocument : IObjectBase<TObjectId, string>, IObjectBaseTenant
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DbSet"/> class.
        /// </summary>
        /// <param name="contextBase">The database context that this DbSet belongs to.</param>
        /// <param name="collectionName">The optional name of the MongoDB collection. If not provided, the default name will be used.</param>
        /// <param name="createCollectionOptionsAction">An optional action to configure options for creating the MongoDB collection.</param>
        /// <param name="mongoCollectionSettingsAction">An optional action to apply settings to the MongoDB collection.</param>
        /// <param name="useTransaction">Indicates whether to use transactions for operations on this collection. The default is false.</param>
        /// <param name="throwIndexExceptions">Indicates whether to throw exceptions related to index operations. The default is false.</param>
        /// <returns>
        /// A new instance of the <see cref="DbSet"/> class.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet([NotNull] ContextBase contextBase, string? collectionName = null,
            Action<CreateCollectionOptions>? createCollectionOptionsAction = null,
            Action<MongoCollectionSettings>? mongoCollectionSettingsAction = null,
            bool useTransaction = false,
            bool throwIndexExceptions = false) : base(contextBase, collectionName, createCollectionOptionsAction, mongoCollectionSettingsAction, useTransaction, throwIndexExceptions)
        {

        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// Overrides the ToString method of the base class.
        /// </summary>
        /// <returns>
        /// A string that represents the current object, or null if the base ToString method returns null.
        /// </returns>
        public override string? ToString() => base.ToString();
    }


    //https://mongodb.github.io/mongo-csharp-driver/2.11/reference/driver/crud/writing/
    /// <summary>
    /// Represents a set of entities of a specific type that can be queried from a database.
    /// This class implements both <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/> 
    /// interfaces, allowing for proper resource management at both synchronous and 
    /// asynchronous levels.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document stored in the database.</typeparam>
    /// <typeparam name="TObjectId">The type of the identifier for the document.</typeparam>
    /// <typeparam name="TUser">The type that represents a user interacting with the document.</typeparam>
    public class DbSet<TDocument, TObjectId, TUser> : IDisposable, IAsyncDisposable
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        #region Fields
        protected readonly IMongoCollection<TDocument> MongoCollection;

        private ContextCollectionMetadata _contextCollectionMetadata;

        private IClientSessionHandle? _sessionHandle = null;


        private readonly ContextBase _contextbase;


        /// <summary>
        /// Gets the logger instance used for logging activities related to 
        /// the DbSet of documents.
        /// </summary>
        /// <remarks>
        /// The logger is of type <see cref="ILogger{T}"/> where T is a 
        /// generic combination of TDocument, TObjectId, and TUser. This allows 
        /// for structured logging specific to the DbSet context.
        /// </remarks>
        /// <value>
        /// An instance of <see cref="ILogger{T}"/> specifically configured 
        /// for logging operations within the DbSet of documents.
        /// </value>
        protected ILogger<DbSet<TDocument, TObjectId, TUser>> Logger
        {
            get;
        }

        /// <summary>
        /// Gets the name of the collection.
        /// </summary>
        /// <value>
        /// A string representing the name of the collection.
        /// </value>
        public string CollectionName
        {
            get;
        }

        

        #endregion Fields


        #region private/internal methods
        /// <summary>
        /// Determines whether a transaction should be started and provides a handle to the client session.
        /// </summary>
        /// <param name="forceTransaction">
        /// A nullable boolean value that indicates whether to force a transaction. 
        /// If true, a transaction will be started even if there is no existing transactional context.
        /// </param>
        /// <param name="clientSessionHandle">
        /// An output parameter that returns the client session handle if a transaction is started, otherwise null.
        /// </param>
        /// <returns>
        /// Returns true if a transaction has been initialized and a valid client session handle is provided; otherwise returns false.
        /// </returns>
        private bool InTransaction(bool? forceTransaction, out IClientSessionHandle? clientSessionHandle)
        {
            if (this._sessionHandle != null && !this._sessionHandle.IsInTransaction)
            {
                this._sessionHandle.StartTransaction();

                clientSessionHandle = this._sessionHandle;
            }
            else if ((!forceTransaction.HasValue || (forceTransaction.HasValue && !forceTransaction.Value)) && this._contextbase.TransactionalContext)
            {
                clientSessionHandle = this._contextbase.StartTransaction();
            }
            else if (forceTransaction.HasValue && forceTransaction.Value)
            {
                if (this._sessionHandle == null)
                {
                    this._sessionHandle = this._contextbase.CreateSession();
                }

                if (!this._sessionHandle.IsInTransaction)
                {
                    this._sessionHandle.StartTransaction();
                }

                clientSessionHandle = this._sessionHandle;
            }
            else
            {
                clientSessionHandle = null;
            }

            return clientSessionHandle != default;
        }

        /// <summary>
        /// Converts the given <see cref="AggregateOptions"/> to <see cref="AggregateOptionsPaging"/>.
        /// </summary>
        /// <param name="findOptionsPaging">
        /// The options to convert, or <c>null</c> if there are no options to convert.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="AggregateOptionsPaging"/> that contains the copied settings 
        /// from the specified <see cref="AggregateOptions"/>, or a default instance if the 
        /// input options were <c>null</c>.
        /// </returns>
        private static AggregateOptionsPaging ConvertInternal(AggregateOptions? findOptionsPaging)
        {
            var opt = new AggregateOptionsPaging();

            if (findOptionsPaging != default)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(findOptionsPaging);

                opt = System.Text.Json.JsonSerializer.Deserialize<AggregateOptionsPaging>(json)!;
            }

            return opt;
        }

        /// <summary>
        /// Converts an optional <see cref="FindOptions"/> instance to a strongly typed
        /// <see cref="FindOptions{TDocument, TDocument}"/> while handling serialization
        /// and deserialization.
        /// </summary>
        /// <param name="findOptions">
        /// An optional <see cref="FindOptions"/> instance to be converted.
        /// </param>
        /// <returns>
        /// A <see cref="FindOptions{TDocument, TDocument}"/> instance that corresponds
        /// to the provided optional <see cref="FindOptions"/>. If the input is null,
        /// a new instance with default values is returned.
        /// </returns>
        private static FindOptions<TDocument, TDocument> ConvertInternal(FindOptions? findOptions)
        {
            var opt = new FindOptions<TDocument, TDocument>();

            if (findOptions != default)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(findOptions);

                opt = System.Text.Json.JsonSerializer.Deserialize<FindOptions<TDocument, TDocument>>(json)!;
            }

            return opt;
        }

        /// <summary>
        /// Converts the provided <see cref="FindOptions{TDocument}"/> to a valid 
        /// <see cref="FindOptions{TDocument}"/> instance. If the provided 
        /// <paramref name="findOptions"/> is null, a new instance of 
        /// <see cref="FindOptions{TDocument}"/> is created and returned.
        /// </summary>
        /// <param name="findOptions">
        /// An optional instance of <see cref="FindOptions{TDocument}"/>. 
        /// This can be null.
        /// </param>
        /// <returns>
        /// A <see cref="FindOptions{TDocument}"/> instance, either the provided 
        /// <paramref name="findOptions"/> or a new instance if <paramref name="findOptions"/> is null.
        /// </returns>
        private static FindOptions<TDocument, TDocument> ConvertInternal(FindOptions<TDocument>? findOptions) => findOptions ?? new FindOptions<TDocument, TDocument>();

        private static FindOptions<TDocument, TProjection> ConvertInternal<TProjection>(FindOptions<TDocument, TProjection>? findOptions) => findOptions ?? new FindOptions<TDocument, TProjection>();

        /// <summary>
        /// Registers class maps for BSON serialization if they are not already registered.
        /// It retrieves all existing BSON class maps and ensures each one is registered before yielding it.
        /// </summary>
        /// <returns>
        /// An enumerable collection of registered <see cref="BsonClassMap"/> objects.
        /// </returns>
        private IEnumerable<BsonClassMap> ReflectionRegisterClassMap()
        {
            var result = this.GetBsonClassMaps().ToArray();

            foreach (var bsonClassMap in result)
            {
                if (!BsonClassMap.IsClassMapRegistered(bsonClassMap.ClassType))
                {
                    BsonClassMap.RegisterClassMap(bsonClassMap);
                }

                yield return bsonClassMap;
            }
        }

        /// <summary>
        /// Retrieves a collection of BsonClassMap instances by inspecting the properties and methods 
        /// of the current context type related to the mapping of generic types.
        /// </summary>
        /// <returns>
        /// An enumerable collection of <see cref="BsonClassMap"/> instances, each representing a mapping
        /// for a particular type found within the context.
        /// </returns>
        private IEnumerable<BsonClassMap> GetBsonClassMaps()
        {
            var thisType = this._contextbase.GetType();

            var props = thisType.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty)
                .Where(w => w.PropertyType.IsGenericType && (w.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) || w.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<,,>)));

            var methods = thisType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                .Where(w => w.Name.Equals("Map", StringComparison.Ordinal) && w.GetParameters().Length > 0 && w.GetParameters().All(a => a.ParameterType.IsGenericType && a.ParameterType.GetGenericTypeDefinition() == typeof(BsonClassMap<>)))
                .Select(s => new { BsonClassMap = s.GetParameters()[0].ParameterType, BsonClassMapGeneric = s.GetParameters()[0].ParameterType.GenericTypeArguments[0], Method = s }).ToArray();

            foreach (var prop in props)
            {
                var objectIdImplementationType = prop.PropertyType.GenericTypeArguments[0];

                var bsonClassMapType = typeof(BsonClassMap<>).MakeGenericType(objectIdImplementationType);

                var basonClassMap = (BsonClassMap)Activator.CreateInstance(typeof(BsonClassMap<>).MakeGenericType(objectIdImplementationType))!;

                basonClassMap.MapExtraElementsProperty("ExtraElements");

                yield return basonClassMap;
            }
        }

        /// <summary>
        /// Asynchronously performs a bulk write operation on a MongoDB collection.
        /// </summary>
        /// <param name="listWriteModel">A list of write models specifying the operations to be performed.</param>
        /// <param name="bulkWriteOptions">Options to control the bulk write operation.</param>
        /// <param name="forceTransaction">Indicates whether to force the operation to run in a transaction.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{long}"/> that represents the asynchronous operation,
        /// containing the total count of documents inserted, updated, matched, or deleted.
        /// Returns -1 if the operation was not acknowledged.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async Task<long> BulkWriteAsync([NotNull] List<WriteModel<TDocument>> listWriteModel,
                    [NotNull] BulkWriteOptions bulkWriteOptions,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Perform the bulk write operation based on the provided options
            BulkWriteResult<TDocument> result;

            IEnumerable<WriteModel<TDocument>> writeModels = listWriteModel;

            this._contextbase.BeforeBulkWriteInternal<TDocument, TObjectId, TDocument, TUser>(this, ref writeModels, ref bulkWriteOptions);

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the bulk write operation with a session
                result = await this.MongoCollection.BulkWriteAsync(clientSessionHandle, writeModels, bulkWriteOptions, cancellationToken);
            }
            else
            {
                // Perform the bulk write operation without a session
                result = await this.MongoCollection.BulkWriteAsync(writeModels, bulkWriteOptions, cancellationToken);
            }

            // Check if the result is default
            if (result == default)
            {
                // Return -1 if the operation was not acknowledged
                return -1;
            }

            this._contextbase.ResultInternal<TDocument, TObjectId, BulkWriteResult<TDocument>, TUser>(this, ref result);

            // Return the total count of documents inserted, updated, matched, or deleted if the operation was acknowledged
            if (result.IsAcknowledged)
            {
                return result.DeletedCount + result.ModifiedCount + result.MatchedCount + result.InsertedCount + result.RequestCount;
            }

            return -1;
        }
        #endregion private/internal methods

        #region constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DbSet{TDocument, TObjectId, TUser}"/> class.
        /// This constructor sets up a collection in the given context, applies optional create 
        /// collection and MongoDB collection settings, and registers necessary indexes. 
        /// Additionally, it handles transaction management and logger initialization.
        ///
        /// </summary>
        /// <param name="contextBase">The context base used to interact with the MongoDB database.</param>
        /// <param name="collectionName">The name of the collection to be used; if null, defaults to 'TDocumentCollection'.</param>
        /// <param name="createCollectionOptionsAction">
        /// An optional action to configure the collection options before creating the collection.
        /// </param>
        /// <param name="mongoCollectionSettingsAction">
        /// An optional action to configure the MongoDB collection settings.
        /// </param>
        /// <param name="useTransaction">
        /// A value indicating whether to use transactions while interacting with the database.
        /// </param>
        /// <param name="thowIndexExceptions">
        /// A value indicating whether to throw exceptions related to index creation.
        /// </param>
        /// <returns>
        /// A new instance of the <see cref="DbSet{TDocument, TObjectId, TUser}"/> class.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet([NotNull] ContextBase contextBase, string? collectionName = null,
            Action<CreateCollectionOptions>? createCollectionOptionsAction = null,
            Action<MongoCollectionSettings>? mongoCollectionSettingsAction = null,
            bool useTransaction = false,
            bool thowIndexExceptions = false)
        {
            this._contextbase = contextBase;

            if (useTransaction)
            {
                this._sessionHandle = this._contextbase.CreateSession();
            }


            var colNames = contextBase.CollectionNames().ToArray();

            if (colNames.All(f => !f.Equals(collectionName ?? $"{nameof(TDocument)}Collection", StringComparison.Ordinal)))
            {
                var createCollectionOptions = new CreateCollectionOptions();

                createCollectionOptionsAction?.Invoke(createCollectionOptions);

                contextBase.Database.CreateCollection(collectionName ?? $"{nameof(TDocument)}Collection", createCollectionOptions);
            }

            this.CollectionName = collectionName ?? $"{nameof(TDocument)}Collection";

            var mongoCollectionSettings = new MongoCollectionSettings() { AssignIdOnInsert = true };

            mongoCollectionSettingsAction?.Invoke(mongoCollectionSettings);

            // Initialize the MongoDB collection
            this.MongoCollection = contextBase.Database.GetCollection<TDocument>(this.CollectionName, mongoCollectionSettings);



            // Initialize the logger
            this.Logger = contextBase.LoggerFactory.CreateLogger<DbSet<TDocument, TObjectId, TUser>>();


            if (!this._contextbase._contextCollectionMetadata.TryGetValue(this.CollectionName, out var contextCollectionMetadata))
            {
                this._contextCollectionMetadata = contextCollectionMetadata;
            }
            else
            {
                var indexKeys = new IndexKeys<TDocument>();

                indexKeys.Ascending(x => x.Ref, (option) =>
                {
                    // Create the index in the background to avoid blocking other operations
                    option.Background = true;
                    option.Unique = true;
                    // Name the index for easy reference
                    option.Name = "IDX_REF";
                });

                indexKeys.Ascending(x => x.Disabled, (option) =>
                {
                    option.Background = true;
                    option.Unique = false;
                    option.Name = "IDX_DISABLED";
                });

                indexKeys.Ascending(x => x.Ref).Ascending(x => x.Disabled, (option) =>
                {
                    option.Background = true;
                    option.Unique = false;
                    option.Name = "IDX_REF_DISABLED";
                });

                indexKeys.Ascending(x => x.Tenant, (option) =>
                {
                    option.Background = true;
                    option.Unique = false;
                    option.Name = "IDX_TENANT";
                });

                indexKeys.Ascending(x => x.Tenant).Ascending(x => x.Ref).Ascending(x => x.Disabled, (option) =>
                {
                    option.Background = true;
                    option.Unique = false;
                    option.Name = "IDX_TENANT_REF_DISABLED";
                });


                this._contextCollectionMetadata = new ContextCollectionMetadata(this.CollectionName)
                {
                    IndexKeys = indexKeys,
                    BsonClassMaps = this.ReflectionRegisterClassMap()
                };


                this._contextbase._contextCollectionMetadata.Add(this.CollectionName, this._contextCollectionMetadata);
            }
            //this._contextCollectionMetadata = this._contextbase._contextCollectionMetadata[this.CollectionName];

            _ = this.InternalIndex(false, thowIndexExceptions);
        }
        #endregion constructor







        #region index methods

        /// <summary>
        /// Asynchronously retrieves a collection of indexes along with their associated field names from the MongoDB collection.
        /// This method utilizes asynchronous iteration and can be called within an async context.
        /// </summary>
        /// <param name="forceTransaction">
        /// An optional boolean that indicates whether to force the operation to run within a transaction.
        /// If null, the method will decide whether to start a transaction based on the current context.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to propagate notification that the operation should be canceled.
        /// </param>
        /// <returns>
        /// An asynchronous enumerable collection of key-value pairs where each pair contains the name of the index
        /// and a list of field names that are included in that index.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled before completion.
        /// </exception>
        /// <exception cref="MongoException">
        /// Thrown when an error occurs while retrieving indexes from the MongoDB collection.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<KeyValuePair<string, List<string>>> GetIndexesAsync(bool? forceTransaction = default,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            IAsyncCursor<BsonDocument> idxsb = null;

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                idxsb = await this.MongoCollection.Indexes.ListAsync(session: clientSessionHandle, cancellationToken);
            }
            else
            {
                idxsb = await this.MongoCollection.Indexes.ListAsync(cancellationToken);
            }

            // Iterate through each batch of indexes
            while (idxsb.MoveNext(cancellationToken))
            {
                var idxs = idxsb.Current;

                // Iterate through each index in the current batch
                foreach (var idx in idxs)
                {
                    // Get the name of the index
                    var name = idx["name"].AsString;

                    // Create a list to store the fields indexed by the index
                    var keysList = new List<string>();

                    // Iterate through each key in the index
                    foreach (var keys in idx["key"].AsBsonArray)
                    {
                        // Iterate through each element in the key document
                        foreach (var item in keys.AsBsonDocument.Elements)
                        {
                            // Add the name of the element to the keysList
                            keysList.Add(item.Name);
                        }
                    }

                    // Yield the key-value pair representing the index and its fields
                    yield return new KeyValuePair<string, List<string>>(name, keysList);
                }
            }

        }



        /// <summary>
        /// Tries to create indexes on the underlying context for the specified document type.
        /// </summary>
        /// <param name="forceTransaction">Optional parameter to indicate if the index creation should be forced as a transaction. Default is null.</param>
        /// <param name="thowIndexExceptions">Optional parameter to specify if exceptions during the index creation should be thrown. Default is false.</param>
        /// <returns>True if the index creation is successful; otherwise, false.</returns>
        private bool InternalIndex(bool? forceTransaction = default, bool thowIndexExceptions = false)
        {
            var contextbaseType = this._contextbase.GetType();
            var methods = contextbaseType.GetMethods();

            var index_method = methods.SingleOrDefault(w => w.Name.Equals("Index", StringComparison.Ordinal) && w.GetParameters().Length == 1 && w.GetParameters()[0].ParameterType == typeof(IndexKeys<TDocument>));

            index_method?.Invoke(this._contextbase, [(IndexKeys<TDocument>)this._contextCollectionMetadata.IndexKeys]);

            try
            {
                var indexKeys = (IndexKeys<TDocument>)this._contextCollectionMetadata.IndexKeys;
                var index = this.IndexAsync(indexKeys, forceTransaction);
                index.Wait();
                return index.Result;
            }
            catch (Exception ex)
            {
                this.Logger.LogError(exception: ex, "Fail create indexes.", this._contextCollectionMetadata.IndexKeys);

                if (thowIndexExceptions)
                    throw;

                return false;
            }
        }

        /// <summary>
        /// Asynchronously creates an index on the specified document collection using the provided index keys.
        /// </summary>
        /// <param name="indexKeys">The keys to be used for indexing the documents. Must not be null.</param>
        /// <param name="forceTransaction">Optional. If true, forces the operation to run inside a transaction.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation, containing a boolean value indicating success or failure of the index creation.</returns>
        public async Task<bool> IndexAsync([NotNull] IndexKeys<TDocument> indexKeys,
            bool? forceTransaction = default,
            CancellationToken cancellationToken = default) => await this.IndexAsync((List<CreateIndexModel<TDocument>>)indexKeys, forceTransaction, cancellationToken);


        /// <summary>
        /// Asynchronously creates multiple indexes on the MongoDB collection.
        /// </summary>
        /// <param name="models">
        /// A list of <see cref="CreateIndexModel{TDocument}"/> instances that define the indexes to be created.
        /// </param>
        /// <param name="forceTransaction">
        /// An optional boolean value that, when set to true, forces the creation of the indexes within a transaction.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> indicating whether the indexes were successfully created.
        /// </returns>
        public async Task<bool> IndexAsync([NotNull] List<CreateIndexModel<TDocument>> models,
            bool? forceTransaction = default,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if session should be used
                if (this.InTransaction(forceTransaction, out var clientSessionHandle))
                {
                    // Use the session to create the indexes
                    _ = await this.MongoCollection.Indexes.CreateManyAsync(clientSessionHandle, models, cancellationToken);
                }
                else
                {
                    // Create the indexes without a session
                    _ = await this.MongoCollection.Indexes.CreateManyAsync(models, cancellationToken);
                }

                return true;
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    throw ex;
                }

                return false;
            }
        }
        #endregion index methods

        #region queryable
        /// <summary>
        /// Converts the current collection into an <see cref="IQueryable{TDocument}"/>.
        /// This allows for building LINQ queries against the collection.
        /// </summary>
        /// <param name="aggregateOptions">
        /// Optional aggregate options to configure the behavior of the query.
        /// If null, defaults to new <see cref="AggregateOptions"/>.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional flag to indicate whether to force the use of a transaction.
        /// If not provided, defaults to the default value for the boolean type.
        /// </param>
        /// <returns>
        /// An <see cref="IQueryable{TDocument}"/> representing the documents in the collection.
        /// </returns>
        public IQueryable<TDocument> AsQueryable(AggregateOptions? aggregateOptions = null,
                    bool? forceTransaction = default)
        {
            IQueryable<TDocument> queryable;

            var option = aggregateOptions ?? new AggregateOptions();

            // If a transaction is in use
            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the replace operation with a session
                queryable = this.MongoCollection.AsQueryable(clientSessionHandle, option);
            }
            // If no transaction is in use
            else
            {
                // Perform the replace operation without a session
                queryable = this.MongoCollection.AsQueryable(option);
            }

            this._contextbase.BeforeQueryableInternal<TDocument, TObjectId, IQueryable<TDocument>, TUser>(this, ref queryable, ref option);

            return queryable;
        }

        /// <summary>
        /// Converts the current instance to an <see cref="IQueryable{TDocument}"/>.
        /// This method allows for the optional application of a provided transformation function 
        /// to the queryable data.
        /// </summary>
        /// <param name="preApprend">
        /// A function that takes an <see cref="IQueryable{TDocument}"/> and returns a 
        /// transformed <see cref="IQueryable{TDocument}"/>. If null, the original 
        /// queryable will be returned.
        /// </param>
        /// <param name="aggregateOptions">
        /// Optional parameters for aggregation. This can be null.
        /// </param>
        /// <param name="forceTransaction">
        /// An optional boolean indicating whether to force the operation to be 
        /// executed within a transaction. This can also be null.
        /// </param>
        /// <returns>
        /// Returns an <see cref="IQueryable{TDocument}"/> that represents the current 
        /// document collection, potentially transformed by the <paramref name="preApprend"/> function.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IQueryable<TDocument> AsQueryable(Func<IQueryable<TDocument>, IQueryable<TDocument>> preApprend,
            [MaybeNull] AggregateOptions? aggregateOptions = default,
            [MaybeNull] bool? forceTransaction = default)
        {
            if (preApprend == default)
            {
                return this.AsQueryable(aggregateOptions, forceTransaction);
            }
            else
            {
                return preApprend(this.AsQueryable(aggregateOptions, forceTransaction));
            }
        }
        #endregion queryable

        #region FirstOrDefault


        /// <summary>
        /// Asynchronously retrieves the first document matching the specified query or returns the default 
        /// value if no documents are found.
        /// </summary>
        /// <param name="query">The query to be executed against the database to find the document.</param>
        /// <param name="findOptions">Optional parameters to refine the find operation (default is null).</param>
        /// <param name="forceTransaction">Optional flag that indicates whether to force the use of a transaction 
        /// (default is null).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests (default is null).</param>
        /// <returns>
        /// A <see cref="Task{TDocument}"/> containing the first matching document or the default value 
        /// for the type if no document was found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TDocument> FirstOrDefaultAsync([NotNull] Query<TDocument> query,
            [MaybeNull] FindOptions? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            var opt = ConvertInternal(findOptions);

            var qry = (Query<TDocument, TDocument>)query;

            return await this.GetOneAsync(qry, opt, forceTransaction, cancellationToken);
        }




        /// <summary>
        /// Asynchronously retrieves the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="TProjection">
        /// The type of the projection to be returned.
        /// </typeparam>
        /// <param name="query">
        /// A query object that defines the criteria for retrieving the document.
        /// Must not be null.
        /// </param>
        /// <param name="findOptions">
        /// Optional parameters to customize the find operation.
        /// Can be null.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional flag indicating whether to force a transaction.
        /// Can be null.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token to propagate notification that operations should be canceled.
        /// Can be null.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TProjection}"/> representing the asynchronous operation.
        /// The task result contains the first element of the sequence, or the default value for the type if no elements are found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TProjection> FirstOrDefaultAsync<TProjection>([NotNull] Query<TDocument, TProjection> query,
            [MaybeNull] FindOptions<TDocument, TProjection>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default) => await this.GetOneAsync(query, findOptions, forceTransaction, cancellationToken);

        #endregion

        #region Any


        /// <summary>
        /// Asynchronously determines whether any documents match the specified query.
        /// This method checks if there are any documents that satisfy the provided <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The query to filter documents.</param>
        /// <param name="countOptions">Options for counting documents, defaulting to null.</param>
        /// <param name="forceTransaction">Indicates whether to force the operation within a transaction, defaulting to null.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether any documents match the query.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> AnyAsync([NotNull] Query<TDocument> query,
            CountOptions? countOptions = default,
            bool? forceTransaction = default,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set limit and skip options for counting
            countOptions ??= new CountOptions();

            FilterDefinition<TDocument> filterDefinition = query;

            this._contextbase.BeforeFindInternal<TDocument, TObjectId, TDocument, TUser>(this, ref filterDefinition, ref countOptions);

            bool any = false;
            // Count documents and check if count is greater than 0
            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                any = await this.MongoCollection.CountDocumentsAsync(clientSessionHandle, filterDefinition, countOptions, cancellationToken) > 0;
            }
            else
            {
                return await this.MongoCollection.CountDocumentsAsync(filterDefinition, countOptions, cancellationToken) > 0;
            }

            this._contextbase.ResultInternal(this, ref any);

            return any;
        }

        #endregion

        #region Get

        #region Get One


        /// <summary>
        /// Asynchronously retrieves a document of type <typeparamref name="TDocument"/> 
        /// from a data source using the provided identifier and optional find options.
        /// </summary>
        /// <param name="id">The unique identifier of the document to retrieve.</param>
        /// <param name="findOptions">Optional settings that modify the query behavior.</param>
        /// <param name="forceTransaction">Optional flag to indicate whether to enforce 
        /// transaction usage for the operation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{TDocument}"/> that represents the asynchronous 
        /// operation. The task result contains the retrieved document, or null if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TDocument?> GetAsync([NotNull] TObjectId id,
            [MaybeNull] FindOptions? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Convert the find options to the correct type
            var opt = ConvertInternal(findOptions);

            // Call the GetAsync method with the converted options
            return await this.GetAsync(id, opt, forceTransaction, cancellationToken);
        }


        /// <summary>
        /// Asynchronously retrieves a document of type <typeparamref name="TDocument"/>
        /// from a data store based on the provided identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the document to retrieve.</param>
        /// <param name="findOptions">Optional parameters to modify the find operation.</param>
        /// <param name="forceTransaction">Optional flag to indicate if the operation should be executed within a transaction.</param>
        /// <param name="cancellationToken">Optional cancellation token to observe while waiting for the asynchronous operation to complete.</param>
        /// <returns>A <see cref="Task{TDocument}"/> representing the asynchronous operation, which returns the retrieved document.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TDocument?> GetAsync([NotNull] TObjectId id,
            [MaybeNull] FindOptions<TDocument>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Convert the find options to the correct type
            var opt = ConvertInternal(findOptions);

            // Call the GetAsync method with the converted options
            return await this.GetAsync(id, opt, forceTransaction, cancellationToken);
        }


        /// <summary>
        /// Asynchronously retrieves a single projection of a document from a data store based on the given object ID.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection to retrieve.</typeparam>
        /// <param name="id">The identifier of the document to retrieve, must not be null.</param>
        /// <param name="findOptions">Optional parameters to modify the find operation. Can be null.</param>
        /// <param name="forceTransaction">Optional parameter indicating whether to force a transaction. Can be null.</param>
        /// <param name="cancellationToken">Optional token to cancel the asynchronous operation. Can be null.</param>
        /// <returns>A <see cref="Task{TProjection}"/> representing the asynchronous operation, containing the retrieved projection of type <typeparamref name="TProjection"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TProjection?> GetAsync<TProjection>([NotNull] TObjectId id,
            [MaybeNull] FindOptions<TDocument, TProjection>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            var qry = Query<TDocument, TProjection>.FromExpression(o => o.Id.Equals(id));

            return await this.GetOneAsync(qry, findOptions, forceTransaction, cancellationToken);
        }


        /// <summary>
        /// Asynchronously retrieves a single document based on the provided query and options.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection to return.</typeparam>
        /// <param name="query">The query object that contains the criteria to filter the documents.</param>
        /// <param name="findOptions">Optional parameters for finding documents, such as sorting or limiting the results. Default is null.</param>
        /// <param name="forceTransaction">Optional parameter indicating whether to force the operation within a transaction. Default is null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. Default is null.</param>
        /// <returns>
        /// A <see cref="Task{TProjection}"/> representing the asynchronous operation. The value of the task 
        /// contains the single document that matches the query, or the default value if no documents are found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TProjection?> GetOneAsync<TProjection>([NotNull] Query<TDocument, TProjection> query,
            [MaybeNull] FindOptions<TDocument, TProjection>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            return await GetFirstOrSingleAsync<TProjection>(false, query, findOptions, forceTransaction, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a single projection of a document based on the provided query.
        /// This method uses aggressive inlining for optimized performance.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection to return.</typeparam>
        /// <param name="query">The query to execute against the document collection.</param>
        /// <param name="findOptions">Optional. The find options to use for the database query.</param>
        /// <param name="forceTransaction">Optional. Indicates whether to force a transaction when executing the query.</param>
        /// <param name="cancellationToken">Optional. A token for cancelling the asynchronous operation.</param>
        /// <returns>
        /// A <see cref="Task{TProjection}"/> representing the result of the asynchronous operation,
        /// containing either the single projection of type <typeparamref name="TProjection"/> 
        /// or the default value if no documents match the query.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TProjection?> GetSingleAsync<TProjection>([NotNull] Query<TDocument, TProjection> query,
            [MaybeNull] FindOptions<TDocument, TProjection>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            return await this.GetFirstOrSingleAsync<TProjection>(true, query, findOptions, forceTransaction, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the first or single document from the collection based on the provided query, 
        /// applying specified options and handling potential transactions.
        /// </summary>
        /// <typeparam name="TProjection">
        /// The type of the projected result.
        /// </typeparam>
        /// <param name="isSingle">
        /// A boolean indicating whether to expect a single result. If true, an exception is thrown if more than one result is found.
        /// </param>
        /// <param name="query">
        /// The query defining the criteria for selecting documents.
        /// </param>
        /// <param name="findOptions">
        /// Optional settings to customize the find operation, such as limit and skip. Defaults to a new instance if null.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional flag to indicate whether to force the operation within a transaction. Defaults to null.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional token to observe cancellation requests. Defaults to null.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TProjection}"/> representing the asynchronous operation, 
        /// containing the first or single document found, or the default value of <typeparamref name="TProjection"/> if no documents match the query.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="isSingle"/> is true and more than one document is found.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<TProjection?> GetFirstOrSingleAsync<TProjection>([NotNull] bool isSingle,
            [NotNull] Query<TDocument, TProjection> query,
            [MaybeNull] FindOptions<TDocument, TProjection>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Initialize the result to the default value of TProjection
            var result = default(TProjection);

            // Set default options if not provided
            findOptions ??= new FindOptions<TDocument, TProjection>();

            // Set skip and limit options for finding the first occurrence
            findOptions.Skip ??= 0;
            findOptions.Limit ??= 1;

            FilterDefinition<TDocument> filterDefinition = (query ?? FilterDefinition<TDocument>.Empty)!;

            this._contextbase.BeforeFindInternal<TDocument, TObjectId, TProjection, TUser>(this, ref filterDefinition, ref findOptions);

            // Create the cursor for the find operation
            IAsyncCursor<TProjection> cursor;

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                cursor = await this.MongoCollection.FindAsync(clientSessionHandle, filterDefinition, findOptions, cancellationToken);
            }
            else
            {
                cursor = await this.MongoCollection.FindAsync(filterDefinition, findOptions, cancellationToken);
            }

            // Iterate over the cursor and retrieve the first occurrence
            while (await cursor.MoveNextAsync(cancellationToken))
            {
                foreach (var item in cursor.Current)
                {
                    if (isSingle && result != null)
                    {
                        throw new InvalidOperationException("More than one result.");
                    }

                    result = item;

                    if (!isSingle)
                    {
                        break;
                    }
                }

            }

            // Dispose the cursor
            cursor.Dispose();

            this._contextbase.ResultInternal<TDocument, TObjectId, TProjection, TUser>(this, ref result);

            return result;
        }
        #endregion

        #region Get Async Enumerable
        /// <summary>
        /// Asynchronously retrieves documents of type <typeparamref name="TDocument"/> from a data source 
        /// based on the specified array of IDs. This method returns an asynchronous stream of documents,
        /// allowing for efficient memory usage when processing large result sets.
        /// </summary>
        /// <param name="ids">An array of identifiers for the documents to retrieve.</param>
        /// <param name="findOptions">Optional parameters that specify how the documents should be retrieved.</param>
        /// <param name="forceTransaction">Optional parameter to dictate whether to force a transaction when 
        /// retrieving documents.</param>
        /// <param name="cancellationToken">Optional cancellation token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous stream of documents of type <typeparamref name="TDocument"/> 
        /// that match the provided identifiers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> GetAsync([NotNull] TObjectId[] ids,
            [MaybeNull] FindOptions? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull][EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var opt = ConvertInternal(findOptions);

            // Create a query to find documents with IDs in the provided array
            var qry = Query<TDocument, TDocument>.FromExpression(f => ids.Contains(f.Id))!;


            // Iterate over the results of the GetAsync method and yield each document
            await foreach (var item in this.GetAsync<TDocument>(qry, opt, forceTransaction, cancellationToken))
            {
                yield return item;
            }
        }


        /// <summary>
        /// Asynchronously retrieves a collection of documents identified by their IDs.
        /// </summary>
        /// <param name="ids">An array of identifiers for the documents to be fetched.</param>
        /// <param name="findOptions">Optional parameters that dictate how the documents should be found.</param>
        /// <param name="forceTransaction">Optional flag that indicates whether to force the operation to run within a transaction.</param>
        /// <param name="cancellationToken">Optional token to observe while waiting for the asynchronous operation to complete.</param>
        /// <returns>
        /// An asynchronous stream of documents of type <typeparamref name="TDocument"/> retrieved from the database.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> GetAsync([NotNull] TObjectId[] ids,
            [MaybeNull] FindOptions<TDocument>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull][EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Create a query to find documents with IDs in the provided array
            var qry = Query<TDocument, TDocument>.FromExpression(f => ids.Contains(f.Id));

            // Convert the findOptions to the correct type for the GetAsync method
            var opt = ConvertInternal(findOptions);

            // Iterate over the results of the GetAsync method and yield return each item
            await foreach (var item in this.GetAsync<TDocument>(qry, opt, forceTransaction, cancellationToken))
            {
                yield return item;
            }
        }


        /// <summary>
        /// Asynchronously retrieves a sequence of projections based on the specified document IDs.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection to retrieve.</typeparam>
        /// <param name="ids">An array of document identifiers to retrieve.</param>
        /// <param name="findOptions">Optional settings for the find operation. Defaults to new FindOptions if null.</param>
        /// <param name="forceTransaction">Optional flag indicating if transactions should be forced. Default is null.</param>
        /// <param name="cancellationToken">A token for canceling the operation. Default is null.</param>
        /// <returns>
        /// An asynchronous enumerable sequence of projections corresponding to the specified document IDs.
        /// </returns>
        /// <remarks>
        /// This method uses asynchronous streaming to yield results as they are retrieved,
        /// which is efficient for large datasets.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> GetAsync<TProjection>([NotNull] TObjectId[] ids,
            [MaybeNull] FindOptions<TDocument, TProjection>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull][EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Set default options if not provided
            findOptions ??= new FindOptions<TDocument, TProjection>();

            // Create a query to find documents with IDs in the provided array
            var qry = Query<TDocument, TProjection>.FromExpression(f => ids.Contains(f.Id));

            // Iterate over the results of the GetAsync method and yield each document
            await foreach (var item in this.GetAsync<TProjection>(qry, findOptions, forceTransaction, cancellationToken))
            {
                yield return item;
            }
        }



        /// <summary>
        /// Asynchronously retrieves a collection of documents based on the provided query filter and options.
        /// This method yields each document one at a time, allowing for efficient processing of large result sets.
        /// </summary>
        /// <param name="filter">The query filter to apply when retrieving documents. This parameter cannot be null.</param>
        /// <param name="findOptions">Optional parameters to define how the documents should be found. Can be null.</param>
        /// <param name="forceTransaction">An optional boolean indicating whether to force the use of a transaction. Can be null.</param>
        /// <param name="cancellationToken">A cancellation token to monitor for cancellation requests. Can be null.</param>
        /// <returns>An <see cref="IAsyncEnumerable{TDocument}"/> that can be enumerated asynchronously to iterate over the retrieved documents.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> GetAsync([NotNull] Query<TDocument> filter,
            [MaybeNull] FindOptions? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull][EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Convert the query to the correct type for the GetAsync method
            Query<TDocument, TDocument> qry = filter;

            // Convert the findOptions to the correct type for the GetAsync method
            var opt = ConvertInternal(findOptions);

            // Iterate over the results of the GetAsync method and yield return each item
            await foreach (var item in this.GetAsync(qry, opt, forceTransaction, cancellationToken))
            {
                yield return item;
            }
        }


        /// <summary>
        /// Asynchronously retrieves a sequence of documents based on the provided query filter.
        /// </summary>
        /// <param name="filter">The query filter used to specify which documents to retrieve.</param>
        /// <param name="findOptions">Optional parameters to customize the find operation. If not provided, default options are used.</param>
        /// <param name="forceTransaction">Optional parameter to determine whether to force the operation to run in a transaction.</param>
        /// <param name="cancellationToken">Optional token to observe while waiting for the asynchronous operation to complete.</param>
        /// <returns>
        /// An asynchronous stream of documents that match the provided query filter.
        /// </returns>
        /// <remarks>
        /// This method utilizes a query of type <typeparamref name="TDocument"/> and allows for asynchronous iteration over the resulting documents. 
        /// It employs <see cref="IAsyncEnumerable{T}"/> to enable efficient enumeration and can be used in a <c>foreach</c> loop or with LINQ.
        ///
        /// If the <paramref name="findOptions"/> parameter is not provided, a new <see cref="FindOptions{TDocument}"/> instance with default values is created.
        /// The <paramref name="filter"/> is converted to the expected query type, and the results are yielded as they are received asynchronously.
        /// It also respects the <paramref name="cancellationToken"/> to enable cancellation of the operation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> GetAsync([NotNull] Query<TDocument> filter,
            [MaybeNull] FindOptions<TDocument>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull][EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // If findOptions is null, create a new instance with default values
            findOptions ??= new FindOptions<TDocument>();

            // Convert the filter to the correct type for the GetAsync method
            Query<TDocument, TDocument> qry = filter;

            // Convert the findOptions to the correct type for the GetAsync method
            var opt = ConvertInternal(findOptions);

            // Iterate over the results of the GetAsync method and yield each document
            await foreach (var item in this.GetAsync(qry, opt, forceTransaction, cancellationToken))
            {
                yield return item;
            }
        }



        /// <summary>
        /// Asynchronously retrieves documents from the MongoDB collection based on the specified filter and options, 
        /// yielding the results as an asynchronous enumerable sequence.
        /// </summary>
        /// <typeparam name="TProjection">
        /// The type of the projected result of the documents retrieved.
        /// </typeparam>
        /// <param name="filter">
        /// The query definition used to filter the documents.
        /// </param>
        /// <param name="findOptions">
        /// Optional; specifies additional options for the find operation. If not provided, defaults will be applied.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional; indicates whether to force the operation to run in a transaction. Defaults to null.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional; token used to signal cancellation of the operation. Defaults to null.
        /// </param>
        /// <returns>
        /// An asynchronous enumerable sequence of projected documents of type <typeparamref name="TProjection"/>.
        /// </returns>
        /// <remarks>
        /// This method uses a cursor to iterate over the results returned from the query and yields each item
        /// that is not null. It ensures to handle both transactional and non-transactional find operations based
        /// on the provided parameters.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> GetAsync<TProjection>([NotNull] Query<TDocument, TProjection> filter,
            [MaybeNull] FindOptions<TDocument, TProjection>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull][EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Set default options if not provided
            findOptions ??= new FindOptions<TDocument, TProjection>();

            FilterDefinition<TDocument> filterDefinition = filter;

            this._contextbase.BeforeFindInternal<TDocument, TObjectId, TProjection, TUser>(this, ref filterDefinition, ref findOptions);


            // Create the cursor for the find operation
            IAsyncCursor<TProjection> cursor;

            // If the find operation should not be performed in a transaction
            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the find operation with the session and filter
                cursor = await this.MongoCollection.FindAsync(clientSessionHandle, filterDefinition, findOptions, cancellationToken);
            }
            else
            {
                cursor = await this.MongoCollection.FindAsync(filterDefinition, findOptions, cancellationToken);
            }

            // Iterate over the cursor and retrieve the search results
            while (await cursor.MoveNextAsync(cancellationToken))
            {
                foreach (var item in cursor.Current)
                {
                    // Yield return each item in the search results, excluding null items
                    if (item != null)
                    {
                        TProjection? itemResult = item;

                        this._contextbase.ResultInternal<TDocument, TObjectId, TProjection, TUser>(this, ref itemResult);

                        yield return itemResult;
                    }
                }
            }
            // Dispose of the cursor
            cursor.Dispose();
        }

        #endregion Get Async Enumerable

        #region Fulltext Search
        /// <summary>
        /// Performs a full-text search asynchronously and yields the results as an asynchronous stream of documents.
        /// </summary>
        /// <param name="text">
        /// The search text to be used in the full-text search.
        /// </param>
        /// <param name="fullTextSearchOptions">
        /// Options for configuring the full-text search behavior.
        /// </param>
        /// <param name="filter">
        /// Optional filter criteria to apply to the search results. Can be null if no filtering is required.
        /// </param>
        /// <param name="findOptions">
        /// Optional options that specify how to find documents. Can be null if default options are sufficient.
        /// </param>
        /// <param name="forceTransaction">
        /// An optional boolean indicating whether to force the execution of the operation within a transaction. Defaults to null.
        /// </param>
        /// <param name="cancellationToken">
        /// An optional cancellation token to observe while waiting for the task to complete. Defaults to null.
        /// </param>
        /// <returns>
        /// An asynchronous enumerable of documents that match the full-text search criteria.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> FulltextSearchAsync([NotNull] string text,
            [MaybeNull] TextSearchOptions? fullTextSearchOptions,
            [MaybeNull] Query<TDocument>? filter = default,
            [MaybeNull] FindOptions<TDocument>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull][EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Perform the full-text search and iterate over the results
            await foreach (var item in this.FulltextSearchAsync<TDocument>(text, fullTextSearchOptions, filter, findOptions, forceTransaction, cancellationToken))
            {
                // Yield return each item in the search results
                yield return item;
            }
        }


        /// <summary>
        /// Performs a full-text search asynchronously and returns an asynchronous enumerable of projected results.
        /// </summary>
        /// <typeparam name="TProjection">
        /// The type of the projected results.
        /// </typeparam>
        /// <param name="text">
        /// The text to search for.
        /// </param>
        /// <param name="fullTextSearchOptions">
        /// Options to customize the full-text search.
        /// </param>
        /// <param name="filter">
        /// An optional filter to further refine the results, can be null.
        /// </param>
        /// <param name="findOptions">
        /// Optional find options to modify the query execution, can be null.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional indicator to force transaction execution, can be null.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token to propagate notifications that the operation should be canceled.
        /// </param>
        /// <returns>
        /// An async enumerable of projected results obtained from the full-text search.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> FulltextSearchAsync<TProjection>([NotNull] string text,
            [MaybeNull] TextSearchOptions? fullTextSearchOptions,
            [MaybeNull] Query<TDocument, TProjection>? filter = default,
            [MaybeNull] FindOptions<TDocument, TProjection>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull][EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            findOptions ??= new FindOptions<TDocument, TProjection>();
            
            FilterDefinition<TDocument> filterSelected = Query<TDocument, TProjection>.FromText(text, fullTextSearchOptions);

            this._contextbase.BeforeFindInternal<TDocument, TObjectId, TProjection, TUser>(this, ref filterSelected, ref findOptions);

            if (filter != default)
            {
                filterSelected += filter;
            }

            IAsyncCursor<TProjection> cursor;

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                cursor = await this.MongoCollection.FindAsync(clientSessionHandle, filterSelected, findOptions, cancellationToken);
            }
            else
            {
                cursor = await this.MongoCollection.FindAsync(filterSelected, findOptions, cancellationToken);
            }

            while (await cursor.MoveNextAsync(cancellationToken))
            {
                foreach (var item in cursor.Current)
                {
                    if (item != null)
                    {
                        TProjection result = item;

                        this._contextbase.ResultInternal<TDocument, TObjectId, TProjection, TUser>(this, ref result);

                        yield return result;
                    }
                }
            }

            // Dispose of the cursor
            cursor.Dispose();
        }
        #endregion Fulltext Search

        #region Get Paged
        /// <summary>
        /// Asynchronously retrieves a paginated result based on the provided filter and pagination options.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection for the paginated result.</typeparam>
        /// <param name="filter">The query filter used to select the documents to be paged.</param>
        /// <param name="findOptionsPaging">The options to customize the paging behavior. This can include sorting, page size, etc.</param>
        /// <param name="forceTransaction">Specifies whether to force the operation to run in a transaction. This is optional.</param>
        /// <param name="cancellationToken">The cancellation token to observe for cancellation requests. This is optional.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a <see cref="PagedResult{TProjection}"/> 
        /// which holds the paginated results of type <typeparamref name="TProjection"/>.
        /// </returns>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<PagedResult<TProjection>> GetPagedAsync<TProjection>([NotNull] Query<TDocument> filter,
            [MaybeNull] FindOptionsPaging<TDocument, TObjectId, TProjection, TUser>? findOptionsPaging,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Convert the filter to the correct type for the GetPagedAsync method
            Query<TDocument, TProjection> qry = filter!;

            //FindOptions<TDocument, TProjection> findOptions = findOptionsPaging;

            // Call the GetPagedAsync method with the converted filter and findOptions
            return await this.GetPagedAsync(qry, findOptionsPaging, forceTransaction, cancellationToken);
        }


        /// <summary>
        /// Asynchronously retrieves a paged result of projections based on the specified filter and find options.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projections returned in the paged result.</typeparam>
        /// <param name="filter">The query filter specifying the documents to retrieve.</param>
        /// <param name="findOptionsPaging">The options for paging the result set, including page size and the current page number.</param>
        /// <param name="forceTransaction">Specifies whether to enforce transaction behavior.</param>
        /// <param name="cancellationToken">A cancellation token to signal when the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the paged result with projections.</returns>
        /// <exception cref="ArgumentException">Thrown when the page size or current page is invalid.</exception>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<PagedResult<TProjection>> GetPagedAsync<TProjection>([NotNull] Query<TDocument, TProjection> filter,
            [MaybeNull] FindOptionsPaging<TDocument, TObjectId, TProjection, TUser>? findOptionsPaging,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Convert the filter and findOptions to strings for logging purposes
            //var fstr = filter.ToString();
            //var fostr = findOptionsPaging.ToString();
            //var fojson = findOptionsPaging.JsonString();

            findOptionsPaging ??= new FindOptionsPaging<TDocument, TObjectId, TProjection, TUser>();

            // Check if the page size is valid
            if (findOptionsPaging.PageSize <= 0)
            {
                throw new ArgumentException("Page size is invalid or null.");
            }

            // Check if the current page is valid
            if (findOptionsPaging.CurrentPage < 0)
            {
                throw new ArgumentException("Current page is invalid or null.");
            }

            FindOptions<TDocument, TProjection> findOptions = findOptionsPaging;



            #region find
            // Create the filter definition from the query
            FilterDefinition<TDocument> filterSelected = filter;


            this._contextbase.BeforeFindInternal<TDocument, TObjectId, TProjection, TUser>(this, ref filterSelected, ref findOptions);


            // Create the count options from the find options and set the limit and skip options to null
            var countOptions = new CountOptions
            {
                Limit = null,
                Skip = null,
                Collation = findOptions.Collation,
                Comment = findOptions.Comment,
                Hint = findOptions.Hint,
                MaxTime = findOptions.MaxTime
            };

            // Asynchronously count the number of documents that match the specified filter
            var total = Convert.ToInt32(await this.CountDocumentsAsync((FilterDefinition<TDocument>)filter, countOptions));

            IAsyncCursor<TProjection> cursor;

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                cursor = await this.MongoCollection.FindAsync(clientSessionHandle, filterSelected, findOptions, cancellationToken);
            }
            else
            {
                cursor = await this.MongoCollection.FindAsync(filterSelected, findOptions, cancellationToken);
            }

            var itens = new TProjection[(countOptions != null && countOptions.Limit.HasValue && countOptions.Limit.Value < total) ? countOptions.Limit.Value : total];

            var lastPos = 0;
            // Iterate over the cursor and retrieve the items
            while (await cursor.MoveNextAsync(cancellationToken))
            {
                foreach (var item in cursor.Current)
                {
                    if (item != null)
                    {
                        itens[lastPos++] = item;
                    }
                }
            }

            // Return the paged result
            cursor.Dispose();

            // Resize the array if necessary    
            if (lastPos < itens.Length)
            {
                Array.Resize(ref itens, lastPos);
            }

            #endregion find

            IEnumerable<TProjection> results = itens;

            this._contextbase.ResultInternal<TDocument, TObjectId, TProjection, TUser>(this, ref results);

            // Perform the find operation and return the result
            return new PagedResult<TProjection>(results, findOptionsPaging.CurrentPage, findOptionsPaging.PageSize, total);
        }
        #endregion Get Paged

        #endregion

        #region FindOneAndUpdateAsync


        /// <summary>
        /// Asynchronously finds a single document that matches the filter and updates it.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection to be returned after the update.</typeparam>
        /// <param name="query">The query that defines the filter and the update operations.</param>
        /// <param name="options">The options for the find and update operation.</param>
        /// <param name="forceTransaction">
        /// Optional. Specifies whether to force the operation to be performed in a transaction.
        /// Returns null if not specified.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional. A cancellation token to signal the operation to be canceled.
        /// Returns null if not specified.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TProjection}"/> representing the asynchronous operation,
        /// containing the updated document of type <typeparamref name="TProjection"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TProjection> FindOneAndUpdateAsync<TProjection>(
            [NotNull] Query<TDocument, TProjection> query,
            [MaybeNull] FindOneAndUpdateOptions<TDocument, TProjection> options,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Declare a variable to hold the result
            TProjection result;

            // Get the filter and update definitions from the query
            FilterDefinition<TDocument> filterDefinition = query;

            UpdateDefinition<TDocument> updateDefinition = query.Update;

            this._contextbase.BeforeUpdateInternal<TDocument, TObjectId, TProjection, TUser>(this, ref filterDefinition, ref updateDefinition, ref options);

            // If the operation should not be performed in a transaction
            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the find and update operation with a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(clientSessionHandle, filterDefinition, updateDefinition, options, cancellationToken);
            }
            else
            {
                // Perform the find and update operation without a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(filterDefinition, updateDefinition, options, cancellationToken);
            }

            // Return the updated document
            return result;
        }

        #endregion FindOneAndUpdateAsync

        #region UpdateManyAsync

        /// <summary>
        /// Asynchronously updates multiple documents in the MongoDB collection 
        /// that match the specified query with the provided update options.
        /// </summary>
        /// <param name="query">
        /// The query that identifies the documents to update.
        /// Must not be null.
        /// </param>
        /// <param name="options">
        /// Options that control how the update operation is executed.
        /// Must not be null.
        /// </param>
        /// <param name="forceTransaction">
        /// Indicates whether to force the operation to run in a transaction.
        /// If null, the method will decide based on the context.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the async operation.
        /// Defaults to default cancellation token behavior.
        /// </param>
        /// <returns>
        /// A Task that represents the asynchronous update operation. 
        /// The task result contains the number of documents modified, 
        /// or -1 if the operation was not acknowledged.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled by the cancellation token.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when the operation is not supported or cannot be executed.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<long> UpdateManyAsync([NotNull] Query<TDocument> query,
                    [NotNull] UpdateOptions options,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Initialize the result to null
            UpdateResult result;

            FilterDefinition<TDocument> filterDefinition = query;

            UpdateDefinition<TDocument> update = query.Update;

            this._contextbase.BeforeUpdateInternal<TDocument, TObjectId, long, TUser>(this, ref filterDefinition, ref update, ref options);

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                result = await this.MongoCollection.UpdateManyAsync(clientSessionHandle, query, update, options, cancellationToken);
            }
            else
            {
                result = await this.MongoCollection.UpdateManyAsync(query, update, options, cancellationToken);
            }

            this._contextbase.ResultInternal<TDocument, TObjectId, UpdateResult, TUser>(this, ref result);

            // Return the number of modified documents, or -1 if the operation was not acknowledged
            return result.IsAcknowledged ? result.ModifiedCount : -1;
        }

        /// <summary>
        /// Asynchronously updates multiple documents in the MongoDB collection based on the specified filter and update criteria.
        /// </summary>
        /// <param name="filter">A string representing the filter criteria used to select the documents to be updated.</param>
        /// <param name="updateStr">A string specifying the updates to be applied to the selected documents.</param>
        /// <param name="options">An instance of <see cref="UpdateOptions"/> that defines options for the update operation.</param>
        /// <param name="forceTransaction">An optional nullable boolean indicating whether to enforce the use of a transaction. Defaults to null.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation, containing the number of modified documents, or -1 if the operation was not acknowledged.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled through the provided <paramref name="cancellationToken"/>.</exception>
        /// <remarks>
        /// This method first checks if a cancellation has been requested. It initializes the update result 
        /// and defines the filter and update according to the parameters provided. 
        /// Depending on whether a transaction is enforced or not, it either performs the update operation using 
        /// a client session or directly on the Mongo collection. 
        /// Finally, it processes the result and returns the count of modified documents or -1 if the update was not acknowledged.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<long> UpdateManyAsync([NotNull] string filter,
            [NotNull] string updateStr,
            [NotNull] UpdateOptions options,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Initialize the result to null
            UpdateResult result;

            FilterDefinition<TDocument> filterDefinition = filter;

            UpdateDefinition<TDocument> update = updateStr;

            this._contextbase.BeforeUpdateInternal<TDocument, TObjectId, long, TUser>(this, ref filterDefinition, ref update, ref options);

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the update operation with a session
                result = await this.MongoCollection.UpdateManyAsync(clientSessionHandle, filter, update, options, cancellationToken);
            }
            else
            {
                // Perform the update operation without a session
                result = await this.MongoCollection.UpdateManyAsync(filter, update, options, cancellationToken);
            }

            this._contextbase.ResultInternal<TDocument, TObjectId, UpdateResult, TUser>(this, ref result);

            // Return the number of modified documents, or -1 if the operation was not acknowledged
            return result.IsAcknowledged ? result.ModifiedCount : -1;
        }

        #endregion UpdateManyAsync

        #region Count

        /// <summary>
        /// Asynchronously counts the number of documents that match the specified query.
        /// </summary>
        /// <param name="query">The query to use for counting documents. Must not be null.</param>
        /// <param name="countOptions">Options that specify how the count should be performed. If not provided, default options will be used.</param>
        /// <param name="forceTransaction">Indicates whether to force the count operation to be executed within a transaction, if applicable.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>
        /// A <see cref="Task{long}"/> representing the asynchronous operation, with the count of documents as the result.
        /// </returns>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<long> CountDocumentsAsync([NotNull] Query<TDocument> query,
            [MaybeNull] CountOptions? countOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            // If countOptions is null, create a new instance with default values
            countOptions ??= new CountOptions();

            // Convert the query to a FilterDefinition and call the CountDocumentsAsync method with it
            return await this.CountDocumentsAsync((FilterDefinition<TDocument>)query, countOptions, forceTransaction, cancellationToken);
        }

        /// <summary>
        /// Asynchronously counts the number of documents that match a specified query.
        /// </summary>
        /// <param name="preApprend">
        /// A function that modifies the <see cref="IQueryable{TDocument}"/> to apply criteria 
        /// for the documents to be counted.
        /// </param>
        /// <param name="aggregateOptions">
        /// Optional aggregation options to apply during the counting process. If not provided, defaults to null.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional flag indicating whether to force the use of a transaction. If not provided, defaults to null.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests. If not provided, defaults to null.
        /// </param>
        /// <returns>
        /// A <see cref="Task{long}"/> representing the asynchronous operation, containing the 
        /// count of documents found in the queryable.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.
        /// </exception>
        /// <remarks>
        /// This method uses an optional transaction if one is indicated, applying a query 
        /// before counting the documents. It leverages asynchronous programming for 
        /// improved performance during database operations.
        /// </remarks>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<long> CountDocumentsAsync([NotNull] Func<IQueryable<TDocument>, IQueryable<TDocument>> preApprend,
            [MaybeNull] AggregateOptions? aggregateOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            IQueryable<TDocument> query;

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                query = this.MongoCollection.AsQueryable(clientSessionHandle, aggregateOptions ?? new AggregateOptions());
            }
            else
            {
                query = this.MongoCollection.AsQueryable(aggregateOptions ?? new AggregateOptions());
            }

            var iq = preApprend.Invoke(query);

            this._contextbase.BeforeQueryableInternal<TDocument, TObjectId, TDocument, TUser>(this, ref iq, ref aggregateOptions);

            long count = await iq.LongCountAsync(cancellationToken);

            // Count the number of documents in the queryable
            this._contextbase.ResultInternal(this, ref count);


            return count;
        }

        /// <summary>
        /// Asynchronously counts the number of documents that match the specified filter definition.
        /// </summary>
        /// <param name="filterDefinition">
        /// A filter definition used to specify the criteria for the documents to count.
        /// Must not be null.
        /// </param>
        /// <param name="countOptions">
        /// Optional. Options to specify how the count operation is performed.
        /// Can be null.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional. Indicates whether to force the count operation to be performed within a transaction.
        /// Can be null.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional. A cancellation token to propagate notification that operations should be canceled.
        /// Can be null.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the count of documents matching the filter.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="filterDefinition"/> is null.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled.
        /// </exception>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<long> CountDocumentsAsync([NotNull] FilterDefinition<TDocument> filterDefinition,
            [MaybeNull] CountOptions? countOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            countOptions ??= new CountOptions();

            this._contextbase.BeforeFindInternal<TDocument, TObjectId, long, TUser>(this, ref filterDefinition, ref countOptions);

            long result = 0;
            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the count operation without a session
                result = await this.MongoCollection.CountDocumentsAsync(clientSessionHandle, filterDefinition, countOptions, cancellationToken);
            }
            else
            {
                result = await this.MongoCollection.CountDocumentsAsync(filterDefinition, countOptions, cancellationToken);
            }

            this._contextbase.ResultInternal(this, ref result);

            return result;
        }

        #endregion

        #region Update

        /// <summary>
        /// Asynchronously updates a document in the database by adding a value to a set.
        /// </summary>
        /// <param name="query">The query defining the document to be updated.</param>
        /// <param name="updateOptions">Optional settings for the update operation.</param>
        /// <param name="forceTransaction">Optional parameter indicating whether to force the operation to run in a transaction.</param>
        /// <param name="cancellationToken">A cancellation token for managing cancellation of the operation.</param>
        /// <returns>
        /// A <see cref="Task{Long}"/> representing the asynchronous operation that returns the number of documents updated.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.
        /// </exception>
        /// <remarks>
        /// This method ensures that if no update options are provided, default values will be utilized.
        /// Additionally, it checks for cancellation requests before proceeding with the update.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<long> UpdateAddToSetAsync([NotNull] Query<TDocument> query,
                    UpdateOptions? updateOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // If updateOptions is null, create a new instance with default values
            updateOptions ??= new UpdateOptions();


            // Call the UpdateAsync method with the query, update, and options
            return await this.UpdateAsync(query, query.Update, updateOptions, forceTransaction, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates a document in the MongoDB collection based on the specified filter and update definitions.
        /// </summary>
        /// <param name="filterDefinition">
        /// The filter definition to locate the document to be updated.
        /// </param>
        /// <param name="updateDefinition">
        /// The update definition that specifies the modifications to apply to the document.
        /// </param>
        /// <param name="updateOptions">
        /// The options to apply to the update operation, such as whether to upsert the document if it does not exist.
        /// </param>
        /// <param name="forceTransaction">
        /// If provided, determines whether to force the operation to run within a transaction; otherwise, default is used.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token to monitor for cancellation requests during the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The value of the task contains:
        /// -1 if the update operation did not acknowledge any modification, or the number of modified documents if acknowledged.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async Task<long> UpdateAsync(
                    [NotNull] FilterDefinition<TDocument> filterDefinition,
                    [NotNull] UpdateDefinition<TDocument> updateDefinition,
                    [NotNull] UpdateOptions updateOptions,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Initialize the result to null
            UpdateResult result;

            this._contextbase.BeforeUpdateInternal<TDocument, TObjectId, long, TUser>(this, ref filterDefinition, ref updateDefinition, ref updateOptions);

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the update operation with a session
                result = await this.MongoCollection.UpdateOneAsync(clientSessionHandle, filterDefinition, updateDefinition, updateOptions, cancellationToken);
            }
            else
            {
                // Perform the update operation without a session
                result = await this.MongoCollection.UpdateOneAsync(filterDefinition, updateDefinition, updateOptions, cancellationToken);
            }

            this._contextbase.ResultInternal<TDocument, TObjectId, UpdateResult, TUser>(this, ref result);


            return result == default ? -1 : result.IsAcknowledged ? result.ModifiedCount : -1;
        }


        #endregion

        #region Insert

        /// <summary>
        /// Asynchronously inserts a document into the collection.
        /// </summary>
        /// <param name="source">The document to insert.</param>
        /// <param name="insertOneOptions">Optional settings for the insert operation.</param>
        /// <param name="forceTransaction">Optional flag to force the operation to run in a transaction.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous insert operation, containing the number of documents inserted.</returns>
        /// <remarks>
        /// If the source document's Id is default or null, the method returns 0,
        /// indicating that no document was inserted. Otherwise, it returns 1.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<long> InsertAsync([NotNull] TDocument source,
                    [MaybeNull] InsertOneOptions? insertOneOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            // If no options were provided, create a new default options object
            insertOneOptions ??= new InsertOneOptions();

            // Create a list to hold the write model for the insert operation
            var writeModels = new List<WriteModel<TDocument>>
            {
                new InsertOneModel<TDocument>(source)
            };


            this._contextbase.BeforeInsertInternal<TDocument, TObjectId, long, TUser>(this, ref source, ref insertOneOptions);

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the update operation with a session
                await this.MongoCollection.InsertOneAsync(clientSessionHandle, source, insertOneOptions, cancellationToken);
            }
            // If no transaction is in use
            else
            {
                // Perform the update operation without a session
                await this.MongoCollection.InsertOneAsync(source, insertOneOptions, cancellationToken);
            }


            this._contextbase.ResultInternal<TDocument, TObjectId, TDocument, TUser>(this, ref source);

            return source.Id.Equals(default) || source.Id.Equals(null) ? 0 : 1;
        }

        /// <summary>
        /// Asynchronously inserts a collection of documents into the database.
        /// This method constructs a bulk write operation, adding an insert model for each document.
        /// </summary>
        /// <param name="docs">The collection of documents to be inserted.</param>
        /// <param name="bulkWriteOptions">Optional settings to control the bulk write operation.</param>
        /// <param name="forceTransaction">Optional flag to force the operation to occur within a transaction.</param>
        /// <param name="cancellationToken">Optional cancellation token to observe while waiting for the asynchronous operation to complete.</param>
        /// <returns>
        /// A <see cref="Task{long}"/> representing the number of documents inserted.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<long> InsertAsync([NotNull] IEnumerable<TDocument> docs,
                    [MaybeNull] BulkWriteOptions? bulkWriteOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create a list to hold the write models for the insert operation
            var writeModels = new List<WriteModel<TDocument>>();

            // Add a write model for each document to insert
            foreach (var doc in docs)
            {
                var insertOneModel = new InsertOneModel<TDocument>(doc);
                writeModels.Add(insertOneModel);
            }

            return await this.BulkWriteAsync(writeModels, bulkWriteOptions, forceTransaction, cancellationToken);
        }

        /// <summary>
        /// Asynchronously inserts a collection of documents into the database.
        /// </summary>
        /// <param name="docs">An enumerable collection of documents to be inserted.</param>
        /// <param name="insertManyOptions">Optional parameters for the insert operation, such as whether to bypass document validation.</param>
        /// <param name="forceTransaction">Optional flag indicating whether to force the operation to execute within a transaction.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{long}"/> representing the asynchronous operation, with the count of documents inserted as the result.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled through the cancellation token.</exception>
        /// <remarks>
        /// This method supports bulk writes with various options, such as ordered or unordered execution of operations.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<long> InsertAsync([NotNull] IEnumerable<TDocument> docs,
                    [MaybeNull] InsertManyOptions? insertManyOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var insertMany = insertManyOptions ?? new InsertManyOptions();

            var bulkWriteOption = new BulkWriteOptions();
            bulkWriteOption.Comment = insertManyOptions.Comment;
            bulkWriteOption.BypassDocumentValidation = insertManyOptions.BypassDocumentValidation;
            bulkWriteOption.IsOrdered = insertManyOptions.IsOrdered;

            // Perform the insert operation using the write models and options
            return await this.InsertAsync(docs, bulkWriteOption, forceTransaction, cancellationToken);
        }

        #endregion

        #region Replace

        /// <summary>
        /// Asynchronously replaces documents in the database based on the provided collection of documents and optional query parameters.
        /// </summary>
        /// <param name="docs">
        /// The collection of documents to be replaced.
        /// </param>
        /// <param name="query">
        /// An optional query to determine which documents to replace. If null, uses a default expression.
        /// </param>
        /// <param name="bulkWriteOptions">
        /// Optional settings to configure bulk write operations. Defaults to a new instance if null.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional flag to indicate whether the operation should be executed within a transaction. Defaults to null.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests. If cancellation is requested, an exception will be thrown.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous replace operation. The task result contains the total number of documents replaced.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<long> ReplaceAsync([NotNull] IEnumerable<TDocument> docs,
                    [MaybeNull] Query<TDocument>? query = null,
                    [MaybeNull] BulkWriteOptions? bulkWriteOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bulkWriteOptions ??= new BulkWriteOptions();

            var updates = new List<WriteModel<TDocument>>();

            Expression<Func<TDocument, TDocument, bool>> exp = (item, constrain) => item.Id.Equals(constrain.Id);

            // Create a filter definition for each document
            foreach (var doc in docs)
            {
                FilterDefinition<TDocument> filterDefinition = (query ?? exp).CompleteExpression(doc);

                var model = new ReplaceOneModel<TDocument>(filterDefinition, doc);

                updates.Add(model);
            }


            return await this.BulkWriteAsync(updates, bulkWriteOptions, forceTransaction, cancellationToken);
        }


        /// <summary>
        /// Asynchronously replaces documents in the database based on the provided enumerable collection of documents.
        /// </summary>
        /// <param name="docs">The enumerable collection of documents to be replaced.</param>
        /// <param name="query">An optional query to filter which documents to replace; if null, all documents in the `docs` will be replaced.</param>
        /// <param name="replaceOptions">Optional settings that control the replace operation.</param>
        /// <param name="forceTransaction">Optional flag to force the operation to be executed within a transaction; default is null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation, containing the number of documents replaced.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<long> ReplaceAsync([NotNull] IEnumerable<TDocument> docs,
                    [MaybeNull] Query<TDocument>? query = null,
                    [MaybeNull] ReplaceOptions? replaceOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            replaceOptions ??= new ReplaceOptions();

            var bulkWriteOptions = new BulkWriteOptions()
            {
                IsOrdered = true,
                BypassDocumentValidation = replaceOptions.BypassDocumentValidation,
                Comment = replaceOptions.Comment,
                Let = replaceOptions.Let
            };

            return await this.ReplaceAsync(docs, query, bulkWriteOptions, forceTransaction, cancellationToken);
        }



        /// <summary>
        /// Asynchronously replaces a document in the database with the specified document.
        /// </summary>
        /// <param name="doc">The document to replace in the database.</param>
        /// <param name="query">An optional query used to match the document to be replaced.</param>
        /// <param name="replaceOptions">Optional settings for the replacement operation.</param>
        /// <param name="forceTransaction">Optional flag to force the operation to be executed in a transaction.</param>
        /// <param name="cancellationToken">A token for canceling the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous replace operation, 
        /// containing the number of modified documents, or -1 if the operation was not acknowledged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<long> ReplaceAsync([NotNull] TDocument doc,
                    [MaybeNull] Query<TDocument>? query = default,
                    [MaybeNull] ReplaceOptions? replaceOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            var result = await this.ReplaceOneAsync(doc, query, replaceOptions, forceTransaction, cancellationToken);


            // Return the number of replaced documents, or -1 if the operation was not acknowledged
            return result == null ? -1 : result.IsAcknowledged ? result.ModifiedCount : -1;
        }

        /// <summary>
        /// Asynchronously replaces a single document in the collection.
        /// </summary>
        /// <param name="doc">The document to replace. This cannot be null.</param>
        /// <param name="query">An optional query to filter the documents to replace. If not provided, a default query will be used based on the document's ID.</param>
        /// <param name="replaceOptions">Optional options for the replace operation. If not specified, default replace options will be used.</param>
        /// <param name="forceTransaction">Optional boolean indicating whether to force the operation to run within a transaction.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task{ReplaceOneResult}"/> that represents the asynchronous replace operation. The result indicates the outcome of the replace operation, including the number of documents replaced.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<ReplaceOneResult> ReplaceOneAsync([NotNull] TDocument doc,
                    [MaybeNull] Query<TDocument>? query = default,
                    [MaybeNull] ReplaceOptions? replaceOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set default replace options if not provided
            replaceOptions ??= new ReplaceOptions();

            // Initialize the result to null
            ReplaceOneResult result;

            // Create a filter definition to match the document
            FilterDefinition<TDocument> filterDefinition = query ?? Query<TDocument>.FromExpression(f => f.Id.Equals(doc.Id));

            this._contextbase.BeforeInsertInternal<TDocument, TObjectId, ReplaceOneResult, TUser>(this, ref doc, ref replaceOptions);

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the replace operation with a session
                result = await this.MongoCollection.ReplaceOneAsync(clientSessionHandle, filterDefinition, doc, replaceOptions, cancellationToken);
            }
            else
            {
                // Perform the replace operation without a session
                result = await this.MongoCollection.ReplaceOneAsync(filterDefinition, doc, replaceOptions, cancellationToken);
            }


            // Return the number of replaced documents, or -1 if the operation was not acknowledged
            this._contextbase.ResultInternal(this, ref result);

            return result;
        }

        #endregion

        #region Delete

        /// <summary>
        /// Asynchronously deletes a document from the database by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the document to delete.</param>
        /// <param name="deleteOptions">Optional parameters to customize the deletion operation.</param>
        /// <param name="forceTransaction">Optional flag to force the deletion to occur within a transaction.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task{long}"/> representing the asynchronous operation. The value contains the number of documents deleted.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<long> DeleteOneAsync([NotNull] TObjectId id,
                    [MaybeNull] DeleteOptions? deleteOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set default delete options if not provided
            deleteOptions ??= new DeleteOptions();


            // Delete the document by its ID
            return await this.DeleteAsync([id], deleteOptions, forceTransaction, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes multiple documents based on the provided write models.
        /// This method utilizes bulk write options and can optionally enforce a transaction.
        /// </summary>
        /// <param name="enumeratorWriteModel">
        /// An <see cref="IEnumerable{WriteModel{TDocument}}"/> containing the write models to be executed.
        /// Each model defines how the associated document should be deleted.
        /// </param>
        /// <param name="bulkWriteOptions">
        /// Optional. A <see cref="BulkWriteOptions"/> object that specifies options for the bulk write operation.
        /// If not provided, a default instance will be created.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional. A <see cref="bool"/> that indicates whether to force the operation to be executed within a transaction.
        /// Defaults to null, meaning it will not force a transaction.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional. A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.
        /// If the cancellation is requested, an <see cref="OperationCanceledException"/> will be thrown.
        /// </param>
        /// <returns>
        /// A <see cref="Task{long}"/> representing the asynchronous operation, 
        /// containing the number of documents deleted as the result.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
        /// </exception>
        /// <remarks>
        /// This method is marked with <see cref="MethodImplOptions.AggressiveInlining"/> 
        /// to suggest that the compiler should inline the method if possible for performance optimization.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<long> DeleteAsync([NotNull] IEnumerable<WriteModel<TDocument>> enumeratorWriteModel,
                    [MaybeNull] BulkWriteOptions? bulkWriteOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bulkWriteOptions ??= new BulkWriteOptions();

            return await this.BulkWriteAsync(enumeratorWriteModel as List<WriteModel<TDocument>> ?? enumeratorWriteModel.ToList(), bulkWriteOptions, forceTransaction, cancellationToken);
        }


        /// <summary>
        /// Asynchronously deletes a collection of documents identified by their IDs from the database.
        /// </summary>
        /// <param name="ids">
        /// A collection of object IDs representing the documents to be deleted.
        /// </param>
        /// <param name="deleteOptions">
        /// Optional settings for the delete operation. If not provided, defaults will be used.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional flag indicating whether the operation should be executed in a forced transaction context.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to signal the cancellation of the asynchronous operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous delete operation. The task result contains the number of documents deleted.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<long> DeleteAsync([NotNull] IEnumerable<TObjectId> ids,
                    [MaybeNull] DeleteOptions? deleteOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set default delete options if not provided
            deleteOptions ??= new DeleteOptions();

            var bulkWriteOptions = new BulkWriteOptions()
            {
                BypassDocumentValidation = false,
                Comment = deleteOptions.Comment,
                IsOrdered = true,
                Let = deleteOptions.Let
            };

            var listWriteModel = new List<WriteModel<TDocument>>();

            foreach (var id in ids)
            {
                Expression<Func<TDocument, bool>> exp = (f) => f.Id.Equals(id);

                FilterDefinition<TDocument> filterDefinition = exp;

                var model = new DeleteOneModel<TDocument>(filterDefinition) { };

                listWriteModel.Add(model);

                this._contextbase.BeforeDeleteInternal<TDocument, TObjectId, TDocument, TUser>(this, ref filterDefinition, ref deleteOptions);
            }

            // Perform the bulk write operation with the write models and options
            return await this.DeleteAsync(listWriteModel, bulkWriteOptions, forceTransaction, cancellationToken);
        }


        /// <summary>
        /// Asynchronously deletes documents matching the given query from the database.
        /// This method constructs a bulk write operation for deletion and executes it.
        /// </summary>
        /// <param name="query">
        /// The query used to identify the documents to be deleted. This parameter cannot be null.
        /// </param>
        /// <param name="deleteOptions">
        /// Optional parameters that define how the delete operation should behave, such as collation and hints.
        /// If no options are specified, default options will be used.
        /// </param>
        /// <param name="forceTransaction">
        /// An optional boolean indicating whether to force the deletion operation to occur within a transaction.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task{T}"/> representing the asynchronous operation. 
        /// The result is the number of documents deleted.
        /// </returns>
        /// <remarks>
        /// This method will create a delete operation using the specified <paramref name="query"/> and 
        /// apply the settings specified in <paramref name="deleteOptions"/>. It will perform the operation 
        /// in a bulk write context, which allows for more efficient execution of the deletion command.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<long> DeleteAsync([NotNull] Query<TDocument> query,
                    [MaybeNull] DeleteOptions? deleteOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            deleteOptions ??= new DeleteOptions();

            var listWriteModel = new List<WriteModel<TDocument>>
            {
                new DeleteManyModel<TDocument>(query)
                {
                    Collation = deleteOptions.Collation,
                    Hint = deleteOptions.Hint
                }
            };

            var bulkWriteOptions = new BulkWriteOptions()
            {
                IsOrdered = false,
                BypassDocumentValidation = true,
                Let = deleteOptions.Let,
                Comment = deleteOptions.Comment
            };

            return await this.BulkWriteAsync(listWriteModel, bulkWriteOptions, forceTransaction, cancellationToken);
        }

        #endregion

        #region Aggregate


        /// <summary>
        /// Asynchronously aggregates the results of a query and returns an asynchronous enumerable of documents.
        /// </summary>
        /// <param name="query">The query object that defines the aggregation criteria.</param>
        /// <param name="aggregateOptions">Optional settings for the aggregation operation.</param>
        /// <param name="forceTransaction">Optional boolean to force the operation to run in a transaction.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>
        /// An asynchronous enumerable that provides the aggregated documents.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> AggregateFacetEnumerableAsync([NotNull] Query<TDocument> query,
                    [MaybeNull] AggregateOptions? aggregateOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull][EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Set default aggregate options if not provided
            aggregateOptions ??= new AggregateOptions();

            // Perform the aggregation operation and iterate over the results
            foreach (var item in (await this.AggregateFacetAsync(query, DbSet<TDocument, TObjectId, TUser>.ConvertInternal(aggregateOptions), forceTransaction, cancellationToken)).Results)
            {
                // Yield return each item in the aggregation results
                yield return item;
            }
        }


        /// <summary>
        /// Asynchronously performs an aggregation operation with pagination on a specified query and returns a paged result.
        /// </summary>
        /// <param name="query">The query to execute as part of the aggregation.</param>
        /// <param name="aggregateOptionsPaging">The options for pagination in the aggregation, including skip and limit.</param>
        /// <param name="forceTransaction">An optional flag to force the operation within a transaction.</param>
        /// <param name="cancellationToken">A cancellation token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="PagedResult{TDocument}"/> containing the results of the aggregation along with pagination information.</returns>
        /// <exception cref="Exception">Throws if the skip or limit options for pagination are invalid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<PagedResult<TDocument>> AggregateFacetAsync([NotNull] Query<TDocument> query,
                    [MaybeNull] AggregateOptionsPaging? aggregateOptionsPaging = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Set default aggregate options if not provided
            aggregateOptionsPaging ??= new AggregateOptionsPaging();

            // Check if the skip and limit options are valid
            if (!aggregateOptionsPaging.Skip.HasValue || aggregateOptionsPaging.Skip.Value < 0)
            {
                throw new Exception("Skip is invalid or null.");
            }

            if (!aggregateOptionsPaging.Limit.HasValue || aggregateOptionsPaging.Limit.Value <= 0)
            {
                throw new Exception("Limit is invalid or null.");
            }


            // Convert the query to a BsonDocument array
            BsonDocument[] bsonDocumentFilter = query;

            AggregateOptions aggregateOptions = aggregateOptionsPaging;

            this._contextbase.BeforeAggregateInternal<TDocument, TObjectId, TDocument, TUser>(this, ref bsonDocumentFilter, ref aggregateOptions);

            // Create a list to hold the paging filters
            var bsonDocumentFilterPaging = ((BsonDocument[])query).ToList();

            // Add skip and limit filters to the paging filters
            bsonDocumentFilterPaging.Add(new BsonDocument(new BsonElement("$skip", BsonValue.Create(aggregateOptionsPaging.Skip.Value))));
            bsonDocumentFilterPaging.Add(new BsonDocument(new BsonElement("$limit", BsonValue.Create(aggregateOptionsPaging.Limit.Value))));

            // Create the aggregation pipeline structure
            var structAggregate = "[ { \"$facet\": { \"result\": [],\"total\": [{\"$count\": \"total\"}]}} ]";

            // Deserialize the aggregation pipeline structure into a BsonDocument array
            var bson = BsonSerializer.Deserialize<BsonDocument[]>(structAggregate);

            // Set the filter part of the aggregation pipeline to the paging filters
            bson[0][0][0] = new BsonArray(bsonDocumentFilterPaging);

            // Add the original filters to the beginning of the filter part of the aggregation pipeline
            foreach (var it in new BsonArray(bsonDocumentFilter).Reverse())
            {
                ((BsonArray)bson[0][0][1]).Insert(0, it);
            }

            // If the debugger is attached, serialize the aggregation pipeline to a string for debugging purposes
            if (Debugger.IsAttached)
            {
                // converter novamente em string para verificar se o json de consulta esta correto
                var stringWriter = new StringWriter();
                BsonSerializer.Serialize(new JsonWriter(stringWriter), bson);
                //var json = stringWriter.ToString();
            }

            // Perform the aggregation operation and iterate over the results
            FacedAggregate<TDocument> item = default;

            IAsyncCursor<FacedAggregate<TDocument>>? cursor = null;

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Create a cursor for the aggregation operation
                cursor = await this.MongoCollection.AggregateAsync<FacedAggregate<TDocument>>(clientSessionHandle, bson, aggregateOptionsPaging, cancellationToken);
            }
            else
            {
                // Create a cursor for the aggregation operation
                cursor = await this.MongoCollection.AggregateAsync<FacedAggregate<TDocument>>(bson, aggregateOptionsPaging, cancellationToken);
            }

            while (await cursor.MoveNextAsync(cancellationToken))
            {
                foreach (var c in cursor.Current)
                {
                    // Set the item variable to the current result
                    item = c;
                }
            }

            // Dispose the cursor
            cursor.Dispose();

            this._contextbase.ResultInternal(this, ref item);

            // Return the result of the aggregation operation, the skip value, the limit value, and the total number of rows
            if (item != default)
            {
                return new PagedResult<TDocument>(item.Result.ToArray(), aggregateOptionsPaging.CurrentPage, aggregateOptionsPaging.PageSize, item.TotalRows());
            }

            // If there is no result, return the default value
            return default;
        }


        /// <summary>
        /// Asynchronously aggregates documents based on the provided query and options.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection to be returned by the aggregation.</typeparam>
        /// <param name="query">The query containing the criteria for aggregation.</param>
        /// <param name="aggregateOptions">Optional parameters that control how the aggregation is performed.</param>
        /// <param name="forceTransaction">Optional flag to indicate whether to force the operation into a transaction.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a read-only list 
        /// of the aggregated projection results, which may include null values.
        /// </returns>
        public async Task<IReadOnlyList<TProjection?>> AggregateAsync<TProjection>([NotNull] Query<TDocument, TProjection> query,
                    [MaybeNull] AggregateOptions? aggregateOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            var result = new List<TProjection?>();

            await foreach (var item in this.AggregateEnumerableAsync<TProjection>(query, aggregateOptions, forceTransaction, cancellationToken))
            {
                result.Add(item);
            }

            return result.AsReadOnly();
        }

        /// <summary>
        /// Asynchronously aggregates results from a query and yields the results as an 
        /// asynchronous enumerable, allowing for efficient processing of large datasets 
        /// without loading everything into memory at once.
        /// </summary>
        /// <typeparam name="TProjection">
        /// The type of the projection to be returned from the aggregation query.
        /// </typeparam>
        /// <param name="query">
        /// The query to be executed, specified as a <see cref="Query{TDocument, TProjection}"/> 
        /// which contains the aggregation filters.
        /// </param>
        /// <param name="aggregateOptions">
        /// Optional parameters for controlling the aggregation operation, specified as an 
        /// instance of <see cref="AggregateOptions"/>. If not provided, default options are used.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional parameter that indicates whether to force the aggregation to run 
        /// within a transaction. Defaults to null.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the operation. If no token is 
        /// provided, a default cancellation token will be used.
        /// </param>
        /// <returns>
        /// An asynchronous enumerable of <typeparamref name="TProjection"/> that allows 
        /// the caller to iterate over each projection result as it becomes available.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the provided <paramref name="cancellationToken"/>.
        /// </exception>
        /// <remarks>
        /// This method utilizes asynchronous processing to yield results one at a time. 
        /// It can operate within a transaction if specified and ensures that even large 
        /// datasets can be processed without overwhelming system memory.
        /// Additionally, if a debugger is attached, the aggregation pipeline is logged 
        /// for debugging purposes.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection?> AggregateEnumerableAsync<TProjection>(
            [NotNull] Query<TDocument, TProjection> query,
            [MaybeNull] AggregateOptions? aggregateOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set default aggregate options if not provided
            aggregateOptions ??= new AggregateOptions();

            // Convert the query to a BsonDocument array
            BsonDocument[] bsonDocumentFilter = query!;

            this._contextbase.BeforeAggregateInternal<TDocument, TObjectId, TProjection, TUser>(this, ref bsonDocumentFilter, ref aggregateOptions);

            // If the debugger is attached, serialize the aggregation pipeline to a string for debugging purposes
            if (Debugger.IsAttached)
            {
                // converter novamente em string para verificar se o json de consulta esta correto
                var stringWriter = new StringWriter();
                BsonSerializer.Serialize(new JsonWriter(stringWriter), bsonDocumentFilter);
                if (stringWriter != null)
                {
                    this.Logger.LogDebug(stringWriter.ToString());
                }
            }

            IAsyncCursor<TProjection> cursor = default!;

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Create a cursor for the aggregation operation with the session and filter
                cursor = await this.MongoCollection.AggregateAsync<TProjection>(clientSessionHandle, bsonDocumentFilter, aggregateOptions, cancellationToken);
            }
            else
            {
                // Create a cursor for the aggregation operation
                cursor = await this.MongoCollection.AggregateAsync<TProjection>(bsonDocumentFilter, aggregateOptions, cancellationToken);
            }


            while (await cursor.MoveNextAsync(cancellationToken))
            {
                foreach (var current in cursor.Current)
                {
                    TProjection? c = current;

                    this._contextbase.ResultInternal(this, ref c);

                    yield return c;
                }
            }

            // Dispose the cursor
            cursor.Dispose();
        }


        #endregion


        /// <summary>
        /// Commits the current transaction if transactions are enabled.
        /// </summary>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> that can be used to observe while waiting for the 
        /// operation to complete and can be used to cancel the operation if needed. The default value 
        /// is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when an attempt is made to commit a transaction while transactions are not enabled.
        /// </exception>
        public void CommitTransaction(CancellationToken cancellationToken = default)
        {
            if (this._sessionHandle != null)
            {
                this._sessionHandle.CommitTransaction(cancellationToken);

                this._sessionHandle = this._contextbase.CreateSession();
            }
            else
            {
                throw new InvalidOperationException("Commit transaction is invalid.");
            }
        }

        /// <summary>
        /// Aborts the current transaction if one is in use.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. Default is <see cref="CancellationToken.None"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if there is no active transaction to abort.</exception>
        public void AbortTransaction(CancellationToken cancellationToken = default)
        {
            if (this._sessionHandle != null)
            {
                this._sessionHandle.AbortTransaction(cancellationToken);

                this._sessionHandle = this._contextbase.CreateSession();
            }
            else
            {
                throw new InvalidOperationException("Abort transaction is invalid.");
            }
        }





        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ContextBase(DbSet<TDocument, TObjectId, TUser> dbSet) => dbSet._contextbase;













        #region Dispose

        /// <summary>
        /// Releases the resources used by the current instance of the class.
        /// This method implements the Dispose pattern and should be called 
        /// when the object is no longer needed.
        /// </summary>
        /// <remarks>
        /// Calling this method will also suppress the finalization of the object, 
        /// preventing the garbage collector from calling the destructor 
        /// if the Dispose method has already been called.
        /// </remarks>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously releases the resources used by the object.
        /// This method should be called when the object is no longer needed.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous operation.
        /// </returns>
        public async ValueTask DisposeAsync()
        {
            await this.DisposeAsyncCore();

            this.Dispose(false);
        }

        /// <summary>
        /// Releases the resources used by the current instance of the class.
        /// </summary>
        /// <param name="disposing">
        /// A boolean value indicating whether the method was called directly or through 
        /// the Dispose method. If true, it means that both managed and unmanaged resources 
        /// should be disposed; if false, only unmanaged resources should be released.
        /// </param>
        /// <remarks>
        /// This method is intended to be overridden by derived classes to dispose of 
        /// their specific resources. It ensures that the disposing field is checked before 
        /// attempting to dispose of managed resources, thereby preventing any potential 
        /// exceptions that could occur when accessing disposed objects.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._sessionHandle?.Dispose();
            }
        }


        /// <summary>
        /// Asynchronously disposes of resources that require disposal, focusing on 
        /// both asynchronous disposable resources and regular disposable resources.
        /// </summary>
        /// <returns>
        /// A Task representing the asynchronous operation of resource disposal.
        /// </returns>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (this._sessionHandle is not null and IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                this._sessionHandle?.Dispose();
            }
        }

        #endregion Dispose
    }




}
