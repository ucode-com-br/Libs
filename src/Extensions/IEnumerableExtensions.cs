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
    public static class IEnumerableExtensions
    {
        [return: NotNull]
        public static async IAsyncEnumerable<T> ToIAsyncEnumerable<T>([NotNull] this IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            await Task.Yield();
        }

        [return: NotNull]
        public static async IAsyncEnumerable<TConvert> Convert<TBase, TConvert>([NotNull] this IAsyncEnumerable<TBase> source, [NotNull] Expression<Func<TBase, TConvert>> expression)
        {
            var func = expression.Compile();

            await foreach (var item in source)
            {
                yield return func(item);
            }
        }


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

        //public static async IAsyncEnumerable<T> RemoveAllAsync<T>([NotNull] this IAsyncEnumerable<T> source, [NotNull] Func<T, bool> remove)
        //{
        //    await foreach (var item in source)
        //        if (remove(item))
        //            yield return item;

        //    await Task.Yield();
        //}

        //public static IEnumerable<T> RemoveAll<T>([NotNull] this IEnumerable<T> source, [NotNull] Func<T, bool> remove)
        //{
        //    //foreach (var item in source)
        //    //    if (remove(item))
        //    //        yield return item;

        //    return RemoveAll(source, (Expression<Func<T, bool>>)Expression.Lambda<Func<T, bool>>(Expression.Call(remove.Method)));
        //}

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

        [return: NotNull]
        public static async Task<IEnumerable<T>> ToIEnumerableAsync<T>([NotNull] this IAsyncEnumerable<T> source) => await ToListAsync(source);

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

        [return: NotNull]
        public static List<T> ToList<T>([NotNull] this IAsyncEnumerable<T> source)
        {
            var task = ToListAsync(source);

            task.Wait();

            return task.Result;
        }

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

        [return: NotNull]
        public static IEnumerable<Dictionary<string, object>> ToFlatDictionary<T>([NotNull] this IEnumerable<T> source)
        {
            var propertyDescriptorCollection = TypeDescriptor.GetProperties(typeof(T));

            foreach (var item in source)
            {
                yield return ToFlat(item, propertyDescriptorCollection);
            }
        }

        [return: NotNull]
        public static async IAsyncEnumerable<dynamic> ToDynamicAsync<T>([NotNull] this IEnumerable<T> source)
        {
            await foreach (var item in ToFlatDictionaryAsync(source))
            {
                yield return ((IDictionary<string, object>)item) as ExpandoObject;
            }
        }

        [return: NotNull]
        public static IEnumerable<dynamic> ToDynamic<T>(this IEnumerable<T> source)
        {
            foreach (var item in ToFlatDictionary(source))
            {
                yield return ((IDictionary<string, object>)item) as ExpandoObject;
            }
        }

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

        //private static T FromFlat<T>(Dictionary<string, object> expando, PropertyDescriptorCollection propertyDescriptorCollection, string before = "", T obj = default)
        //{
        //    if (obj == null)
        //        obj = Activator.CreateInstance<T>();

        //    //foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(obj.GetType()))
        //    foreach (PropertyDescriptor property in propertyDescriptorCollection)
        //    {
        //        var nextBefore = $"{((before != default) ? $"{before}.{property.Name}" : property.Name)}";
        //        bool propertyValue = expando.ContainsKey(nextBefore);

        //        if (propertyValue)
        //        {
        //            if (property.PropertyType.IsPrimitive ||
        //                property.PropertyType == typeof(Decimal) ||
        //                property.PropertyType == typeof(String) ||
        //                property.PropertyType == typeof(DateTime) ||
        //                property.PropertyType == typeof(DateTimeOffset))
        //            {
        //                //PRIMITIVE
        //                property.SetValue(obj, expando[nextBefore]);
        //            }
        //            else
        //            {
        //                if (property.PropertyType.BaseType == typeof(System.ValueType) &&
        //                    property.PropertyType.GetGenericTypeDefinition() == typeof(System.Nullable<>) &&
        //                    property.PropertyType.GenericTypeArguments != default &&
        //                    property.PropertyType.GenericTypeArguments.Length > 0)
        //                {
        //                    //NULLABLE
        //                    if (expando[nextBefore] != default)
        //                    {
        //                        // VALUE
        //                        property.SetValue(obj, expando[nextBefore]);
        //                    }
        //                }
        //                else
        //                {
        //                    if ((property.PropertyType.IsArray && property.PropertyType.IsAssignableFrom(property.PropertyType.GetElementType())) ||
        //                        (property.PropertyType.GetInterfaces().Any(a => (new Type[] {
        //                            typeof(IList<>),
        //                            typeof(ICollection<>),
        //                            typeof(IEnumerable<>),
        //                            typeof(IEnumerator<>),
        //                            typeof(IEnumerable),
        //                            typeof(IEnumerator),
        //                            typeof(IList),
        //                            typeof(IReadOnlyList<>),
        //                            typeof(IReadOnlyCollection<>)}).Contains(a))))
        //                    {
        //                        //ARRAY
        //                        int pos = 0;
        //                        var arrs = (propertyValue is IEnumerator ? (IEnumerator)propertyValue : ((System.Collections.IEnumerable)propertyValue).GetEnumerator());
        //                        while (arrs.MoveNext())
        //                        {
        //                            var arr = arrs.Current;

        //                            _ = ToFlat(arr, propertyDescriptorCollection, $"{nextBefore}[{pos++}]", expando);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        //OBJECT
        //                        _ = ToFlat(propertyValue, propertyDescriptorCollection, nextBefore, expando);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return obj;
        //}

    }
}
