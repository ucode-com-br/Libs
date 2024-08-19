using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;

namespace UCode.Mongo
{

    /// <summary>
    /// Represents an update operation for a MongoDB document.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public class Update<TDocument>
    {
        private UpdateDefinition<TDocument> _updateDefinition;

        #region Constructors

        internal Update()
        {
        }

        internal Update(UpdateDefinition<TDocument> updateDefinition) => this._updateDefinition = updateDefinition;

        #endregion Constructors

        public override string ToString()
        {
            var filterDefinition = (UpdateDefinition<TDocument>)this;

            var json = filterDefinition.ToBson().ToJson();

            return json;
        }

        /// <summary>
        /// Hashcode from ToString
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => this.ToString().GetHashCode();

        #region Operator & +
        public static bool operator ==(Update<TDocument> lhs, Update<TDocument> rhs) => lhs.GetHashCode() == rhs.GetHashCode();
        public static bool operator !=(Update<TDocument> lhs, Update<TDocument> rhs) => lhs.GetHashCode() != rhs.GetHashCode();


        /// <summary>
        /// Implements the operator &.
        /// </summary>
        /// <param name="lhs">left query</param>
        /// <param name="rhs">right query</param>
        /// <returns>The result of the operator.</returns>
        public static Update<TDocument> operator &(Update<TDocument> lhs, Update<TDocument> rhs)
        {
            var updated = new UpdateDefinitionBuilder<TDocument>().Combine(lhs, rhs);

            return updated;
        }

        /// <summary>
        /// Implements the operator &.
        /// </summary>
        /// <param name="lhs">left query</param>
        /// <param name="rhs">right query</param>
        /// <returns>The result of the operator.</returns>
        public static Update<TDocument> operator +(Update<TDocument> lhs, Update<TDocument> rhs)
        {
            var updated = new UpdateDefinitionBuilder<TDocument>().Combine(lhs, rhs);

            return updated;
        }


        #endregion Operator & | !


        public static implicit operator Update<TDocument>(string json)
        {
            Update<TDocument> result;


            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    var jsonElements = jsonElement.EnumerateArray().ToArray().Select(s => JsonSerializer.Serialize(s));

                    var pipelineDefinition = PipelineDefinition<TDocument, TDocument>.Create(jsonElements);
                    var pipelineUpdateDefinition = new PipelineUpdateDefinition<TDocument>(pipelineDefinition);
                    //Builders<BsonDocument>.Update.Pipeline(pipelineDefinition);

                    result = new Update<TDocument>
                    {
                        //_updateDefinition = update
                        //_updateDefinition = BsonDocument.Parse(update)
                        //_updateDefinition = new JsonUpdateDefinition<TDocument>(update)
                        _updateDefinition = pipelineUpdateDefinition
                    };
                }
                else
                {
                    result = new Update<TDocument>
                    {
                        _updateDefinition = json
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fail convert json. [Update<TDocument>(string update)] \n\n{json}", ex);
            }

            return result;
        }

        public static implicit operator Update<TDocument>(string[] stages)
        {
            var pipelineDefinition = PipelineDefinition<TDocument, TDocument>.Create(stages);

            var pipelineUpdateDefinition = new PipelineUpdateDefinition<TDocument>(pipelineDefinition);

            return new Update<TDocument>
            {
                _updateDefinition = pipelineUpdateDefinition
            };
        }


        public static implicit operator UpdateDefinition<TDocument>(Update<TDocument> update) => update._updateDefinition;

        public static implicit operator Update<TDocument>(UpdateDefinition<TDocument> update) => new(update);


        #region ExpressionQuery

        public Update<TDocument> Pipeline(string[] stages)
        {
            var pipelineDefinition = PipelineDefinition<TDocument, TDocument>.Create(stages);

            var pipelineUpdateDefinition = new PipelineUpdateDefinition<TDocument>(pipelineDefinition);

            this._updateDefinition = pipelineUpdateDefinition;

            return this;
        }

        public Update<TDocument> AddToSet<TItem>(string fieldName, TItem value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.AddToSet(fieldName, value)
                : this._updateDefinition.AddToSet(fieldName, value);

            return this;
        }

        public Update<TDocument> AddToSet<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.AddToSet(field, value)
                : this._updateDefinition.AddToSet(field, value);

            return this;
        }

        public Update<TDocument> AddToSetEach<TItem>(string fieldName, IEnumerable<TItem> values)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.AddToSetEach(fieldName, values)
                : this._updateDefinition.AddToSetEach(fieldName, values);

            return this;
        }

        public Update<TDocument> AddToSetEach<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field,
            IEnumerable<TItem> values)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.AddToSetEach(field, values)
                : this._updateDefinition.AddToSetEach(field, values);

            return this;
        }

        public Update<TDocument> BitwiseAnd<TField>(string fieldName, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.BitwiseAnd(fieldName, value)
                : this._updateDefinition.BitwiseAnd(fieldName, value);

            return this;
        }

        public Update<TDocument> BitwiseAnd<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.BitwiseAnd(field, value)
                : this._updateDefinition.BitwiseAnd(field, value);

            return this;
        }

        public Update<TDocument> BitwiseOr<TField>(string fieldName, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.BitwiseOr(fieldName, value)
                : this._updateDefinition.BitwiseOr(fieldName, value);

            return this;
        }

        public Update<TDocument> BitwiseOr<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.BitwiseOr(field, value)
                : this._updateDefinition.BitwiseOr(field, value);

            return this;
        }

        public Update<TDocument> BitwiseXor<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.BitwiseXor(field, value)
                : this._updateDefinition.BitwiseXor(field, value);

            return this;
        }

        public Update<TDocument> BitwiseXor<TField>(string fieldName, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.BitwiseXor(fieldName, value)
                : this._updateDefinition.BitwiseXor(fieldName, value);

            return this;
        }

        public Update<TDocument> CurrentDate(string fieldName, UpdateDefinitionCurrentDateType? type = null)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.CurrentDate(fieldName, type)
                : this._updateDefinition.CurrentDate(fieldName, type);

            return this;
        }

        public Update<TDocument> CurrentDate(Expression<Func<TDocument, object>> field, UpdateDefinitionCurrentDateType? type = null)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.CurrentDate(field, type)
                : this._updateDefinition.CurrentDate(field, type);

            return this;
        }

        public Update<TDocument> Inc<TField>(string fieldName, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Inc(fieldName, value)
                : this._updateDefinition.Inc(fieldName, value);

            return this;
        }

        public Update<TDocument> Inc<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Inc(field, value)
                : this._updateDefinition.Inc(field, value);

            return this;
        }

        public Update<TDocument> Max<TField>(string fieldName, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Max(fieldName, value)
                : this._updateDefinition.Max(fieldName, value);

            return this;
        }

        public Update<TDocument> Max<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Max(field, value)
                : this._updateDefinition.Max(field, value);

            return this;
        }

        public Update<TDocument> Min<TField>(string fieldName, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Min(fieldName, value)
                : this._updateDefinition.Min(fieldName, value);

            return this;
        }

        public Update<TDocument> Min<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Min(field, value)
                : this._updateDefinition.Min(field, value);

            return this;
        }

        public Update<TDocument> Mul<TField>(string fieldName, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Mul(fieldName, value)
                : this._updateDefinition.Mul(fieldName, value);

            return this;
        }

        public Update<TDocument> Mul<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Mul(field, value)
                : this._updateDefinition.Mul(field, value);

            return this;
        }

        public Update<TDocument> PopFirst(Expression<Func<TDocument, object>> field)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PopFirst(field)
                : this._updateDefinition.PopFirst(field);

            return this;
        }

        public Update<TDocument> PopFirst(string fieldName)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PopFirst(fieldName)
                : this._updateDefinition.PopFirst(fieldName);

            return this;
        }

        public Update<TDocument> PopLast(string fieldName)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PopLast(fieldName)
                : this._updateDefinition.PopLast(fieldName);

            return this;
        }

        public Update<TDocument> PopLast(Expression<Func<TDocument, object>> field)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PopLast(field)
                : this._updateDefinition.PopLast(field);

            return this;
        }

        public Update<TDocument> Pull<TItem>(string fieldName, TItem value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Pull(fieldName, value)
                : this._updateDefinition.Pull(fieldName, value);

            return this;
        }

        public Update<TDocument> Pull<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Pull(field, value)
                : this._updateDefinition.Pull(field, value);

            return this;
        }

        public Update<TDocument> PullAll<TItem>(string fieldName, IEnumerable<TItem> values)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Pull(fieldName, values)
                : this._updateDefinition.Pull(fieldName, values);

            return this;
        }

        public Update<TDocument> PullAll<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, IEnumerable<TItem> values)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PullAll(field, values)
                : this._updateDefinition.PullAll(field, values);

            return this;
        }

        public Update<TDocument> PullFilter<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field,
            Expression<Func<TItem, bool>> filter)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PullFilter(field, filter)
                : this._updateDefinition.PullFilter(field, filter);

            return this;
        }

        public Update<TDocument> PullFilter<TItem>(string fieldName, FilterDefinition<TItem> filter)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PullFilter(fieldName, filter)
                : this._updateDefinition.PullFilter(fieldName, filter);

            return this;
        }

        public Update<TDocument> PullFilter<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field,
            FilterDefinition<TItem> filter)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PullFilter(field, filter)
                : this._updateDefinition.PullFilter(field, filter);

            return this;
        }

        public Update<TDocument> Push<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Push(field, value)
                : this._updateDefinition.Push(field, value);

            return this;
        }

        public Update<TDocument> Push<TItem>(string fieldName, TItem value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Push(fieldName, value)
                : this._updateDefinition.Push(fieldName, value);

            return this;
        }

        public Update<TDocument> PushEach<TItem>(string fieldName, IEnumerable<TItem> values, int? slice = null,
            int? position = null, SortDefinition<TItem> sort = null)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PushEach(fieldName, values, slice, position, sort)
                : this._updateDefinition.PushEach(fieldName, values, slice, position, sort);

            return this;
        }

        public Update<TDocument> PushEach<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, IEnumerable<TItem> values,
            int? slice = null, int? position = null, SortDefinition<TItem> sort = null)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PushEach(field, values, slice, position, sort)
                : this._updateDefinition.PushEach(field, values, slice, position, sort);

            return this;
        }

        public Update<TDocument> Rename(string fieldName, string newName)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Rename(fieldName, newName)
                : this._updateDefinition.Rename(fieldName, newName);

            return this;
        }

        public Update<TDocument> Rename(Expression<Func<TDocument, object>> field, string newName)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Rename(field, newName)
                : this._updateDefinition.Rename(field, newName);

            return this;
        }

        public Update<TDocument> Set<TField>(string fieldName, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Set(fieldName, value)
                : this._updateDefinition.Set(fieldName, value);

            return this;
        }

        public Update<TDocument> Set<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Set(field, value)
                : this._updateDefinition.Set(field, value);

            return this;
        }

        public Update<TDocument> SetOnInsert<TField>(string fieldName, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.SetOnInsert(fieldName, value)
                : this._updateDefinition.SetOnInsert(fieldName, value);

            return this;
        }

        public Update<TDocument> SetOnInsert<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.SetOnInsert(field, value)
                : this._updateDefinition.SetOnInsert(field, value);

            return this;
        }

        public Update<TDocument> Unset(string fieldName)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Unset(fieldName)
                : this._updateDefinition.Unset(fieldName);

            return this;
        }

        public Update<TDocument> Unset(Expression<Func<TDocument, object>> field)
        {
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Unset(field)
                : this._updateDefinition.Unset(field);

            return this;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            throw new NotImplementedException();
        }

        #endregion
    }
}
