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
    /// Json Extensions
    /// </summary>
    public static class JsonExtensions
    {
        private static JsonSerializerOptions _globalSerializerOptions;


        /// <summary>
        /// Global serializer options
        /// </summary>
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
        /// Convert Json element to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <param name="options"></param>
        /// <returns></returns>
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
        /// Convert Json document to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [return: MaybeNull]
        public static T? JsonObject<T>([NotNull] this JsonDocument document, [MaybeNull] JsonSerializerOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(document);

            return document.RootElement.JsonObject<T>(options ?? GlobalSerializerOptions);
        }


        /// <summary>
        /// Convert Json element to object
        /// </summary>
        /// <param name="element"></param>
        /// <param name="returnType"></param>
        /// <param name="options"></param>
        /// <returns></returns>
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
        /// Convert Json document to object
        /// </summary>
        /// <param name="document"></param>
        /// <param name="returnType"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [return: MaybeNull]
        public static object? JsonObject([NotNull] this JsonDocument document, [NotNull] Type returnType, [MaybeNull] JsonSerializerOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(document);

            return document.RootElement.JsonObject(returnType, options ?? GlobalSerializerOptions);
        }


        /// <summary>
        /// Convert object to json string
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="source">object instance</param>
        /// <param name="options">json options</param>
        /// <returns></returns>
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
        /// Serialize object to utf8 byte array
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="source">object instance</param>
        /// <param name="options">json options</param>
        /// <returns></returns>
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
        /// Json string to object
        /// </summary>
        /// <typeparam name="T">type definition</typeparam>
        /// <param name="json">json string</param>
        /// <param name="options">deserialize options</param>
        /// <returns></returns>
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
        /// Json string to object
        /// </summary>
        /// <param name="json">json string</param>
        /// <param name="type">type definition</param>
        /// <param name="options">deserialize options</param>
        /// <returns></returns>
        [return: MaybeNull]
        public static object? JsonObject([MaybeNull] this string? json, Type type, [MaybeNull] JsonSerializerOptions? options = null)
        {
            if (json == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize(json, type, options ?? GlobalSerializerOptions);
        }


        public static void Populate<T>(this JsonObject jsonObject, T target)
        {
            // Converter o JsonObject para um JsonElement
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonObject.ToJsonString());

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
