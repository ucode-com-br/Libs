using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using UCode.Extensions;
using UCode.Mongo.Options;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents a query for documents of type TDocument.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
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

        /// <summary>
        /// Creates a new instance of the Query class from a string query.
        /// </summary>
        /// <param name="query">The query string.</param>
        /// <param name="update">The update operation.</param>
        /// <returns>A new instance of the Query class.</returns>
        public static Query<TDocument> FromQuery([NotNull] string query, Update<TDocument> update = default) => new(query)
        {
            // Initialize the Update property with the provided update
            Update = update
        };

        /// <summary>
        /// Creates a new instance of the Query class from an expression query.
        /// </summary>
        /// <param name="func">The expression query.</param>
        /// <param name="update">The update operation.</param>
        /// <returns>A new instance of the Query class.</returns>
        public static Query<TDocument> FromExpression([NotNull] Expression<Func<TDocument, bool>> func, Update<TDocument> update = default) => new(func)
        {
            // Initialize the Update property with the provided update
            Update = update
        };

        /// <summary>
        /// Creates a new instance of the Query class from an expression query.
        /// </summary>
        /// <param name="func">The expression query.</param>
        /// <param name="update">The update operation.</param>
        /// <returns>A new instance of the Query class.</returns>
        public static Query<TDocument> FromExpression([NotNull] Expression<Func<TDocument, TDocument, bool>> func, Update<TDocument> update = default) => new(func)
        {
            // Initialize the Update property with the provided update
            Update = update
        };

        /// <summary>
        /// Creates a new instance of the Query class from a full-text search query.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="fulltextSearchOptions">Options for the full-text search.</param>
        /// <returns>A new instance of the Query class.</returns>
        public static Query<TDocument> FromText([NotNull] string text, [NotNull] FullTextSearchOptions<TDocument> fulltextSearchOptions = default) => new(Builders<TDocument>.Filter.Text(text, fulltextSearchOptions));
        #endregion Static Methods

        /// <summary>
        /// Completes the expression of the query by replacing the incomplete expression with a constant value.
        /// </summary>
        /// <param name="constrainValue">The constant value to replace the incomplete expression with.</param>
        /// <returns>A new Query object with the completed expression.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the query does not have an incomplete expression.</exception>
        public Query<TDocument> CompleteExpression(TDocument constrainValue)
        {
            // Create a new Query object with the completed expression
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

        /// <summary>
        /// Implicitly converts a <see cref="Func{TDocument, bool}"/> to a <see cref="Query{TDocument}"/>.
        /// </summary>
        /// <param name="expression">The expression to convert.</param>
        /// <returns>A new instance of <see cref="Query{TDocument}"/>.</returns>
        [return: NotNull]
        public static implicit operator Query<TDocument>([NotNull] Func<TDocument, bool> expression) => new(expression);

        /// <summary>
        /// Implicitly converts a <see cref="Query{TDocument}"/> to a <see cref="FilterDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="query">The query to convert.</param>
        /// <returns>A <see cref="FilterDefinition{TDocument}"/> representing the query.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the query requires a constant but does not have one.</exception>
        [return: NotNull]
        public static implicit operator FilterDefinition<TDocument>([NotNull] Query<TDocument> query)
        {
            if (query == default)
            {
                return default;
            }

            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                // If the query is a JSON query, create a new JsonFilterDefinition
                return new JsonFilterDefinition<TDocument>(query.JsonQuery);
            }
            else if (query.ExpressionQuery != null)
            {
                // If the query is an expression query, create a new ExpressionFilterDefinition
                return new ExpressionFilterDefinition<TDocument>(query.ExpressionQuery);
            }
            else if (query.FullTextSearchOptions != default)
            {
                // If the query is a full-text search query, create a new TextFilterDefinition
                return Builders<TDocument>.Filter.Text(query.FullTextSearchOptions!.Value.Item1, (TextSearchOptions)query.FullTextSearchOptions!.Value.Item2);
            }
            else if (query.IncompletedExpressionQuery != null)
            {
                // If the query requires a constant but does not have one, throw an exception
                throw new InvalidOperationException("Fail to convert an expression that requires an constant.");
            }
            else
            {
                // If the query is empty, return an empty FilterDefinition
                return FilterDefinition<TDocument>.Empty;
            }
        }

        /// <summary>
        /// Implicitly converts a <see cref="Query{TDocument}"/> to a <see cref="UpdateDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="query">The query to convert.</param>
        /// <returns>A <see cref="UpdateDefinition{TDocument}"/> representing the query.</returns>
        [return: NotNull]
        public static implicit operator UpdateDefinition<TDocument>([NotNull] Query<TDocument> query)
        {
            // If the query is null, return the default value
            if (query == default)
            {
                return default;
            }

            // If the query has an Update property, return it
            if (query.Update != null)
            {
                return query.Update;
            }
            else
            {
                // Otherwise, return the default value
                return default;
            }
        }

        /// <summary>
        /// Implicitly converts a <see cref="Query{TDocument}"/> to a <see cref="ProjectionDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="query">The query to convert.</param>
        /// <returns>A <see cref="ProjectionDefinition{TDocument}"/> representing the query.</returns>
        [return: NotNull]
        public static implicit operator ProjectionDefinition<TDocument>([NotNull] Query<TDocument> query)
        {
            // If the query is null, return the default value
            if (query == default)
            {
                return default;
            }

            // Convert the query to a BsonDocument and return it
            return ((FilterDefinition<TDocument>)query).ToBsonDocument();


            //if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            //    return query.JsonQuery;

            //return query.ExpressionQuery.ToBsonDocument();
        }

        /// <summary>
        /// Implicitly converts a <see cref="Query{TDocument}"/> to a <see cref="SortDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="query">The query to convert.</param>
        /// <returns>A <see cref="SortDefinition{TDocument}"/> representing the query.</returns>
        [return: NotNull]
        public static implicit operator SortDefinition<TDocument>([NotNull] Query<TDocument> query)
        {
            // If the query is null, return the default value
            if (query == default)
            {
                return default;
            }

            // Convert the query to a BsonDocument and return it
            return ((FilterDefinition<TDocument>)query).ToBsonDocument();

            //if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            //    return query.JsonQuery;

            //return query.ExpressionQuery.ToBsonDocument();
        }

        /// <summary>
        /// Implicitly converts a <see cref="Query{TDocument}"/> to a <see cref="BsonDocument"/>.
        /// </summary>
        /// <param name="query">The query to convert.</param>
        /// <returns>A <see cref="BsonDocument"/> representing the query.</returns>
        [return: NotNull]
        public static implicit operator BsonDocument([NotNull] Query<TDocument> query)
        {
            // If the query is null, return the default value
            if (query == default)
            {
                return default;
            }

            // Convert the query to a BsonDocument and return it
            var f = (FilterDefinition<TDocument>)query;

            return f.ToBsonDocument();

            //return f.Render(new BsonClassMapSerializer<TDocument>(BsonClassMap<TDocument>.LookupClassMap(typeof(TDocument))), BsonSerializer.SerializerRegistry);

            //if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            //    return BsonSerializer.Deserialize<BsonDocument>();

            //return query.ExpressionQuery.ToBsonDocument();
        }

        #region implicity to constructors

        /// <summary>
        /// Implicitly converts a <see cref="FilterDefinition{TDocument}"/> to a <see cref="Query{TDocument}"/>.
        /// </summary>
        /// <param name="source">The filter definition to convert.</param>
        /// <returns>A new instance of <see cref="Query{TDocument}"/> initialized with the provided filter definition.</returns>
        public static implicit operator Query<TDocument>([NotNull] FilterDefinition<TDocument> source) => new(source);

        /// <summary>
        /// Implicitly converts a string to a <see cref="Query{TDocument}"/>.
        /// </summary>
        /// <param name="query">The query string to convert.</param>
        /// <returns>A new instance of <see cref="Query{TDocument}"/> initialized with the provided query string.</returns>
        public static implicit operator Query<TDocument>(string query) => new(query);

        /// <summary>
        /// Implicitly converts an expression to a <see cref="Query{TDocument}"/>.
        /// </summary>
        /// <param name="expression">The expression to convert.</param>
        /// <returns>A new instance of <see cref="Query{TDocument}"/> initialized with the provided expression.</returns>
        public static implicit operator Query<TDocument>(Expression<Func<TDocument, bool>> expression)
        {
            // Create a new instance of Query<TDocument> with the provided expression
            Query<TDocument> query = new(expression);

            // Return the new instance
            return query;
        }

        /// <summary>
        /// Implicitly converts an expression to a <see cref="Query{TDocument}"/>.
        /// </summary>
        /// <param name="expression">The expression to convert.</param>
        /// <returns>A new instance of <see cref="Query{TDocument}"/> initialized with the provided expression.</returns>
        public static implicit operator Query<TDocument>(Expression<Func<TDocument, TDocument, bool>> expression)
        {
            // Create a new instance of Query<TDocument> with the provided expression
            Query<TDocument> query = new(expression);

            // Return the newly created Query<TDocument> instance
            return query;
        }
        #endregion implicity to constructors
    }
}
