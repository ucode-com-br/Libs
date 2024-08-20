using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for a MongoDB find operation with paging.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document being queried.</typeparam>
    public record FindOptionsPaging<TDocument> : FindOptionsPagingBase
    {
        /// <summary>
        /// Gets or sets the hint for the query.
        /// </summary>
        public Query<TDocument> Hint
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the minimum value for the query.
        /// </summary>
        public Query<TDocument> Min
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the maximum value for the query.
        /// </summary>
        public Query<TDocument> Max
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the sort order for the query.
        /// </summary>
        public Query<TDocument> Sort
        {
            get; set;
        }

        /// <summary>
        /// Converts a <see cref="FindOptionsPaging"/> object to a <see cref="FindOptionsPaging{TDocument}"/> object.
        /// </summary>
        /// <param name="source">The source object to convert.</param>
        /// <returns>The converted object.</returns>
        [return: NotNull]
        public static implicit operator FindOptionsPaging<TDocument>([NotNull] FindOptionsPaging source)
        {
            // If the source is null, return the default value
            if (source == default)
            {
                return default;
            }

            // Create a new instance of FindOptionsPaging<TDocument>
            var result = new FindOptionsPaging<TDocument>
            {

                // Copy the properties from the source object
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
                CurrentPage = source.CurrentPage,
                PageSize = source.PageSize
            };

            // Return the converted object
            return result;
        }

        /// <summary>
        /// Converts a <see cref="FindOptionsPaging{TDocument}"/> object to a <see cref="MongoDB.Driver.FindOptions{TDocument}"/> object.
        /// </summary>
        /// <param name="source">The source object to convert.</param>
        /// <returns>The converted object.</returns>
        [return: NotNull]
        public static implicit operator MongoDB.Driver.FindOptions<TDocument>([NotNull] FindOptionsPaging<TDocument> source)
        {
            // If the source is null, return the default value
            if (source == default)
            {
                return default;
            }

            // Create a new instance of FindOptions<TDocument>
            var result = new MongoDB.Driver.FindOptions<TDocument>
            {
                // Copy the properties from the source object
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
                Skip = source.Skip,
                Limit = source.Limit,
                Sort = source.Sort
            };

            return result;
        }
    }
}
