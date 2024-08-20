namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for a MongoDB bulk write operation.
    /// </summary>
    public record BulkWriteOptions : IOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether document validation should be bypassed for the bulk write operation.
        /// </summary>
        public bool? BypassDocumentValidation
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the bulk write operation should be ordered.
        /// </summary>
        /// <remarks>
        /// If set to true, the bulk write operation will stop processing if an error occurs.
        /// If set to false, the bulk write operation will continue processing even if an error occurs.
        /// The default value is true.
        /// </remarks>
        public bool IsOrdered { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the bulk write operation should not be performed within a transaction.
        /// </summary>
        public bool NotPerformInTransaction
        {
            get; set;
        }

        /// <summary>
        /// Represents the implicit conversion from <see cref="BulkWriteOptions"/> to <see cref="MongoDB.Driver.BulkWriteOptions"/>.
        /// </summary>
        /// <param name="source">The source <see cref="BulkWriteOptions"/> object.</param>
        /// <returns>The converted <see cref="MongoDB.Driver.BulkWriteOptions"/> object.</returns>
        public static implicit operator MongoDB.Driver.BulkWriteOptions(BulkWriteOptions source)
        {
            // If the source object is null, return the default bulk write options
            if (source == default)
            {
                return default;
            }

            // Create a new instance of MongoDB.Driver.BulkWriteOptions
            var bulkWriteOptions = new MongoDB.Driver.BulkWriteOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                IsOrdered = source.IsOrdered
            };

            // Return the converted bulk write options
            return bulkWriteOptions;
        }
    }
}
