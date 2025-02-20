using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using UCode.Mongo.Attributes;

namespace UCode.Mongo.Serializers
{
    /// <summary>
    /// A generic BSON serializer that ignores properties or fields decorated with
    /// <see cref="IgnorableDataAttribute"/> when the object being serialized is not the root document.
    /// This serializer also implements <see cref="IBsonDocumentSerializer"/> so that member metadata is available.
    /// It additionally supports many standard MongoDB serialization attributes including:
    /// <list type="bullet">
    /// <item><description>BsonSerializerAttribute</description></item>
    /// <item><description>BsonGuidRepresentationAttribute &amp; BsonRepresentationAttribute</description></item>
    /// <item><description>BsonDateTimeOptionsAttribute</description></item>
    /// <item><description>BsonTimeSpanOptionsAttribute</description></item>
    /// <item><description>BsonTimeOnlyOptionsAttribute</description></item>
    /// <item><description>BsonSerializationOptionsAttribute</description> (currently deferred to the default serializer)</item>
    /// <item><description>BsonIgnore, BsonIgnoreIfNull, BsonIgnoreIfDefault, BsonDefaultValue, BsonElement, BsonId</description></item>
    /// <item><description>BsonDictionaryOptions, BsonExtraElements, BsonKnownTypes, BsonDiscriminator</description></item>
    /// </list>
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    public class IgnorableDataSerializer<T> : SerializerBase<T>, IBsonDocumentSerializer
    {
        // A lazy static dictionary mapping member names to BsonSerializationInfo.
        // This metadata supports IBsonDocumentSerializer.
        private static readonly Lazy<Dictionary<string, BsonSerializationInfo>> MemberSerializationInfo =
            new Lazy<Dictionary<string, BsonSerializationInfo>>(() =>
            {
                var keyValuePairs = new Dictionary<string, BsonSerializationInfo>();

                // Process public properties.
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);
                foreach (var prop in properties)
                {
                    // For metadata, we use the declared type and element name (which may be overridden by BsonElement/BsonId).
                    var serializer = GetMemberSerializer(prop, prop.PropertyType);
                    var elementName = GetElementName(prop);
                    keyValuePairs[elementName] = new BsonSerializationInfo(elementName, serializer, prop.PropertyType);
                }

                // Process public fields.
                var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var serializer = GetMemberSerializer(field, field.FieldType);
                    var elementName = GetElementName(field);
                    keyValuePairs[elementName] = new BsonSerializationInfo(elementName, serializer, field.FieldType);
                }

                return keyValuePairs;
            }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Serializes an instance of <typeparamref name="T"/> to BSON.
        /// </summary>
        /// <param name="context">The BSON serialization context.</param>
        /// <param name="args">The BSON serialization arguments.</param>
        /// <param name="value">The value to serialize. If <c>null</c>, a BSON null is written.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying writer cannot be cast to a <see cref="BsonWriter"/>.
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
            var isRoot = bsonWriter.SerializationDepth == 0;

            bsonWriter.WriteStartDocument();

            ProcessProperties(context, value, bsonWriter, isRoot, typeof(T));
            ProcessFields(context, value, bsonWriter, isRoot, typeof(T));

            bsonWriter.WriteEndDocument();
        }

        /// <summary>
        /// Processes and serializes all public properties of the object.
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
                // Skip if decorated with BsonIgnore.
                if (prop.IsDefined(typeof(BsonIgnoreAttribute), inherit: true))
                {
                    continue;
                }
                // Also, skip if [IgnorableData] is present and the object is not the root.
                if (!isRoot && prop.IsDefined(typeof(IgnorableDataAttribute), inherit: true))
                {
                    continue;
                }

                // Determine the element name (may be overridden by BsonElement/BsonId).
                var elementName = GetElementName(prop);
                bsonWriter.WriteName(elementName);
                var propValue = prop.GetValue(value);

                // Respect BsonIgnoreIfNull.
                if (prop.IsDefined(typeof(BsonIgnoreIfNullAttribute), inherit: true) && propValue == null)
                {
                    continue;
                }

                // Respect BsonIgnoreIfDefault/BsonDefaultValue.
                if (prop.IsDefined(typeof(BsonIgnoreIfDefaultAttribute), inherit: true))
                {
                    var defaultValue = prop.GetCustomAttribute<BsonDefaultValueAttribute>(inherit: true)?.DefaultValue
                                             ?? GetDefault(prop.PropertyType);
                    if (object.Equals(propValue, defaultValue))
                    {
                        continue;
                    }
                }

                // Use serializer based on declared type so that attributes (BsonRepresentation, etc.) are honored.
                var serializer = GetMemberSerializer(prop, prop.PropertyType);
                serializer.Serialize(context, propValue);
            }
        }

        /// <summary>
        /// Processes and serializes all public fields of the object.
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
                if (field.IsDefined(typeof(BsonIgnoreAttribute), inherit: true))
                {
                    continue;
                }

                if (!isRoot && field.IsDefined(typeof(IgnorableDataAttribute), inherit: true))
                {
                    continue;
                }

                var elementName = GetElementName(field);
                bsonWriter.WriteName(elementName);
                var fieldValue = field.GetValue(value);

                if (field.IsDefined(typeof(BsonIgnoreIfNullAttribute), inherit: true) && fieldValue == null)
                    continue;

                if (field.IsDefined(typeof(BsonIgnoreIfDefaultAttribute), inherit: true))
                {
                    var defaultValue = field.GetCustomAttribute<BsonDefaultValueAttribute>(inherit: true)?.DefaultValue ?? GetDefault(field.FieldType);

                    if (object.Equals(fieldValue, defaultValue))
                    {
                        continue;
                    }
                }

                var serializer = GetMemberSerializer(field, field.FieldType);
                serializer.Serialize(context, fieldValue);
            }
        }

        /// <summary>
        /// Returns the element name for a property, taking into account BsonElement and BsonId attributes.
        /// </summary>
        /// <param name="prop">The property info.</param>
        /// <returns>The element name to use during serialization.</returns>
        private static string GetElementName(PropertyInfo prop)
        {
            var elemAttr = prop.GetCustomAttribute<BsonElementAttribute>(inherit: true);
            if (elemAttr != null && !string.IsNullOrWhiteSpace(elemAttr.ElementName))
            {
                return elemAttr.ElementName;
            }

            if (prop.IsDefined(typeof(BsonIdAttribute), inherit: true))
            {
                return "_id";
            }
            return prop.Name;
        }

        /// <summary>
        /// Returns the element name for a field, taking into account BsonElement and BsonId attributes.
        /// </summary>
        /// <param name="field">The field info.</param>
        /// <returns>The element name to use during serialization.</returns>
        private static string GetElementName(FieldInfo field)
        {
            var elemAttr = field.GetCustomAttribute<BsonElementAttribute>(inherit: true);
            if (elemAttr != null && !string.IsNullOrWhiteSpace(elemAttr.ElementName))
                return elemAttr.ElementName;
            if (field.IsDefined(typeof(BsonIdAttribute), inherit: true))
                return "_id";
            return field.Name;
        }

        /// <summary>
        /// Returns the default value for a given type.
        /// </summary>
        /// <param name="type">The type for which to return a default value.</param>
        /// <returns>The default value for that type.</returns>
        private static object? GetDefault(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;

        /// <summary>
        /// Helper method that retrieves the appropriate serializer for a member,
        /// taking into account various MongoDB attributes such as BsonSerializer, BsonRepresentation,
        /// BsonGuidRepresentation, BsonDateTimeOptions, BsonTimeSpanOptions, BsonTimeOnlyOptions,
        /// and BsonSerializationOptions.
        /// Additionally, this method now supports a wide range of types when a representation attribute is applied:
        /// ObjectId, short (Int16), int (Int32), long (Int64), float (Single), double, decimal, byte, bool, string,
        /// DateTimeOffset, and others.
        /// </summary>
        /// <param name="member">The member (property or field).</param>
        /// <param name="declaredType">The declared type of the member.</param>
        /// <returns>
        /// An instance of <see cref="IBsonSerializer"/> that honors the member’s declared type and any relevant attributes.
        /// </returns>
        private static IBsonSerializer GetMemberSerializer(MemberInfo member, Type declaredType)
        {
            // 1. Check for a custom serializer via the BsonSerializer attribute.
            var customSerializerAttr = member.GetCustomAttribute<BsonSerializerAttribute>(inherit: true);
            if (customSerializerAttr != null)
            {
                return (IBsonSerializer)Activator.CreateInstance(customSerializerAttr.SerializerType)!;
            }

            // 2. If a BsonRepresentation (or BsonGuidRepresentation) attribute is present,
            // try to return a serializer constructed with the specified representation.
            // Note: repAttr.Representation is of type BsonType.
            var guidRepAttr = member.GetCustomAttribute<BsonGuidRepresentationAttribute>(inherit: true);
            if (guidRepAttr != null && (declaredType == typeof(Guid) || declaredType == typeof(Guid?)))
            {
                return new GuidSerializer(guidRepAttr.GuidRepresentation);
            }

            var repAttr = member.GetCustomAttribute<BsonRepresentationAttribute>(inherit: true);
            if (repAttr != null)
            {
                var rep = repAttr.Representation;

                if (declaredType == typeof(Guid) || declaredType == typeof(Guid?))
                {
                    return new GuidSerializer(rep);
                }
                else if (declaredType == typeof(ObjectId) || declaredType == typeof(ObjectId?))
                {
                    return new ObjectIdSerializer(rep);
                }
                else if (declaredType == typeof(short) || declaredType == typeof(short?))
                {
                    return new Int16Serializer(rep);
                }
                else if (declaredType == typeof(int) || declaredType == typeof(int?))
                {
                    return new Int32Serializer(rep);
                }
                else if (declaredType == typeof(long) || declaredType == typeof(long?))
                {
                    return new Int64Serializer(rep);
                }
                else if (declaredType == typeof(float) || declaredType == typeof(float?))
                {
                    return new SingleSerializer(rep);
                }
                else if (declaredType == typeof(double) || declaredType == typeof(double?))
                {
                    return new DoubleSerializer(rep);
                }
                else if (declaredType == typeof(decimal) || declaredType == typeof(decimal?))
                {
                    return new DecimalSerializer(rep);
                }
                else if (declaredType == typeof(byte) || declaredType == typeof(byte?))
                {
                    // ByteSerializer exists and supports a representation parameter.
                    return new ByteSerializer(rep);
                }
                else if (declaredType == typeof(bool) || declaredType == typeof(bool?))
                {
                    // BooleanSerializer typically does not support a representation override.
                    // Fall back to the default serializer.
                }
                else if (declaredType == typeof(string))
                {
                    // For strings, the representation is inherently a string.
                    return new StringSerializer();
                }
                else if (declaredType == typeof(DateTimeOffset) || declaredType == typeof(DateTimeOffset?))
                {
                    return new DateTimeOffsetSerializer(rep);
                }
                // Add additional cases as needed. For types such as Stream, IEnumerator, IEnumerable, 
                // byte[], char, char[], BsonDocument, BsonValue, JsonDocument, JsonElement, JsonNode, 
                // Dictionary<string, object?>, DateOnly, and ArrayObject, it is assumed that the default serializer
                // (or a collection serializer) is appropriate.
            }

            // 3. Check for BsonDateTimeOptionsAttribute (for DateTime types).
            if ((declaredType == typeof(DateTime) || declaredType == typeof(DateTime?)) &&
                member.IsDefined(typeof(BsonDateTimeOptionsAttribute), inherit: true))
            {
                var dtOptions = member.GetCustomAttribute<BsonDateTimeOptionsAttribute>(inherit: true)!;
                return new DateTimeSerializer(dtOptions.Kind, dtOptions.Representation);
            }

            // 4. Check for BsonTimeSpanOptionsAttribute (for TimeSpan types).
            if ((declaredType == typeof(TimeSpan) || declaredType == typeof(TimeSpan?)) &&
                member.IsDefined(typeof(BsonTimeSpanOptionsAttribute), inherit: true))
            {
                var tsOptions = member.GetCustomAttribute<BsonTimeSpanOptionsAttribute>(inherit: true)!;
                return new TimeSpanSerializer(tsOptions.Representation);
            }

            // 5. Check for BsonTimeOnlyOptionsAttribute (for TimeOnly types).
            if ((declaredType == typeof(TimeOnly) || declaredType == typeof(TimeOnly?)) &&
                member.IsDefined(typeof(BsonTimeOnlyOptionsAttribute), inherit: true))
            {
                var toOptions = member.GetCustomAttribute<BsonTimeOnlyOptionsAttribute>(inherit: true)!;
                // Assuming a TimeOnlySerializer exists that accepts a BsonType.
                return new TimeOnlySerializer(toOptions.Representation);
            }

            // 6. For BsonSerializationOptionsAttribute and others (BsonDictionaryOptions, BsonExtraElements, etc.),
            // we assume that the default serializer returned by BsonSerializer.LookupSerializer honors these options.
            // 7. Fallback: return the default serializer for the declared type.
            return BsonSerializer.LookupSerializer(declaredType);
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
        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo) =>
            MemberSerializationInfo.Value.TryGetValue(memberName, out serializationInfo);

        /// <summary>
        /// Gets all member names represented by this serializer.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{String}"/> containing all member names.</returns>
        public IEnumerable<string> GetMemberNames() => MemberSerializationInfo.Value.Keys;

        #endregion
    }
}
