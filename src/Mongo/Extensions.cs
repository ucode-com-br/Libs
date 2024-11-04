//using System;
//using System.Linq;
//using MongoDB.Driver;
//using MongoDB.Driver.Linq;

//namespace UCode.Mongo
//{
//    /// <summary>
//    /// Provides extension methods for full-text search functionality.
//    /// </summary>
//    public static class Extensions
//    {

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
//    }
//}
