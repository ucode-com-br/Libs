using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace UCode.Mongo
{
    public static class Extensions
    {
        public static IMongoQueryable<T> FullTextSearch<T>(this IMongoQueryable<T> query, string search)
        {
            var filter = Builders<T>.Filter.Text(search);
            return query.Where(_ => filter.Inject());
        }

        public static IMongoQueryable<T> FullTextSearch<T>(this IMongoQueryable<T> query, string search, string language)
        {
            var filter = Builders<T>.Filter.Text(search, language);
            return query.Where(_ => filter.Inject());
        }

        public static IMongoQueryable<T> FullTextSearch<T>(this IMongoQueryable<T> query, string search, TextSearchOptions textSearchOptions)
        {
            var filter = Builders<T>.Filter.Text(search, textSearchOptions);
            return query.Where(_ => filter.Inject());
        }


        public static IQueryable<T> FullTextSearch<T>(this IQueryable<T> query, string search)
        {
            if (query is IMongoQueryable<T>)
            {
                var filter = Builders<T>.Filter.Text(search);
                return query.Where(_ => filter.Inject());
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static IQueryable<T> FullTextSearch<T>(this IQueryable<T> query, string search, Options.FullTextSearchOptions<T> textSearchOptions)
        {
            if (query is IMongoQueryable<T>)
            {
                var filter = Builders<T>.Filter.Text(search, textSearchOptions);
                return query.Where(_ => filter.Inject());
            }
            else
            {
                throw new NotImplementedException();
            }
        }



    }
}
