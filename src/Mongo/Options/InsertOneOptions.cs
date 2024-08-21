namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for a MongoDB insertOne operation.
    /// </summary>
    public record InsertOneOptions : IOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to bypass document validation.
        /// </summary>
        public bool? BypassDocumentValidation
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the insertOne operation should be performed outside of a transaction.
        /// </summary>
        public bool NotPerformInTransaction
        {
            get; set;
        }

        /// <summary>
        /// Converts an <see cref="InsertOneOptions"/> instance to a <see cref="MongoDB.Driver.InsertOneOptions"/> instance.
        /// </summary>
        /// <param name="source">The <see cref="InsertOneOptions"/> instance to convert.</param>
        /// <returns>A <see cref="MongoDB.Driver.InsertOneOptions"/> instance with the same properties as the <see cref="InsertOneOptions"/> instance, or null if the source is default.</returns>
        public static implicit operator MongoDB.Driver.InsertOneOptions(InsertOneOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.InsertOneOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation
            };

            return result;
        }

        /// <summary>
        /// Converts an <see cref="InsertOneOptions"/> instance to a <see cref="BulkWriteOptions"/> instance.
        /// </summary>
        /// <param name="source">The <see cref="InsertOneOptions"/> instance to convert.</param>
        /// <returns>A <see cref="BulkWriteOptions"/> instance with the same properties as the <see cref="InsertOneOptions"/> instance, or null if the source is default.</returns>
        public static implicit operator BulkWriteOptions(InsertOneOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var bulkWriteOptions = new BulkWriteOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                NotPerformInTransaction = source.NotPerformInTransaction
            };

            return bulkWriteOptions;
        }
    }
}
