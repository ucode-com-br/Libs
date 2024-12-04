using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.Web;
using System.Collections;
using System.Reflection.Metadata;
using System.Text.Json;

namespace UCode.Extensions
{
    /// <summary>
    /// Provides extension methods for manipulating and working with text strings.
    /// </summary>
    /// <remarks>
    /// This static class includes various utility functions that enhance the 
    /// functionality of the <see cref="string"/> class. It allows for cleaner 
    /// and more readable code when performing common string operations.
    /// </remarks>
    public static class TextExtensions
    {
        /// <summary>
        /// Encodes a specified string to a URL-safe format.
        ///
        /// This method is an extension method for the string class. It utilizes the
        /// `HttpUtility.UrlEncode` method to convert the provided string into a format
        /// that can be safely transmitted in a URL. If the input string is null,
        /// the method returns null.
        ///
        /// </summary>
        /// <param name="source">
        /// The string to be encoded. This parameter can be null.
        /// </param>
        /// <returns>
        /// A URL-encoded string, or null if the input source is null.
        /// </returns>
        public static string? UrlEncode(this string? source) => source == null ? null : HttpUtility.UrlEncode(source);

        /// <summary>
        /// Extends the <see cref="string"/> class to provide a method for URL decoding.
        /// </summary>
        /// <param name="source">The URL-encoded string to decode. If <c>null</c>, the result will also be <c>null</c>.</param>
        /// <returns>
        /// Returns the decoded string, or <c>null</c> if <paramref name="source"/> is <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This method uses the <see cref="HttpUtility.UrlDecode(string)"/> method 
        /// to perform the decoding of the URL-encoded string.
        /// </remarks>
        public static string? UrlDecode(this string? source) => source == null ? null : HttpUtility.UrlDecode(source);

        /// <summary>
        /// Encodes a specified string by replacing each occurrence of a predefined set of characters 
        /// with their corresponding HTML entity representations. If the input string is null, it returns null.
        /// </summary>
        /// <param name="source">The string to be HTML encoded. Can be null.</param>
        /// <returns>
        /// A string that represents the HTML-encoded version of the input string, 
        /// or null if the input string is null.
        /// </returns>
        public static string? HtmlEncode(this string? source) => source == null ? null : HttpUtility.HtmlDecode(source);

        /// <summary>
        /// Decodes a string that has been encoded for HTML display.
        /// This method returns the decoded version of the input string,
        /// or null if the input is also null.
        /// </summary>
        /// <param name="source">
        /// The string that is to be decoded. This can be null.
        /// </param>
        /// <returns>
        /// The decoded string if the input is not null; otherwise, null.
        /// </returns>
        /// <remarks>
        /// This method utilizes the HttpUtility.HtmlDecode method 
        /// to decode HTML-encoded strings, converting entities such as &amp;,
        /// &lt;, and &gt; back to their respective characters.
        /// </remarks>
        public static string? HtmlDecode(this string? source) => source == null ? null : HttpUtility.HtmlDecode(source);


        /// <summary>
        /// Converts the provided object to a query string.
        /// </summary>
        /// <typeparam name="T">The type of the source object.</typeparam>
        /// <param name="source">The source object to be converted to a query string. It can be null.</param>
        /// <returns>
        /// A query string representation of the object, or null if the source is null.
        /// </returns>
        public static string? ToQueryString<T>(this T? source)
        {
            if (source == null)
                return null;

            var jsonDoc = JsonDocument.Parse(JsonSerializer.Serialize(source));

            var jsonRoot = jsonDoc.RootElement;



            /*var names = jsonRoot
                .EnumerateArray()
                .SelectMany(o => o.EnumerateObject())
                .Select(p => p.Name);

            return string.Join("&", names.Select(s=> $"{s}={UrlEncode(jsonRoot.GetProperty(s).GetString())}" ));*/

            return ToQueryString(jsonRoot, null);
        }

        /// <summary>
        /// Converts a JSON element to a query string representation.
        /// This method handles various JSON value types, including null, string, array, boolean, number, and object.
        /// </summary>
        /// <param name="obj">The JSON element to convert, which may be null.</param>
        /// <param name="name">The name to associate with the JSON element in the query string.</param>
        /// <returns>A query string representation of the JSON element, or null if the input is null.</returns>
        private static string? ToQueryString(JsonElement? obj, string? name)
        {
            switch (obj?.ValueKind)
            {
                case JsonValueKind.Null:
                    return $"{name}=";
                case JsonValueKind.String:
                    return $"{name}={UrlEncode(obj?.GetString())}";
                case JsonValueKind.Array:
                    int pos0 = 0;
                    var kv = obj?.EnumerateArray().Select(s => ToQueryString(s, $"{name}{pos0++}"));
                    return string.Join("&", kv);
                case JsonValueKind.True:
                    return $"{name}=true";
                case JsonValueKind.False:
                    return $"{name}=false";
                case JsonValueKind.Number:
                    return $"{name}={UrlEncode(obj?.GetRawText())}";
                case JsonValueKind.Undefined:
                    return $"{name}=";
                case JsonValueKind.Object:
                    var pos1 = 0;
                    var kv1 = obj?.EnumerateObject().Select(p => {
                        var keyName = $"{name}{(!string.IsNullOrWhiteSpace(name) ? pos1++ : "")}{(!string.IsNullOrWhiteSpace(name) ? "." : "")}{p.Name}";
                        var value = ToQueryString(obj?.GetProperty(p.Name), keyName);
                        return $"{value}";
                    });
                    return string.Join("&", kv1);
                default:
                    return $"{name}={UrlEncode(obj?.GetRawText())}";
            }
        }


        /// <summary>
        /// Converts a string to its Soundex representation specific to Brazilian Portuguese.
        /// Soundex is a phonetic algorithm that indexes words by their sounds when pronounced in English.
        /// This method processes the input string to eliminate vowels and certain letter combinations,
        /// replacing them with specific characters to produce a sound-like representation.
        /// </summary>
        /// <param name="source">The input string to be converted to Soundex.</param>
        /// <returns>
        /// A Soundex representation of the input string as a string.
        /// The output will primarily consist of consonants with certain rules applied to eliminate 
        /// specific characters and group similar sounding characters together.
        /// </returns>
        public static string SoundexPtBr(this string source)
        {
            StringBuilder sb;

            sb = new StringBuilder();

            #region Remover caracteres especiais

            foreach (var c in source.ToUpper(System.Globalization.CultureInfo.CurrentCulture))
            {
                if (c is (>= '0' and <= '9') or (>= 'A' and <= 'Z'))
                {
                    sb.Append(c);
                }
            }

            #endregion Remover caracteres especiais

            var rm = 0;
            while (rm < sb.Length)
            {
                rm++;

                while (rm < sb.Length && sb[rm - 1] == sb[rm])
                {
                    sb.Remove(rm, 1);
                }
            }


            sb.Replace("Á", "A");

            sb.Replace("À", "A");

            sb.Replace("Ã", "A");

            sb.Replace("Ê", "E");

            sb.Replace("É", "E");

            sb.Replace("Í", "I");

            sb.Replace("Ó", "O");

            sb.Replace("Õ", "O");

            sb.Replace("Ú", "U");

            sb.Replace("Y", "I");

            sb.Replace("BR", "B");

            sb.Replace("BL", "B");

            sb.Replace("PH", "F");

            sb.Replace("MG", "G");

            sb.Replace("NG", "G");

            sb.Replace("RG", "G");

            sb.Replace("GE", "J");

            sb.Replace("GI", "J");

            sb.Replace("RJ", "J");

            sb.Replace("MJ", "J");

            sb.Replace("NJ", "J");

            sb.Replace("GR", "G");

            sb.Replace("GL", "G");

            sb.Replace("CE", "S");

            //sb.Replace("CI", "S");
            sb.Replace("CI", "SI");

            sb.Replace("CH", "S");

            sb.Replace("CT", "T");

            sb.Replace("CS", "S");

            sb.Replace("Q", "K");

            sb.Replace("CA", "K");

            sb.Replace("CO", "K");

            sb.Replace("CU", "K");

            sb.Replace("CK", "K");

            sb.Replace("C", "K");

            sb.Replace("LH", "L");

            sb.Replace("RM", "SM");

            sb.Replace("N", "M");

            sb.Replace("GM", "M");

            sb.Replace("MD", "M");

            sb.Replace("NH", "N");

            sb.Replace("PR", "P");

            sb.Replace("X", "S");

            sb.Replace("TS", "S");

            sb.Replace("C", "S");

            sb.Replace("Z", "S");

            sb.Replace("RS", "S");

            sb.Replace("TR", "T");

            sb.Replace("TL", "T");

            sb.Replace("LT", "T");

            sb.Replace("RT", "T");

            sb.Replace("ST", "T");

            sb.Replace("W", "V");

            #region Tratar terminações

            var tam = sb.Length - 1;

            if (tam > -1)
            {
                if (sb[tam] is 'S' or 'Z' or 'R' or 'M' or 'N' or
                    'L')
                {
                    sb.Remove(tam, 1);
                }
            }

            tam = sb.Length - 2;


            if (tam > -1)
            {
                if (sb[tam] == 'A' && sb[tam + 1] == 'O')
                {
                    sb.Remove(tam, 2);
                }
            }

            #endregion Tratar terminações

            #region

            //sb.Replace("VA", "V");
            //sb.Replace("VE", "V");
            //sb.Replace("VI", "V");
            //sb.Replace("VO", "V");
            //sb.Replace("VU", "V");

            //sb.Replace("DA", "D");
            //sb.Replace("DE", "D");
            //sb.Replace("DI", "D");
            //sb.Replace("DO", "D");
            //sb.Replace("DU", "D");

            //sb.Replace("KA", "K");

            #endregion


            sb.Replace("Ç", "S");
            sb.Replace("L", "R");


            #region Remover Vogais

            sb.Replace("A", "");

            sb.Replace("E", "");

            sb.Replace("I", "");

            sb.Replace("O", "");

            sb.Replace("U", "");

            #endregion

            sb.Replace("H", "");

            var frasesaida = new StringBuilder();

            frasesaida.Append(sb[0]);


            for (var i = 1; i <= sb.Length - 1; i += 1)
            {
                if (frasesaida[^1] != sb[i] || char.IsDigit(sb[i]))
                {
                    frasesaida.Append(sb[i]);
                }
            }

            return sb.ToString();
        }
    }
}
