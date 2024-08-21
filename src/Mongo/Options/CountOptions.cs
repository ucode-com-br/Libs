using System;

namespace UCode.Mongo.Options
{

    /// <summary>
    /// Represents the options for a MongoDB count operation.
    /// </summary>
    public record CountOptions : IOptions
    {
        /// <summary>
        /// Gets or sets the collation for the count operation.
        /// </summary>
        public Collation Collation
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the number of documents to skip before counting.
        /// </summary>
        public long? Skip
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the maximum number of documents to count.
        /// </summary>
        public long? Limit
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the maximum amount of time to allow the count operation to run.
        /// </summary>
        public TimeSpan? MaxTime
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the count operation should not be performed within a transaction.
        /// </summary>
        public bool NotPerformInTransaction
        {
            get; set;
        }

        /// <summary>
        /// Represents the implicit conversion from <see cref="CountOptions"/> to <see cref="MongoDB.Driver.CountOptions"/>.
        /// </summary>
        /// <param name="source">The source <see cref="CountOptions"/> object.</param>
        /// <returns>The converted <see cref="MongoDB.Driver.CountOptions"/> object.</returns>
        public static implicit operator MongoDB.Driver.CountOptions(CountOptions source)
        {
            // If the source object is null, return the default count options
            if (source == default)
            {
                return default;
            }

            // Create a new instance of MongoDB.Driver.CountOptions
            var result = new MongoDB.Driver.CountOptions
            {
                Collation = source.Collation,
                Limit = source.Limit,
                Skip = source.Skip,
                MaxTime = source.MaxTime
            };
            // Return the converted count options
            return result;
        }
    }
}
