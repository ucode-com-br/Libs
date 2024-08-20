namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for a MongoDB replace operation.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document being replaced.</typeparam>
    public record ReplaceOptions<TDocument> : ReplaceOptions
    {
        /// <summary>
        /// Gets or sets the hint for the operation.
        /// </summary>
        public Query<TDocument> Hint
        {
            get; set;
        }

        /// <summary>
        /// Converts a <see cref="ReplaceOptions{TDocument}"/> to a <see cref="MongoDB.Driver.ReplaceOptions"/>.
        /// </summary>
        /// <param name="source">The <see cref="ReplaceOptions{TDocument}"/> to convert.</param>
        /// <returns>The converted <see cref="MongoDB.Driver.ReplaceOptions"/>.</returns>
        public static implicit operator MongoDB.Driver.ReplaceOptions(ReplaceOptions<TDocument> source)
        {
            var result = new MongoDB.Driver.ReplaceOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                Collation = source.Collation,
                IsUpsert = source.IsUpsert,
                Hint = source.Hint
            };

            return result;
        }

        /// <summary>
        /// Converts a <see cref="ReplaceOptions{TDocument}"/> to a <see cref="BulkWriteOptions"/>.
        /// </summary>
        /// <param name="source">The <see cref="ReplaceOptions{TDocument}"/> to convert.</param>
        /// <returns>The converted <see cref="BulkWriteOptions"/>.</returns>
        public static implicit operator BulkWriteOptions(ReplaceOptions<TDocument> source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new BulkWriteOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                NotPerformInTransaction = source.NotPerformInTransaction
            };

            return result;
        }
    }
}
