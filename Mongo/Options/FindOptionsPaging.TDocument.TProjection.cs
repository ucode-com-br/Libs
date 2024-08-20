using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for a MongoDB find operation with paging and projection.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document being queried.</typeparam>
    /// <typeparam name="TProjection">The type of the projection.</typeparam>
    public record FindOptionsPaging<TDocument, TProjection> : FindOptionsPagingBase
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
        /// Gets or sets the projection for the query.
        /// </summary>
        public Query<TDocument, TProjection> Projection
        {
            get; set;
        }

        /// <summary>
        /// Implicitly converts a <see cref="FindOptionsPaging{TDocument}"/> to a <see cref="FindOptionsPaging{TDocument, TProjection}"/>.
        /// </summary>
        /// <param name="source">The source <see cref="FindOptionsPaging{TDocument}"/> to convert from.</param>
        /// <returns>The converted <see cref="FindOptionsPaging{TDocument, TProjection}"/>.</returns>
        [return: NotNull]
        public static implicit operator FindOptionsPaging<TDocument, TProjection>([NotNull] FindOptionsPaging<TDocument> source)
        {
            // If the source is default, return default
            if (source == default)
            {
                return default;
            }

            // Create a new instance of FindOptionsPaging<TDocument, TProjection>
            var result = new FindOptionsPaging<TDocument, TProjection>
            {
                // Copy over properties from the source
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
            // Return the converted result
            return result;
        }

        /// <summary>
        /// Implicitly converts a <see cref="FindOptionsPaging{TDocument, TProjection}"/> to a <see cref="FindOptions{TDocument, TProjection}"/>.
        /// </summary>
        /// <param name="source">The source <see cref="FindOptionsPaging{TDocument, TProjection}"/> to convert from.</param>
        /// <returns>The converted <see cref="FindOptions{TDocument, TProjection}"/>.</returns>
        [return: NotNull]
        public static implicit operator MongoDB.Driver.FindOptions<TDocument, TProjection>([NotNull] FindOptionsPaging<TDocument, TProjection> source)
        {
            // If the source is default, return default
            if (source == default)
            {
                return default;
            }

            // Create a new instance of FindOptions<TDocument, TProjection>
            var result = new MongoDB.Driver.FindOptions<TDocument, TProjection>
            {
                // Set the properties of the result object from the source object
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
