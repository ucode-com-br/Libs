using System;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Specifies the usage of the attribute on properties or fields.
    /// This attribute can be applied to properties and fields, 
    /// and it cannot be applied multiple times to the same member. 
    /// It can be inherited by derived classes.
    /// </summary>
    /// <remarks>
    /// The <see cref="AttributeUsageAttribute"/> is used to specify the valid 
    /// targets for this attribute and how it interacts with derived classes.
    /// </remarks>
    /// <param name="AttributeTargets.Property|AttributeTargets.Field">Indicates that the attribute can be applied to both properties and fields.</param>
    /// <param name="AllowMultiple=false">Indicates that multiple instances of the attribute cannot be applied to a single member.</param>
    /// <param name="Inherited=true">Indicates that the attribute can be inherited by derived classes.</param>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    
    public class MessageIdAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageIdAttribute"/> class.
        /// </summary>
        public MessageIdAttribute()
        {

        }

        /// <summary>
        /// Gets a unique identifier for the type represented by this attribute.
        /// The identifier is composed of the names of the class and its enclosing structures.
        /// </summary>
        /// <returns>
        /// A string that represents the unique type identifier, formatted as 
        /// "UCode.ServiceBus.MessageIdAttribute".
        /// </returns>
        public override object TypeId => $"{nameof(UCode)}.{nameof(ServiceBus)}.{nameof(MessageIdAttribute)}";
    }
}
