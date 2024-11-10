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
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using NPOI.OpenXml4Net.OPC.Internal.Unmarshallers;
using UCode.Extensions;
using UCode.Mongo.Options;
using UCode.Repositories;

namespace UCode.Mongo
{
    //https://mongodb.github.io/mongo-csharp-driver/2.11/reference/driver/crud/writing/
    /// <summary>
    /// Represents a set of documents of type TDocument.
    /// </summary>
    /// <typeparam name="TDocument">The type of document in the set.</typeparam>
    public class DbSet<TDocument> : DbSet<TDocument, string>
        where TDocument : IObjectBase<string>, IObjectBaseTenant
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DbSet"/> class.
        /// </summary>
        /// <param name="contextBase">The context base that is used to interact with the database.</param>
        /// <param name="collectionName">An optional name of the collection. If <c>null</c>, a default name is used.</param>
        /// <param name="createCollectionOptionsAction">An optional action to configure the collection creation options.</param>
        /// <param name="mongoCollectionSettingsAction">An optional action to configure the MongoDB collection settings.</param>
        /// <param name="useTransaction">Specifies whether to use transactions for operations on this collection.</param>
        /// <returns>
        /// A new instance of the <see cref="DbSet"/> class.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet([NotNull] ContextBase contextBase, string? collectionName = null,
            Action<CreateCollectionOptions>? createCollectionOptionsAction = null,
            Action<MongoCollectionSettings>? mongoCollectionSettingsAction = null,
            bool useTransaction = false) : base(contextBase, collectionName, createCollectionOptionsAction, mongoCollectionSettingsAction)
        {

        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string? ToString() => base.ToString();
    }


    //https://mongodb.github.io/mongo-csharp-driver/2.11/reference/driver/crud/writing/
    /// <summary>
    /// Represents a set of documents in a MongoDB collection with CRUD functionalities.
    /// This class implements asynchronous and synchronous operations to interact with 
    /// the MongoDB database, including methods for finding, inserting, updating, and deleting documents.
    /// </summary>
    /// <typeparam name="TDocument">The type of the documents in the collection.</typeparam>
    /// <typeparam name="TObjectId">The type of the object identifier.</typeparam>
    public class DbSet<TDocument, TObjectId> : IDisposable, IAsyncDisposable
            where TDocument : IObjectBase<TObjectId>, IObjectBaseTenant
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        #region Fields
        protected readonly IMongoCollection<TDocument> MongoCollection;

        private ContextCollectionMetadata _contextCollectionMetadata;


        private readonly ContextBase _contextbase;


        protected ILogger<DbSet<TDocument, TObjectId>> Logger
        {
            get;
        }

        public string CollectionName
        {
            get;
        }

        public bool UseTransaction
        {
            get;
        }
        #endregion Fields


        #region private methods
        /// <summary>
        /// Determines whether a transaction is active or should be initiated based on the 
        /// provided forceTransaction parameter and the current transactional context.
        /// </summary>
        /// <param name="forceTransaction">A nullable boolean that indicates whether to force 
        /// the initiation of a transaction. If null, the current transactional context is checked.</param>
        /// <param name="clientSessionHandle">An output parameter that will hold the session handle 
        /// if a transaction is started or a new session is created. It will be null if no session is created.</param>
        /// <returns>
        /// Returns a boolean value indicating whether the client session handle is the default value.
        /// </returns>
        private bool InTransaction(bool? forceTransaction, out IClientSessionHandle? clientSessionHandle)
        {
            if (!forceTransaction.HasValue && this._contextbase.TransactionalContext)
            {
                clientSessionHandle = this._contextbase.StartTransaction();
            }
            else if (forceTransaction.HasValue && forceTransaction.Value)
            {
                clientSessionHandle = this._contextbase.CreateSession();
            }
            else
            {
                clientSessionHandle = null;
            }

            return clientSessionHandle != default;
        }

        /// <summary>
        /// Converts the specified <see cref="FindOptionsPaging{TDocument}"/> to a <see cref="FindOptionsPaging{TDocument, TProjection}"/>.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="findOptionsPaging">The options to be converted, or <c>null</c> if no options are provided.</param>
        /// <returns>A <see cref="FindOptionsPaging{TDocument, TProjection}"/> instance with the converted options.</returns>
        private static FindOptionsPaging<TDocument, TProjection> ConvertInternal<TProjection>(FindOptionsPaging<TDocument>? findOptionsPaging)
        {
            var opt = new FindOptionsPaging<TDocument, TProjection>();

            if (findOptionsPaging != default)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(findOptionsPaging);

                opt = System.Text.Json.JsonSerializer.Deserialize<FindOptionsPaging<TDocument, TProjection>>(json)!;
            }

            return opt;
        }

        /// <summary>
        /// Converts the provided <see cref="AggregateOptions"/> to an 
        /// instance of <see cref="AggregateOptionsPaging"/>. If the 
        /// provided options are not null, it serializes them to JSON 
        /// and then deserializes into the <see cref="AggregateOptionsPaging"/> 
        /// format. If the provided options are null or default, it 
        /// initializes a new instance of <see cref="AggregateOptionsPaging"/>.
        /// </summary>
        /// <param name="findOptionsPaging">
        /// An optional parameter of type <see cref="AggregateOptions"/> 
        /// that contains the options to be converted. It can be null.
        /// </param>
        /// <returns>
        /// Returns an instance of <see cref="AggregateOptionsPaging"/>. 
        /// If <paramref name="findOptionsPaging"/> is null or default, 
        /// an empty <see cref="AggregateOptionsPaging"/> instance is returned.
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
        /// Converts the provided FindOptions to a FindOptions of type TDocument.
        /// </summary>
        /// <param name="findOptions">The FindOptions instance to convert. Can be null.</param>
        /// <returns>
        /// A FindOptions&lt;TDocument, TDocument&gt; instance.
        /// If findOptions is null, returns a new instance with default values.
        /// Otherwise, returns a deserialized instance based on the input findOptions.
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
        /// Converts the specified <see cref="FindOptions{TDocument}"/> to a new instance if it is null.
        /// </summary>
        /// <param name="findOptions">The options to convert, which can be null.</param>
        /// <returns>
        /// A <see cref="FindOptions{TDocument}"/> instance. If <paramref name="findOptions"/> is not null, 
        /// it returns the existing instance; otherwise, it returns a new instance of <see cref="FindOptions{TDocument}"/>.
        /// </returns>
        private static FindOptions<TDocument, TDocument> ConvertInternal(FindOptions<TDocument>? findOptions) => findOptions ?? new FindOptions<TDocument>();
        #endregion private methods

        #region constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DbSet{TDocument, TObjectId}"/> class.
        /// </summary>
        /// <param name="contextBase">The context base that provides access to the database.</param>
        /// <param name="collectionName">Optional. The name of the collection to be used. If not provided, a default name will be generated.</param>
        /// <param name="createCollectionOptionsAction">Optional. An action to configure collection creation options.</param>
        /// <param name="mongoCollectionSettingsAction">Optional. An action to configure MongoDB collection settings.</param>
        /// <param name="useTransaction">Indicates whether to use transactions when performing operations.</param>
        /// <returns>
        /// A new instance of the <see cref="DbSet{TDocument, TObjectId}"/> class with the specified configurations.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet([NotNull] ContextBase contextBase, string? collectionName = null,
            Action<CreateCollectionOptions>? createCollectionOptionsAction = null,
            Action<MongoCollectionSettings>? mongoCollectionSettingsAction = null,
            bool useTransaction = false)
        {
            this.UseTransaction = useTransaction;

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

            // Set the context base
            this._contextbase = contextBase;

            // Initialize the logger
            this.Logger = contextBase.LoggerFactory.CreateLogger<DbSet<TDocument, TObjectId>>();


            if (!this._contextbase._contextCollectionMetadata.ContainsKey(this.CollectionName))
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
                    option.Unique = true;
                    option.Name = "IDX_REF_DISABLED";
                });

                indexKeys.Ascending(x => x.Tenant, (option) =>
                {
                    // Create the index in the background to avoid blocking other operations
                    option.Background = true;
                    option.Unique = false;
                    // Name the index for easy reference
                    option.Name = "IDX_TENANT";
                });

                indexKeys.Ascending(x => x.Tenant).Ascending(x => x.Ref).Ascending(x => x.Disabled, (option) =>
                {
                    option.Background = true;
                    option.Unique = true;
                    option.Name = "IDX_TENANT_REF_DISABLED";
                });


                var contextCollectionMetadata = new ContextCollectionMetadata(this.CollectionName)
                {
                    IndexKeys = indexKeys,
                    BsonClassMaps = this.ReflectionRegisterClassMap()
                };


                this._contextbase._contextCollectionMetadata.Add(this.CollectionName, contextCollectionMetadata);
            }
            this._contextCollectionMetadata = this._contextbase._contextCollectionMetadata[this.CollectionName];

            _ = this.InternalIndex(false);
        }
        #endregion constructor

        private IEnumerable<BsonClassMap> ReflectionRegisterClassMap()
        {
            var result = GetBsonClassMaps().ToArray();

            foreach (var bsonClassMap in result)
            {
                if (!BsonClassMap.IsClassMapRegistered(bsonClassMap.ClassType))
                {
                    BsonClassMap.RegisterClassMap(bsonClassMap);
                }

                yield return bsonClassMap;
            }
        }

        private IEnumerable<BsonClassMap> GetBsonClassMaps()
        {
            var thisType = this._contextbase.GetType();

            var props = thisType.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty)
                .Where(w => w.PropertyType.IsGenericType && (w.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) || w.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<,>)));

            var methods = thisType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                .Where(w => w.Name.Equals("Map", StringComparison.Ordinal) && w.GetParameters().Length > 0 && w.GetParameters().All(a => a.ParameterType.IsGenericType && a.ParameterType.GetGenericTypeDefinition() == typeof(BsonClassMap<>)))
                .Select(s => new { BsonClassMap = s.GetParameters()[0].ParameterType, BsonClassMapGeneric = s.GetParameters()[0].ParameterType.GenericTypeArguments[0], Method = s }).ToArray();

            foreach (var prop in props)
            {
                var objectIdImplementationType = prop.PropertyType.GenericTypeArguments[0];

                var bsonClassMapType = typeof(BsonClassMap<>).MakeGenericType(objectIdImplementationType);

                var basonClassMap = (BsonClassMap)Activator.CreateInstance(typeof(BsonClassMap<>).MakeGenericType(objectIdImplementationType))!;

                basonClassMap.MapExtraElementsProperty("ExtraElements");
                //var method = methods.SingleOrDefault(s => s.BsonClassMap == bsonClassMapType);

                //_ = method?.Method.Invoke(this, [bsonClassMapType]);

                yield return basonClassMap;
            }
        }





        #region index methods

        /// <summary>
        /// Asynchronously retrieves the indexes from a MongoDB collection and their corresponding fields.
        /// Yields a key-value pair for each index where the key is the index name 
        /// and the value is a list of fields indexed by that index.
        /// </summary>
        /// <param name="forceTransaction">
        /// Optional. A boolean flag indicating whether to force the use of a transaction. 
        /// If not specified, the default value is null.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional. A token to monitor for cancellation requests. The default is a new <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>
        /// An asynchronous sequence of key-value pairs, where each pair consists of:
        /// - Key: the name of the index (string).
        /// - Value: a list of strings representing the fields indexed by the index (List&lt;string&gt;).
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<KeyValuePair<string, List<string>>> GetIndexesAsync(bool? forceTransaction = default,
            CancellationToken cancellationToken = default)
        {
            IAsyncCursor<BsonDocument> idxsb = null;

            if (InTransaction(forceTransaction, out var clientSessionHandle))
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


        private bool InternalIndex(bool? forceTransaction = default)
        {
            var contextbaseType = this._contextbase.GetType();
            var methods = contextbaseType
                .GetMethods();

            var index_method = methods.SingleOrDefault(w => w.Name.Equals("Index", StringComparison.Ordinal) && w.GetParameters().Length == 1 && w.GetParameters()[0].ParameterType == typeof(IndexKeys<TDocument>));
            
            index_method?.Invoke(this._contextbase, [(IndexKeys<TDocument>)_contextCollectionMetadata.IndexKeys]);


            return this.IndexAsync((IndexKeys<TDocument>)_contextCollectionMetadata.IndexKeys, forceTransaction).GetAwaiter().GetResult();
        }

        public async ValueTask<bool> IndexAsync([NotNull] IndexKeys<TDocument> indexKeys,
            bool? forceTransaction = default,
            CancellationToken cancellationToken = default) => await this.IndexAsync((List<CreateIndexModel<TDocument>>)indexKeys, forceTransaction, cancellationToken);


        public async ValueTask<bool> IndexAsync([NotNull] List<CreateIndexModel<TDocument>> models,
            bool? forceTransaction = default,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if session should be used
                if (InTransaction(forceTransaction, out var clientSessionHandle))
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
        /// Converts the current collection into an <see cref="IQueryable{TDocument}"/> 
        /// representation, allowing for queries to be executed against the document store.
        /// </summary>
        /// <param name="aggregateOptions">An optional <see cref="AggregateOptions"/> 
        /// instance specifying options for the aggregation operation. If not provided, 
        /// default aggregation options are used.</param>
        /// <param name="forceTransaction">An optional boolean indicating whether 
        /// to force the use of a transaction. If <c>null</c>, default behavior is applied.</param>
        /// <returns>
        /// An <see cref="IQueryable{TDocument}"/> representing the documents in the 
        /// collection, which can be further queried as needed.
        /// </returns>
        public IQueryable<TDocument> AsQueryable(AggregateOptions? aggregateOptions = null,
                    bool? forceTransaction = default)
        {
            IQueryable<TDocument> queryable;

            var option = aggregateOptions ?? new AggregateOptions();

            // If a transaction is in use
            if (InTransaction(forceTransaction, out var clientSessionHandle))
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

            return queryable;
        }
        #endregion queryable


        #region FirstOrDefault


        /// <summary>
        /// Asynchronously retrieves the first document that matches the specified query.
        /// Returns the default value of TDocument if no documents match the query.
        /// </summary>
        /// <param name="query">
        /// The query to be executed against the collection, which defines the criteria to find the document.
        /// </param>
        /// <param name="findOptions">
        /// Optional parameters that specify additional options for the find operation.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional boolean indicating whether to force the operation to run within a transaction.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional cancellation token to cancel the operation.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TDocument}"/> representing the asynchronous operation, with the first matched document or a default value if no documents were found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TDocument> FirstOrDefaultAsync([NotNull] Query<TDocument> query,
            [MaybeNull] FindOptions? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            var opt = ConvertInternal(findOptions);

            var qry = (Query<TDocument, TDocument>)query;

            return await this.GetOneAsync(qry, opt, forceTransaction, cancellationToken);
        }




        /// <summary>
        /// Asynchronously retrieves the first element of a sequence that satisfies a specified condition
        /// or a default value if no such element is found. The operation will use the provided query and options.
        /// </summary>
        /// <typeparam name="TProjection">The type of the elements in the result projection.</typeparam>
        /// <param name="query">The query that is used to filter the elements.</param>
        /// <param name="findOptions">
        /// Optional. Options that specify how to retrieve the elements. If null, default options will be used.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional. A boolean flag indicating whether to force the query to execute within a transaction. 
        /// If null, the default behavior will be used.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional. A cancellation token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is the first element of 
        /// the specified sequence that satisfies the condition, or the default value if no such element 
        /// is found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TProjection> FirstOrDefaultAsync<TProjection>([NotNull] Query<TDocument, TProjection> query,
            [MaybeNull] FindOptions<TDocument, TProjection>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default) => await this.GetOneAsync(query, findOptions, forceTransaction, cancellationToken);

        #endregion


        #region Any


        /// <summary>
        /// Asynchronously determines whether any documents in the collection match the provided query.
        /// </summary>
        /// <param name="query">The query to match documents against.</param>
        /// <param name="countOptions">Optional parameters to customize the count operation.</param>
        /// <param name="forceTransaction">Specifies whether to force the operation to run within a transaction.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{bool}"/> that represents the asynchronous operation, 
        /// containing a value that indicates whether any documents were found that match the query.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is canceled via the <paramref name="cancellationToken"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<bool> AnyAsync([NotNull] Query<TDocument> query,
            CountOptions? countOptions = default,
            bool? forceTransaction = default,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set limit and skip options for counting
            countOptions ??= new CountOptions();

            // Count documents and check if count is greater than 0
            if (InTransaction(forceTransaction, out var clientSessionHandle))
            {
                return await this.MongoCollection.CountDocumentsAsync(clientSessionHandle, query, countOptions, cancellationToken) > 0;
            }
            else
            {
                return await this.MongoCollection.CountDocumentsAsync(query, countOptions, cancellationToken) > 0;
            }
        }

        #endregion

        #region Get

        #region Get One


        /// <summary>
        /// Asynchronously retrieves a document of type <typeparamref name="TDocument"/> 
        /// from a data source based on the specified identifier and options.
        /// </summary>
        /// <param name="id">The unique identifier of the document to retrieve. Must not be null.</param>
        /// <param name="findOptions">Optional. The options to customize the find operation. Can be null.</param>
        /// <param name="forceTransaction">Optional. A flag indicating whether to force the operation to run in a transaction. Can be null.</param>
        /// <param name="cancellationToken">Optional. A token to monitor for cancellation requests. Can be null.</param>
        /// <returns>A <see cref="ValueTask{TDocument}"/> representing the asynchronous operation, 
        /// containing the retrieved document or the default value if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TDocument> GetAsync([NotNull] TObjectId id,
            [NotNull] FindOptions? findOptions = default,
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
        /// from the data source using the specified identifier and options.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the document to retrieve. This parameter cannot be null.
        /// </param>
        /// <param name="findOptions">
        /// Optional parameters that control the behavior of the find operation.
        /// This can be null, in which case default options will be used.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional flag indicating whether to force the operation to be performed within a transaction.
        /// This can be null, in which case the default transaction behavior will apply.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the asynchronous operation. 
        /// This can be null, and default cancellation behavior will be used.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TDocument}"/> representing the asynchronous operation, 
        /// which will contain the document of type <typeparamref name="TDocument"/> 
        /// if found, or a default value if not found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TDocument> GetAsync([NotNull] TObjectId id,
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
        /// Asynchronously retrieves a projection of a document specified by its identifier.
        /// </summary>
        /// <typeparam name="TProjection">
        /// The type of the projection to be returned.
        /// </typeparam>
        /// <param name="id">
        /// The identifier of the document to retrieve. This parameter must not be null.
        /// </param>
        /// <param name="findOptions">
        /// Optional arguments for finding the document. Defaults to null.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional parameter indicating whether to force a transaction. Defaults to null.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional token to observe while waiting for the task to complete. Defaults to null.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TProjection}"/> representing the asynchronous operation,
        /// containing the projection of the document if found, or the default value of <typeparamref name="TProjection"/> if not.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TProjection> GetAsync<TProjection>([NotNull] TObjectId id,
            [MaybeNull] FindOptions<TDocument, TProjection>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Create a query to find the document by its ID
            var qry = Query<TDocument, TProjection>.FromExpression(o => o.Id.Equals(id));

            // Call the GetOneAsync method with the query and options
            return await this.GetOneAsync(qry, findOptions, forceTransaction, cancellationToken);
        }


        /// <summary>
        /// Retrieves a single projected document asynchronously from a MongoDB collection based on the specified query and options.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projected result.</typeparam>
        /// <param name="query">The query object that defines the filter for the search.</param>
        /// <param name="findOptions">Optional parameters controlling the find operation, such as limits and skips.</param>
        /// <param name="forceTransaction">Optional parameter to force the operation to run in a transaction.</param>
        /// <param name="cancellationToken">A cancellation token for the asynchronous operation.</param>
        /// <returns>A <see cref="ValueTask{TProjection}"/> containing the first occurrence of the projected document, or the default value of <typeparamref name="TProjection"/> if none are found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TProjection> GetOneAsync<TProjection>([NotNull] Query<TDocument, TProjection> query,
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

            // Create the filter definition from the query
            FilterDefinition<TDocument> filterSelected = query;


            // Create the cursor for the find operation
            IAsyncCursor<TProjection> cursor;

            if (InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the find operation with the session and filter
                cursor = await this.MongoCollection.FindAsync(clientSessionHandle, filterSelected, findOptions, cancellationToken);
            }
            else
            {
                cursor = await this.MongoCollection.FindAsync(filterSelected, findOptions, cancellationToken);
            }

            // Iterate over the cursor and retrieve the first occurrence
            while (await cursor.MoveNextAsync(cancellationToken))
            {
                foreach (var item in cursor.Current)
                {
                    // Set the result to the current item
                    result = item;
                    break;
                }

            }

            // Dispose the cursor
            cursor.Dispose();

            // Return the result
            return result;
        }
        #endregion


        /// <summary>
        /// Asynchronously retrieves a sequence of documents of type TDocument identified by the provided array of TObjectId.
        /// </summary>
        /// <param name="ids">An array of TObjectId representing the IDs of the documents to retrieve. Must not be null.</param>
        /// <param name="findOptions">Optional FindOptions to customize the query behavior. Can be null.</param>
        /// <param name="forceTransaction">Optional flag that indicates whether to force a transaction for the retrieval. Can be null.</param>
        /// <param name="cancellationToken">Optional CancellationToken to observe while waiting for the asynchronous operation to complete. Can be null.</param>
        /// <returns>
        /// An asynchronous enumerable of TDocument representing the retrieved documents.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> GetAsync([NotNull] TObjectId[] ids,
            [MaybeNull] FindOptions? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
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
        /// Asynchronously retrieves a collection of documents based on the specified IDs.
        /// </summary>
        /// <param name="ids">An array of document IDs to find.</param>
        /// <param name="findOptions">Optional parameters to customize the find operation.</param>
        /// <param name="forceTransaction">Optional boolean indicating whether to force the use of a transaction.</param>
        /// <param name="cancellationToken">A cancellation token to signal cancellation of the operation.</param>
        /// <returns>An asynchronous enumerable of documents that match the specified IDs.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> GetAsync([NotNull] TObjectId[] ids,
            [MaybeNull] FindOptions<TDocument>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Create a query to find documents with IDs in the provided array
            var qry = Query<TDocument, TDocument>.FromExpression(f => ids.Contains(f.Id));

            // Convert the findOptions to the correct type for the GetAsync method
            var opt = ConvertInternal(findOptions);

            // Iterate over the results of the GetAsync method and yield return each item
            await foreach (var item in this.GetAsync<TDocument>(qry, opt))
            {
                yield return item;
            }
        }


        /// <summary>
        /// Asynchronously retrieves a collection of documents of type <typeparamref name="TProjection"/>
        /// based on the specified identifiers. The method yields each document as it is retrieved.
        /// </summary>
        /// <typeparam name="TProjection">
        /// The type of the projected result.
        /// </typeparam>
        /// <param name="ids">
        /// An array of identifiers of type <typeparamref name="TObjectId"/> to find the corresponding documents.
        /// </param>
        /// <param name="findOptions">
        /// Optional. The options to use when finding documents. If not specified, default options will be applied.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional. A flag indicating whether to force the operation to run within a transaction. Defaults to null.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional. A cancellation token to cancel the asynchronous operation. Defaults to null.
        /// </param>
        /// <returns>
        /// An asynchronous enumerable of documents of type <typeparamref name="TProjection"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> GetAsync<TProjection>([NotNull] TObjectId[] ids,
            [MaybeNull] FindOptions<TDocument, TProjection>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Set default options if not provided
            findOptions ??= new FindOptions<TDocument, TProjection>();

            // Create a query to find documents with IDs in the provided array
            var qry = Query<TDocument, TProjection>.FromExpression(f => ids.Contains(f.Id));

            // Iterate over the results of the GetAsync method and yield each document
            await foreach (var item in this.GetAsync<TProjection>(qry, findOptions))
            {
                yield return item;
            }
        }



        /// <summary>
        /// Asynchronously retrieves a sequence of documents that match the specified filter.
        /// </summary>
        /// <param name="filter">The query used to filter the documents. Must not be null.</param>
        /// <param name="findOptions">Optional settings for the find operation. Can be null.</param>
        /// <param name="forceTransaction">A flag indicating whether to force a transaction. Can be null.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation. Can be null.</param>
        /// <returns>An asynchronous sequence of documents that match the filter.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> GetAsync([NotNull] Query<TDocument> filter,
            [MaybeNull] FindOptions? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
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
        /// Asynchronously retrieves documents based on the specified query filter and options.
        /// This method returns an asynchronous stream of documents of type <typeparamref name="TDocument"/>.
        /// </summary>
        /// <param name="filter">
        /// The query to be applied for filtering the documents.
        /// This must not be null.
        /// </param>
        /// <param name="findOptions">
        /// Options that govern the behavior of the find operation. 
        /// If not provided, a new instance with default values will be created.
        /// </param>
        /// <param name="forceTransaction">
        /// An optional flag indicating whether to force the operation to run within a transaction.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to observe while waiting for the asynchronous operation to complete.
        /// </param>
        /// <returns>
        /// An asynchronous enumerable of documents of type <typeparamref name="TDocument"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> GetAsync([NotNull] Query<TDocument> filter,
            [MaybeNull] FindOptions<TDocument>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
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
        /// Performs an asynchronous full-text search for documents based on the specified search text
        /// and optional parameters, yielding documents as they are found.
        /// </summary>
        /// <param name="text">The search text to be used for the full-text search.</param>
        /// <param name="fullTextSearchOptions">Options that specify the details of the full-text search.</param>
        /// <param name="filter">An optional query filter to apply to the search results.</param>
        /// <param name="findOptions">Optional configurations for finding documents, such as limit or sort.</param>
        /// <param name="forceTransaction">An optional boolean indicating whether to force the search within a transaction.</param>
        /// <param name="cancellationToken">An optional token for canceling the operation if needed.</param>
        /// <returns>An asynchronous enumerator that yields documents matching the full-text search.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> FulltextSearchAsync([NotNull] string text,
            [NotNull] TextSearchOptions? fullTextSearchOptions,
            [MaybeNull] Query<TDocument>? filter = default,
            [MaybeNull] FindOptions<TDocument>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Perform the full-text search and iterate over the results
            await foreach (var item in this.FulltextSearchAsync<TDocument>(text, fullTextSearchOptions, filter, findOptions, forceTransaction, cancellationToken))
            {
                // Yield return each item in the search results
                yield return item;
            }
        }


        /// <summary>
        /// Performs an asynchronous full-text search on a MongoDB collection and returns an 
        /// asynchronous enumerable of projected results based on the search criteria.
        /// </summary>
        /// <typeparam name="TProjection">
        /// The type of the projected result.
        /// </typeparam>
        /// <param name="text">
        /// The search text to be used for the full-text search.
        /// </param>
        /// <param name="fullTextSearchOptions">
        /// Options that determine the behavior of the full-text search.
        /// </param>
        /// <param name="filter">
        /// An optional additional query filter for more granular search results.
        /// </param>
        /// <param name="findOptions">
        /// Options for the find operation, such as sort and limit.
        /// </param>
        /// <param name="forceTransaction">
        /// An optional boolean indicating whether to force the find operation to execute in a transaction.
        /// </param>
        /// <param name="cancellationToken">
        /// A token for cancelling the operation if needed.
        /// </param>
        /// <returns>
        /// An asynchronous enumerable containing the search results of type <typeparamref name="TProjection"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> FulltextSearchAsync<TProjection>([NotNull] string text,
            [NotNull] TextSearchOptions fullTextSearchOptions,
            [MaybeNull] Query<TDocument, TProjection>? filter = default,
            [MaybeNull] FindOptions<TDocument, TProjection>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Set default options if not provided
            findOptions ??= new FindOptions<TDocument, TProjection>();

            // Create the filter definition from the text and full-text search options
            FilterDefinition<TDocument> filterSelected = Query<TDocument, TProjection>.FromText(text, fullTextSearchOptions);

            // If a filter is provided, add it to the filter definition
            if (filter != default)
            {
                filterSelected += filter;
            }


            // Create the cursor for the find operation
            IAsyncCursor<TProjection> cursor;

            // If the find operation should not be performed in a transaction
            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the find operation with the session and filter
                cursor = await this.MongoCollection.FindAsync(clientSessionHandle, filterSelected, findOptions, cancellationToken);
            }
            else
            {
                cursor = await this.MongoCollection.FindAsync(filterSelected, findOptions, cancellationToken);
            }

            // Iterate over the cursor and retrieve the search results
            while (await cursor.MoveNextAsync(cancellationToken))
            {
                foreach (var item in cursor.Current)
                {
                    // Yield return each item in the search results, excluding null items
                    if (item != null)
                    {
                        yield return item;
                    }
                }
            }

            // Dispose of the cursor
            cursor.Dispose();
        }



        /// <summary>
        /// Converts the current object into an IQueryable<TDocument> representation,
        /// with optional preprocessing, aggregate options, and transaction settings.
        /// </summary>
        /// <param name="preApprend">
        /// A function that takes an IQueryable<TDocument> and returns an IQueryable<TDocument> 
        /// after applying certain preprocessing. If null, returns the current IQueryable 
        /// without modifications.
        /// </param>
        /// <param name="aggregateOptions">
        /// Optional settings for aggregation. The default is null, which means no special 
        /// aggregation options are applied.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional flag indicating whether to force a transaction. If null, the default 
        /// behavior is applied.
        /// </param>
        /// <returns>
        /// An IQueryable<TDocument> representing the documents after applying the 
        /// preprocessing function, if provided. Otherwise, returns the current 
        /// IQueryable.
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


        /// <summary>
        /// Asynchronously retrieves a collection of projected documents from a MongoDB 
        /// collection based on the specified query filter and options.
        /// </summary>
        /// <typeparam name="TProjection">
        /// The type of the projected documents to be returned.
        /// </typeparam>
        /// <param name="filter">
        /// A query defining the filter for the documents to retrieve.
        /// This is a non-nullable parameter, and must be provided.
        /// </param>
        /// <param name="findOptions">
        /// Optional parameters for the find operation, which may be null.
        /// </param>
        /// <param name="forceTransaction">
        /// A flag indicating whether to force the operation to be performed in a transaction.
        /// This is nullable and defaults to null.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to cancel the asynchronous operation, if needed.
        /// This is nullable and defaults to null.
        /// </param>
        /// <returns>
        /// An asynchronous enumerable of projected documents that match the filter.
        /// Each item may be null, and is excluded from the result set.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> GetAsync<TProjection>([NotNull] Query<TDocument, TProjection> filter,
            [MaybeNull] FindOptions<TDocument, TProjection>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Set default options if not provided
            findOptions ??= new FindOptions<TDocument, TProjection>();

            // Create the filter definition from the query
            FilterDefinition<TDocument> filterSelected = filter;

            // Create the options for the find operation
            var options = findOptions;

            // Create the cursor for the find operation
            IAsyncCursor<TProjection> cursor;

            // If the find operation should not be performed in a transaction
            if (InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the find operation with the session and filter
                cursor = await this.MongoCollection.FindAsync(clientSessionHandle, filterSelected, options, cancellationToken);
            }
            else
            {
                cursor = await this.MongoCollection.FindAsync(filterSelected, options, cancellationToken);
            }

            // Iterate over the cursor and retrieve the search results
            while (await cursor.MoveNextAsync(cancellationToken))
            {
                foreach (var item in cursor.Current)
                {
                    // Yield return each item in the search results, excluding null items
                    if (item != null)
                    {
                        yield return item;
                    }
                }
            }
            // Dispose of the cursor
            cursor.Dispose();
        }


        /// <summary>
        /// Asynchronously retrieves a paged result of the specified type based on the provided filter and options.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projected result.</typeparam>
        /// <param name="filter">The query filter to apply when retrieving the paged results.</param>
        /// <param name="findOptions">Optional pagination and sorting options. Can be null.</param>
        /// <param name="forceTransaction">Optional parameter indicating whether to force the operation into a transaction. Can be null.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation. Can be null.</param>
        /// <returns>A task representing the asynchronous operation, containing a <see cref="PagedResult{TProjection}"/> with the results.</returns>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<PagedResult<TProjection>> GetPagedAsync<TProjection>([NotNull] Query<TDocument> filter,
            [MaybeNull] FindOptionsPaging<TDocument>? findOptions,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Convert the filter to the correct type for the GetPagedAsync method
            Query<TDocument, TProjection> qry = filter;

            // Convert the findOptions to the correct type for the GetPagedAsync method
            var opt = ConvertInternal<TProjection>(findOptions);

            // Call the GetPagedAsync method with the converted filter and findOptions
            return await this.GetPagedAsync(qry, opt, forceTransaction, cancellationToken);
        }


        /// <summary>
        /// Asynchronously retrieves a paginated result set of documents based on the specified filter and find options.
        /// The method validates the input pagination parameters and performs a MongoDB 
        /// find operation while considering whether a transaction should be used.
        /// </summary>
        /// <typeparam name="TProjection">The type to project the documents into.</typeparam>
        /// <param name="filter">The query defining the filter to be applied to the documents.</param>
        /// <param name="findOptions">Options that specify pagination settings.</param>
        /// <param name="forceTransaction">An optional flag indicating if a transaction should be enforced.</param>
        /// <param name="cancellationToken">A token to signal cancellation of the operation.</param>
        /// <returns>
        /// A <see cref="PagedResult{TProjection}"/> containing the projected documents, the current page number, 
        /// the page size, and the total count of documents matching the filter.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the page size or current page is invalid.</exception>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<PagedResult<TProjection>> GetPagedAsync<TProjection>([NotNull] Query<TDocument, TProjection> filter,
            [MaybeNull] FindOptionsPaging<TDocument, TProjection>? findOptions,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Convert the filter and findOptions to strings for logging purposes
            var fstr = filter.ToString();
            var fostr = findOptions.ToString();
            var fojson = findOptions.JsonString();


            // Check if the page size is valid
            if (findOptions.PageSize <= 0)
            {
                throw new ArgumentException("Page size is invalid or null.");
            }

            // Check if the current page is valid
            if (findOptions.CurrentPage < 0)
            {
                throw new ArgumentException("Current page is invalid or null.");
            }




            #region find
            // Create the filter definition from the query
            FilterDefinition<TDocument> filterSelected = filter;

            // Create the options for the find operation
            MongoDB.Driver.FindOptions<TDocument, TProjection> options = findOptions;

            // Create the count options from the find options and set the limit and skip options to null
            var countOptions = new CountOptions
            {
                Limit = null,
                Skip = null,
                Collation = options.Collation,
                Comment = options.Comment,
                Hint = options.Hint,
                MaxTime = options.MaxTime
            };

            // Asynchronously count the number of documents that match the specified filter
            var total = System.Convert.ToInt32(await this.CountDocumentsAsync((FilterDefinition<TDocument>)filter, countOptions));

            IAsyncCursor<TProjection> cursor;

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                cursor = await this.MongoCollection.FindAsync(clientSessionHandle, filterSelected, options, cancellationToken);
            }
            else
            {
                cursor = await this.MongoCollection.FindAsync(filterSelected, options, cancellationToken);
            }

            // Create an array to hold the items
            var itens = new TProjection[(options != null && options.Limit.HasValue && options.Limit.Value < total) ? options.Limit.Value : total];

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

            // Perform the find operation and return the result
            return new PagedResult<TProjection>(itens, System.Convert.ToInt32(findOptions.CurrentPage), System.Convert.ToInt32(findOptions.PageSize), total);
        }

        #endregion

        #region FindOneAndUpdateAsync


        /// <summary>
        /// Asynchronously finds a single document and updates it in the database.
        /// </summary>
        /// <param name="query">The query used to filter the document to find and update.</param>
        /// <param name="options">Options that specify how the operation behaves.</param>
        /// <param name="forceTransaction">A value indicating whether the operation should be forced to run in a transaction.</param>
        /// <param name="cancellationToken">A token for canceling the operation.</param>
        /// <returns>A <see cref="ValueTask{TDocument}"/> representing the asynchronous operation, containing the updated document.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TDocument> FindOneAndUpdateAsync([NotNull] Query<TDocument> query,
            [MaybeNull] FindOneAndUpdateOptions<TDocument> options,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create a copy of the options to avoid modifying the original
            var findOneAndUpdateOptions = options;

            // Declare a variable to hold the result
            TDocument result;

            // Get the filter and update definitions from the query
            FilterDefinition<TDocument> filter = query;
            UpdateDefinition<TDocument> update = _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(query.Update);

            // If the operation should not be performed in a transaction
            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the find and update operation with a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(clientSessionHandle, filter, update, findOneAndUpdateOptions, cancellationToken);
            }
            // If no transaction is in use
            else
            {
                // Perform the find and update operation without a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, findOneAndUpdateOptions, cancellationToken);
            }

            // Return the updated document
            return result;
        }

        /*
        /// <summary>
        /// Asynchronously finds a single document and updates it in the MongoDB collection.
        /// </summary>
        /// <param name="filter">The filter used to select the document to update.</param>
        /// <param name="update">The update operations to apply to the document.</param>
        /// <param name="options">The options for the find and update operation.</param>
        /// <param name="forceTransaction">Specifies whether to force the operation within a transaction.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated document.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TDocument> FindOneAndUpdateAsync([NotNull] string filter, [NotNull] string update,
            [NotNull] FindOneAndUpdateOptions<TDocument> options,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create a copy of the options to avoid modifying the original
            var fouOptions = options;

            // Declare a variable to hold the result
            TDocument result;

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the find and update operation with a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(clientSessionHandle, filter, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(update), fouOptions, cancellationToken);
            }
            // If no transaction is in use
            else
            {
                // Perform the find and update operation without a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(update), fouOptions, cancellationToken);
            }

            // Return the updated document
            return result;
        }


        /// <summary>
        /// Asynchronously finds a document in the database that matches the specified filter 
        /// and updates it with the provided update definition. The operation can be performed 
        /// within a transaction if indicated.
        /// </summary>
        /// <param name="filter">The filter string used to locate the document in the database.</param>
        /// <param name="update">The update definition that specifies the modifications to apply.</param>
        /// <param name="options">Optional settings for the find-and-update operation.</param>
        /// <param name="forceTransaction">Optional flag indicating whether to enforce a transaction.</param>
        /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
        /// <returns>A <see cref="ValueTask{TDocument}"/> representing the asynchronous operation, 
        /// with the updated document if found; otherwise, the default value of <typeparamref name="TDocument"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TDocument> FindOneAndUpdateAsync(
           [NotNull] string filter,
           [NotNull] PipelineUpdateDefinition<TDocument> update,
           [MaybeNull] FindOneAndUpdateOptions<TDocument>? options = default,
           [MaybeNull] bool? forceTransaction = default,
           [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create a copy of the options to avoid modifying the original
            var fouOptions = options ?? new FindOneAndUpdateOptions<TDocument>();

            // Declare a variable to hold the result
            TDocument result;

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the find and update operation with a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(clientSessionHandle, filter, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(update), fouOptions, cancellationToken);
            }
            // If no transaction is in use
            else
            {
                // Perform the find and update operation without a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(update), fouOptions, cancellationToken);
            }

            // Return the result
            return result;
        }
        */
        #endregion FindOneAndUpdateAsync

        #region UpdateManyAsync

        /// <summary>
        /// Asynchronously updates multiple documents in the MongoDB collection that match the specified query.
        /// </summary>
        /// <param name="query">The query containing the criteria for selecting the documents to update.</param>
        /// <param name="options">Options that define the behavior of the update operation.</param>
        /// <param name="forceTransaction">Optional parameter to force the update operation to execute within a transaction.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A <see cref="ValueTask{long}"/> representing the asynchronous operation, 
        /// containing the number of modified documents, or -1 if the operation was not acknowledged.</returns>
        /// <remarks>
        /// This method efficiently updates multiple documents while considering if it should 
        /// run within a transaction based on the given <paramref name="forceTransaction"/> flag.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> UpdateManyAsync([NotNull] Query<TDocument> query,
                    [NotNull] UpdateOptions options,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Initialize the result to null
            UpdateResult result;

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the update operation without a session
                result = await this.MongoCollection.UpdateManyAsync(clientSessionHandle, query, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(query.Update), options, cancellationToken);
            }
            else
            {
                result = await this.MongoCollection.UpdateManyAsync(query, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(query.Update), options, cancellationToken);
            }

            // Return the number of modified documents, or -1 if the operation was not acknowledged
            return result.IsAcknowledged ? result.ModifiedCount : -1;
        }

        /// <summary>
        /// Updates multiple documents in the MongoDB collection based on the specified filter and update definitions.
        /// This method can optionally perform the operation within a transaction if configured.
        /// </summary>
        /// <param name="filter">
        /// A string that defines the filter to select the documents to be updated. It should be a valid 
        /// MongoDB query filter.
        /// </param>
        /// <param name="update">
        /// A string that represents the update operations to be applied to the selected documents. It should 
        /// adhere to the MongoDB update syntax.
        /// </param>
        /// <param name="options">
        /// An instance of <see cref="UpdateOptions"/> that specifies options for the update operation, 
        /// such as whether to upsert documents.
        /// </param>
        /// <param name="forceTransaction">
        /// An optional boolean that if set to true forces the operation to be conducted within a transaction. 
        /// Default is null which allows the use of normal operations if no transaction is active.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to cancel the operation. Default is null.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{long}"/> that represents the asynchronous operation, containing the number 
        /// of documents modified by the update operation. Returns -1 if the update operation was not acknowledged.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation is cancelled via the cancellation token.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> UpdateManyAsync([NotNull] string filter, [NotNull] string update,
                    [NotNull] UpdateOptions options,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Initialize the result to null
            UpdateResult result;

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the update operation with a session
                result = await this.MongoCollection.UpdateManyAsync(clientSessionHandle, filter, update, options, cancellationToken);
            }
            // If no transaction is in use
            else
            {
                // Perform the update operation without a session
                result = await this.MongoCollection.UpdateManyAsync(filter, update, options, cancellationToken);
            }

            // Return the number of modified documents, or -1 if the operation was not acknowledged
            return result.IsAcknowledged ? result.ModifiedCount : -1;
        }

        #endregion UpdateManyAsync

        #region Count

        /// <summary>
        /// Asynchronously counts the number of documents that match the specified query.
        /// </summary>
        /// <param name="query">The query to filter the documents that will be counted. Must not be null.</param>
        /// <param name="countOptions">The options that specify how the count operation should be performed. Defaults to null, in which case default options will be used.</param>
        /// <param name="forceTransaction">Indicates whether the count operation should be forced to run in a transaction. Defaults to null.</param>
        /// <param name="cancellationToken">A token for canceling the operation if needed. Defaults to null.</param>
        /// <returns>
        /// A <see cref="ValueTask{long}"/> representing the asynchronous operation that returns the count of documents matching the query.
        /// </returns>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> CountDocumentsAsync([NotNull] Query<TDocument> query, CountOptions? countOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            // If countOptions is null, create a new instance with default values
            countOptions ??= new CountOptions();

            // Convert the query to a FilterDefinition and call the CountDocumentsAsync method with it
            return await this.CountDocumentsAsync((FilterDefinition<TDocument>)query, countOptions, forceTransaction, cancellationToken);
        }

        /// <summary>
        /// Asynchronously counts the number of documents in a MongoDB collection based on a provided query function.
        /// </summary>
        /// <param name="preApprend">
        /// A function that takes an <see cref="IQueryable{TDocument}"/> and returns an <see cref="IQueryable{TDocument}"/> 
        /// for applying additional query logic before counting the documents.
        /// </param>
        /// <param name="aggregateOptions">
        /// Optional aggregate options to apply to the query. Default is null.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional value indicating whether to force the operation within a transaction. Default is null.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional token to observe while waiting for the task to complete. Default is null.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task's result contains the number of documents counted.
        /// </returns>
        /// <remarks>
        /// This method uses the specified <paramref name="preApprend"/> function to modify the queryable before 
        /// executing the count operation. In addition, it manages transactions and applies aggregate options 
        /// as needed while ensuring thread safety and optimal performance by employing aggressive inlining.
        /// </remarks>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> CountDocumentsAsync([NotNull] Func<IQueryable<TDocument>, IQueryable<TDocument>> preApprend, AggregateOptions? aggregateOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            IQueryable<TDocument> query;

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
                // Create a queryable from the MongoCollection using the session and default aggregate options
                query = this.MongoCollection.AsQueryable(clientSessionHandle, new AggregateOptions());
            else
                query = this.MongoCollection.AsQueryable(new AggregateOptions());

            // Invoke the preApprend function on the queryable
            var queryAppended = preApprend.Invoke(query);

            // Count the number of documents in the queryable
            var count = queryAppended.LongCount();

            // Await a completed task to allow the method to be awaited
            await Task.CompletedTask;

            // Return the count of documents
            return count;
        }

        /// <summary>
        /// Asynchronously counts the number of documents in a collection that match the specified filter.
        /// </summary>
        /// <param name="filterDefinition">The filter definition to apply to the documents being counted.</param>
        /// <param name="countOptions">Optional. Specifies options for the count operation.</param>
        /// <param name="forceTransaction">Optional. Indicates whether to force the operation to be performed in a transaction.</param>
        /// <param name="cancellationToken">Optional. A cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="ValueTask{long}"/> representing the asynchronous operation, with the count of matching documents.</returns>
        /// <remarks>
        /// If <paramref name="countOptions"/> is not provided, a new instance with default values is created.
        /// If the operation is within a transaction based on <paramref name="forceTransaction"/>, it counts the documents using the appropriate session.
        /// Otherwise, it counts the documents without a session.
        /// </remarks>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<long> CountDocumentsAsync([NotNull] FilterDefinition<TDocument> filterDefinition, CountOptions? countOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            // If countOptions is null, create a new instance with default values
            countOptions ??= new CountOptions();

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the count operation without a session
                return await this.MongoCollection.CountDocumentsAsync(clientSessionHandle, filterDefinition, countOptions, cancellationToken);
            }
            else
            {
                return await this.MongoCollection.CountDocumentsAsync(filterDefinition, countOptions, cancellationToken);
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// Asynchronously updates the documents that match the specified query by adding items to the set 
        /// specified in the update operation. This method supports optional transaction management and cancellation.
        /// </summary>
        /// <param name="query">The query that determines which documents to update.</param>
        /// <param name="updateOptions">Options that modify how the update is applied. If null, a new instance with default values is created.</param>
        /// <param name="forceTransaction">Indicates whether to force the update to execute within a transaction, if applicable.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation if needed.</param>
        /// <returns>A task that represents the asynchronous operation, containing the number of documents that were updated.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the query is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> UpdateAddToSetAsync([NotNull] Query<TDocument> query,
                    UpdateOptions? updateOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // If updateOptions is null, create a new instance with default values
            updateOptions ??= new UpdateOptions();


            // Call the UpdateAsync method with the query, update, and options
            return await this.UpdateAsync(query, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(query.Update), updateOptions, forceTransaction, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates a single document in a MongoDB collection based on the provided filter and update definitions.
        /// The method supports optional transaction handling, allowing updates to be performed within a transaction context if desired.
        /// </summary>
        /// <param name="filterDefinition">
        /// A <see cref="FilterDefinition{TDocument}"/> that specifies the criteria for selecting the document to update.
        /// This parameter cannot be null.
        /// </param>
        /// <param name="updateDefinition">
        /// An <see cref="UpdateDefinition{TDocument}"/> that defines the modifications to apply to the selected document.
        /// This parameter cannot be null.
        /// </param>
        /// <param name="updateOptions">
        /// An <see cref="UpdateOptions"/> object that contains options for the update operation.
        /// This parameter cannot be null.
        /// </param>
        /// <param name="forceTransaction">
        /// An optional boolean that, when specified, indicates whether to force the operation to be executed in a transaction.
        /// If null, the method will determine the transaction context based on the current state.
        /// </param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.
        /// If null, the default token will be used, allowing the operation to run until completion.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{T}"/> representing the asynchronous operation. 
        /// The result is the number of documents modified by the update operation, or -1 if the operation was not acknowledged.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<long> UpdateAsync(
                    [NotNull] FilterDefinition<TDocument> filterDefinition,
                    [NotNull] UpdateDefinition<TDocument> updateDefinition,
                    [NotNull] UpdateOptions updateOptions,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Initialize the result to null
            UpdateResult result;


            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the update operation with a session
                result = await this.MongoCollection.UpdateOneAsync(clientSessionHandle, filterDefinition, updateDefinition, updateOptions, cancellationToken);
            }
            // If no transaction is in use
            else
            {
                // Perform the update operation without a session
                result = await this.MongoCollection.UpdateOneAsync(filterDefinition, updateDefinition, updateOptions, cancellationToken);
            }

            // Return the number of modified documents, or -1 if the operation was not acknowledged
            return result == default ? -1 : result.IsAcknowledged ? result.ModifiedCount : -1;
        }


        #endregion

        #region Insert

        /// <summary>
        /// Asynchronously inserts a document into the collection with optional insert options and transaction handling.
        /// </summary>
        /// <param name="source">
        /// The document to insert. This parameter cannot be null.
        /// </param>
        /// <param name="insertOneOptions">
        /// Optional insert options. If not provided, a default options object will be created.
        /// </param>
        /// <param name="forceTransaction">
        /// A nullable boolean indicating whether to force the operation within a transaction context. 
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token to observe while waiting for the asynchronous operation to complete.
        /// </param>
        /// <returns>
        /// A value task representing the asynchronous operation, containing the number of documents inserted (should be 1).
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> InsertAsync([NotNull] TDocument source,
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

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the update operation with a session
                await this.MongoCollection.InsertOneAsync(clientSessionHandle, _contextbase.BeforeInsertInternal<TDocument, TObjectId>(source), insertOneOptions, cancellationToken);
            }
            // If no transaction is in use
            else
            {
                // Perform the update operation without a session
                await this.MongoCollection.InsertOneAsync(_contextbase.BeforeInsertInternal<TDocument, TObjectId>(source), insertOneOptions, cancellationToken);
            }

            // Return the number of inserted documents, which should always be 1
            return source.Id.Equals(default) || source.Id.Equals(null) ? 0 : 1;
        }

        /// <summary>
        /// Asynchronously inserts a collection of documents into a data store using bulk write operations.
        /// </summary>
        /// <param name="docs">An <see cref="IEnumerable{T}"/> containing the documents to be inserted.</param>
        /// <param name="bulkWriteOptions">Optional <see cref="BulkWriteOptions"/> for configuring the write operation.</param>
        /// <param name="forceTransaction">Optional boolean indicating whether to force the operation to run within a transaction.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="ValueTask{long}"/> representing the asynchronous operation, containing the number of documents inserted.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> InsertAsync([NotNull] IEnumerable<TDocument> docs,
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
                writeModels.Add(new InsertOneModel<TDocument>(_contextbase.BeforeInsertInternal<TDocument, TObjectId>(doc)));
            }

            // Perform the insert operation using the write models and options
            return await this.BulkWriteAsync(writeModels, bulkWriteOptions, forceTransaction, cancellationToken);
        }

        /// <summary>
        /// Inserts a collection of documents asynchronously into the database.
        /// </summary>
        /// <param name="docs">The documents to insert. This is an enumerable collection of documents of type TDocument.</param>
        /// <param name="insertManyOptions">Options for the insert operation. This can include settings such as whether to bypass document validation and whether the inserts should be ordered.</param>
        /// <param name="forceTransaction">A boolean value indicating whether to force the insert operation to execute within a transaction. This can be null.</param>
        /// <param name="cancellationToken">A token to cancel the operation if needed. This can be null.</param>
        /// <returns>A <see cref="ValueTask{long}"/> representing the asynchronous operation, with a long value indicating the number of documents inserted.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the cancellation token.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> InsertAsync([NotNull] IEnumerable<TDocument> docs,
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
        /// Asynchronously replaces documents in a collection based on the provided 
        /// enumerable of documents and an optional query. This method uses a bulk 
        /// write operation for efficiency. 
        /// </summary>
        /// <param name="docs">
        /// The collection of documents to replace in the database. This parameter 
        /// must not be null. Each document in the collection must contain an Id 
        /// property that correlates to the document's identifier in the database.
        /// </param>
        /// <param name="query">
        /// An optional query that specifies the criteria for the replacement. 
        /// If not provided, a default expression that matches the Ids will be used.
        /// </param>
        /// <param name="bulkWriteOptions">
        /// Optional configuration options for the bulk write operation. If null, 
        /// default options will be applied.
        /// </param>
        /// <param name="forceTransaction">
        /// An optional flag that indicates whether to force the operation to run 
        /// within a transaction. The default is null.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the operation. The default 
        /// value is null, which allows for cancellation requests to be passed.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing the total 
        /// number of documents replaced as a long value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> ReplaceAsync([NotNull] IEnumerable<TDocument> docs,
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
                FilterDefinition<TDocument> filterDefinition = (query ?? exp).CompleteExpression(_contextbase.BeforeReplaceInternal<TDocument, TObjectId>(doc));

                var model = new ReplaceOneModel<TDocument>(filterDefinition, doc);

                updates.Add(model);
            }


            // Perform the bulk write operation with the update models and options
            return await this.BulkWriteAsync(updates, bulkWriteOptions, forceTransaction, cancellationToken);
        }


        /// <summary>
        /// Asynchronously replaces documents in a data store based on the provided collection of documents and optional query parameters.
        /// </summary>
        /// <param name="docs">An <see cref="IEnumerable{TDocument}"/> containing the documents to replace.</param>
        /// <param name="query">An optional <see cref="Query{TDocument}"/> used to specify which documents to replace.</param>
        /// <param name="replaceOptions">Optional <see cref="ReplaceOptions"/> for customization of the replace operation.</param>
        /// <param name="forceTransaction">Optional boolean flag to indicate whether to force a transaction.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to propagate notifications that the operation should be canceled.</param>
        /// <returns>
        /// A <see cref="ValueTask{long}"/> representing the asynchronous operation, with a long value indicating the number of documents replaced.
        /// </returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> ReplaceAsync([NotNull] IEnumerable<TDocument> docs,
                    [MaybeNull] Query<TDocument>? query = null,
                    [MaybeNull] ReplaceOptions? replaceOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            replaceOptions ??= new ReplaceOptions();

            BulkWriteOptions bulkWriteOptions = new BulkWriteOptions()
            {
                IsOrdered = true,
                BypassDocumentValidation = replaceOptions.BypassDocumentValidation,
                Comment = replaceOptions.Comment,
                Let = replaceOptions.Let
            };

            return await this.ReplaceAsync(docs, query, bulkWriteOptions, forceTransaction, cancellationToken);
        }



        /// <summary>
        /// Asynchronously replaces a document in the collection based on the specified query.
        /// </summary>
        /// <param name="doc">
        /// The document to be replaced in the collection.
        /// </param>
        /// <param name="query">
        /// The query that specifies the document to replace. Can be null.
        /// </param>
        /// <param name="replaceOptions">
        /// Options that specify how the replace operation will be conducted. Can be null.
        /// </param>
        /// <param name="forceTransaction">
        /// Indicates whether to force the operation to run within a transaction. Can be null.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that allows the operation to be cancelled. Can be null.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous replace operation. 
        /// The task result contains the number of documents modified or -1 if the operation was not acknowledged.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> ReplaceAsync([NotNull] TDocument doc,
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
        /// Asynchronously replaces a document in the MongoDB collection with the provided document.
        /// </summary>
        /// <param name="doc">The document to replace with.</param>
        /// <param name="query">The query used to find the document to replace; if null, defaults to matching by document ID.</param>
        /// <param name="replaceOptions">Options for the replace operation; if null, default options will be used.</param>
        /// <param name="forceTransaction">If set to true, forces the use of a transaction for the operation.</param>
        /// <param name="cancellationToken">A cancellation token to signal when the operation should be canceled.</param>
        /// <returns>A <see cref="ValueTask{ReplaceOneResult}"/> representing the asynchronous operation, with the result containing information about the replace operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<ReplaceOneResult> ReplaceOneAsync([NotNull] TDocument doc,
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

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the replace operation with a session
                result = await this.MongoCollection.ReplaceOneAsync(clientSessionHandle, filterDefinition, _contextbase.BeforeInsertInternal<TDocument, TObjectId>(doc), replaceOptions, cancellationToken);
            }
            // If no transaction is in use
            else
            {
                // Perform the replace operation without a session
                result = await this.MongoCollection.ReplaceOneAsync(filterDefinition, _contextbase.BeforeInsertInternal<TDocument, TObjectId>(doc), replaceOptions, cancellationToken);
            }


            // Return the number of replaced documents, or -1 if the operation was not acknowledged
            return result;
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes a document asynchronously identified by the specified ID.
        /// </summary>
        /// <param name="id">The ID of the document to delete.</param>
        /// <param name="deleteOptions">Optional. Options to modify the delete operation.</param>
        /// <param name="forceTransaction">Optional. Indicates whether to force the operation within a transaction.</param>
        /// <param name="cancellationToken">Optional. A cancellation token for canceling the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous delete operation. The value of the task contains the number of documents deleted.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the cancellation token is triggered.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the ID parameter is null.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> DeleteOneAsync([NotNull] TObjectId id,
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
        /// Asynchronously deletes documents identified by the specified IDs from a data source.
        /// This method creates a list of delete write models for each ID, executes a bulk write operation,
        /// and returns the total number of documents deleted.
        /// </summary>
        /// <param name="ids">The collection of identifiers for the documents to be deleted.</param>
        /// <param name="bulkWriteOptions">Options that control the bulk write operation. If null, 
        /// defaults will be used.</param>
        /// <param name="forceTransaction">Indicates whether the operation should be executed in a 
        /// transaction, if applicable.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation, with a 
        /// result of type <see cref="long"/> which indicates the total number of documents deleted.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> DeleteAsync([NotNull] IEnumerable<TObjectId> ids,
                    [MaybeNull] BulkWriteOptions? bulkWriteOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set default delete options if not provided
            bulkWriteOptions ??= new BulkWriteOptions();

            // Create a list to hold the write models for the delete operations
            var listWriteModel = new List<WriteModel<TDocument>>();

            // Iterate over the IDs and create delete write models for each ID
            foreach (var id in ids)
            {
                // Create a filter definition to match the document by its ID
                Expression<Func<TDocument, bool>> exp = (f) => f.Id.Equals(id);

                FilterDefinition<TDocument> filterDefinition = exp;

                // Create a delete write model with the filter definition and delete options
                var model = new DeleteOneModel<TDocument>(filterDefinition);

                // Add the write model to the list
                listWriteModel.Add(model);
            }


            // Perform the bulk write operation with the write models and options
            return await this.BulkWriteAsync(listWriteModel, bulkWriteOptions, forceTransaction, cancellationToken);
        }


        /// <summary>
        /// Asynchronously deletes a collection of objects identified by their IDs.
        /// </summary>
        /// <param name="ids">The collection of object IDs to delete.</param>
        /// <param name="deleteOptions">Optional parameters for deletion such as comments or additional configurations.</param>
        /// <param name="forceTransaction">Optional flag indicating whether to force the use of a transaction during deletion.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the asynchronous operation to complete.</param>
        /// <returns>A <see cref="ValueTask{long}"/> representing the asynchronous operation. The result is the number of deleted objects.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> DeleteAsync([NotNull] IEnumerable<TObjectId> ids,
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

            // Perform the bulk write operation with the write models and options
            return await this.DeleteAsync(ids, bulkWriteOptions, forceTransaction, cancellationToken);
        }


        /// <summary>
        /// Asynchronously deletes documents that match the specified query.
        /// </summary>
        /// <param name="query">
        /// The query specifying the documents to delete. Must not be null.
        /// </param>
        /// <param name="deleteOptions">
        /// Optional. The options specifying how the delete operation should be performed.
        /// If not provided, defaults to a new instance of <see cref="DeleteOptions"/>.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional. A flag indicating whether to force the operation to run within a transaction.
        /// If not provided, defaults to null.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional. A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
        /// If not provided, defaults to null.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{long}"/> representing the asynchronous operation,
        /// with a long value representing the number of documents deleted.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> DeleteAsync([NotNull] Query<TDocument> query,
                    [MaybeNull] DeleteOptions? deleteOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            deleteOptions ??= new DeleteOptions();

            var listWriteModel = new List<WriteModel<TDocument>> { new DeleteManyModel<TDocument>(query) };

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
        /// Asynchronously aggregates documents based on the provided query and options,
        /// yielding the results as an asynchronous enumerable.
        /// </summary>
        /// <param name="query">The query used to filter the documents to be aggregated.</param>
        /// <param name="aggregateOptions">
        /// Optional parameters for aggregation, which may include specific settings such as
        /// groupings, projections, and other configuration options. If not provided, 
        /// default options will be used.
        /// </param>
        /// <param name="forceTransaction">
        /// Optional boolean to indicate if the operation should enforce a transaction.
        /// Defaults to null, allowing the implementation to decide based on context.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token to observe while waiting for the asynchronous operation to 
        /// complete. This can be used to cancel the operation if needed.
        /// </param>
        /// <returns>
        /// An asynchronous enumerable of aggregated documents of type <typeparamref name="TDocument"/>.
        /// This allows for efficient iteration over the results without loading all of them
        /// into memory at once.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> AggregateFacetEnumerableAsync([NotNull] Query<TDocument> query,
                    [MaybeNull] AggregateOptions? aggregateOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Set default aggregate options if not provided
            aggregateOptions ??= new AggregateOptions();

            // Perform the aggregation operation and iterate over the results
            foreach (var item in (await this.AggregateFacetAsync(query, DbSet<TDocument, TObjectId>.ConvertInternal(aggregateOptions), forceTransaction, cancellationToken)).Results)
            {
                // Yield return each item in the aggregation results
                yield return item;
            }
        }


        /// <summary>
        /// Asynchronously performs an aggregation operation with paging options on a specified query
        /// and returns the results as a paged result.
        /// </summary>
        /// <param name="query">The query to be executed for the aggregation.</param>
        /// <param name="aggregateOptions">The options for aggregating and paging results. Default is null.</param>
        /// <param name="forceTransaction">A boolean indicating whether to force a transaction. Default is null.</param>
        /// <param name="cancellationToken">A cancellation token to monitor for cancellation requests. Default is null.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="PagedResult{TDocument}"/> 
        /// object of the aggregation results if successful; otherwise, null.</returns>
        /// <exception cref="Exception">Thrown when the skip or limit options are invalid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<PagedResult<TDocument>> AggregateFacetAsync([NotNull] Query<TDocument> query,
                    [MaybeNull] AggregateOptionsPaging? aggregateOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            // Set default aggregate options if not provided
            aggregateOptions ??= new AggregateOptionsPaging();

            // Check if the skip and limit options are valid
            if (!aggregateOptions.Skip.HasValue || aggregateOptions.Skip.Value < 0)
            {
                throw new Exception("Skip is invalid or null.");
            }

            if (!aggregateOptions.Limit.HasValue || aggregateOptions.Limit.Value <= 0)
            {
                throw new Exception("Limit is invalid or null.");
            }

            // Convert the query to a BsonDocument array
            BsonDocument[] bsonDocumentFilter = _contextbase.BeforeAggregateInternal<TDocument, TObjectId, TDocument>(query);

            // Create a list to hold the paging filters
            var bsonDocumentFilterPaging = ((BsonDocument[])query).ToList();

            // Add skip and limit filters to the paging filters
            bsonDocumentFilterPaging.Add(new BsonDocument(new BsonElement("$skip", BsonValue.Create(aggregateOptions.Skip.Value))));
            bsonDocumentFilterPaging.Add(new BsonDocument(new BsonElement("$limit", BsonValue.Create(aggregateOptions.Limit.Value))));

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

            // Create a cursor for the aggregation operation
            var cursor = await this.MongoCollection.AggregateAsync<FacedAggregate<TDocument>>(bson, aggregateOptions, cancellationToken);

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

            // Return the result of the aggregation operation, the skip value, the limit value, and the total number of rows
            if (item != default)
            {
                return new PagedResult<TDocument>(item.Result.ToArray(), aggregateOptions.CurrentPage, aggregateOptions.PageSize, item.TotalRows());
            }

            // If there is no result, return the default value
            return default;
        }


        /// <summary>
        /// Asynchronously aggregates a collection of documents according to the specified query and options, 
        /// returning the results as a read-only list of projections.
        /// </summary>
        /// <typeparam name="TProjection">
        /// The type of the projection to which the documents will be transformed.
        /// </typeparam>
        /// <param name="query">
        /// The query that defines the criteria for aggregating documents.
        /// Must not be null.
        /// </param>
        /// <param name="aggregateOptions">
        /// Options that specify how the aggregation should be performed.
        /// Can be null.
        /// </param>
        /// <param name="forceTransaction">
        /// A flag indicating whether to force a transaction for the aggregation operation.
        /// Can be null.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the operation.
        /// Can be null.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation, 
        /// containing a read-only list of the aggregated projections.
        /// The list may contain null elements.
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
        /// Asynchronously aggregates a sequence of documents using the specified query and options, returning the results as an asynchronous enumerable.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projected results.</typeparam>
        /// <param name="query">The query containing the criteria for the aggregation.</param>
        /// <param name="aggregateOptions">Optional settings for the aggregation operation. Defaults to null.</param>
        /// <param name="forceTransaction">Optional flag to force a transaction. Defaults to null.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation. Defaults to null.</param>
        /// <returns>An asynchronous enumerable of projected results, allowing iteration over the aggregated results.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection?> AggregateEnumerableAsync<TProjection>([NotNull] Query<TDocument, TProjection> query,
                    [MaybeNull] AggregateOptions? aggregateOptions = default,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set default aggregate options if not provided
            aggregateOptions ??= new AggregateOptions();


            // Convert the query to a BsonDocument array
            BsonDocument[] bsonDocumentFilter = _contextbase.BeforeAggregateInternal<TDocument, TObjectId, TProjection>(query);


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

            if (InTransaction(forceTransaction, out var clientSessionHandle))
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
                foreach (var c in cursor.Current)
                {
                    yield return c;
                }
            }

            // Dispose the cursor
            cursor.Dispose();
        }


        #endregion



        /// <summary>
        /// Asynchronously performs a bulk write operation using the provided list of write models and options.
        /// The operation can optionally be executed within a transaction if specified.
        /// </summary>
        /// <param name="writeModel">
        /// A list of write models that define the operations to be performed in the bulk write.
        /// Each model can represent an insert, update, or delete operation.
        /// </param>
        /// <param name="bulkWriteOptions">
        /// Options that specify how the bulk write operation should be executed, such as whether to bypass document validation.
        /// </param>
        /// <param name="forceTransaction">
        /// An optional boolean that indicates whether the operation should be executed within a transaction.
        /// If null, the default transaction behavior will be applied.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the asynchronous operation if needed.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{long}"/> that represents the asynchronous bulk write operation.
        /// The result is the total count of documents that were inserted, updated, matched, or deleted,
        /// or -1 if the operation was not acknowledged.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<long> BulkWriteAsync([NotNull] List<WriteModel<TDocument>> writeModel,
                    [NotNull] BulkWriteOptions bulkWriteOptions,
                    [MaybeNull] bool? forceTransaction = default,
                    [MaybeNull] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Perform the bulk write operation based on the provided options
            BulkWriteResult result;

            if (this.InTransaction(forceTransaction, out var clientSessionHandle))
            {
                // Perform the bulk write operation with a session
                result = await this.MongoCollection.BulkWriteAsync(clientSessionHandle, writeModel, bulkWriteOptions, cancellationToken);
            }
            else
            {
                // Perform the bulk write operation without a session
                result = await this.MongoCollection.BulkWriteAsync(writeModel, bulkWriteOptions, cancellationToken);
            }

            // Check if the result is default
            if (result == default)
            {
                // Return -1 if the operation was not acknowledged
                return -1;
            }

            // Return the total count of documents inserted, updated, matched, or deleted if the operation was acknowledged
            if (result.IsAcknowledged)
            {
                return result.DeletedCount + result.ModifiedCount + result.MatchedCount + result.InsertedCount;
            }

            return -1;
        }

        /// <summary>
        /// Defines an explicit operator that converts a <see cref="DbSet{TDocument,TObjectId}"/> instance to a <see cref="ContextBase"/>.
        /// </summary>
        /// <param name="dbSet">The <see cref="DbSet{TDocument,TObjectId}"/> instance to convert.</param>
        /// <returns>A <see cref="ContextBase"/> that represents the provided <see cref="DbSet{TDocument,TObjectId}"/>.</returns>
        /// <remarks>
        /// This operator is marked with <see cref="MethodImplAttribute"/> to indicate that it should be aggressively inlined 
        /// by the compiler, optimizing performance by reducing function call overhead.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ContextBase(DbSet<TDocument, TObjectId> dbSet) => dbSet._contextbase;













        #region Dispose

        /// <summary>
        /// Releases the resources used by the current instance of the class.
        /// </summary>
        /// <remarks>
        /// This method calls the Dispose method with true to release both 
        /// managed and unmanaged resources and then suppresses finalization 
        /// to optimize garbage collection for this instance.
        /// </remarks>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously releases the resources used by the instance.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation of disposing resources.
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
        /// A Boolean value that indicates whether the method has been called directly
        /// or indirectly by a user's code (`true` if called by user code; `false` if called by the runtime).
        /// </param>
        /// <remarks>
        /// When called with disposing set to true, the method frees both managed and unmanaged resources.
        /// When called with disposing set to false, the method only frees unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._contextbase.ContextSession?.Dispose();
            }
            //(_asyncDisposableResource as IDisposable)?.Dispose();

            //_contextbase.Session = null;
            // _asyncDisposableResource = null;
        }


        /// <summary>
        /// Asynchronously disposes of the resources used by the class.
        /// </summary>
        /// <remarks>
        /// This method is intended to be overridden in derived classes to provide
        /// custom disposal logic for resources held by the class. It ensures
        /// proper cleanup of both asynchronous and synchronous disposable resources.
        /// </remarks>
        /// <returns>
        /// A ValueTask representing the asynchronous operation of disposing resources.
        /// </returns>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            //if (_asyncDisposableResource is not null)
            //{
            //    await _asyncDisposableResource.DisposeAsync().ConfigureAwait(false);
            //}

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (this._contextbase.ContextSession is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                this._contextbase.ContextSession?.Dispose();
            }

            //_asyncDisposableResource = null;
            //_disposableResource = null;
        }


        #endregion Dispose
    }




}
