using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace UCode.RedisDatabase.Serializer
{
    /// <summary>
    /// BSON serializer
    /// </summary>
    public sealed class BsonSerializer : ISerializer
    {
        private readonly Action<BsonDeserializationContext.Builder> _bsonDeserializationConfigurator;
        private readonly Action<BsonSerializationContext.Builder> _bsonSerializationConfigurator;

        public BsonSerializer()
        {

        }

        public BsonSerializer(
            Action<BsonDeserializationContext.Builder> bsonDeserializationConfigurator = null,
            Action<BsonSerializationContext.Builder> bsonSerializationConfigurator = null)
        {
            _bsonDeserializationConfigurator = bsonDeserializationConfigurator;
            _bsonSerializationConfigurator = bsonSerializationConfigurator;
        }

        /// <summary>
        /// JSON serialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public byte[]? Serialize<T>(T? source)
        {
            if (source == null)
            {
                return null;
            }

            if (source.Equals(default(T)))
            {
                return Array.Empty<byte>();
            }


            var bson = source.ToBsonDocument(configurator: _bsonSerializationConfigurator);

            return bson.ToBson();
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

            return MongoDB.Bson.Serialization.BsonSerializer.Deserialize<T>(source, _bsonDeserializationConfigurator);
        }
    }
}
