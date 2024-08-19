using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
//using Newtonsoft.Json;

namespace UCode.Extensions
{
    /// <summary>
    /// Extension of types
    /// </summary>
    public static class TypesExtensions
    {
        [return: NotNull]
        public static byte[] JoinByteArrays([NotNull] this byte[] @base, [NotNull] params byte[][] parameters)
        {
            var rv = new byte[@base.Length + parameters.Sum(a => a.Length)];
            Buffer.BlockCopy(@base, 0, rv, 0, @base.Length);
            var offset = @base.Length;
            foreach (var array in parameters)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }

            return rv;
        }

        public static TDestination? ConvertWithJson<TDestination>([NotNull] this object? source)
        {
            if (source == null)
            {
                return default;
            }

            var json = System.Text.Json.JsonSerializer.Serialize(source);

            return System.Text.Json.JsonSerializer.Deserialize<TDestination>(json);
        }

        [return: NotNull]
        public static void Remove<T>([NotNull] this ConcurrentBag<T> bag, T item)
        {
            while (!bag.IsEmpty)
            {
                bag.TryTake(out var result);

                if (result != null)
                {
                    if (result.Equals(item))
                    {
                        break;
                    }

                    bag.Add(result);
                }
            }
        }




        public static TOut FirstNotNullOrDefault<TIn, TOut>([NotNull] this TIn source, params TIn?[] alternatives)
        {
            var lObjects = new List<TIn>
            {
                source
            };

            foreach (var item in alternatives)
            {
                if (item != null)
                {
                    lObjects.Add(item);
                }
            }

            return FirstNotNullOrDefault<TIn, TOut>(lObjects);
        }

        public static TOut FirstNotNullOrDefault<TIn, TOut>([NotNull] this IEnumerable<TIn?> alternatives)
        {
            foreach (var item in alternatives.Where(w => w != null))
            {
                switch (item)
                {
                    case TOut t:
                        return t;

                    case string str:
                        if (str.TryCastUsingOperator<string, TOut>(out var rstr))
                        {
                            return rstr;
                        }
                        else
                        {
                            var jObj = str.JsonObject<TOut>();

                            if (jObj != null)
                            {
                                return jObj;
                            }
                        }
                        continue;

                    case long long1:
                        if (!long1.TryCastUsingOperator<long, TOut>(out var rlong))
                        {
                            var json = long1.JsonString();

                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                var jObj = json.JsonObject<TOut>();

                                if (jObj != null)
                                {
                                    return jObj;
                                }
                            }
                        }
                        else
                        {
                            return rlong;
                        }
                        continue;
                    case int int1:
                        if (!int1.TryCastUsingOperator<int, TOut>(out var rint))
                        {
                            var json = int1.JsonString();

                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                var jObj = json.JsonObject<TOut>();

                                if (jObj != null)
                                {
                                    return jObj;
                                }
                            }
                        }
                        else
                        {
                            return rint;
                        }
                        continue;
                    case short short1:
                        if (!short1.TryCastUsingOperator<short, TOut>(out var rshort))
                        {
                            var json = short1.JsonString();

                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                var jObj = json.JsonObject<TOut>();

                                if (jObj != null)
                                {
                                    return jObj;
                                }
                            }
                        }
                        else
                        {
                            return rshort;
                        }
                        continue;
                    case decimal dec1:
                        if (!dec1.TryCastUsingOperator<decimal, TOut>(out var rdec))
                        {
                            var json = dec1.JsonString();

                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                var jObj = json.JsonObject<TOut>();

                                if (jObj != null)
                                {
                                    return jObj;
                                }
                            }
                        }
                        else
                        {
                            return rdec;
                        }
                        continue;

                    default:
                        return (TOut)(object)item;
                }
            }

            return default;
        }

        public static TOut FirstNotNullOrDefault<TIn, TOut>([NotNull] params TIn?[] alternatives) => FirstNotNullOrDefault<TIn, TOut>((IEnumerable<TIn?>)alternatives);
    }
}
