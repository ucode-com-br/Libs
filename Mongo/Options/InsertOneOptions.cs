namespace UCode.Mongo.Options
{
    public record InsertOneOptions : IOptions
    {
        public bool? BypassDocumentValidation
        {
            get; set;
        }
        public bool NotPerformInTransaction
        {
            get; set;
        }

        public static implicit operator MongoDB.Driver.InsertOneOptions(InsertOneOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.InsertOneOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation
            };

            return result;
        }

        public static implicit operator BulkWriteOptions(InsertOneOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var bulkWriteOptions = new BulkWriteOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                NotPerformInTransaction = source.NotPerformInTransaction
            };

            return bulkWriteOptions;
        }
    }
}
