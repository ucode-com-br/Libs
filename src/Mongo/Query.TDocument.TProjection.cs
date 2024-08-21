using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using UCode.Mongo.Options;

namespace UCode.Mongo
{
    public record Query<TDocument, TProjection> : QueryBase<TDocument>
    {
        #region Constructors
        internal Query([NotNull] string str) : base(str)
        {
        }

        internal Query([NotNull] Expression<Func<TDocument, bool>> expressionQuery) : base(expressionQuery)
        {

        }

        internal Query([NotNull] Expression<Func<TDocument, TDocument, bool>> expressionQuery) : base(expressionQuery)
        {

        }

        internal Query([NotNull] FilterDefinition<TDocument> filterDefinition) : base(filterDefinition)
        {

        }

        internal Query(string text, FullTextSearchOptions<TDocument> fullTextSearchOptions) : base(text, fullTextSearchOptions)
        {
        }
        #endregion Constructors

        public Query<TDocument, TProjection> CompleteExpression(TDocument constrainValue)
        {
            return new Query<TDocument, TProjection>(base.CompleteExpression(constrainValue));
         }


        #region Static Methods
        public static Query<TDocument, TProjection> FromQuery([NotNull] string query, Update<TDocument> update = default) => new(query)
        {
            Update = update
        };

        public static Query<TDocument, TProjection> FromExpression([NotNull] Expression<Func<TDocument, bool>> func, Update<TDocument> update = default) => new(func)
        {
            Update = update
        };

        public static Query<TDocument, TProjection> FromExpression([NotNull] Expression<Func<TDocument, TDocument, bool>> func, Update<TDocument> update = default) => new(func)
        {
            Update = update
        };

        public static Query<TDocument, TProjection> FromText([NotNull] string text, [NotNull] Options.FullTextSearchOptions<TDocument> fulltextSearchOptions = default) => new(Builders<TDocument>.Filter.Text(text, fulltextSearchOptions));
        #endregion Static Methods

        #region Operator & | !

        /// <summary>
        /// Implements the operator &.
        /// </summary>
        /// <param name="lhs">left query</param>
        /// <param name="rhs">right query</param>
        /// <returns>The result of the operator.</returns>
        public static Query<TDocument, TProjection> operator +(Query<TDocument, TProjection> lhs, Query<TDocument, TProjection> rhs) => (FilterDefinition<TDocument>)lhs & (FilterDefinition<TDocument>)lhs;

        /// <summary>
        /// Implements the operator &.
        /// </summary>
        /// <param name="lhs">left query</param>
        /// <param name="rhs">right query</param>
        /// <returns>The result of the operator.</returns>
        public static Query<TDocument, TProjection> operator &(Query<TDocument, TProjection> lhs, Query<TDocument, TProjection> rhs) => (FilterDefinition<TDocument>)lhs & (FilterDefinition<TDocument>)lhs;

        /// <summary>
        /// Implements the operator |.
        /// </summary>
        /// <param name="lhs">left query</param>
        /// <param name="rhs">right query</param>
        /// <returns>The result of the operator.</returns>
        public static Query<TDocument, TProjection> operator |(Query<TDocument, TProjection> lhs, Query<TDocument, TProjection> rhs) => (FilterDefinition<TDocument>)lhs | (FilterDefinition<TDocument>)lhs;

        /// <summary>
        /// Implements the operator !.
        /// </summary>
        /// <param name="op">Query will be denied</param>
        /// <returns>The result of the operator.</returns>
        public static Query<TDocument, TProjection> operator !(Query<TDocument, TProjection> op) => !(FilterDefinition<TDocument>)op;
        #endregion Operator & | !

        #region Operator & | !

        /// <summary>
        /// Implements the operator &.
        /// </summary>
        /// <param name="lhs">left query</param>
        /// <param name="rhs">right query</param>
        /// <returns>The result of the operator.</returns>
        public static Query<TDocument, TProjection> operator &(Query<TDocument, TProjection> lhs, Query<TDocument> rhs) => (FilterDefinition<TDocument>)lhs & (FilterDefinition<TDocument>)lhs;

        /// <summary>
        /// Implements the operator &.
        /// </summary>
        /// <param name="lhs">left query</param>
        /// <param name="rhs">right query</param>
        /// <returns>The result of the operator.</returns>
        public static Query<TDocument, TProjection> operator &(Query<TDocument> lhs, Query<TDocument, TProjection> rhs) => (FilterDefinition<TDocument>)lhs & (FilterDefinition<TDocument>)lhs;

        /// <summary>
        /// Implements the operator |.
        /// </summary>
        /// <param name="lhs">left query</param>
        /// <param name="rhs">right query</param>
        /// <returns>The result of the operator.</returns>
        public static Query<TDocument, TProjection> operator |(Query<TDocument, TProjection> lhs, Query<TDocument> rhs) => (FilterDefinition<TDocument>)lhs | (FilterDefinition<TDocument>)lhs;

        /// <summary>
        /// Implements the operator |.
        /// </summary>
        /// <param name="lhs">left query</param>
        /// <param name="rhs">right query</param>
        /// <returns>The result of the operator.</returns>
        public static Query<TDocument, TProjection> operator |(Query<TDocument> lhs, Query<TDocument, TProjection> rhs) => (FilterDefinition<TDocument>)lhs | (FilterDefinition<TDocument>)lhs;

        #endregion Operator & | !

        /// <inheritdoc/>
        public override string ToString() => base.ToString();



        [return: NotNull]
        public static implicit operator Query<TDocument, TProjection>([NotNull] Query<TDocument> query)
        {
            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                return new(query.JsonQuery)
                {
                    Update = query.Update
                };
            }

            if (query.ExpressionQuery != null)
            {
                return new(query.ExpressionQuery)
                {
                    Update = query.Update
                };
            }

            if (query.IncompletedExpressionQuery != null)
            {
                return new(query.IncompletedExpressionQuery)
                {
                    Update = query.Update
                };
            }

            if (query.FilterDefinition != null)
            {
                return new(query.FilterDefinition)
                {
                    Update = query.Update
                };
            }

            if (query.FullTextSearchOptions != null)
            {
                return new(query.FullTextSearchOptions!.Value.Item1, query.FullTextSearchOptions!.Value.Item2)
                {
                    Update = query.Update
                };
            }

            throw new InvalidOperationException("Fail to convert object.");
        }

        [return: NotNull]
        public static implicit operator Query<TDocument>([NotNull] Query<TDocument, TProjection> query)
        {
            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                return new(query.JsonQuery)
                {
                    Update = query.Update
                };
            }

            if (query.ExpressionQuery != null)
            {
                return new(query.ExpressionQuery)
                {
                    Update = query.Update
                };
            }

            if (query.IncompletedExpressionQuery != null)
            {
                return new(query.IncompletedExpressionQuery)
                {
                    Update = query.Update
                };
            }

            if (query.FilterDefinition != null)
            {
                return new(query.FilterDefinition)
                {
                    Update = query.Update
                };
            }

            if (query.FullTextSearchOptions != null)
            {
                return new(query.FullTextSearchOptions!.Value.Item1, query.FullTextSearchOptions!.Value.Item2)
                {
                    Update = query.Update
                };
            }

            throw new InvalidOperationException("Fail to convert object.");
        }



        /// <summary>
        /// Empression to "Query<TDocument, TProjection>"
        /// </summary>
        /// <param name="expression"></param>
        [return: NotNull]
        public static implicit operator Query<TDocument, TProjection>([NotNull] Func<TDocument, bool> expression) => new(expression);

        [return: NotNull]
        public static implicit operator Query<TDocument, TProjection>([NotNull] Func<TDocument, TDocument, bool> expression) => new(expression);

        [return: NotNull]
        public static implicit operator FilterDefinition<TDocument>([NotNull] Query<TDocument, TProjection> query)
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
        public static implicit operator UpdateDefinition<TDocument>([NotNull] Query<TDocument, TProjection> query)
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
        public static implicit operator ProjectionDefinition<TDocument, TProjection>([NotNull] Query<TDocument, TProjection> query)
        {
            if (query == default)
            {
                return default;
            }

            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                return query.JsonQuery;
            }

            return query.ExpressionQuery.ToBsonDocument();
        }

        [return: NotNull]
        public static implicit operator SortDefinition<TDocument>([NotNull] Query<TDocument, TProjection> query)
        {
            if (query == default)
            {
                return default;
            }

            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                return query.JsonQuery;
            }

            return query.ExpressionQuery.ToBsonDocument();
        }

        [return: NotNull]
        public static implicit operator BsonDocument([NotNull] Query<TDocument, TProjection> query)
        {
            if (query == default)
            {
                return default;
            }

            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                return BsonSerializer.Deserialize<BsonDocument>(query.JsonQuery);
            }

            return query.ExpressionQuery.ToBsonDocument();
        }



        #region implicity to constructors
        public static implicit operator Query<TDocument, TProjection>(string query)
        {
            if (query == default)
            {
                return default;
            }

            return new(query);
        }

        public static implicit operator Query<TDocument, TProjection>([NotNull] Expression<Func<TDocument, bool>> query)
        {
            if (query == default)
            {
                return default;
            }

            return new(query);
        }

        public static implicit operator Query<TDocument, TProjection>([NotNull] Expression<Func<TDocument, TDocument, bool>> query)
        {
            if (query == default)
            {
                return default;
            }

            return new(query);
        }

        public static implicit operator Query<TDocument, TProjection>([NotNull] FilterDefinition<TDocument> source) => new(source);


        #endregion implicity to constructors




    }
}
