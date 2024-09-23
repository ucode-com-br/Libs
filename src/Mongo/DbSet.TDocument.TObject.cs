using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Auth.AccessControlPolicy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    /// <summary>
    /// Represents a set of documents in a MongoDB collection.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <typeparam name="TObjectId">The type of the object ID.</typeparam>
    public class DbSet<TDocument, TObjectId> : IDisposable, IAsyncDisposable
        where TDocument : IObjectId<TObjectId>
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        /// <summary>
        /// The MongoDB collection associated with this DbSet.
        /// </summary>
        protected readonly IMongoCollection<TDocument> MongoCollection;

        /// <summary>
        /// The context associated with this DbSet.
        /// </summary>
        private readonly ContextBase _contextbase;

        /// <summary>
        /// The logger for this DbSet.
        /// </summary>
        protected ILogger<DbSet<TDocument, TObjectId>> Logger
        {
            get;
        }

        /// <summary>
        /// The name of the collection.
        /// </summary>
        public string CollectionName
        {
            get;
        }




        /// <summary>
        /// Initializes a new instance of the <see cref="DbSet{TDocument, TObjectId}"/> class.
        /// </summary>
        /// <param name="contextBase">The context base.</param>
        /// <param name="collectionName">The name of the collection.</param>
        /// <param name="timeSeriesOptions">The time series options.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet([NotNull] ContextBase contextBase, string? collectionName = null, Options.TimerSeriesOptions? timeSeriesOptions = null)
        {
            // If time series options are provided, create the collection if it doesn't exist
            if (timeSeriesOptions is not null)
            {
                if (contextBase.CollectionNames().FirstOrDefault(f => f.Equals(collectionName, StringComparison.Ordinal)) == default)
                {
                    contextBase.Database.CreateCollection(collectionName ?? $"{nameof(TDocument)}Collection", new CreateCollectionOptions()
                    {
                        TimeSeriesOptions = timeSeriesOptions,
                        ExpireAfter = TimeSpan.FromSeconds(timeSeriesOptions.ExpireAfterSeconds)
                    });
                }
            };

            CollectionName = collectionName ?? $"{nameof(TDocument)}Collection";

            // Initialize the MongoDB collection
            this.MongoCollection =
                contextBase.Database.GetCollection<TDocument>(CollectionName, new MongoCollectionSettings());

            // Set the context base
            this._contextbase = contextBase;

            // Initialize the logger
            this.Logger = contextBase.LoggerFactory.CreateLogger<DbSet<TDocument, TObjectId>>();

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
        public async IAsyncEnumerable<KeyValuePair<string, List<string>>> GetIndexesAsync(bool useSession = false)
        {
            IAsyncCursor<BsonDocument> idxsb = null;

            if (useSession)
            {
                idxsb = await this.MongoCollection.Indexes.ListAsync(session: this._contextbase.Session, default);
            }
            else
            {
                idxsb = await this.MongoCollection.Indexes.ListAsync();
            }

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
        /// Asynchronously creates multiple indexes in the collection.
        /// </summary>
        /// <param name="indexKeysDefinitions">A dictionary of index keys and their corresponding options.</param>
        /// <param name="useSession">Whether to use a session for the operation. Default is false.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask IndexAsync([NotNull] Dictionary<IndexKeysDefinition<TDocument>, CreateIndexOptions> indexKeysDefinitions, bool useSession = false)
        {
            // Create a list to hold the models
            var models = new List<CreateIndexModel<TDocument>>();

            // Iterate over the dictionary of index keys and options
            foreach (var indexKeysDefinition in indexKeysDefinitions)
            {
                // Create a new model with the index key and options
                models.Add(new CreateIndexModel<TDocument>(indexKeysDefinition.Key,
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
        /// Returns an IQueryable of type TDocument.
        /// </summary>
        /// <returns>An IQueryable of type TDocument.</returns>
        public IQueryable<TDocument> AsQueryable(AggregateOptions<TDocument>? aggregateOptions = null)
        {
            IQueryable<TDocument> queryable;

            AggregateOptions option = aggregateOptions ?? new AggregateOptions();

            // If the replace operation should not be performed in a transaction
            if (aggregateOptions != null && aggregateOptions.NotPerformInTransaction)
            {
                // Perform the replace operation without a session
                queryable = this.MongoCollection.AsQueryable(option);
            }
            // If a transaction is in use
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the replace operation with a session
                queryable = this.MongoCollection.AsQueryable(this._contextbase.Session, option);
            }
            // If no transaction is in use
            else
            {
                // Perform the replace operation without a session
                queryable = this.MongoCollection.AsQueryable(option);
            }

            return queryable;
        }

        #region FirstOrDefault

        /// <summary>
        /// Retrieves the first document that matches the specified query.
        /// </summary>
        /// <param name="query">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>The first document that matches the specified query, or the default value of TDocument if no matching document is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TDocument> FirstOrDefaultAsync([NotNull] Query<TDocument> query, Options.FindOptions findOptions = default)
        {
            // Convert the findOptions to the correct type
            var opt = (Options.FindOptions<TDocument, TDocument>)findOptions;

            // Convert the query to the correct type
            var qry = (Query<TDocument, TDocument>)query;

            // Get the first document that matches the query
            return await this.GetOneAsync(qry, opt);
        }

        /// <summary>
        /// Retrieves the first document that matches the specified query.
        /// </summary>
        /// <param name="query">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>The first document that matches the specified query, or the default value of TDocument if no matching document is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TDocument> FirstOrDefaultAsync([NotNull] Query<TDocument> query, Options.FindOptions<TDocument> findOptions = default)
        {
            // Convert the query to the correct type
            Options.FindOptions<TDocument, TDocument> opt = findOptions;

            // Convert the query to the correct type
            Query<TDocument, TDocument> qry = query;

            // Get the first document that matches the query
            return await this.GetOneAsync(qry, opt);
        }
        /// <summary>
        /// Retrieves the first document that matches the specified query.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="query">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>The first document that matches the specified query, or the default value of TDocument if no matching document is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TProjection> FirstOrDefaultAsync<TProjection>([NotNull] Query<TDocument, TProjection> query, Options.FindOptions<TDocument, TProjection> findOptions = default) => await this.GetOneAsync(query, findOptions);

        #endregion

        #region Any

        /// <summary>
        /// Checks if any document matches the specified query.
        /// </summary>
        /// <param name="query">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>True if any document matches the query, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<bool> AnyAsync([NotNull] Query<TDocument> query, Options.FindOptions<TDocument>? findOptions = default, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set default options if not provided
            findOptions ??= new Options.FindOptions<TDocument>();

            // Set limit and skip options for counting
            MongoDB.Driver.CountOptions countOptions = findOptions;
            countOptions.Limit = 1;
            countOptions.Skip = 0;

            // Count documents and check if count is greater than 0
            if (findOptions.NotPerformInTransaction)
            {
                return await this.MongoCollection.CountDocumentsAsync(query, countOptions, cancellationToken) > 0;
            }
            else if (this._contextbase.IsUseTransaction)
            {
                return await this.MongoCollection.CountDocumentsAsync(this._contextbase.Session, query, countOptions, cancellationToken) > 0;
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
        /// Retrieves a document from the database by its ID.
        /// </summary>
        /// <param name="id">The ID of the document to retrieve.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>The retrieved document, or the default value of TDocument if no matching document is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TDocument> GetAsync([NotNull] TObjectId id, [NotNull] Options.FindOptions findOptions = default)
        {
            // Convert the find options to the correct type
            Options.FindOptions<TDocument, TDocument> opt = findOptions;

            // Call the GetAsync method with the converted options
            return await this.GetAsync(id, opt);
        }

        /// <summary>
        /// Retrieves a document from the database by its ID.
        /// </summary>
        /// <param name="id">The ID of the document to retrieve.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>The retrieved document, or the default value of TDocument if no matching document is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TDocument> GetAsync([NotNull] TObjectId id, [NotNull] Options.FindOptions<TDocument> findOptions = default)
        {
            // Convert the find options to the correct type
            Options.FindOptions<TDocument, TDocument> opt = findOptions;

            // Call the GetAsync method with the converted options
            return await this.GetAsync(id, opt);
        }

        /// <summary>
        /// Retrieves a projection of a document from the database by its ID.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="id">The ID of the document to retrieve.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>The retrieved projection, or the default value of TProjection if no matching document is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TProjection> GetAsync<TProjection>([NotNull] TObjectId id,
            [NotNull] Options.FindOptions<TDocument, TProjection> findOptions = default)
        {
            // Create a query to find the document by its ID
            var qry = Query<TDocument, TProjection>.FromExpression(o => o.Id.Equals(id));

            // Call the GetOneAsync method with the query and options
            return await this.GetOneAsync(qry, findOptions);
        }

        /// <summary>
        /// Retrieves the first occurrence of a projection from the collection.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="query">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>The first occurrence of the projection, or the default value of TProjection if no matching document is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TProjection> GetOneAsync<TProjection>([NotNull] Query<TDocument, TProjection> query, [NotNull] Options.FindOptions<TDocument, TProjection> findOptions = default)
        {
            // Initialize the result to the default value of TProjection
            var result = default(TProjection);

            // Set default options if not provided
            findOptions ??= new Options.FindOptions<TDocument, TProjection>();

            // Set skip and limit options for finding the first occurrence
            findOptions.Skip ??= 0;
            findOptions.Limit ??= 1;

            // Create the filter definition from the query
            FilterDefinition<TDocument> filterSelected = query;

            // Create the options for the find operation
            MongoDB.Driver.FindOptions<TDocument, TProjection> options = findOptions;

            // Create the cursor for the find operation
            IAsyncCursor<TProjection> cursor;

            // If the find operation should not be performed in a transaction
            if (findOptions.NotPerformInTransaction)
            {
                // Perform the find operation with the filter
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the find operation with the session and filter
                cursor = await this.MongoCollection.FindAsync(this._contextbase.Session, filterSelected, options);
            }
            else
            {
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

            // Return the result
            return result;
        }
        #endregion

        /// <summary>
        /// Retrieves documents from the collection based on the provided array of IDs.
        /// </summary>
        /// <param name="ids">An array of IDs to search for.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the provided IDs.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> GetAsync([NotNull] TObjectId[] ids, Options.FindOptions? findOptions = default)
        {
            // Set default options if not provided
            findOptions ??= new Options.FindOptions();

            // Create a query to find documents with IDs in the provided array
            var qry = Query<TDocument, TDocument>.FromExpression(f => ids.Contains(f.Id));

            // Convert the findOptions to the correct type for the GetAsync method
            Options.FindOptions<TDocument, TDocument> opt = findOptions;

            // Iterate over the results of the GetAsync method and yield each document
            await foreach (var item in this.GetAsync<TDocument>(qry, opt))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Retrieves documents from the collection based on the provided array of IDs.
        /// </summary>
        /// <param name="ids">An array of IDs to search for.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the provided IDs.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> GetAsync([NotNull] TObjectId[] ids, Options.FindOptions<TDocument>? findOptions = default)
        {
            // Set default options if not provided
            findOptions ??= new Options.FindOptions<TDocument>();

            // Create a query to find documents with IDs in the provided array
            var qry = Query<TDocument, TDocument>.FromExpression(f => ids.Contains(f.Id));

            // Convert the findOptions to the correct type for the GetAsync method
            Options.FindOptions<TDocument, TDocument> opt = findOptions;

            // Iterate over the results of the GetAsync method and yield return each item
            await foreach (var item in this.GetAsync<TDocument>(qry, opt))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Retrieves documents from the collection based on the provided array of IDs.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="ids">An array of IDs to search for.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the provided IDs.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> GetAsync<TProjection>([NotNull] TObjectId[] ids, Options.FindOptions<TDocument, TProjection>? findOptions = default)
        {
            // Set default options if not provided
            findOptions ??= new Options.FindOptions<TDocument, TProjection>();

            // Create a query to find documents with IDs in the provided array
            var qry = Query<TDocument, TProjection>.FromExpression(f => ids.Contains(f.Id));

            // Iterate over the results of the GetAsync method and yield each document
            await foreach (var item in this.GetAsync<TProjection>(qry, findOptions))
            {
                yield return item;
            }
        }

        //public IEnumerable<TProjection> Get<TProjection>([NotNull] Query<TDocument, TProjection> filter, [NotNull] Options.FindOptions<TDocument, TProjection> findOptions)
        //{
        //    var enumerable = await GetAsync<TProjection>(filter, findOptions).ToIEnumerable();
        //    foreach (var item in enumerable)
        //    {
        //        yield return item;
        //    }
        //}

        /// <summary>
        /// Retrieves documents from the collection based on the provided query and find options.
        /// </summary>
        /// <param name="filter">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the provided query.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> GetAsync([NotNull] Query<TDocument> filter, [NotNull] Options.FindOptions? findOptions = default)
        {
            // Set default options if not provided
            findOptions ??= new Options.FindOptions();

            // Convert the query to the correct type for the GetAsync method
            Query<TDocument, TDocument> qry = filter;

            // Convert the findOptions to the correct type for the GetAsync method
            Options.FindOptions<TDocument, TDocument> opt = findOptions!;

            // Iterate over the results of the GetAsync method and yield return each item
            await foreach (var item in this.GetAsync(qry, opt))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Retrieves documents from the collection based on the provided query and find options.
        /// </summary>
        /// <param name="filter">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the provided query.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> GetAsync([NotNull] Query<TDocument> filter, [NotNull] Options.FindOptions<TDocument>? findOptions = default)
        {
            // If findOptions is null, create a new instance with default values
            findOptions ??= new Options.FindOptions<TDocument>();

            // Convert the filter to the correct type for the GetAsync method
            Query<TDocument, TDocument> qry = filter;

            // Convert the findOptions to the correct type for the GetAsync method
            Options.FindOptions<TDocument, TDocument> opt = findOptions;

            // Iterate over the results of the GetAsync method and yield each document
            await foreach (var item in this.GetAsync(qry, opt))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Asynchronously performs a full-text search on the collection.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="fullTextSearchOptions">Options for the full-text search.</param>
        /// <param name="filter">An optional filter to apply to the search results.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the search criteria.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> FulltextSearchAsync([NotNull] string text,
            [NotNull] Options.FullTextSearchOptions<TDocument> fullTextSearchOptions,
            [NotNull] Query<TDocument>? filter = default,
            [NotNull] Options.FindOptions<TDocument>? findOptions = default)
        {
            // Perform the full-text search and iterate over the results
            await foreach (var item in this.FulltextSearchAsync<TDocument>(text, fullTextSearchOptions, filter, findOptions))
            {
                // Yield return each item in the search results
                yield return item;
            }
        }

        /// <summary>
        /// Asynchronously performs a full-text search on the collection.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="text">The text to search for.</param>
        /// <param name="fullTextSearchOptions">Options for the full-text search.</param>
        /// <param name="filter">An optional filter to apply to the search results.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the search criteria.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> FulltextSearchAsync<TProjection>([NotNull] string text,
            [NotNull] Options.FullTextSearchOptions<TDocument> fullTextSearchOptions,
            [NotNull] Query<TDocument, TProjection>? filter = default,
            [NotNull] Options.FindOptions<TDocument, TProjection>? findOptions = default)
        {
            // Set default options if not provided
            findOptions ??= new Options.FindOptions<TDocument, TProjection>();

            // Create the filter definition from the text and full-text search options
            FilterDefinition<TDocument> filterSelected = Query<TDocument, TProjection>.FromText(text, fullTextSearchOptions);

            // If a filter is provided, add it to the filter definition
            if (filter != default)
            {
                filterSelected += filter;
            }

            // Create the options for the find operation
            MongoDB.Driver.FindOptions<TDocument, TProjection> options = findOptions;

            // Create the cursor for the find operation
            IAsyncCursor<TProjection> cursor;

            // If the find operation should not be performed in a transaction
            if (findOptions.NotPerformInTransaction)
            {
                // Perform the find operation with the filter
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the find operation with the session and filter
                cursor = await this.MongoCollection.FindAsync(this._contextbase.Session, filterSelected, options);
            }
            else
            {
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }

            // Iterate over the cursor and retrieve the search results
            while (await cursor.MoveNextAsync())
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

        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TProjection> Queryable<TProjection>(
            [NotNull] Expression<Func<TDocument, TProjection>> queryable, 
            [NotNull] Options.AggregateOptions<TDocument> aggregateOptions = default)
        {
            if (queryable == default)
                throw new ArgumentException($"Argument \"{nameof(queryable)}\" is null.");

            aggregateOptions ??= new Options.AggregateOptions<TDocument>();

            MongoDB.Driver.AggregateOptions options = aggregateOptions;

            //var build = queryable.Compile();
            //var linqProvider = MongoCollection.Database.Client.Settings.LinqProvider;
            //var t = Type.GetType("MongoDB.Driver.Linq.Linq3Implementation.LinqProviderV3");
            //var adapter = Activator.CreateInstance(t);
            //linqProvider.GetAdapter().AsQueryable(MongoCollection, _contextbase.Session, aggregateOptions);
            
            IMongoQueryable<TDocument> mongoIquerable = MongoCollection.AsQueryable(_contextbase.Session, options);

            var mongoQueryable = mongoIquerable.Provider.CreateQuery<TProjection>(queryable);

            //var projection = build.Invoke(mongoIquerable);

            return mongoQueryable;
        }*/

        //public IQueryable<TDocument> AsQueryable([NotNull] Options.AggregateOptions<TDocument> aggregateOptions = default)
        //{
        //    return MongoCollection.AsQueryable(_contextbase.Session, aggregateOptions);
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IQueryable<TDocument> AsQueryable(Func<IQueryable<TDocument>, IQueryable<TDocument>> preApprend, Options.AggregateOptions<TDocument>? aggregateOptions = default)
        {
            if (preApprend == default)
            {
                return this.AsQueryable(aggregateOptions);
            }
            else
            {
                return preApprend(this.AsQueryable(aggregateOptions));
            }
        }

        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TProjection> Queryable3<TProjection>(
            [NotNull] Expression<Func<TDocument, TProjection>> queryable,
            [NotNull] Options.AggregateOptions<TDocument> aggregateOptions = default)
        {
            if (queryable == default)
                throw new ArgumentException($"Argument \"{nameof(queryable)}\" is null.");

            aggregateOptions ??= new Options.AggregateOptions<TDocument>();

            MongoDB.Driver.AggregateOptions options = aggregateOptions;

            //var build = queryable.Compile();
            //var linqProvider = MongoCollection.Database.Client.Settings.LinqProvider;
            //var t = Type.GetType("MongoDB.Driver.Linq.Linq3Implementation.LinqProviderV3");
            //var adapter = (LinqProvider)Activator.CreateInstance(t);

            var linqProviderAdapterV3Type= Type.GetType("MongoDB.Driver.Linq.Linq3Implementation.LinqProviderAdapterV3");
            var linqProviderAdapterV3 = Activator.CreateInstance(linqProviderAdapterV3Type);
            var querableMethod = linqProviderAdapterV3Type.GetMethod("AsQueryable");
            IMongoQueryable<TDocument> mongoIquerable = (IMongoQueryable<TDocument>)querableMethod.MakeGenericMethod(typeof(TDocument)).Invoke(linqProviderAdapterV3, new object[] { MongoCollection, _contextbase.Session, aggregateOptions });

            
            //MongoDB.Driver.Linq.Linq3Implementation.LinqProviderAdapterV3 >> MongoDB.Driver.Linq.LinqProviderAdapter
            //linqProvider.GetAdapter().AsQueryable(MongoCollection, _contextbase.Session, aggregateOptions);

            //IMongoQueryable<TDocument> mongoIquerable = MongoCollection.AsQueryable(_contextbase.Session, options);

            var mongoQueryable = mongoIquerable.Provider.CreateQuery<TProjection>(queryable);

            //var projection = build.Invoke(mongoIquerable);

            return mongoQueryable;
        }
        */

        /// <summary>
        /// Retrieves documents from the collection based on the provided query and find options.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="filter">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the provided query.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> GetAsync<TProjection>([NotNull] Query<TDocument, TProjection> filter, [NotNull] Options.FindOptions<TDocument, TProjection>? findOptions = default)
        {
            // Set default options if not provided
            findOptions ??= new Options.FindOptions<TDocument, TProjection>();

            // Create the filter definition from the query
            FilterDefinition<TDocument> filterSelected = filter;

            // Create the options for the find operation
            MongoDB.Driver.FindOptions<TDocument, TProjection> options = findOptions;

            // Create the cursor for the find operation
            IAsyncCursor<TProjection> cursor;

            // If the find operation should not be performed in a transaction
            if (findOptions.NotPerformInTransaction)
            {
                // Perform the find operation with the filter
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the find operation with the session and filter
                cursor = await this.MongoCollection.FindAsync(this._contextbase.Session, filterSelected, options);
            }
            else
            {
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }

            // Iterate over the cursor and retrieve the search results
            while (await cursor.MoveNextAsync())
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
        /// Retrieves a paged result of documents from the collection based on the provided query and find options.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="filter">The query to match documents against.</param>
        /// <param name="findOptions">Options for the find operation. Default is null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paged result of documents matching the provided query.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the filter or findOptions parameter is null.</exception>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<PagedResult<TProjection>> GetPagedAsync<TProjection>([NotNull] Query<TDocument> filter, [NotNull] Options.FindOptionsPaging<TDocument>? findOptions)
        {
            // Convert the filter to the correct type for the GetPagedAsync method
            Query<TDocument, TProjection> qry = filter;

            // Convert the findOptions to the correct type for the GetPagedAsync method
            Options.FindOptionsPaging<TDocument, TProjection> opt = findOptions;

            // Call the GetPagedAsync method with the converted filter and findOptions
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
        public async Task<PagedResult<TProjection>> GetPagedAsync<TProjection>([NotNull] Query<TDocument, TProjection> filter, [NotNull] Options.FindOptionsPaging<TDocument, TProjection>? findOptions)
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
            FilterDefinition<TDocument> filterSelected = filter;

            // Create the options for the find operation
            MongoDB.Driver.FindOptions<TDocument, TProjection> options = findOptions;

            // Create the count options from the find options and set the limit and skip options to null
            var countOptions = findOptions.CopyTo<Options.IOptions, Options.CountOptions>();
            countOptions.Limit = null;
            countOptions.Skip = null;

            // Asynchronously count the number of documents that match the specified filter
            var total = Convert.ToInt32(await this.CountDocumentsAsync((FilterDefinition<TDocument>)filter, countOptions));

            IAsyncCursor<TProjection> cursor;

            // If the find operation should not be performed in a transaction
            if (findOptions.NotPerformInTransaction)
            {
                // Perform the find operation with the filter
                this.Logger.LogDebug($"Call \"this.MongoCollection.FindAsync(...)\" without session, filter: \"{filterSelected}\" and options: {options.JsonString()}");
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the find operation with the session and filter
                this.Logger.LogDebug($"Call \"this.MongoCollection.FindAsync(...)\" with session, filter: \"{filterSelected}\" and options: {options.JsonString()}");
                cursor = await this.MongoCollection.FindAsync(this._contextbase.Session, filterSelected, options);
            }
            else
            {
                // Perform the find operation with the filter
                this.Logger.LogDebug($"Call \"this.MongoCollection.FindAsync(...)\" without session, filter: \"{filterSelected}\" and options: {options.JsonString()}");
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }

            // Create an array to hold the items
            var itens = new TProjection[(options != null && options.Limit.HasValue && options.Limit.Value < total) ? options.Limit.Value : total];

            var lastPos = 0;
            // Iterate over the cursor and retrieve the items
            while (await cursor.MoveNextAsync())
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
            return new PagedResult<TProjection>(itens, Convert.ToInt32(findOptions.CurrentPage), Convert.ToInt32(findOptions.PageSize), total);
        }

        #endregion

        #region FindOneAndUpdateAsync

        /// <summary>
        /// Asynchronously finds a single document and updates it.
        /// </summary>
        /// <param name="query">The query to match the document against.</param>
        /// <param name="options">The options for the find and update operation.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated document.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TDocument> FindOneAndUpdateAsync([NotNull] Query<TDocument> query,
            [NotNull] Options.FindOneAndUpdateOptions<TDocument> options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create a copy of the options to avoid modifying the original
            MongoDB.Driver.FindOneAndUpdateOptions<TDocument> fouOptions = options;

            // Declare a variable to hold the result
            TDocument result;

            // Get the filter and update definitions from the query
            FilterDefinition<TDocument> filter = query;
            UpdateDefinition<TDocument> update = _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(query.Update);

            // If the operation should not be performed in a transaction
            if (options.NotPerformInTransaction)
            {
                // Perform the find and update operation without a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions, cancellationToken);
            }
            // If a transaction is in use
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the find and update operation with a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(this._contextbase.Session, filter, update, fouOptions, cancellationToken);
            }
            // If no transaction is in use
            else
            {
                // Perform the find and update operation without a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions, cancellationToken);
            }

            // Return the updated document
            return result;
        }

        /// <summary>
        /// Asynchronously finds a single document and updates it based on the specified filter and update.
        /// </summary>
        /// <param name="filter">The filter to apply to the documents.</param>
        /// <param name="update">The update to apply to the documents.</param>
        /// <param name="options">The options for the find and update operation.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated document, or null if no document matches the filter.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TDocument> FindOneAndUpdateAsync([NotNull] string filter, [NotNull] string update,
            [NotNull] Options.FindOneAndUpdateOptions<TDocument> options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create a copy of the options to avoid modifying the original
            MongoDB.Driver.FindOneAndUpdateOptions<TDocument> fouOptions = options;

            // Declare a variable to hold the result
            TDocument result;

            // If the operation should not be performed in a transaction
            if (options.NotPerformInTransaction)
            {
                // Perform the find and update operation without a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(update), fouOptions, cancellationToken);
            }
            // If a transaction is in use
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the find and update operation with a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(this._contextbase.Session, filter, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(update), fouOptions, cancellationToken);
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
        /// Asynchronously finds a single document and updates it based on the specified filter and update definition.
        /// </summary>
        /// <param name="filter">The filter to match documents against.</param>
        /// <param name="update">The update definition to apply to the matched document.</param>
        /// <param name="options">The options for the find and update operation. Default is null.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated document, or null if no document matches the filter.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TDocument> FindOneAndUpdateAsync(
           [NotNull] string filter,
           [NotNull] PipelineUpdateDefinition<TDocument> update,
           [NotNull] Options.FindOneAndUpdateOptions<TDocument>? options = default,
           CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create a copy of the options to avoid modifying the original
            MongoDB.Driver.FindOneAndUpdateOptions<TDocument> fouOptions = options ?? new Options.FindOneAndUpdateOptions<TDocument>();

            // Declare a variable to hold the result
            TDocument result;

            // If the operation should not be performed in a transaction
            if (options.NotPerformInTransaction)
            {
                // Perform the find and update operation without a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(update), fouOptions, cancellationToken);
            }
            // If a transaction is in use
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the find and update operation with a session
                result = await this.MongoCollection.FindOneAndUpdateAsync(this._contextbase.Session, filter, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(update), fouOptions, cancellationToken);
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

        #endregion FindOneAndUpdateAsync

        #region UpdateManyAsync

        /// <summary>
        /// Updates multiple documents in the collection that match the specified query.
        /// </summary>
        /// <param name="query">The query to match the documents against.</param>
        /// <param name="options">The options for the update operation. Default is null.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of modified documents, or -1 if the operation was not acknowledged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> UpdateManyAsync([NotNull] Query<TDocument> query,
            [NotNull] UpdateOptions<TDocument> options, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Initialize the result to null
            UpdateResult result;

            // If a transaction is not in use and the operation is not set to not perform in a transaction
            if (options.NotPerformInTransaction)
            {
                // Perform the update operation without a session
                result = await this.MongoCollection.UpdateManyAsync(query, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(query.Update), options, cancellationToken);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the update operation without a session
                result = await this.MongoCollection.UpdateManyAsync(this._contextbase.Session, query, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(query.Update), options, cancellationToken);
            }
            else
            {
                result = await this.MongoCollection.UpdateManyAsync(query, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(query.Update), options, cancellationToken);
            }

            // Return the number of modified documents, or -1 if the operation was not acknowledged
            return result.IsAcknowledged ? result.ModifiedCount : -1;
        }

        /// <summary>
        /// Updates multiple documents in the collection that match the specified filter and update.
        /// </summary>
        /// <param name="filter">The filter to match the documents against.</param>
        /// <param name="update">The update to apply to the matched documents.</param>
        /// <param name="options">The options for the update operation. Default is null.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of modified documents, or -1 if the operation was not acknowledged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> UpdateManyAsync([NotNull] string filter, [NotNull] string update,
            [NotNull] UpdateOptions<TDocument> options, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Initialize the result to null
            UpdateResult result;

            // If the operation should not be performed in a transaction
            if (options.NotPerformInTransaction)
            {
                // Perform the update operation without a session
                result = await this.MongoCollection.UpdateManyAsync(filter, update, options, cancellationToken);
            }
            // If a transaction is in use
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the update operation with a session
                result = await this.MongoCollection.UpdateManyAsync(this._contextbase.Session, filter, update, options, cancellationToken);
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
        /// Asynchronously counts the number of documents in the collection that match the specified query.
        /// </summary>
        /// <param name="query">The query to match documents against.</param>
        /// <param name="countOptions">Options for the count operation. Default is null.</param>
        /// <returns>The number of documents that match the query.</returns>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> CountDocumentsAsync([NotNull] Query<TDocument> query, Options.CountOptions? countOptions = default)
        {
            // If countOptions is null, create a new instance with default values
            countOptions ??= new Options.CountOptions();

            // Convert the query to a FilterDefinition and call the CountDocumentsAsync method with it
            return await this.CountDocumentsAsync((FilterDefinition<TDocument>)query, countOptions);
        }

        /// <summary>
        /// Asynchronously counts the number of documents in the collection that match the specified query.
        /// </summary>
        /// <param name="preApprend">A function that takes an IQueryable of TDocument and returns an IQueryable of TDocument.</param>
        /// <param name="aggregateOptions">Options for the aggregate operation. Default is null.</param>
        /// <returns>The number of documents that match the query.</returns>
        /// <exception cref="ArgumentNullException">Thrown if preApprend is null.</exception>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> CountDocumentsAsync([NotNull] Func<IQueryable<TDocument>, IQueryable<TDocument>> preApprend, Options.AggregateOptions<TDocument>? aggregateOptions = default)
        {
            // Create a queryable from the MongoCollection using the session and default aggregate options
            var query = this.MongoCollection.AsQueryable(this._contextbase.Session, new AggregateOptions());

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
        /// Asynchronously counts the number of documents in the collection that match the specified filter.
        /// </summary>
        /// <param name="filterDefinition">The filter to match the documents against.</param>
        /// <param name="countOptions">Options for the count operation. Default is null.</param>
        /// <returns>The number of documents that match the filter.</returns>
        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<long> CountDocumentsAsync([NotNull] FilterDefinition<TDocument> filterDefinition, Options.CountOptions? countOptions = default)
        {
            // If countOptions is null, create a new instance with default values
            countOptions ??= new Options.CountOptions();

            // If the operation should not be performed in a transaction
            if (countOptions.NotPerformInTransaction)
            {
                // If a transaction is in use
                return await this.MongoCollection.CountDocumentsAsync(filterDefinition, countOptions);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the count operation without a session
                return await this.MongoCollection.CountDocumentsAsync(this._contextbase.Session, filterDefinition, countOptions);
            }
            else
            {
                return await this.MongoCollection.CountDocumentsAsync(filterDefinition, countOptions);
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// Asynchronously updates a document in the collection by adding an element to a set.
        /// </summary>
        /// <param name="query">The query to match the document against.</param>
        /// <param name="updateOptions">Options for the update operation. Default is null.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of modified documents, or -1 if the operation was not acknowledged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> UpdateAddToSetAsync([NotNull] Query<TDocument> query,
            UpdateOptions<TDocument>? updateOptions = default, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // If updateOptions is null, create a new instance with default values
            updateOptions ??= new UpdateOptions<TDocument>();


            // Call the UpdateAsync method with the query, update, and options
            return await this.UpdateAsync(query, _contextbase.BeforeUpdateInternal<TDocument, TObjectId>(query.Update), updateOptions, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates a document in the collection that matches the specified filter.
        /// </summary>
        /// <param name="filterDefinition">The filter to match the document against.</param>
        /// <param name="updateDefinition">The update to apply to the document.</param>
        /// <param name="updateOptions">Options for the update operation.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of modified documents, or -1 if the operation was not acknowledged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<long> UpdateAsync(
            [NotNull] FilterDefinition<TDocument> filterDefinition,
            [NotNull] UpdateDefinition<TDocument> updateDefinition,
            [NotNull] Options.UpdateOptions updateOptions,
            CancellationToken cancellationToken = default)
        {
            // Initialize the result to null
            UpdateResult result;


            // Respect first wise if the operation should not be performed in a transaction
            if (updateOptions.NotPerformInTransaction)
            {
                // Perform the update operation without a session
                result = await this.MongoCollection.UpdateOneAsync(filterDefinition, updateDefinition, updateOptions, cancellationToken);
            }
            // If a transaction is in use
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the update operation with a session
                result = await this.MongoCollection.UpdateOneAsync(this._contextbase.Session, filterDefinition, updateDefinition, updateOptions, cancellationToken);
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
        /// Inserts a single document into the collection.
        /// </summary>
        /// <param name="source">The document to insert.</param>
        /// <param name="insertOneOptions">Options for the insert operation. Default is null.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of inserted documents, which should always be 1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> InsertAsync([NotNull] TDocument source, Options.InsertOneOptions? insertOneOptions = default, CancellationToken cancellationToken = default)
        {
            // If no options were provided, create a new default options object
            insertOneOptions ??= new Options.InsertOneOptions();

            // Create a list to hold the write model for the insert operation
            var writeModels = new List<WriteModel<TDocument>>
            {
                new InsertOneModel<TDocument>(source)
            };

            // Respect first wise if the operation should not be performed in a transaction
            if (insertOneOptions.NotPerformInTransaction)
            {
                // Perform the update operation without a session
                await this.MongoCollection.InsertOneAsync(_contextbase.BeforeInsertInternal<TDocument, TObjectId>(source), insertOneOptions, cancellationToken);
            }
            // If a transaction is in use
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the update operation with a session
                await this.MongoCollection.InsertOneAsync(this._contextbase.Session, _contextbase.BeforeInsertInternal<TDocument, TObjectId>(source), insertOneOptions, cancellationToken);
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
        /// Inserts multiple documents into the collection.
        /// </summary>
        /// <param name="docs">The documents to insert.</param>
        /// <param name="insertManyOptions">Options for the insert operation. Default is null.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of inserted documents.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> InsertAsync([NotNull] IEnumerable<TDocument> docs, Options.InsertManyOptions? insertManyOptions = default, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // If no options were provided, create a new default options object
            insertManyOptions ??= new Options.InsertManyOptions();

            // Create a list to hold the write models for the insert operation
            var writeModels = new List<WriteModel<TDocument>>();

            // Add a write model for each document to insert
            foreach (var doc in docs)
            {
                writeModels.Add(new InsertOneModel<TDocument>(_contextbase.BeforeInsertInternal<TDocument, TObjectId>(doc)));
            }

            // Perform the insert operation using the write models and options
            return await this.BulkWriteAsync(writeModels, insertManyOptions, cancellationToken);
        }

        #endregion

        #region Replace

        /// <summary>
        /// Replaces multiple documents in the collection.
        /// </summary>
        /// <param name="docs">The documents to replace.</param>
        /// <param name="query">The query to match the documents against. Default is null.</param>
        /// <param name="replaceOptions">Options for the replace operation. Default is null.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of replaced documents.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> ReplaceAsync([NotNull] IEnumerable<TDocument> docs, Query<TDocument>? query = null, ReplaceOptions<TDocument>? replaceOptions = default, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // If no options were provided, create a new default options object
            replaceOptions ??= new ReplaceOptions<TDocument>();

            var updates = new List<WriteModel<TDocument>>();

            Expression<Func<TDocument, TDocument, bool>> exp = (item, constrain) => item.Id.Equals(constrain.Id);

            // Create a filter definition for each document
            foreach (var doc in docs)
            {
                FilterDefinition<TDocument> filterDefinition = (query ?? exp).CompleteExpression(_contextbase.BeforeReplaceInternal<TDocument, TObjectId>(doc));

                // Create a replace one model with the filter definition and the document
                var model = new ReplaceOneModel<TDocument>(filterDefinition, doc)
                {
                    IsUpsert = replaceOptions.IsUpsert,
                    Collation = replaceOptions.Collation,
                    Hint = replaceOptions.Hint
                };

                updates.Add(model);
            }

            // Create bulk write options with the provided replace options or a new default options object
            Options.BulkWriteOptions bulkWriteOptions = replaceOptions;
            bulkWriteOptions.IsOrdered = false;

            // Perform the bulk write operation with the update models and options
            return await this.BulkWriteAsync(updates, bulkWriteOptions, cancellationToken);
        }

        /// <summary>
        /// Replaces a document in the collection.
        /// </summary>
        /// <param name="doc">The document to replace.</param>
        /// <param name="replaceOptions">Options for the replace operation. Default is null.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of replaced documents.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> ReplaceAsync([NotNull] TDocument doc, ReplaceOptions<TDocument>? replaceOptions = default, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Log the method call with the document ID and options
            this.Logger.LogDebug($"Called {nameof(ReplaceAsync)}(...), with id: \"{doc.Id}\" and options: \"{(replaceOptions != null ? replaceOptions.JsonString() : "NULL")}\"");

            // Get the ID of the document
            var id = doc.Id;

            // Create a query to match the document by ID
            var query = Query<TDocument>.FromExpression(d => d.Id.Equals(id));

            // Create a JSON string to find the document by ID
            var jsonFind = $"{{ \"_id\": {{\"$eq\": \"{doc.Id}\"}} }}";

            // Call the ReplaceAsync method with the query, document, and options
            return await this.ReplaceAsync(query, _contextbase.BeforeReplaceInternal<TDocument, TObjectId>(doc), replaceOptions, cancellationToken);
            //return await this.ReplaceAsync(Query<TDocument>.FromExpression(f => f.Id.Equals(doc.Id)), doc, replaceOptions);
        }

        /// <summary>
        /// Replaces a document in the collection.
        /// </summary>
        /// <param name="query">The query to match the document against. If null, the document will be matched by its ID.</param>
        /// <param name="doc">The document to replace.</param>
        /// <param name="replaceOptions">Options for the replace operation. Default is null.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of replaced documents. If the operation was not acknowledged, -1 is returned.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> ReplaceAsync([NotNull] Query<TDocument> query, [NotNull] TDocument doc, ReplaceOptions<TDocument>? replaceOptions = default, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set default replace options if not provided
            replaceOptions ??= new ReplaceOptions<TDocument>();

            // Initialize the result to null
            ReplaceOneResult result;

            // Create a filter definition to match the document
            FilterDefinition<TDocument> filterDefinition = query ?? Query<TDocument>.FromExpression(f => f.Id.Equals(doc.Id));

            // If the replace operation should not be performed in a transaction
            if (replaceOptions.NotPerformInTransaction)
            {
                // Perform the replace operation without a session
                result = await this.MongoCollection.ReplaceOneAsync(filterDefinition, _contextbase.BeforeReplaceInternal<TDocument, TObjectId>(doc), replaceOptions, cancellationToken);
            }
            // If a transaction is in use
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the replace operation with a session
                result = await this.MongoCollection.ReplaceOneAsync(this._contextbase.Session, filterDefinition, _contextbase.BeforeInsertInternal<TDocument, TObjectId>(doc), replaceOptions, cancellationToken);
            }
            // If no transaction is in use
            else
            {
                // Perform the replace operation without a session
                result = await this.MongoCollection.ReplaceOneAsync(filterDefinition, _contextbase.BeforeInsertInternal<TDocument, TObjectId>(doc), replaceOptions, cancellationToken);
            }


#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.

            // Return the number of replaced documents, or -1 if the operation was not acknowledged
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
        public async ValueTask<long> DeleteOneAsync([NotNull] TObjectId id, DeleteOptions<TDocument>? deleteOptions = default, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set default delete options if not provided
            deleteOptions ??= new Options.DeleteOptions<TDocument>();

            // Delete the document by its ID
            return await this.DeleteAsync([id], deleteOptions, cancellationToken);
        }

        /// <summary>
        /// Deletes multiple documents from the collection by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the documents to delete.</param>
        /// <param name="deleteOptions">Options for the delete operation. Default is null.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of deleted documents. If the operation was not acknowledged, -1 is returned.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> DeleteAsync([NotNull] IEnumerable<TObjectId> ids,
            DeleteOptions<TDocument>? deleteOptions = default, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set default delete options if not provided
            deleteOptions ??= new DeleteOptions<TDocument>();

            // Create a list to hold the write models for the delete operations
            var listWriteModel = new List<WriteModel<TDocument>>();

            // Iterate over the IDs and create delete write models for each ID
            foreach (var id in ids)
            {
                // Create a filter definition to match the document by its ID
                Expression<Func<TDocument, bool>> exp = (f) => f.Id.Equals(id);

                FilterDefinition<TDocument> filterDefinition = exp;

                // Create a delete write model with the filter definition and delete options
                var model = new DeleteOneModel<TDocument>(filterDefinition)
                {
                    Collation = deleteOptions.Collation,
                    Hint = deleteOptions.Hint
                };

                // Add the write model to the list
                listWriteModel.Add(model);
            }

            // Create bulk write options with the delete options
            Options.BulkWriteOptions bulkWriteOptions = deleteOptions;
            bulkWriteOptions.IsOrdered = false;
            bulkWriteOptions.BypassDocumentValidation = true;

            // Perform the bulk write operation with the write models and options
            return await this.BulkWriteAsync(listWriteModel, bulkWriteOptions, cancellationToken);
        }

        /// <summary>
        /// Deletes multiple documents from the collection that match the specified query.
        /// </summary>
        /// <param name="query">The query to match the documents against.</param>
        /// <param name="deleteOptions">Options for the delete operation. Default is null.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of deleted documents. If the operation was not acknowledged, -1 is returned.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> DeleteAsync([NotNull] Query<TDocument> query,
            DeleteOptions<TDocument>? deleteOptions = default, CancellationToken cancellationToken = default)
        {
            // Set default delete options if not provided
            deleteOptions ??= new DeleteOptions<TDocument>();

            // Create a list to hold the write model for the delete operation
            var listWriteModel = new List<WriteModel<TDocument>> { new DeleteManyModel<TDocument>(query) };

            // Create bulk write options with the delete options
            Options.BulkWriteOptions bulkWriteOptions = deleteOptions;
            bulkWriteOptions.IsOrdered = false;
            bulkWriteOptions.BypassDocumentValidation = true;

            // Perform the bulk write operation with the update models and options
            return await this.BulkWriteAsync(listWriteModel, bulkWriteOptions, cancellationToken);
        }

        #endregion

        #region Aggregate
        /// <summary>
        /// Asynchronously performs an aggregation operation on the collection.
        /// </summary>
        /// <param name="query">The query to match the documents against.</param>
        /// <param name="aggregateOptions">Options for the aggregation operation. Default is null.</param>
        /// <returns>An asynchronous enumerable of documents matching the aggregation criteria.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TDocument> AggregateFacetEnumerableAsync([NotNull] Query<TDocument> query,
            AggregateOptions<TDocument>? aggregateOptions = default)
        {
            // Set default aggregate options if not provided
            aggregateOptions ??= new AggregateOptions<TDocument>();

            // Perform the aggregation operation and iterate over the results
            foreach (var item in (await this.AggregateFacetAsync(query, aggregateOptions)).result)
            {
                // Yield return each item in the aggregation results
                yield return item;
            }
        }

        /// <summary>
        /// Asynchronously performs an aggregation operation on the collection and returns the result.
        /// </summary>
        /// <param name="query">The query to match the documents against.</param>
        /// <param name="aggregateOptions">Options for the aggregation operation. Default is null.</param>
        /// <returns>A tuple containing the result of the aggregation operation, the skip value, the limit value, and the total number of rows.</returns>
        /// <exception cref="Exception">Thrown if the skip or limit options are invalid or null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<(IEnumerable<TDocument> result, int skip, int limit, int total)> AggregateFacetAsync([NotNull] Query<TDocument> query,
            AggregateOptions<TDocument>? aggregateOptions = default)
        {
            // Set default aggregate options if not provided
            aggregateOptions ??= new AggregateOptions<TDocument>();

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
            var cursor = await this.MongoCollection.AggregateAsync<FacedAggregate<TDocument>>(bson);

            while (await cursor.MoveNextAsync())
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
                return (item.Result.ToArray(), aggregateOptions.Skip.Value,
                    aggregateOptions.Limit.Value, item.TotalRows());
            }

            // If there is no result, return the default value
            return default;
        }

        /// <summary>
        /// Asynchronously performs an aggregation operation on the collection and returns the result.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="query">The query to match the documents against.</param>
        /// <param name="aggregateOptions">Options for the aggregation operation. Default is null.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the result of the aggregation operation.</returns>
        public async Task<IReadOnlyList<TProjection?>> AggregateAsync<TProjection>([NotNull] Query<TDocument, TProjection> query,
            AggregateOptions<TDocument>? aggregateOptions = default, CancellationToken cancellationToken = default)
        {
            var result = new List<TProjection?>();

            await foreach (var item in this.AggregateEnumerableAsync<TProjection>(query, aggregateOptions, cancellationToken))
            {
                result.Add(item);
            }

            return result.AsReadOnly();
        }

        /// <summary>
        /// Asynchronously performs an aggregation operation on the collection and returns the result.
        /// </summary>
        /// <typeparam name="TDocument"> The type of the result.</typeparam>
        /// <typeparam name="TProjection"> The type of the projection.</typeparam>
        /// <param name="query"> The query to match the documents against. </param>
        /// <param name="aggregateOptions"> Options for the aggregation operation. Default is null.</param>
        /// <param name="cancellationToken"></param>
        /// <returns> A task that represents the asynchronous operation. The task result contains the result of the aggregation operation. </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection?> AggregateEnumerableAsync<TProjection>([NotNull] Query<TDocument, TProjection> query,
            AggregateOptions<TDocument>? aggregateOptions = default, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set default aggregate options if not provided
            aggregateOptions ??= new AggregateOptions<TDocument>();


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

            // Perform the aggregation operation and iterate over the results
            IList<TProjection?> item = default;

            IAsyncCursor<TProjection> cursor = default!;

            if (aggregateOptions.NotPerformInTransaction)
            {
                // Create a cursor for the aggregation operation
                cursor = await this.MongoCollection.AggregateAsync<TProjection>(bsonDocumentFilter, aggregateOptions, cancellationToken);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                // Create a cursor for the aggregation operation with the session and filter
                cursor = await this.MongoCollection.AggregateAsync<TProjection>(this._contextbase.Session, bsonDocumentFilter, aggregateOptions, cancellationToken);
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
                    cancellationToken.ThrowIfCancellationRequested();

                    yield return c;
                }
            }

            // Dispose the cursor
            cursor.Dispose();
        }


        #endregion

        /// <summary>
        /// Asynchronously performs a bulk write operation on the collection.
        /// </summary>
        /// <param name="writeModel">The list of write models to apply.</param>
        /// <param name="bulkWriteOptions">The options for the bulk write operation.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The total count of documents inserted, updated, matched, or deleted.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<long> BulkWriteAsync([NotNull] List<WriteModel<TDocument>> writeModel, [NotNull] Options.BulkWriteOptions bulkWriteOptions, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Perform the bulk write operation based on the provided options
            BulkWriteResult result;

            // If the bulk write operation should not be performed in a transaction or if a transaction is in use
            if (bulkWriteOptions.NotPerformInTransaction)
            {
                // Perform the bulk write operation without a session
                result = await this.MongoCollection.BulkWriteAsync(writeModel, bulkWriteOptions, cancellationToken);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                // Perform the bulk write operation with a session
                result = await this.MongoCollection.BulkWriteAsync(this._contextbase.Session, writeModel, bulkWriteOptions, cancellationToken);
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

            // Return -1 if the operation was not acknowledged
            return -1;
            //return (result == default ? -1 : (result.IsAcknowledged ? result.DeletedCount : -1));
        }

        /// <summary>
        /// Converts a DbSet to a MongoCollectionBase of TDocument.
        /// </summary>
        /// <param name="dbSet">The DbSet to convert.</param>
        /// <returns>The MongoCollectionBase of TDocument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator MongoCollectionBase<TDocument>(DbSet<TDocument, TObjectId> dbSet) => (dbSet.MongoCollection as MongoCollectionBase<TDocument>)!;

        /// <summary>
        /// Converts a DbSet back to parent context.
        /// </summary>
        /// <param name="dbSet">The DbSet to convert.</param>
        /// <returns>The ContextBase of current DbSet.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ContextBase(DbSet<TDocument, TObjectId> dbSet) => dbSet._contextbase;

        #region Dispose

        /// <summary>
        /// Disposes the DbSet.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the DbSet asynchronously.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await this.DisposeAsyncCore();

            this.Dispose(false);
        }

        /// <summary>
        /// Disposes the DbSet.
        /// </summary>
        /// <param name="disposing">True if disposing, false otherwise.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._contextbase.Session?.Dispose();
            }
            //(_asyncDisposableResource as IDisposable)?.Dispose();

            //_contextbase.Session = null;
            // _asyncDisposableResource = null;
        }

        /// <summary>
        /// Disposes the DbSet asynchronously.
        /// </summary>
        /// <returns>A ValueTask representing the asynchronous dispose operation.</returns>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            //if (_asyncDisposableResource is not null)
            //{
            //    await _asyncDisposableResource.DisposeAsync().ConfigureAwait(false);
            //}

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (this._contextbase.Session is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                this._contextbase.Session?.Dispose();
            }

            //_asyncDisposableResource = null;
            //_disposableResource = null;
        }

        #endregion Dispose
    }
}
