using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    public record FindOptionsPaging<TDocument, TProjection> : FindOptionsPagingBase
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

        public Query<TDocument, TProjection> Projection
        {
            get; set;
        }


        [return: NotNull]
        public static implicit operator FindOptionsPaging<TDocument, TProjection>([NotNull] FindOptionsPaging<TDocument> source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new FindOptionsPaging<TDocument, TProjection>
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
                Sort = source.Sort,
                CurrentPage = source.CurrentPage,
                PageSize = source.PageSize
            };

            return result;
        }


        [return: NotNull]
        public static implicit operator MongoDB.Driver.FindOptions<TDocument, TProjection>([NotNull] FindOptionsPaging<TDocument, TProjection> source)
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
