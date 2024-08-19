using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    public record FindOptions<TDocument> : FindOptionsBase
    {
        public Query<TDocument> Hint
        {
            get; set;
        }
        public Query<TDocument> Min
        {
            get; set;
        }
        public Query<TDocument> Max
        {
            get; set;
        }
        public Query<TDocument> Sort
        {
            get; set;
        }

        public static implicit operator MongoDB.Driver.FindOptions(FindOptions<TDocument> source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.FindOptions
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
                Min = source.Min
            };

            return result;
        }

        public static implicit operator FindOptions<TDocument>(FindOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new FindOptions<TDocument>
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
                ShowRecordId = source.ShowRecordId
            };

            return result;
        }

        public static implicit operator MongoDB.Driver.CountOptions(FindOptions<TDocument> source)
        {
            if (source == default)
            {
                return default;
            }

            var countOptions = new MongoDB.Driver.CountOptions
            {
                Collation = source.Collation,
                Hint = source.Hint,
                Limit = source.Limit,
                MaxTime = source.MaxTime,
                Skip = source.Skip
            };

            return countOptions;
        }
    }
}
