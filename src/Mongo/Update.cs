using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Xml.Linq;
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
        /// <summary>
        /// Represents the update definition for a document of type TDocument.
        /// </summary>
        private UpdateDefinition<TDocument> _updateDefinition;

        #region Constructors

        internal Update()
        {
        }

        internal Update(UpdateDefinition<TDocument> updateDefinition) => this._updateDefinition = updateDefinition;

        #endregion Constructors

        /// <summary>
        /// Returns a JSON representation of the update definition.
        /// </summary>
        /// <returns>The JSON representation of the update definition.</returns>
        public override string ToString()
        {
            // Cast the current object to an UpdateDefinition<TDocument>
            var filterDefinition = (UpdateDefinition<TDocument>)this;

            // Render the update definition to a BsonDocument
            var json = filterDefinition.ToBson().ToJson();

            // Return the JSON representation of the rendered BsonDocument
            return json;
        }

        /// <summary>
        /// Hashcode from ToString
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => this.ToString().GetHashCode();

        #region Operator == != & +
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator == (Update<TDocument>? lhs, Update<TDocument>? rhs)
        {
            if (ReferenceEquals(lhs, rhs) || lhs!.Equals(rhs))
            {
                return true;
            }

            return lhs!.GetHashCode() == rhs!.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator != (Update<TDocument>? lhs, Update<TDocument>? rhs) => !(lhs == rhs);


        /// <summary>
        /// Implements the operator & for combining two Update objects.
        /// </summary>
        /// <param name="lhs">left query</param>
        /// <param name="rhs">right query</param>
        /// <returns>The result of the operator.</returns>
        public static Update<TDocument> operator &(Update<TDocument> lhs, Update<TDocument> rhs)
        {
            ArgumentNullException.ThrowIfNull(lhs);
            ArgumentNullException.ThrowIfNull(rhs);

            var updated = new UpdateDefinitionBuilder<TDocument>().Combine(lhs, rhs);

            return updated;
        }

        /// <summary>
        /// Implements the operator + for combining two Update objects.
        /// </summary>
        /// <param name="lhs">left query</param>
        /// <param name="rhs">right query</param>
        /// <returns>The result of the operator.</returns>
        public static Update<TDocument> operator +(Update<TDocument> lhs, Update<TDocument> rhs)
        {
            ArgumentNullException.ThrowIfNull(lhs);
            ArgumentNullException.ThrowIfNull(rhs);

            var updated = new UpdateDefinitionBuilder<TDocument>().Combine(lhs, rhs);

            return updated;
        }


        #endregion Operator & | !

        /// <summary>
        /// Implicitly converts a JSON string to an Update object of type TDocument.
        /// </summary>
        /// <param name="json">The JSON string to convert.</param>
        /// <returns>An Update object of type TDocument.</returns>
        /// <exception cref="Exception">Thrown if the JSON string cannot be converted to an Update object.</exception>
        public static implicit operator Update<TDocument>(string json)
        {
            Update<TDocument> result;


            try
            {
                // Deserialize the JSON string into a JsonElement
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

                // Check if the JSON string is an array
                if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    // Serialize each element of the array into a JSON string
                    var jsonElements = jsonElement.EnumerateArray().ToArray().Select(s => JsonSerializer.Serialize(s));

                    // Create a pipeline definition from the JSON strings
                    var pipelineDefinition = PipelineDefinition<TDocument, TDocument>.Create(jsonElements);

                    // Create a pipeline update definition from the pipeline definition
                    var pipelineUpdateDefinition = new PipelineUpdateDefinition<TDocument>(pipelineDefinition);
                    //Builders<BsonDocument>.Update.Pipeline(pipelineDefinition);

                    // Create an Update object with the pipeline update definition
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
                    // Create an Update object with the JSON string
                    result = new Update<TDocument>
                    {
                        _updateDefinition = json
                    };
                }
            }
            catch (Exception ex)
            {
                // Throw an exception if the JSON string cannot be converted to an Update object
                throw new Exception($"Fail convert json. [Update<TDocument>(string update)] \n\n{json}", ex);
            }

            return result;
        }

        /// <summary>
        /// Implicitly converts a string array into an Update object.
        /// </summary>
        /// <param name="stages">The stages of the pipeline.</param>
        /// <returns>An Update object representing the pipeline.</returns>
        public static implicit operator Update<TDocument>(string[] stages)
        {
            // Create a pipeline definition from the stages
            var pipelineDefinition = PipelineDefinition<TDocument, TDocument>.Create(stages);

            // Create a pipeline update definition from the pipeline definition
            var pipelineUpdateDefinition = new PipelineUpdateDefinition<TDocument>(pipelineDefinition);

            // Create a new Update object with the pipeline update definition
            return new Update<TDocument>
            {
                _updateDefinition = pipelineUpdateDefinition
            };
        }

        /// <summary>
        /// Implicitly converts an Update object to an UpdateDefinition object.
        /// </summary>
        /// <param name="update">The Update object to convert.</param>
        /// <returns>An UpdateDefinition object representing the Update object.</returns>
        public static implicit operator UpdateDefinition<TDocument>(Update<TDocument> update) => update._updateDefinition;

        /// <summary>
        /// Implicitly converts an UpdateDefinition object to an Update object.
        /// </summary>
        /// <param name="update">The UpdateDefinition object to convert.</param>
        /// <returns>An Update object representing the UpdateDefinition object.</returns>
        public static implicit operator Update<TDocument>(UpdateDefinition<TDocument> update) => new(update);


        #region ExpressionQuery

        /// <summary>
        /// Creates a pipeline update definition from the given stages and sets it as the update definition for this Update object.
        /// </summary>
        /// <param name="stages">The stages of the pipeline.</param>
        /// <returns>This Update object with the pipeline update definition set.</returns>
        public Update<TDocument> Pipeline(string[] stages)
        {
            // Create a pipeline definition from the given stages
            var pipelineDefinition = PipelineDefinition<TDocument, TDocument>.Create(stages);

            // Create a pipeline update definition from the pipeline definition
            var pipelineUpdateDefinition = new PipelineUpdateDefinition<TDocument>(pipelineDefinition);

            // Set the pipeline update definition as the update definition for this Update object
            this._updateDefinition = pipelineUpdateDefinition;

            // Return this Update object
            return this;
        }

        /// <summary>
        /// Adds a value to a set field in the document.
        /// If the field does not exist, it is created.
        /// If the field is not of type Set, it is converted to a Set type.
        /// If the value already exists in the set, it is not added again.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the set.</typeparam>
        /// <param name="fieldName">The name of the field to add the value to.</param>
        /// <param name="value">The value to add to the set.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> AddToSet<TItem>(string fieldName, TItem value)
        {
            // If the update definition is null, create a new AddToSet update definition
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.AddToSet(fieldName, value)
                : this._updateDefinition.AddToSet(fieldName, value);

            return this;
        }

        /// <summary>
        /// Adds a value to a set field in the document.
        /// If the field does not exist, it is created.
        /// If the field is not of type Set, it is converted to a Set type.
        /// If the value already exists in the set, it is not added again.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the set.</typeparam>
        /// <param name="field">An expression representing the field to add the value to.</param>
        /// <param name="value">The value to add to the set.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> AddToSet<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem value)
        {
            // If the update definition is null, create a new one using the field and value
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.AddToSet(field, value)
                : this._updateDefinition.AddToSet(field, value);

            // Return the updated Update object
            return this;
        }

        /// <summary>
        /// Adds multiple values to a set field in the document.
        /// If the field does not exist, it is created.
        /// If the field is not of type Set, it is converted to a Set type.
        /// If any of the values already exist in the set, they are not added again.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the set.</typeparam>
        /// <param name="fieldName">The name of the field to add the values to.</param>
        /// <param name="values">The values to add to the set.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> AddToSetEach<TItem>(string fieldName, IEnumerable<TItem> values)
        {
            // If the update definition is null, create a new AddToSetEach update definition
            // Otherwise, add the values to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.AddToSetEach(fieldName, values)
                : this._updateDefinition.AddToSetEach(fieldName, values);

            return this;
        }

        /// <summary>
        /// Adds multiple values to a set field in the document.
        /// If the field does not exist, it is created.
        /// If the field is not of type Set, it is converted to a Set type.
        /// If any of the values already exist in the set, they are not added again.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the set.</typeparam>
        /// <param name="field">An expression representing the field to add the values to.</param>
        /// <param name="values">The values to add to the set.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> AddToSetEach<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field,
            IEnumerable<TItem> values)
        {
            // If the update definition is null, create a new one using the field and values
            // Otherwise, add the values to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.AddToSetEach(field, values)
                : this._updateDefinition.AddToSetEach(field, values);

            // Return the updated Update object
            return this;
        }

        /// <summary>
        /// Sets the value of a bitwise AND operation for a field in the document.
        /// If the field does not exist, it is created.
        /// If the field is not of type Integer, it is converted to an Integer type.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="fieldName">The name of the field to perform the bitwise AND operation on.</param>
        /// <param name="value">The value to perform the bitwise AND operation with.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> BitwiseAnd<TField>(string fieldName, TField value)
        {
            // If the update definition is null, create a new BitwiseAnd update definition
            // Otherwise, add the bitwise AND operation to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.BitwiseAnd(fieldName, value)
                : this._updateDefinition.BitwiseAnd(fieldName, value);

            return this;
        }

        /// <summary>
        /// Sets the value of a bitwise AND operation for a field in the document.
        /// If the field does not exist, it is created.
        /// If the field is not of type Integer, it is converted to an Integer type.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">An expression representing the field to perform the bitwise AND operation on.</param>
        /// <param name="value">The value to perform the bitwise AND operation with.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> BitwiseAnd<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            // If the update definition is null, create a new one using the field and value
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.BitwiseAnd(field, value)
                : this._updateDefinition.BitwiseAnd(field, value);

            return this;
        }

        /// <summary>
        /// Sets the value of a bitwise OR operation for a field in the document.
        /// If the field does not exist, it is created.
        /// If the field is not of type Integer, it is converted to an Integer type.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="fieldName">The name of the field to perform the bitwise OR operation on.</param>
        /// <param name="value">The value to perform the bitwise OR operation with.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> BitwiseOr<TField>(string fieldName, TField value)
        {
            // If the update definition is null, create a new BitwiseOr update definition
            // Otherwise, add the bitwise OR operation to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.BitwiseOr(fieldName, value)
                : this._updateDefinition.BitwiseOr(fieldName, value);

            return this;
        }

        /// <summary>
        /// Sets the value of a bitwise OR operation for a field in the document.
        /// If the field does not exist, it is created.
        /// If the field is not of type Integer, it is converted to an Integer type.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">An expression representing the field to perform the bitwise OR operation on.</param>
        /// <param name="value">The value to perform the bitwise OR operation with.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> BitwiseOr<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            // If the update definition is null, create a new one using the field and value
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.BitwiseOr(field, value)
                : this._updateDefinition.BitwiseOr(field, value);

            return this;
        }

        /// <summary>
        /// Sets the value of a bitwise XOR operation for a field in the document.
        /// If the field does not exist, it is created.
        /// If the field is not of type Integer, it is converted to an Integer type.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">An expression representing the field to perform the bitwise XOR operation on.</param>
        /// <param name="value">The value to perform the bitwise XOR operation with.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> BitwiseXor<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            // If the update definition is null, create a new BitwiseXor update definition
            // Otherwise, add the bitwise XOR operation to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.BitwiseXor(field, value)
                : this._updateDefinition.BitwiseXor(field, value);

            return this;
        }

        /// <summary>
        /// Updates the value of a bitwise XOR operation for a field in the document.
        /// If the field does not exist, it is created.
        /// If the field is not of type Integer, it is converted to an Integer type.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="fieldName">The name of the field to perform the bitwise XOR operation on.</param>
        /// <param name="value">The value to perform the bitwise XOR operation with.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> BitwiseXor<TField>(string fieldName, TField value)
        {
            // If the update definition is null, create a new one using the field and value
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.BitwiseXor(fieldName, value)
                : this._updateDefinition.BitwiseXor(fieldName, value);

            return this;
        }

        /// <summary>
        /// Sets the value of the current date and time for a field in the document.
        /// If the field does not exist, it is created.
        /// </summary>
        /// <param name="fieldName">The name of the field to set the current date and time for.</param>
        /// <param name="type">The type of the current date and time. Default is null.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> CurrentDate(string fieldName, UpdateDefinitionCurrentDateType? type = null)
        {
            // If the update definition is null, create a new one using the field and type
            // Otherwise, add the field and type to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.CurrentDate(fieldName, type)
                : this._updateDefinition.CurrentDate(fieldName, type);

            return this;
        }

        /// <summary>
        /// Sets the value of the current date and time for a field in the document.
        /// If the field does not exist, it is created.
        /// </summary>
        /// <param name="field">An expression representing the field to set the current date and time for.</param>
        /// <param name="type">The type of the current date and time. Default is null.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> CurrentDate(Expression<Func<TDocument, object>> field, UpdateDefinitionCurrentDateType? type = null)
        {
            // If the update definition is null, create a new one using the field and value
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.CurrentDate(field, type)
                : this._updateDefinition.CurrentDate(field, type);

            return this;
        }

        /// <summary>
        /// Increments the value of a field in the document by a specified amount.
        /// If the field does not exist, it is created.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="fieldName">The name of the field to increment.</param>
        /// <param name="value">The amount to increment the field by.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Inc<TField>(string fieldName, TField value)
        {
            // If the update definition is null, create a new one using the field and value
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Inc(fieldName, value)
                : this._updateDefinition.Inc(fieldName, value);

            return this;
        }

        /// <summary>
        /// Increments the value of a field in the document by a specified amount.
        /// If the field does not exist, it is created.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">An expression representing the field to increment.</param>
        /// <param name="value">The amount to increment the field by.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Inc<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            // If the update definition is null, create a new one using the field and value
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Inc(field, value)
                : this._updateDefinition.Inc(field, value);

            return this;
        }

        /// <summary>
        /// Sets the maximum value of a field in the document.
        /// If the field does not exist, it is created.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="fieldName">The name of the field to set the maximum value for.</param>
        /// <param name="value">The maximum value to set.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Max<TField>(string fieldName, TField value)
        {
            // If the update definition is null, create a new one using the field and value
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Max(fieldName, value)
                : this._updateDefinition.Max(fieldName, value);

            return this;
        }

        /// <summary>
        /// Sets the maximum value of a field in the document.
        /// If the field does not exist, it is created.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">An expression representing the field to set the maximum value for.</param>
        /// <param name="value">The maximum value to set.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Max<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            // If the update definition is null, create a new one using the field and value
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Max(field, value)
                : this._updateDefinition.Max(field, value);

            return this;
        }

        /// <summary>
        /// Sets the minimum value of a field in the document.
        /// If the field does not exist, it is created.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="fieldName">The name of the field to set the minimum value for.</param>
        /// <param name="value">The minimum value to set.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Min<TField>(string fieldName, TField value)
        {
            // If the update definition is null, create a new Min update definition
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Min(fieldName, value)
                : this._updateDefinition.Min(fieldName, value);

            return this;
        }

        /// <summary>
        /// Sets the minimum value of a field in the document.
        /// If the field does not exist, it is created.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">An expression representing the field to set the minimum value for.</param>
        /// <param name="value">The minimum value to set.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Min<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            // If the update definition is null, create a new one using the field and value
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Min(field, value)
                : this._updateDefinition.Min(field, value);

            return this;
        }

        /// <summary>
        /// Sets the value of a multiplication operation for a field in the document.
        /// If the field does not exist, it is created.
        /// If the field is not of type Integer, it is converted to an Integer type.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="fieldName">The name of the field to perform the multiplication operation on.</param>
        /// <param name="value">The value to perform the multiplication operation with.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Mul<TField>(string fieldName, TField value)
        {
            // If the update definition is null, create a new Mul update definition
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Mul(fieldName, value)
                : this._updateDefinition.Mul(fieldName, value);

            return this;
        }

        /// <summary>
        /// Sets the value of a multiplication operation for a field in the document.
        /// If the field does not exist, it is created.
        /// If the field is not of type Integer, it is converted to an Integer type.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">An expression representing the field to perform the multiplication operation on.</param>
        /// <param name="value">The value to perform the multiplication operation with.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Mul<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            // If the update definition is null, create a new one using the field and value
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Mul(field, value)
                : this._updateDefinition.Mul(field, value);

            return this;
        }

        /// <summary>
        /// Removes the first element from an array field in the document.
        /// If the field does not exist, this operation has no effect.
        /// If the field is not of type array, this operation has no effect.
        /// </summary>
        /// <param name="field">An expression representing the field to remove the first element from.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> PopFirst(Expression<Func<TDocument, object>> field)
        {
            // If the update definition is null, create a new PopFirst update definition
            // Otherwise, add the field to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PopFirst(field)
                : this._updateDefinition.PopFirst(field);

            return this;
        }

        /// <summary>
        /// Removes the first element from an array field in the document.
        /// If the field does not exist, this operation has no effect.
        /// If the field is not of type array, this operation has no effect.
        /// </summary>
        /// <param name="fieldName">The name of the field to remove the first element from.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> PopFirst(string fieldName)
        {
            // If the update definition is null, create a new PopFirst update definition
            // Otherwise, add the field name to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PopFirst(fieldName)
                : this._updateDefinition.PopFirst(fieldName);

            return this;
        }

        /// <summary>
        /// Removes the last element from an array field in the document.
        /// If the field does not exist, this operation has no effect.
        /// If the field is not of type array, this operation has no effect.
        /// </summary>
        /// <param name="fieldName">The name of the field to remove the last element from.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> PopLast(string fieldName)
        {
            // If the update definition is null, create a new PopLast update definition
            // Otherwise, add the field to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PopLast(fieldName)
                : this._updateDefinition.PopLast(fieldName);

            return this;
        }

        /// <summary>
        /// Removes the last element from an array field in the document.
        /// If the field does not exist, this operation has no effect.
        /// If the field is not of type array, this operation has no effect.
        /// </summary>
        /// <param name="field">An expression representing the field to remove the last element from.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> PopLast(Expression<Func<TDocument, object>> field)
        {
            // If the update definition is null, create a new PopLast update definition
            // Otherwise, add the field to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PopLast(field)
                : this._updateDefinition.PopLast(field);

            return this;
        }

        /// <summary>
        /// Removes all occurrences of a value from an array field in the document.
        /// If the field does not exist, this operation has no effect.
        /// If the field is not of type array, this operation has no effect.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the array.</typeparam>
        /// <param name="fieldName">The name of the field to remove the value from.</param>
        /// <param name="value">The value to remove from the array.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Pull<TItem>(string fieldName, TItem value)
        {
            // If the update definition is null, create a new Pull update definition
            // Otherwise, add the field name and value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Pull(fieldName, value)
                : this._updateDefinition.Pull(fieldName, value);

            return this;
        }

        /// <summary>
        /// Removes all occurrences of a value from an array field in the document.
        /// If the field does not exist, this operation has no effect.
        /// If the field is not of type array, this operation has no effect.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the array.</typeparam>
        /// <param name="field">An expression representing the field to remove the value from.</param>
        /// <param name="value">The value to remove from the array.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Pull<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem value)
        {
            // If the update definition is null, create a new Pull update definition
            // Otherwise, add the field and value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Pull(field, value)
                : this._updateDefinition.Pull(field, value);

            return this;
        }

        /// <summary>
        /// Removes all occurrences of a value from an array field in the document.
        /// If the field does not exist, this operation has no effect.
        /// If the field is not of type array, this operation has no effect.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the array.</typeparam>
        /// <param name="fieldName">The name of the field to remove the value from.</param>
        /// <param name="values">The values to remove from the array.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> PullAll<TItem>(string fieldName, IEnumerable<TItem> values)
        {
            // If the update definition is null, create a new Pull update definition
            // Otherwise, add the field name and values to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Pull(fieldName, values)
                : this._updateDefinition.Pull(fieldName, values);

            return this;
        }

        /// <summary>
        /// Removes all occurrences of a value from an array field in the document.
        /// If the field does not exist, this operation has no effect.
        /// If the field is not of type array, this operation has no effect.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the array.</typeparam>
        /// <param name="field">An expression representing the field to remove the value from.</param>
        /// <param name="values">The values to remove from the array.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> PullAll<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, IEnumerable<TItem> values)
        {
            // If the update definition is null, create a new PullAll update definition
            // Otherwise, add the field and values to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PullAll(field, values)
                : this._updateDefinition.PullAll(field, values);

            return this;
        }

        /// <summary>
        /// Removes all elements from an array field that match the specified filter.
        /// If the field does not exist, this operation has no effect.
        /// If the field is not of type array, this operation has no effect.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the array.</typeparam>
        /// <param name="field">An expression representing the field to remove elements from.</param>
        /// <param name="filter">A filter to apply to the elements in the array.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> PullFilter<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field,
            Expression<Func<TItem, bool>> filter)
        {
            // If the update definition is null, create a new PullFilter update definition
            // Otherwise, add the field and filter to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PullFilter(field, filter)
                : this._updateDefinition.PullFilter(field, filter);

            return this;
        }

        /// <summary>
        /// Adds a filter to the update definition that removes all array elements matching the given filter.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the array.</typeparam>
        /// <param name="fieldName">The name of the field to be updated.</param>
        /// <param name="filter">The filter to match the elements to be removed.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> PullFilter<TItem>(string fieldName, FilterDefinition<TItem> filter)
        {
            // If the update definition is null, create a new one with the pull filter.
            // Otherwise, add the pull filter to the existing update definition.
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PullFilter(fieldName, filter)
                : this._updateDefinition.PullFilter(fieldName, filter);

            return this;
        }

        /// <summary>
        /// This method is used to remove items from an array field in a document that match a specified filter.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the items in the array field.</typeparam>
        /// <param name="field">The field to pull items from.</param>
        /// <param name="filter">The filter to apply to the items in the array field.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> PullFilter<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field,
            FilterDefinition<TItem> filter)
        {
            // If the update definition is null, create a new one using the Builders class.
            // Otherwise, add the pull filter to the existing update definition.
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PullFilter(field, filter)
                : this._updateDefinition.PullFilter(field, filter);

            // Return the updated Update object.
            return this;
        }

        /// <summary>
        /// Adds a value to an array field in a document.
        /// If the field does not exist, it is created.
        /// If the field is not of type array, it is converted to an array type.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the array.</typeparam>
        /// <param name="field">An expression representing the field to add the value to.</param>
        /// <param name="value">The value to add to the array.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Push<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, TItem value)
        {
            // If the update definition is null, create a new Push update definition
            // Otherwise, add the field and value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Push(field, value)
                : this._updateDefinition.Push(field, value);

            return this;
        }

        /// <summary>
        /// Adds a value to an array field in a document.
        /// If the field does not exist, it is created.
        /// If the field is not of type array, it is converted to an array type.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the array.</typeparam>
        /// <param name="fieldName">The name of the field to add the value to.</param>
        /// <param name="value">The value to add to the array.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Push<TItem>(string fieldName, TItem value)
        {
            // If the update definition is null, create a new one using the field name and value
            // Otherwise, add the value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Push(fieldName, value)
                : this._updateDefinition.Push(fieldName, value);

            return this;
        }

        /// <summary>
        /// Adds multiple values to an array field in a document.
        /// If the field does not exist, it is created.
        /// If the field is not of type array, it is converted to an array type.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the array.</typeparam>
        /// <param name="fieldName">The name of the field to add the values to.</param>
        /// <param name="values">The values to add to the array.</param>
        /// <param name="slice">The maximum number of elements to retain in the array after the push operation. Default is null.</param>
        /// <param name="position">The position in the array to insert the values. Default is null.</param>
        /// <param name="sort">The sort order for the elements in the array. Default is null.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> PushEach<TItem>(string fieldName, IEnumerable<TItem> values, int? slice = null,
            int? position = null, SortDefinition<TItem> sort = null)
        {
            // If the update definition is null, create a new PushEach update definition
            // Otherwise, add the field and values to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PushEach(fieldName, values, slice, position, sort)
                : this._updateDefinition.PushEach(fieldName, values, slice, position, sort);

            return this;
        }

        /// <summary>
        /// Adds multiple values to an array field in a document.
        /// If the field does not exist, it is created.
        /// If the field is not of type array, it is converted to an array type.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the array.</typeparam>
        /// <param name="field">An expression representing the field to add the values to.</param>
        /// <param name="values">The values to add to the array.</param>
        /// <param name="slice">The maximum number of elements to retain in the array after the push operation. Default is null.</param>
        /// <param name="position">The position in the array to insert the values. Default is null.</param>
        /// <param name="sort">The sort order for the elements in the array. Default is null.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> PushEach<TItem>(Expression<Func<TDocument, IEnumerable<TItem>>> field, IEnumerable<TItem> values,
            int? slice = null, int? position = null, SortDefinition<TItem> sort = null)
        {
            // If the update definition is null, create a new PushEach update definition
            // Otherwise, add the values to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.PushEach(field, values, slice, position, sort)
                : this._updateDefinition.PushEach(field, values, slice, position, sort);

            return this;
        }

        /// <summary>
        /// Renames a field in the document.
        /// If the field does not exist, this operation has no effect.
        /// </summary>
        /// <param name="fieldName">The name of the field to rename.</param>
        /// <param name="newName">The new name of the field.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Rename(string fieldName, string newName)
        {
            // If the update definition is null, create a new Rename update definition
            // Otherwise, add the field and new name to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Rename(fieldName, newName)
                : this._updateDefinition.Rename(fieldName, newName);

            return this;
        }

        /// <summary>
        /// Renames a field in the document.
        /// If the field does not exist, this operation has no effect.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="field">An expression representing the field to rename.</param>
        /// <param name="newName">The new name of the field.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Rename(Expression<Func<TDocument, object>> field, string newName)
        {
            // If the update definition is null, create a new Rename update definition
            // Otherwise, add the field and new name to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Rename(field, newName)
                : this._updateDefinition.Rename(field, newName);

            return this;
        }

        /// <summary>
        /// Sets the value of a field in the document.
        /// If the field does not exist, it is created.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="fieldName">The name of the field to set the value for.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Set<TField>(string fieldName, TField value)
        {
            // If the update definition is null, create a new Set update definition
            // Otherwise, add the field and value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Set(fieldName, value)
                : this._updateDefinition.Set(fieldName, value);

            return this;
        }

        /// <summary>
        /// Sets the value of a field in the document.
        /// If the field does not exist, it is created.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">An expression representing the field to set the value for.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Set<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            // If the update definition is null, create a new one using the field and value
            // Otherwise, add the field and value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Set(field, value)
                : this._updateDefinition.Set(field, value);

            return this;
        }

        /// <summary>
        /// Sets the value of a field in the document if the document is being inserted.
        /// If the field does not exist, it is created.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="fieldName">The name of the field to set the value for.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> SetOnInsert<TField>(string fieldName, TField value)
        {
            // If the update definition is null, create a new SetOnInsert update definition
            // Otherwise, add the field and value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.SetOnInsert(fieldName, value)
                : this._updateDefinition.SetOnInsert(fieldName, value);

            return this;
        }

        /// <summary>
        /// Sets the value of a field in the document if the document is being inserted.
        /// If the field does not exist, it is created.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">An expression representing the field to set the value for.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> SetOnInsert<TField>(Expression<Func<TDocument, TField>> field, TField value)
        {
            // If the update definition is null, create a new one using the field and value
            // Otherwise, add the field and value to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.SetOnInsert(field, value)
                : this._updateDefinition.SetOnInsert(field, value);

            return this;
        }

        /// <summary>
        /// Removes a field from the document.
        /// If the field does not exist, this operation has no effect.
        /// </summary>
        /// <param name="fieldName">The name of the field to remove.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Unset(string fieldName)
        {
            // If the update definition is null, create a new Unset update definition
            // Otherwise, add the field to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Unset(fieldName)
                : this._updateDefinition.Unset(fieldName);

            return this;
        }

        /// <summary>
        /// Removes a field from the document.
        /// If the field does not exist, this operation has no effect.
        /// </summary>
        /// <param name="field">An expression representing the field to remove.</param>
        /// <returns>The updated Update object.</returns>
        public Update<TDocument> Unset(Expression<Func<TDocument, object>> field)
        {
            // If the update definition is null, create a new Unset update definition
            // Otherwise, add the field to the existing update definition
            this._updateDefinition = this._updateDefinition == null
                ? Builders<TDocument>.Update.Unset(field)
                : this._updateDefinition.Unset(field);

            return this;
        }

        /// <summary>
        /// Determines whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the objects are equal, false otherwise.</returns>
        public override bool Equals(object? obj)
        {
            // Check if the current object is the same as the object being compared
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            // Check if the object being compared is null
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            return this.GetHashCode() == obj.GetHashCode();
        }


        /// <summary>
        /// Set fields values in document. (recursive)
        /// </summary>
        /// <param name="bsonDocument">The bson document.</param>
        /// <param name="updateNullValues">if set to <c>true</c> [update null values].</param>
        /// <param name="ignoreElement">The ignore BsonElement.</param>
        /// <returns>The Update object.</returns>
        public Update<TDocument> Set(BsonDocument bsonDocument, bool updateNullValues = true, Func<BsonElement, bool>? ignoreElement = null)
        {
            var list = BuildNestedUpdate("", bsonDocument, updateNullValues, ignoreElement ?? (x => false));

            var combined = Builders<TDocument>.Update.Combine(list);

            if (this._updateDefinition != null)
            {
                list.Add(this._updateDefinition);
            }
            this._updateDefinition = Builders<TDocument>.Update.Combine(list);

            return this;
        }

        /// <summary>
        /// Set fields values in document. (recursive)
        /// </summary>
        /// <param name="doc"> Document instance.</param>
        /// <param name="updateNullValues">if set to <c>true</c> [update null values].</param>
        /// <param name="ignoreElement">The ignore BsonElement.</param>
        /// <returns>The Update object.</returns>
        public Update<TDocument> Set(TDocument doc, bool updateNullValues = true, Func<BsonElement, bool>? ignoreElement = null)
        {
            var bson = BsonDocument.Create(doc);
            return Set(bson, updateNullValues, ignoreElement);
        }

        /// <summary>
        /// Set fields values in document. (recursive)
        /// </summary>
        /// <param name="doc">JsonDocument</param>
        /// <param name="updateNullValues">if set to <c>true</c> [update null values].</param>
        /// <param name="ignoreElement">The ignore BsonElement.</param>
        /// <returns>The Update object.</returns>
        public Update<TDocument> Set(JsonDocument doc, bool updateNullValues = true, Func<BsonElement, bool>? ignoreElement = null)
        {
            var bson = BsonDocument.Parse(doc.ToJson());
            return Set(bson, updateNullValues, ignoreElement);
        }

        private List<UpdateDefinition<TDocument>> BuildNestedUpdate(string path, BsonDocument bsonDocument, bool updateNullValues, Func<BsonElement, bool> ignoreElement)
        {
            var updateBuilder = Builders<TDocument>.Update;
            var updateDefinitions = new List<UpdateDefinition<TDocument>>();


            foreach (var element in bsonDocument.Elements)
            {
                if (ignoreElement(element))
                {
                    continue;
                }

                var fullPath = string.IsNullOrWhiteSpace(path) ? element.Name : $"{path}.{element.Name}";

                if (element.Value.IsBsonDocument)
                {
                    updateDefinitions.AddRange(BuildNestedUpdate(fullPath, element.Value.AsBsonDocument, updateNullValues, ignoreElement));
                }
                else
                {
                    var v = element.Value;

                    if (updateNullValues || (!updateNullValues && !v.IsBsonNull))
                    {
                        //_ = Set(fullPath, v);
                        updateDefinitions.Add(updateBuilder.Set(fullPath, v));
                    }
                }
            }

            return updateDefinitions;
        }

        #endregion
    }
}
