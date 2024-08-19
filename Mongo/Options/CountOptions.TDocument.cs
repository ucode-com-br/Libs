namespace UCode.Mongo.Options
{
    public record CountOptions<TDocument> : CountOptions
    {
        public Query<TDocument> Hint
        {
            get; set;
        }

        public static implicit operator MongoDB.Driver.CountOptions(CountOptions<TDocument> source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.CountOptions
            {
                Collation = source.Collation,
                Hint = source.Hint,
                Limit = source.Limit,
                Skip = source.Skip,
                MaxTime = source.MaxTime
            };

            return result;
        }
    }
}
