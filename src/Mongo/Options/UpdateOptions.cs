// ReSharper disable IdentifierTypo

namespace UCode.Mongo.Options
{
    public record UpdateOptions : IOptions
    {
        public bool? BypassDocumentValidation
        {
            get; set;
        }

        public bool IsUpsert
        {
            get; set;
        }

        public Collation Collation
        {
            get; set;
        }
        public bool NotPerformInTransaction
        {
            get; set;
        }


        public static implicit operator MongoDB.Driver.UpdateOptions(UpdateOptions source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.UpdateOptions
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                Collation = source.Collation,
                IsUpsert = source.IsUpsert
            };


            return result;
        }
    }
}
