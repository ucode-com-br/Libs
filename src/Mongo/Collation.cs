using MongoDB.Driver;

namespace UCode.Mongo
{
    /// <summary>
    ///     Collation allows users to specify language-specific rules for string comparison, such as rules for letter case and
    ///     accent marks.
    ///     https://docs.mongodb.com/manual/reference/collation/
    /// </summary>
    public class Collation
    {
        public Collation(string locale = "pt") => this.Locale = locale;

        /// <summary>
        ///     Flag that determines whether to check if text require normalization and to perform normalization. Generally,
        ///     majority of text does not require this normalization processing.
        ///     If true, check if fully normalized and perform normalization to compare text.
        ///     If false, does not check.
        /// </summary>
        public bool? Normalization { get; init; } = false;

        /// <summary>
        ///     Field that determines up to which characters are considered ignorable when alternate: "shifted". Has no effect if
        ///     alternate: "non-ignorable"
        /// </summary>
        public bool? MaxVariable
        {
            get; init;
        }

        /// <summary>
        ///     The ICU locale.See Supported Languages and Locales for a list of supported locales.
        ///     https://docs.mongodb.com/manual/reference/collation-locales-defaults/#collation-languages-locales
        /// </summary>
        public string Locale
        {
            get; init;
        }

        /// <summary>
        ///     Flag that determines whether to include case comparison at strength level 1 or 2.
        ///     If true, include case comparison; i.e.
        ///     When used with strength:1, collation compares base characters and case.
        ///     When used with strength:2, collation compares base characters, diacritics(and possible other secondary differences)
        ///     and case.
        ///     If false, do not include case comparison at level 1 or 2. The default is false.
        /// </summary>
        public bool? CaseLevel
        {
            get; set;
        }

        /// <summary>
        ///     Upper case first that determines sort order of case differences during tertiary level comparisons.
        ///     True = Uppercase sorts before lowercase.
        ///     False = Lowercase sorts before uppercase.
        /// </summary>
        public bool? UpperCaseFirst
        {
            get; init;
        }

        /// <summary>
        ///     Flag that determines whether strings with diacritics sort from back of the string, such as with some French
        ///     dictionary ordering.
        ///     If true, compare from back to front.
        ///     If false, compare from front to back.
        /// </summary>
        public bool? Backwards { get; init; } = false;

        /// <summary>
        ///     Field that determines whether collation should consider whitespace and punctuation as base characters for purposes
        ///     of comparison.
        ///     True = non-ignorable = Whitespace and punctuation are considered base characters.
        ///     False = shifted = Whitespace and punctuation are not considered base characters and are only distinguished at
        ///     strength levels greater than 3.
        /// </summary>
        public bool Alternate { get; init; } = true;

        /// <summary>
        ///     The level of comparison to perform. Corresponds to ICU Comparison Levels. Possible values are:
        ///     1	Primary level of comparison. Collation performs comparisons of the base characters only, ignoring other
        ///     differences such as diacritics and case.
        ///     2	Secondary level of comparison. Collation performs comparisons up to secondary differences, such as diacritics.
        ///     That is, collation performs comparisons of base characters (primary differences) and diacritics (secondary
        ///     differences). Differences between base characters takes precedence over secondary differences.
        ///     3	Tertiary level of comparison.Collation performs comparisons up to tertiary differences, such as case and letter
        ///     variants.That is, collation performs comparisons of base characters (primary differences), diacritics (secondary
        ///     differences), and case and variants (tertiary differences). Differences between base characters takes precedence
        ///     over secondary differences, which takes precedence over tertiary differences.
        ///     4	Quaternary Level. Limited for specific use case to consider punctuation when levels 1-3 ignore punctuation or for
        ///     processing Japanese text.
        ///     5	Identical Level. Limited for specific use case of tie breaker.
        /// </summary>
        public int Strength { get; init; } = 3;

        /// <summary>
        ///     Flag that determines whether to compare numeric strings as numbers or as strings.
        ///     If true, compare as numbers; i.e. "10" is greater than "2".
        ///     If false, compare as strings; i.e. "10" is less than "2".
        /// </summary>
        public bool? NumericOrdering { get; init; } = false;

        public static implicit operator MongoDB.Driver.Collation(Collation collation)
        {
            if (collation == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.Collation(collation.Locale,
                collation.CaseLevel,
                collation.UpperCaseFirst.HasValue
                    ? collation.UpperCaseFirst.Value ? CollationCaseFirst.Upper : CollationCaseFirst.Lower
                    : CollationCaseFirst.Off,
                (CollationStrength)collation.Strength,
                collation.NumericOrdering,
                collation.Alternate ? CollationAlternate.NonIgnorable : CollationAlternate.Shifted,
                collation.MaxVariable.HasValue
                    ? collation.MaxVariable.Value ? CollationMaxVariable.Space : CollationMaxVariable.Punctuation
                    : null,
                collation.Normalization,
                collation.Backwards);

            return result;
        }
    }
}
