// ReSharper disable IdentifierTypo

namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for a MongoDB replace operation.
    /// </summary>
    public record ReplaceOptions : IOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to bypass document validation.
        /// </summary>
        public bool? BypassDocumentValidation
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the collation for the replace operation.
        /// </summary>
        public Collation Collation
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to perform an upsert.
        /// </summary>
        public bool IsUpsert
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to not perform the operation in a transaction.
        /// </summary>
        public bool NotPerformInTransaction
        {
            get; set;
        }

        /// <summary>
        /// Converts a <see cref="ReplaceOptions"/> to a <see cref="MongoDB.Driver.ReplaceOptions"/>.
        /// </summary>
        /// <param name="source">The <see cref="ReplaceOptions"/> to convert.</param>
        /// <returns>The converted <see cref="MongoDB.Driver.ReplaceOptions"/>.</returns>
        public static implicit operator MongoDB.Driver.ReplaceOptions(ReplaceOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.ReplaceOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                Collation = source.Collation,
                IsUpsert = source.IsUpsert
            };

            return result;
        }

        /// <summary>
        /// Converts a <see cref="ReplaceOptions"/> to a <see cref="BulkWriteOptions"/>.
        /// </summary>
        /// <param name="source">The <see cref="ReplaceOptions"/> to convert.</param>
        /// <returns>The converted <see cref="BulkWriteOptions"/>.</returns>
        public static implicit operator BulkWriteOptions(ReplaceOptions source)
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
