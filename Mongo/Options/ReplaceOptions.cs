// ReSharper disable IdentifierTypo

namespace UCode.Mongo.Options
{
    public record ReplaceOptions : IOptions
    {
        public bool? BypassDocumentValidation
        {
            get; set;
        }

        public Collation Collation
        {
            get; set;
        }

        public bool IsUpsert
        {
            get; set;
        }
        public bool NotPerformInTransaction
        {
            get; set;
        }

        public static implicit operator MongoDB.Driver.ReplaceOptions(ReplaceOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.ReplaceOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                Collation = source.Collation,
                IsUpsert = source.IsUpsert
            };

            return result;
        }

        public static implicit operator BulkWriteOptions(ReplaceOptions source)
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
