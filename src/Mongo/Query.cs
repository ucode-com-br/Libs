using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents a query for a specific document type in a generic manner.
    /// This is a record type that inherits from QueryBase, allowing for 
    /// immutable instances that provide equality and value-based behavior.
    /// </summary>
    /// <typeparam name="TDocument">The type of document that this query is targeting.</typeparam>
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



        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class with the specified BSON elements.
        /// This constructor passes the provided BSON documents to the base class constructor.
        /// </summary>
        /// <param name="bsonElements">An array of <see cref="BsonDocument"/> used to initialize the query.</param>
        /// <returns>
        /// This constructor does not return a value, but initializes the instance of the <see cref="Query"/> class.
        /// </returns>
        internal Query(BsonDocument[] bsonElements) : base(bsonElements)
        {
        }
        #endregion Constructors

        #region Static Methods

        /// <summary>
        /// Creates a new <see cref="Query{TDocument}"/> instance from the provided aggregation pipeline.
        /// </summary>
        /// <param name="pipeline">
        /// An array of <see cref="BsonDocument"/> representing the stages of the aggregation pipeline.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="Query{TDocument}"/> initialized with the specified pipeline and a default update.
        /// </returns>
        public static Query<TDocument> FromPipeline([NotNull] BsonDocument[] pipeline) => new(pipeline)
        {
            Update = default
        };



        /// <summary>
        /// Creates a new <see cref="Query{TDocument}"/> object from the specified BSON query and update document.
        /// </summary>
        /// <param name="query">The BSON document representing the query filters.</param>
        /// <param name="update">The BSON document representing the update operations to apply.</param>
        /// <returns>A <see cref="Query{TDocument}"/> object containing the specified query and update information.</returns>
        public static Query<TDocument> FromQuery([NotNull] BsonDocument query, BsonDocument update) => new(query)
        {
            Update = new Update<TDocument>(update)
        };

        /// <summary>
        /// Creates a new instance of <see cref="Query{TDocument}"/> using the specified BsonDocument query and update.
        /// </summary>
        /// <param name="query">The BsonDocument that represents the query to be executed.</param>
        /// <param name="update">An <see cref="Update{TDocument}"/> instance that contains the update operations to be applied.</param>
        /// <returns>
        /// A <see cref="Query{TDocument}"/> object that encapsulates the specified query and update.
        /// </returns>
        public static Query<TDocument> FromQuery([NotNull] BsonDocument query, Update<TDocument> update) => new(query)
        {
            Update = update
        };

        /// <summary>
        /// Creates a <see cref="Query{TDocument}"/> instance from a specified BSON document query.
        /// </summary>
        /// <param name="query">
        /// A <see cref="BsonDocument"/> that represents the query. 
        /// This parameter must not be null.
        /// </param>
        /// <returns>
        /// A <see cref="Query{TDocument}"/> that corresponds to the provided BSON document query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="query"/> is null.
        /// </exception>
        public static Query<TDocument> FromQuery([NotNull] BsonDocument query) => new(query);


        /// <summary>
        /// Creates a new instance of the <see cref="Query{TDocument}"/> class from a specified query string and an optional update.
        /// </summary>
        /// <param name="query">A string representing the query to be executed.</param>
        /// <param name="update">An optional <see cref="Update{TDocument}"/> instance containing the updates to be applied.</param>
        /// <returns>A new <see cref="Query{TDocument}"/> instance configured with the specified query and optional update.</returns>
        public static Query<TDocument> FromQuery([NotNull] string query, Update<TDocument> update = default) => new(query)
        {
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
        public Query<TDocument> CompleteExpression(TDocument constrainValue) =>
            // Create a new Query object with the completed expression
            new Query<TDocument>(base.CompleteExpression(constrainValue));


        #region Operator & | ! +

        /// <summary>
        /// Defines the addition operator for combining two <see cref="Query{TDocument}"/> instances.
        /// </summary>
        /// <param name="lhs">The left-hand side <see cref="Query{TDocument}"/> operand.</param>
        /// <param name="rhs">The right-hand side <see cref="Query{TDocument}"/> operand.</param>
        /// <returns>
        /// A new <see cref="Query{TDocument}"/> instance that represents the combined filter 
        /// of the given left-hand side and right-hand side queries.
        /// </returns>
        public static Query<TDocument>? operator +(Query<TDocument>? lhs, Query<TDocument>? rhs) =>
            (lhs == default && rhs == default) ?
                default :
            (lhs == default && rhs != default) ?
                rhs :
            (lhs != default && rhs == default) ?
                lhs :
            (FilterDefinition<TDocument>)lhs! & (FilterDefinition<TDocument>)rhs!;

        /// <summary>
        /// Defines the bitwise AND operator for combining two queries.
        /// </summary>
        /// <typeparam name="TDocument">The type of the documents in the query.</typeparam>
        /// <param name="lhs">The left-hand side query to combine.</param>
        /// <param name="rhs">The right-hand side query to combine.</param>
        /// <returns>A new <see cref="Query{TDocument}"/> that combines the two specified queries using a bitwise AND operation.</returns>
        /// <remarks>
        /// This operator allows for combining two queries so that the results match both conditions specified in the left and right queries.
        /// </remarks>
        public static Query<TDocument>? operator &(Query<TDocument>? lhs, Query<TDocument>? rhs) =>
            (lhs == default && rhs == default) ?
                default :
            (lhs == default && rhs != default) ?
                rhs :
            (lhs != default && rhs == default) ?
                lhs :
            (FilterDefinition<TDocument>)lhs! & (FilterDefinition<TDocument>)rhs!;

        /// <summary>
        /// Defines the bitwise OR operator for two <see cref="Query{TDocument}"/> instances.
        /// This operator combines the filter definitions of both queries.
        /// </summary>
        /// <param name="lhs">The left-hand side <see cref="Query{TDocument}"/>.</param>
        /// <param name="rhs">The right-hand side <see cref="Query{TDocument}"/>.</param>
        /// <returns>
        /// A new <see cref="Query{TDocument}"/> that represents the combined filter 
        /// of the two given queries. The operator applies the bitwise OR operation to their 
        /// corresponding <see cref="FilterDefinition{TDocument}"/> instances.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="lhs"/> or <paramref name="rhs"/> is <c>null</c>.
        /// </exception>
        public static Query<TDocument>? operator |(Query<TDocument>? lhs, Query<TDocument>? rhs) =>
            (lhs == default && rhs == default) ?
                default :
            (lhs == default && rhs != default) ?
                rhs :
            (lhs != default && rhs == default) ?
                lhs :
            (FilterDefinition<TDocument>)lhs! | (FilterDefinition<TDocument>)rhs!;

        /// <summary>
        /// Defines a logical negation operator for the <see cref="Query{TDocument}"/> class.
        /// This operator allows for the inversion of a query by applying the logical NOT operation.
        /// </summary>
        /// <param name="op">The <see cref="Query{TDocument}"/> instance to negate.</param>
        /// <returns>
        /// Returns a new <see cref="Query{TDocument}"/> instance that represents the negation
        /// of the specified query.
        /// </returns>
        /// <remarks>
        /// This operator is particularly useful when constructing complex queries where negation 
        /// is needed to filter out documents matching certain criteria.
        /// </remarks>
        public static Query<TDocument>? operator !(Query<TDocument>? op) =>
            op == default ?
            default :
            !(FilterDefinition<TDocument>)op!;
        #endregion Operator & | ! +




        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object. This method 
        /// is overridden to provide a string representation of the object 
        /// by calling the base class's ToString method.
        /// </returns>
        public override string ToString() => base.ToString();


        /// <summary>
        /// Implicitly converts a function that takes a <typeparamref name="TDocument"/> 
        /// and returns a boolean into a <see cref="Query{TDocument}"/>.
        /// </summary>
        /// <param name="expression">A function that evaluates a <typeparamref name="TDocument"/>.</param>
        /// <returns>A <see cref="Query{TDocument}"/> that represents the provided function.</returns>
        /// <typeparam name="TDocument">The type of document used in the query.</typeparam>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
        /// <remarks>
        /// This operator allows a simple syntax for creating a Query from a predicate that 
        /// can be used for filtering <typeparamref name="TDocument"/> items.
        /// </remarks>
        [return: MaybeNull]
        public static implicit operator Query<TDocument>([MaybeNull] Func<TDocument, bool> expression) => new(expression);


        /// <summary>
        /// Converts a <see cref="Query{TDocument}"/> to a <see cref="FilterDefinition{TDocument}"/>.
        /// This implicit operator allows for seamless conversion between query types to a MongoDB filter definition.
        /// </summary>
        /// <param name="query">The query to convert. It should be a valid instance of <see cref="Query{TDocument}"/>.</param>
        /// <returns>A <see cref="FilterDefinition{TDocument}"/> that corresponds to the provided query.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the expression query is incomplete and requires a constant.</exception>
        [return: MaybeNull]
        public static implicit operator FilterDefinition<TDocument>([MaybeNull] Query<TDocument> query)
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

        /// <summary>
        /// Implicitly converts a <see cref="Query{TDocument}"/> to an <see cref="UpdateDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="query">The query to be converted. Must not be null.</param>
        /// <returns>
        /// An <see cref="UpdateDefinition{TDocument}"/> representing the update information 
        /// from the given <see cref="Query{TDocument}"/>, or the default value if 
        /// the query is null or has no update property.
        /// </returns>
        [return: MaybeNull]
        public static implicit operator UpdateDefinition<TDocument>([MaybeNull] Query<TDocument> query)
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
        /// Converts a <see cref="Query{TDocument}"/> to a <see cref="ProjectionDefinition{TDocument}"/> 
        /// for use in MongoDB queries.
        /// </summary>
        /// <param name="query">The query object to be converted.</param>
        /// <returns>
        /// A <see cref="ProjectionDefinition{TDocument}"/> that represents the converted query.
        /// Returns the default value if the input query is null or uninitialized.
        /// </returns>
        [return: MaybeNull]
        public static implicit operator ProjectionDefinition<TDocument>?([MaybeNull] Query<TDocument>? query)
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
        /// Implicitly converts a <see cref="Query{TDocument}"/> instance to a <see cref="SortDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="query">The <see cref="Query{TDocument}"/> instance to convert.</param>
        /// <returns>
        /// A <see cref="SortDefinition{TDocument}"/> that represents the converted <paramref name="query"/>.
        /// If the <paramref name="query"/> is null or has a default value, it returns the default <see cref="SortDefinition{TDocument}"/>.
        /// </returns>
        [return: MaybeNull]
        public static implicit operator SortDefinition<TDocument>?([MaybeNull] Query<TDocument>? query)
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
        /// <param name="query">The query to be converted to a BsonDocument. This parameter is not null.</param>
        /// <returns>
        /// A <see cref="BsonDocument"/> representation of the query. 
        /// Returns the default value if the query is null or default.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="query"/> is null.</exception>
        [return: MaybeNull]
        public static implicit operator BsonDocument?([MaybeNull] Query<TDocument>? query)
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


        /// <summary>
        /// Converts an instance of <see cref="Query{TDocument}"/> to an array of <see cref="BsonDocument"/>.
        /// </summary>
        /// <param name="query">The query to convert, which can be null.</param>
        /// <returns>
        /// An array of <see cref="BsonDocument"/> representing the query's pipeline, or null if the query is null. 
        /// Throws <see cref="ArgumentNullException"/> if the pipeline is null.
        /// </returns>
        [return: MaybeNull]
        public static implicit operator BsonDocument[]?([MaybeNull] Query<TDocument>? query)
        {
            // If the query is null, return the default value
            if (query == default)
            {
                return default;
            }

            return query.Pipeline ?? throw new ArgumentNullException("Pipeline is null.");

        }


        #region implicity to constructors

        /// <summary>
        /// Implicitly converts a <see cref="FilterDefinition{TDocument}"/> to a <see cref="Query{TDocument}"/>.
        /// </summary>
        /// <param name="source">
        /// The <see cref="FilterDefinition{TDocument}"/> instance to be converted.
        /// </param>
        /// <returns>
        /// An instance of <see cref="Query{TDocument}"/> that represents the given <see cref="FilterDefinition{TDocument}"/>.
        /// </returns>
        [return: MaybeNull]
        public static implicit operator Query<TDocument>?([MaybeNull] FilterDefinition<TDocument>? source) => source == default ? default : new (source);


        /// <summary>
        /// Defines an implicit conversion from a string to a <see cref="Query{TDocument}"/>.
        /// </summary>
        /// <param name="query">
        /// A string representing the query that will be converted into a <see cref="Query{TDocument}"/> instance.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="Query{TDocument}"/> initialized with the provided query string.
        /// </returns>
        [return: MaybeNull]
        public static implicit operator Query<TDocument>?([MaybeNull] string? query) => query == default ? default : new (query);


        /// <summary>
        /// Implicitly converts an expression of type <see cref="Expression{Func{TDocument, bool}}"/> 
        /// to a <see cref="Query{TDocument}"/>.
        /// </summary>
        /// <param name="expression">The expression to convert, representing a predicate 
        /// for filtering documents of type <typeparamref name="TDocument"/>.</param>
        /// <returns>A new instance of <see cref="Query{TDocument}"/> initialized with the given expression.</returns>
        [return: MaybeNull]
        public static implicit operator Query<TDocument>?([MaybeNull] Expression<Func<TDocument, bool>>? expression)
        {
            if (expression == default)
            {
                return default;
            }

            // Create a new instance of Query<TDocument> with the provided expression
            Query<TDocument> query = new(expression);

            // Return the new instance
            return query;
        }


        /// <summary>
        /// Implicitly converts a lambda expression that represents a predicate for filtering the documents 
        /// into a <see cref="Query{TDocument}"/> instance.
        /// </summary>
        /// <param name="expression">A lambda expression of type <see cref="Expression{Func{TDocument, TDocument, bool}}"/> 
        /// that defines the filtering criteria for the documents.</param>
        /// <returns>A new instance of <see cref="Query{TDocument}"/> initialized with the specified expression.</returns>
        [return: MaybeNull]
        public static implicit operator Query<TDocument>?([MaybeNull] Expression<Func<TDocument, TDocument, bool>>? expression)
        {
            if (expression == default)
            {
                return default;
            }

            // Create a new instance of Query<TDocument> with the provided expression
            Query<TDocument> query = new(expression);

            // Return the newly created Query<TDocument> instance
            return query;
        }
        #endregion implicity to constructors
    }

    /// <summary>
    /// Represents a query that encapsulates both the document type and the projection type.
    /// This record inherits from <see cref="QueryBase{TDocument}"/>.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document being queried.</typeparam>
    /// <typeparam name="TProjection">The type of the projection that will be returned from the query.</typeparam>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class with the specified BSON elements.
        /// This constructor passes the provided BSON documents to the base class constructor.
        /// </summary>
        /// <param name="bsonElements">An array of <see cref="BsonDocument"/> used to initialize the query.</param>
        /// <returns>
        /// This constructor does not return a value, but initializes the instance of the <see cref="Query"/> class.
        /// </returns>
        internal Query(BsonDocument[] bsonElements) : base(bsonElements)
        {
        }
        #endregion Constructors


        /// <summary>
        /// Completes the expression for a query with a specified constraint value.
        /// </summary>
        /// <param name="constrainValue">The document that serves as a constraint for the query expression.</param>
        /// <returns>A new instance of <see cref="Query{TDocument, TProjection}"/> that is completed based on the provided constraint.</returns>
        /// <remarks>
        /// This method overrides the base class’s implementation and initializes
        /// the new Query instance using the result from the base’s CompleteExpression method.
        /// </remarks>
        public new Query<TDocument, TProjection> CompleteExpression(TDocument constrainValue) => new Query<TDocument, TProjection>(base.CompleteExpression(constrainValue));


        #region Static Methods

        /// <summary>
        /// Creates a new <see cref="Query{TDocument}"/> instance from the provided aggregation pipeline.
        /// </summary>
        /// <param name="pipeline">
        /// An array of <see cref="BsonDocument"/> representing the stages of the aggregation pipeline.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="Query{TDocument}"/> initialized with the specified pipeline and a default update.
        /// </returns>
        public static Query<TDocument, TProjection> FromPipeline([NotNull] BsonDocument[] pipeline) => new(pipeline)
        {
            Update = default
        };

        /// <summary>
        /// Creates a new instance of the <see cref="Query{TDocument, TProjection}"/> class 
        /// from the specified BsonDocument query and update documents.
        /// </summary>
        /// <param name="query">The BsonDocument representing the query parameters.</param>
        /// <param name="update">The BsonDocument representing the update operations to be applied.</param>
        /// <returns>
        /// Returns a new instance of the <see cref="Query{TDocument, TProjection}"/> 
        /// containing the specified query and update.
        /// </returns>
        public static Query<TDocument, TProjection> FromQuery([NotNull] BsonDocument query, [NotNull] BsonDocument update) => new(query)
        {
            Update = new Update<TDocument>(update)
        };

        /// <summary>
        /// Creates a new instance of the <see cref="Query{TDocument, TProjection}"/> class 
        /// from a specified BsonDocument query and an update.
        /// </summary>
        /// <param name="query">
        /// The BsonDocument representing the query to be used for the new Query instance.
        /// This document should be constructed according to the MongoDB query format.
        /// </param>
        /// <param name="update">
        /// An instance of <see cref="Update{TDocument}"/> which defines the updates 
        /// to be applied to the documents that match the query.
        /// </param>
        /// <returns>
        /// A new <see cref="Query{TDocument, TProjection}"/> instance that uses the specified 
        /// query and update parameters.
        /// </returns>
        public static Query<TDocument, TProjection> FromQuery([NotNull] BsonDocument query, [NotNull] Update<TDocument> update) => new(query)
        {
            Update = update
        };

        /// <summary>
        /// Creates a new instance of the <see cref="Query{TDocument, TProjection}"/> class from the specified BSON document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document to be queried.</typeparam>
        /// <typeparam name="TProjection">The type of the projected result.</typeparam>
        /// <param name="query">A <see cref="BsonDocument"/> representing the query to be executed.</param>
        /// <returns>A <see cref="Query{TDocument, TProjection}"/> instance initialized with the specified query.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="query"/> parameter is null.</exception>
        public static Query<TDocument, TProjection> FromQuery([NotNull] BsonDocument query) => new(query);

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


        #region Operator & | ! +

        /// <summary>
        /// Defines the addition operator for combining two <see cref="Query{TDocument, TProjection}"/> instances.
        /// </summary>
        /// <param name="lhs">The left-hand side <see cref="Query{TDocument, TProjection}"/> operand.</param>
        /// <param name="rhs">The right-hand side <see cref="Query{TDocument, TProjection}"/> operand.</param>
        /// <returns>
        /// A new <see cref="Query{TDocument, TProjection}"/> instance that represents the combined filter 
        /// of the given left-hand side and right-hand side queries.
        /// </returns>
        public static Query<TDocument, TProjection>? operator +(Query<TDocument, TProjection>? lhs, Query<TDocument, TProjection>? rhs) =>
            lhs == default && rhs == default ?
            default :
            lhs != default && rhs == default ?
            lhs :
            lhs == default && rhs != default ?
            rhs :
            (FilterDefinition<TDocument>)lhs! & (FilterDefinition<TDocument>)lhs!;

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
        public static Query<TDocument, TProjection>? operator &(Query<TDocument, TProjection>? lhs, Query<TDocument, TProjection>? rhs) =>
            lhs == default && rhs == default ?
            default :
            lhs != default && rhs == default ?
            lhs :
            lhs == default && rhs != default ?
            rhs :
            (FilterDefinition<TDocument>)lhs! & (FilterDefinition<TDocument>)lhs!;

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
        public static Query<TDocument, TProjection>? operator |(Query<TDocument, TProjection>? lhs, Query<TDocument, TProjection>? rhs) =>
            lhs == default && rhs == default ?
            default :
            lhs != default && rhs == default ?
            lhs :
            lhs == default && rhs != default ?
            rhs :
            (FilterDefinition<TDocument>)lhs! | (FilterDefinition<TDocument>)lhs!;

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
        public static Query<TDocument, TProjection>? operator !(Query<TDocument, TProjection>? op) =>
            op == default ?
            default :
            !(FilterDefinition<TDocument>)op!;
        #endregion Operator & | ! +




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
        [return: MaybeNull]
        public static implicit operator Query<TDocument, TProjection>?([MaybeNull] Query<TDocument>? query)
        {
            if (query == default)
                return default;

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
        [return: MaybeNull]
        public static implicit operator Query<TDocument>?([MaybeNull] Query<TDocument, TProjection>? query)
        {
            if (query == default)
                return default;

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
        [return: MaybeNull]
        public static implicit operator Query<TDocument, TProjection>?([MaybeNull] Func<TDocument, bool> expression) => expression == default ? default : new(expression);

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
        [return: MaybeNull]
        public static implicit operator Query<TDocument, TProjection>?([MaybeNull] Func<TDocument, TDocument, bool>? expression) => expression == default ? default : new(expression);

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
        [return: MaybeNull]
        public static implicit operator FilterDefinition<TDocument>?([MaybeNull] Query<TDocument, TProjection>? query)
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
        [return: MaybeNull]
        public static implicit operator UpdateDefinition<TDocument>?([NotNull] Query<TDocument, TProjection>? query)
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
        [return: MaybeNull]
        public static implicit operator ProjectionDefinition<TDocument, TProjection>?([MaybeNull] Query<TDocument, TProjection>? query)
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
        /// Implicitly converts a <see cref="Query{TDocument, TProjection}"/> to a <see cref="SortDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="query">The query to convert. Must not be null.</param>
        /// <returns>A <see cref="SortDefinition{TDocument}"/> representing the query.</returns>
        [return: MaybeNull]
        public static implicit operator SortDefinition<TDocument>?([MaybeNull] Query<TDocument, TProjection>? query)
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
        [return: MaybeNull]
        public static implicit operator BsonDocument?([MaybeNull] Query<TDocument, TProjection>? query)
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



        #region Implicity to constructors

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
        [return: MaybeNull]
        public static implicit operator Query<TDocument, TProjection>?(string? query)
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
        [return: MaybeNull]
        public static implicit operator Query<TDocument, TProjection>?([MaybeNull] Expression<Func<TDocument, bool>>? query)
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
        [return: MaybeNull]
        public static implicit operator Query<TDocument, TProjection>?([MaybeNull] Expression<Func<TDocument, TDocument, bool>>? query)
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
        [return: MaybeNull]
        public static implicit operator Query<TDocument, TProjection>?([MaybeNull] FilterDefinition<TDocument>? source) => source == default ? default : new(source);

        /// <summary>
        /// Implicitly converts a <see cref="Query{TDocument, TProjection}"/> to an array of <see cref="BsonDocument"/>.
        /// </summary>
        /// <param name="query">The <see cref="Query{TDocument, TProjection}"/> to convert.</param>
        /// <returns>
        /// An array of <see cref="BsonDocument"/> if the query is not null; otherwise, returns null.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="query"/> has a null <c>Pipeline</c>.</exception>
        [return: MaybeNull]
        public static implicit operator BsonDocument[]?([MaybeNull] Query<TDocument, TProjection>? query)
        {

            // If the query is null, return the default value
            if (query == default)
            {
                return default;
            }

            return query.Pipeline ?? throw new ArgumentNullException("Pipeline is null.");

        }

        #endregion implicity to constructors




    }
}
