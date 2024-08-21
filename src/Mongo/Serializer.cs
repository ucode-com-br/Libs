using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace UCode.Mongo
{
    public class Serializer
    {
        private static readonly WeakReference<GuidSerializer> _guidSerializer;
        public static GuidSerializer GuidSerializer
        {
            get => _guidSerializer.TryGetTarget(out var target) ? target : new GuidSerializer(BsonType.String);
            private set
            {
                if (_guidSerializer.TryGetTarget(out var target))
                {
                    target = value;
                }
            }
        }

        static Serializer()
        {
            if (BsonSerializer.LookupSerializer<GuidSerializer>() == default)
            {
                BsonSerializer.RegisterSerializer(GuidSerializer);
            }
        }

        public static void ChangeGuidSerializer(GuidSerializer guidSerializer) => GuidSerializer = guidSerializer;

        public static void RegisterSerializer<T>(IBsonSerializer<T> serializer)
        {
            var current = BsonSerializer.LookupSerializer<T>();

            if (current == default)
            {
                BsonSerializer.RegisterSerializer(serializer);
            }
        }

    }
}
