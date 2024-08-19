namespace UCode.Mongo.Options
{
    public record UpdateOptions<TDocument> : UpdateOptions
    {
        public Query<TDocument> Hint
        {
            get; set;
        }


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
