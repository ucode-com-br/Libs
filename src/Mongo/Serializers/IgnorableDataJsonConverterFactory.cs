//using System;
//using System.Linq;
//using System.Reflection;
//using System.Text.Json.Serialization;
//using System.Text.Json;
//using UCode.Mongo.Attributes;

//namespace UCode.Mongo.Serializers
//{
//    /// <summary>
//    /// A converter factory that creates <see cref="IgnorableDataJsonConverter{T}"/> instances for types that have members
//    /// decorated with <see cref="IgnorableDataAttribute"/>.
//    /// </summary>
//    /// <remarks>
//    /// <para>
//    /// The factory's <see cref="CanConvert"/> method returns <c>true</c> if the type contains at least one public property
//    /// or field marked with <see cref="IgnorableDataAttribute"/>; otherwise, it returns <c>false</c>, allowing the default serializer to be used.
//    /// </para>
//    /// <para>
//    /// <b>Example:</b>
//    /// <code language="csharp">
//    /// // During application startup:
//    /// var options = new JsonSerializerOptions();
//    /// options.Converters.Add(new IgnorableDataJsonConverterFactory());
//    /// 
//    /// // When serializing a Person object, nested Company objects will omit the properties Value and City.
//    /// var json = JsonSerializer.Serialize(person, options);
//    /// </code>
//    /// </para>
//    /// </remarks>
//    public class IgnorableDataJsonConverterFactory : JsonConverterFactory
//    {
//        /// <summary>
//        /// Determines whether the specified type can be converted by this factory.
//        /// </summary>
//        /// <param name="typeToConvert">The type to convert.</param>
//        /// <returns>
//        /// <c>true</c> if the type has at least one public property or field marked with <see cref="IgnorableDataAttribute"/>;
//        /// otherwise, <c>false</c>.
//        /// </returns>
//        public override bool CanConvert(Type typeToConvert)
//        {
//            // Only support classes and structs.
//            if (!typeToConvert.IsClass && !typeToConvert.IsValueType)
//                return false;

//            var hasIgnorable = typeToConvert.GetProperties(BindingFlags.Public | BindingFlags.Instance)
//                .Any(p => p.IsDefined(typeof(IgnorableDataAttribute), inherit: true))
//                || typeToConvert.GetFields(BindingFlags.Public | BindingFlags.Instance)
//                .Any(f => f.IsDefined(typeof(IgnorableDataAttribute), inherit: true));

//            return hasIgnorable;
//        }

//        /// <summary>
//        /// Creates a converter for the specified type.
//        /// </summary>
//        /// <param name="typeToConvert">The type to convert.</param>
//        /// <param name="options">Serialization options.</param>
//        /// <returns>A <see cref="JsonConverter"/> for the specified type.</returns>
//        /// <exception cref="ArgumentNullException">Thrown if <paramref name="typeToConvert"/> is null.</exception>
//        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
//        {
//            if (typeToConvert is null)
//                throw new ArgumentNullException(nameof(typeToConvert));

//            // Create an instance of IgnorableDataJsonConverter<T> using reflection.
//            var converterType = typeof(IgnorableDataJsonConverter<>).MakeGenericType(typeToConvert);
//            return (JsonConverter)Activator.CreateInstance(converterType);
//        }
//    }
//}
