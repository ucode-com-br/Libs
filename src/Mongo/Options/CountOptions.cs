using System;

namespace UCode.Mongo.Options
{
    public record CountOptions : IOptions
    {
        public Collation Collation
        {
            get; set;
        }

        public long? Skip
        {
            get; set;
        }

        public long? Limit
        {
            get; set;
        }

        public TimeSpan? MaxTime
        {
            get; set;
        }
        public bool NotPerformInTransaction
        {
            get; set;
        }

        public static implicit operator MongoDB.Driver.CountOptions(CountOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.CountOptions
            {
                Collation = source.Collation,
                Limit = source.Limit,
                Skip = source.Skip,
                MaxTime = source.MaxTime
            };

            return result;
        }
    }
}
