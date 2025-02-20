using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using UCode.Mongo.Attributes;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;

namespace UCode.Mongo.Serializers
{
    /// <summary>
    /// A unified BSON serializer/deserializer that ignores properties or fields decorated with
    /// <see cref="IgnorableDataAttribute"/> when the object is not the root document during serialization.
    /// For deserialization, the BSON document is assumed to be at the root level.
    /// This implementation considers the element name and order as defined by [BsonElement] (or, if missing, by [JsonPropertyName]
    /// and [JsonPropertyOrder]). If neither is defined, the member name is used and the order is int.MaxValue.
    /// Supported attributes include BsonElement, BsonId, BsonIgnore, BsonIgnoreIfNull, BsonIgnoreIfDefault,
    /// JsonPropertyName and JsonPropertyOrder.
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
        /// Lazy-initialized dictionary mapping member keys (both the BSON element name and the C# member name)
        /// to member metadata. This dictionary is used during deserialization to map element names to members.
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
                        if (prop.IsDefined(typeof(BsonIgnoreAttribute), true))
                            continue;
                        var serializer = GetMemberSerializer(prop, prop.PropertyType);
                        string elementName =  GetElementName(prop);
                        bool isIgnorable = prop.IsDefined(typeof(IgnorableDataAttribute), true);
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
                        if (field.IsDefined(typeof(BsonIgnoreAttribute), true))
                            continue;
                        var serializer = GetMemberSerializer(field, field.FieldType);
                        string elementName = GetElementName(field);
                        bool isIgnorable = field.IsDefined(typeof(IgnorableDataAttribute), true);
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
        /// When the object is not the root document, properties or fields marked with [IgnorableData] are ignored.
        /// The element names and the order of the elements are determined by [BsonElement] (or [JsonPropertyName] and [JsonPropertyOrder]).
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

            var bsonWriter = context.Writer as BsonWriter ?? throw new InvalidOperationException("Expected writer to be a BsonWriter.");

            // Determine if the object is the root document.
            var isRoot = bsonWriter.SerializationDepth == 0;

            var type = typeof(T);

            bsonWriter.WriteStartDocument();


            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(w => w.MemberType is MemberTypes.Property or MemberTypes.Field &&
                    ((w is PropertyInfo prop && prop.CanRead && prop.GetIndexParameters().Length == 0) || (w is FieldInfo field)) ).OrderBy(p => GetOrder(p));

            foreach (var member in members)
            {
                if (member is PropertyInfo prop)
                {
                    if (prop.IsDefined(typeof(BsonIgnoreAttribute), true))
                        continue;

                    if (!isRoot && prop.IsDefined(typeof(IgnorableDataAttribute), true))
                        continue;

                    // Determine the element name based on [BsonElement] or [JsonPropertyName].
                    var elementName = GetElementName(prop);
                    bsonWriter.WriteName(elementName);
                    var propValue = prop.GetValue(value);

                    if (prop.IsDefined(typeof(BsonIgnoreIfNullAttribute), true) && propValue == null)
                        continue;

                    if (prop.IsDefined(typeof(BsonIgnoreIfDefaultAttribute), true))
                    {
                        var defaultValue = GetDefault(member, prop.PropertyType);
                        if (object.Equals(propValue, defaultValue))
                            continue;
                    }

                    var serializer = GetMemberSerializer(prop, prop.PropertyType);
                    serializer.Serialize(context, propValue);
                }
                else if (member is FieldInfo field)
                {
                    if (field.IsDefined(typeof(BsonIgnoreAttribute), true))
                        continue;

                    if (!isRoot && field.IsDefined(typeof(IgnorableDataAttribute), true))
                        continue;

                    var elementName = GetElementName(field);
                    bsonWriter.WriteName(elementName);
                    var fieldValue = field.GetValue(value);

                    if (field.IsDefined(typeof(BsonIgnoreIfNullAttribute), true) && fieldValue == null)
                        continue;

                    if (field.IsDefined(typeof(BsonIgnoreIfDefaultAttribute), true))
                    {
                        var defaultValue = GetDefault(member, field.FieldType);
                        if (object.Equals(fieldValue, defaultValue))
                            continue;
                    }

                    var serializer = GetMemberSerializer(field, field.FieldType);
                    serializer.Serialize(context, fieldValue);
                }
            }


            bsonWriter.WriteEndDocument();
        }


        #endregion

        #region Deserialization Methods

        /// <summary>
        /// Deserializes a BSON document into an instance of <typeparamref name="T"/>.
        /// The deserialization considers the element name and order: values are read and then applied
        /// in the order determined by the attributes.
        /// For deserialization, the document is assumed to be at the root level.
        /// </summary>
        /// <param name="context">The BSON deserialization context.</param>
        /// <param name="args">The BSON deserialization arguments.</param>
        /// <returns>An instance of <typeparamref name="T"/> populated with data from the BSON document.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the reader is not a BsonReader.</exception>
        public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
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

            // Temporary list to hold deserialized values along with their order.
            var deserializedMembers = new List<(int Order, MemberDataInfo MemberData, object Value)>();

            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var elementName = bsonReader.ReadName();

                if (MemberDataDictionary.Value.TryGetValue(elementName, out MemberDataInfo memberData))
                {
                    if (!isRoot && memberData.IsIgnorable)
                    {
                        bsonReader.SkipValue();
                        continue;
                    }

                    var value = memberData.Serializer.Deserialize(context);
                    int order = GetOrder(memberData.Member);
                    deserializedMembers.Add((order, memberData, value));
                }
                else
                {
                    bsonReader.SkipValue();
                }
            }

            bsonReader.ReadEndDocument();

            // Apply the deserialized values in order.
            foreach (var item in deserializedMembers.OrderBy(m => m.Order))
            {
                SetMemberValue(instance, item.MemberData.Member, item.Value);
            }

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
        private static object? GetDefault(MemberInfo memberInfo, Type type) =>
            memberInfo.GetCustomAttribute<BsonDefaultValueAttribute>(true)?.DefaultValue ?? (type.IsValueType ? Activator.CreateInstance(type) : null);

        /// <summary>
        /// Determines the element name for a property.
        /// It first checks for [BsonElement] with a non-empty ElementName; if not found, then [JsonPropertyName];
        /// otherwise, it uses the property name (or "_id" if marked with [BsonId]).
        /// </summary>
        private static string GetElementName(PropertyInfo prop)
        {
            var bsonElem = prop.GetCustomAttribute<BsonElementAttribute>(true);
            if (bsonElem != null && !string.IsNullOrWhiteSpace(bsonElem.ElementName))
                return bsonElem.ElementName;

            var jsonProp = prop.GetCustomAttribute<JsonPropertyNameAttribute>(true);
            if (jsonProp != null && !string.IsNullOrWhiteSpace(jsonProp.Name))
                return jsonProp.Name;

            if (prop.IsDefined(typeof(BsonIdAttribute), true))
                return "_id";

            return prop.Name;
        }

        /// <summary>
        /// Determines the element name for a field.
        /// It first checks for [BsonElement] with a non-empty ElementName; if not found, then [JsonPropertyName];
        /// otherwise, it uses the field name (or "_id" if marked with [BsonId]).
        /// </summary>
        private static string GetElementName(FieldInfo field)
        {
            var bsonElem = field.GetCustomAttribute<BsonElementAttribute>(true);
            if (bsonElem != null && !string.IsNullOrWhiteSpace(bsonElem.ElementName))
                return bsonElem.ElementName;
            var jsonProp = field.GetCustomAttribute<JsonPropertyNameAttribute>(true);
            if (jsonProp != null && !string.IsNullOrWhiteSpace(jsonProp.Name))
                return jsonProp.Name;
            if (field.IsDefined(typeof(BsonIdAttribute), true))
                return "_id";
            return field.Name;
        }

        /// <summary>
        /// Retrieves the appropriate serializer for a member, taking into account various MongoDB attributes.
        /// </summary>
        private static IBsonSerializer GetMemberSerializer(MemberInfo member, Type declaredType)
        {
            var customSerializerAttr = member.GetCustomAttribute<BsonSerializerAttribute>(true);
            if (customSerializerAttr != null)
            {
                return (IBsonSerializer)Activator.CreateInstance(customSerializerAttr.SerializerType)!;
            }

            var guidRepAttr = member.GetCustomAttribute<BsonGuidRepresentationAttribute>(true);
            if (guidRepAttr != null && (declaredType == typeof(Guid) || declaredType == typeof(Guid?)))
            {
                return new GuidSerializer(guidRepAttr.GuidRepresentation);
            }

            var repAttr = member.GetCustomAttribute<BsonRepresentationAttribute>(true);
            if (repAttr != null)
            {
                var rep = repAttr.Representation;
                if (declaredType == typeof(Guid) || declaredType == typeof(Guid?))
                    return new GuidSerializer(rep);
                else if (declaredType == typeof(ObjectId) || declaredType == typeof(ObjectId?))
                    return new ObjectIdSerializer(rep);
                else if (declaredType == typeof(short) || declaredType == typeof(short?))
                    return new Int16Serializer(rep);
                else if (declaredType == typeof(int) || declaredType == typeof(int?))
                    return new Int32Serializer(rep);
                else if (declaredType == typeof(long) || declaredType == typeof(long?))
                    return new Int64Serializer(rep);
                else if (declaredType == typeof(float) || declaredType == typeof(float?))
                    return new SingleSerializer(rep);
                else if (declaredType == typeof(double) || declaredType == typeof(double?))
                    return new DoubleSerializer(rep);
                else if (declaredType == typeof(decimal) || declaredType == typeof(decimal?))
                    return new DecimalSerializer(rep);
                else if (declaredType == typeof(byte) || declaredType == typeof(byte?))
                    return new ByteSerializer(rep);
                else if (declaredType == typeof(bool) || declaredType == typeof(bool?))
                {
                    // BooleanSerializer typically does not support a representation override.
                }
                else if (declaredType == typeof(string))
                    return new StringSerializer();
                else if (declaredType == typeof(DateTimeOffset) || declaredType == typeof(DateTimeOffset?))
                    return new DateTimeOffsetSerializer(rep);
            }

            if ((declaredType == typeof(DateTime) || declaredType == typeof(DateTime?)) &&
                member.IsDefined(typeof(BsonDateTimeOptionsAttribute), true))
            {
                var dtOptions = member.GetCustomAttribute<BsonDateTimeOptionsAttribute>(true)!;
                return new DateTimeSerializer(dtOptions.Kind, dtOptions.Representation);
            }

            if ((declaredType == typeof(TimeSpan) || declaredType == typeof(TimeSpan?)) &&
                member.IsDefined(typeof(BsonTimeSpanOptionsAttribute), true))
            {
                var tsOptions = member.GetCustomAttribute<BsonTimeSpanOptionsAttribute>(true)!;
                return new TimeSpanSerializer(tsOptions.Representation);
            }

            if ((declaredType == typeof(TimeOnly) || declaredType == typeof(TimeOnly?)) &&
                member.IsDefined(typeof(BsonTimeOnlyOptionsAttribute), true))
            {
                var toOptions = member.GetCustomAttribute<BsonTimeOnlyOptionsAttribute>(true)!;
                return new TimeOnlySerializer(toOptions.Representation);
            }

            return BsonSerializer.LookupSerializer(declaredType);
        }

        /// <summary>
        /// Retrieves the order for a member.
        /// It first checks for [BsonElement] and uses its Order if defined (and not int.MaxValue);
        /// otherwise, it checks for [JsonPropertyOrder]; if not found, returns int.MaxValue.
        /// </summary>
        private static int GetOrder(MemberInfo member)
        {
            var bsonElem = member.GetCustomAttribute<BsonElementAttribute>(true);
            if (bsonElem != null && bsonElem.Order != int.MaxValue)
                return bsonElem.Order;
            var jsonOrder = member.GetCustomAttribute<JsonPropertyOrderAttribute>(true);
            if (jsonOrder != null)
                return jsonOrder.Order;
            return int.MaxValue;
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
