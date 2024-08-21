using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for a MongoDB find operation.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document being queried.</typeparam>
    public record FindOptions<TDocument> : FindOptionsBase
    {
        /// <summary>
        /// Gets or sets the hint for the query.
        /// </summary>
        /// <value>The hint for the query.</value>
        public Query<TDocument> Hint
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the minimum value for the query.
        /// </summary>
        /// <value>The minimum value for the query.</value>
        public Query<TDocument> Min
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the maximum value for the query.
        /// </summary>
        /// <value>The maximum value for the query.</value>
        public Query<TDocument> Max
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the sort order for the query.
        /// </summary>
        /// <value>The sort order for the query.</value>
        public Query<TDocument> Sort
        {
            get; set;
        }

        /// <summary>
        /// Provides implicit conversion from <see cref="FindOptions{TDocument}"/> to <see cref="MongoDB.Driver.FindOptions"/>.
        /// </summary>
        /// <param name="source">The <see cref="FindOptions{TDocument}"/> object to convert.</param>
        /// <returns>The converted <see cref="MongoDB.Driver.FindOptions"/> object.</returns>
        public static implicit operator MongoDB.Driver.FindOptions(FindOptions<TDocument> source)
        {
            // If the source object is null, return the default find options
            if (source == default)
            {
                return default;
            }

            // Create a new instance of FindOptions
            var result = new MongoDB.Driver.FindOptions
            {
                // Set the Collation property
                Collation = source.Collation,

                Hint = source.Hint,
                AllowDiskUse = source.AllowDiskUse,
                AllowPartialResults = source.AllowPartialResults,
                MaxTime = source.MaxTime,
                BatchSize = source.BatchSize,
                Comment = source.Comment,
                CursorType = (CursorType)source.CursorType, // Set the CursorType property based on the value of CursorType
                MaxAwaitTime = source.MaxAwaitTime,
                NoCursorTimeout = source.NoCursorTimeout,
                ReturnKey = source.ReturnKey,
                ShowRecordId = source.ShowRecordId,
                Max = source.Max,
                Min = source.Min
            };

            return result;
        }

        /// <summary>
        /// Provides implicit conversion from <see cref="FindOptions"/> to <see cref="FindOptions{TDocument}"/>.
        /// </summary>
        /// <param name="source">The <see cref="FindOptions"/> object to convert.</param>
        /// <returns>The converted <see cref="FindOptions{TDocument}"/> object.</returns>
        public static implicit operator FindOptions<TDocument>(FindOptions source)
        {
            // If the source object is null, return the default find options
            if (source == default)
            {
                return default;
            }

            // Create a new instance of FindOptions
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

        /// <summary>
        /// Provides implicit conversion from <see cref="FindOptions{TDocument}"/> to <see cref="MongoDB.Driver.CountOptions"/>.
        /// </summary>
        /// <param name="source">The <see cref="FindOptions{TDocument}"/> object to convert.</param>
        /// <returns>The converted <see cref="MongoDB.Driver.CountOptions"/> object.</returns>
        public static implicit operator MongoDB.Driver.CountOptions(FindOptions<TDocument> source)
        {
            // If the source object is null, return the default count options
            if (source == default)
            {
                return default;
            }

            // Create a new instance of CountOptions
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
