using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    /// <summary>
    /// Find options for <see cref="IMongoCollection{TDocument}.Find{TProjection}(FilterDefinition{TDocument}, MongoDB.Driver.FindOptions{TDocument, TProjection})"/>
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    /// <typeparam name="TProjection"></typeparam>
    public record FindOptions<TDocument, TProjection> : FindOptionsBase
    {

        /// <summary>
        /// Hint to use in query
        /// </summary>
        public Query<TDocument>? Hint
        {
            get; set;
        }

        /// <summary>
        /// Minimum itens in result set
        /// </summary>
        public Query<TDocument>? Min
        {
            get; set;
        }


        /// <summary>
        /// Maximum itens in result set
        /// </summary>
        public Query<TDocument>? Max
        {
            get; set;
        }


        /// <summary>
        /// Sortting query
        /// </summary>
        public Query<TDocument>? Sort
        {
            get; set;
        }

        /// <summary>
        /// Projection query to select fields to return
        /// </summary>
        public Query<TDocument, TProjection>? Projection
        {
            get; set;
        }

        /// <summary>
        /// Converts <see cref="MongoDB.Driver.FindOptions{TDocument, TProjection}"/> to <see cref="FindOptions{TDocument, TProjection}"/>
        /// </summary>
        /// <param name="source"></param>
        public static implicit operator FindOptions<TDocument, TProjection>(FindOptions<TDocument> source)
        {
            var result = new FindOptions<TDocument, TProjection>
            {
                Collation = source.Collation,
                Hint = source.Hint,
                AllowDiskUse = source.AllowDiskUse,
                AllowPartialResults = source.AllowPartialResults,
                MaxTime = source.MaxTime,
                BatchSize = source.BatchSize,
                Comment = source.Comment,
                CursorType = source.CursorType,
                MaxAwaitTime = source.MaxAwaitTime,
                NoCursorTimeout = source.NoCursorTimeout,
                ReturnKey = source.ReturnKey,
                ShowRecordId = source.ShowRecordId,
                Max = source.Max,
                Min = source.Min,
                Skip = source.Skip,
                Limit = source.Limit,
                Sort = source.Sort
            };

            return result;
        }

        /// <summary>
        /// Converts <see cref="MongoDB.Driver.FindOptions{TDocument, TProjection}"/> to <see cref="FindOptions{TDocument, TProjection}"/>
        /// </summary>
        /// <param name="source"></param>
        public static implicit operator FindOptions<TDocument, TProjection>?(FindOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new FindOptions<TDocument, TProjection>
            {
                Collation = source.Collation,
                AllowDiskUse = source.AllowDiskUse,
                AllowPartialResults = source.AllowPartialResults,
                MaxTime = source.MaxTime,
                BatchSize = source.BatchSize,
                Comment = source.Comment,
                CursorType = source.CursorType,
                MaxAwaitTime = source.MaxAwaitTime,
                NoCursorTimeout = source.NoCursorTimeout,
                ReturnKey = source.ReturnKey,
                ShowRecordId = source.ShowRecordId,
                Skip = source.Skip,
                Limit = source.Limit,
            };

            return result;
        }

        /// <summary>
        /// Converts <see cref="FindOptions{TDocument, TProjection}"/> to <see cref="MongoDB.Driver.FindOptions{TDocument, TProjection}"/>
        /// </summary>
        /// <param name="source"></param>
        public static implicit operator MongoDB.Driver.FindOptions<TDocument, TProjection>?(FindOptions<TDocument, TProjection> source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.FindOptions<TDocument, TProjection>
            {
                Collation = source.Collation,
                Hint = source.Hint,
                AllowDiskUse = source.AllowDiskUse,
                AllowPartialResults = source.AllowPartialResults,
                MaxTime = source.MaxTime,
                BatchSize = source.BatchSize,
                Comment = source.Comment,
                CursorType = (CursorType)source.CursorType,
                MaxAwaitTime = source.MaxAwaitTime,
                NoCursorTimeout = source.NoCursorTimeout,
                ReturnKey = source.ReturnKey,
                ShowRecordId = source.ShowRecordId,
                Max = source.Max,
                Min = source.Min,
                Projection = source.Projection,
                Skip = source.Skip,
                Limit = source.Limit,
                Sort = source.Sort
            };

            return result;
        }
    }
}
