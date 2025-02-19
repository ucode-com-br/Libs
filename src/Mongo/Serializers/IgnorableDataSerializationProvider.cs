using System;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization;
using UCode.Mongo.Attributes;

namespace UCode.Mongo.Serializers
{
    /// <summary>
    /// A BSON serialization provider that returns an <see cref="IgnorableDataSerializer{T}"/>
    /// for types that have at least one property or field decorated with <see cref="IgnorableDataAttribute"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the <c>GetSerializer</c> method returns <c>null</c>, the default serializer is used.
    /// For example, if a type does not have any member marked with <see cref="IgnorableDataAttribute"/>,
    /// this provider returns <c>null</c> so that the standard MongoDB serializer is applied.
    /// </para>
    /// </remarks>
    public class IgnorableDataSerializationProvider : IBsonSerializationProvider
    {
        /// <summary>
        /// Gets a serializer for the given type if it contains any members decorated with <see cref="IgnorableDataAttribute"/>.
        /// </summary>
        /// <param name="type">The type to be serialized.</param>
        /// <returns>
        /// An instance of <see cref="IBsonSerializer"/> for the type if at least one member is marked with <see cref="IgnorableDataAttribute"/>;
        /// otherwise, <c>null</c> so that the default serializer will be used.
        /// </returns>
        /// <example>
        /// For the following type:
        /// <code>
        /// public class Company
        /// {
        ///     public string CompanyName { get; set; }
        ///     
        ///     [IgnorableData]
        ///     public double Value { get; set; }
        ///     
        ///     [IgnorableData]
        ///     public string City { get; set; }
        /// }
        /// </code>
        /// The provider will return an <see cref="IgnorableDataSerializer{Company}"/> because the properties
        /// <c>Value</c> and <c>City</c> are marked with <c>[IgnorableData]</c>. If a type has no such members,
        /// this method returns <c>null</c>.
        /// </example>
        public IBsonSerializer? GetSerializer(Type type)
        {
            // Only consider classes and structs.
            if (!type.IsClass && !type.IsValueType)
            {
                return null;
            }

            // Determine if the type has any public properties or fields with the IgnorableData attribute.
            var hasIgnorableMembers = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                           .Any(p => p.IsDefined(typeof(IgnorableDataAttribute), inherit: true))
                                       || type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                                              .Any(f => f.IsDefined(typeof(IgnorableDataAttribute), inherit: true));

            if (!hasIgnorableMembers)
            {
                // Return null so that the default serializer will be used.
                return null;
            }

            // Create an instance of IgnorableDataSerializer<T> using reflection.
            var serializerType = typeof(IgnorableDataSerializer<>).MakeGenericType(type);
            return (IBsonSerializer?)Activator.CreateInstance(serializerType);
        }
    }


}
