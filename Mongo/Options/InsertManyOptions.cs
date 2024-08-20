namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for a MongoDB insertMany operation.
    /// </summary>
    public record InsertManyOptions : IOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to bypass document validation.
        /// </summary>
        public bool? BypassDocumentValidation
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the insertMany operation should be ordered.
        /// Defaults to true.
        /// </summary>
        public bool IsOrdered { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the insertMany operation should be performed outside of a transaction.
        /// </summary>
        public bool NotPerformInTransaction
        {
            get; set;
        }

        /// <summary>
        /// Converts an <see cref="InsertManyOptions"/> instance to a <see cref="MongoDB.Driver.InsertManyOptions"/> instance.
        /// </summary>
        /// <param name="source">The <see cref="InsertManyOptions"/> instance to convert.</param>
        /// <returns>A <see cref="MongoDB.Driver.InsertManyOptions"/> instance with the same properties as the <see cref="InsertManyOptions"/> instance, or null if the source is default.</returns>
        public static implicit operator MongoDB.Driver.InsertManyOptions(InsertManyOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.InsertManyOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                IsOrdered = source.IsOrdered
            };

            return result;
        }

        /// <summary>
        /// Converts an <see cref="InsertManyOptions"/> instance to a <see cref="BulkWriteOptions"/> instance.
        /// </summary>
        /// <param name="source">The <see cref="InsertManyOptions"/> instance to convert.</param>
        /// <returns>A <see cref="BulkWriteOptions"/> instance with the same properties as the <see cref="InsertManyOptions"/> instance, or null if the source is default.</returns>
        public static implicit operator BulkWriteOptions(InsertManyOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var bulkWriteOptions = new BulkWriteOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                NotPerformInTransaction = source.NotPerformInTransaction,
                IsOrdered = source.IsOrdered
            };

            return bulkWriteOptions;
        }
    }
}
