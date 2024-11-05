using System;

namespace UCode.Mongo
{
    /// <summary>
    /// Provides extension methods for full-text search functionality.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Determines whether the specified type is a structure (value type).
        /// </summary>
        /// <param name="objectId">The object identifier instance to extend.</param>
        /// <param name="localType">The type to check for structure status.</param>
        /// <returns>
        /// True if the <paramref name="localType"/> is a structure; otherwise, false.
        /// </returns>
        public static bool IsStructure(this IObjectId objectId, Type localType) => IsStructur(localType);

        /// <summary>
        /// Extension method that checks if the provided local type is a structure.
        /// </summary>
        /// <typeparam name="TObjectId">The type of the object identifier, which must implement 
        /// <see cref="IComparable{T}"/> and <see cref="IEquatable{T}"/>.</typeparam>
        /// <param name="objectId">The instance of the object identifier implementing 
        /// <see cref="IObjectId{T}"/>.</param>
        /// <param name="localType">The type to check if it is a structure.</param>
        /// <returns>
        /// Returns true if the local type is a structure; otherwise, false.
        /// </returns>
        public static bool IsStructure<TObjectId>(this IObjectId<TObjectId> objectId, Type localType)
                    where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId> => IsStructur(localType);

        /// <summary>
        /// Determines if a type is a structure.
        /// </summary>
        /// <param name="localType">The type to check.</param>
        /// <returns>True if the type is a structure, false otherwise.</returns>
        public static bool IsStructure<TObjectId, TUser>(this IObjectId<TObjectId, TUser> objectId, Type localType)
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
    }
}
