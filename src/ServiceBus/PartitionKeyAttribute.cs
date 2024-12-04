using System;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Specifies the usage of the attribute on properties and fields within the program.
    /// This attribute can be applied only once to a member and is inherited by derived classes.
    /// </summary>
    /// <remarks>
    /// The <see cref="AttributeUsageAttribute"/> defines how and where the attribute can be applied.
    /// </remarks>
    /// <param name="AttributeTargets.Property | AttributeTargets.Field">Specifies that the attribute can be applied to properties and fields.</param>
    /// <param name="AllowMultiple = false">Indicates that multiple instances of this attribute cannot be applied to a single member.</param>
    /// <param name="Inherited = true">Specifies that the attribute can be inherited by derived classes.</param>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class PartitionKeyAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionKeyAttribute"/> class.
        /// </summary>
        public PartitionKeyAttribute()
        {

        }

        /// <summary>
        /// Gets the unique identifier for the type, represented as a string.
        /// The identifier is a concatenation of the names of the UCode, ServiceBus, and PartitionKeyAttribute classes.
        /// </summary>
        /// <value>
        /// A string representing the unique identifier for the type.
        /// </value>
        /// <returns>
        /// A string that uniquely identifies this type.
        /// </returns>
        public override object TypeId => $"{nameof(UCode)}.{nameof(ServiceBus)}.{nameof(PartitionKeyAttribute)}";
    }
}
