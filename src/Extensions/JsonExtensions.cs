using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UCode.Extensions
{
    /// <summary>
    /// Provides extension methods for working with JSON objects.
    /// </summary>
    /// <remarks>
    /// This class contains functionality to facilitate the manipulation and serialization of JSON data.
    /// </remarks>
    public static class JsonExtensions
    {
        private static JsonSerializerOptions _globalSerializerOptions;


        /// <summary>
        /// Gets or sets the global JsonSerializerOptions used for serialization.
        /// </summary>
        /// <value>
        /// The global <see cref="JsonSerializerOptions"/> instance configured with default settings for web serialization.
        /// </value>
        /// <remarks>
        /// The property initializes the global serializer options lazily, ensuring that a new instance is only created if one does not already exist.
        /// </remarks>
        [NotNull]
        public static JsonSerializerOptions GlobalSerializerOptions
        {
            get
            {
                _globalSerializerOptions ??= new JsonSerializerOptions(JsonSerializerDefaults.Web);

                return _globalSerializerOptions;
            }

            set => _globalSerializerOptions = value;
        }


        /// <summary>
        /// Converts a <see cref="JsonElement"/> to an object of type <typeparamref name="T"/> by serializing the JSON and deserializing it.
        /// </summary>
        /// <typeparam name="T">The type of the object to be deserialized from the JSON element.</typeparam>
        /// <param name="element">The <see cref="JsonElement"/> that contains the JSON data to convert.</param>
        /// <param name="options">Optional <see cref="JsonSerializerOptions"/> that can be used to customize the deserialization process. If <c>null</c>, global options will be used.</param>
        /// <returns>An instance of type <typeparamref name="T"/> resulting from deserialization, or <c>null</c> if the JSON is not of the expected type.</returns>
        [return: MaybeNull]
        public static T? JsonObject<T>([NotNull] this JsonElement element, [MaybeNull] JsonSerializerOptions? options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
            {
                element.WriteTo(writer);
            }

            return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options ?? GlobalSerializerOptions);
        }


        /// <summary>
        /// Parses the root element of a given <see cref="JsonDocument"/> and converts it into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object that the root element will be deserialized into.</typeparam>
        /// <param name="document">The <see cref="JsonDocument"/> containing the JSON to deserialize.</param>
        /// <param name="options">Optional <see cref="JsonSerializerOptions"/> to customize the serialization behavior.</param>
        /// <returns>
        /// An instance of <typeparamref name="T"/> representing the deserialized JSON object, or <c>null</c> if the JSON could not be deserialized 
        /// into the specified type <typeparamref name="T"/>.
        /// </returns>
        [return: MaybeNull]
        public static T? JsonObject<T>([NotNull] this JsonDocument document, [MaybeNull] JsonSerializerOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(document);

            return document.RootElement.JsonObject<T>(options ?? GlobalSerializerOptions);
        }


        /// <summary>
        /// Converts a <see cref="JsonElement"/> to a specified object type using JSON deserialization.
        /// </summary>
        /// <param name="element">
        /// The <see cref="JsonElement"/> instance to be converted.
        /// </param>
        /// <param name="returnType">
        /// The <see cref="Type"/> of the object to which the JSON should be deserialized.
        /// </param>
        /// <param name="options">
        /// Optional <see cref="JsonSerializerOptions"/> that can be used for customizing the deserialization process.
        /// If null, global serializer options will be applied.
        /// </param>
        /// <returns>
        /// An instance of the specified <paramref name="returnType"/> if deserialization is successful; otherwise, null.
        /// </returns>
        [return: MaybeNull]
        public static object? JsonObject([NotNull] this JsonElement element, [NotNull] Type returnType, [MaybeNull] JsonSerializerOptions? options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
            {
                element.WriteTo(writer);
            }

            return JsonSerializer.Deserialize(bufferWriter.WrittenSpan, returnType, options ?? GlobalSerializerOptions);
        }


        /// <summary>
        /// Converts the root element of the provided <see cref="JsonDocument"/> to an object of the specified type.
        /// </summary>
        /// <param name="document">The <see cref="JsonDocument"/> containing the JSON data to deserialize.</param>
        /// <param name="returnType">The <see cref="Type"/> of the object to convert the JSON to.</param>
        /// <param name="options">Optional <see cref="JsonSerializerOptions"/> to customize the serialization behavior; defaults to <see cref="GlobalSerializerOptions"/> if null.</param>
        /// <returns>
        /// An object of the specified <paramref name="returnType"/> representing the JSON data, or null if the conversion fails.
        /// </returns>
        [return: MaybeNull]
        public static object? JsonObject([NotNull] this JsonDocument document, [NotNull] Type returnType, [MaybeNull] JsonSerializerOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(document);

            return document.RootElement.JsonObject(returnType, options ?? GlobalSerializerOptions);
        }


        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="source">The object to serialize, which can be null.</param>
        /// <param name="options">Optional serializer options; if null, defaults to global serializer options.</param>
        /// <returns>
        /// A JSON string representation of the object, or null if the source is null.
        /// </returns>
        [return: MaybeNull]
        public static string? JsonString<T>([MaybeNull] this T? source, [MaybeNull] JsonSerializerOptions? options = null)
        {
            if (source == null)
            {
                return default;
            }

            return JsonSerializer.Serialize(source, options ?? GlobalSerializerOptions);
        }

        /// <summary>
        /// Serializes an object of type T to a byte array in JSON format.
        /// </summary>
        /// <param name="source">
        /// The object to be serialized. It can be null, in which case the method returns null.
        /// </param>
        /// <param name="options">
        /// Optional <see cref="JsonSerializerOptions"/> to customize the serialization process. 
        /// If null, the default global options will be used.
        /// </param>
        /// <returns>
        /// A byte array representing the serialized JSON, or null if the <paramref name="source"/> is null.
        /// </returns>
        [return: MaybeNull]
        public static byte[]? JsonBytes<T>([MaybeNull] this T? source, [MaybeNull] JsonSerializerOptions? options = null)
        {
            if (source == null)
            {
                return default;
            }

            return JsonSerializer.SerializeToUtf8Bytes(source, options ?? GlobalSerializerOptions);
        }


        /// <summary>
        /// Deserializes a JSON string into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to which the JSON string will be deserialized.</typeparam>
        /// <param name="json">The JSON string to deserialize. It can be null, in which case the default value for type T is returned.</param>
        /// <param name="options">Optional serializer options that may be used to control the deserialization process. If null, global serializer options are used.</param>
        /// <returns>
        /// An object of type T representing the deserialized JSON string, or the default value for type T if the input JSON string is null.
        /// </returns>
        [return: MaybeNull]
        public static T? JsonObject<T>([MaybeNull] this string json, [MaybeNull] JsonSerializerOptions? options = null)
        {
            if (json == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json, options ?? GlobalSerializerOptions);
        }


        /// <summary>
        /// Deserializes a JSON string into an object of the specified type.
        /// </summary>
        /// <param name="json">The JSON string to deserialize. This parameter can be null.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <param name="options">Optional <see cref="JsonSerializerOptions"/> that can be used to customize the deserialization process. This parameter can be null.</param>
        /// <returns>
        /// An object of the specified <paramref name="type"/> deserialized from the JSON string, or null if <paramref name="json"/> is null.
        /// </returns>
        [return: MaybeNull]
        public static object? JsonObject([MaybeNull] this string? json, Type type, [MaybeNull] JsonSerializerOptions? options = null)
        {
            if (json == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize(json, type, options ?? GlobalSerializerOptions);
        }


        /// <summary>
        /// Populates the properties of the specified target object with values from a given <see cref="JsonElement"/>.
        /// This method uses reflection to match properties of the target object with the corresponding properties in the JSON element.
        /// If a property is a class (excluding strings), it recursively populates the property using the same method.
        /// </summary>
        /// <typeparam name="T">The type of the target object which should match the properties in the <see cref="JsonElement"/>.</typeparam>
        /// <param name="jsonElement">The <see cref="JsonElement"/> from which to populate the values.</param>
        /// <param name="target">The target object whose properties will be populated.</param>
        public static void Populate<T>(this JsonElement jsonElement, T target)
        {
            // Obter o tipo do objeto alvo
            var targetType = typeof(T);

            // Iterar sobre as propriedades do objeto alvo
            foreach (var property in targetType.GetProperties())
            {
                // Verificar se a propriedade existe no JsonElement
                if (jsonElement.TryGetProperty(property.Name, out var propertyValue))
                {
                    // Obter o valor da propriedade do JsonElement
                    var value = GetValueFromJsonElement(propertyValue, property.PropertyType);

                    // Se a propriedade for uma classe, record ou struct, chamar recursivamente Populate
                    if (value is JsonObject nestedJsonObject && property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        var nestedInstance = Activator.CreateInstance(property.PropertyType);
                        Populate(nestedJsonObject, nestedInstance);
                        value = nestedInstance;
                    }

                    // Definir o valor da propriedade no objeto alvo
                    property.SetValue(target, value);
                }
            }
        }

        /// <summary>
        /// Populates an object of type T with values from a JsonObject.
        /// This method uses reflection to match the properties of the target object 
        /// with the properties contained within the JsonObject and sets the values accordingly.
        /// If a property is a complex type (class, record, or struct), 
        /// it recursively populates that property as well.
        /// </summary>
        /// <typeparam name="T">The type of the target object to populate.</typeparam>
        /// <param name="jsonObject">The JsonObject containing the data to populate the target object.</param>
        /// <param name="target">The target object that will receive the populated values.</param>
        public static void Populate<T>(this JsonObject jsonObject, T target)
        {
            // Converter o JsonObject para um JsonElement
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonObject.ToJsonString());

            // Obter o tipo do objeto alvo
            var targetType = target.GetType();

            // Iterar sobre as propriedades do objeto alvo
            foreach (var property in targetType.GetProperties())
            {
                // Verificar se a propriedade existe no JsonElement
                if (jsonElement.TryGetProperty(property.Name, out var propertyValue))
                {
                    // Obter o valor da propriedade do JsonElement
                    var value = GetValueFromJsonElement(propertyValue, property.PropertyType);

                    // Se a propriedade for uma classe, record ou struct, chamar recursivamente Populate
                    if (value is JsonObject nestedJsonObject && property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        var nestedInstance = Activator.CreateInstance(property.PropertyType);
                        Populate(nestedJsonObject, nestedInstance);
                        value = nestedInstance;
                    }

                    // Definir o valor da propriedade no objeto alvo
                    property.SetValue(target, value);
                }
            }
        }

        /// <summary>
        /// Retrieves a value from a JsonElement and converts it to the specified target type.
        /// Handles various types including primitives, arrays, and JsonObjects.
        /// </summary>
        /// <param name="jsonElement">The JsonElement from which the value is retrieved.</param>
        /// <param name="targetType">The Type to which the value should be converted.</param>
        /// <returns>
        /// An object of the specified targetType containing the value extracted from the jsonElement,
        /// or the raw JSON string if conversion is not possible.
        /// </returns>
        private static object? GetValueFromJsonElement(JsonElement jsonElement, Type targetType)
        {
            // Converter o JsonElement para o tipo da propriedade
            if (targetType == typeof(string))
            {
                return jsonElement.GetString();
            }
            else if (targetType == typeof(int))
            {
                return jsonElement.GetInt32();
            }
            else if (targetType == typeof(bool))
            {
                return jsonElement.GetBoolean();
            }
            else if (targetType == typeof(double))
            {
                return jsonElement.GetDouble();
            }
            else if (targetType == typeof(float))
            {
                return jsonElement.GetSingle();
            }
            else if (targetType == typeof(long))
            {
                return jsonElement.GetInt64();
            }
            else if (targetType == typeof(short))
            {
                return jsonElement.GetInt16();
            }
            else if (targetType == typeof(byte))
            {
                return jsonElement.GetByte();
            }
            else if (targetType == typeof(char))
            {
                return jsonElement.GetString()[0];
            }
            else if (targetType == typeof(DateTime))
            {
                return jsonElement.GetDateTime();
            }
            else if (targetType == typeof(DateOnly))
            {
                return DateOnly.Parse(jsonElement.GetString());
            }
            else if (targetType == typeof(DateTimeOffset))
            {
                return jsonElement.GetDateTimeOffset();
            }
            else if (targetType == typeof(Guid))
            {
                return jsonElement.GetGuid();
            }
            else if (targetType.IsArray)
            {
                var elementType = targetType.GetElementType()!;
                var array = jsonElement.EnumerateArray().Select(e => GetValueFromJsonElement(e, elementType)).ToArray();
                var newArray = Array.CreateInstance(elementType, array.Length);
                Array.Copy(array, newArray, array.Length);
                return newArray;
            }
            else if (targetType == typeof(JsonObject))
            {
                var jsonObject = System.Text.Json.Nodes.JsonObject.Create(jsonElement);

                if (TryImplicit(targetType, typeof(JsonObject), jsonObject, out var resultJsonObjectImplicty))
                {
                    return resultJsonObjectImplicty;
                }
                if (TryExplicity(targetType, typeof(JsonObject), jsonObject, out var resultJsonObjectExplicity))
                {
                    return resultJsonObjectExplicity;
                }
            }
            else if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(IList<>))
            {
                var elementType = targetType.GetGenericArguments()[0];

                var list = jsonElement.EnumerateArray().Select(e => JsonSerializer.Deserialize(e.GetRawText(), elementType)).ToList();

                var typedList = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;

                var addMethod = typedList.GetType().GetMethod("Add")!;

                foreach (var item in list)
                {
                    addMethod.Invoke(typedList, [item]);
                }
                return typedList;
            }

            // Tentar converter usando Convert
            try
            {
                var converted = Convert.ChangeType(jsonElement.GetString(), targetType);

                if (converted != null)
                    return converted;
            }
            catch
            {
                // Ignorar exceção e tentar conversão implícita ou explícita
            }

            if (TryImplicit(targetType, typeof(string), jsonElement.GetString(), out var resultImplicty))
            {
                return resultImplicty;
            }
            if (TryExplicity(targetType, typeof(string), jsonElement.GetString(), out var resultExplicity))
            {
                return resultExplicity;
            }

            // Retornar o valor do JsonElement como JSON
            return jsonElement.GetRawText();
        }

        /// <summary>
        /// Attempts to find and invoke an implicit conversion operator for the specified types.
        /// </summary>
        /// <param name="instanceType">The type that contains the implicit operator.</param>
        /// <param name="to">The target type to which the value should be converted.</param>
        /// <param name="value">The value to be converted.</param>
        /// <param name="result">When this method returns, contains the converted value if the conversion was successful; otherwise, null.</param>
        /// <returns>
        /// Returns true if the implicit conversion was successful; otherwise, false.
        /// </returns>
        private static bool TryImplicit(Type instanceType, Type to, object value, out object? result)
        {
            var implicitOperator = instanceType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                               .FirstOrDefault(m => m.Name == "op_Implicit" && m.ReturnType == instanceType && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == to);

            if (implicitOperator != null)
            {
                result = implicitOperator.Invoke(null, new object[] { value });

                return true;
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Attempts to explicitly convert a value of a specified type to another type using
        /// a user-defined explicit conversion operator (if available).
        /// </summary>
        /// <param name="instanceType">The type of the instance to check for an explicit conversion operator.</param>
        /// <param name="to">The type to which the value should be converted.</param>
        /// <param name="value">The value to be converted.</param>
        /// <param name="result">The converted value, or null if the conversion was unsuccessful.</param>
        /// <returns>
        /// Returns true if the explicit conversion was successful, otherwise false.
        /// </returns>
        private static bool TryExplicity(Type instanceType, Type to, object value, out object? result)
        {
            var explicitOperator = instanceType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                               .FirstOrDefault(m => m.Name == "op_Explicit" && m.ReturnType == instanceType && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == to);

            if (explicitOperator != null)
            {
                result = explicitOperator.Invoke(null, new object[] { value });

                return true;
            }

            result = null;
            return false;
        }

    }
}
