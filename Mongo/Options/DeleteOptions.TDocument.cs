using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    public record DeleteOptions<TDocument> : IOptions
    {
        public Collation Collation
        {
            get; set;
        }
        public Query<TDocument> Hint
        {
            get; set;
        }
        public bool NotPerformInTransaction
        {
            get; set;
        }


        public static implicit operator DeleteOptions(DeleteOptions<TDocument> source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new DeleteOptions
            {
                Hint = source.Hint,
                Collation = source.Collation
            };

            return result;
        }

        //
        public static implicit operator BulkWriteOptions(DeleteOptions<TDocument> source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new BulkWriteOptions
            {
                NotPerformInTransaction = source.NotPerformInTransaction
            };

            return result;
        }
    }
}
