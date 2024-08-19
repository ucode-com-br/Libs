using System;
using System.Diagnostics.CodeAnalysis;
using BinaryPack;

namespace UCode.RedisDatabase.Serializer
{
    /// <summary>
    /// Binnary Serializer
    /// </summary>
    public sealed class BinnarySerializer : ISerializer
    {
        /// <summary>
        /// Package to hide class new() serializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private sealed class Pack<T>
        {
            public Pack()
            {

            }

            public T Content
            {
                get; set;
            }

            public static implicit operator T(Pack<T> pack) => pack.Content;

            public static implicit operator Pack<T>(T instance) => new() { Content = instance };
        }

        /// <summary>
        /// Serialize a object to a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        [return: NotNull]
        public byte[] Serialize<T>([NotNull] T source)
        {
            if (source == null || source.Equals(default))
            {
                return Array.Empty<byte>();
            }

            return BinaryConverter.Serialize((Pack<T>)source);
        }

        /// <summary>
        /// Deserialize a byte array to a object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        [return: NotNull]
        public T Deserialize<T>([NotNull] byte[] source)
        {
            if (source == null || source.Length == 0)
            {
                return default;
            }

            var result = BinaryConverter.Deserialize<Pack<T>>(source);

            return result;
        }
    }
}
