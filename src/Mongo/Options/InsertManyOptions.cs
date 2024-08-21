namespace UCode.Mongo.Options
{
    public record InsertManyOptions : IOptions
    {
        public bool? BypassDocumentValidation
        {
            get; set;
        }

        public bool IsOrdered { get; set; } = true;
        public bool NotPerformInTransaction
        {
            get; set;
        }


        public static implicit operator MongoDB.Driver.InsertManyOptions(InsertManyOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.InsertManyOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                IsOrdered = source.IsOrdered
            };

            return result;
        }

        public static implicit operator BulkWriteOptions(InsertManyOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var bulkWriteOptions = new BulkWriteOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                NotPerformInTransaction = source.NotPerformInTransaction,
                IsOrdered = source.IsOrdered
            };

            return bulkWriteOptions;
        }
    }
}
