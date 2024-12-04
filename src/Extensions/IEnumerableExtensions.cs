using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    /// <summary>
    /// This static class contains extension methods for the <see cref="System.Collections.Generic.IEnumerable{T}"/> interface.
    /// </summary>
    /// <remarks>
    /// Extension methods allow you to add new functionality to existing types without modifying their source code.
    /// </remarks>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Converts an IEnumerable<T> to an IAsyncEnumerable<T>.
        /// This allows the collection to be processed asynchronously, enabling the use of 
        /// asynchronous iteration over the elements of the source collection.
        /// </summary>
        /// <param name="source">
        /// The IEnumerable<T> source collection that will be converted to IAsyncEnumerable<T>.
        /// This collection must not be null.
        /// </param>
        /// <returns>
        /// An IAsyncEnumerable<T> that represents the asynchronous iteration over the 
        /// elements in the source collection. The elements will be yielded one by one 
        /// asynchronously.
        /// </returns>
        [return: NotNull]
        public static async IAsyncEnumerable<T> ToIAsyncEnumerable<T>([NotNull] this IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            await Task.Yield();
        }

        /// <summary>
        /// Converts elements of an asynchronous enumerable sequence from one type to another 
        /// using a specified conversion expression.
        /// </summary>
        /// <typeparam name="TBase">The type of the elements in the source asynchronous enumerable.</typeparam>
        /// <typeparam name="TConvert">The type of the elements in the resulting asynchronous enumerable.</typeparam>
        /// <param name="source">The asynchronous enumerable sequence of type <typeparamref name="TBase"/> to convert.</param>
        /// <param name="expression">An expression that defines the conversion from <typeparamref name="TBase"/> to <typeparamref name="TConvert"/>.</param>
        /// <returns>
        /// An asynchronous enumerable sequence of converted elements of type <typeparamref name="TConvert"/>.
        /// </returns>
        [return: NotNull]
        public static async IAsyncEnumerable<TConvert> Convert<TBase, TConvert>([NotNull] this IAsyncEnumerable<TBase> source, [NotNull] Expression<Func<TBase, TConvert>> expression)
        {
            var func = expression.Compile();

            await foreach (var item in source)
            {
                yield return func(item);
            }
        }


        /// <summary>
        /// Asynchronously filters an <see cref="IAsyncEnumerable{T}"/> by removing items that match a specified condition.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements in the source <see cref="IAsyncEnumerable{T}"/>.
        /// </typeparam>
        /// <param name="source">
        /// The source <see cref="IAsyncEnumerable{T}"/> from which elements will be removed.
        /// </param>
        /// <param name="remove">
        /// An expression that defines the condition for removing elements. Only elements that do not match this condition will be yielded.
        /// </param>
        /// <returns>
        /// An asynchronous enumerable that yields items from the source that do not match the specified condition.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="remove"/> is <c>null</c>.
        /// </exception>
        [return: NotNull]
        public static async IAsyncEnumerable<T> RemoveAllAsync<T>([NotNull] this IAsyncEnumerable<T> source, [NotNull] Expression<Func<T, bool>> remove)
        {
            var compiled = remove.Compile();

            await foreach (var item in source)
            {
                if (!compiled(item))
                {
                    yield return item;
                }
            }
        }


        /// <summary>
        /// Asynchronously removes all elements from the given IAsyncEnumerable<T> source that meet the criteria defined by the given predicate function.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source.</typeparam>
        /// <param name="source">The IAsyncEnumerable<T> source from which to remove elements.</param>
        /// <param name="remove">A function that defines the criteria for removing elements.</param>
        /// <returns>A task that represents the asynchronous operation, yielding the elements that do not match the remove criteria.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source or remove function is null.</exception>
        [return: NotNull]
        public static IEnumerable<T> RemoveAll<T>([NotNull] this IEnumerable<T> source, [NotNull] Expression<Func<T, bool>> remove)
        {
            var compiled = remove.Compile();
            foreach (var item in source)
            {
                if (compiled(item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Asynchronously converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source <see cref="IAsyncEnumerable{T}"/>.</typeparam>
        /// <param name="source">The asynchronous enumerable sequence to convert.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IEnumerable{T}"/> representing the converted sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter is null.</exception>
        /// <remarks>
        /// This method is an extension method for <see cref="IAsyncEnumerable{T}"/> and provides a way to convert 
        /// the asynchronous sequence to a standard enumerable list by awaiting the completion of the conversion.
        /// </remarks>
        [return: NotNull]
        public static async Task<IEnumerable<T>> ToIEnumerableAsync<T>([NotNull] this IAsyncEnumerable<T> source) => await ToListAsync(source);

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IEnumerable{T}"/>.
        /// This method allows for the consumption of asynchronous enumerable collections in a synchronous manner.
        /// </summary>
        /// <typeparam name="T">The type of elements in the asynchronous enumerable.</typeparam>
        /// <param name="source">The asynchronous enumerable to convert.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> that contains the elements from the <paramref name="source"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
        [return: NotNull]
        public static IEnumerable<T> ToIEnumerable<T>([NotNull] this IAsyncEnumerable<T> source)
        {
            var enumerator = source.GetAsyncEnumerator();

            var next = enumerator.MoveNextAsync();

            var task = next.AsTask();

            task.Wait();

            while (next.Result)
            {
                yield return enumerator.Current;

                next = enumerator.MoveNextAsync();
            }
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to a <see cref="List{T}"/> by awaiting the asynchronous enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source collection.</typeparam>
        /// <param name="source">An asynchronous sequence of elements to convert to a list. This cannot be null.</param>
        /// <returns>
        /// A <see cref="List{T}"/> containing the elements from the asynchronous sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
        [return: NotNull]
        public static List<T> ToList<T>([NotNull] this IAsyncEnumerable<T> source)
        {
            var task = ToListAsync(source);

            task.Wait();

            return task.Result;
        }

        /// <summary>
        /// Converts an asynchronous enumerable sequence to a list asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the asynchronous enumerable.</typeparam>
        /// <param name="source">The source asynchronous enumerable to be converted to a list.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a list containing the elements of the source.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        /// <remarks>
        /// This method collects all elements from the provided asynchronous enumerable into a list.
        /// It ensures that the operation is performed asynchronously and allows for other tasks to run 
        /// concurrently by yielding back to the scheduler after processing the source items.
        /// </remarks>
        [return: NotNull]
        public static async Task<List<T>> ToListAsync<T>([NotNull] this IAsyncEnumerable<T> source)
        {
            var result = new List<T>();

            await foreach (var item in source)
            {
                result.Add(item);
            }

            await Task.Yield();

            return result;
        }

        /// <summary>
        /// Asynchronously converts a collection of objects of type T to a flat dictionary 
        /// representation where each dictionary contains properties of the objects as key-value pairs.
        /// </summary>
        /// <typeparam name="T">The type of the objects in the input collection.</typeparam>
        /// <param name="source">An enumerable collection of objects of type T to be converted.</param>
        /// <returns>An asynchronous stream of dictionaries where each dictionary represents 
        /// the properties of a corresponding object in the source collection.</returns>
        /// <remarks>
        /// The method utilizes parallel processing to enqueue the flattened dictionaries 
        /// efficiently while allowing concurrent access. The method is designed to work 
        /// seamlessly with asynchronous programming patterns in C#.
        /// </remarks>
        [return: NotNull]
        public static async IAsyncEnumerable<Dictionary<string, object>> ToFlatDictionaryAsync<T>(
            [NotNull] this IEnumerable<T> source)
        {
            var propertyDescriptorCollection = TypeDescriptor.GetProperties(typeof(T));

            var concurrentQueue = new ConcurrentQueue<Dictionary<string, object>>();
            var finished = false;
            var autoResetEvent = new AutoResetEvent(false);
            var task = Task.Factory.StartNew(() =>
            {
                Parallel.ForEach(source,
                    (item, state, pos) => concurrentQueue.Enqueue(ToFlat(item, propertyDescriptorCollection)));

                finished = true;
                autoResetEvent.Set();
            });

            while (!finished)
            {
                autoResetEvent.WaitOne();
                while (!concurrentQueue.IsEmpty)
                {
                    if (concurrentQueue.TryDequeue(out var res))
                    {
                        yield return res;
                    }
                }
            }

            task.Wait();
            task.Dispose();

            await Task.Yield();
        }

        /// <summary>
        /// Transforms a collection of objects into an enumerable collection of flat dictionaries.
        /// Each object in the source collection is represented as a dictionary where the keys
        /// are the property names and the values are the corresponding property values.
        /// </summary>
        /// <typeparam name="T">The type of the objects in the source collection.</typeparam>
        /// <param name="source">The source collection of objects to be transformed.</param>
        /// <returns>
        /// An enumerable collection of dictionaries, where each dictionary represents a flattened 
        /// view of an individual object with property names as keys and property values as values.
        /// </returns>
        [return: NotNull]
        public static IEnumerable<Dictionary<string, object>> ToFlatDictionary<T>([NotNull] this IEnumerable<T> source)
        {
            var propertyDescriptorCollection = TypeDescriptor.GetProperties(typeof(T));

            foreach (var item in source)
            {
                yield return ToFlat(item, propertyDescriptorCollection);
            }
        }

        /// <summary>
        /// Asynchronously converts an <see cref="IEnumerable{T}"/> to an <see cref="IAsyncEnumerable{ExpandoObject}"/> by flattening 
        /// the source elements into a dictionary format.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="source">The source <see cref="IEnumerable{T}"/> to be converted.</param>
        /// <returns>
        /// An asynchronous enumerable sequence of elements that are represented as <see cref="ExpandoObject"/> 
        /// created from the flattened dictionaries.
        /// </returns>
        [return: NotNull]
        public static async IAsyncEnumerable<dynamic> ToDynamicAsync<T>([NotNull] this IEnumerable<T> source)
        {
            await foreach (var item in ToFlatDictionaryAsync(source))
            {
                yield return ((IDictionary<string, object>)item) as ExpandoObject;
            }
        }

        /// <summary>
        /// Converts an enumerable collection of type T into an enumerable collection of dynamic objects.
        /// Each item is transformed into an ExpandoObject representation of its dictionary form.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source enumerable.</typeparam>
        /// <param name="source">An enumerable collection of type T to be converted.</param>
        /// <returns>
        /// An enumerable collection of dynamic objects (ExpandoObject) that represent the 
        /// flat dictionary of each element in the source collection.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Thrown when the item cannot be cast to an IDictionary<string, object>.
        /// </exception>
        [return: NotNull]
        public static IEnumerable<dynamic> ToDynamic<T>(this IEnumerable<T> source)
        {
            foreach (var item in ToFlatDictionary(source))
            {
                yield return ((IDictionary<string, object>)item) as ExpandoObject;
            }
        }

        /// <summary>
        /// Converts an object of type T into a flat dictionary representation.
        /// The keys of the dictionary represent the property names of the object,
        /// with support for nested objects and collections.
        /// </summary>
        /// <typeparam name="T">The type of the object to flatten.</typeparam>
        /// <param name="obj">The object to be flattened into a dictionary.</param>
        /// <param name="propertyDescriptorCollection">A collection of properties that describe the properties of the object.</param>
        /// <param name="before">The prefix to prepend to the keys in the dictionary (used for nested properties).</param>
        /// <param name="expando">An optional dictionary to store the flattened properties. If not provided, a new dictionary is created.</param>
        /// <returns>
        /// A dictionary where the keys are dot-separated property names and the values are the corresponding property values,
        /// or null if the property value was null. 
        /// The dictionary structure supports nested objects and collections being represented in a flat format.
        /// </returns>
        [return: NotNull]
        private static Dictionary<string, object> ToFlat<T>(T obj,
            PropertyDescriptorCollection propertyDescriptorCollection, string before = "",
            Dictionary<string, object> expando = default)
        {
            if (expando == default)
            {
                expando = new Dictionary<string, object>();
            }

            //foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(obj.GetType()))
            foreach (PropertyDescriptor property in propertyDescriptorCollection)
            {
                var propertyValue = property.GetValue(obj);
                var nextBefore = $"{(before != default ? $"{before}.{property.Name}" : property.Name)}";

                if (propertyValue == null)
                {
                    //NULL
                    expando.Add(nextBefore, null);
                }
                else
                {
                    if (property.PropertyType.IsPrimitive ||
                        property.PropertyType == typeof(decimal) ||
                        property.PropertyType == typeof(String) ||
                        property.PropertyType == typeof(DateTime) ||
                        property.PropertyType == typeof(DateTimeOffset))
                    {
                        //PRIMITIVE
                        expando.Add(nextBefore, propertyValue);
                    }
                    else
                    {
                        if (property.PropertyType.BaseType == typeof(ValueType) &&
                            property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                            property.PropertyType.GenericTypeArguments.Length > 0)
                        {
                            //NULLABLE
                            if ((bool)property.PropertyType.GetProperty("HasValue").GetValue(propertyValue))
                            {
                                // VALUE
                                expando.Add(nextBefore,
                                    property.PropertyType.GetProperty("Value").GetValue(propertyValue));
                            }
                            else
                            {
                                // NULL
                                expando.Add(nextBefore, null);
                            }
                        }
                        else
                        {
                            if ((property.PropertyType.IsArray &&
                                property.PropertyType.IsAssignableFrom(property.PropertyType.GetElementType())) ||
                                property.PropertyType.GetInterfaces().Any(a => new[]
                                {
                                    typeof(IList<>),
                                    typeof(ICollection<>),
                                    typeof(IEnumerable<>),
                                    typeof(IEnumerator<>),
                                    typeof(IEnumerable),
                                    typeof(IEnumerator),
                                    typeof(IList),
                                    typeof(IReadOnlyList<>),
                                    typeof(IReadOnlyCollection<>)
                                }.Contains(a)))
                            {
                                //ARRAY
                                var pos = 0;
                                var arrs = propertyValue is IEnumerator
                                    ? (IEnumerator)propertyValue
                                    : ((System.Collections.IEnumerable)propertyValue).GetEnumerator();
                                while (arrs.MoveNext())
                                {
                                    var arr = arrs.Current;

                                    _ = ToFlat(arr, propertyDescriptorCollection, $"{nextBefore}[{pos++}]", expando);
                                }
                            }
                            else
                            {
                                //OBJECT
                                _ = ToFlat(propertyValue, propertyDescriptorCollection, nextBefore, expando);
                            }
                        }
                    }
                }
            }

            return expando;
        }


    }
}
