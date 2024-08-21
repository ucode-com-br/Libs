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
    public static partial class StringExtensions
    {
        [GeneratedRegex(@"(?<!{)(?<!\\\\){(?!{)(?<exp>(?:[^{}]+|{(\1)})*)(?<!})}", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled, "pt-BR")]
        private static partial Regex findExpression();



        public record NamedPameters : NamedPameters<object>
        {
            public NamedPameters([NotNull] object instance) : base(instance)
            {

            }


            public NamedPameters(Type type, object? instance) : base(type, instance)
            {

            }

            public NamedPameters(string name, Type type, object? instance) : base(name, type, instance)
            {

            }
        }
        public record NamedPameters<T>
        {
            private readonly T? _value;

            [NotNull]
            public string Name
            {
                get; init;
            }

            [NotNull]
            public Type Type
            {
                get; init;
            }


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

            [MaybeNull]
            public T? DefaultValue
            {
                get; set;
            }


            public NamedPameters([NotNull] T value, [MaybeNull] T? defaultValue = default) : this(value.GetType().Name, value.GetType(), value, defaultValue)
            {

            }


            public NamedPameters([NotNull] Type type, [NotNull] T? value, [MaybeNull] T? defaultValue = default) : this(type.Name, type, value, defaultValue)
            {

            }

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
        ///     Remove caracteres com acentos ou ilegiveis, que são os seguintes:
        ///     `^~´¨¯°ˇˆªº·äæǽöœÄÜÖÀÁÂÃÄÅǺĀĂĄǍΑΆẢẠẦẪẨẬẰẮẴẲẶАàáâãåǻāăąǎªαάảạầấẫẩậằắẵẳặаБбÇĆĈĊČçćĉċčДдÐĎĐΔðďđδÈÉÊËĒĔĖĘĚΕΈẼẺẸỀẾỄỂỆЕЭèéêëēĕėęěέεẽẻẹềếễểệеэФфĜĞĠĢΓГҐĝğġģγгґĤĦĥħÌÍÎÏĨĪĬǏĮİΗΉΊΙΪỈỊИЫìíîïĩīĭǐįıηήίιϊỉịиыїĴĵĶΚКķκкĹĻĽĿŁΛЛĺļľŀłλлМмÑŃŅŇΝНñńņňŉνнÒÓÔÕŌŎǑŐƠØǾΟΌΩΏỎỌỒỐỖỔỘỜỚỠỞỢОòóôõōŏǒőơøǿºοόωώỏọồốỗổộờớỡởợоПпŔŖŘΡРŕŗřρрŚŜŞȘŠΣСśŝşșšſσςсȚŢŤŦτТțţťŧтÙÚÛŨŪŬŮŰŲƯǓǕǗǙǛŨỦỤỪỨỮỬỰУùúûũūŭůűųưǔǖǘǚǜυύϋủụừứữửựуÝŸŶΥΎΫỲỸỶỴЙýÿŷỳỹỷỵйВвŴŵŹŻŽΖЗźżžζзÆǼßĲĳŒƒξπβμψЁёЄєЇЖжХхЦцЧчШшЩщЪъЬьЮюЯя
        /// </summary>
        /// <param name="char"></param>
        /// <returns></returns>
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
        ///     Remove caracteres com acentos ou ilegiveis, que são os seguintes:
        ///     `^~´¨¯°ˇˆªº·äæǽöœÄÜÖÀÁÂÃÄÅǺĀĂĄǍΑΆẢẠẦẪẨẬẰẮẴẲẶАàáâãåǻāăąǎªαάảạầấẫẩậằắẵẳặаБбÇĆĈĊČçćĉċčДдÐĎĐΔðďđδÈÉÊËĒĔĖĘĚΕΈẼẺẸỀẾỄỂỆЕЭèéêëēĕėęěέεẽẻẹềếễểệеэФфĜĞĠĢΓГҐĝğġģγгґĤĦĥħÌÍÎÏĨĪĬǏĮİΗΉΊΙΪỈỊИЫìíîïĩīĭǐįıηήίιϊỉịиыїĴĵĶΚКķκкĹĻĽĿŁΛЛĺļľŀłλлМмÑŃŅŇΝНñńņňŉνнÒÓÔÕŌŎǑŐƠØǾΟΌΩΏỎỌỒỐỖỔỘỜỚỠỞỢОòóôõōŏǒőơøǿºοόωώỏọồốỗổộờớỡởợоПпŔŖŘΡРŕŗřρрŚŜŞȘŠΣСśŝşșšſσςсȚŢŤŦτТțţťŧтÙÚÛŨŪŬŮŰŲƯǓǕǗǙǛŨỦỤỪỨỮỬỰУùúûũūŭůűųưǔǖǘǚǜυύϋủụừứữửựуÝŸŶΥΎΫỲỸỶỴЙýÿŷỳỹỷỵйВвŴŵŹŻŽΖЗźżžζзÆǼßĲĳŒƒξπβμψЁёЄєЇЖжХхЦцЧчШшЩщЪъЬьЮюЯя
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
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

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes">byte array</param>
        /// <param name="taster">number of bytes to check of the file (to save processing). Higher
        // value is slower, but more reliable (especially UTF-8 with special characters
        // later on may appear to be ASCII initially). If taster = 0, then taster
        // becomes the length of the byte array (for maximum reliability).</param>
        /// <returns></returns>
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
        /// Clone string using encoding
        /// </summary>
        /// <param name="source">source of string to copy</param>
        /// <param name="encoding">Encoder for clone (default: UTF8)</param>
        /// <returns></returns>
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
