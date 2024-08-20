namespace UCode.Mongo.Options
{
    public record CountOptions<TDocument> : CountOptions
    {
        /// <summary>
        /// Gets or sets the hint for the query.
        /// </summary>
        /// <value>The hint for the query.</value>
        public Query<TDocument> Hint
        {
            get; set;
        }

        /// <summary>
        /// Represents the implicit conversion from <see cref="CountOptions{TDocument}"/> to <see cref="MongoDB.Driver.CountOptions"/>.
        /// </summary>
        /// <param name="source">The source <see cref="CountOptions{TDocument}"/> object.</param>
        /// <returns>The converted <see cref="MongoDB.Driver.CountOptions"/> object.</returns>
        public static implicit operator MongoDB.Driver.CountOptions(CountOptions<TDocument> source)
        {
            // If the source object is null, return the default count options
            if (source == default)
            {
                return default;
            }

            // Create a new instance of MongoDB.Driver.CountOptions
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
