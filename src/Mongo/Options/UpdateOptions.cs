// ReSharper disable IdentifierTypo

namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for a MongoDB update operation.
    /// </summary>
    public record UpdateOptions : IOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to bypass document validation.
        /// </summary>
        public bool? BypassDocumentValidation
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to perform an upsert operation.
        /// </summary>
        public bool IsUpsert
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the collation for the update operation.
        /// </summary>
        public Collation Collation
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to perform the update operation outside of a transaction.
        /// </summary>
        public bool NotPerformInTransaction
        {
            get; set;
        }

        /// <summary>
        /// Converts the <see cref="UpdateOptions"/> object to a <see cref="MongoDB.Driver.UpdateOptions"/> object.
        /// </summary>
        /// <param name="source">The <see cref="UpdateOptions"/> object to convert.</param>
        /// <returns>A <see cref="MongoDB.Driver.UpdateOptions"/> object.</returns>
        public static implicit operator MongoDB.Driver.UpdateOptions(UpdateOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.UpdateOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                Collation = source.Collation,
                IsUpsert = source.IsUpsert
            };


            return result;
        }
    }
}
