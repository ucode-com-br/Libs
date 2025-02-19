using System;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using UCode.Mongo.Attributes;

namespace UCode.Mongo.Serializers
{
    /// <summary>
    /// A generic BSON serializer that ignores properties or fields decorated with
    /// <see cref="IgnorableDataAttribute"/> when the object being serialized is not the root document.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <remarks>
    /// <para>
    /// When serializing an object of type <typeparamref name="T"/>, any public property or field
    /// marked with <see cref="IgnorableDataAttribute"/> will be omitted if the object is nested (i.e. not the root).
    /// For example, consider a <c>Company</c> class where properties such as <c>Value</c> and <c>City</c> are marked
    /// with <c>[IgnorableData]</c>. When a <c>Company</c> object is embedded inside another object (e.g., as an element
    /// of an array in a <c>Person</c> document), those properties will be excluded from the BSON output.
    /// </para>
    /// <para>
    /// **Exceptions:**  
    /// If the <see cref="BsonSerializationContext.Writer"/> cannot be cast to a <see cref="BsonWriter"/>, an
    /// <see cref="InvalidOperationException"/> is thrown.
    /// </para>
    /// </remarks>
    public class IgnorableDataSerializer<T> : SerializerBase<T>
    {

        /// <summary>
        /// Serializes an instance of <typeparamref name="T"/> to BSON.
        /// </summary>
        /// <param name="context">The BSON serialization context.</param>
        /// <param name="args">The BSON serialization arguments.</param>
        /// <param name="value">The value to serialize. If <c>null</c>, a BSON null is written.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying writer cannot be cast to <see cref="BsonWriter"/>.
        /// </exception>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
        {
            // Write BSON null if the object is null.
            if (value == null)
            {
                context.Writer.WriteNull();
                return;
            }

            var type = typeof(T);

            // Cast the writer to BsonWriter to access the SerializationDepth property.
            // (e.g. BsonBinaryWriter and BsonJsonWriter derive from BsonWriter.)
            var bsonWriter = context.Writer as BsonWriter ?? throw new InvalidOperationException("Expected writer to be a BsonWriter.");

            // When the object is being serialized as the root document, SerializationDepth == 0.
            var isRoot = bsonWriter.SerializationDepth == 0;

            bsonWriter.WriteStartDocument();

            // Serialize properties.
            ProcessProperties(context, value, bsonWriter, isRoot, type);

            // Serialize fields.
            ProcessFields(context, value, bsonWriter, isRoot, type);

            bsonWriter.WriteEndDocument();
        }

        /// <summary>
        /// Processes and serializes all public properties of the given object.
        /// </summary>
        /// <param name="context">The BSON serialization context.</param>
        /// <param name="value">The object containing the properties.</param>
        /// <param name="bsonWriter">The BSON writer.</param>
        /// <param name="isRoot">Indicates if the object is the root document.</param>
        /// <param name="type">The type of the object.</param>
        private static void ProcessProperties(BsonSerializationContext context, T value, BsonWriter bsonWriter, bool isRoot, Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

            foreach (var prop in properties)
            {
                // Skip properties marked with [IgnorableData] if the object is not the root.
                if (!isRoot && prop.IsDefined(typeof(IgnorableDataAttribute), inherit: true))
                {
                    continue;
                }

                bsonWriter.WriteName(prop.Name);

                // Retrieve the property value.
                var propValue = prop.GetValue(value);

                // Look up the appropriate serializer for the property’s type.
                var serializer = BsonSerializer.LookupSerializer(propValue?.GetType() ?? typeof(object));


                serializer.Serialize(context, propValue);
            }
        }

        /// <summary>
        /// Processes and serializes all public fields of the given object.
        /// </summary>
        /// <param name="context">The BSON serialization context.</param>
        /// <param name="value">The object containing the fields.</param>
        /// <param name="bsonWriter">The BSON writer.</param>
        /// <param name="isRoot">Indicates if the object is the root document.</param>
        /// <param name="type">The type of the object.</param>
        private static void ProcessFields(BsonSerializationContext context, T value, BsonWriter bsonWriter, bool isRoot, Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                // Skip fields marked with [IgnorableData] if the object is not the root.
                if (!isRoot && field.IsDefined(typeof(IgnorableDataAttribute), inherit: true))
                {
                    continue;
                }

                bsonWriter.WriteName(field.Name);
                var fieldValue = field.GetValue(value);
                var serializer = BsonSerializer.LookupSerializer(fieldValue?.GetType() ?? typeof(object));
                serializer.Serialize(context, fieldValue);
            }
        }

        /// <summary>
        /// Deserializes a BSON document into an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="context">The BSON deserialization context.</param>
        /// <param name="args">The BSON deserialization arguments.</param>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// This implementation delegates to the default BSON deserializer for type <typeparamref name="T"/>.
        /// </remarks>
        public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) =>
            BsonSerializer.Deserialize<T>(context.Reader);
    }


}
