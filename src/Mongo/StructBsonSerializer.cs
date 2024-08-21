using System;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents a serializer for struct types.
    /// </summary>
    /// <typeparam name="T">The struct type to be serialized.</typeparam>
    public class StructBsonSerializer<T> : IBsonSerializer<T>
    {
        /// <summary>
        /// Gets the type of the value being serialized.
        /// </summary
        public Type ValueType => typeof(T);

        /// <summary>
        /// Deserializes a BSON document into an instance of the struct type.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization arguments.</param>
        /// <returns>The deserialized object.</returns>
        public T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // Create a new instance of the struct type
            var obj = Activator.CreateInstance<T>();

            // Get the BSON reader from the context
            var bsonReader = context.Reader;

            // Read the start of the BSON document
            bsonReader.ReadStartDocument();

            // Iterate over the fields and properties of the struct type
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();

                // Try to get the field with the current name
                var field = this.ValueType.GetField(name);
                if (field != null)
                {
                    // Deserialize the value of the field
                    var value = BsonSerializer.Deserialize(bsonReader, field.FieldType);
                    field.SetValue(obj, value);
                }

                // Try to get the property with the current name
                var prop = this.ValueType.GetProperty(name);
                if (prop != null)
                {
                    // Deserialize the value of the property
                    var value = BsonSerializer.Deserialize(bsonReader, prop.PropertyType);
                    prop.SetValue(obj, value, null);
                }
            }

            // Read the end of the BSON document
            bsonReader.ReadEndDocument();

            // Return the deserialized object
            return obj;
        }

        /// <summary>
        /// Serializes a struct instance to a BSON document.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization arguments.</param>
        /// <param name="value">The struct instance to serialize.</param>
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
        {
            // Get all public instance fields and properties of the struct type
            var fields = this.ValueType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var propsAll = this.ValueType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // Filter out properties that cannot be written to
            var props = new List<PropertyInfo>();
            foreach (var prop in propsAll)
            {
                if (prop.CanWrite)
                {
                    props.Add(prop);
                }
            }

            var bsonWriter = context.Writer;

            // Start writing the BSON document
            bsonWriter.WriteStartDocument();

            // Serialize each field and property
            foreach (var field in fields)
            {
                // Write the field name and value to the BSON document
                bsonWriter.WriteName(field.Name);
                BsonSerializer.Serialize(bsonWriter, field.FieldType, field.GetValue(value));
            }
            foreach (var prop in props)
            {
                // Write the property name and value to the BSON document
                bsonWriter.WriteName(prop.Name);
                BsonSerializer.Serialize(bsonWriter, prop.PropertyType, prop.GetValue(value, null));
            }

            // End writing the BSON document
            bsonWriter.WriteEndDocument();
        }

        /// <summary>
        /// Serializes an object to a BSON document.
        /// Cast the object to the struct type and call the Serialize method with the appropriate arguments
        /// </summary>
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value) => this.Serialize(context, (T)value);

        /// <summary>
        /// Deserializes a BSON document to an object.
        /// Call the Deserialize method with the appropriate arguments and return the result
        /// </summary>
        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) => this.Deserialize(context, args);
    }
}
