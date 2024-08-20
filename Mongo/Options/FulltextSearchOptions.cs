using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for a MongoDB full text search operation.
    /// </summary>
    /// <typeparam name="T">The type of the document being queried.</typeparam>
    public struct FullTextSearchOptions<T> : IOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FullTextSearchOptions{T}"/> struct.
        /// </summary>
        public FullTextSearchOptions()
        {
        }

        /// <summary>
        /// Gets or sets the language of the search. Defaults to "pt".
        /// </summary>
        public string Language { get; set; } = "pt";

        /// <summary>
        /// Gets or sets a value indicating whether the search should be case sensitive. Defaults to false.
        /// </summary>
        public bool CaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the search should be diacritic sensitive. Defaults to false.
        /// </summary>
        public bool DiacriticSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the search should be performed outside of a transaction. Defaults to true.
        /// </summary>
        public bool NotPerformInTransaction { get; set; } = true;

        /// <summary>
        /// Determines whether two <see cref="FullTextSearchOptions{T}"/> instances are equal.
        /// </summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the instances are equal, false otherwise.</returns>
        public static bool operator ==(FullTextSearchOptions<T> lhs, FullTextSearchOptions<T> rhs) => lhs.Language == rhs.Language &&
                lhs.CaseSensitive == rhs.CaseSensitive &&
                lhs.DiacriticSensitive == rhs.DiacriticSensitive &&
                lhs.NotPerformInTransaction == rhs.NotPerformInTransaction;

        /// <summary>
        /// Determines whether two <see cref="FullTextSearchOptions{T}"/> instances are not equal.
        /// </summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the instances are not equal, false otherwise.</returns>
        public static bool operator !=(FullTextSearchOptions<T> lhs, FullTextSearchOptions<T> rhs) => lhs.Language != rhs.Language &&
                lhs.CaseSensitive != rhs.CaseSensitive &&
                lhs.DiacriticSensitive != rhs.DiacriticSensitive &&
                lhs.NotPerformInTransaction != rhs.NotPerformInTransaction;

        /// <summary>
        /// Converts a <see cref="FullTextSearchOptions{T}"/> instance to a <see cref="TextSearchOptions"/> instance.
        /// </summary>
        /// <param name="source">The <see cref="FullTextSearchOptions{T}"/> instance to convert.</param>
        /// <returns>A <see cref="TextSearchOptions"/> instance with the same properties as the <see cref="FullTextSearchOptions{T}"/> instance, or null if the source is default.</returns>
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

        /// <summary>
        /// Converts a nullable <see cref="FullTextSearchOptions{T}"/> instance to a <see cref="TextSearchOptions"/> instance.
        /// </summary>
        /// <param name="source">The nullable <see cref="FullTextSearchOptions{T}"/> instance to convert.</param>
        /// <returns>A new <see cref="TextSearchOptions"/> instance with the same properties as the <see cref="FullTextSearchOptions{T}"/> instance, or null if the nullable instance is null.</returns>
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

        /// <summary>
        /// Determines whether two <see cref="FullTextSearchOptions{T}"/> instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>True if the instances are equal, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            throw new System.NotImplementedException();
        }
    }
}
