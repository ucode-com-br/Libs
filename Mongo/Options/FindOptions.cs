using MongoDB.Driver;

// ReSharper disable CommentTypo

namespace UCode.Mongo.Options
{
    public record FindOptions : FindOptionsBase
    {
        /// <summary>
        /// Default convertion to MongoDB.Driver.FindOptions
        /// </summary>
        /// <param name="source"></param>
        public static implicit operator MongoDB.Driver.FindOptions?(FindOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.FindOptions
            {
                Collation = source.Collation,
                AllowDiskUse = source.AllowDiskUse,
                AllowPartialResults = source.AllowPartialResults,
                MaxTime = source.MaxTime,
                BatchSize = source.BatchSize,
                Comment = source.Comment,
                CursorType = (CursorType)source.CursorType,
                MaxAwaitTime = source.MaxAwaitTime,
                NoCursorTimeout = source.NoCursorTimeout,
                ReturnKey = source.ReturnKey,
                ShowRecordId = source.ShowRecordId
            };

            return result;
        }




    }
}
