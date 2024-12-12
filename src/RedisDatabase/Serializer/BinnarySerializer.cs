using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BinaryPack;

namespace UCode.RedisDatabase.Serializer
{
    /// <summary>
    /// Represents a binary serializer that implements the ISerializer interface.
    /// This class is responsible for serializing and deserializing objects
    /// in a binary format.
    /// </summary>
    /// <remarks>
    /// The <see cref="BinnarySerializer"/> is sealed, meaning it cannot be
    /// inherited. This is often done to maintain a consistent serialization
    /// behavior, ensuring that the methods cannot be overridden.
    /// </remarks>
    public sealed class BinnarySerializer : ISerializer
    {
        /// <summary>
        /// Represents a generic sealed class that can handle a specific type parameter.
        /// </summary>
        /// <typeparam name="T">The type of elements contained in the Pack.</typeparam>
        private sealed class Pack<T>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Pack"/> class.
            /// This constructor is used to create a new object of the <see cref="Pack"/> type.
            /// </summary>
            public Pack()
            {

            }

            /// <summary>
            /// Represents the content of the object.
            /// </summary>
            /// <typeparam name="T">The type of the content.</typeparam>
            /// <value>
            /// The content of type <typeparamref name="T"/>.
            /// </value>
            /// <remarks>
            /// This property allows for getting and setting the content for the object. 
            /// It is a generic property, meaning it can hold any data type specified by the user.
            /// </remarks>
            public T Content
            {
                get; set;
            }

            public static implicit operator T(Pack<T> pack) => pack.Content;

            public static implicit operator Pack<T>(T instance) => new() { Content = instance };
        }

        /// <summary>
        /// Serializes an object of type T into a byte array. 
        /// If the source object is null, it returns null. 
        /// If the source object equals the default value of T, 
        /// it returns an empty byte array.
        /// </summary>
        /// <typeparam name="T">The type of the source object to be serialized.</typeparam>
        /// <param name="source">The object to serialize. This can be null.</param>
        /// <returns>
        /// A byte array representation of the source object, 
        /// or null if the source is null, or an empty byte array if the source equals the default value of T.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public byte[]? Serialize<T>(T? source)
        {
            if (source == null)
            {
                return null;
            }

            if (source.Equals(default))
            {
                return Array.Empty<byte>();
            }

            return BinaryConverter.Serialize((Pack<T>)source);
        }

        /// <summary>
        /// Deserializes a byte array into an object of type T.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="source">The byte array containing the serialized data. This can be null or empty.</param>
        /// <returns>
        /// An object of type T if deserialization is successful, or the default value of type T if the source is null or empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public T? Deserialize<T>(byte[]? source)
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
