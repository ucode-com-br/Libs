namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for a MongoDB update operation with a specific document type.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document being updated.</typeparam>
    public record UpdateOptions<TDocument> : UpdateOptions
    {
        /// <summary>
        /// Gets or sets the hint for the update operation.
        /// </summary>
        public Query<TDocument> Hint
        {
            get; set;
        }

        /// <summary>
        /// Converts the <see cref="UpdateOptions{TDocument}"/> to a <see cref="MongoDB.Driver.UpdateOptions"/>.
        /// </summary>
        /// <param name="source">The <see cref="UpdateOptions{TDocument}"/> to convert.</param>
        /// <returns>The converted <see cref="MongoDB.Driver.UpdateOptions"/>.</returns>
        public static implicit operator MongoDB.Driver.UpdateOptions(UpdateOptions<TDocument> source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.UpdateOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                Collation = source.Collation,
                Hint = source.Hint,
                IsUpsert = source.IsUpsert
            };

            return result;
        }
    }
}
