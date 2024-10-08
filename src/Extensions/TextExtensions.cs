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
    public static class TextExtensions
    {
        public static string? UrlEncode(this string? source) => source == null ? null : HttpUtility.UrlEncode(source);

        public static string? UrlDecode(this string? source) => source == null ? null : HttpUtility.UrlDecode(source);

        public static string? HtmlEncode(this string? source) => source == null ? null : HttpUtility.HtmlDecode(source);

        public static string? HtmlDecode(this string? source) => source == null ? null : HttpUtility.HtmlDecode(source);


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
