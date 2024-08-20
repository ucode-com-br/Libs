using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using UCode.Extensions;
using UCode.Mongo.Options;
using static UCode.Extensions.ExpressionExtensions;

namespace UCode.Mongo
{
    public abstract record QueryBase<TDocument>
    {
        internal Expression<Func<TDocument, TDocument, bool>>? IncompletedExpressionQuery;
        internal Expression<Func<TDocument, bool>>? ExpressionQuery;
        internal string? JsonQuery;
        internal (string, FullTextSearchOptions<TDocument>)? FullTextSearchOptions;
        internal FilterDefinition<TDocument>? FilterDefinition;

        #region Constructors
        internal QueryBase([NotNull] FilterDefinition<TDocument> filterDefinition) => this.FilterDefinition = filterDefinition;

        internal QueryBase([NotNull] string str) => this.JsonQuery = str;

        internal QueryBase(string text, FullTextSearchOptions<TDocument> fullTextSearchOptions) => this.FullTextSearchOptions = (text, fullTextSearchOptions);

        internal QueryBase([NotNull] Expression<Func<TDocument, bool>> expressionQuery) => this.ExpressionQuery = expressionQuery;

        internal QueryBase([NotNull] Expression<Func<TDocument, TDocument, bool>> expressionQuery) => this.IncompletedExpressionQuery = expressionQuery;
        #endregion Constructors


        /// <summary>
        /// Completes the expression of the query by replacing the incomplete expression with a constant value.
        /// </summary>
        /// <param name="constrainValue">The constant value to replace the incomplete expression with.</param>
        /// <returns>A new Expression object with the completed expression.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the query does not have an incomplete expression.</exception>
        public Expression<Func<TDocument, bool>> CompleteExpression(TDocument constrainValue)
        {
            // Check if the query has an incomplete expression
            if (this.IncompletedExpressionQuery == null)
            {
                throw new InvalidOperationException("This query does not have incomplete expression.");
            }

            // Create a new ParameterExpressionItem object
            ParameterExpressionItem arg = new ParameterExpressionItem();

            // Replace the incomplete expression with the constant value
            return this.IncompletedExpressionQuery.ReplaceToConstant<Func<TDocument, TDocument, bool>, TDocument, Func<TDocument, bool>>(col => col.Where((ref ParameterExpressionItem x) => this.ReplaceParam(ref x, constrainValue)));
        }

        /// <summary>
        /// Replaces the parameter with a constant value if it has an index of 1.
        /// </summary>
        /// <param name="arg">The parameter to replace.</param>
        /// <param name="defaultValue">The constant value to replace the parameter with.</param>
        /// <returns>True if the parameter was replaced, false otherwise.</returns>
        private bool ReplaceParam(ref ParameterExpressionItem arg, TDocument defaultValue)
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
        /// Renders a FilterDefinition to a BsonDocument.
        /// </summary>
        /// <typeparam name="T">The type of the document being filtered.</typeparam>
        /// <param name="filter">The filter to render.</param>
        /// <returns>The rendered BsonDocument.</returns>
        public BsonDocument RenderToBsonDocument<T>(FilterDefinition<T> filter)
        {
            // Get the serializer registry from BsonSerializer
            var serializerRegistry = BsonSerializer.SerializerRegistry;

            // Get the document serializer for the specified type
            var documentSerializer = serializerRegistry.GetSerializer<T>();

            // Render the filter using the document serializer and serializer registry
            return filter.Render(documentSerializer, serializerRegistry);
        }

        /// <summary>
        /// Returns a JSON representation of the filter definition.
        /// </summary>
        /// <returns>The JSON representation of the filter definition.</returns>
        public override string ToString()
        {
            // Cast the current object to a FilterDefinition<TDocument>
            var filterDefinition = (FilterDefinition<TDocument>)this;

            // Render the filter definition to a BsonDocument
            var render = this.RenderToBsonDocument(filterDefinition);

            //var json = filterDefinition.ToBson().ToJson();

            // Return the JSON representation of the rendered BsonDocument
            return render.ToJson();
        }

        /// <summary>
        /// Get hashcode from ToString() method
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => this.ToString().GetHashCode();


        /// <summary>
        /// Represents an update operation on a document in a MongoDB collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document being updated.</typeparam>
        private Update<TDocument> _update;

        /// <summary>
        /// Gets or sets the update operation for the document.
        /// </summary>
        /// <value>
        /// The update operation. If the property has not been set, it will create a new instance of <see cref="Update{TDocument}"/>.
        /// </value>
        public Update<TDocument> Update
        {
            // If the update operation has not been set, create a new instance and return it
            get => this._update ??= new Update<TDocument>();

            // Set the update operation to the specified value
            internal set => this._update = value;
        }


        public static bool operator ==(QueryBase<TDocument> left, object right) => Equals(left, right);
        public static bool operator !=(QueryBase<TDocument> left, object right) => !Equals(left, right);

        public static bool operator ==(object left, QueryBase<TDocument> right) => Equals(right, left);
        public static bool operator !=(object left, QueryBase<TDocument> right) => !Equals(right, left);


        /// <summary>
        /// Compare two QueryBase objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool Equals<T>(T? left, object? right) where T : QueryBase<TDocument> => Equals((object)left, right);/*// Both sides are null
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


            return false;*/

        /// <summary>
        /// Compare two QueryBase objects
        /// </summary>
        /// <typeparam name="TL"></typeparam>
        /// <typeparam name="TR"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool Equals<TL, TR>(TL? left, TR? right)
            where TL : QueryBase<TDocument>
            where TR : QueryBase<TDocument> => Equals((object)left, right);/*// Both sides are null
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


            return false;*/

        /// <summary>
        /// Compare two QueryBase objects
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
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
        /// Implicitly converts a <see cref="QueryBase{TDocument}"/> to a <see cref="BsonDocument"/> array.
        /// </summary>
        /// <param name="query">The <see cref="QueryBase{TDocument}"/> to convert.</param>
        /// <returns>The converted <see cref="BsonDocument"/> array.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the conversion fails due to an incomplete expression.</exception>
        [return: NotNull]
        public static implicit operator BsonDocument[]([NotNull] QueryBase<TDocument> query)
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
                return Array.Empty<BsonDocument>();
            }
        }

        /// <summary>
        /// Implicitly converts a QueryBase object to a FilterDefinition object.
        /// </summary>
        /// <param name="query">The QueryBase object to convert.</param>
        /// <returns>The converted FilterDefinition object.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the conversion fails due to an incomplete expression.</exception>
        [return: NotNull]
        public static implicit operator FilterDefinition<TDocument>([NotNull] QueryBase<TDocument> query)
        {
            // If the query is null, return null
            if (query == default)
            {
                return default;
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
