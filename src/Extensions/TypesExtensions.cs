using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
//using Newtonsoft.Json;

namespace UCode.Extensions
{
    /// <summary>
    /// Provides extension methods for various types.
    /// </summary>
    /// <remarks>
    /// This class is static and cannot be instantiated. It contains methods that add additional functionality 
    /// to existing types in a convenient manner. Extension methods allow you to call these methods as if they were 
    /// instance methods on the type being extended, without modifying the original type.
    /// </remarks>
    public static class TypesExtensions
    {
        /// <summary>
        /// Concatenates a base array with a collection of byte arrays into a single array.
        /// </summary>
        /// <param name="base">The base byte array to be concatenated with the other arrays. Cannot be null.</param>
        /// <param name="parameters">An array of byte arrays to be concatenated to the base array. Cannot be null.</param>
        /// <returns>
        /// A new byte array containing all the elements from the base array followed by the elements from the provided byte arrays.
        /// The resulting array is not null.
        /// </returns>
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

        /// <summary>
        /// Converts an object to a specified destination type using JSON serialization and deserialization.
        /// </summary>
        /// <typeparam name="TDestination">The type of the object to which the source object should be converted.</typeparam>
        /// <param name="source">The source object to be converted. This can be null.</param>
        /// <returns>
        /// Returns an object of type TDestination, or null if the source object is null.
        /// </returns>
        public static TDestination? ConvertWithJson<TDestination>([NotNull] this object? source)
        {
            if (source == null)
            {
                return default;
            }

            var json = System.Text.Json.JsonSerializer.Serialize(source);

            return System.Text.Json.JsonSerializer.Deserialize<TDestination>(json);
        }

        /// <summary>
        /// Removes a specified item from a <see cref="ConcurrentBag{T}"/> if it exists.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the bag.</typeparam>
        /// <param name="bag">The <see cref="ConcurrentBag{T}"/> from which the item should be removed.</param>
        /// <param name="item">The item to be removed from the bag.</param>
        /// <remarks>
        /// This method iterates through the elements in the bag, removing the specified item if found.
        /// If the item is found, the removal stops. If the item is not found, the method will continue until the bag is empty.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bag"/> is null.</exception>
        /// <returns>
        /// This method does not return a value. It modifies the <paramref name="bag"/> by removing the specified item.
        /// </returns>
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




        /// <summary>
        /// Returns the first non-null element from the specified source and alternatives.
        /// </summary>
        /// <typeparam name="TIn">The type of the input elements.</typeparam>
        /// <typeparam name="TOut">The type of the output element.</typeparam>
        /// <param name="source">The primary source element from which to check for non-null values.</param>
        /// <param name="alternatives">An array of alternative elements to check for non-null values.</param>
        /// <returns>
        /// The first non-null element from the source or alternatives. If all elements are null, returns default(TOut).
        /// </returns>
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

        /// <summary>
        /// Returns the first non-null value from the provided collection of alternatives, 
        /// cast to the specified output type. If no valid non-null value is found, it returns
        /// the default value of the output type.
        /// </summary>
        /// <typeparam name="TIn">The input type of the alternative values.</typeparam>
        /// <typeparam name="TOut">The output type that the non-null value should be cast to.</typeparam>
        /// <param name="alternatives">An enumerable collection of nullable input values.</param>
        /// <returns>
        /// The first non-null value in the collection, converted to the output type, 
        /// or the default value of the output type if no valid value is found.
        /// </returns>
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

        /// <summary>
        /// Returns the first element in the specified array of alternatives that is not null, or the default value of 
        /// the type <typeparamref name="TOut"/> if all elements are null.
        /// </summary>
        /// <typeparam name="TIn">The type of the elements in the alternatives array.</typeparam>
        /// <typeparam name="TOut">The type of the return value.</typeparam>
        /// <param name="alternatives">An array of alternatives which may contain null elements.</param>
        /// <returns>
        /// The first non-null element from the <paramref name="alternatives"/> array, converted to type <typeparamref name="TOut"/>, 
        /// or the default value of <typeparamref name="TOut"/> if all elements are null.
        /// </returns>
        public static TOut FirstNotNullOrDefault<TIn, TOut>([NotNull] params TIn?[] alternatives) => FirstNotNullOrDefault<TIn, TOut>((IEnumerable<TIn?>)alternatives);
    }
}
