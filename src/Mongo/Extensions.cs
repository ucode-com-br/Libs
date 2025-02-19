using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UCode.Mongo.Attributes;
using UCode.Mongo.Models;
using UCode.Extensions;
using System.Collections.Generic;
using UCode.Extensions.CodeGenerator;
using System.Text.Json;
using UCode.Mongo.Serializers;

namespace UCode.Mongo
{
    /// <summary>
    /// Provides extension methods for full-text search functionality.
    /// </summary>
    public static class Extensions
    {
        public static void AddIgnorableDataConverter(this JsonSerializerOptions options) => options.Converters.Add(new IgnorableDataJsonConverterFactory());


        /// <summary>
        /// Determines whether the specified type is a structure (value type).
        /// </summary>
        /// <param name="objectId">The object identifier instance to extend.</param>
        /// <param name="localType">The type to check for structure status.</param>
        /// <returns>
        /// True if the <paramref name="localType"/> is a structure; otherwise, false.
        /// </returns>
        public static bool IsStructure(this IObjectBase objectId, Type localType) => IsStructur(localType);

        /// <summary>
        /// Extension method that checks if the provided local type is a structure.
        /// </summary>
        /// <typeparam name="TObjectId">The type of the object identifier, which must implement 
        /// <see cref="IComparable{T}"/> and <see cref="IEquatable{T}"/>.</typeparam>
        /// <param name="objectId">The instance of the object identifier implementing 
        /// <see cref="IObjectBase{T}"/>.</param>
        /// <param name="localType">The type to check if it is a structure.</param>
        /// <returns>
        /// Returns true if the local type is a structure; otherwise, false.
        /// </returns>
        public static bool IsStructure<TObjectId>(this IObjectBase<TObjectId> objectId, Type localType)
                    where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId> => IsStructur(localType);

        /// <summary>
        /// Determines if a type is a structure.
        /// </summary>
        /// <param name="localType">The type to check.</param>
        /// <returns>True if the type is a structure, false otherwise.</returns>
        public static bool IsStructure<TObjectId, TUser>(this IObjectBase<TObjectId, TUser> objectId, Type localType)
            where TUser : notnull
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId> => IsStructur(localType);

        /// <summary>
        /// Determines whether the specified type is a structure (a value type that is not a primitive type).
        /// </summary>
        /// <param name="localType">The type to be evaluated for structure characteristics.</param>
        /// <returns>
        /// Returns <c>true</c> if the specified type is a value type and not a primitive type, <c>false</c> otherwise.
        /// </returns>
        private static bool IsStructur(Type localType)
        {
            var result = false;

            if (localType.IsValueType)
            {
                // Is a value type
                if (!localType.IsPrimitive)
                {
                    /* Is not primitive. Remember that primitives are:
                    Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32,
                    Int64, UInt64, IntPtr, UIntPtr, Char, Double, Single.
                    This way, could still be Decimal, Date or Enum. */
                    if (localType != typeof(decimal))
                    {
                        //Is not Decimal
                        if (localType != typeof(DateTime))
                        {
                            //Is not Date
                            if (!localType.IsEnum)
                            {
                                //Is not Enum. Consequently it is a structure.
                                result = true;
                            }
                        }
                    }
                }
            }

            return result;
        }

        //        /// <summary>
        //        /// Performs a full-text search on the specified query.
        //        /// </summary>
        //        /// <typeparam name="T">The type of the projection.</typeparam>
        //        /// <param name="query">The query to match documents against.</param>
        //        /// <param name="search">The text to search for.</param>
        //        /// <returns>An asynchronous enumerable of documents matching the specified query.</returns>
        //        public static IMongoQueryable<T> FullTextSearch<T>(this IMongoQueryable<T> query, string search)
        //        {
        //            var filter = Builders<T>.Filter.Text(search);
        //            return query.Where(_ => filter.Inject());
        //        }

        //        /// <summary>
        //        /// Performs a full-text search on the specified query with the specified language.
        //        /// </summary>
        //        /// <typeparam name="T">The type of the projection.</typeparam>
        //        /// <param name="query">The query to match documents against.</param>
        //        /// <param name="search">The text to search for.</param>
        //        /// <param name="language">The language of the text to search for.</param>
        //        /// <returns>An asynchronous enumerable of documents matching the specified query.</returns>
        //        public static IMongoQueryable<T> FullTextSearch<T>(this IMongoQueryable<T> query, string search, string language)
        //        {
        //            var filter = Builders<T>.Filter.Text(search, language);
        //            return query.Where(_ => filter.Inject());
        //        }

        //        /// <summary>
        //        /// Performs a full-text search on the specified query with the specified options.
        //        /// </summary>
        //        /// <typeparam name="T">The type of the projection.</typeparam>
        //        /// <param name="query">The query to match documents against.</param>
        //        /// <param name="search">The text to search for.</param>
        //        /// <param name="textSearchOptions">Options for the full-text search.</param>
        //        /// <returns>An asynchronous enumerable of documents matching the specified query.</returns>
        //        public static IMongoQueryable<T> FullTextSearch<T>(this IMongoQueryable<T> query, string search, TextSearchOptions textSearchOptions)
        //        {
        //            var filter = Builders<T>.Filter.Text(search, textSearchOptions);
        //            return query.Where(_ => filter.Inject());
        //        }

        //        /// <summary>
        //        /// Performs a full-text search on the specified query.
        //        /// </summary>
        //        /// <typeparam name="T">The type of the projection.</typeparam>
        //        /// <param name="query">The query to match documents against.</param>
        //        /// <param name="search">The text to search for.</param>
        //        /// <returns>An enumerable of documents matching the specified query.</returns>
        //        /// <exception cref="NotImplementedException">Thrown if the query is not of type IMongoQueryable.</exception>
        //        public static IQueryable<T> FullTextSearch<T>(this IQueryable<T> query, string search)
        //        {
        //            // If the query is of type IMongoQueryable, perform a full-text search on the specified query.
        //            if (query is IMongoQueryable<T>)
        //            {
        //                var filter = Builders<T>.Filter.Text(search);
        //                return query.Where(_ => filter.Inject());
        //            }

        //            // Otherwise, throw a NotImplementedException.
        //            else
        //            {
        //                throw new NotImplementedException();
        //            }
        //        }

        //        /// <summary>
        //        /// Performs a full-text search on the specified query using the specified text search options.
        //        /// </summary>
        //        /// <typeparam name="T">The type of the projection.</typeparam>
        //        /// <param name="query">The query to match documents against.</param>
        //        /// <param name="search">The text to search for.</param>
        //        /// <param name="textSearchOptions">The options for the full-text search.</param>
        //        /// <returns>An enumerable of documents matching the specified query.</returns>
        //        /// <exception cref="NotImplementedException">Thrown if the query is not of type IMongoQueryable.</exception>
        //        public static IQueryable<T> FullTextSearch<T>(this IQueryable<T> query, string search, Options.FullTextSearchOptions<T> textSearchOptions)
        //        {
        //            // Check if the query is of type IMongoQueryable
        //            if (query is IMongoQueryable<T>)
        //            {
        //                // Create a filter for the full-text search
        //                var filter = Builders<T>.Filter.Text(search, textSearchOptions);

        //                // Apply the filter to the query and return the result
        //                return query.Where(_ => filter.Inject());
        //            }
        //            else
        //            {
        //                // Throw an exception if the query is not of type IMongoQueryable
        //                throw new NotImplementedException();
        //            }
        //        }


        internal static object? ProcessIgnorableData(this object? obj, bool isRoot = true) => ProcessIgnorableData<object>(obj, isRoot);


        /// <summary>
        /// Processa as propriedades e campos de um objeto em busca do atributo <see cref="IgnorableDataAttribute"/>.
        /// Caso o atributo seja encontrado em um membro (propriedade ou campo) que não esteja na raiz (isRoot = false),
        /// seta o valor default definido no atributo (se houver).
        /// Em seguida, faz a recursão para membros complexos (objetos ou coleções).
        /// </summary>
        /// <param name="obj">Objeto a ser processado.</param>
        /// <param name="isRoot">Se é o objeto raiz na chamada inicial.</param>
        /// <returns>O próprio objeto (apenas para facilitar encadeamentos se desejado).</returns>
        internal static T? ProcessIgnorableData<T>(this T? obj, bool isRoot = true)
        {
            if (obj is null)
            {
                return default;
            }

            var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);


            T clonedObject = CloneRuntimeHelper.DeepCloneRuntime<T>(obj, visited)!;

            var type = typeof(T) ?? obj.GetType();

            // Obtém tanto propriedades quanto campos públicos de instância
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.MemberType is MemberTypes.Field or MemberTypes.Property);

            foreach (var member in members)
            {
                // Recupera o atributo [IgnorableData]
                var ignorableAttr = member.GetCustomAttribute<IgnorableDataAttribute>(inherit: true);

                // Obtém o valor do membro (propriedade ou campo)
                var memberValue = member switch
                {
                    PropertyInfo p when p.CanRead => p.GetValue(clonedObject),
                    FieldInfo f => f.GetValue(clonedObject),
                    _ => null
                };

                // Se o atributo está presente e não é a raiz, aplicar valor default
                if (ignorableAttr is not null && !isRoot)
                {
                    // Tenta usar o DefaultValue definido no atributo
                    var defaultValue = ignorableAttr.DefaultValue;

                    // Se não houver, ainda assim podemos sobrescrever com null
                    member.SetValue(clonedObject, defaultValue);
                }
                else
                {
                    // Se não tem atributo ou é a raiz, verifica se precisa recursão
                    if (memberValue is not null && !IsSimpleType(memberValue.GetType()))
                    {
                        // Se for coleção, processa cada item recursivamente
                        if (memberValue is IEnumerable enumerable)
                        {
                            foreach (var item in enumerable)
                            {
                                // item pode ser null ou um tipo primitivo
                                if (item is not null && !IsSimpleType(item.GetType()))
                                {
                                    item.ProcessIgnorableData(isRoot: false);
                                }
                            }
                        }
                        else
                        {
                            // Objeto complexo (não é coleção e não é tipo simples)
                            memberValue.ProcessIgnorableData(isRoot: false);
                        }
                    }
                }
            }

            return clonedObject;
        }

        internal static bool IsProcessIgnorableData<T>(this T? clonedObject, bool isRoot = true)
        {
            if (clonedObject is null)
            {
                return default;
            }

            var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);


            var type = typeof(T) ?? clonedObject.GetType();

            // Obtém tanto propriedades quanto campos públicos de instância
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.MemberType is MemberTypes.Field or MemberTypes.Property);

            foreach (var member in members)
            {
                // Recupera o atributo [IgnorableData]
                var ignorableAttr = member.GetCustomAttribute<IgnorableDataAttribute>(inherit: true);

                // Obtém o valor do membro (propriedade ou campo)
                var memberValue = member switch
                {
                    PropertyInfo p when p.CanRead => p.GetValue(clonedObject),
                    FieldInfo f => f.GetValue(clonedObject),
                    _ => null
                };

                // Se o atributo está presente
                if (ignorableAttr is not null)
                {
                    return true;
                }
                else
                {
                    // verifica se precisa recursão
                    if (memberValue is not null && !IsSimpleType(memberValue.GetType()))
                    {
                        // Se for coleção, processa cada item recursivamente
                        if (memberValue is IEnumerable enumerable)
                        {
                            foreach (var item in enumerable)
                            {
                                // item pode ser null ou um tipo primitivo
                                if (item is not null && !IsSimpleType(item.GetType()))
                                {
                                    if (item.IsProcessIgnorableData(isRoot: false))
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Objeto complexo (não é coleção e não é tipo simples)
                            return memberValue.IsProcessIgnorableData(isRoot: false);
                        }
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Método auxiliar para setar valor em um FieldInfo ou PropertyInfo.
        /// </summary>
        private static void SetValue(this MemberInfo member, object target, object? value)
        {
            switch (member)
            {
                case PropertyInfo prop when prop.CanWrite:
                    prop.SetValue(target, value);
                    break;
                case FieldInfo field:
                    field.SetValue(target, value);
                    break;
            }
        }

        /// <summary>
        /// Determina se um tipo é simples (primitivo, string, decimal, etc.) e não precisa de processamento recursivo.
        /// </summary>
        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive
                   || type.IsEnum
                   || type == typeof(string)
                   || type == typeof(decimal)
                   || type == typeof(DateTime)
                   || type == typeof(DateTimeOffset)
                   || type == typeof(TimeSpan)
                   || type == typeof(Guid);
        }

    }
}
