using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace UCode.Extensions
{
    /// <summary>
    /// A static partial class that contains extension methods for string manipulation.
    /// </summary>
    public static partial class StringExtensions
    {
        /// <summary>
        /// A generated regular expression that matches a specific pattern of expressions enclosed in curly braces,
        /// while accounting for various escape sequences and nested structures.
        /// This regex is designed to avoid matching nested braces within captured groups.
        /// </summary>
        /// <returns>
        /// A <see cref="Regex"/> object that can be used to find expressions matching the specified pattern.
        /// </returns>
        [GeneratedRegex(@"(?<!{)(?<!\\\\){(?!{)(?<exp>(?:[^{}]+|{(\1)})*)(?<!})}", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled, "pt-BR")]
        private static partial Regex findExpression();



        public record NamedPameters : NamedPameters<object>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="NamedPameters"/> class,
            /// using the specified object instance.
            /// </summary>
            /// <param name="instance">
            /// The object instance that will be passed to the base class constructor.
            /// This parameter cannot be null.
            /// </param>
            public NamedPameters([NotNull] object instance) : base(instance)
            {

            }


            /// <summary>
            /// Initializes a new instance of the <see cref="NamedPameters"/> class.
            /// </summary>
            /// <param name="type">The type that this instance will represent.</param>
            /// <param name="instance">The object instance associated with the specified type, or null if there is no instance.</param>
            /// <returns>
            /// A new instance of the <see cref="NamedPameters"/> class.
            /// </returns>
            public NamedPameters(Type type, object? instance) : base(type, instance)
            {

            }

            /// <summary>
            /// Initializes a new instance of the <see cref="NamedPameters"/> class.
            /// </summary>
            /// <param name="name">The name of the parameter.</param>
            /// <param name="type">The type of the parameter.</param>
            /// <param name="instance">The instance associated with the parameter, which can be null.</param>
            /// <returns>
            /// A new instance of the <see cref="NamedPameters"/> class.
            /// </returns>
            public NamedPameters(string name, Type type, object? instance) : base(name, type, instance)
            {

            }
        }
        public record NamedPameters<T>
        {
            private readonly T? _value;

            /// <summary>
            /// Represents the name of an entity. The name cannot be null.
            /// </summary>
            /// <remarks>
            /// The <c>Name</c> property is initialized at the time of object creation and is read-only thereafter.
            /// </remarks>
            /// <value>
            /// A non-null string that represents the name of the entity.
            /// </value>
            /// <exception cref="ArgumentNullException">Thrown when an attempt is made to assign null to the Name property.</exception>
            [NotNull]
            public string Name
            {
                get; init;
            }

            /// <summary>
            /// Represents a type that is marked as not nullable.
            /// </summary>
            /// <remarks>
            /// This property is used to store a <see cref="Type"/> which cannot be null. 
            /// The <c>init</c> accessor allows the property to be set only during object initialization.
            /// </remarks>
            /// <value>
            /// The <see cref="Type"/> of the object.
            /// </value>
            [NotNull]
            public Type Type
            {
                get; init;
            }


            /// <summary>
            /// Gets the value of the property, returning a default value if the 
            /// 
            [NotNull]
            public T? Value
            {
                get
                {
                    try
                    {
                        return this._value ?? this.DefaultValue;
                    }
                    catch (Exception ex)
                    {
                        return this.DefaultValue;
                    }
                }
                init => this._value = value;
            }

            /// <summary>
            /// Gets or sets the default value of type T.
            /// The property is marked with the <see cref="MaybeNullAttribute"/>,
            /// indicating that it is possible for the default value to be null.
            /// </summary>
            /// <typeparam name="T">The type of the default value.</typeparam>
            /// <value>
            /// The default value. This can be null if T is a reference type or if it is 
            /// marked with <see langword="Nullable"/>. 
            /// </value>
            /// <remarks>
            /// Use this property to define a value that can be used as a fallback option 
            /// when no other value is specified.
            /// </remarks>
            [MaybeNull]
            public T? DefaultValue
            {
                get; set;
            }


            /// <summary>
            /// Initializes a new instance of the <see cref="NamedPameters{T}"/> class 
            /// using the specified value and an optional default value.
            /// </summary>
            /// <param name="value">The primary value for the instance. This parameter cannot be null.</param>
            /// <param name="defaultValue">An optional default value, which can be null.</param>
            /// <returns>
            /// A new instance of the <see cref="NamedPameters{T}"/> class.
            /// </returns>
            public NamedPameters([NotNull] T value, [MaybeNull] T? defaultValue = default) : this(value.GetType().Name, value.GetType(), value, defaultValue)
            {

            }


            /// <summary>
            /// Initializes a new instance of the <see cref="NamedPameters"/> class.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> that this parameter represents. Must not be null.</param>
            /// <param name="value">The value of the parameter. This value cannot be null.</param>
            /// <param name="defaultValue">An optional default value of the parameter, which can be null.</param>
            /// <returns>
            /// A new instance of <see cref="NamedPameters"/> initialized with the specified type, value, and default value.
            /// </returns>
            public NamedPameters([NotNull] Type type, [NotNull] T? value, [MaybeNull] T? defaultValue = default) : this(type.Name, type, value, defaultValue)
            {

            }

            /// <summary>
            /// Initializes an instance of the <see cref="NamedParameters"/> class with the specified parameters.
            /// </summary>
            /// <param name="name">
            /// The name of the parameter. This should not be null.
            /// </param>
            /// <param name="type">
            /// The type of the parameter. This should not be null.
            /// </param>
            /// <param name="value">
            /// The value of the parameter. This should not be null. It can be of type <typeparamref name="T"/> or a nullable type.
            /// </param>
            /// <param name="defaultValue">
            /// The default value of the parameter, which can be null. If not provided, it defaults to the default value of <typeparamref name="T"/>.
            /// </param>
            /// <typeparam name="T">
            /// The type of the value and defaultValue parameters. It can be any reference or nullable value type.
            /// </typeparam>
            public NamedPameters([NotNull] string name, [NotNull] Type type, [NotNull] T? value, [MaybeNull] T? defaultValue = default)
            {
                this.Name = name;
                this.Type = type;
                this.Value = value;
                this.DefaultValue = defaultValue;
            }

            public static implicit operator NamedPameters<T>([NotNull] T? value)
            {
                if (value == null)
                {
                    return default;
                }

                return new NamedPameters<T>(value, default);
            }

            public static implicit operator T?(NamedPameters<T>? value)
            {
                if (value == null)
                {
                    return default;
                }

                return value.Value;
            }
        }


        /// <summary>
        /// Extends the string class to perform interpolation using named parameters and expressions.
        /// This method allows for dynamic expression parsing and evaluation based on the provided named parameters.
        /// It can return a default result if no interpolated values are found.
        /// </summary>
        /// <param name="value">The original string that contains the expressions to be evaluated.</param>
        /// <param name="namedPameters">An enumerable collection of named parameters used in the interpolation expressions.</param>
        /// <param name="defaultResult">An optional default result to return if no valid expressions are found. Default is null.</param>
        /// <param name="expressionsFoundRewriter">
        /// An optional function to rewrite the found expressions before evaluation. 
        /// It takes the index and expression and returns a modified expression string.
        /// </param>
        /// <param name="beginInvoke">
        /// An optional function that is called before invoking the expression. 
        /// It provides the index, the named parameters, the current expression, and the result type.
        /// </param>
        /// <param name="endInvoke">
        /// An optional function that is called after invoking the expression. 
        /// It provides the index, named parameters, current expression, result type, and the result value.
        /// This function can modify the result before returning it.
        /// </param>
        /// <returns>
        /// Returns a string that is the result of interpolating the original value using the provided named parameters. 
        /// If no valid interpolated values are found, the default result is returned.
        /// </returns>
        public static string? LinqInterpolation(this string value, IEnumerable<NamedPameters> namedPameters,
            string? defaultResult = default, Func<(int Index, string Expression), string>? expressionsFoundRewriter = null,
            Func<(int Index, IEnumerable<NamedPameters> NamedParameters, string Expression, Type ResultType), Type> beginInvoke = null,
            Func<(int Index, IEnumerable<NamedPameters> NamedParameters, string Expression, Type ResultType, object? ResultValue), object?> endInvoke = null)
        {
            expressionsFoundRewriter ??= ((int Index, string Expression) args) => args.Expression;
            beginInvoke ??= ((int Index, IEnumerable<NamedPameters> NamedParameters, string Expression, Type ResultType) args) => args.ResultType;
            endInvoke ??= ((int Index, IEnumerable<NamedPameters> NamedParameters, string Expression, Type ResultType, object? ResultValue) args) => args.ResultValue;

            var parameters = namedPameters.Select(s => Expression.Parameter(s.Type, s.Name)).ToArray();
            var args = namedPameters.Select(s => s.Value).ToArray();

            //var regex = new Regex(@"(?<!{)(?<!\\){(?!{)(?<exp>(?:[^{}]+|{(?1)})*)(?<!})}", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
            var regex = findExpression();

            var compiledCache = new Dictionary<string, Delegate>();
            var valuesCache = new Dictionary<string, object?>();

            var count = 0;
            return regex.Replace(value, match =>
            {
                var expression = match.Groups["exp"].Value;

                expression = expressionsFoundRewriter((count++, expression));

                var type = beginInvoke((count, namedPameters, expression, typeof(object)));

                var hash = (expression + type.GUID.ToString()).CalculateSha256Hash();
                if (!compiledCache.ContainsKey(hash))
                {
                    var lambda = System.Linq.Dynamic.Core.DynamicExpressionParser.ParseLambda(parameters, type, expression);
                    compiledCache.Add(hash, lambda.Compile());
                }

                var result = compiledCache[hash].DynamicInvoke(args);

                return endInvoke((count, namedPameters, expression, type, result))?.ToString() ?? defaultResult;
            });
        }
        /*/// <summary>
        /// Interpulate string with Linq expression using Dynamic Linq
        /// inside string use {expression} to interpolate the expression with the args
        /// </summary>
        /// <param name="value">string containing "{expression}"</param>
        /// <param name="defaultResult">default value for each interpulation</param>
        /// <param name="args">variables acessible</param>
        /// <returns></returns>
        public static string? LinqInterpolation(this string value, string? defaultResult = null, params object?[] args)
        {
            var parameters = args.Select(s => Expression.Parameter(s.GetType())).ToArray();

            //var regex = new Regex(@"(?<!{)(?<!\\){(?!{)(?<exp>(?:[^{}]+|{(?1)})*)(?<!})}", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
            var regex = findExpression();

            var compiledCache = new Dictionary<string, Delegate>();

            return regex.Replace(value, match =>
                {
                    var expression = match.Groups["exp"].Value;
                    var hash = expression.CalculateSha256Hash();
                    if (!compiledCache.ContainsKey(hash))
                    {
                        var lambda = System.Linq.Dynamic.Core.DynamicExpressionParser.ParseLambda(parameters, typeof(object), expression);
                        compiledCache.Add(hash, lambda.Compile());
                    }

                    return (compiledCache[hash].DynamicInvoke(args)?.ToString() ?? defaultResult);
                });
        }*/


        public static Dictionary<string, string> DiacriticsCharacters = new()
        {
            {"`", " "},
            {"^", " "},
            {"~", " "},
            {"´", " "},
            {"¨", " "},
            {"¯", " "},
            {"°", " "},
            {"ˇ", " "},
            {"ˆ", " "},
            {"ª", "a"},
            {"º", "o"},
            {"·", "."},
            {"äæǽ", "ae"},
            {"öœ", "oe"},
            {"ü", "ue"},
            {"Ä", "Ae"},
            {"Ü", "Ue"},
            {"Ö", "Oe"},
            {"ÀÁÂÃÄÅǺĀĂĄǍΑΆẢẠẦẪẨẬẰẮẴẲẶА", "A"},
            {"àáâãåǻāăąǎªαάảạầấẫẩậằắẵẳặа", "a"},
            {"Б", "B"},
            {"б", "b"},
            {"ÇĆĈĊČ", "C"},
            {"çćĉċč", "c"},
            {"Д", "D"},
            {"д", "d"},
            {"ÐĎĐΔ", "Dj"},
            {"ðďđδ", "dj"},
            {"ÈÉÊËĒĔĖĘĚΕΈẼẺẸỀẾỄỂỆЕЭ", "E"},
            {"èéêëēĕėęěέεẽẻẹềếễểệеэ", "e"},
            {"Ф", "F"},
            {"ф", "f"},
            {"ĜĞĠĢΓГҐ", "G"},
            {"ĝğġģγгґ", "g"},
            {"ĤĦ", "H"},
            {"ĥħ", "h"},
            {"ÌÍÎÏĨĪĬǏĮİΗΉΊΙΪỈỊИЫ", "I"},
            {"ìíîïĩīĭǐįıηήίιϊỉịиыї", "i"},
            {"Ĵ", "J"},
            {"ĵ", "j"},
            {"ĶΚК", "K"},
            {"ķκк", "k"},
            {"ĹĻĽĿŁΛЛ", "L"},
            {"ĺļľŀłλл", "l"},
            {"М", "M"},
            {"м", "m"},
            {"ÑŃŅŇΝН", "N"},
            {"ñńņňŉνн", "n"},
            {"ÒÓÔÕŌŎǑŐƠØǾΟΌΩΏỎỌỒỐỖỔỘỜỚỠỞỢО", "O"},
            {"òóôõōŏǒőơøǿºοόωώỏọồốỗổộờớỡởợо", "o"},
            {"П", "P"},
            {"п", "p"},
            {"ŔŖŘΡР", "R"},
            {"ŕŗřρр", "r"},
            {"ŚŜŞȘŠΣС", "S"},
            {"śŝşșšſσςс", "s"},
            {"ȚŢŤŦτТ", "T"},
            {"țţťŧт", "t"},
            {"ÙÚÛŨŪŬŮŰŲƯǓǕǗǙǛŨỦỤỪỨỮỬỰУ", "U"},
            {"ùúûũūŭůűųưǔǖǘǚǜυύϋủụừứữửựу", "u"},
            {"ÝŸŶΥΎΫỲỸỶỴЙ", "Y"},
            {"ýÿŷỳỹỷỵй", "y"},
            {"В", "V"},
            {"в", "v"},
            {"Ŵ", "W"},
            {"ŵ", "w"},
            {"ŹŻŽΖЗ", "Z"},
            {"źżžζз", "z"},
            {"ÆǼ", "AE"},
            {"ß", "ss"},
            {"Ĳ", "IJ"},
            {"ĳ", "ij"},
            {"Œ", "OE"},
            {"ƒ", "f"},
            {"ξ", "ks"},
            {"π", "p"},
            {"β", "v"},
            {"μ", "m"},
            {"ψ", "ps"},
            {"Ё", "Yo"},
            {"ё", "yo"},
            {"Є", "Ye"},
            {"є", "ye"},
            {"Ї", "Yi"},
            {"Ж", "Zh"},
            {"ж", "zh"},
            {"Х", "Kh"},
            {"х", "kh"},
            {"Ц", "Ts"},
            {"ц", "ts"},
            {"Ч", "Ch"},
            {"ч", "ch"},
            {"Ш", "Sh"},
            {"ш", "sh"},
            {"Щ", "Shch"},
            {"щ", "shch"},
            {"ЪъЬь", ""},
            {"Ю", "Yu"},
            {"ю", "yu"},
            {"Я", "Ya"},
            {"я", "ya"}
        };

        /// <summary>
        /// Removes diacritics from the specified character by checking against a predefined 
        /// collection of characters with diacritics (DiacriticsCharacters). If a matching 
        /// diacritic character is found, it returns the corresponding character without diacritics.
        /// </summary>
        /// <param name="char">The character from which to remove the diacritics.</param>
        /// <returns>
        /// Returns the character without diacritics if a match is found; otherwise, 
        /// returns the original character unmodified.
        /// </returns>
        public static char RemoveDiacritics(this char @char)
        {
            foreach (var entry in DiacriticsCharacters)
            {
                if (entry.Key.IndexOf(@char) != -1)
                {
                    return entry.Value[0];
                }
            }

            return @char;
        }

        /// <summary>
        /// Replaces all occurrences of a specified string (oldValue) with a new string (newValue) in the source string,
        /// until no more replacements can be made based on the specified string comparison rules.
        /// </summary>
        /// <param name="source">The input string in which replacements are to be made.</param>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace oldValue with.</param>
        /// <param name="comparisonType">The type of comparison to use when searching for oldValue.</param>
        /// <param name="newSource">The output parameter that will hold the modified string after replacements.</param>
        /// <returns>The number of replacements made during the operation.</returns>
        /// <exception cref="ArgumentException">Thrown when an invalid comparison type is specified.</exception>
        public static int ReplaceUntil(this string source, string oldValue, string newValue, StringComparison comparisonType, out string newSource)
        {
            var result = 0;

            var transientSource = new StringBuilder(source);


            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                {
                    while ((result += ReplaceUntil(transientSource.ToString(), oldValue, newValue, CultureInfo.CurrentCulture.CompareInfo, CompareOptions.Ordinal, out var stringBuilder)) > 0)
                    {
                        transientSource = stringBuilder;
                    }
                }
                break;
                case StringComparison.CurrentCultureIgnoreCase:
                {
                    while ((result += ReplaceUntil(transientSource.ToString(), oldValue, newValue, CultureInfo.CurrentCulture.CompareInfo, CompareOptions.OrdinalIgnoreCase, out var stringBuilder)) > 0)
                    {
                        transientSource = stringBuilder;
                    }
                }
                break;
                case StringComparison.InvariantCulture:
                {
                    while ((result += ReplaceUntil(transientSource.ToString(), oldValue, newValue, CultureInfo.InvariantCulture.CompareInfo, CompareOptions.Ordinal, out var stringBuilder)) > 0)
                    {
                        transientSource = stringBuilder;
                    }
                }
                break;
                case StringComparison.InvariantCultureIgnoreCase:
                {
                    while ((result += ReplaceUntil(transientSource.ToString(), oldValue, newValue, CultureInfo.InvariantCulture.CompareInfo, CompareOptions.OrdinalIgnoreCase, out var stringBuilder)) > 0)
                    {
                        transientSource = stringBuilder;
                    }
                }
                break;
                case StringComparison.Ordinal:
                {
                    while ((result += ReplaceUntil(transientSource.ToString(), oldValue, newValue, CultureInfo.InvariantCulture.CompareInfo, CompareOptions.Ordinal, out var stringBuilder)) > 0)
                    {
                        transientSource = stringBuilder;
                    }
                }
                break;
                case StringComparison.OrdinalIgnoreCase:
                {
                    while ((result += ReplaceUntil(transientSource.ToString(), oldValue, newValue, CultureInfo.InvariantCulture.CompareInfo, CompareOptions.OrdinalIgnoreCase, out var stringBuilder)) > 0)
                    {
                        transientSource = stringBuilder;
                    }
                }
                break;
                default:
                    throw new ArgumentException(null, nameof(comparisonType));
            }

            newSource = transientSource.ToString();

            return result;
        }

        /// <summary>
        /// Replaces all occurrences of a specified substring within a given read-only span 
        /// of characters with a new substring and appends the modified content to a 
        /// StringBuilder instance. The comparison for the old value is performed based 
        /// on specified comparison options.
        /// </summary>
        /// <param name="searchSpace">
        /// The read-only span of characters to search within.
        /// </param>
        /// <param name="oldValue">
        /// The read-only span of characters that represents the substring to be replaced.
        /// </param>
        /// <param name="newValue">
        /// The read-only span of characters that represents the substring to replace with.
        /// </param>
        /// <param name="compareInfo">
        /// An instance of CompareInfo that provides culture-specific comparisons.
        /// </param>
        /// <param name="options">
        /// A CompareOptions value that specifies how to perform the comparison.
        /// </param>
        /// <param name="stringBuilder">
        /// An output parameter that, on successful completion, contains the modified string 
        /// with all replacements made.
        /// </param>
        /// <returns>
        /// The total number of replacements made in the search space.
        /// </returns>
        public static int ReplaceUntil(ReadOnlySpan<char> searchSpace, ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue, CompareInfo compareInfo, CompareOptions options, out StringBuilder stringBuilder)
        {
            Debug.Assert(!oldValue.IsEmpty);
            Debug.Assert(compareInfo != null);

            stringBuilder = new StringBuilder(searchSpace.Length);

            var replacements = 0;

            while (true)
            {
                var index = compareInfo.IndexOf(searchSpace, oldValue, options, out var matchLength);


                if (index < 0 || matchLength == 0)
                {
                    break;
                }

                // append the unmodified portion of search space
                stringBuilder.Append(searchSpace[..index]);

                // append the replacement
                stringBuilder.Append(newValue);

                searchSpace = searchSpace[(index + matchLength)..];
                replacements++;
            }

            stringBuilder.Append(searchSpace);


            return replacements;
        }



        /// <summary>
        /// Removes diacritics from the given string.
        /// </summary>
        /// <param name="string">The input string from which diacritics will be removed.</param>
        /// <returns>
        /// A new string that has all diacritics removed from the original string.
        /// </returns>
        public static string RemoveDiacritics(this string @string)
        {
            //StringBuilder sb = new StringBuilder ();
            var text = "";


            foreach (var c in @string)
            {
                var len = text.Length;

                foreach (var entry in DiacriticsCharacters)
                {
                    if (entry.Key.IndexOf(c) != -1)
                    {
                        text += entry.Value;
                        break;
                    }
                }

                if (len == text.Length)
                {
                    text += c;
                }
            }

            return text;
        }

        /// <summary>
        /// Compares two <see cref="SecureString"/> instances for equality.
        /// </summary>
        /// <param name="ss1">The first <see cref="SecureString"/> instance to compare.</param>
        /// <param name="ss2">The second <see cref="SecureString"/> instance to compare.</param>
        /// <returns>
        /// Returns <c>true</c> if the two <see cref="SecureString"/> instances are equal in content; otherwise, <c>false</c>.
        /// </returns>
        public static bool Equal(this SecureString ss1, SecureString ss2)
        {
            var bstr1 = nint.Zero;
            var bstr2 = nint.Zero;
            try
            {
                bstr1 = Marshal.SecureStringToBSTR(ss1);
                bstr2 = Marshal.SecureStringToBSTR(ss2);
                var length1 = Marshal.ReadInt32(bstr1, -4);
                var length2 = Marshal.ReadInt32(bstr2, -4);
                if (length1 == length2)
                {
                    for (var x = 0; x < length1; ++x)
                    {
                        var b1 = Marshal.ReadByte(bstr1, x);
                        var b2 = Marshal.ReadByte(bstr2, x);
                        if (b1 != b2)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }

                return true;
            }
            finally
            {
                if (bstr2 != nint.Zero)
                {
                    Marshal.ZeroFreeBSTR(bstr2);
                }

                if (bstr1 != nint.Zero)
                {
                    Marshal.ZeroFreeBSTR(bstr1);
                }
            }
        }

        /// <summary>
        /// Converts a SecureString to a regular string.
        /// </summary>
        /// <param name="value">The SecureString to convert.</param>
        /// <returns>A string representation of the SecureString.</returns>
        public static string ConvertToString(this SecureString value)
        {
            var bstr = Marshal.SecureStringToBSTR(value);

            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }

        /// <summary>
        /// Converts a regular string to a <see cref="SecureString"/>.
        /// This method creates a secure string that can be used to store sensitive information securely in memory.
        /// </summary>
        /// <param name="str">
        /// The input string that will be converted to <see cref="SecureString"/>.
        /// Must not be null or empty.
        /// </param>
        /// <returns>
        /// A <see cref="SecureString"/> containing the characters from the input string.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the input string is null or consists only of white-space characters.
        /// </exception>
        public static SecureString ToSecureString(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentNullException("String is null or empty.");
            }

            var result = new SecureString();

            foreach (var c in str)
            {
                result.AppendChar(c);
            }

            result.MakeReadOnly();

            return result;
        }

        
        // value is slower, but more reliable (especially UTF-8 with special characters
        // later on may appear to be ASCII initially). If taster = 0, then taster
        // becomes the length of the byte array (for maximum reliability).</param>
        /// <returns></returns>
        /// <summary>
        /// Detects the encoding of a byte array by checking for a Byte Order Mark (BOM) 
        /// or by analyzing the byte patterns to identify the encoding. 
        /// The function can take a 'taster' parameter to determine how much of the byte 
        /// array should be analyzed for encoding detection.
        /// </summary>
        /// <param name="bytes">
        /// The byte array to analyze for encoding detection. This array should not be null.
        /// </param>
        /// <param name="taster">
        /// The number of bytes to sample for encoding detection. If set to 0, the whole
        /// byte array length is used. The default value is 0.
        /// </param>
        /// <returns>
        /// The detected <see cref="Encoding"/> of the input byte array. Returns the
        /// default encoding if no other encoding can be reliably detected.
        /// </returns>
        public static Encoding DetectEncoding([NotNull] this byte[] bytes, /*out string text,*/ int taster = 0)
        {
            //byte[] b = System.Text.Encoding.
            //byte[] b = File.ReadAllBytes(filename);

            //////////////// First check the low hanging fruit by checking if a
            //////////////// BOM/signature exists (sourced from http://www.unicode.org/faq/utf_bom.html#bom4)
            if (bytes.Length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
            {
                //text = Encoding.GetEncoding("utf-32BE").GetString(bytes, 4, bytes.Length - 4);
                return Encoding.GetEncoding("utf-32BE");
            }  // UTF-32, big-endian 
            else if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
            {
                //text = Encoding.UTF32.GetString(bytes, 4, bytes.Length - 4);
                return Encoding.UTF32;
            }    // UTF-32, little-endian
            else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                //text = Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
                return Encoding.BigEndianUnicode;
            }     // UTF-16, big-endian
            else if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                //text = Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
                return Encoding.Unicode;
            }              // UTF-16, little-endian
            else if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                //text = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                return Encoding.UTF8;
            } // UTF-8
            else if (bytes.Length >= 3 && bytes[0] == 0x2b && bytes[1] == 0x2f && bytes[2] == 0x76)
            {
                //text = Encoding.UTF7.GetString(bytes, 3, bytes.Length - 3);
                return Encoding.UTF7;
            } // UTF-7


            //////////// If the code reaches here, no BOM/signature was found, so now
            //////////// we need to 'taste' the file to see if can manually discover
            //////////// the encoding. A high taster value is desired for UTF-8
            if (taster == 0 || taster > bytes.Length)
                taster = bytes.Length;    // Taster size can't be bigger than the filesize obviously.


            // Some text files are encoded in UTF8, but have no BOM/signature. Hence
            // the below manually checks for a UTF8 pattern. This code is based off
            // the top answer at: https://stackoverflow.com/questions/6555015/check-for-invalid-utf8
            // For our purposes, an unnecessarily strict (and terser/slower)
            // implementation is shown at: https://stackoverflow.com/questions/1031645/how-to-detect-utf-8-in-plain-c
            // For the below, false positives should be exceedingly rare (and would
            // be either slightly malformed UTF-8 (which would suit our purposes
            // anyway) or 8-bit extended ASCII/UTF-16/32 at a vanishingly long shot).
            int i = 0;
            bool utf8 = false;
            while (i < taster - 4)
            {
                if (bytes[i] <= 0x7F)
                {
                    i += 1;
                    continue;
                }     // If all characters are below 0x80, then it is valid UTF8, but UTF8 is not 'required' (and therefore the text is more desirable to be treated as the default codepage of the computer). Hence, there's no "utf8 = true;" code unlike the next three checks.
                if (bytes[i] >= 0xC2 && bytes[i] < 0xE0 && bytes[i + 1] >= 0x80 && bytes[i + 1] < 0xC0)
                {
                    i += 2;
                    utf8 = true;
                    continue;
                }
                if (bytes[i] >= 0xE0 && bytes[i] < 0xF0 && bytes[i + 1] >= 0x80 && bytes[i + 1] < 0xC0 && bytes[i + 2] >= 0x80 && bytes[i + 2] < 0xC0)
                {
                    i += 3;
                    utf8 = true;
                    continue;
                }
                if (bytes[i] >= 0xF0 && bytes[i] < 0xF5 && bytes[i + 1] >= 0x80 && bytes[i + 1] < 0xC0 && bytes[i + 2] >= 0x80 && bytes[i + 2] < 0xC0 && bytes[i + 3] >= 0x80 && bytes[i + 3] < 0xC0)
                {
                    i += 4;
                    utf8 = true;
                    continue;
                }
                utf8 = false;
                break;
            }
            if (utf8 == true)
            {
                //text = Encoding.UTF8.GetString(bytes);
                return Encoding.UTF8;
            }


            // The next check is a heuristic attempt to detect UTF-16 without a BOM.
            // We simply look for zeroes in odd or even byte places, and if a certain
            // threshold is reached, the code is 'probably' UF-16.          
            double threshold = 0.1; // proportion of chars step 2 which must be zeroed to be diagnosed as utf-16. 0.1 = 10%
            int count = 0;
            for (int n = 0; n < taster; n += 2)
                if (bytes[n] == 0)
                    count++;
            if (((double)count) / taster > threshold)
            {
                //text = Encoding.BigEndianUnicode.GetString(bytes);
                return Encoding.BigEndianUnicode;
            }
            count = 0;
            for (int n = 1; n < taster; n += 2)
                if (bytes[n] == 0)
                    count++;
            if (((double)count) / taster > threshold)
            {
                //text = Encoding.Unicode.GetString(bytes);
                return Encoding.Unicode;
            } // (little-endian)


            // Finally, a long shot - let's see if we can find "charset=xyz" or
            // "encoding=xyz" to identify the encoding:
            for (int n = 0; n < taster - 9; n++)
            {
                if (
                    ((bytes[n + 0] == 'c' || bytes[n + 0] == 'C') && (bytes[n + 1] == 'h' || bytes[n + 1] == 'H') && (bytes[n + 2] == 'a' || bytes[n + 2] == 'A') && (bytes[n + 3] == 'r' || bytes[n + 3] == 'R') && (bytes[n + 4] == 's' || bytes[n + 4] == 'S') && (bytes[n + 5] == 'e' || bytes[n + 5] == 'E') && (bytes[n + 6] == 't' || bytes[n + 6] == 'T') && (bytes[n + 7] == '=')) ||
                    ((bytes[n + 0] == 'e' || bytes[n + 0] == 'E') && (bytes[n + 1] == 'n' || bytes[n + 1] == 'N') && (bytes[n + 2] == 'c' || bytes[n + 2] == 'C') && (bytes[n + 3] == 'o' || bytes[n + 3] == 'O') && (bytes[n + 4] == 'd' || bytes[n + 4] == 'D') && (bytes[n + 5] == 'i' || bytes[n + 5] == 'I') && (bytes[n + 6] == 'n' || bytes[n + 6] == 'N') && (bytes[n + 7] == 'g' || bytes[n + 7] == 'G') && (bytes[n + 8] == '='))
                    )
                {
                    if (bytes[n + 0] == 'c' || bytes[n + 0] == 'C')
                        n += 8;
                    else
                        n += 9;
                    if (bytes[n] == '"' || bytes[n] == '\'')
                        n++;
                    int oldn = n;
                    while (n < taster && (bytes[n] == '_' || bytes[n] == '-' || (bytes[n] >= '0' && bytes[n] <= '9') || (bytes[n] >= 'a' && bytes[n] <= 'z') || (bytes[n] >= 'A' && bytes[n] <= 'Z')))
                    {
                        n++;
                    }
                    byte[] nb = new byte[n - oldn];
                    Array.Copy(bytes, oldn, nb, 0, n - oldn);
                    try
                    {
                        string internalEnc = Encoding.ASCII.GetString(nb);
                        //text = Encoding.GetEncoding(internalEnc).GetString(bytes);
                        return Encoding.GetEncoding(internalEnc);
                    }
                    catch { break; }    // If C# doesn't recognize the name of the encoding, break.
                }
            }


            // If all else fails, the encoding is probably (though certainly not
            // definitely) the user's local codepage! One might present to the user a
            // list of alternative encodings as shown here: https://stackoverflow.com/questions/8509339/what-is-the-most-common-encoding-of-each-language
            // A full list can be found using Encoding.GetEncodings();
            //text = Encoding.Default.GetString(bytes);
            return Encoding.Default;
        }


        /// <summary>
        /// Creates a clone of the specified string using the specified encoding.
        /// If the source string is null, the method returns null.
        /// If no encoding is specified, UTF8 encoding is used by default.
        /// </summary>
        /// <param name="source">The string to clone. This can be null.</param>
        /// <param name="encoding">The optional encoding to use for the cloning process. Defaults to UTF8 if not specified.</param>
        /// <returns>
        /// A new string that is a copy of the source string, 
        /// or null if the source string is null.
        /// </returns>
        public static string? Clone(this string? source, Encoding? encoding = null)
        {
            if(source == null)
                return null;

            encoding ??= Encoding.UTF8;

            var sourceByteArray = encoding.GetBytes(source);
            var destinationByteArray = new byte[sourceByteArray.Length];

            sourceByteArray.CopyTo(destinationByteArray, 0);

            return encoding.GetString(destinationByteArray);
        }
    }
}
