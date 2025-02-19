using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using UCode.Mongo.Attributes;

namespace UCode.Mongo.Serializers
{
    /// <summary>
    /// A generic System.Text.Json converter that ignores properties or fields decorated with
    /// <see cref="IgnorableDataAttribute"/> when the object being serialized is not the root document.
    /// </summary>
    /// <typeparam name="T">The type to convert.</typeparam>
    /// <remarks>
    /// <para>
    /// When serializing an object of type <typeparamref name="T"/>, any public property or field marked with 
    /// <see cref="IgnorableDataAttribute"/> will be omitted if the object is nested (i.e. not the root document).
    /// For example, consider the following classes:
    /// </para>
    /// <code language="csharp">
    /// using UCode.Mongo.Attributes;
    /// 
    /// public class Company
    /// {
    ///     public string CompanyName { get; set; }
    ///     
    ///     [IgnorableData]
    ///     public double Value { get; set; }
    ///     
    ///     [IgnorableData]
    ///     public string City { get; set; }
    /// }
    /// 
    /// public class Person
    /// {
    ///     public string Name { get; set; }
    ///     public Company[] Companies { get; set; }
    ///     public int Age { get; set; }
    /// }
    /// </code>
    /// <para>
    /// When serializing a <c>Person</c> object, if a <c>Company</c> object is nested (for example, as an element in the 
    /// <c>Companies</c> array), the <c>Value</c> and <c>City</c> members will be omitted.
    /// </para>
    /// <para>
    /// <b>Exceptions:</b>  
    /// This converter throws an <see cref="ArgumentNullException"/> if the provided writer is null. Other exceptions may be thrown if reflection fails.
    /// </para>
    /// </remarks>
    public class IgnorableDataJsonConverter<T> : JsonConverter<T>
    {
        /// <summary>
        /// Reads and converts the JSON to type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">Serialization options.</param>
        /// <returns>An instance of type <typeparamref name="T"/>.</returns>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // For deserialization, we delegate to the default serializer.
            return JsonSerializer.Deserialize<T>(ref reader, options);
        }

        /// <summary>
        /// Writes the JSON representation of the object, omitting members decorated with 
        /// <see cref="IgnorableDataAttribute"/> if the object is not the root document.
        /// </summary>
        /// <param name="writer">The JSON writer.</param>
        /// <param name="value">The object value to write. If null, a JSON null is written.</param>
        /// <param name="options">Serialization options.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="writer"/> is null.</exception>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            // Determine if this object is the root document.
            // When Write is called at the root level, writer.CurrentDepth is 0.
            var initialDepth = writer.CurrentDepth;
            var isRoot = initialDepth == 0;

            writer.WriteStartObject();

            var type = typeof(T);

            // Process public properties.
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);
            foreach (var prop in properties)
            {
                // If not root and the property is marked with IgnorableDataAttribute, skip it.
                if (!isRoot && prop.IsDefined(typeof(IgnorableDataAttribute), inherit: true))
                    continue;

                writer.WritePropertyName(prop.Name);
                var propValue = prop.GetValue(value);
                JsonSerializer.Serialize(writer, propValue, prop.PropertyType, options);
            }

            // Process public fields.
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (!isRoot && field.IsDefined(typeof(IgnorableDataAttribute), inherit: true))
                    continue;

                writer.WritePropertyName(field.Name);
                var fieldValue = field.GetValue(value);
                JsonSerializer.Serialize(writer, fieldValue, field.FieldType, options);
            }

            writer.WriteEndObject();
        }
    }
}
