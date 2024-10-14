using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace UCode.RedisDatabase.Serializer
{
    /// <summary>
    /// JSON serializer
    /// </summary>
    public sealed class JsonSerializer : ISerializer
    {
        /// <summary>
        /// JSON serialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public byte[] Serialize<T>(T source)
        {
            if (source == null || source.Equals(default(T)))
            {
                return Array.Empty<byte>();
            }

            return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(source);
        }

        /// <summary>
        /// JSON deserialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
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
