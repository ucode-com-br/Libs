using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    public struct FullTextSearchOptions<T> : IOptions
    {
        public FullTextSearchOptions()
        {
        }


        public string Language { get; set; } = "pt";

        public bool CaseSensitive { get; set; } = false;

        public bool DiacriticSensitive { get; set; } = false;

        public bool NotPerformInTransaction { get; set; } = true;

        public static bool operator ==(FullTextSearchOptions<T> lhs, FullTextSearchOptions<T> rhs) => lhs.Language == rhs.Language &&
                lhs.CaseSensitive == rhs.CaseSensitive &&
                lhs.DiacriticSensitive == rhs.DiacriticSensitive &&
                lhs.NotPerformInTransaction == rhs.NotPerformInTransaction;
        public static bool operator !=(FullTextSearchOptions<T> lhs, FullTextSearchOptions<T> rhs) => lhs.Language != rhs.Language &&
                lhs.CaseSensitive != rhs.CaseSensitive &&
                lhs.DiacriticSensitive != rhs.DiacriticSensitive &&
                lhs.NotPerformInTransaction != rhs.NotPerformInTransaction;


        public static implicit operator TextSearchOptions(FullTextSearchOptions<T> source)
        {
            if (source == default)
            {
                return null;
            }

            return new TextSearchOptions()
            {
                CaseSensitive = source.CaseSensitive,
                DiacriticSensitive = source.DiacriticSensitive,
                Language = source.Language,
            };
        }

        public static implicit operator TextSearchOptions(FullTextSearchOptions<T>? source)
        {
            if (source.HasValue)
            {
                return new TextSearchOptions()
                {
                    CaseSensitive = source.Value.CaseSensitive,
                    DiacriticSensitive = source.Value.DiacriticSensitive,
                    Language = source.Value.Language,
                };
            }

            return null;
        }

        public override bool Equals(object obj)
        {
            throw new System.NotImplementedException();
        }
    }
}
