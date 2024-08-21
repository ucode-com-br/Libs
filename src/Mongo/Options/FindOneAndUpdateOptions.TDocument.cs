using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    public record FindOneAndUpdateOptions<TDocument> : FindOneAndUpdateOptions<TDocument, TDocument>
    {
        /// <summary>
        /// Implicit conversion operator from <see cref="FindOneAndUpdateOptions{TDocument}"/> to <see cref="MongoDB.Driver.FindOneAndUpdateOptions{TDocument}"/>.
        /// </summary>
        /// <param name="source">The source <see cref="FindOneAndUpdateOptions{TDocument}"/> object.</param>
        /// <returns>The converted <see cref="MongoDB.Driver.FindOneAndUpdateOptions{TDocument}"/> object.</returns>
        public static implicit operator MongoDB.Driver.FindOneAndUpdateOptions<TDocument>(
            FindOneAndUpdateOptions<TDocument> source)
        {
            // If the source object is null, return the default find one and update options
            if (source == default)
            {
                return default;
            }

            // Create a new instance of FindOneAndUpdateOptions
            var result = new MongoDB.Driver.FindOneAndUpdateOptions<TDocument>
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                Collation = source.Collation,
                Hint = source.Hint,
                IsUpsert = source.IsUpsert,
                MaxTime = source.MaxTime,
                Projection = source.Projection,
                ReturnDocument = source.ReturnDocumentAfter ? ReturnDocument.After : ReturnDocument.Before, // Set the ReturnDocument property based on the value of ReturnDocumentAfter

                Sort = source.Sort
            };

            return result;
        }
    }
}
