namespace UCode.Mongo.Options
{
    public record ReplaceOptions<TDocument> : ReplaceOptions
    {
        public Query<TDocument> Hint
        {
            get; set;
        }


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
