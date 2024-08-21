using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace UCode.Mongo
{
    public class Serializer
    {
        /// Weak reference to the GuidSerializer instance.
        private static readonly WeakReference<GuidSerializer> _guidSerializer;

        /// <summary>
        /// Gets or sets the GuidSerializer instance.
        /// If the instance is not yet created, it creates a new one.
        /// </summary>
        public static GuidSerializer GuidSerializer
        {
            // Try to get the target of the weak reference
            // If the target exists, return it
            get => _guidSerializer.TryGetTarget(out var target) ? target : new GuidSerializer(BsonType.String);
            private set
            {
                // Try to get the target of the weak reference
                if (_guidSerializer.TryGetTarget(out var target))
                {
                    // If the target exists, update it with the new value
                    target = value;
                }
            }
        }

        /// <summary>
        /// Static constructor that registers a GuidSerializer instance if it hasn't already been registered.
        /// </summary>
        static Serializer()
        {
            // Check if the GuidSerializer has already been registered
            if (BsonSerializer.LookupSerializer<GuidSerializer>() == default)
            {
                // If not, create a new instance and register it
                BsonSerializer.RegisterSerializer(GuidSerializer);
            }
        }

        /// <summary>
        /// Sets the GuidSerializer instance to the provided value.
        /// If the provided value is null, a new GuidSerializer instance is created with the default BsonType.
        /// </summary>
        /// <param name="guidSerializer">The GuidSerializer instance to set.</param>
        public static void ChangeGuidSerializer(GuidSerializer guidSerializer) => GuidSerializer = guidSerializer;

        /// <summary>
        /// Registers the provided serializer for the specified type.
        /// If a serializer has already been registered for the specified type, the provided serializer is ignored.
        /// </summary>
        /// <typeparam name="T">The type to register the serializer for.</typeparam>
        /// <param name="serializer">The serializer to register.</param>
        public static void RegisterSerializer<T>(IBsonSerializer<T> serializer)
        {
            // Get the current serializer for the specified type
            var current = BsonSerializer.LookupSerializer<T>();

            // If no serializer has been registered for the specified type, register the provided serializer
            if (current == default)
            {
                BsonSerializer.RegisterSerializer(serializer);
            }
        }

    }
}
