namespace UCode.Mongo.Options
{
    public record BulkWriteOptions : IOptions
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


        public static implicit operator MongoDB.Driver.BulkWriteOptions(BulkWriteOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var bulkWriteOptions = new MongoDB.Driver.BulkWriteOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                IsOrdered = source.IsOrdered
            };

            return bulkWriteOptions;
        }
    }
}
