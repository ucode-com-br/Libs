using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

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

    }
}
