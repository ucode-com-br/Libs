using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace UCode.Mongo
{
    /// <summary>
    /// The <see cref="Serializer"/> class provides methods for serializing and deserializing objects.
    /// This class is intended for use in scenarios where object persistence is required.
    /// </summary>
    public class Serializer
    {
        /// <summary>
        /// Weak reference to the GuidSerializer instance.
        /// </summary>
        private static readonly WeakReference<GuidSerializer> _guidSerializer;

        /// <summary>
        /// Gets or sets the <see cref="GuidSerializer"/> instance.
        /// If the target of the weak reference exists, it returns that instance, otherwise it creates a new instance 
        /// of <see cref="GuidSerializer"/> with BsonType.String.
        /// </summary>
        /// <value>
        /// A <see cref="GuidSerializer"/> instance associated with the property.
        /// </value>
        /// <returns>
        /// If the target exists, returns the existing <see cref="GuidSerializer"/>; otherwise, returns a new instance.
        /// </returns>
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
        /// Static constructor for the Serializer class.
        /// This constructor checks if the GuidSerializer is already registered
        /// with the BsonSerializer. If it is not registered, it creates a new 
        /// instance of GuidSerializer and registers it.
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
        /// Sets the specified <see cref="GuidSerializer"/> instance as the current serializer.
        /// This method allows for changing the default behavior of GUID serialization
        /// throughout the application.
        /// </summary>
        /// <param name="guidSerializer">
        /// The <see cref="GuidSerializer"/> instance to set as the current serializer.
        /// </param>
        public static void ChangeGuidSerializer(GuidSerializer guidSerializer) => GuidSerializer = guidSerializer;

        /// <summary>
        /// Registers a specified BSON serializer for a given type.
        /// </summary>
        /// <typeparam name="T">The type for which the BSON serializer is being registered.</typeparam>
        /// <param name="serializer">The BSON serializer to be registered for the specified type.</param>
        /// <remarks>
        /// This method checks if a serializer is already registered for the type <typeparamref name="T"/>. 
        /// If no serializer is currently registered, it registers the provided <paramref name="serializer"/>.
        /// </remarks>
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
