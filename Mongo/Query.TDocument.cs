using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using UCode.Extensions;
using UCode.Mongo.Options;

namespace UCode.Mongo
{
    public record Query<TDocument> : QueryBase<TDocument>
    {
        #region Constructors
        internal Query(string str) : base(str)
        {
        }

        internal Query(Expression<Func<TDocument, bool>> expressionQuery) : base(expressionQuery)
        {
        }

        internal Query(Expression<Func<TDocument, TDocument, bool>> expressionQuery) : base(expressionQuery)
        {
        }

        internal Query(FilterDefinition<TDocument> filterDefinition) : base(filterDefinition)
        {
        }

        internal Query(string text, FullTextSearchOptions<TDocument> fullTextSearchOptions) : base(text, fullTextSearchOptions)
        {
        }
        #endregion Constructors

        #region Static Methods
        public static Query<TDocument> FromQuery([NotNull] string query, Update<TDocument> update = default) => new(query)
        {
            Update = update
        };

        public static Query<TDocument> FromExpression([NotNull] Expression<Func<TDocument, bool>> func, Update<TDocument> update = default) => new(func)
        {
            Update = update
        };

        public static Query<TDocument> FromExpression([NotNull] Expression<Func<TDocument, TDocument, bool>> func, Update<TDocument> update = default) => new(func)
        {
            Update = update
        };


        public static Query<TDocument> FromText([NotNull] string text, [NotNull] FullTextSearchOptions<TDocument> fulltextSearchOptions = default) => new(Builders<TDocument>.Filter.Text(text, fulltextSearchOptions));
        #endregion Static Methods

        public Query<TDocument> CompleteExpression(TDocument constrainValue)
        {
            return new Query<TDocument>(base.CompleteExpression(constrainValue));
            //if (base.IncompletedExpressionQuery == null)
            //{
            //    throw new InvalidOperationException("This query does not have incomplete expression.");
            //}

            //return new Query<TDocument>(base.IncompletedExpressionQuery.ReplaceToConstant<Func<TDocument, TDocument, bool>, TDocument, Func<TDocument, bool>>(col => col.Where(p => { if (p.Index == 1) { p.Constant(constrainValue); return true; } return false; })));
        }


        #region Operator & | !


        /// <summary>
        /// Implements the operator &.
        /// </summary>
        /// <param name="lhs">left query</param>
        /// <param name="rhs">right query</param>
        /// <returns>The result of the operator.</returns>
        public static Query<TDocument> operator &(Query<TDocument> lhs, Query<TDocument> rhs) => (FilterDefinition<TDocument>)lhs & (FilterDefinition<TDocument>)lhs;

        /// <summary>
        /// Implements the operator |.
        /// </summary>
        /// <param name="lhs">left query</param>
        /// <param name="rhs">right query</param>
        /// <returns>The result of the operator.</returns>
        public static Query<TDocument> operator |(Query<TDocument> lhs, Query<TDocument> rhs) => (FilterDefinition<TDocument>)lhs | (FilterDefinition<TDocument>)lhs;

        /// <summary>
        /// Implements the operator !.
        /// </summary>
        /// <param name="op">Query will be denied</param>
        /// <returns>The result of the operator.</returns>
        public static Query<TDocument> operator !(Query<TDocument> op) => !(FilterDefinition<TDocument>)op;
        #endregion Operator & | !

        public override string ToString() => base.ToString();


        [return: NotNull]
        public static implicit operator Query<TDocument>([NotNull] Func<TDocument, bool> expression) => new(expression);

        [return: NotNull]
        public static implicit operator FilterDefinition<TDocument>([NotNull] Query<TDocument> query)
        {
            if (query == default)
            {
                return default;
            }

            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                return new JsonFilterDefinition<TDocument>(query.JsonQuery);
            }
            else if (query.ExpressionQuery != null)
            {
                return new ExpressionFilterDefinition<TDocument>(query.ExpressionQuery);
            }
            else if (query.FullTextSearchOptions != default)
            {
                return Builders<TDocument>.Filter.Text(query.FullTextSearchOptions!.Value.Item1, (TextSearchOptions)query.FullTextSearchOptions!.Value.Item2);
            }
            else if (query.IncompletedExpressionQuery != null)
            {
                throw new InvalidOperationException("Fail to convert an expression that requires an constant.");
            }
            else
            {
                return FilterDefinition<TDocument>.Empty;
            }
        }

        [return: NotNull]
        public static implicit operator UpdateDefinition<TDocument>([NotNull] Query<TDocument> query)
        {
            if (query == default)
            {
                return default;
            }

            if (query.Update != null)
            {
                return query.Update;
            }
            else
            {
                return default;
            }
        }

        [return: NotNull]
        public static implicit operator ProjectionDefinition<TDocument>([NotNull] Query<TDocument> query)
        {
            if (query == default)
            {
                return default;
            }

            return ((FilterDefinition<TDocument>)query).ToBsonDocument();
            //if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            //    return query.JsonQuery;

            //return query.ExpressionQuery.ToBsonDocument();
        }

        [return: NotNull]
        public static implicit operator SortDefinition<TDocument>([NotNull] Query<TDocument> query)
        {
            if (query == default)
            {
                return default;
            }

            return ((FilterDefinition<TDocument>)query).ToBsonDocument();
            //if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            //    return query.JsonQuery;

            //return query.ExpressionQuery.ToBsonDocument();
        }

        [return: NotNull]
        public static implicit operator BsonDocument([NotNull] Query<TDocument> query)
        {
            if (query == default)
            {
                return default;
            }

            var f = (FilterDefinition<TDocument>)query;

            return f.ToBsonDocument();

            //return f.Render(new BsonClassMapSerializer<TDocument>(BsonClassMap<TDocument>.LookupClassMap(typeof(TDocument))), BsonSerializer.SerializerRegistry);

            //if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            //    return BsonSerializer.Deserialize<BsonDocument>();

            //return query.ExpressionQuery.ToBsonDocument();
        }



        #region implicity to constructors

        public static implicit operator Query<TDocument>([NotNull] FilterDefinition<TDocument> source) => new(source);

        public static implicit operator Query<TDocument>(string query) => new(query);

        public static implicit operator Query<TDocument>(Expression<Func<TDocument, bool>> expression)
        {
            Query<TDocument> query = new(expression);
            return query;
        }

        public static implicit operator Query<TDocument>(Expression<Func<TDocument, TDocument, bool>> expression)
        {
            Query<TDocument> query = new(expression);
            return query;
        }
        #endregion implicity to constructors
    }
}
