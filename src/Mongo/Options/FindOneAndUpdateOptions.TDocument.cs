using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    public record FindOneAndUpdateOptions<TDocument> : FindOneAndUpdateOptions<TDocument, TDocument>
    {
        public static implicit operator MongoDB.Driver.FindOneAndUpdateOptions<TDocument>(
            FindOneAndUpdateOptions<TDocument> source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.FindOneAndUpdateOptions<TDocument>
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                Collation = source.Collation,
                Hint = source.Hint,
                IsUpsert = source.IsUpsert,
                MaxTime = source.MaxTime,
                Projection = source.Projection,
                ReturnDocument = source.ReturnDocumentAfter ? ReturnDocument.After : ReturnDocument.Before,
                Sort = source.Sort
            };

            return result;
        }
    }
}
