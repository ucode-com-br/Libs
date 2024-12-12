using System;
using System.Runtime.CompilerServices;

namespace UCode.RedisDatabase.Serializer
{
    /// <summary>
    /// Represents a JSON serializer that can convert objects to and from JSON format.
    /// This class implements the ISerializer interface, providing functionality
    /// to serialize objects into JSON strings and deserialize JSON strings back
    /// into objects.
    /// </summary>
    /// <remarks>
    /// The JsonSerializer class is sealed, meaning it cannot be inherited.
    /// It is designed for scenarios where JSON data interchange is needed,
    /// such as web services or configuration settings.
    /// </remarks>
    public sealed class JsonSerializer : ISerializer
    {
        /// <summary>
        /// Serializes the given source object of type T into a byte array.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the source object to be serialized.
        /// </typeparam>
        /// <param name="source">
        /// The source object to serialize. If this is null, the method will return null.
        /// </param>
        /// <returns>
        /// A byte array containing the serialized data, or an empty array if the source object is 
        /// the default value for type T, or null if the source is null.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public byte[]? Serialize<T>(T source)
        {
            if (source == null)
            {
                return default;
            }

            if (source.Equals(default(T)))
            {
                return Array.Empty<byte>();
            }

            return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(source);
        }

        /// <summary>
        /// Deserializes a byte array into an object of type T using the System.Text.Json.JsonSerializer.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="source">A byte array representing the JSON formatted data to deserialize. 
        /// It can be null or empty, in which case the default value of T will be returned.</param>
        /// <returns>
        /// An object of type T that has been deserialized from the provided byte array, or the default value of T if the source is null or empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public T? Deserialize<T>(byte[]? source)
        {
            if (source == null || source.Length == 0)
            {
                return default;
            }

            var result = System.Text.Json.JsonSerializer.Deserialize<T>(source);

            return result;
        }
    }
}
