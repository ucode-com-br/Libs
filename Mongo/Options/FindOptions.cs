using MongoDB.Driver;

// ReSharper disable CommentTypo

namespace UCode.Mongo.Options
{
    public record FindOptions : FindOptionsBase
    {
        /// <summary>
        /// Implicit conversion operator from <see cref="FindOptions"/> to <see cref="MongoDB.Driver.FindOptions"/>.
        /// </summary>
        /// <param name="source">The source <see cref="FindOptions"/> object.</param>
        /// <returns>The converted <see cref="MongoDB.Driver.FindOptions"/> object.</returns>
        public static implicit operator MongoDB.Driver.FindOptions?(FindOptions source)
        {
            // If the source object is null, return the default find options
            if (source == default)
            {
                return default;
            }

            // Create a new instance of FindOptions
            var result = new MongoDB.Driver.FindOptions
            {
                Collation = source.Collation,
                AllowDiskUse = source.AllowDiskUse,
                AllowPartialResults = source.AllowPartialResults,
                MaxTime = source.MaxTime,
                BatchSize = source.BatchSize,
                Comment = source.Comment,
                CursorType = (CursorType)source.CursorType, // Set the CursorType property based on the value of CursorType
                MaxAwaitTime = source.MaxAwaitTime,
                NoCursorTimeout = source.NoCursorTimeout,
                ReturnKey = source.ReturnKey,
                ShowRecordId = source.ShowRecordId
            };

            return result;
        }
    }
}
