using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using UCode.Extensions;
using UCode.Repositories;

namespace UCode.Mongo
{
    //https://mongodb.github.io/mongo-csharp-driver/2.11/reference/driver/crud/writing/
    /// <summary>
    /// Represents a raw database set.
    /// </summary>
    public class RawDbSet : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// The MongoDB collection associated with this raw database set.
        /// </summary>
        protected readonly IMongoCollection<BsonDocument> MongoCollection;

        /// <summary>
        /// The context base associated with this raw database set.
        /// </summary>
        private readonly ContextBase _contextbase;

        /// <summary>
        /// The logger associated with this raw database set.
        /// </summary>
        protected ILogger<RawDbSet> Logger
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawDbSet"/> class.
        /// </summary>
        /// <param name="contextBase">The context base associated with this raw database set.</param>
        /// <param name="collectionName">The name of the MongoDB collection.</param>
        /// <param name="timeSeriesOptions">The time series options for the MongoDB collection.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawDbSet([NotNull] ContextBase contextBase, string collectionName, Options.TimerSeriesOptions? timeSeriesOptions = null)
        {
            // If time series options are provided, create the collection if it doesn't exist
            if (timeSeriesOptions is not null)
            {
                if (contextBase.CollectionNames().FirstOrDefault(f => f.Equals(collectionName, StringComparison.Ordinal)) == default)
                {
                    contextBase.Database.CreateCollection(collectionName, new CreateCollectionOptions()
                    {
                        TimeSeriesOptions = timeSeriesOptions,
                        ExpireAfter = TimeSpan.FromSeconds(timeSeriesOptions.ExpireAfterSeconds)
                    });
                }
            };

            // Initialize the MongoDB collection
            this.MongoCollection =
                contextBase.Database.GetCollection<BsonDocument>(collectionName, new MongoCollectionSettings());

            // Set the context base
            this._contextbase = contextBase;

            // Initialize the logger
            this.Logger = contextBase.LoggerFactory.CreateLogger<RawDbSet>();

            //this.SetLogger(_ => _.Logger);
        }

        /// <summary>
        /// Retrieves the names and fields indexed by each index in the collection.
        /// This method requires further testing.
        /// </summary>
        /// <returns>
        /// An asynchronous enumerable of key-value pairs, where the key is the name of the index
        /// and the value is a list of the fields indexed by that index.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<KeyValuePair<string, List<string>>> GetIndexesAsync()
        {
            // Retrieve the list of indexes in the collection
            var idxsb = await this.MongoCollection.Indexes.ListAsync(session: this._contextbase.Session, default);

            // Iterate through each batch of indexes
            while (idxsb.MoveNext())
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
        /// Creates indexes for the collection.
        /// </summary>
        /// <param name="indexKeysDefinitions">Dictionary of index keys and their corresponding options.</param>
        /// <param name="useSession">Indicates whether to use a session. Default is false.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask IndexAsync([NotNull] Dictionary<IndexKeysDefinition<BsonDocument>, CreateIndexOptions> indexKeysDefinitions, bool useSession = false)
        {
            // Create a list to hold the models
            var models = new List<CreateIndexModel<BsonDocument>>();

            // Iterate over the dictionary of index keys and options
            foreach (var indexKeysDefinition in indexKeysDefinitions)
            {
                // Create a new model with the index key and options
                models.Add(new CreateIndexModel<BsonDocument>(indexKeysDefinition.Key,
                    indexKeysDefinition.Value ?? new CreateIndexOptions() { }));
            }

            // Check if session should be used
            if (useSession)
            {
                // Use the session to create the indexes
                _ = await this.MongoCollection.Indexes.CreateManyAsync(this._contextbase.Session, models);
            }
            else
            {
                // Create the indexes without a session
                _ = await this.MongoCollection.Indexes.CreateManyAsync(models);
            }
        }

        /// <summary>
        /// IQuerable com tipo definido
        /// </summary>
        /// <returns>Retorna o IQuerable do tipo da collection</returns>
        public IQueryable<BsonDocument> AsIQueryable() => this.MongoCollection.AsQueryable();

        #region FirstOrDefault
        /// <summary>
        /// Retrieves the first document that matches the specified query.
        /// </summary>
        /// <param name="query">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>The first document that matches the specified query, or the default value of BsonDocument if no matching document is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<BsonDocument> FirstOrDefaultAsync([NotNull] Query<BsonDocument> query, Options.FindOptions findOptions = default)
        {
            // Convert the findOptions to the correct type
            var opt = (Options.FindOptions<BsonDocument, BsonDocument>)findOptions;

            // Convert the query to the correct type
            var qry = (Query<BsonDocument, BsonDocument>)query;

            // Get the first document that matches the query
            return await this.GetOneAsync(qry, opt);
        }

        /// <summary>
        /// Retrieves the first document that matches the specified query.
        /// </summary>
        /// <param name="query">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>The first document that matches the specified query, or the default value of BsonDocument if no matching document is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<BsonDocument> FirstOrDefaultAsync([NotNull] Query<BsonDocument> query, Options.FindOptions<BsonDocument> findOptions = default)
        {
            // Convert the findOptions to the correct type
            Options.FindOptions<BsonDocument, BsonDocument> opt = findOptions;

            // Convert the query to the correct type
            Query<BsonDocument, BsonDocument> qry = query;

            // Get the first document that matches the query
            return await this.GetOneAsync(qry, opt);
        }

        /// <summary>
        /// Retrieves the first document that matches the specified query.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="query">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>The first document that matches the specified query, or the default value of TProjection if no matching document is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TProjection> FirstOrDefaultAsync<TProjection>([NotNull] Query<BsonDocument, TProjection> query, Options.FindOptions<BsonDocument, TProjection> findOptions = default) => await this.GetOneAsync(query, findOptions);

        #endregion

        #region Any
        /// <summary>
        /// Checks if any documents match the given query.
        /// </summary>
        /// <param name="query">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>True if any documents match the query, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<bool> AnyAsync([NotNull] Query<BsonDocument> query, Options.FindOptions<BsonDocument> findOptions = default)
        {
            // If findOptions is null, create a new instance with default values
            findOptions ??= new Options.FindOptions<BsonDocument>();

            // Create a new CountOptions instance based on findOptions
            CountOptions countOptions = findOptions;

            // Set the limit to 1 and skip to 0 to only count the first matching document
            countOptions.Limit = 1;
            countOptions.Skip = 0;

            // If the operation should not be performed in a transaction, use the session in the context
            if (findOptions.NotPerformInTransaction)
            {
                return await this.MongoCollection.CountDocumentsAsync(this._contextbase.Session, query, countOptions) > 0;
            }

            // Otherwise, use the default CountDocumentsAsync method
            return await this.MongoCollection.CountDocumentsAsync(query, countOptions) > 0;
        }

        #endregion

        #region Get

        #region Get One

        /// <summary>
        /// Retrieves a document from the collection based on the provided ObjectId.
        /// </summary>
        /// <param name="id">The ObjectId of the document to retrieve.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>The document with the specified ObjectId, or null if no document was found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<BsonDocument> GetAsync([NotNull] ObjectId id, [NotNull] Options.FindOptions findOptions = default)
        {
            // Convert the findOptions to the correct type
            Options.FindOptions<BsonDocument, BsonDocument> opt = findOptions;

            // Call the GetAsync method with the ObjectId and options
            return await this.GetAsync(id, opt);
        }

        /// <summary>
        /// Retrieves a document from the collection based on the provided ObjectId.
        /// </summary>
        /// <param name="id">The ObjectId of the document to retrieve.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>The document with the specified ObjectId, or null if no document was found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<BsonDocument> GetAsync([NotNull] ObjectId id, [NotNull] Options.FindOptions<BsonDocument> findOptions = default)
        {
            // Convert the findOptions to the correct type
            Options.FindOptions<BsonDocument, BsonDocument> opt = findOptions;

            // Call the GetAsync method with the ObjectId and converted options
            return await this.GetAsync(id, opt);
        }

        /// <summary>
        /// Retrieves a single document from the collection based on the provided ObjectId.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="id">The ObjectId of the document to retrieve.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>The document with the specified ObjectId, or the default value of TProjection if no matching document is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TProjection> GetAsync<TProjection>([NotNull] ObjectId id,
            [NotNull] Options.FindOptions<BsonDocument, TProjection> findOptions = default)
        {
            // Create a query to find the document with the specified ObjectId
            var qry = Query<BsonDocument, TProjection>.FromExpression(o => o["_id"].AsObjectId.Equals(id));

            // Retrieve the document using the GetOneAsync method
            return await this.GetOneAsync(qry, findOptions);
        }

        /// <summary>
        /// Retrieves a single document from the collection based on the provided query.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="query">The query to match the document against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>The document matching the query, or the default value of TProjection if no matching document is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TProjection> GetOneAsync<TProjection>([NotNull] Query<BsonDocument, TProjection> query, [NotNull] Options.FindOptions<BsonDocument, TProjection> findOptions = default)
        {
            // Initialize the result to the default value of TProjection
            var result = default(TProjection);

            // Set default options if not provided
            findOptions ??= new Options.FindOptions<BsonDocument, TProjection>();

            // Set skip and limit options for finding the first occurrence
            findOptions.Skip ??= 0;
            findOptions.Limit ??= 1;

            // Create the filter definition from the query
            FilterDefinition<BsonDocument> filterSelected = query;

            // Create the options for the find operation
            FindOptions<BsonDocument, TProjection> options = findOptions;

            // Create the cursor for the find operation
            IAsyncCursor<TProjection> cursor;

            // If the find operation should not be performed in a transaction
            if (findOptions.NotPerformInTransaction)
            {
                // Perform the find operation with the session and filter
                cursor = await this.MongoCollection.FindAsync(this._contextbase.Session, filterSelected, options);
            }
            else
            {
                // Perform the find operation with the filter
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }

            // Iterate over the cursor and retrieve the first occurrence
            while (await cursor.MoveNextAsync())
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

            return result;
        }
        #endregion
        /// <summary>
        /// Asynchronously retrieves documents from the collection by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the documents to retrieve.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the specified IDs.</returns>
        /// <remarks>
        /// This method is used to retrieve documents from the collection by their IDs.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<BsonDocument> GetAsync([NotNull] ObjectId[] ids, Options.FindOptions findOptions = default)
        {
            // Set default findOptions if not provided
            findOptions ??= new Options.FindOptions();

            // Create a query to match documents by their IDs
            var qry = Query<BsonDocument, BsonDocument>.FromExpression(f => ids.Contains(f["_id"].AsObjectId));
            Options.FindOptions<BsonDocument, BsonDocument> opt = findOptions;

            // Perform the find operation asynchronously
            await foreach (var item in this.GetAsync<BsonDocument>(qry, opt))
            {
                // Yield the retrieved document
                yield return item;
            }
        }

        /// <summary>
        /// Asynchronously retrieves documents from the collection by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the documents to retrieve.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the specified IDs.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<BsonDocument> GetAsync([NotNull] ObjectId[] ids, Options.FindOptions<BsonDocument> findOptions = default)
        {
            // Set default findOptions if not provided
            findOptions ??= new Options.FindOptions<BsonDocument>();

            // Create a query to match documents by their IDs
            var qry = Query<BsonDocument, BsonDocument>.FromExpression(f => ids.Contains(f["_id"].AsObjectId));
            Options.FindOptions<BsonDocument, BsonDocument> opt = findOptions;

            // Perform the find operation asynchronously
            await foreach (var item in this.GetAsync<BsonDocument>(qry, opt))
            {
                // Yield each matching document
                yield return item;
            }
        }

        /// <summary>
        /// Asynchronously retrieves documents from the collection by their IDs.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="ids">The IDs of the documents to retrieve.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the specified IDs.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> GetAsync<TProjection>([NotNull] ObjectId[] ids, Options.FindOptions<BsonDocument, TProjection> findOptions = default)
        {
            // Set default findOptions if not provided
            findOptions ??= new Options.FindOptions<BsonDocument, TProjection>();

            // Create a query to match documents by their IDs
            var qry = Query<BsonDocument, TProjection>.FromExpression(f => ids.Contains(f["_id"].AsObjectId));

            // Perform the find operation asynchronously
            await foreach (var item in this.GetAsync<TProjection>(qry, findOptions))
            {
                // Yield the retrieved document
                yield return item;
            }
        }

        //public IEnumerable<TProjection> Get<TProjection>([NotNull] Query<BsonDocument, TProjection> filter, [NotNull] Options.FindOptions<BsonDocument, TProjection> findOptions)
        //{
        //    var enumerable = await GetAsync<TProjection>(filter, findOptions).ToIEnumerable();
        //    foreach (var item in enumerable)
        //    {
        //        yield return item;
        //    }
        //}

        /// <summary>
        /// Asynchronously retrieves documents from the collection based on the provided query and find options.
        /// </summary>
        /// <param name="filter">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the provided query.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<BsonDocument> GetAsync([NotNull] Query<BsonDocument> filter, [NotNull] Options.FindOptions findOptions = default)
        {
            // Set default findOptions if not provided
            findOptions ??= new Options.FindOptions();

            // Convert the filter to the correct type for the GetAsync method
            Query<BsonDocument, BsonDocument> qry = filter;

            // Convert the findOptions to the correct type for the GetAsync method
            Options.FindOptions<BsonDocument, BsonDocument> opt = findOptions;

            // Perform the find operation asynchronously
            await foreach (var item in this.GetAsync(qry, opt))
            {
                // Yield each matching document
                yield return item;
            }
        }

        /// <summary>
        /// Asynchronously retrieves documents from the collection based on the provided query and find options.
        /// </summary>
        /// <param name="filter">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the provided query.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<BsonDocument> GetAsync([NotNull] Query<BsonDocument> filter, [NotNull] Options.FindOptions<BsonDocument> findOptions = default)
        {
            // Set default findOptions if not provided
            findOptions ??= new Options.FindOptions<BsonDocument>();

            // Convert the filter to the correct type for the GetAsync method
            Query<BsonDocument, BsonDocument> qry = filter;

            // Convert the findOptions to the correct type for the GetAsync method
            Options.FindOptions<BsonDocument, BsonDocument> opt = findOptions;

            // Perform the find operation asynchronously
            await foreach (var item in this.GetAsync(qry, opt))
            {
                // Yield the retrieved document
                yield return item;
            }
        }

        /// <summary>
        /// Asynchronously performs a full-text search on the collection.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="fullTextSearchOptions">Options for the full-text search.</param>
        /// <param name="filter">An optional filter to apply to the search results. Default is null.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the search criteria.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<BsonDocument> FulltextSearchAsync([NotNull] string text,
            [NotNull] Options.FullTextSearchOptions<BsonDocument> fullTextSearchOptions,
            [NotNull] Query<BsonDocument> filter = default,
            [NotNull] Options.FindOptions<BsonDocument> findOptions = default)
        {
            // Use the generic method to perform the full-text search and yield each result
            await foreach (var item in this.FulltextSearchAsync<BsonDocument>(text, fullTextSearchOptions, filter, findOptions))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Asynchronously performs a full-text search on the collection.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="text">The text to search for.</param>
        /// <param name="fullTextSearchOptions">Options for the full-text search.</param>
        /// <param name="filter">An optional filter to apply to the search results. Default is null.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the search criteria.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> FulltextSearchAsync<TProjection>([NotNull] string text,
            [NotNull] Options.FullTextSearchOptions<BsonDocument> fullTextSearchOptions,
            [NotNull] Query<BsonDocument, TProjection> filter = default,
            [NotNull] Options.FindOptions<BsonDocument, TProjection> findOptions = default)
        {
            // Set default findOptions if not provided
            findOptions ??= new Options.FindOptions<BsonDocument, TProjection>();

            // Create the filter definition from the text and full-text search options
            FilterDefinition<BsonDocument> filterSelected = Query<BsonDocument, TProjection>.FromText(text, fullTextSearchOptions);

            // If a filter is provided, add it to the filter definition
            if (filter != default)
            {
                filterSelected += filter;
            }

            // Create the options for the find operation
            FindOptions<BsonDocument, TProjection> options = findOptions;

            // Create the cursor for the find operation
            IAsyncCursor<TProjection> cursor;

            // If the find operation should not be performed in a transaction
            if (findOptions.NotPerformInTransaction)
            {
                // Perform the find operation with the session and filter
                cursor = await this.MongoCollection.FindAsync(this._contextbase.Session, filterSelected, options);
            }
            else
            {
                // Perform the find operation with the filter
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }

            // Iterate over the cursor and retrieve the search results
            while (await cursor.MoveNextAsync())
            {
                foreach (var item in cursor.Current)
                {
                    // Yield each matching document
                    if (item != null)
                    {
                        yield return item;
                    }
                }
            }
            // Dispose the cursor
            cursor.Dispose();
        }

        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TProjection> Queryable<TProjection>(
            [NotNull] Expression<Func<BsonDocument, TProjection>> queryable, 
            [NotNull] Options.AggregateOptions<BsonDocument> aggregateOptions = default)
        {
            if (queryable == default)
                throw new ArgumentException($"Argument \"{nameof(queryable)}\" is null.");

            aggregateOptions ??= new Options.AggregateOptions<BsonDocument>();

            MongoDB.Driver.AggregateOptions options = aggregateOptions;

            //var build = queryable.Compile();
            //var linqProvider = MongoCollection.Database.Client.Settings.LinqProvider;
            //var t = Type.GetType("MongoDB.Driver.Linq.Linq3Implementation.LinqProviderV3");
            //var adapter = Activator.CreateInstance(t);
            //linqProvider.GetAdapter().AsQueryable(MongoCollection, _contextbase.Session, aggregateOptions);
            
            IMongoQueryable<BsonDocument> mongoIquerable = MongoCollection.AsQueryable(_contextbase.Session, options);

            var mongoQueryable = mongoIquerable.Provider.CreateQuery<TProjection>(queryable);

            //var projection = build.Invoke(mongoIquerable);

            return mongoQueryable;
        }*/

        //public IQueryable<BsonDocument> AsQueryable([NotNull] Options.AggregateOptions<BsonDocument> aggregateOptions = default)
        //{
        //    return MongoCollection.AsQueryable(_contextbase.Session, aggregateOptions);
        //}

        /// <summary>
        /// Returns an IQueryable of BsonDocument objects.
        /// </summary>
        /// <param name="preApprend">Optional function to pre-append to the queryable.</param>
        /// <param name="aggregateOptions">Optional aggregate options.</param>
        /// <returns>An IQueryable of BsonDocument objects.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IQueryable<BsonDocument> AsQueryable(Func<IQueryable<BsonDocument>, IQueryable<BsonDocument>> preApprend = default, Options.AggregateOptions<BsonDocument> aggregateOptions = default) => preApprend == default ?
                this.MongoCollection.AsQueryable(this._contextbase.Session, aggregateOptions) :
                preApprend(this.MongoCollection.AsQueryable(this._contextbase.Session, aggregateOptions));

        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TProjection> Queryable3<TProjection>(
            [NotNull] Expression<Func<BsonDocument, TProjection>> queryable,
            [NotNull] Options.AggregateOptions<BsonDocument> aggregateOptions = default)
        {
            if (queryable == default)
                throw new ArgumentException($"Argument \"{nameof(queryable)}\" is null.");

            aggregateOptions ??= new Options.AggregateOptions<BsonDocument>();

            MongoDB.Driver.AggregateOptions options = aggregateOptions;

            //var build = queryable.Compile();
            //var linqProvider = MongoCollection.Database.Client.Settings.LinqProvider;
            //var t = Type.GetType("MongoDB.Driver.Linq.Linq3Implementation.LinqProviderV3");
            //var adapter = (LinqProvider)Activator.CreateInstance(t);

            var linqProviderAdapterV3Type= Type.GetType("MongoDB.Driver.Linq.Linq3Implementation.LinqProviderAdapterV3");
            var linqProviderAdapterV3 = Activator.CreateInstance(linqProviderAdapterV3Type);
            var querableMethod = linqProviderAdapterV3Type.GetMethod("AsQueryable");
            IMongoQueryable<BsonDocument> mongoIquerable = (IMongoQueryable<BsonDocument>)querableMethod.MakeGenericMethod(typeof(BsonDocument)).Invoke(linqProviderAdapterV3, new object[] { MongoCollection, _contextbase.Session, aggregateOptions });

            
            //MongoDB.Driver.Linq.Linq3Implementation.LinqProviderAdapterV3 >> MongoDB.Driver.Linq.LinqProviderAdapter
            //linqProvider.GetAdapter().AsQueryable(MongoCollection, _contextbase.Session, aggregateOptions);

            //IMongoQueryable<BsonDocument> mongoIquerable = MongoCollection.AsQueryable(_contextbase.Session, options);

            var mongoQueryable = mongoIquerable.Provider.CreateQuery<TProjection>(queryable);

            //var projection = build.Invoke(mongoIquerable);

            return mongoQueryable;
        }
        */

        /// <summary>
        /// Asynchronously retrieves documents from the collection that match the specified filter.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="filter">The filter to apply to the documents.</param>
        /// <param name="findOptions">The options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents that match the filter.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> GetAsync<TProjection>([NotNull] Query<BsonDocument, TProjection> filter, [NotNull] Options.FindOptions<BsonDocument, TProjection> findOptions = default)
        {
            // If findOptions is null, create a new instance with default values
            findOptions ??= new Options.FindOptions<BsonDocument, TProjection>();

            // Get the selected filter
            FilterDefinition<BsonDocument> filterSelected = filter;

            // Get the find options
            FindOptions<BsonDocument, TProjection> options = findOptions;

            // Get the cursor for the documents that match the filter
            IAsyncCursor<TProjection> cursor;
            if (findOptions.NotPerformInTransaction)
            {
                // If the operation should not be performed in a transaction, use the session in the context
                cursor = await this.MongoCollection.FindAsync(this._contextbase.Session, filterSelected, options);
            }
            else
            {
                // Otherwise, use the default FindAsync method
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }

            // Iterate through the documents in the cursor
            while (await cursor.MoveNextAsync())
            {
                // Iterate through the documents in the current batch
                foreach (var item in cursor.Current)
                {
                    // If the item is not null, yield it
                    if (item != null)
                    {
                        yield return item;
                    }
                }
            }
            // Dispose the cursor
            cursor.Dispose();
        }

        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<PagedResult<TProjection>> GetPagedAsync<TProjection>([NotNull] Query<BsonDocument> filter, [NotNull] Options.FindOptionsPaging<BsonDocument> findOptions)
        {
            Query<BsonDocument, TProjection> qry = filter;

            Options.FindOptionsPaging<BsonDocument, TProjection> opt = findOptions;

            return await this.GetPagedAsync(qry, opt);
        }

        /// <summary>
        /// Retrieves a paged result of documents from the collection based on the provided query and find options.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="filter">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paged result of documents matching the provided query.</returns>
        /// <exception cref="ArgumentException">Thrown if the page size or current page is invalid or null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the filter or findOptions parameter is null.</exception>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<PagedResult<TProjection>> GetPagedAsync<TProjection>([NotNull] Query<BsonDocument, TProjection> filter, [NotNull] Options.FindOptionsPaging<BsonDocument, TProjection> findOptions)
        {
            // Convert the filter and findOptions to strings for logging purposes
            var fstr = filter.ToString();
            var fostr = findOptions.ToString();
            var fojson = findOptions.JsonString();

            // Log the method call with filter and findOptions
            this.Logger.LogDebug($"Called \"{nameof(GetPagedAsync)}(...)\" with args (filter: \"{filter}\", findOptions: \"{fojson}\")");

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
            FilterDefinition<BsonDocument> filterSelected = filter;

            // Create the options for the find operation
            FindOptions<BsonDocument, TProjection> options = findOptions;

            /// <summary>
            /// Creates a new instance of <see cref="CountOptions"/> by copying the properties from the provided <see cref="FindOptionsPaging{BsonDocument, TProjection}"/>.
            /// </summary>
            /// <param name="findOptions">The <see cref="FindOptionsPaging{BsonDocument, TProjection}"/> to copy properties from.</param>
            /// <returns>A new instance of <see cref="CountOptions"/> with properties copied from <paramref name="findOptions"/>.</returns>
            var countOptions = findOptions.CopyTo<Options.IOptions, Options.CountOptions>();

            // Set the Limit property to null to count all documents
            countOptions.Limit = null;

            // Set the Skip property to null to start counting from the first document
            countOptions.Skip = null;

            /// <summary>
            /// Asynchronously counts the number of documents in the collection that match the provided filter.
            /// </summary>
            /// <param name="filter">The filter to apply to the documents.</param>
            /// <param name="countOptions">The options for the count operation.</param>
            /// <returns>The total number of documents that match the filter.</returns>
            var total = Convert.ToInt32(await this.CounBsonDocumentsAsync((FilterDefinition<BsonDocument>)filter, countOptions));

            // Create the cursor for the find operation
            IAsyncCursor<TProjection> cursor;

            // If the find operation should not be performed in a transaction
            if (findOptions.NotPerformInTransaction)
            {
                // Log the method call with session, filter, and options
                this.Logger.LogDebug($"Call \"this.MongoCollection.FindAsync(...)\" with session, filter: \"{filterSelected}\" and options: {options.JsonString()}");

                // Perform the find operation with the session and filter
                cursor = await this.MongoCollection.FindAsync(this._contextbase.Session, filterSelected, options);
            }
            else
            {
                // Log the method call without session, filter, and options
                this.Logger.LogDebug($"Call \"this.MongoCollection.FindAsync(...)\" without session, filter: \"{filterSelected}\" and options: {options.JsonString()}");

                // Perform the find operation without the session
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }

            // Initialize the result array with the specified size
            var itens = new TProjection[options.Limit < total ? options.Limit.Value : total];

            // Initialize the current position in the result array
            var trPos = 0;


            // Initialize the last position in the result array
            var lastPos = 0;

            // Iterate over each batch of documents in the cursor
            while (await cursor.MoveNextAsync())
            {
                // Iterate over each item in the current batch
                foreach (var item in cursor.Current)
                {
                    // Store the current position in the result array
                    lastPos = trPos++;

                    // Check if the item is not null
                    if (item != null)
                    {
                        // Store the item in the result array
                        itens[lastPos] = item;
                    }
                }
            }

            // Disposes the cursor after it has been used.
            cursor.Dispose();

            // If the last position in the items array is less than the length of the array, resizes the array to the last position.
            if (lastPos + 1 < itens.Length)
            {
                Array.Resize(ref itens, lastPos);
            }

            #endregion find
            // Returns a PagedResult object containing the items array, current page, page size, and total number of items.
            return new PagedResult<TProjection>(itens, Convert.ToInt32(findOptions.CurrentPage), Convert.ToInt32(findOptions.PageSize), total);
        }

        #endregion

        #region FindOneAndUpdateAsync
        /// <summary>
        /// Asynchronously finds a single document and updates it.
        /// </summary>
        /// <param name="query">The query to match the document against.</param>
        /// <param name="options">The options for the find and update operation.</param>
        /// <returns>The updated document.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<BsonDocument> FindOneAndUpdateAsync([NotNull] Query<BsonDocument> query,
            [NotNull] Options.FindOneAndUpdateOptions<BsonDocument> options)
        {
            // Create a copy of the options to avoid modifying the original
            FindOneAndUpdateOptions<BsonDocument> fouOptions = options;

            // Declare a variable to hold the result
            BsonDocument result;

            // Get the filter and update definitions from the query
            FilterDefinition<BsonDocument> filter = query;
            UpdateDefinition<BsonDocument> update = query.Update;

            // If the operation should not be performed in a transaction
            if (options.NotPerformInTransaction)
            {
                // Perform the find and update operation without a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);
            }

            // If a transaction is in use
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the find and update operation with a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(this._contextbase.Session, filter, update, fouOptions);
            }

            // If no transaction is in use
            else
            {
                // Perform the find and update operation without a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.FindOneAndUpdateAsync(_contextbase.Session, filter, update, fouOptions);
            //else
            //    result = await MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);

            // Return the updated document
            return result;
        }

        /// <summary>
        /// Asynchronously finds a single document and updates it based on the specified filter and update.
        /// </summary>
        /// <param name="filter">The filter to match the document against.</param>
        /// <param name="update">The update to apply to the document.</param>
        /// <param name="options">Options for the find and update operation.</param>
        /// <returns>The updated document.</returns>        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<BsonDocument> FindOneAndUpdateAsync([NotNull] string filter, [NotNull] string update,
            [NotNull] Options.FindOneAndUpdateOptions<BsonDocument> options)
        {
            // Create a copy of the options to avoid modifying the original
            FindOneAndUpdateOptions<BsonDocument> fouOptions = options;

            BsonDocument result;

            // If the operation should not be performed in a transaction
            if (options.NotPerformInTransaction)
            {
                // Perform the find and update operation without a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);
            }

            // If a transaction is in use
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the find and update operation with a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(this._contextbase.Session, filter, update, fouOptions);
            }

            // If no transaction is in use
            else
            {
                // Perform the find and update operation without a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.FindOneAndUpdateAsync(_contextbase.Session, filter, update, fouOptions);
            //else
            //    result = await MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);

            return result;
        }

        /// <summary>
        /// Asynchronously finds a single document and updates it based on the specified filter and update.
        /// </summary>
        /// <param name="filter">The filter to match the document against.</param>
        /// <param name="update">The update to apply to the document.</param>
        /// <param name="options">Options for the find and update operation. Default is null.</param>
        /// <returns>The updated document.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<BsonDocument> FindOneAndUpdateAsync(
           [NotNull] string filter,
           [NotNull] PipelineUpdateDefinition<BsonDocument> update,
           [NotNull] Options.FindOneAndUpdateOptions<BsonDocument> options = default)
        {

            // Create a copy of the options to avoid modifying the original
            FindOneAndUpdateOptions<BsonDocument> fouOptions = options;

            BsonDocument result;

            // If the operation should not be performed in a transaction
            if (options.NotPerformInTransaction)
            {
                // Perform the find and update operation without a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);
            }

            // If a transaction is in use
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the find and update operation with a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(this._contextbase.Session, filter, update, fouOptions);
            }

            // If no transaction is in use
            else
            {
                // Perform the find and update operation without a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.FindOneAndUpdateAsync(_contextbase.Session, filter, update, fouOptions);
            //else
            //    result = await MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);

            // Return the updated document
            return result;
        }

        #endregion FindOneAndUpdateAsync

        #region UpdateManyAsync

        /// <summary>
        /// Updates multiple documents in the collection that match the specified query.
        /// </summary>
        /// <param name="query">The query to match the documents against.</param>
        /// <param name="options">The options for the update operation. Default is null.</param>
        /// <returns>The number of modified documents, or -1 if the operation was not acknowledged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> UpdateManyAsync([NotNull] Query<BsonDocument> query,
            [NotNull] Options.UpdateOptions<BsonDocument> options)
        {
            // Initialize the result to null
            UpdateResult result;

            // If a transaction is not in use and the operation is not set to not perform in a transaction
            if (!this._contextbase.IsUseTransaction && !options.NotPerformInTransaction)
            {
                // Perform the update operation without a session
                result = await this.MongoCollection.UpdateManyAsync(query, query.Update, options);
            }
            else
            {
                // Perform the update operation with a session
                result = await this.MongoCollection.UpdateManyAsync(this._contextbase.Session, query, query.Update, options);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.UpdateManyAsync(_contextbase.Session, query, query.Update, options);
            //else
            //    result = await MongoCollection.UpdateManyAsync(query, query.Update, options);

            // Return the number of modified documents, or -1 if the operation was not acknowledged
            return result.IsAcknowledged ? result.ModifiedCount : -1;
        }

        /// <summary>
        /// Updates multiple documents in the collection that match the specified filter and update.
        /// </summary>
        /// <param name="filter">The filter to match the documents against.</param>
        /// <param name="update">The update to apply to the matched documents.</param>
        /// <param name="options">The options for the update operation. Default is null.</param>
        /// <returns>The number of modified documents, or -1 if the operation was not acknowledged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> UpdateManyAsync([NotNull] string filter, [NotNull] string update,
            [NotNull] Options.UpdateOptions<BsonDocument> options)
        {
            // Initialize the result to null
            UpdateResult result;

            // If the operation should not be performed in a transaction
            if (options.NotPerformInTransaction)
            {
                // Perform the update operation without a session
                result = await this.MongoCollection.UpdateManyAsync(filter, update, options);
            }

            // If a transaction is in use
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the update operation with a session
                result = await this.MongoCollection.UpdateManyAsync(this._contextbase.Session, filter, update, options);
            }

            // If no transaction is in use
            else
            {
                // Perform the update operation without a session
                result = await this.MongoCollection.UpdateManyAsync(filter, update, options);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.UpdateManyAsync(_contextbase.Session, filter, update, options);
            //else
            //    result = await MongoCollection.UpdateManyAsync(filter, update, options);

            // Return the number of modified documents, or -1 if the operation was not acknowledged
            return result.IsAcknowledged ? result.ModifiedCount : -1;
        }

        #endregion UpdateManyAsync

        #region Count

        /// <summary>
        /// Asynchronously counts the number of BsonDocuments in the collection that match the specified query.
        /// </summary>
        /// <param name="query">The query to match the BsonDocuments against.</param>
        /// <param name="countOptions">Options for the count operation. Default is null.</param>
        /// <returns>The number of BsonDocuments that match the query.</returns>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> CounBsonDocumentsAsync([NotNull] Query<BsonDocument> query, Options.CountOptions countOptions = default)
        {
            // If countOptions is null, create a new instance with default values
            countOptions ??= new Options.CountOptions();

            // Convert the query to a FilterDefinition and call the CounBsonDocumentsAsync method with it
            return await this.CounBsonDocumentsAsync((FilterDefinition<BsonDocument>)query, countOptions);
        }

        /// <summary>
        /// Asynchronously counts the number of BsonDocuments in the collection that match the specified query.
        /// </summary>
        /// <param name="preApprend">A function to pre-append to the queryable.</param>
        /// <param name="aggregateOptions">Optional aggregate options.</param>
        /// <returns>The number of BsonDocuments that match the query.</returns>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> CounBsonDocumentsAsync([NotNull] Func<IQueryable<BsonDocument>, IQueryable<BsonDocument>> preApprend, Options.AggregateOptions<BsonDocument> aggregateOptions = default)
        {
            // Get the queryable for the collection
            var query = this.MongoCollection.AsQueryable(this._contextbase.Session, new AggregateOptions());

            // Apply the pre-append function to the queryable
            var queryAppended = preApprend.Invoke(query);

            // Count the number of documents in the queryable
            var count = queryAppended.LongCount();

            // Await an already completed task to allow for async execution
            await Task.CompletedTask;

            // Return the count of documents
            return count;
        }

        /// <summary>
        /// Asynchronously counts the number of BsonDocuments in the collection that match the specified filter.
        /// </summary>
        /// <param name="filterDefinition">The filter to match the BsonDocuments against.</param>
        /// <param name="countOptions">Options for the count operation. Default is null.</param>
        /// <returns>The number of BsonDocuments that match the filter.</returns>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<long> CounBsonDocumentsAsync([NotNull] FilterDefinition<BsonDocument> filterDefinition, Options.CountOptions countOptions = default)
        {
            // If countOptions is null, create a new instance with default values
            countOptions ??= new Options.CountOptions();

            // If the operation should not be performed in a transaction, use the session in the context
            if (countOptions.NotPerformInTransaction)
            {
                return await this.MongoCollection.CountDocumentsAsync(this._contextbase.Session, filterDefinition, countOptions);
            }

            // Otherwise, use the default CountDocumentsAsync method
            return await this.MongoCollection.CountDocumentsAsync(filterDefinition, countOptions);
        }

        #endregion

        #region Update

        /// <summary>
        /// Asynchronously updates a document in the collection by adding an element to a set.
        /// </summary>
        /// <param name="query">The query to match the document against.</param>
        /// <param name="updateOptions">Options for the update operation. Default is null.</param>
        /// <returns>The number of modified documents, or -1 if the operation was not acknowledged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> UpdateAddToSetAsync([NotNull] Query<BsonDocument> query,
            Options.UpdateOptions<BsonDocument> updateOptions = default)
        {
            // If updateOptions is null, create a new instance with default values
            updateOptions ??= new Options.UpdateOptions<BsonDocument>();

            // Call the UpdateAsync method with the query, update, and options
            return await this.UpdateAsync(query, query.Update, updateOptions);
        }

        /// <summary>
        /// Asynchronously updates a document in the collection that matches the specified filter.
        /// </summary>
        /// <param name="filterDefinition">The filter to match the document against.</param>
        /// <param name="updateDefinition">The update to apply to the document.</param>
        /// <param name="updateOptions">Options for the update operation.</param>
        /// <returns>The number of modified documents, or -1 if the operation was not acknowledged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<long> UpdateAsync(
            [NotNull] FilterDefinition<BsonDocument> filterDefinition,
            [NotNull] UpdateDefinition<BsonDocument> updateDefinition,
            [NotNull] Options.UpdateOptions updateOptions)
        {
            // Initialize the result to null
            UpdateResult result;

            // If the operation should not be performed in a transaction
            if (updateOptions.NotPerformInTransaction)
            {
                // Perform the update operation without a session
                result = await this.MongoCollection.UpdateOneAsync(filterDefinition, updateDefinition, updateOptions);
            }

            // If a transaction is in use
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the update operation with a session
                result = await this.MongoCollection.UpdateOneAsync(this._contextbase.Session, filterDefinition, updateDefinition, updateOptions);
            }

            // If no transaction is in use
            else
            {
                // Perform the update operation without a session
                result = await this.MongoCollection.UpdateOneAsync(filterDefinition, updateDefinition, updateOptions);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.UpdateOneAsync(_contextbase.Session, filterDefinition, updateDefinition, updateOptions);
            //else
            //    result = await MongoCollection.UpdateOneAsync(filterDefinition, updateDefinition, updateOptions);

            // Return the number of modified documents, or -1 if the operation was not acknowledged
            return result == default ? -1 : result.IsAcknowledged ? result.ModifiedCount : -1;
        }


        #endregion

        #region Insert

        /// <summary>
        /// Inserts a single document into the collection.
        /// </summary>
        /// <param name="source">The document to insert.</param>
        /// <param name="insertOneOptions">Options for the insert operation. Default is null.</param>
        /// <returns>The number of inserted documents.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> InsertAsync([NotNull] BsonDocument source, Options.InsertOneOptions insertOneOptions = default)
        {
            // If no options were provided, create a new default options object
            insertOneOptions ??= new Options.InsertOneOptions();

            // Create a list to hold the write models for the insert operation
            var writeModels = new List<WriteModel<BsonDocument>>
            {
                new InsertOneModel<BsonDocument>(source)
            };

            // Perform the insert operation using the write models and options
            ///return await this.BulkWriteAsync(writeModels, insertOneOptions);
            if ((this._contextbase.Session != null && this._contextbase.Session.IsInTransaction) || insertOneOptions.NotPerformInTransaction)
            {
                // If a session is in use or the operation should not be performed in a transaction, use the session in the context
                await this.MongoCollection.InsertOneAsync(this._contextbase.Session, source, insertOneOptions);
            }
            else
            {
                // Otherwise, use the default InsertOneAsync method
                await this.MongoCollection.InsertOneAsync(source, insertOneOptions);
            }

            // Return the number of inserted documents
            return 1;
        }

        /// <summary>
        /// Inserts multiple documents into the collection.
        /// </summary>
        /// <param name="docs">The documents to insert.</param>
        /// <param name="insertManyOptions">Options for the insert operation. Default is null.</param>
        /// <returns>The number of inserted documents.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> InsertAsync([NotNull] IEnumerable<BsonDocument> docs, Options.InsertManyOptions insertManyOptions = default)
        {
            // If no options were provided, create a new default options object
            insertManyOptions ??= new Options.InsertManyOptions();

            // Create a list to hold the write models for the insert operation
            var writeModels = new List<WriteModel<BsonDocument>>();

            // Iterate over the documents and create insert write models for each document
            foreach (var doc in docs)
            {
                writeModels.Add(new InsertOneModel<BsonDocument>(doc));
            }

            // Perform the insert operation using the write models and options
            return await this.BulkWriteAsync(writeModels, insertManyOptions);
            //if (insertManyOptions.UseSession)
            //    await MongoCollection.InsertManyAsync(_contextbase.Session, docs, insertManyOptions);
            //else
            //    await MongoCollection.InsertManyAsync(docs, insertManyOptions);
        }

        #endregion

        #region Replace
        /// <summary>
        /// Replaces multiple documents in the collection.
        /// </summary>
        /// <param name="docs">The documents to replace.</param>
        /// <param name="query">An optional query to match the documents to be replaced. Default is null.</param>
        /// <param name="replaceOptions">Options for the replace operation. Default is null.</param>
        /// <returns>The number of replaced documents.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> ReplaceAsync([NotNull] IEnumerable<BsonDocument> docs, Query<BsonDocument>? query = null, Options.ReplaceOptions<BsonDocument>? replaceOptions = default)
        {
            // Set default replace options if not provided
            replaceOptions ??= new Options.ReplaceOptions<BsonDocument>();

            var updates = new List<WriteModel<BsonDocument>>();

            // Create a lambda expression to match documents by their IDs
            Expression<Func<BsonDocument, BsonDocument, bool>> exp = (item, constrain) => item["_id"].AsObjectId.Equals(constrain["_id"].AsObjectId);

            foreach (var doc in docs)
            {
                //Expression<Func<BsonDocument, bool>> exp = f => f.Id.Equals(doc.Id);

                //ExpressionFilterDefinition<BsonDocument>

                // Create a filter definition to match the document by its ID
                FilterDefinition<BsonDocument> filterDefinition = (query ?? exp).CompleteExpression(doc);

                // Create a replace write model with the filter definition and document
                var model = new ReplaceOneModel<BsonDocument>(filterDefinition, doc)
                {
                    IsUpsert = replaceOptions.IsUpsert,
                    Collation = replaceOptions.Collation,
                    Hint = replaceOptions.Hint
                };

                updates.Add(model);
            }

            // Create bulk write options with the replace options
            Options.BulkWriteOptions bulkWriteOptions = replaceOptions;
            bulkWriteOptions.IsOrdered = false;

            // Perform the bulk write operation with the write models and options
            return await this.BulkWriteAsync(updates, bulkWriteOptions);
        }

        /// <summary>
        /// Replaces a document in the collection.
        /// </summary>
        /// <param name="doc">The document to replace.</param>
        /// <param name="replaceOptions">Options for the replace operation. Default is null.</param>
        /// <returns>The number of replaced documents.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> ReplaceAsync([NotNull] BsonDocument doc, Options.ReplaceOptions<BsonDocument> replaceOptions = default)
        {
            // Log the method call with the document ID and options
            this.Logger.LogDebug($"Called {nameof(ReplaceAsync)}(...), with id: \"{doc["_id"].AsObjectId.ToString()}\" and options: \"{(replaceOptions != null ? replaceOptions.JsonString() : "NULL")}\"");

            // Get the ID of the document
            var id = doc["_id"].AsObjectId;

            // Create a query to match the document by ID
            var query = Query<BsonDocument>.FromExpression(d => d["_id"].AsObjectId.Equals(id));

            // Create a JSON string to find the document by ID
            var jsonFind = $"{{ \"_id\": {{\"$eq\": \"{doc["_id"].AsObjectId}\"}} }}";

            // Call the ReplaceAsync method with the query, document, and options
            return await this.ReplaceAsync(query, doc, replaceOptions);
            //return await this.ReplaceAsync(Query<BsonDocument>.FromExpression(f => f.Id.Equals(doc.Id)), doc, replaceOptions);
        }

        /// <summary>
        /// Replaces a document in the collection.
        /// </summary>
        /// <param name="query">The query to match the document to be replaced. If null, the document is matched by its ID.</param>
        /// <param name="doc">The document to replace.</param>
        /// <param name="replaceOptions">Options for the replace operation. Default is null.</param>
        /// <returns>The number of replaced documents. Returns -1 if the operation was not acknowledged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> ReplaceAsync([NotNull] Query<BsonDocument> query, [NotNull] BsonDocument doc, Options.ReplaceOptions<BsonDocument> replaceOptions = default)
        {
            // Set default replace options if not provided
            replaceOptions ??= new Options.ReplaceOptions<BsonDocument>();

            // Perform the replace operation
            ReplaceOneResult result;

            // Create a filter definition to match the document to be replaced
            FilterDefinition<BsonDocument> filterDefinition = query ?? Query<BsonDocument>.FromExpression(f => f["_id"].AsObjectId.Equals(doc["_id"].AsObjectId));

            if (replaceOptions.NotPerformInTransaction)
            {
                // Perform the replace operation without a transaction
                result = await this.MongoCollection.ReplaceOneAsync(filterDefinition, doc, replaceOptions);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the replace operation within a transaction
                result = await this.MongoCollection.ReplaceOneAsync(this._contextbase.Session, filterDefinition, doc, replaceOptions);
            }
            else
            {
                // Perform the replace operation without a transaction
                result = await this.MongoCollection.ReplaceOneAsync(filterDefinition, doc, replaceOptions);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.ReplaceOneAsync(_contextbase.Session, filterDefinition, doc, replaceOptions);
            //else
            //    result = await MongoCollection.ReplaceOneAsync(filterDefinition, doc, replaceOptions);

            // Return the number of replaced documents, or -1 if the operation was not acknowledged
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
            return result == null ? -1 : result.IsAcknowledged ? result.ModifiedCount : -1;
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes a single document from the collection by its ID.
        /// </summary>
        /// <param name="id">The ID of the document to delete.</param>
        /// <param name="deleteOptions">Options for the delete operation. Default is null.</param>
        /// <returns>The number of deleted documents. If the operation was not acknowledged, -1 is returned.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> DeleteOneAsync([NotNull] ObjectId id, Options.DeleteOptions<BsonDocument> deleteOptions = default)
        {
            // Set default delete options if not provided
            deleteOptions ??= new Options.DeleteOptions<BsonDocument>();

            // Delete the document by its ID
            return await this.DeleteAsync(new[] { id }, deleteOptions);
        }

        /// <summary>
        /// Deletes multiple documents from the collection by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the documents to delete.</param>
        /// <param name="deleteOptions">Options for the delete operation. Default is null.</param>
        /// <returns>The number of deleted documents. If the operation was not acknowledged, -1 is returned.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> DeleteAsync([NotNull] IEnumerable<ObjectId> ids,
            Options.DeleteOptions<BsonDocument> deleteOptions = default)
        {
            // Set default delete options if not provided
            deleteOptions ??= new Options.DeleteOptions<BsonDocument>();

            // Create a list to hold the write models for the delete operations
            var updates = new List<WriteModel<BsonDocument>>();

            // Iterate over the IDs and create delete write models for each ID
            foreach (var id in ids)
            {
                // Create a filter definition to match the document by its ID
                Expression<Func<BsonDocument, bool>> exp = (f) => f["_id"].AsObjectId.Equals(id);

                FilterDefinition<BsonDocument> filterDefinition = exp;

                // Create a delete write model with the filter definition and delete options
                var model = new DeleteOneModel<BsonDocument>(filterDefinition)
                {
                    Collation = deleteOptions.Collation,
                    Hint = deleteOptions.Hint
                };

                // Add the write model to the list
                updates.Add(model);
            }

            // Create bulk write options with the delete options
            Options.BulkWriteOptions bulkWriteOptions = deleteOptions;
            bulkWriteOptions.IsOrdered = false;
            bulkWriteOptions.BypassDocumentValidation = true;

            // Perform the bulk write operation with the write models and options
            return await this.BulkWriteAsync(updates, bulkWriteOptions);
        }

        /// <summary>
        /// Deletes multiple documents from the collection that match the specified query.
        /// </summary>
        /// <param name="query">The query to match documents against.</param>
        /// <param name="deleteOptions">Options for the delete operation. Default is null.</param>
        /// <returns>The number of deleted documents. If the operation was not acknowledged, -1 is returned.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> DeleteAsync([NotNull] Query<BsonDocument> query,
            Options.DeleteOptions<BsonDocument> deleteOptions = default)
        {
            // Set default delete options if not provided
            deleteOptions ??= new Options.DeleteOptions<BsonDocument>();

            // Create a list with a single DeleteManyModel that matches the query
            var updates = new List<WriteModel<BsonDocument>> { new DeleteManyModel<BsonDocument>(query) };

            // Create bulk write options with the delete options
            Options.BulkWriteOptions bulkWriteOptions = deleteOptions;
            bulkWriteOptions.IsOrdered = false;
            bulkWriteOptions.BypassDocumentValidation = true;

            // Perform the bulk write operation with the write models and options
            return await this.BulkWriteAsync(updates, bulkWriteOptions);
        }

        #endregion

        #region Aggregate
        /// <summary>
        /// Asynchronously performs an aggregation operation on the collection and returns the results as an asynchronous enumerable of BsonDocument objects.
        /// </summary>
        /// <param name="query">The query to match the documents against.</param>
        /// <param name="aggregateOptions">Options for the aggregation operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of BsonDocument objects.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<BsonDocument> Aggregate([NotNull] Query<BsonDocument> query,
            Options.AggregateOptions<BsonDocument> aggregateOptions = default)
        {
            // Set default aggregate options if not provided
            aggregateOptions ??= new Options.AggregateOptions<BsonDocument>();

            // Perform the aggregation operation and iterate over the results
            foreach (var item in (await this.Aggregate<BsonDocument>(query, aggregateOptions)).Item1)
            {
                // Yield return each item in the aggregation results
                yield return item;
            }
        }

        /// <summary>
        /// Asynchronously performs an aggregation operation on the collection and returns the results along with the total count, skip, and limit.
        /// </summary>
        /// <typeparam name="TR">The type of the result documents.</typeparam>
        /// <param name="query">The query to match the documents against.</param>
        /// <param name="aggregateOptions">Options for the aggregation operation. Default is null.</param>
        /// <returns>A tuple containing the results, total count, skip, and limit.</returns>
        /// <exception cref="Exception">Thrown if the skip or limit options are invalid or null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<(IEnumerable<TR>, int, int, int)> Aggregate<TR>([NotNull] Query<TR> query,
            Options.AggregateOptions<BsonDocument> aggregateOptions = default)
        {
            // Set default aggregate options if not provided
            aggregateOptions ??= new Options.AggregateOptions<BsonDocument>();

            // Set default aggregate options if not provided
            if (!aggregateOptions.Skip.HasValue || aggregateOptions.Skip.Value < 0)
            {
                throw new Exception("Skip is invalid or null.");
            }

            if (!aggregateOptions.Limit.HasValue || aggregateOptions.Limit.Value <= 0)
            {
                throw new Exception("Limit is invalid or null.");
            }

            // Convert the query to a BsonDocument array
            BsonDocument[] bsonDocumentFilter = query;

            // Create a list of BsonDocument filters with skip and limit
            var bsonDocumentFilterPaging = ((BsonDocument[])query).ToList();

            // Add skip and limit filters to the list
            bsonDocumentFilterPaging.Add(
                new BsonDocument(new BsonElement("$skip", BsonValue.Create(aggregateOptions.Skip.Value))));
            bsonDocumentFilterPaging.Add(
                new BsonDocument(new BsonElement("$limit", BsonValue.Create(aggregateOptions.Limit.Value))));

            // Create a structured aggregate query with result and total count
            var structAggregate = /*lang=json,strict*/ "[ { \"$facet\": { \"result\": [],\"total\": [{\"$count\": \"total\"}]}} ]";

            // Deserialize the structured aggregate query into a BsonDocument array
            var bson = BsonSerializer.Deserialize<BsonDocument[]>(structAggregate);

            // Replace the first element of the facet with the paging filters
            bson[0][0][0] = new BsonArray(bsonDocumentFilterPaging);

            // Add the query filters to the total count facet
            foreach (var it in new BsonArray(bsonDocumentFilter).Reverse())
            {
                ((BsonArray)bson[0][0][1]).Insert(0, it);
            }

            // If the debugger is attached, convert the BsonDocument to a JSON string for debugging purposes
            if (Debugger.IsAttached)
            {
                // converter novamente em string para verificar se o json de consulta esta correto
                var stringWriter = new StringWriter();
                BsonSerializer.Serialize(new JsonWriter(stringWriter), bson);
                //var json = stringWriter.ToString();
            }

            // Initialize the result variable
            FacedAggregate<TR> item = default;

            // Execute the aggregation operation and get the cursor
            var cursor = await this.MongoCollection.AggregateAsync<FacedAggregate<TR>>(bson);

            // Iterate over the cursor and get the first result
            while (await cursor.MoveNextAsync())
            {
                foreach (var c in cursor.Current)
                {
                    item = c;
                }
            }

            cursor.Dispose();

            // If the aggregation operation returned any documents, return the results, skip, limit, and total count
            if (item != default)
            {
                return (item.Result.ToArray(), aggregateOptions.Skip.Value,
                    aggregateOptions.Limit.Value, item.TotalRows());
            }

            // If the aggregation operation did not return any documents, return the default value
            return default;
        }

        /*public async Task<TR> Aggregate<TR>([NotNull] Query<TR> query,
            AggregateOptions<BsonDocument> aggregateOptions = default)
        {
            if (aggregateOptions == default)
                aggregateOptions = new AggregateOptions<BsonDocument>();

            if (!aggregateOptions.Skip.HasValue || aggregateOptions.Skip.Value < 0)
                throw new Exception("Skip is invalid or null.");

            if (!aggregateOptions.Limit.HasValue || aggregateOptions.Limit.Value <= 0)
                throw new Exception("Limit is invalid or null.");


            BsonDocument[] bsonDocumentFilter = query;
            List<BsonDocument> bsonDocumentFilterPaging = ((BsonDocument[])query).ToList();

            bsonDocumentFilterPaging.Add(
                new BsonDocument(new BsonElement("$skip", BsonValue.Create(aggregateOptions.Skip.Value))));
            bsonDocumentFilterPaging.Add(
                new BsonDocument(new BsonElement("$limit", BsonValue.Create(aggregateOptions.Limit.Value))));

            var structAggregate = "[ { \"$facet\": { \"result\": [],\"total\": [{\"$count\": \"total\"}]}} ]";

            var bson = BsonSerializer.Deserialize<BsonDocument[]>(structAggregate);

            bson[0][0][0] = new BsonArray(bsonDocumentFilterPaging);
            foreach (var it in new BsonArray(bsonDocumentFilter).Reverse()) ((BsonArray) bson[0][0][1]).Insert(0, it);

            if (Debugger.IsAttached)
            {
                // converter novamente em string para verificar se o json de consulta esta correto
                var stringWriter = new StringWriter();
                BsonSerializer.Serialize(new JsonWriter(stringWriter), bson);
                //var json = stringWriter.ToString();
            }

            FacedAggregate<TR> item = default;

            var cursor = await MongoCollection.AggregateAsync<FacedAggregate<TR>>(bson);

            while (await cursor.MoveNextAsync())
                foreach (var c in cursor.Current)
                    item = c;

            cursor.Dispose();

            if (item != default)
                return new PagedResult<TR>(item.Result.ToArray(), aggregateOptions.Skip.Value,
                    aggregateOptions.Limit.Value, item.TotalRows());

            return default;
        }*/

        #endregion

        /// <summary>
        /// Asynchronously performs a bulk write operation on the collection.
        /// </summary>
        /// <param name="writeModel">The list of write models to apply.</param>
        /// <param name="bulkWriteOptions">The options for the bulk write operation.</param>
        /// <returns>The total count of documents inserted, updated, matched, or deleted.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<long> BulkWriteAsync([NotNull] List<WriteModel<BsonDocument>> writeModel, [NotNull] Options.BulkWriteOptions bulkWriteOptions)
        {
            // Initialize the result variable
            BulkWriteResult result;

            //if (!bulkWriteOptions.NotPerformInTransaction || this._contextbase.IsUseTransaction)
            //{
            //    if (this._contextbase.Session == null)
            //    {
            //        result = await this.MongoCollection.BulkWriteAsync(writeModel, bulkWriteOptions);
            //    }

            //    result = await this.MongoCollection.BulkWriteAsync(this._contextbase.Session, writeModel, bulkWriteOptions);
            //}
            //else
            //{
            //    result = await this.MongoCollection.BulkWriteAsync(writeModel, bulkWriteOptions);
            //}

            // Check if the bulk write operation should not be performed in a transaction or if a transaction is in use
            if (bulkWriteOptions.NotPerformInTransaction || !this._contextbase.IsUseTransaction || this._contextbase.Session == null)
            {
                // Perform the bulk write operation without a session
                result = await this.MongoCollection.BulkWriteAsync(writeModel, bulkWriteOptions);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the bulk write operation with a session
                result = await this.MongoCollection.BulkWriteAsync(this._contextbase.Session, writeModel, bulkWriteOptions);
            }
            else
            {
                // Perform the bulk write operation without a session
                result = await this.MongoCollection.BulkWriteAsync(writeModel, bulkWriteOptions);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.BulkWriteAsync(_contextbase.Session, writeModel, bulkWriteOptions);
            //else
            //    result = await MongoCollection.BulkWriteAsync(writeModel, bulkWriteOptions);

            // Check if the result is default
            if (result == default)
            {
                return -1;
            }

            // Check if the bulk write operation was acknowledged
            if (result.IsAcknowledged)
            {
                // Return the total count of documents inserted, updated, matched, or deleted
                return result.DeletedCount + result.ModifiedCount + result.MatchedCount + result.InsertedCount;
            }

            // Return -1 if the bulk write operation was not acknowledged
            return -1;
            //return (result == default ? -1 : (result.IsAcknowledged ? result.DeletedCount : -1));
        }

        /// <summary>
        /// Implicitly converts a <see cref="RawDbSet"/> to a <see cref="MongoCollectionBase{BsonDocument}"/>.
        /// </summary>
        /// <param name="dbSet">The <see cref="RawDbSet"/> to convert.</param>
        /// <returns>The converted <see cref="MongoCollectionBase{BsonDocument}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator MongoCollectionBase<BsonDocument>(RawDbSet dbSet) => dbSet.MongoCollection as MongoCollectionBase<BsonDocument>;

        #region Dispose

        /// <summary>
        /// Disposes the object and releases any resources associated with it.
        /// </summary>
        public void Dispose()
        {
            // Call the Dispose method with a value of true to release managed resources.
            this.Dispose(true);

            // Suppress the finalizer for this object.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously disposes the object and releases any resources associated with it.
        /// </summary>
        /// <returns>A ValueTask representing the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            // Call the DisposeAsyncCore method to release any managed resources.
            await this.DisposeAsyncCore();

            // Call the Dispose method with a value of false to release unmanaged resources.
            this.Dispose(false);
        }

        /// <summary>
        /// Disposes the object and releases any resources associated with it.
        /// </summary>
        /// <param name="disposing">True if disposing, false otherwise.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check if we are disposing managed resources
            if (disposing)
            {
                // Dispose the session if it exists
                this._contextbase.Session?.Dispose();
            }
            //(_asyncDisposableResource as IDisposable)?.Dispose();

            //_contextbase.Session = null;
            // _asyncDisposableResource = null;
        }

        /// <summary>
        /// Asynchronously disposes the object and releases any resources associated with it.
        /// </summary>
        /// <returns>A ValueTask representing the asynchronous dispose operation.</returns>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            //if (_asyncDisposableResource is not null)
            //{
            //    await _asyncDisposableResource.DisposeAsync().ConfigureAwait(false);
            //}

            // ReSharper disable once SuspiciousTypeConversion.Global
            // Dispose the session if it is an IAsyncDisposable, otherwise dispose it synchronously
            if (this._contextbase.Session is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                this._contextbase.Session?.Dispose();
            }

            // Null out the async disposable resource and disposable resource
            //_asyncDisposableResource = null;
            //_disposableResource = null;
        }

        #endregion Dispose
    }
}
