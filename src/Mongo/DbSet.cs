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
using UCode.Extensions;
using UCode.Mongo.Options;
using UCode.Repositories;

namespace UCode.Mongo
{
    //https://mongodb.github.io/mongo-csharp-driver/2.11/reference/driver/crud/writing/
    public class DbSet<TDocument> : DbSet<TDocument, string>
        where TDocument : IObjectBase<string>, IObjectBaseTenant
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet([NotNull] ContextBase contextBase, string? collectionName = null,
            Action<CreateCollectionOptions>? createCollectionOptionsAction = null,
            Action<MongoCollectionSettings>? mongoCollectionSettingsAction = null,
            bool useTransaction = false,
            bool thowIndexExceptions = false) : base(contextBase, collectionName, createCollectionOptionsAction, mongoCollectionSettingsAction, thowIndexExceptions)
        {

        }

        public override string? ToString() => base.ToString();
    }


    //https://mongodb.github.io/mongo-csharp-driver/2.11/reference/driver/crud/writing/
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

        private static FindOptions<TDocument, TDocument> ConvertInternal(FindOptions<TDocument>? findOptions) => findOptions ?? new FindOptions<TDocument>();
        #endregion private methods

        #region constructor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet([NotNull] ContextBase contextBase, string? collectionName = null,
            Action<CreateCollectionOptions>? createCollectionOptionsAction = null,
            Action<MongoCollectionSettings>? mongoCollectionSettingsAction = null,
            bool useTransaction = false,
            bool thowIndexExceptions = false)
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

            _ = this.InternalIndex(false, thowIndexExceptions);
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
                
                yield return basonClassMap;
            }
        }





        #region index methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<KeyValuePair<string, List<string>>> GetIndexesAsync(bool? forceTransaction = default,
            CancellationToken cancellationToken = default)
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


        private bool InternalIndex(bool? forceTransaction = default, bool thowIndexExceptions = false)
        {
            var contextbaseType = this._contextbase.GetType();
            var methods = contextbaseType.GetMethods();

            var index_method = methods.SingleOrDefault(w => w.Name.Equals("Index", StringComparison.Ordinal) && w.GetParameters().Length == 1 && w.GetParameters()[0].ParameterType == typeof(IndexKeys<TDocument>));
            
            index_method?.Invoke(this._contextbase, [(IndexKeys<TDocument>)_contextCollectionMetadata.IndexKeys]);

            try
            {
                return this.IndexAsync((IndexKeys<TDocument>)_contextCollectionMetadata.IndexKeys, forceTransaction).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.LogError(exception: ex, "Fail create indexes.", _contextCollectionMetadata.IndexKeys);

                if (thowIndexExceptions)
                    throw;

                return false;
            }
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




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TProjection> FirstOrDefaultAsync<TProjection>([NotNull] Query<TDocument, TProjection> query,
            [MaybeNull] FindOptions<TDocument, TProjection>? findOptions = default,
            [MaybeNull] bool? forceTransaction = default,
            [MaybeNull] CancellationToken cancellationToken = default) => await this.GetOneAsync(query, findOptions, forceTransaction, cancellationToken);

        #endregion


        #region Any


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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ContextBase(DbSet<TDocument, TObjectId> dbSet) => dbSet._contextbase;













        #region Dispose

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await this.DisposeAsyncCore();

            this.Dispose(false);
        }

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
