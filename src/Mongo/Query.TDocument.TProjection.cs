using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using UCode.Mongo.Options;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents a query for documents of type TDocument with a projection of type TProjection.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <typeparam name="TProjection">The type of the projection.</typeparam>
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

        /// <summary>
        /// Completes the expression of the query by replacing the incomplete expression with a constant value.
        /// </summary>
        /// <param name="constrainValue">The constant value to replace the incomplete expression with.</param>
        /// <returns>A new Query object with the completed expression.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the query does not have an incomplete expression.</exception>
        public Query<TDocument, TProjection> CompleteExpression(TDocument constrainValue)
        {
            // Call the base class method to complete the expression
            return new Query<TDocument, TProjection>(base.CompleteExpression(constrainValue));
         }


        #region Static Methods
        /// <summary>
        /// Initializes a new instance of the Query class from a string query.
        /// </summary>
        /// <param name="query">The query string.</param>
        /// <param name="update">The update operation. Default is null.</param>
        public static Query<TDocument, TProjection> FromQuery([NotNull] string query, Update<TDocument> update = default) => new(query)
        {
            Update = update
        };

        /// <summary>
        /// Initializes a new instance of the Query class from an expression query.
        /// </summary>
        /// <param name="func">The expression query.</param>
        /// <param name="update">The update operation. Default is null.</param>
        public static Query<TDocument, TProjection> FromExpression([NotNull] Expression<Func<TDocument, bool>> func, Update<TDocument> update = default) => new(func)
        {
            Update = update
        };

        /// <summary>
        /// Initializes a new instance of the Query class from a binary expression query.
        /// </summary>
        /// <param name="func">The binary expression query.</param>
        /// <param name="update">The update operation. Default is null.</param>
        public static Query<TDocument, TProjection> FromExpression([NotNull] Expression<Func<TDocument, TDocument, bool>> func, Update<TDocument> update = default) => new(func)
        {
            Update = update
        };

        /// <summary>
        /// Creates a new instance of the Query class using the provided text and fulltext search options.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="fulltextSearchOptions">The options for the full text search. Defaults to null.</param>
        /// <returns>A new instance of the Query class.</returns>
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

        /// <summary>
        /// Implicitly converts a Query object of type TDocument to a Query object of type TDocument and TProjection.
        /// </summary>
        /// <param name="query">The Query object to convert.</param>
        /// <returns>A new Query object of type TDocument and TProjection.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the conversion fails.</exception>
        [return: NotNull]
        public static implicit operator Query<TDocument, TProjection>([NotNull] Query<TDocument> query)
        {
            // Check if the query has a JSON query
            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                // Create a new Query object with the JSON query and update
                return new(query.JsonQuery)
                {
                    Update = query.Update
                };
            }

            // Check if the query has an expression query
            if (query.ExpressionQuery != null)
            {
                // Create a new Query object with the expression query and update
                return new(query.ExpressionQuery)
                {
                    Update = query.Update
                };
            }

            // Check if the query has an incomplete expression query
            if (query.IncompletedExpressionQuery != null)
            {
                // Create a new Query object with the incomplete expression query and update
                return new(query.IncompletedExpressionQuery)
                {
                    Update = query.Update
                };
            }

            // Check if the query has a filter definition
            if (query.FilterDefinition != null)
            {
                // Create a new Query object with the filter definition and update
                return new(query.FilterDefinition)
                {
                    Update = query.Update
                };
            }

            // Check if the query has full-text search options
            if (query.FullTextSearchOptions != null)
            {
                // Create a new Query object with the full-text search options and update
                return new(query.FullTextSearchOptions!.Value.Item1, query.FullTextSearchOptions!.Value.Item2)
                {
                    Update = query.Update
                };
            }
            // Throw an exception if the conversion fails
            throw new InvalidOperationException("Fail to convert object.");
        }

        /// <summary>
        /// Implicitly converts a Query object of type TDocument and TProjection to a Query object of type TDocument.
        /// </summary>
        /// <param name="query">The Query object to convert.</param>
        /// <returns>A new Query object of type TDocument.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the query cannot be converted.</exception>
        [return: NotNull]
        public static implicit operator Query<TDocument>([NotNull] Query<TDocument, TProjection> query)
        {
            // Check if the query has a JSON query
            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                // Create a new Query object with the JSON query and update
                return new(query.JsonQuery)
                {
                    Update = query.Update
                };
            }

            // Check if the query has an expression query
            if (query.ExpressionQuery != null)
            {
                // Create a new Query object with the expression query and update
                return new(query.ExpressionQuery)
                {
                    Update = query.Update
                };
            }

            // Check if the query has an incomplete expression query
            if (query.IncompletedExpressionQuery != null)
            {
                // Create a new Query object with the incomplete expression query and update
                return new(query.IncompletedExpressionQuery)
                {
                    Update = query.Update
                };
            }

            // Check if the query has a filter definition
            if (query.FilterDefinition != null)
            {
                // Create a new Query object with the filter definition and update
                return new(query.FilterDefinition)
                {
                    Update = query.Update
                };
            }

            // Check if the query has full-text search options
            if (query.FullTextSearchOptions != null)
            {
                // Create a new Query object with the full-text search options and update
                return new(query.FullTextSearchOptions!.Value.Item1, query.FullTextSearchOptions!.Value.Item2)
                {
                    Update = query.Update
                };
            }
            // Throw an exception if the query cannot be converted
            throw new InvalidOperationException("Fail to convert object.");
        }



        /// <summary>
        /// Empression to "Query<TDocument, TProjection>"
        /// </summary>
        /// <param name="expression"></param>
        [return: NotNull]
        public static implicit operator Query<TDocument, TProjection>([NotNull] Func<TDocument, bool> expression) => new(expression);

        /// <summary>
        /// Implicit conversion operator from a lambda expression with a single parameter to a Query object.
        /// </summary>
        /// <param name="expression">The lambda expression.</param>
        /// <returns>A new Query object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the expression is null.</exception>
        [return: NotNull]
        public static implicit operator Query<TDocument, TProjection>([NotNull] Func<TDocument, TDocument, bool> expression) => new(expression);

        /// <summary>
        /// Implicit conversion operator from a Query object to a FilterDefinition object.
        /// </summary>
        /// <param name="query">The Query object to convert.</param>
        /// <returns>A new FilterDefinition object.</returns>
        [return: NotNull]
        public static implicit operator FilterDefinition<TDocument>([NotNull] Query<TDocument, TProjection> query)
        {
            // Check if the query is null or default
            if (query == default)
            {
                return default;
            }

            // Check if the query has a JSON query
            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                // Create a new JsonFilterDefinition object with the JSON query
                return new JsonFilterDefinition<TDocument>(query.JsonQuery);
            }

            // Check if the query has an expression query
            else if (query.ExpressionQuery != null)
            {
                // Create a new ExpressionFilterDefinition object with the expression query
                return new ExpressionFilterDefinition<TDocument>(query.ExpressionQuery);
            }

            // Check if the query has full-text search options
            else if (query.FullTextSearchOptions != default)
            {
                // Create a new TextFilterDefinition object with the full-text search options
                return Builders<TDocument>.Filter.Text(query.FullTextSearchOptions!.Value.Item1, (TextSearchOptions)query.FullTextSearchOptions!.Value.Item2);
            }

            // Check if the query has an incomplete expression query
            else if (query.IncompletedExpressionQuery != null)
            {
                // Throw an exception if the query requires a constant value
                throw new InvalidOperationException("Fail to convert an expression that requires an constant.");
            }

            // Return an empty FilterDefinition object if none of the above conditions are met
            else
            {
                return FilterDefinition<TDocument>.Empty;
            }
        }

        /// <summary>
        /// Implicitly converts a Query object to an UpdateDefinition object.
        /// </summary>
        /// <param name="query">The Query object to convert.</param>
        /// <returns>A new UpdateDefinition object.</returns>
        [return: NotNull]
        public static implicit operator UpdateDefinition<TDocument>([NotNull] Query<TDocument, TProjection> query)
        {
            // If the query is null or default, return the default UpdateDefinition
            if (query == default)
            {
                return default;
            }

            // If the query has an update, return it
            if (query.Update != null)
            {
                return query.Update;
            }
            else
            {
                // Otherwise, return the default UpdateDefinition
                return default;
            }
        }

        /// <summary>
        /// Implicitly converts a Query object to a ProjectionDefinition object.
        /// </summary>
        /// <param name="query">The Query object to convert.</param>
        /// <returns>A new ProjectionDefinition object.</returns>
        [return: NotNull]
        public static implicit operator ProjectionDefinition<TDocument, TProjection>([NotNull] Query<TDocument, TProjection> query)
        {
            // If the query is null or default, return the default ProjectionDefinition
            if (query == default)
            {
                return default;
            }

            // If the query has a JSON query, return it
            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                return query.JsonQuery;
            }

            // Otherwise, convert the expression query to a BsonDocument and return it
            return query.ExpressionQuery.ToBsonDocument();
        }

        /// <summary>
        /// Implicitly converts a Query object to a SortDefinition object.
        /// </summary>
        /// <param name="query">The Query object to convert.</param>
        /// <returns>A new SortDefinition object.</returns>
        [return: NotNull]
        public static implicit operator SortDefinition<TDocument>([NotNull] Query<TDocument, TProjection> query)
        {
            // If the query is null or default, return the default SortDefinition
            if (query == default)
            {
                return default;
            }

            // If the query has a JSON query, return it
            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                return query.JsonQuery;
            }

            // Otherwise, convert the expression query to a BsonDocument and return it
            return query.ExpressionQuery.ToBsonDocument();
        }

        /// <summary>
        /// Implicitly converts a Query object to a BsonDocument object.
        /// </summary>
        /// <param name="query">The Query object to convert.</param>
        /// <returns>A new BsonDocument object.</returns>
        [return: NotNull]
        public static implicit operator BsonDocument([NotNull] Query<TDocument, TProjection> query)
        {
            // If the query is null or default, return the default BsonDocument
            if (query == default)
            {
                return default;
            }

            // If the query has a JSON query, deserialize it into a BsonDocument and return it
            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                return BsonSerializer.Deserialize<BsonDocument>(query.JsonQuery);
            }

            // Otherwise, convert the expression query to a BsonDocument and return it
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
