using System;
using System.Collections.Generic;
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
    /// This serializer also implements <see cref="IBsonDocumentSerializer"/>, so that member metadata is available
    /// (i.e. members are represented as fields).
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <remarks>
    /// <para>
    /// When serializing an object of type <typeparamref name="T"/>, any public property or field marked with 
    /// <see cref="IgnorableDataAttribute"/> will be omitted if the object is nested (i.e. not the root document).
    /// For example, if a <c>Company</c> class has properties <c>Value</c> and <c>City</c> marked with <c>[IgnorableData]</c>,
    /// those members will be omitted when a <c>Company</c> is embedded inside another object, but they will be included if the
    /// <c>Company</c> object is the root.
    /// </para>
    /// <para>
    /// <b>Exceptions:</b>  
    /// An <see cref="InvalidOperationException"/> is thrown if the underlying writer cannot be cast to a <see cref="BsonWriter"/>.
    /// </para>
    /// </remarks>
    public class IgnorableDataSerializer<T> : SerializerBase<T>, IBsonDocumentSerializer
    {
        // A static dictionary that maps member names to BsonSerializationInfo.
        // This metadata is used to support IBsonDocumentSerializer.
        private static readonly Dictionary<string, BsonSerializationInfo> __memberSerializationInfo;

        static IgnorableDataSerializer()
        {
            __memberSerializationInfo = new Dictionary<string, BsonSerializationInfo>();

            // Process public properties.
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                      .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);
            foreach (var prop in properties)
            {
                var serializer = BsonSerializer.LookupSerializer(prop.PropertyType);
                // Create metadata for the property. Even if the property is marked with IgnorableData,
                // it must be included in the mapping.
                __memberSerializationInfo[prop.Name] =
                    new BsonSerializationInfo(prop.Name, serializer, prop.PropertyType);
            }

            // Process public fields.
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var serializer = BsonSerializer.LookupSerializer(field.FieldType);
                __memberSerializationInfo[field.Name] =
                    new BsonSerializationInfo(field.Name, serializer, field.FieldType);
            }
        }

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
            if (value == null)
            {
                context.Writer.WriteNull();
                return;
            }

            // Cast the writer to BsonWriter to access SerializationDepth.
            var bsonWriter = context.Writer as BsonWriter
                             ?? throw new InvalidOperationException("Expected writer to be a BsonWriter.");
            // When the object is being serialized as the root document, SerializationDepth == 0.
            bool isRoot = bsonWriter.SerializationDepth == 0;

            bsonWriter.WriteStartDocument();

            ProcessProperties(context, value, bsonWriter, isRoot, typeof(T));
            ProcessFields(context, value, bsonWriter, isRoot, typeof(T));

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
                // If the property is decorated with IgnorableDataAttribute and the object is not the root, skip it.
                if (!isRoot && prop.IsDefined(typeof(IgnorableDataAttribute), inherit: true))
                    continue;

                bsonWriter.WriteName(prop.Name);
                var propValue = prop.GetValue(value);
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
                if (!isRoot && field.IsDefined(typeof(IgnorableDataAttribute), inherit: true))
                    continue;

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

        #region IBsonDocumentSerializer Members

        /// <summary>
        /// Attempts to get the member serialization information for a given member name.
        /// </summary>
        /// <param name="memberName">The name of the member.</param>
        /// <param name="serializationInfo">
        /// When this method returns, contains the <see cref="BsonSerializationInfo"/> for the specified member,
        /// if found; otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the member was found; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo) => __memberSerializationInfo.TryGetValue(memberName, out serializationInfo);

        /// <summary>
        /// Gets all member names represented by this serializer.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{String}"/> containing all member names.</returns>
        public IEnumerable<string> GetMemberNames() => __memberSerializationInfo.Keys;

        #endregion
    }



}
