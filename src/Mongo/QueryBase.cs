using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using UCode.Extensions;
using static UCode.Extensions.ExpressionExtensions;

namespace UCode.Mongo
{
    /// <summary>
    /// Abstract base record for building MongoDB queries with support for multiple query types
    /// (expression, JSON, text search) and conversion operators.
    /// </summary>
    /// <typeparam name="TDocument">The document type this query operates on</typeparam>
    /// <summary>
    /// Base class for MongoDB query operations
    /// </summary>
    /// <typeparam name="TDocument">The document type this query operates on</typeparam>
    /// <remarks>
    /// Implements basic CRUD operations and provides common functionality
    /// for derived query classes. Uses MongoDB driver for data access.
    /// <para>
    /// Supports multiple query types including expression-based, JSON-based,
    /// and full-text search queries.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class UserQuery : QueryBase<User>
    /// {
    ///     public async Task<User> GetByEmail(string email)
    ///     {
    ///         return await Collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract record QueryBase<TDocument>
    {
        /// <summary>
        /// Represents a pipeline of BsonDocuments used for constructing a query.
        /// </summary>
        /// <remarks>
        /// This field holds an array of BsonDocument objects that can be used
        /// in various operations, such as aggregation, to define the stages 
        /// of processing the data.
        /// </remarks>
        /// <value>
        /// An array of <see cref="BsonDocument"/> objects, or null if there are no documents in the pipeline.
        /// </value>
        internal BsonDocument[]? Pipeline;


        /// <summary>
        /// Represents an optional expression that can be used to query documents.
        /// This expression takes two documents of type TDocument and returns a boolean value.
        /// It is defined using the Expression<Func<TDocument, TDocument, bool>> delegate,
        /// allowing for the construction of strongly-typed expression trees for querying.
        /// </summary>
        /// <remarks>
        /// The expression may be null, indicating that no query is provided.
        /// </remarks>
        internal Expression<Func<TDocument, TDocument, bool>>? IncompletedExpressionQuery;


        /// <summary>
        /// Represents an expression query that can be used to filter documents of type <typeparamref name="TDocument"/>.
        /// This query is defined as a lambda expression that takes a <typeparamref name="TDocument"/> object 
        /// and returns a boolean value indicating whether the object satisfies certain conditions.
        /// </summary>
        /// <typeparam name="TDocument">The type of documents that will be filtered by the expression.</typeparam>
        /// <remarks>
        /// The <c>ExpressionQuery</c> is nullable, meaning it may not contain a valid expression, 
        /// in which case the filtered query would not apply. This is useful for cases 
        /// where queries are conditionally applied based on specific runtime criteria. 
        /// </remarks>
        internal Expression<Func<TDocument, bool>>? ExpressionQuery;


        /// <summary>
        /// Represents a query in JSON format that may be null.
        /// </summary>
        /// <remarks>
        /// This field can be used to store a JSON query string that is optional and can be absent (null).
        /// </remarks>
        internal string? JsonQuery;


        /// <summary>
        /// Represents an optional full-text search configuration.
        /// </summary>
        /// <remarks>
        /// This field is a nullable tuple that can hold two values: 
        /// a string that specifies the search query, and a 
        /// <see cref="TextSearchOptions"/> value that indicates 
        /// the options for the text search.
        /// </remarks>
        /// <value>
        /// A tuple containing the search query as a string and 
        /// the <c>TextSearchOptions</c>. If not set, it will be null.
        /// </value>
        internal (string, TextSearchOptions)? FullTextSearchOptions;


        /// <summary>
        /// Represents a filter definition to be applied on documents of type <typeparamref name="TDocument"/>.
        /// This field may hold a filter definition or be null if no filter has been specified.
        /// </summary>
        /// <typeparam name="TDocument">The type of the documents that the filter will be applied to.</typeparam>
        /// <value>
        /// A <see cref="FilterDefinition{T}"/> object which defines the filter criteria, or null if no filter is defined.
        /// </value>
        internal FilterDefinition<TDocument>? FilterDefinition;


        /// <summary>
        /// Holds an optional Update object for the specified document type.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document to be updated.</typeparam>
        /// <value>
        /// An <see cref="Update{TDocument}"/> instance or <c>null</c> if no update is specified.
        /// </value>
        private Update<TDocument>? _update;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBase"/> class.
        /// </summary>
        /// <param name="filterDefinition">
        /// The filter definition to be applied to the query. 
        /// Must not be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="filterDefinition"/> is null.
        /// </exception>
        internal QueryBase([NotNull] FilterDefinition<TDocument> filterDefinition) => this.FilterDefinition = filterDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBase"/> class.
        /// </summary>
        /// <param name="str">A string containing the JSON query. This parameter cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="str"/> parameter is null.</exception>
        internal QueryBase([NotNull] string str) => this.JsonQuery = str;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBase"/> class.
        /// </summary>
        /// <param name="text">
        /// A string representing the search text to be used in the query.
        /// </param>
        /// <param name="fullTextSearchOptions">
        /// An instance of <see cref="TextSearchOptions"/> 
        /// that contains options for full text search.
        /// </param>
        internal QueryBase(string text, TextSearchOptions fullTextSearchOptions) => this.FullTextSearchOptions = (text, fullTextSearchOptions);

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBase"/> class.
        /// </summary>
        /// <param name="expressionQuery">The expression that defines the query criteria.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="expressionQuery"/> is null.</exception>
        internal QueryBase([NotNull] Expression<Func<TDocument, bool>> expressionQuery)
        {
            var rewriter = new ExpressionVisitorRewriter();

            this.ExpressionQuery = rewriter.Rewrite(expressionQuery);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBase"/> class.
        /// </summary>
        /// <param name="expressionQuery">
        /// A lambda expression that defines a query, used for evaluating conditions
        /// against documents of type <typeparamref name="TDocument"/>.
        /// </param>
        /// <remarks>
        /// This constructor requires a non-null expression query to ensure that
        /// the query can be constructed without encountering null reference issues.
        /// </remarks>
        internal QueryBase([NotNull] Expression<Func<TDocument, TDocument, bool>> expressionQuery)
        {
            var rewriter = new ExpressionVisitorRewriter();

            this.IncompletedExpressionQuery = rewriter.Rewrite(expressionQuery);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBase"/> class with the specified BSON documents.
        /// </summary>
        /// <param name="bsonDocuments">An array of <see cref="BsonDocument"/> objects that represent the BSON documents to be used in the query pipeline.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bsonDocuments"/> is null.</exception>
        internal QueryBase([NotNull] BsonDocument[] bsonDocuments) => this.Pipeline = bsonDocuments;

        internal QueryBase([NotNull] BsonArray bsonArrayPipeline)
        {
            this.Pipeline = bsonArrayPipeline.Select(s=>s.AsBsonDocument).ToArray();
        }
        #endregion Constructors


        /// <summary>
        /// Generates a complete expression from the current incomplete expression query,
        /// replacing any parameters within the expression with a provided constant value.
        /// </summary>
        /// <param name="constrainValue">
        /// The constant value to replace the parameters in the incomplete expression.
        /// </param>
        /// <returns>
        /// An expression that represents a complete query based on the provided constant value.
        /// </returns>
        public Expression<Func<TDocument, bool>> CompleteExpression(TDocument constrainValue)
        {
            // Check if the query has an incomplete expression
            if (this.IncompletedExpressionQuery == null)
            {
                throw new InvalidOperationException("This query does not have incomplete expression.");
            }

            // Replace the incomplete expression with the constant value
            return this.IncompletedExpressionQuery.ReplaceToConstant<Func<TDocument, TDocument, bool>, TDocument, Func<TDocument, bool>>(col => col.Where((ref ParameterExpressionItem x) => QueryBase<TDocument>.ReplaceParam(ref x, constrainValue)));
        }

        /// <summary>
        /// Replaces the parameter's value with a default constant value
        /// if the parameter's index is equal to 1.
        /// </summary>
        /// <param name="arg">A reference to the parameter expression item to be modified.</param>
        /// <param name="defaultValue">The default value to replace the parameter with.</param>
        /// <returns>
        /// Returns true if the parameter was successfully replaced; otherwise, false.
        /// </returns>
        private static bool ReplaceParam(ref ParameterExpressionItem arg, TDocument defaultValue)
        {
            // Check if the parameter has an index of 1
            if (arg.Index == 1)
            {
                // Replace the parameter with the constant value
                arg.Constant(defaultValue);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Renders a filter definition to a BsonDocument.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the document that the filter is based on.
        /// </typeparam>
        /// <param name="filter">
        /// The filter definition to render.
        /// </param>
        /// <returns>
        /// A BsonDocument that represents the rendered filter.
        /// </returns>
        public static BsonDocument RenderToBsonDocument<T>(FilterDefinition<T> filter)
        {
            // Get the serializer registry from BsonSerializer
            var serializerRegistry = BsonSerializer.SerializerRegistry;

            // Get the document serializer for the specified type
            var documentSerializer = serializerRegistry.GetSerializer<T>();

            var renderArgs = new RenderArgs<T>(documentSerializer, serializerRegistry, renderForFind: true);

            // Render the filter using the document serializer and serializer registry
            return filter.Render(renderArgs);
        }

        /// <summary>
        /// Returns a JSON representation of the current filter definition as a BsonDocument.
        /// </summary>
        /// <returns>
        /// A string containing the JSON representation of the filter definition.
        /// </returns>
        public override string ToString()
        {
            // Cast the current object to a FilterDefinition<TDocument>
            var filterDefinition = (FilterDefinition<TDocument>)this;

            // Render the filter definition to a BsonDocument
            var render = RenderToBsonDocument(filterDefinition);

            //var json = filterDefinition.ToBson().ToJson();

            // Return the JSON representation of the rendered BsonDocument
            return render.ToJson();
        }

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>
        /// An integer that represents the hash code for the current instance, 
        /// which is generated by calling the ToString method and then 
        /// retrieving its hash code.
        /// </returns>
        /// <remarks>
        /// This method overrides the default GetHashCode implementation. 
        /// Since it relies on the string representation of the object, 
        /// it may not provide a unique hash code for different instances 
        /// with the same string representation. Ensure that the ToString 
        /// method is properly overridden to provide meaningful output for 
        /// this hash code implementation.
        /// </remarks>
        public override int GetHashCode() => this.ToString().GetHashCode();


        /// <summary>
        /// Gets or sets the update operation for the document of type TDocument.
        /// If the update operation has not been set, a new instance is created.
        /// </summary>
        /// <returns>
        /// Returns the current update operation if it exists; otherwise, a new Update<TDocument> instance.
        /// </returns>
        /// <remarks>
        /// The setter for this property is 
        public Update<TDocument> Update
        {
            // If the update operation has not been set, create a new instance and return it
            get => this._update ??= new Update<TDocument>();

            // Set the update operation to the specified value
            internal set => this._update = value;
        }

        /// <summary>
        /// Overloads the equality operator (==) to compare the specified <see cref="QueryBase{TDocument}"/> instance
        /// with an object for equality.
        /// </summary>
        /// <param name="left">The left operand of the equality comparison, which is a <see cref="QueryBase{TDocument}"/> instance.</param>
        /// <param name="right">The right operand of the equality comparison, which is an object.</param>
        /// <returns>
        /// Returns <c>true</c> if the left operand is equal to the right operand; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This operator calls the <see cref="Equals(object, object)"/> method to determine the equality between
        /// the two operands. It's important to ensure that the <see cref="QueryBase{TDocument}"/> implementation
        /// provides proper equality semantics.
        /// </remarks>
        public static bool operator ==(QueryBase<TDocument> left, object right) => Equals(left, right);

        /// <summary>
        /// Overloads the inequality operator (!=) to compare a <see cref="QueryBase{TDocument}"/> instance with an <see cref="object"/>.
        /// </summary>
        /// <param name="left">The left operand, an instance of <see cref="QueryBase{TDocument}"/> to compare.</param>
        /// <param name="right">The right operand, an <see cref="object"/> to compare against.</param>
        /// <returns>
        /// Returns <c>true</c> if the left operand is not equal to the right operand; otherwise, returns <c>false</c>.
        /// </returns>
        public static bool operator !=(QueryBase<TDocument> left, object right) => !Equals(left, right);

        /// <summary>
        /// Defines the equality operator for comparing an object with a <see cref="QueryBase{TDocument}"/>.
        /// </summary>
        /// <param name="left">The object to compare on the left-hand side of the equality operator.</param>
        /// <param name="right">The <see cref="QueryBase{TDocument}"/> to compare on the right-hand side of the equality operator.</param>
        /// <returns>
        /// Returns <c>true</c> if the <paramref name="left"/> and <paramref name="right"/> are considered equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(object left, QueryBase<TDocument> right) => Equals(right, left);

        /// <summary>
        /// Defines the inequality operator for comparing an object with an instance of the <see cref="QueryBase{TDocument}"/> class.
        /// </summary>
        /// <param name="left">The left-hand side operand of type <see cref="object"/> to compare.</param>
        /// <param name="right">The right-hand side operand of type <see cref="QueryBase{TDocument}"/> to compare.</param>
        /// <returns>
        /// Returns <c>true</c> if the <paramref name="left"/> and <paramref name="right"/> operands are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(object left, QueryBase<TDocument> right) => !Equals(right, left);


        /// <summary>
        /// Compares two objects for equality, where the first object is a nullable 
        /// instance of a type that derives from <see cref="QueryBase{TDocument}"/>. 
        /// The method returns a boolean value indicating whether the two objects are equal.
        /// </summary>
        /// <typeparam name="T">The type of the first object, which must inherit from <see cref="QueryBase{TDocument}"/>.</typeparam>
        /// <param name="left">The first object to compare, which can be null.</param>
        /// <param name="right">The second object to compare, which can also be null.</param>
        /// <returns>
        /// True if the two objects are equal; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This method uses the <see cref="object.Equals(object, object)"/> method 
        /// to perform the comparison, which checks for reference equality 
        /// and then for value equality if the two objects are of the same type.
        /// </remarks>
        public static bool Equals<T>(T? left, object? right) where T : QueryBase<TDocument> => Equals((object)left, right);

        /// <summary>
        /// Determines whether two instances of types derived from <see cref="QueryBase{TDocument}"/> are considered equal.
        /// </summary>
        /// <typeparam name="TL">The type of the left comparison, which must inherit from <see cref="QueryBase{TDocument}"/>.</typeparam>
        /// <typeparam name="TR">The type of the right comparison, which must inherit from <see cref="QueryBase{TDocument}"/>.</typeparam>
        /// <param name="left">The first object to compare, which may be null.</param>
        /// <param name="right">The second object to compare, which may be null.</param>
        /// <returns>
        /// True if both objects are considered equal; otherwise, false. This also considers
        /// both sides being null as equal, and takes into account their hash codes and update states.
        /// </returns>
        public static bool Equals<TL, TR>(TL? left, TR? right)
            where TL : QueryBase<TDocument>
            where TR : QueryBase<TDocument> => Equals((object?)left, right);

        /// <summary>
        /// Determines whether two object instances are equal. This method checks for null references, 
        /// if both instances are the same reference, and compares their hashes and updates if they are 
        /// of the same derived type of <see cref="QueryBase{TDocument}"/>.
        /// </summary>
        /// <param name="left">The first object to compare, which may be null.</param>
        /// <param name="right">The second object to compare, which may be null.</param>
        /// <returns>
        /// True if both objects are considered equal; otherwise, false.
        /// </returns>
        public static bool Equals(object? left, object? right)
        {
            // Both sides are null
            if (left == null && right == null)
            {
                return true;
            }

            // One side is null
            if ((left != null && right == null) || (left == null && right != null))
            {
                return false;
            }

            // Both sides are the same instance
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            // Both sides are the same type
            if (right is QueryBase<TDocument> lrhs && left is QueryBase<TDocument> rlhs)
            {
                return lrhs.GetHashCode() == rlhs.GetHashCode() && lrhs.Update == rlhs.Update;
            }


            return false;
        }

        /// <summary>
        /// Converts a <see cref="QueryBase{TDocument}"/> object into an array of <see cref="BsonDocument"/>.
        /// The method handles various scenarios based on the properties of the query object.
        /// </summary>
        /// <param name="query">An optional query object that may be null. It is of type <see cref="QueryBase{TDocument}"/>.</param>
        /// <returns>
        /// An array of <see cref="BsonDocument"/> resulting from deserialization of the query object.
        /// If the query is null or none of the properties contain valid data, an empty array is returned.
        /// </returns>
        public static implicit operator BsonDocument[]?([MaybeNull] QueryBase<TDocument>? query)
        {
            // If the query is null, return null
            if (query == default)
            {
                return default;
            }

            //if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            //    return BsonSerializer.Deserialize<BsonDocument[]>(query.JsonQuery);

            //return BsonSerializer.Deserialize<BsonDocument[]>(query.ExpressionQuery.ToBson());

            // If the JsonQuery property is not null or whitespace, deserialize it into a BsonDocument array and return it
            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                return BsonSerializer.Deserialize<BsonDocument[]>(query.JsonQuery);
            }

            // If the ExpressionQuery property is not null, deserialize it into a BsonDocument array and return it
            else if (query.ExpressionQuery != null)
            {
                return BsonSerializer.Deserialize<BsonDocument[]>(query.ExpressionQuery.ToBson());
            }

            // If the FullTextSearchOptions property is not null, build a filter using the TextSearchOptions and deserialize it into a BsonDocument array and return it
            else if (query.FullTextSearchOptions != default)
            {
                return BsonSerializer.Deserialize<BsonDocument[]>(Builders<TDocument>.Filter.Text(query.FullTextSearchOptions!.Value.Item1, (TextSearchOptions)query.FullTextSearchOptions!.Value.Item2).ToBson());
            }

            // If the IncompletedExpressionQuery property is not null, throw an InvalidOperationException
            else if (query.IncompletedExpressionQuery != null)
            {
                throw new InvalidOperationException("Fail to convert an expression that requires an constant.");
            }
            // If none of the above conditions are met, return an empty BsonDocument array
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Implicitly converts a <see cref="QueryBase{TDocument}"/> to a <see cref="FilterDefinition{TDocument}"/>.
        /// The conversion results in a <see cref="FilterDefinition{TDocument}"/> based on the properties of the query.
        /// </summary>
        /// <param name="query">The <see cref="QueryBase{TDocument}"/> to convert, can be null.</param>
        /// <returns>
        /// A <see cref="FilterDefinition{TDocument}"/> that represents the converted query.
        /// If the input query is null or does not provide any relevant properties, an empty <see cref="FilterDefinition{TDocument}"/> is returned.
        /// </returns>
        public static implicit operator FilterDefinition<TDocument>([MaybeNull] QueryBase<TDocument>? query)
        {
            // If the query is null, return null
            if (query == default)
            {
                return FilterDefinition<TDocument>.Empty;
            }

            // If the JsonQuery property is not null or whitespace, create a new JsonFilterDefinition object and return it
            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                return new JsonFilterDefinition<TDocument>(query.JsonQuery);
            }
            // If the ExpressionQuery property is not null, create a new ExpressionFilterDefinition object and return it
            else if (query.ExpressionQuery != null)
            {
                return new ExpressionFilterDefinition<TDocument>(query.ExpressionQuery);
            }
            // If the FullTextSearchOptions property is not null, build a filter using the TextSearchOptions and return it
            else if (query.FullTextSearchOptions != default)
            {
                return Builders<TDocument>.Filter.Text(query.FullTextSearchOptions!.Value.Item1, (TextSearchOptions)query.FullTextSearchOptions!.Value.Item2);
            }
            else if (query.FilterDefinition != null)
            {
                return query.FilterDefinition;
            }
            // If the IncompletedExpressionQuery property is not null, throw an InvalidOperationException
            else if (query.IncompletedExpressionQuery != null)
            {
                throw new InvalidOperationException("Fail to convert an expression that requires an constant.");
            }

            // If none of the above conditions are met, return an empty FilterDefinition object
            else
            {
                return FilterDefinition<TDocument>.Empty;
            }
        }
    }
}
