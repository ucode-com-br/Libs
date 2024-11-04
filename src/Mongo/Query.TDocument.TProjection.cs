using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace UCode.Mongo
{
    public record Query<TDocument, TProjection> : QueryBase<TDocument>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class with the specified string.
        /// </summary>
        /// <param name="str">The input string used to initialize the <see cref="Query"/> instance.</param>
        /// <returns>
        /// This constructor does not return a value.
        /// </returns>
        internal Query([NotNull] string str) : base(str)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Query{TDocument}"/> class.
        /// </summary>
        /// <param name="expressionQuery">The expression query that specifies the conditions for the documents to be queried.</param>
        internal Query([NotNull] Expression<Func<TDocument, bool>> expressionQuery) : base(expressionQuery)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class.
        /// </summary>
        /// <param name="expressionQuery">
        /// A lambda expression that defines a query to be executed against <typeparamref name="TDocument"/>.
        /// </param>
        internal Query([NotNull] Expression<Func<TDocument, TDocument, bool>> expressionQuery) : base(expressionQuery)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Query{TDocument}"/> class 
        /// with the specified filter definition.
        /// </summary>
        /// <param name="filterDefinition">The filter definition to be applied to the query.
        /// It must not be null.</param>
        internal Query([NotNull] FilterDefinition<TDocument> filterDefinition) : base(filterDefinition)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class with the specified text and search options.
        /// </summary>
        /// <param name="text">The text to be queried.</param>
        /// <param name="fullTextSearchOptions">The options for full text search.</param>
        /// <returns>
        /// A new instance of the <see cref="Query"/> class.
        /// </returns>
        internal Query(string text, TextSearchOptions fullTextSearchOptions) : base(text, fullTextSearchOptions)
        {
        }
        #endregion Constructors

        /// <summary>
        /// Completes the expression for the specified document, allowing for additional constraints
        /// to be applied. This method overrides the base class method to provide custom behavior.
        /// </summary>
        /// <param name="constrainValue">The document constraint value to complete the expression with.</param>
        /// <returns>A new instance of <see cref="Query{TDocument, TProjection}"/> that contains the completed expression.</returns>
        public new Query<TDocument, TProjection> CompleteExpression(TDocument constrainValue) =>
            // Call the base class method to complete the expression
            new Query<TDocument, TProjection>(base.CompleteExpression(constrainValue));


        #region Static Methods
        /// <summary>
        /// Creates a new instance of the <see cref="Query{TDocument, TProjection}"/> class
        /// initialized with the specified query string and optional update operation.
        /// </summary>
        /// <param name="query">
        /// The query string used to construct the query.
        /// </param>
        /// <param name="update">
        /// An optional update operation of type <see cref="Update{TDocument}"/> 
        /// that can be applied to the document. If not provided, defaults to <c>default</c>.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="Query{TDocument, TProjection}"/> that 
        /// contains the specified query and update operation.
        /// </returns>
        public static Query<TDocument, TProjection> FromQuery([NotNull] string query, Update<TDocument> update = default) => new(query)
        {
            Update = update
        };

        /// <summary>
        /// Creates a new instance of the <see cref="Query{TDocument, TProjection}"/> class
        /// from the provided expression and optional update parameter.
        /// </summary>
        /// <param name="func">
        /// An expression that defines the criteria to use for the query. This is a
        /// function that takes a <typeparamref name="TDocument"/> as input and returns
        /// a boolean result.
        /// </param>
        /// <param name="update">
        /// An optional <see cref="Update{TDocument}"/> object that defines the update
        /// to apply to the documents that match the query criteria. If no update
        /// is provided, the default value is used.
        /// </param>
        /// <returns>
        /// A <see cref="Query{TDocument, TProjection}"/> instance representing the query
        /// defined by the provided expression and the optional update.
        /// </returns>
        public static Query<TDocument, TProjection> FromExpression([NotNull] Expression<Func<TDocument, bool>> func, Update<TDocument> update = default) => new(func)
        {
            Update = update
        };

        /// <summary>
        /// Creates a new instance of the <see cref="Query{TDocument, TProjection}"/> class 
        /// using the specified expression for filtering documents.
        /// </summary>
        /// <param name="func">
        /// A lambda expression that takes two parameters of type <typeparamref name="TDocument"/> 
        /// and returns a boolean indicating whether a document matches the criteria.
        /// </param>
        /// <param name="update">
        /// An optional <see cref="Update{TDocument}"/> instance that specifies the update operations 
        /// to be applied to the documents that match the query. This is set to <c>default</c> 
        /// if not provided.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="Query{TDocument, TProjection}"/> that contains the 
        /// specified filtering expression and the update information.
        /// </returns>
        public static Query<TDocument, TProjection> FromExpression([NotNull] Expression<Func<TDocument, TDocument, bool>> func, Update<TDocument> update = default) => new(func)
        {
            Update = update
        };

        /// <summary>
        /// Creates a query that filters documents based on a full-text search for a specified text.
        /// </summary>
        /// <typeparam name="TDocument">The type of the documents to be queried.</typeparam>
        /// <typeparam name="TProjection">The type of the projection for the results.</typeparam>
        /// <param name="text">The text to search for in the documents.</param>
        /// <param name="fulltextSearchOptions">Options that control the behavior of the full-text search.</param>
        /// <returns>A <see cref="Query{TDocument, TProjection}"/> object that represents the full-text search query.</returns>
        public static Query<TDocument, TProjection> FromText([NotNull] string text, [NotNull] TextSearchOptions fulltextSearchOptions = default) => new(Builders<TDocument>.Filter.Text(text, fulltextSearchOptions));
        #endregion Static Methods



        #region Operator & | !

        /// <summary>
        /// Defines the addition operator for combining two <see cref="Query{TDocument, TProjection}"/> instances.
        /// </summary>
        /// <param name="lhs">The left-hand side <see cref="Query{TDocument, TProjection}"/> operand.</param>
        /// <param name="rhs">The right-hand side <see cref="Query{TDocument, TProjection}"/> operand.</param>
        /// <returns>
        /// A new <see cref="Query{TDocument, TProjection}"/> instance that represents the combined filter 
        /// of the given left-hand side and right-hand side queries.
        /// </returns>
        public static Query<TDocument, TProjection> operator +(Query<TDocument, TProjection> lhs, Query<TDocument, TProjection> rhs) => (FilterDefinition<TDocument>)lhs & (FilterDefinition<TDocument>)lhs;

        /// <summary>
        /// Defines the bitwise AND operator for combining two queries.
        /// </summary>
        /// <typeparam name="TDocument">The type of the documents in the query.</typeparam>
        /// <typeparam name="TProjection">The type of the projection for the query results.</typeparam>
        /// <param name="lhs">The left-hand side query to combine.</param>
        /// <param name="rhs">The right-hand side query to combine.</param>
        /// <returns>A new <see cref="Query{TDocument, TProjection}"/> that combines the two specified queries using a bitwise AND operation.</returns>
        /// <remarks>
        /// This operator allows for combining two queries so that the results match both conditions specified in the left and right queries.
        /// </remarks>
        public static Query<TDocument, TProjection> operator &(Query<TDocument, TProjection> lhs, Query<TDocument, TProjection> rhs) => (FilterDefinition<TDocument>)lhs & (FilterDefinition<TDocument>)lhs;

        /// <summary>
        /// Defines the bitwise OR operator for two <see cref="Query{TDocument, TProjection}"/> instances.
        /// This operator combines the filter definitions of both queries.
        /// </summary>
        /// <param name="lhs">The left-hand side <see cref="Query{TDocument, TProjection}"/>.</param>
        /// <param name="rhs">The right-hand side <see cref="Query{TDocument, TProjection}"/>.</param>
        /// <returns>
        /// A new <see cref="Query{TDocument, TProjection}"/> that represents the combined filter 
        /// of the two given queries. The operator applies the bitwise OR operation to their 
        /// corresponding <see cref="FilterDefinition{TDocument}"/> instances.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="lhs"/> or <paramref name="rhs"/> is <c>null</c>.
        /// </exception>
        public static Query<TDocument, TProjection> operator |(Query<TDocument, TProjection> lhs, Query<TDocument, TProjection> rhs) => (FilterDefinition<TDocument>)lhs | (FilterDefinition<TDocument>)lhs;

        /// <summary>
        /// Defines a logical negation operator for the <see cref="Query{TDocument, TProjection}"/> class.
        /// This operator allows for the inversion of a query by applying the logical NOT operation.
        /// </summary>
        /// <param name="op">The <see cref="Query{TDocument, TProjection}"/> instance to negate.</param>
        /// <returns>
        /// Returns a new <see cref="Query{TDocument, TProjection}"/> instance that represents the negation
        /// of the specified query.
        /// </returns>
        /// <remarks>
        /// This operator is particularly useful when constructing complex queries where negation 
        /// is needed to filter out documents matching certain criteria.
        /// </remarks>
        public static Query<TDocument, TProjection> operator !(Query<TDocument, TProjection> op) => !(FilterDefinition<TDocument>)op;
        #endregion Operator & | !


        #region Operator & | !

        /// <summary>
        /// Defines a binary operator for combining two <see cref="Query{TDocument, TProjection}"/> instances 
        /// using a logical AND operation.
        /// </summary>
        /// <param name="lhs">The left-hand side <see cref="Query{TDocument, TProjection}"/> to combine.</param>
        /// <param name="rhs">The right-hand side <see cref="Query{TDocument}"/> to combine.</param>
        /// <returns>A <see cref="Query{TDocument, TProjection}"/> that represents the combined criteria.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the filters cannot be combined.
        /// </exception>
        public static Query<TDocument, TProjection> operator &(Query<TDocument, TProjection> lhs, Query<TDocument> rhs) => (FilterDefinition<TDocument>)lhs & (FilterDefinition<TDocument>)lhs;

        /// <summary>
        /// Defines the bitwise AND operator for combining two <see cref="Query{TDocument}"/> objects.
        /// </summary>
        /// <param name="lhs">The left-hand side <see cref="Query{TDocument}"/> instance to combine.</param>
        /// <param name="rhs">The right-hand side <see cref="Query{TDocument, TProjection}"/> instance to combine.</param>
        /// <returns>
        /// A combined <see cref="Query{TDocument, TProjection}"/> resulting from the bitwise AND operation on the two queries.
        /// </returns>
        /// <remarks>
        /// This operator casts both queries to <see cref="FilterDefinition{TDocument}"/> 
        /// before performing the bitwise AND operation. 
        /// It is assumed that both queries are compatible for combining.
        /// </remarks>
        public static Query<TDocument, TProjection> operator &(Query<TDocument> lhs, Query<TDocument, TProjection> rhs) => (FilterDefinition<TDocument>)lhs & (FilterDefinition<TDocument>)lhs;

        /// <summary>
        /// Defines the bitwise OR operator for combining two <see cref="Query{TDocument, TProjection}"/> instances.
        /// </summary>
        /// <param name="lhs">The left-hand side <see cref="Query{TDocument, TProjection}"/> instance.</param>
        /// <param name="rhs">The right-hand side <see cref="Query{TDocument}"/> instance.</param>
        /// <returns>A <see cref="Query{TDocument, TProjection}"/> that represents the result of the bitwise OR operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="lhs"/> or <paramref name="rhs"/> is null.</exception>
        public static Query<TDocument, TProjection> operator |(Query<TDocument, TProjection> lhs, Query<TDocument> rhs) => (FilterDefinition<TDocument>)lhs | (FilterDefinition<TDocument>)lhs;

        /// <summary>
        /// Defines a bitwise OR operator for combining two queries.
        /// </summary>
        /// <param name="lhs">The left-hand side query of type <see cref="Query{TDocument}"/>.</param>
        /// <param name="rhs">The right-hand side query of type <see cref="Query{TDocument, TProjection}"/>.</param>
        /// <returns>A new <see cref="Query{TDocument, TProjection}"/> resulting from the bitwise OR operation of the two queries.</returns>
        /// <remarks>
        /// The implementation casts both query operands to <see cref="FilterDefinition{TDocument}"/> 
        /// before performing the bitwise OR operation.
        /// </remarks>
        public static Query<TDocument, TProjection> operator |(Query<TDocument> lhs, Query<TDocument, TProjection> rhs) => (FilterDefinition<TDocument>)lhs | (FilterDefinition<TDocument>)lhs;

        #endregion Operator & | !


        /// <inheritdoc/>
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object. In this implementation,
        /// it calls the base class's ToString method.
        /// </returns>
        /// <inheritdoc/>
        public override string ToString() => base.ToString();

        /// <summary>
        /// Implicitly converts a <see cref="Query{TDocument}"/> to a <see cref="Query{TDocument, TProjection}"/>.
        /// It checks various properties of the input query to determine the appropriate conversion.
        /// </summary>
        /// <param name="query">The query to convert, which is of type <see cref="Query{TDocument}"/>.</param>
        /// <returns>A new instance of <see cref="Query{TDocument, TProjection}"/> converted from the input query.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the conversion fails due to lacking valid properties in the input query.</exception>
        [return: NotNull]
        public static implicit operator Query<TDocument, TProjection>([NotNull] Query<TDocument> query)
        {
            // Check if the query has a JSON query
            if (!string.IsNullOrWhiteSpace(query.jsonQuery))
            {
                // Create a new Query object with the JSON query and update
                return new(query.jsonQuery)
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
        /// Implicitly converts a <see cref="Query{TDocument, TProjection}"/> to a <see cref="Query{TDocument}"/>.
        /// This conversion allows for automatic type conversion when assigning a query of type <see cref="Query{TDocument, TProjection}"/> 
        /// to a variable of type <see cref="Query{TDocument}"/>.
        /// </summary>
        /// <param name="query">
        /// The <see cref="Query{TDocument, TProjection}"/> instance to be converted.
        /// This instance may contain different forms of query definitions such as JSON queries, 
        /// expression queries, incomplete expression queries, filter definitions, or full-text search options.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="Query{TDocument}"/> that is created based on the provided <paramref name="query"/>. 
        /// The new instance will maintain the relevant properties such as the query structure and updates.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the provided <paramref name="query"/> cannot be converted into <see cref="Query{TDocument}"/> 
        /// because it lacks the necessary query definitions.
        /// </exception>
        [return: NotNull]
        public static implicit operator Query<TDocument>([NotNull] Query<TDocument, TProjection> query)
        {
            // Check if the query has a JSON query
            if (!string.IsNullOrWhiteSpace(query.jsonQuery))
            {
                // Create a new Query object with the JSON query and update
                return new(query.jsonQuery)
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
                return new(query.FullTextSearchOptions!.Value.Item1, query.FullTextSearchOptions.Value.Item2)
                {
                    Update = query.Update
                };
            }
            // Throw an exception if the query cannot be converted
            throw new InvalidOperationException("Fail to convert object.");
        }



        /// <summary>
        /// Defines an implicit conversion operator from a function that takes a 
        /// <typeparamref name="TDocument"/> and returns a boolean to a <see cref="Query{TDocument, TProjection}"/>.
        /// </summary>
        /// <param name="expression">
        /// A function that evaluates to <see langword="true"/> or <see langword="false"/> for a given 
        /// <typeparamref name="TDocument"/> instance.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="Query{TDocument, TProjection}"/> initialized with the provided function.
        /// </returns>
        /// <typeparam name="TDocument">
        /// The type of documents that this query will operate on.
        /// </typeparam>
        /// <typeparam name="TProjection">
        /// The type of projection that will be applied to the documents in the query.
        /// </typeparam>
        [return: NotNull]
        public static implicit operator Query<TDocument, TProjection>([NotNull] Func<TDocument, bool> expression) => new(expression);

        /// <summary>
        /// Implicitly converts a function that takes two <see cref="TDocument"/> parameters and returns a boolean value
        /// into a <see cref="Query{TDocument, TProjection}"/> instance.
        /// </summary>
        /// <param name="expression">
        /// A function that takes two <see cref="TDocument"/> objects and returns a boolean, 
        /// which is used to define the query logic.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="Query{TDocument, TProjection}"/> 
        /// initialized with the provided expression.
        /// </returns>
        /// <remarks>
        /// This operator allows for a more intuitive way to create queries by enabling 
        /// a functional syntax instead of requiring explicit constructor calls.
        /// </remarks>
        [return: NotNull]
        public static implicit operator Query<TDocument, TProjection>([NotNull] Func<TDocument, TDocument, bool> expression) => new(expression);

        /// <summary>
        /// Implicitly converts a <see cref="Query{TDocument,TProjection}"/> to a <see cref="FilterDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="query">The query to be converted, of type <see cref="Query{TDocument,TProjection}"/>.</param>
        /// <returns>
        /// A <see cref="FilterDefinition{TDocument}"/> object that corresponds to the provided query.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the query has an incomplete expression that requires a constant value.
        /// </exception>
        [return: NotNull]
        public static implicit operator FilterDefinition<TDocument>([NotNull] Query<TDocument, TProjection> query)
        {
            // Check if the query is null or default
            if (query == default)
            {
                return default;
            }


            // Check if the query has a JSON query
            if (!string.IsNullOrWhiteSpace(query.jsonQuery))
            {
                // Create a new JsonFilterDefinition object with the JSON query
                return new JsonFilterDefinition<TDocument>(query.jsonQuery);
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
            else if (query.FilterDefinition != null)
            {
                return query.FilterDefinition;
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
        /// Implicitly converts a <see cref="Query{TDocument, TProjection}"/> to an 
        /// <see cref="UpdateDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="query">The query to convert, which is a 
        /// <see cref="Query{TDocument, TProjection}"/> instance. It cannot be null.</param>
        /// <returns>
        /// An <see cref="UpdateDefinition{TDocument}"/> that results from the conversion. 
        /// If the <paramref name="query"/> is null or default, the default 
        /// <see cref="UpdateDefinition{TDocument}"/> is returned.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
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
        /// Implicitly converts a <see cref="Query{TDocument, TProjection}"/> into a 
        /// <see cref="MongoDB.Driver.ProjectionDefinition{TDocument, TProjection}"/>.
        /// This conversion allows the use of a query object directly where a projection 
        /// definition is expected.
        /// </summary>
        /// <param name="query">
        /// The query object that contains either a JSON query or an expression query.
        /// This parameter must not be null.
        /// </param>
        /// <returns>
        /// Returns a <see cref="MongoDB.Driver.ProjectionDefinition{TDocument, TProjection}"/> 
        /// that represents the provided query. If the query is null or default, 
        /// it returns the default projection definition. If the query contains a 
        /// JSON query, it returns the JSON query; otherwise, it converts the expression 
        /// query to a <see cref="BsonDocument"/>.
        /// </returns>
        [return: NotNull]
        public static implicit operator MongoDB.Driver.ProjectionDefinition<TDocument, TProjection>([NotNull] Query<TDocument, TProjection> query)
        {
            // If the query is null or default, return the default ProjectionDefinition
            if (query == default)
            {
                return default;
            }

            // If the query has a JSON query, return it
            if (!string.IsNullOrWhiteSpace(query.jsonQuery))
            {
                return query.jsonQuery;
            }

            // Otherwise, convert the expression query to a BsonDocument and return it
            return query.ExpressionQuery.ToBsonDocument();
        }

        /// <summary>
        /// Implicitly converts a <see cref="Query{TDocument, TProjection}"/> to a <see cref="SortDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="query">The query to convert. Must not be null.</param>
        /// <returns>A <see cref="SortDefinition{TDocument}"/> representing the query.</returns>
        [return: NotNull]
        public static implicit operator SortDefinition<TDocument>([NotNull] Query<TDocument, TProjection> query)
        {
            // If the query is null or default, return the default SortDefinition
            if (query == default)
            {
                return default;
            }

            // If the query has a JSON query, return it
            if (!string.IsNullOrWhiteSpace(query.jsonQuery))
            {
                return query.jsonQuery;
            }

            // Otherwise, convert the expression query to a BsonDocument and return it
            return query.ExpressionQuery.ToBsonDocument();
        }

        /// <summary>
        /// Implicitly converts a <see cref="Query{TDocument, TProjection}"/> instance 
        /// to a <see cref="BsonDocument"/>. This conversion handles both JSON query strings 
        /// and expression queries.
        ///
        /// </summary>
        /// <param name="query">The <see cref="Query{TDocument, TProjection}"/> instance 
        /// to be converted to a <see cref="BsonDocument"/>.
        /// </param>
        /// <returns>
        /// A <see cref="BsonDocument"/> representation of the provided query. 
        /// Returns the default <see cref="BsonDocument"/> if the query is null or the default value.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
        [return: NotNull]
        public static implicit operator BsonDocument([NotNull] Query<TDocument, TProjection> query)
        {
            // If the query is null or default, return the default BsonDocument
            if (query == default)
            {
                return default;
            }

            // If the query has a JSON query, deserialize it into a BsonDocument and return it
            if (!string.IsNullOrWhiteSpace(query.jsonQuery))
            {
                return BsonSerializer.Deserialize<BsonDocument>(query.jsonQuery);
            }

            // Otherwise, convert the expression query to a BsonDocument and return it
            return query.ExpressionQuery.ToBsonDocument();
        }



        #region implicity to constructors
        /// <summary>
        /// Implicitly converts a string representation of a query into a 
        /// <see cref="Query{TDocument, TProjection}"/> object.
        /// </summary>
        /// <param name="query">
        /// A string that contains the query to be converted. If the 
        /// string is null or empty, the method returns the default value.
        /// </param>
        /// <returns>
        /// A <see cref="Query{TDocument, TProjection}"/> object 
        /// created from the specified query string. If the input string 
        /// is null or empty, the default value for the <see cref="Query{TDocument, TProjection}"/>
        /// type is returned.
        /// </returns>
        public static implicit operator Query<TDocument, TProjection>(string query)
        {
            if (query == default)
            {
                return default;
            }

            return new(query);
        }

        /// <summary>
        /// Implicitly converts an expression that defines a query into a <see cref="Query{TDocument, TProjection}"/> instance.
        /// </summary>
        /// <param name="query">
        /// An expression representing a predicate for filtering documents of type <typeparamref name="TDocument"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Query{TDocument, TProjection}"/> instance corresponding to the specified query expression.
        /// If the <paramref name="query"/> is default, the method returns the default value of <see cref="Query{TDocument, TProjection}"/>.
        /// </returns>
        public static implicit operator Query<TDocument, TProjection>([NotNull] Expression<Func<TDocument, bool>> query)
        {
            if (query == default)
            {
                return default;
            }

            return new(query);
        }

        /// <summary>
        /// This implicit operator converts a specified query expression into a 
        /// <see cref="Query{TDocument, TProjection}"/> object.
        /// </summary>
        /// <param name="query">
        /// An expression of type <see cref="Expression{Func{TDocument, TDocument, bool}}"/> 
        /// that defines the query to be converted. 
        /// It represents a function that takes two <typeparamref name="TDocument"/> 
        /// objects and returns a boolean value.
        /// </param>
        /// <returns>
        /// A <see cref="Query{TDocument, TProjection}"/> instance 
        /// that represents the converted query. Returns the default if 
        /// the input query is null or not provided.
        /// </returns>
        public static implicit operator Query<TDocument, TProjection>([NotNull] Expression<Func<TDocument, TDocument, bool>> query)
        {
            if (query == default)
            {
                return default;
            }

            return new(query);
        }

        /// <summary>
        /// Defines an implicit conversion operator that allows a 
        /// <see cref="FilterDefinition{TDocument}"/> to be converted to a 
        /// <see cref="Query{TDocument, TProjection}"/>.
        /// </summary>
        /// <param name="source">
        /// The filter definition to be converted. This parameter is marked as 
        /// <see langword="NotNull"/> to indicate that it cannot be null.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="Query{TDocument, TProjection}"/> 
        /// created from the provided <paramref name="source"/> filter definition.
        /// </returns>
        public static implicit operator Query<TDocument, TProjection>([NotNull] FilterDefinition<TDocument> source) => new(source);


        #endregion implicity to constructors




    }
}
