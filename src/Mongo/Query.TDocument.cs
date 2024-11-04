using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using UCode.Extensions;

namespace UCode.Mongo
{
    public record Query<TDocument> : QueryBase<TDocument>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class.
        /// </summary>
        /// <param name="str">A <see cref="string"/> used to initialize the query.</param>
        /// <returns>
        /// This constructor does not return a value.
        /// </returns>
        internal Query(string str) : base(str)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class.
        /// </summary>
        /// <param name="expressionQuery">
        /// An expression that represents the query condition for filtering documents.
        /// </param>
        /// <returns>
        /// A <see cref="Query"/> instance configured with the specified query expression.
        /// </returns>
        internal Query(Expression<Func<TDocument, bool>> expressionQuery) : base(expressionQuery)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class.
        /// </summary>
        /// <param name="expressionQuery">The expression that defines the query logic to be executed.</param>
        internal Query(Expression<Func<TDocument, TDocument, bool>> expressionQuery) : base(expressionQuery)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Query{TDocument}"/> class.
        /// </summary>
        /// <param name="filterDefinition">
        /// The filter definition which is applied to the query. This defines the criteria for filtering the documents.
        /// </param>
        /// <returns>
        /// This constructor does not return a value, but initializes the base class with the provided filter definition.
        /// </returns>
        internal Query(FilterDefinition<TDocument> filterDefinition) : base(filterDefinition)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class.
        /// </summary>
        /// <param name="text">
        /// A string that represents the text to be searched.
        /// </param>
        /// <param name="fullTextSearchOptions">
        /// An instance of <see cref="TextSearchOptions"/> that specifies the options for full-text search.
        /// </param>
        /// <returns>
        /// The <see cref="Query"/> instance initialized with the specified text and search options.
        /// </returns>
        internal Query(string text, TextSearchOptions fullTextSearchOptions) : base(text, fullTextSearchOptions)
        {
        }
        #endregion Constructors

        #region Static Methods

        /// <summary>
        /// Creates a new instance of the <see cref="Query{TDocument}"/> class from a specified query string and an optional update.
        /// </summary>
        /// <param name="query">A string representing the query to be executed.</param>
        /// <param name="update">An optional <see cref="Update{TDocument}"/> instance containing the updates to be applied.</param>
        /// <returns>A new <see cref="Query{TDocument}"/> instance configured with the specified query and optional update.</returns>
        public static Query<TDocument> FromQuery([NotNull] string query, Update<TDocument> update = default) => new(query)
        {
            // Initialize the Update property with the provided update
            Update = update
        };

        /// <summary>
        /// Creates a new instance of the <see cref="Query{TDocument}"/> class from the specified lambda expression 
        /// and an optional update operation.
        /// </summary>
        /// <param name="func">A lambda expression that defines the criteria for the query, representing a 
        /// predicate to filter documents of type <typeparamref name="TDocument"/>.</param>
        /// <param name="update">An optional <see cref="Update{TDocument}"/> instance that specifies the update 
        /// operation to apply to the documents that match the query criteria. Defaults to <c>default</c>.</param>
        /// <returns>
        /// A new instance of <see cref="Query{TDocument}"/> that encapsulates the provided expression and update.
        /// </returns>
        public static Query<TDocument> FromExpression([NotNull] Expression<Func<TDocument, bool>> func, Update<TDocument> update = default) => new(func)
        {
            // Initialize the Update property with the provided update
            Update = update
        };

        /// <summary>
        /// Creates a new instance of the <see cref="Query{TDocument}"/> class from the specified expression.
        /// </summary>
        /// <param name="func">
        /// A function that represents a predicate for filtering documents. The function should take two 
        /// parameters of type <typeparamref name="TDocument"/> and return a <see cref="bool"/> indicating 
        /// whether the condition is met.
        /// </param>
        /// <param name="update">
        /// An optional <see cref="Update{TDocument}"/> instance that represents the update operations to 
        /// apply to the documents that match the predicate. This parameter is optional and defaults to 
        /// the default value of <typeparamref name="Update{TDocument}"/>.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="Query{TDocument}"/> that encapsulates the provided expression 
        /// and update operations.
        /// </returns>
        public static Query<TDocument> FromExpression([NotNull] Expression<Func<TDocument, TDocument, bool>> func, Update<TDocument> update = default) => new(func)
        {
            // Initialize the Update property with the provided update
            Update = update
        };

        /// <summary>
        /// Creates a text search query for the specified text and options.
        /// </summary>
        /// <typeparam name="TDocument">
        /// The type of the document to search.
        /// </typeparam>
        /// <param name="text">
        /// The text to search for. This parameter cannot be null.
        /// </param>
        /// <param name="fulltextSearchOptions">
        /// Options that define the behavior of the text search.
        /// This parameter is optional and defaults to a new instance of <see cref="TextSearchOptions"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Query{TDocument}"/> that represents the text search query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> is null.
        /// </exception>
        public static Query<TDocument> FromText([NotNull] string text, [NotNull] TextSearchOptions fulltextSearchOptions = default) => new(Builders<TDocument>.Filter.Text(text, fulltextSearchOptions));
        #endregion Static Methods

        /// <summary>
        /// Completes the expression for the specified document type and creates a new Query object.
        /// </summary>
        /// <param name="constrainValue">The document value that will be used to complete the expression.</param>
        /// <returns>Returns a new instance of the Query<TDocument> with the completed expression.</returns>
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

        public static Query<TDocument> operator &(Query<TDocument> lhs, Query<TDocument> rhs) => (FilterDefinition<TDocument>)lhs & (FilterDefinition<TDocument>)lhs;

        public static Query<TDocument> operator |(Query<TDocument> lhs, Query<TDocument> rhs) => (FilterDefinition<TDocument>)lhs | (FilterDefinition<TDocument>)lhs;

        public static Query<TDocument> operator !(Query<TDocument> op) => !(FilterDefinition<TDocument>)op;
        #endregion Operator & | !

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object. This method 
        /// is overridden to provide a string representation of the object 
        /// by calling the base class's ToString method.
        /// </returns>
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

            if (!string.IsNullOrWhiteSpace(query.jsonQuery))
            {
                // If the query is a JSON query, create a new JsonFilterDefinition
                return new JsonFilterDefinition<TDocument>(query.jsonQuery);
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
            else if (query.FilterDefinition != null)
            {
                return query.FilterDefinition;
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

        [return: NotNull]
        public static implicit operator MongoDB.Driver.ProjectionDefinition<TDocument>([NotNull] Query<TDocument> query)
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

        public static implicit operator Query<TDocument>([NotNull] FilterDefinition<TDocument> source) => new(source);

        public static implicit operator Query<TDocument>(string query) => new(query);

        public static implicit operator Query<TDocument>(Expression<Func<TDocument, bool>> expression)
        {
            // Create a new instance of Query<TDocument> with the provided expression
            Query<TDocument> query = new(expression);

            // Return the new instance
            return query;
        }

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
