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
    public class RawDbSet : IDisposable, IAsyncDisposable
    {
        protected readonly IMongoCollection<BsonDocument> MongoCollection;
        private readonly ContextBase _contextbase;
        protected ILogger<RawDbSet> Logger
        {
            get;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawDbSet([NotNull] ContextBase contextBase, string collectionName, Options.TimerSeriesOptions timeSeriesOptions = null)
        {
            if (timeSeriesOptions is not null)
            {
                if (ContextBase.CollectionNames().FirstOrDefault(f => f.Equals(collectionName, StringComparison.Ordinal)) == default)
                {
                    contextBase.Database.CreateCollection(collectionName, new CreateCollectionOptions()
                    {
                        TimeSeriesOptions = timeSeriesOptions,
                        ExpireAfter = TimeSpan.FromSeconds(timeSeriesOptions.ExpireAfterSeconds)
                    });
                }
            };

            this.MongoCollection =
                contextBase.Database.GetCollection<BsonDocument>(collectionName, new MongoCollectionSettings());

            this._contextbase = contextBase;

            this.Logger = contextBase.LoggerFactory.CreateLogger<RawDbSet>();

            //this.SetLogger(_ => _.Logger);
        }

        /// <summary>
        /// Necessita maiores testes
        /// </summary>
        /// <returns>Lista o nome dos indices e os campos indexados por cada indice da collection</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<KeyValuePair<string, List<string>>> GetIndexesAsync()
        {
            var idxsb = await this.MongoCollection.Indexes.ListAsync(session: this._contextbase.Session, default);

            while (idxsb.MoveNext())
            {
                var idxs = idxsb.Current;

                foreach (var idx in idxs)
                {
                    var name = idx["name"].AsString;
                    var keysList = new List<string>();

                    foreach (var keys in idx["key"].AsBsonArray)
                    {
                        foreach (var item in keys.AsBsonDocument.Elements)
                        {
                            keysList.Add(item.Name);
                        }
                    }

                    yield return new KeyValuePair<string, List<string>>(name, keysList);
                }
            }

        }


        /// <summary>
        /// Cria o indice da collection
        /// </summary>
        /// <param name="indexKeysDefinitions">Definiçoes de indices</param>
        /// <param name="useSession">Aponta o uso de sessão usando session</param>
        /// <returns>void</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask IndexAsync([NotNull] Dictionary<IndexKeysDefinition<BsonDocument>, CreateIndexOptions> indexKeysDefinitions, bool useSession = false)
        {
            var models = new List<CreateIndexModel<BsonDocument>>();

            foreach (var indexKeysDefinition in indexKeysDefinitions)
            {
                models.Add(new CreateIndexModel<BsonDocument>(indexKeysDefinition.Key,
                    indexKeysDefinition.Value ?? new CreateIndexOptions() { }));
            }

            if (useSession)
            {
                _ = await this.MongoCollection.Indexes.CreateManyAsync(this._contextbase.Session, models);
            }
            else
            {
                _ = await this.MongoCollection.Indexes.CreateManyAsync(models);
            }
        }

        /// <summary>
        /// IQuerable com tipo definido
        /// </summary>
        /// <returns>Retorna o IQuerable do tipo da collection</returns>
        public IQueryable<BsonDocument> AsIQueryable() => this.MongoCollection.AsQueryable();

        #region FirstOrDefault

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<BsonDocument> FirstOrDefaultAsync([NotNull] Query<BsonDocument> query, Options.FindOptions findOptions = default)
        {
            var opt = (Options.FindOptions<BsonDocument, BsonDocument>)findOptions;
            var qry = (Query<BsonDocument, BsonDocument>)query;

            return await this.GetOneAsync(qry, opt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<BsonDocument> FirstOrDefaultAsync([NotNull] Query<BsonDocument> query, Options.FindOptions<BsonDocument> findOptions = default)
        {
            Options.FindOptions<BsonDocument, BsonDocument> opt = findOptions;
            Query<BsonDocument, BsonDocument> qry = query;

            return await this.GetOneAsync(qry, opt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TProjection> FirstOrDefaultAsync<TProjection>([NotNull] Query<BsonDocument, TProjection> query, Options.FindOptions<BsonDocument, TProjection> findOptions = default) => await this.GetOneAsync(query, findOptions);

        #endregion

        #region Any
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<bool> AnyAsync([NotNull] Query<BsonDocument> query, Options.FindOptions<BsonDocument> findOptions = default)
        {
            findOptions ??= new Options.FindOptions<BsonDocument>();

            CountOptions countOptions = findOptions;

            countOptions.Limit = 1;
            countOptions.Skip = 0;

            if (findOptions.NotPerformInTransaction)
            {
                return await this.MongoCollection.CountDocumentsAsync(this._contextbase.Session, query, countOptions) > 0;
            }

            return await this.MongoCollection.CountDocumentsAsync(query, countOptions) > 0;
        }

        #endregion

        #region Get

        #region Get One

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<BsonDocument> GetAsync([NotNull] ObjectId id, [NotNull] Options.FindOptions findOptions = default)
        {
            Options.FindOptions<BsonDocument, BsonDocument> opt = findOptions;

            return await this.GetAsync(id, opt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<BsonDocument> GetAsync([NotNull] ObjectId id, [NotNull] Options.FindOptions<BsonDocument> findOptions = default)
        {
            Options.FindOptions<BsonDocument, BsonDocument> opt = findOptions;

            return await this.GetAsync(id, opt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<TProjection> GetAsync<TProjection>([NotNull] ObjectId id,
            [NotNull] Options.FindOptions<BsonDocument, TProjection> findOptions = default)
        {
            var qry = Query<BsonDocument, TProjection>.FromExpression(o => o["_id"].AsObjectId.Equals(id));

            return await this.GetOneAsync(qry, findOptions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// Get first occurrence
        /// </summary>
        /// <typeparam name="TProjection">Projection class</typeparam>
        /// <param name="query">Filter definition</param>
        /// <param name="findOptions">Find options</param>
        /// <returns></returns>
        public async ValueTask<TProjection> GetOneAsync<TProjection>([NotNull] Query<BsonDocument, TProjection> query, [NotNull] Options.FindOptions<BsonDocument, TProjection> findOptions = default)
        {
            var result = default(TProjection);

            findOptions ??= new Options.FindOptions<BsonDocument, TProjection>();
            findOptions.Skip ??= 0;
            findOptions.Limit ??= 1;

            FilterDefinition<BsonDocument> filterSelected = query;

            FindOptions<BsonDocument, TProjection> options = findOptions;

            IAsyncCursor<TProjection> cursor;

            if (findOptions.NotPerformInTransaction)
            {
                cursor = await this.MongoCollection.FindAsync(this._contextbase.Session, filterSelected, options);
            }
            else
            {
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }

            while (await cursor.MoveNextAsync())
            {
                foreach (var item in cursor.Current)
                {
                    result = item;
                    break;
                }

            }

            cursor.Dispose();

            return result;
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<BsonDocument> GetAsync([NotNull] ObjectId[] ids, Options.FindOptions findOptions = default)
        {
            findOptions ??= new Options.FindOptions();

            
            var qry = Query<BsonDocument, BsonDocument>.FromExpression(f => ids.Contains(f["_id"].AsObjectId));
            Options.FindOptions<BsonDocument, BsonDocument> opt = findOptions;

            await foreach (var item in this.GetAsync<BsonDocument>(qry, opt))
            {
                yield return item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<BsonDocument> GetAsync([NotNull] ObjectId[] ids, Options.FindOptions<BsonDocument> findOptions = default)
        {
            findOptions ??= new Options.FindOptions<BsonDocument>();

            var qry = Query<BsonDocument, BsonDocument>.FromExpression(f => ids.Contains(f["_id"].AsObjectId));
            Options.FindOptions<BsonDocument, BsonDocument> opt = findOptions;

            await foreach (var item in this.GetAsync<BsonDocument>(qry, opt))
            {
                yield return item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> GetAsync<TProjection>([NotNull] ObjectId[] ids, Options.FindOptions<BsonDocument, TProjection> findOptions = default)
        {
            findOptions ??= new Options.FindOptions<BsonDocument, TProjection>();

            var qry = Query<BsonDocument, TProjection>.FromExpression(f => ids.Contains(f["_id"].AsObjectId));

            await foreach (var item in this.GetAsync<TProjection>(qry, findOptions))
            {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<BsonDocument> GetAsync([NotNull] Query<BsonDocument> filter, [NotNull] Options.FindOptions findOptions = default)
        {
            findOptions ??= new Options.FindOptions();

            Query<BsonDocument, BsonDocument> qry = filter;
            Options.FindOptions<BsonDocument, BsonDocument> opt = findOptions;

            await foreach (var item in this.GetAsync(qry, opt))
            {
                yield return item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<BsonDocument> GetAsync([NotNull] Query<BsonDocument> filter, [NotNull] Options.FindOptions<BsonDocument> findOptions = default)
        {
            findOptions ??= new Options.FindOptions<BsonDocument>();

            Query<BsonDocument, BsonDocument> qry = filter;

            Options.FindOptions<BsonDocument, BsonDocument> opt = findOptions;

            await foreach (var item in this.GetAsync(qry, opt))
            {
                yield return item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<BsonDocument> FulltextSearchAsync([NotNull] string text,
            [NotNull] Options.FullTextSearchOptions<BsonDocument> fullTextSearchOptions,
            [NotNull] Query<BsonDocument> filter = default,
            [NotNull] Options.FindOptions<BsonDocument> findOptions = default)
        {
            await foreach (var item in this.FulltextSearchAsync<BsonDocument>(text, fullTextSearchOptions, filter, findOptions))
            {
                yield return item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> FulltextSearchAsync<TProjection>([NotNull] string text,
            [NotNull] Options.FullTextSearchOptions<BsonDocument> fullTextSearchOptions,
            [NotNull] Query<BsonDocument, TProjection> filter = default,
            [NotNull] Options.FindOptions<BsonDocument, TProjection> findOptions = default)
        {
            findOptions ??= new Options.FindOptions<BsonDocument, TProjection>();

            FilterDefinition<BsonDocument> filterSelected = Query<BsonDocument, TProjection>.FromText(text, fullTextSearchOptions);

            if (filter != default)
            {
                filterSelected += filter;
            }

            FindOptions<BsonDocument, TProjection> options = findOptions;

            IAsyncCursor<TProjection> cursor;


            if (findOptions.NotPerformInTransaction)
            {
                cursor = await this.MongoCollection.FindAsync(this._contextbase.Session, filterSelected, options);
            }
            else
            {
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }

            while (await cursor.MoveNextAsync())
            {
                foreach (var item in cursor.Current)
                {
                    if (item != null)
                    {
                        yield return item;
                    }
                }
            }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<TProjection> GetAsync<TProjection>([NotNull] Query<BsonDocument, TProjection> filter, [NotNull] Options.FindOptions<BsonDocument, TProjection> findOptions = default)
        {
            findOptions ??= new Options.FindOptions<BsonDocument, TProjection>();

            FilterDefinition<BsonDocument> filterSelected = filter;

            FindOptions<BsonDocument, TProjection> options = findOptions;

            IAsyncCursor<TProjection> cursor;
            if (findOptions.NotPerformInTransaction)
            {
                cursor = await this.MongoCollection.FindAsync(this._contextbase.Session, filterSelected, options);
            }
            else
            {
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }

            while (await cursor.MoveNextAsync())
            {
                foreach (var item in cursor.Current)
                {
                    if (item != null)
                    {
                        yield return item;
                    }
                }
            }

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

        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<PagedResult<TProjection>> GetPagedAsync<TProjection>([NotNull] Query<BsonDocument, TProjection> filter, [NotNull] Options.FindOptionsPaging<BsonDocument, TProjection> findOptions)
        {
            var fstr = filter.ToString();
            var fostr = findOptions.ToString();
            var fojson = findOptions.JsonString();

            this.Logger.LogDebug($"Called \"{nameof(GetPagedAsync)}(...)\" with args (filter: \"{filter}\", findOptions: \"{fojson}\")");

            if (findOptions.PageSize <= 0)
            {
                throw new ArgumentException("Page size is invalid or null.");
            }

            if (findOptions.CurrentPage < 0)
            {
                throw new ArgumentException("Current page is invalid or null.");
            }




            #region find

            FilterDefinition<BsonDocument> filterSelected = filter;

            FindOptions<BsonDocument, TProjection> options = findOptions;

            var countOptions = findOptions.CopyTo<Options.IOptions, Options.CountOptions>();
            countOptions.Limit = null;
            countOptions.Skip = null;
            var total = Convert.ToInt32(await this.CounBsonDocumentsAsync((FilterDefinition<BsonDocument>)filter, countOptions));

            IAsyncCursor<TProjection> cursor;

            if (findOptions.NotPerformInTransaction)
            {
                this.Logger.LogDebug($"Call \"this.MongoCollection.FindAsync(...)\" with session, filter: \"{filterSelected}\" and options: {options.JsonString()}");
                cursor = await this.MongoCollection.FindAsync(this._contextbase.Session, filterSelected, options);
            }
            else
            {
                this.Logger.LogDebug($"Call \"this.MongoCollection.FindAsync(...)\" without session, filter: \"{filterSelected}\" and options: {options.JsonString()}");
                cursor = await this.MongoCollection.FindAsync(filterSelected, options);
            }

            var itens = new TProjection[options.Limit < total ? options.Limit.Value : total];

            var trPos = 0;

            var lastPos = 0;

            while (await cursor.MoveNextAsync())
            {
                foreach (var item in cursor.Current)
                {
                    lastPos = trPos++;
                    if (item != null)
                    {
                        itens[lastPos] = item;
                    }
                }
            }

            cursor.Dispose();

            if (lastPos + 1 < itens.Length)
            {
                Array.Resize(ref itens, lastPos);
            }

            #endregion find


            return new PagedResult<TProjection>(itens, Convert.ToInt32(findOptions.CurrentPage), Convert.ToInt32(findOptions.PageSize), total);
        }

        #endregion

        #region FindOneAndUpdateAsync

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<BsonDocument> FindOneAndUpdateAsync([NotNull] Query<BsonDocument> query,
            [NotNull] Options.FindOneAndUpdateOptions<BsonDocument> options)
        {
            FindOneAndUpdateOptions<BsonDocument> fouOptions = options;

            BsonDocument result;

            FilterDefinition<BsonDocument> filter = query;

            UpdateDefinition<BsonDocument> update = query.Update;

            if (options.NotPerformInTransaction)
            {
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                result = await this.MongoCollection.FindOneAndUpdateAsync(this._contextbase.Session, filter, update, fouOptions);
            }
            else
            {
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.FindOneAndUpdateAsync(_contextbase.Session, filter, update, fouOptions);
            //else
            //    result = await MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<BsonDocument> FindOneAndUpdateAsync([NotNull] string filter, [NotNull] string update,
            [NotNull] Options.FindOneAndUpdateOptions<BsonDocument> options)
        {
            FindOneAndUpdateOptions<BsonDocument> fouOptions = options;

            BsonDocument result;

            if (options.NotPerformInTransaction)
            {
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                result = await this.MongoCollection.FindOneAndUpdateAsync(this._contextbase.Session, filter, update, fouOptions);
            }
            else
            {
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.FindOneAndUpdateAsync(_contextbase.Session, filter, update, fouOptions);
            //else
            //    result = await MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<BsonDocument> FindOneAndUpdateAsync(
           [NotNull] string filter,
           [NotNull] PipelineUpdateDefinition<BsonDocument> update,
           [NotNull] Options.FindOneAndUpdateOptions<BsonDocument> options = default)
        {
            FindOneAndUpdateOptions<BsonDocument> fouOptions = options;

            BsonDocument result;

            if (options.NotPerformInTransaction)
            {
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                result = await this.MongoCollection.FindOneAndUpdateAsync(this._contextbase.Session, filter, update, fouOptions);
            }
            else
            {
                result = await this.MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.FindOneAndUpdateAsync(_contextbase.Session, filter, update, fouOptions);
            //else
            //    result = await MongoCollection.FindOneAndUpdateAsync(filter, update, fouOptions);

            return result;
        }

        #endregion FindOneAndUpdateAsync

        #region UpdateManyAsync

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> UpdateManyAsync([NotNull] Query<BsonDocument> query,
            [NotNull] Options.UpdateOptions<BsonDocument> options)
        {
            UpdateResult result;

            if (!this._contextbase.IsUseTransaction && !options.NotPerformInTransaction)
            {
                result = await this.MongoCollection.UpdateManyAsync(query, query.Update, options);
            }
            else
            {
                result = await this.MongoCollection.UpdateManyAsync(this._contextbase.Session, query, query.Update, options);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.UpdateManyAsync(_contextbase.Session, query, query.Update, options);
            //else
            //    result = await MongoCollection.UpdateManyAsync(query, query.Update, options);

            return result.IsAcknowledged ? result.ModifiedCount : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> UpdateManyAsync([NotNull] string filter, [NotNull] string update,
            [NotNull] Options.UpdateOptions<BsonDocument> options)
        {
            UpdateResult result;

            if (options.NotPerformInTransaction)
            {
                result = await this.MongoCollection.UpdateManyAsync(filter, update, options);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                result = await this.MongoCollection.UpdateManyAsync(this._contextbase.Session, filter, update, options);
            }
            else
            {
                result = await this.MongoCollection.UpdateManyAsync(filter, update, options);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.UpdateManyAsync(_contextbase.Session, filter, update, options);
            //else
            //    result = await MongoCollection.UpdateManyAsync(filter, update, options);

            return result.IsAcknowledged ? result.ModifiedCount : -1;
        }

        #endregion UpdateManyAsync

        #region Count

        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> CounBsonDocumentsAsync([NotNull] Query<BsonDocument> query, Options.CountOptions countOptions = default)
        {
            countOptions ??= new Options.CountOptions();

            return await this.CounBsonDocumentsAsync((FilterDefinition<BsonDocument>)query, countOptions);
        }

        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> CounBsonDocumentsAsync([NotNull] Func<IQueryable<BsonDocument>, IQueryable<BsonDocument>> preApprend, Options.AggregateOptions<BsonDocument> aggregateOptions = default)
        {
            var query = this.MongoCollection.AsQueryable(this._contextbase.Session, new AggregateOptions());

            var queryAppended = preApprend.Invoke(query);

            var count = queryAppended.LongCount();

            await Task.CompletedTask;

            return count;
        }

        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<long> CounBsonDocumentsAsync([NotNull] FilterDefinition<BsonDocument> filterDefinition, Options.CountOptions countOptions = default)
        {
            countOptions ??= new Options.CountOptions();

            if (countOptions.NotPerformInTransaction)
            {
                return await this.MongoCollection.CountDocumentsAsync(this._contextbase.Session, filterDefinition, countOptions);
            }

            return await this.MongoCollection.CountDocumentsAsync(filterDefinition, countOptions);
        }

        #endregion

        #region Update

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> UpdateAddToSetAsync([NotNull] Query<BsonDocument> query,
            Options.UpdateOptions<BsonDocument> updateOptions = default)
        {
            updateOptions ??= new Options.UpdateOptions<BsonDocument>();

            return await this.UpdateAsync(query, query.Update, updateOptions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<long> UpdateAsync(
            [NotNull] FilterDefinition<BsonDocument> filterDefinition,
            [NotNull] UpdateDefinition<BsonDocument> updateDefinition,
            [NotNull] Options.UpdateOptions updateOptions)
        {
            UpdateResult result;



            if (updateOptions.NotPerformInTransaction)
            {
                result = await this.MongoCollection.UpdateOneAsync(filterDefinition, updateDefinition, updateOptions);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                result = await this.MongoCollection.UpdateOneAsync(this._contextbase.Session, filterDefinition, updateDefinition, updateOptions);
            }
            else
            {
                result = await this.MongoCollection.UpdateOneAsync(filterDefinition, updateDefinition, updateOptions);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.UpdateOneAsync(_contextbase.Session, filterDefinition, updateDefinition, updateOptions);
            //else
            //    result = await MongoCollection.UpdateOneAsync(filterDefinition, updateDefinition, updateOptions);

            return result == default ? -1 : result.IsAcknowledged ? result.ModifiedCount : -1;
        }


        #endregion

        #region Insert

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> InsertAsync([NotNull] BsonDocument source, Options.InsertOneOptions insertOneOptions = default)
        {
            insertOneOptions ??= new Options.InsertOneOptions();

            var writeModels = new List<WriteModel<BsonDocument>>
            {
                new InsertOneModel<BsonDocument>(source)
            };




            ///return await this.BulkWriteAsync(writeModels, insertOneOptions);
            if ((this._contextbase.Session != null && this._contextbase.Session.IsInTransaction) || insertOneOptions.NotPerformInTransaction)
            {
                await this.MongoCollection.InsertOneAsync(this._contextbase.Session, source, insertOneOptions);
            }
            else
            {
                await this.MongoCollection.InsertOneAsync(source, insertOneOptions);
            }

            return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> InsertAsync([NotNull] IEnumerable<BsonDocument> docs, Options.InsertManyOptions insertManyOptions = default)
        {
            insertManyOptions ??= new Options.InsertManyOptions();

            var writeModels = new List<WriteModel<BsonDocument>>();

            foreach (var doc in docs)
            {
                writeModels.Add(new InsertOneModel<BsonDocument>(doc));
            }


            return await this.BulkWriteAsync(writeModels, insertManyOptions);
            //if (insertManyOptions.UseSession)
            //    await MongoCollection.InsertManyAsync(_contextbase.Session, docs, insertManyOptions);
            //else
            //    await MongoCollection.InsertManyAsync(docs, insertManyOptions);
        }

        #endregion

        #region Replace

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> ReplaceAsync([NotNull] IEnumerable<BsonDocument> docs, Query<BsonDocument>? query = null, Options.ReplaceOptions<BsonDocument>? replaceOptions = default)
        {
            replaceOptions ??= new Options.ReplaceOptions<BsonDocument>();

            var updates = new List<WriteModel<BsonDocument>>();


            Expression<Func<BsonDocument, BsonDocument, bool>> exp = (item, constrain) => item["_id"].AsObjectId.Equals(constrain["_id"].AsObjectId);

            foreach (var doc in docs)
            {
                //Expression<Func<BsonDocument, bool>> exp = f => f.Id.Equals(doc.Id);
                
                //ExpressionFilterDefinition<BsonDocument>
                FilterDefinition<BsonDocument> filterDefinition = (query ?? exp).CompleteExpression(doc);

                var model = new ReplaceOneModel<BsonDocument>(filterDefinition, doc)
                {
                    IsUpsert = replaceOptions.IsUpsert,
                    Collation = replaceOptions.Collation,
                    Hint = replaceOptions.Hint
                };

                updates.Add(model);
            }

            Options.BulkWriteOptions bulkWriteOptions = replaceOptions;
            bulkWriteOptions.IsOrdered = false;


            return await this.BulkWriteAsync(updates, bulkWriteOptions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> ReplaceAsync([NotNull] BsonDocument doc, Options.ReplaceOptions<BsonDocument> replaceOptions = default)
        {
            this.Logger.LogDebug($"Called {nameof(ReplaceAsync)}(...), with id: \"{doc["_id"].AsObjectId.ToString()}\" and options: \"{(replaceOptions != null ? replaceOptions.JsonString() : "NULL")}\"");

            var id = doc["_id"].AsObjectId;

            var query = Query<BsonDocument>.FromExpression(d => d["_id"].AsObjectId.Equals(id));

            var jsonFind = $"{{ \"_id\": {{\"$eq\": \"{doc["_id"].AsObjectId}\"}} }}";

            return await this.ReplaceAsync(query, doc, replaceOptions);
            //return await this.ReplaceAsync(Query<BsonDocument>.FromExpression(f => f.Id.Equals(doc.Id)), doc, replaceOptions);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> ReplaceAsync([NotNull] Query<BsonDocument> query, [NotNull] BsonDocument doc, Options.ReplaceOptions<BsonDocument> replaceOptions = default)
        {
            replaceOptions ??= new Options.ReplaceOptions<BsonDocument>();

            ReplaceOneResult result;

            FilterDefinition<BsonDocument> filterDefinition = query ?? Query<BsonDocument>.FromExpression(f => f["_id"].AsObjectId.Equals(doc["_id"].AsObjectId));

            if (replaceOptions.NotPerformInTransaction)
            {
                result = await this.MongoCollection.ReplaceOneAsync(filterDefinition, doc, replaceOptions);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                result = await this.MongoCollection.ReplaceOneAsync(this._contextbase.Session, filterDefinition, doc, replaceOptions);
            }
            else
            {
                result = await this.MongoCollection.ReplaceOneAsync(filterDefinition, doc, replaceOptions);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.ReplaceOneAsync(_contextbase.Session, filterDefinition, doc, replaceOptions);
            //else
            //    result = await MongoCollection.ReplaceOneAsync(filterDefinition, doc, replaceOptions);

#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
            return result == null ? -1 : result.IsAcknowledged ? result.ModifiedCount : -1;
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.
        }

        #endregion

        #region Delete

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> DeleteOneAsync([NotNull] ObjectId id, Options.DeleteOptions<BsonDocument> deleteOptions = default)
        {
            deleteOptions ??= new Options.DeleteOptions<BsonDocument>();

            return await this.DeleteAsync(new[] { id }, deleteOptions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> DeleteAsync([NotNull] IEnumerable<ObjectId> ids,
            Options.DeleteOptions<BsonDocument> deleteOptions = default)
        {
            deleteOptions ??= new Options.DeleteOptions<BsonDocument>();

            var updates = new List<WriteModel<BsonDocument>>();

            foreach (var id in ids)
            {
                Expression<Func<BsonDocument, bool>> exp = (f) => f["_id"].AsObjectId.Equals(id);

                FilterDefinition<BsonDocument> filterDefinition = exp;

                var model = new DeleteOneModel<BsonDocument>(filterDefinition)
                {
                    Collation = deleteOptions.Collation,
                    Hint = deleteOptions.Hint
                };

                updates.Add(model);
            }

            Options.BulkWriteOptions bulkWriteOptions = deleteOptions;
            bulkWriteOptions.IsOrdered = false;
            bulkWriteOptions.BypassDocumentValidation = true;

            return await this.BulkWriteAsync(updates, bulkWriteOptions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<long> DeleteAsync([NotNull] Query<BsonDocument> query,
            Options.DeleteOptions<BsonDocument> deleteOptions = default)
        {
            deleteOptions ??= new Options.DeleteOptions<BsonDocument>();

            var updates = new List<WriteModel<BsonDocument>> { new DeleteManyModel<BsonDocument>(query) };

            Options.BulkWriteOptions bulkWriteOptions = deleteOptions;
            bulkWriteOptions.IsOrdered = false;
            bulkWriteOptions.BypassDocumentValidation = true;

            return await this.BulkWriteAsync(updates, bulkWriteOptions);
        }

        #endregion

        #region Aggregate

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async IAsyncEnumerable<BsonDocument> Aggregate([NotNull] Query<BsonDocument> query,
            Options.AggregateOptions<BsonDocument> aggregateOptions = default)
        {
            aggregateOptions ??= new Options.AggregateOptions<BsonDocument>();

            foreach (var item in (await this.Aggregate<BsonDocument>(query, aggregateOptions)).Item1)
            {
                yield return item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<(IEnumerable<TR>, int, int, int)> Aggregate<TR>([NotNull] Query<TR> query,
            Options.AggregateOptions<BsonDocument> aggregateOptions = default)
        {
            aggregateOptions ??= new Options.AggregateOptions<BsonDocument>();

            if (!aggregateOptions.Skip.HasValue || aggregateOptions.Skip.Value < 0)
            {
                throw new Exception("Skip is invalid or null.");
            }

            if (!aggregateOptions.Limit.HasValue || aggregateOptions.Limit.Value <= 0)
            {
                throw new Exception("Limit is invalid or null.");
            }

            BsonDocument[] bsonDocumentFilter = query;
            var bsonDocumentFilterPaging = ((BsonDocument[])query).ToList();

            bsonDocumentFilterPaging.Add(
                new BsonDocument(new BsonElement("$skip", BsonValue.Create(aggregateOptions.Skip.Value))));
            bsonDocumentFilterPaging.Add(
                new BsonDocument(new BsonElement("$limit", BsonValue.Create(aggregateOptions.Limit.Value))));

            var structAggregate = /*lang=json,strict*/ "[ { \"$facet\": { \"result\": [],\"total\": [{\"$count\": \"total\"}]}} ]";

            var bson = BsonSerializer.Deserialize<BsonDocument[]>(structAggregate);

            bson[0][0][0] = new BsonArray(bsonDocumentFilterPaging);
            foreach (var it in new BsonArray(bsonDocumentFilter).Reverse())
            {
                ((BsonArray)bson[0][0][1]).Insert(0, it);
            }

            if (Debugger.IsAttached)
            {
                // converter novamente em string para verificar se o json de consulta esta correto
                var stringWriter = new StringWriter();
                BsonSerializer.Serialize(new JsonWriter(stringWriter), bson);
                //var json = stringWriter.ToString();
            }

            FacedAggregate<TR> item = default;

            var cursor = await this.MongoCollection.AggregateAsync<FacedAggregate<TR>>(bson);

            while (await cursor.MoveNextAsync())
            {
                foreach (var c in cursor.Current)
                {
                    item = c;
                }
            }

            cursor.Dispose();

            if (item != default)
            {
                return (item.Result.ToArray(), aggregateOptions.Skip.Value,
                    aggregateOptions.Limit.Value, item.TotalRows());
            }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask<long> BulkWriteAsync([NotNull] List<WriteModel<BsonDocument>> writeModel, [NotNull] Options.BulkWriteOptions bulkWriteOptions)
        {
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


            if (bulkWriteOptions.NotPerformInTransaction || !this._contextbase.IsUseTransaction || this._contextbase.Session == null)
            {
                result = await this.MongoCollection.BulkWriteAsync(writeModel, bulkWriteOptions);
            }
            else if (this._contextbase.IsUseTransaction)
            {
                result = await this.MongoCollection.BulkWriteAsync(this._contextbase.Session, writeModel, bulkWriteOptions);
            }
            else
            {
                result = await this.MongoCollection.BulkWriteAsync(writeModel, bulkWriteOptions);
            }

            //if (_contextbase.IsUseTransaction)
            //    result = await MongoCollection.BulkWriteAsync(_contextbase.Session, writeModel, bulkWriteOptions);
            //else
            //    result = await MongoCollection.BulkWriteAsync(writeModel, bulkWriteOptions);

            if (result == default)
            {
                return -1;
            }

            if (result.IsAcknowledged)
            {
                return result.DeletedCount + result.ModifiedCount + result.MatchedCount + result.InsertedCount;
            }

            return -1;
            //return (result == default ? -1 : (result.IsAcknowledged ? result.DeletedCount : -1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator MongoCollectionBase<BsonDocument>(RawDbSet dbSet) => dbSet.MongoCollection as MongoCollectionBase<BsonDocument>;

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
                this._contextbase.Session?.Dispose();
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
