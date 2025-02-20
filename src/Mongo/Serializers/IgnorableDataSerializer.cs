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
    /// A unified BSON serializer/deserializer that ignores properties or fields decorated with
    /// <see cref="IgnorableDataAttribute"/> when the object is not the root document during serialization.
    /// For deserialization, this implementation assumes that the BSON document is at the root level.
    /// It implements <see cref="IBsonDocumentSerializer"/> to provide member metadata.
    /// Supported attributes include BsonElement, BsonId, BsonIgnore, BsonIgnoreIfNull, BsonIgnoreIfDefault, etc.
    /// </summary>
    /// <typeparam name="T">The type to serialize/deserialize.</typeparam>
    public class IgnorableDataSerializer<T> : SerializerBase<T>, IBsonDocumentSerializer
    {
        /// <summary>
        /// Internal structure to hold member metadata.
        /// </summary>
        private struct MemberDataInfo
        {
            public MemberInfo Member;
            public IBsonSerializer Serializer;
            public bool IsIgnorable;
        }

        /// <summary>
        /// Lazy-initialized dictionary mapping member names (both BSON element names and C# names)
        /// to member metadata (member info, serializer, and whether it is marked with [IgnorableData]).
        /// </summary>
        private static readonly Lazy<Dictionary<string, MemberDataInfo>> MemberDataDictionary =
            new Lazy<Dictionary<string, MemberDataInfo>>(() =>
            {
                var dict = new Dictionary<string, MemberDataInfo>(StringComparer.Ordinal);
                Type currentType = typeof(T);
                while (currentType != null && currentType != typeof(object))
                {
                    // Process properties.
                    var properties = currentType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                                .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0);
                    foreach (var prop in properties)
                    {
                        if (prop.IsDefined(typeof(BsonIgnoreAttribute), inherit: true))
                            continue;
                        var serializer = GetMemberSerializer(prop, prop.PropertyType);
                        string elementName = GetElementName(prop);
                        bool isIgnorable = prop.IsDefined(typeof(IgnorableDataAttribute), inherit: true);
                        if (!dict.ContainsKey(elementName))
                        {
                            dict[elementName] = new MemberDataInfo { Member = prop, Serializer = serializer, IsIgnorable = isIgnorable };
                        }
                        if (!string.Equals(prop.Name, elementName, StringComparison.Ordinal) && !dict.ContainsKey(prop.Name))
                        {
                            dict[prop.Name] = new MemberDataInfo { Member = prop, Serializer = serializer, IsIgnorable = isIgnorable };
                        }
                    }

                    // Process fields.
                    var fields = currentType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    foreach (var field in fields)
                    {
                        if (field.IsDefined(typeof(BsonIgnoreAttribute), inherit: true))
                            continue;
                        var serializer = GetMemberSerializer(field, field.FieldType);
                        string elementName = GetElementName(field);
                        bool isIgnorable = field.IsDefined(typeof(IgnorableDataAttribute), inherit: true);
                        if (!dict.ContainsKey(elementName))
                        {
                            dict[elementName] = new MemberDataInfo { Member = field, Serializer = serializer, IsIgnorable = isIgnorable };
                        }
                        if (!string.Equals(field.Name, elementName, StringComparison.Ordinal) && !dict.ContainsKey(field.Name))
                        {
                            dict[field.Name] = new MemberDataInfo { Member = field, Serializer = serializer, IsIgnorable = isIgnorable };
                        }
                    }
                    currentType = currentType.BaseType;
                }
                return dict;
            }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        #region Serialization Methods

        /// <summary>
        /// Serializes an instance of <typeparamref name="T"/> to BSON.
        /// If the object is not the root document, properties or fields marked with [IgnorableData] are ignored.
        /// </summary>
        /// <param name="context">The BSON serialization context.</param>
        /// <param name="args">The BSON serialization arguments.</param>
        /// <param name="value">The instance to serialize. If null, writes a BSON null.</param>
        /// <exception cref="InvalidOperationException">Thrown if the writer is not a BsonWriter.</exception>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
        {
            if (value == null)
            {
                context.Writer.WriteNull();
                return;
            }

            var bsonWriter = context.Writer as BsonWriter
                ?? throw new InvalidOperationException("Expected writer to be a BsonWriter.");

            // Determine if the object is the root document.
            // Note: BsonWriter provides SerializationDepth.
            var isRoot = bsonWriter.SerializationDepth == 0;

            bsonWriter.WriteStartDocument();

            // Process properties.
            ProcessProperties(context, value, bsonWriter, isRoot, typeof(T));

            // Process fields.
            ProcessFields(context, value, bsonWriter, isRoot, typeof(T));

            bsonWriter.WriteEndDocument();
        }

        /// <summary>
        /// Processes and serializes all public properties of the object.
        /// </summary>
        private static void ProcessProperties(BsonSerializationContext context, T value, BsonWriter bsonWriter, bool isRoot, Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0);
            foreach (var prop in properties)
            {
                // Skip if decorated with BsonIgnore.
                if (prop.IsDefined(typeof(BsonIgnoreAttribute), inherit: true))
                    continue;

                // Skip if [IgnorableData] is present and the object is not the root.
                if (!isRoot && prop.IsDefined(typeof(IgnorableDataAttribute), inherit: true))
                    continue;

                var elementName = GetElementName(prop);
                bsonWriter.WriteName(elementName);
                var propValue = prop.GetValue(value);

                // Respect BsonIgnoreIfNull.
                if (prop.IsDefined(typeof(BsonIgnoreIfNullAttribute), inherit: true) && propValue == null)
                    continue;

                // Respect BsonIgnoreIfDefault.
                if (prop.IsDefined(typeof(BsonIgnoreIfDefaultAttribute), inherit: true))
                {
                    var defaultValue = prop.GetCustomAttribute<BsonDefaultValueAttribute>(inherit: true)?.DefaultValue
                                             ?? GetDefault(prop.PropertyType);
                    if (object.Equals(propValue, defaultValue))
                        continue;
                }

                var serializer = GetMemberSerializer(prop, prop.PropertyType);
                serializer.Serialize(context, propValue);
            }
        }

        /// <summary>
        /// Processes and serializes all public fields of the object.
        /// </summary>
        private static void ProcessFields(BsonSerializationContext context, T value, BsonWriter bsonWriter, bool isRoot, Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.IsDefined(typeof(BsonIgnoreAttribute), inherit: true))
                    continue;

                if (!isRoot && field.IsDefined(typeof(IgnorableDataAttribute), inherit: true))
                    continue;

                var elementName = GetElementName(field);
                bsonWriter.WriteName(elementName);
                var fieldValue = field.GetValue(value);

                if (field.IsDefined(typeof(BsonIgnoreIfNullAttribute), inherit: true) && fieldValue == null)
                    continue;

                if (field.IsDefined(typeof(BsonIgnoreIfDefaultAttribute), inherit: true))
                {
                    var defaultValue = field.GetCustomAttribute<BsonDefaultValueAttribute>(inherit: true)?.DefaultValue
                                             ?? GetDefault(field.FieldType);
                    if (object.Equals(fieldValue, defaultValue))
                        continue;
                }

                var serializer = GetMemberSerializer(field, field.FieldType);
                serializer.Serialize(context, fieldValue);
            }
        }

        #endregion

        #region Deserialization Methods

        /// <summary>
        /// Deserializes a BSON document into an instance of <typeparamref name="T"/>.
        /// For deserialization, this implementation assumes that the document is at the root level.
        /// </summary>
        /// <param name="context">The BSON deserialization context.</param>
        /// <param name="args">The BSON deserialization arguments.</param>
        /// <returns>An instance of <typeparamref name="T"/> populated with data from the BSON document.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the reader is not a BsonReader.</exception>
        public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // For deserialization, we assume the document is at the root.
            var isRoot = true;
            var bsonReader = context.Reader as BsonReader
                ?? throw new InvalidOperationException("Expected reader to be a BsonReader.");

            if (bsonReader.GetCurrentBsonType() == BsonType.Null)
            {
                bsonReader.ReadNull();
                return default!;
            }

            bsonReader.ReadStartDocument();
            var instance = (T)Activator.CreateInstance(typeof(T))!;

            // Iterate over BSON elements.
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var elementName = bsonReader.ReadName();

                // Look up the member metadata using the unified dictionary.
                if (MemberDataDictionary.Value.TryGetValue(elementName, out MemberDataInfo memberData))
                {
                    // If not root and the member is ignorable, skip its value.
                    // (Since we assume isRoot = true in deserialization, this condition never skips.)
                    if (!isRoot && memberData.IsIgnorable)
                    {
                        bsonReader.SkipValue();
                        continue;
                    }

                    var value = memberData.Serializer.Deserialize(context);
                    SetMemberValue(instance, memberData.Member, value);
                }
                else
                {
                    // Skip unknown elements.
                    bsonReader.SkipValue();
                }
            }

            bsonReader.ReadEndDocument();
            return instance;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Sets the value of a property or field on the given instance.
        /// </summary>
        /// <param name="instance">The object instance to update.</param>
        /// <param name="member">The member (property or field) to set.</param>
        /// <param name="value">The value to assign.</param>
        /// <exception cref="InvalidOperationException">Thrown if the member type is unsupported.</exception>
        private static void SetMemberValue(object instance, MemberInfo member, object? value)
        {
            if (member is PropertyInfo property)
            {
                property.SetValue(instance, value);
            }
            else if (member is FieldInfo field)
            {
                field.SetValue(instance, value);
            }
            else
            {
                throw new InvalidOperationException("Unsupported member type.");
            }
        }

        /// <summary>
        /// Returns the default value for a given type.
        /// </summary>
        private static object? GetDefault(Type type) =>
            type.IsValueType ? Activator.CreateInstance(type) : null;

        /// <summary>
        /// Returns the BSON element name for a property, considering BsonElement and BsonId attributes.
        /// </summary>
        private static string GetElementName(PropertyInfo prop)
        {
            var elemAttr = prop.GetCustomAttribute<BsonElementAttribute>(inherit: true);
            if (elemAttr != null && !string.IsNullOrWhiteSpace(elemAttr.ElementName))
                return elemAttr.ElementName;
            if (prop.IsDefined(typeof(BsonIdAttribute), inherit: true))
                return "_id";
            return prop.Name;
        }

        /// <summary>
        /// Returns the BSON element name for a field, considering BsonElement and BsonId attributes.
        /// </summary>
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
        /// Retrieves the appropriate serializer for a member, taking into account various MongoDB attributes.
        /// </summary>
        private static IBsonSerializer GetMemberSerializer(MemberInfo member, Type declaredType)
        {
            // 1. Check for a custom serializer via the BsonSerializer attribute.
            var customSerializerAttr = member.GetCustomAttribute<BsonSerializerAttribute>(inherit: true);
            if (customSerializerAttr != null)
            {
                return (IBsonSerializer)Activator.CreateInstance(customSerializerAttr.SerializerType)!;
            }

            // 2. Check for BsonGuidRepresentation attribute.
            var guidRepAttr = member.GetCustomAttribute<BsonGuidRepresentationAttribute>(inherit: true);
            if (guidRepAttr != null && (declaredType == typeof(Guid) || declaredType == typeof(Guid?)))
            {
                return new GuidSerializer(guidRepAttr.GuidRepresentation);
            }

            // 3. Check for BsonRepresentation attribute.
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
                    return new ByteSerializer(rep);
                }
                else if (declaredType == typeof(bool) || declaredType == typeof(bool?))
                {
                    // BooleanSerializer typically does not support a representation override.
                }
                else if (declaredType == typeof(string))
                {
                    return new StringSerializer();
                }
                else if (declaredType == typeof(DateTimeOffset) || declaredType == typeof(DateTimeOffset?))
                {
                    return new DateTimeOffsetSerializer(rep);
                }
            }

            // 4. Check for BsonDateTimeOptions attribute for DateTime types.
            if ((declaredType == typeof(DateTime) || declaredType == typeof(DateTime?)) &&
                member.IsDefined(typeof(BsonDateTimeOptionsAttribute), inherit: true))
            {
                var dtOptions = member.GetCustomAttribute<BsonDateTimeOptionsAttribute>(inherit: true)!;
                return new DateTimeSerializer(dtOptions.Kind, dtOptions.Representation);
            }

            // 5. Check for BsonTimeSpanOptions attribute for TimeSpan types.
            if ((declaredType == typeof(TimeSpan) || declaredType == typeof(TimeSpan?)) &&
                member.IsDefined(typeof(BsonTimeSpanOptionsAttribute), inherit: true))
            {
                var tsOptions = member.GetCustomAttribute<BsonTimeSpanOptionsAttribute>(inherit: true)!;
                return new TimeSpanSerializer(tsOptions.Representation);
            }

            // 6. Check for BsonTimeOnlyOptions attribute for TimeOnly types.
            if ((declaredType == typeof(TimeOnly) || declaredType == typeof(TimeOnly?)) &&
                member.IsDefined(typeof(BsonTimeOnlyOptionsAttribute), inherit: true))
            {
                var toOptions = member.GetCustomAttribute<BsonTimeOnlyOptionsAttribute>(inherit: true)!;
                // Assuming a TimeOnlySerializer exists that accepts a BsonType.
                return new TimeOnlySerializer(toOptions.Representation);
            }

            // 7. Fallback: return the default serializer for the declared type.
            return BsonSerializer.LookupSerializer(declaredType);
        }

        #endregion

        #region IBsonDocumentSerializer Members

        /// <summary>
        /// Attempts to get the member serialization information for a given member name.
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <param name="serializationInfo">
        /// When this method returns, contains the <see cref="BsonSerializationInfo"/> for the specified member, if found.
        /// </param>
        /// <returns><c>true</c> if the member was found; otherwise, <c>false</c>.</returns>
        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            if (MemberDataDictionary.Value.TryGetValue(memberName, out MemberDataInfo data))
            {
                var memberType = data.Member is PropertyInfo prop ? prop.PropertyType :
                                  data.Member is FieldInfo field ? field.FieldType :
                                  throw new InvalidOperationException("Unsupported member type.");
                serializationInfo = new BsonSerializationInfo(memberName, data.Serializer, memberType);
                return true;
            }
            serializationInfo = null!;
            return false;
        }

        /// <summary>
        /// Gets all member names represented by this serializer.
        /// </summary>
        public IEnumerable<string> GetMemberNames() => MemberDataDictionary.Value.Keys;

        #endregion
    }
}
