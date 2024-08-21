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


        public Expression<Func<TDocument, bool>> CompleteExpression(TDocument constrainValue)
        {
            if (this.IncompletedExpressionQuery == null)
            {
                throw new InvalidOperationException("This query does not have incomplete expression.");
            }

            ParameterExpressionItem arg = new ParameterExpressionItem();


            return this.IncompletedExpressionQuery.ReplaceToConstant<Func<TDocument, TDocument, bool>, TDocument, Func<TDocument, bool>>(col => col.Where((ref ParameterExpressionItem x) => this.ReplaceParam(ref x, constrainValue)));
        }

        private bool ReplaceParam(ref ParameterExpressionItem arg, TDocument defaultValue)
        {
            if (arg.Index == 1)
            {
                arg.Constant(defaultValue);
                return true;
            }
            return false;
        }


        public BsonDocument RenderToBsonDocument<T>(FilterDefinition<T> filter)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<T>();
            return filter.Render(documentSerializer, serializerRegistry);
        }

        /// <summary>
        /// To string method
        /// </summary>
        /// <returns>Return json representation of this object</returns>
        public override string ToString()
        {
            var filterDefinition = (FilterDefinition<TDocument>)this;

            var render = this.RenderToBsonDocument(filterDefinition);

            //var json = filterDefinition.ToBson().ToJson();

            return render.ToJson();
        }

        /// <summary>
        /// Get hashcode from ToString() method
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => this.ToString().GetHashCode();




        private Update<TDocument> _update;

        public Update<TDocument> Update
        {
            get => this._update ??= new Update<TDocument>();

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


        [return: NotNull]
        public static implicit operator BsonDocument[]([NotNull] QueryBase<TDocument> query)
        {
            if (query == default)
            {
                return default;
            }

            //if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            //    return BsonSerializer.Deserialize<BsonDocument[]>(query.JsonQuery);

            //return BsonSerializer.Deserialize<BsonDocument[]>(query.ExpressionQuery.ToBson());

            if (!string.IsNullOrWhiteSpace(query.JsonQuery))
            {
                return BsonSerializer.Deserialize<BsonDocument[]>(query.JsonQuery);
            }
            else if (query.ExpressionQuery != null)
            {
                return BsonSerializer.Deserialize<BsonDocument[]>(query.ExpressionQuery.ToBson());
            }
            else if (query.FullTextSearchOptions != default)
            {
                return BsonSerializer.Deserialize<BsonDocument[]>(Builders<TDocument>.Filter.Text(query.FullTextSearchOptions!.Value.Item1, (TextSearchOptions)query.FullTextSearchOptions!.Value.Item2).ToBson());
            }
            else if (query.IncompletedExpressionQuery != null)
            {
                throw new InvalidOperationException("Fail to convert an expression that requires an constant.");
            }
            else
            {
                return Array.Empty<BsonDocument>();
            }
        }

        [return: NotNull]
        public static implicit operator FilterDefinition<TDocument>([NotNull] QueryBase<TDocument> query)
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


    }
}
