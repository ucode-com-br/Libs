using System;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Specifies how the attributed property or field can be used.
    /// This attribute can be applied to properties and fields, 
    /// and it disallows multiple applications and allows inheritance.
    /// </summary>
    /// <remarks>
    /// The <c>AttributeUsage</c> attribute defines the usage 
    /// constraints for the custom attributes. In this case, 
    /// it is set to allow the attribute to be applied to properties 
    /// and fields only, while not allowing multiple instances 
    /// of the attribute to be applied to the same member. 
    /// Furthermore, this attribute can be inherited by derived classes.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    
    public class CorrelationIdAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdAttribute"/> class.
        /// This attribute is used to store correlation ID for tracking requests across various services.
        /// </summary>
        public CorrelationIdAttribute()
        {

        }

        /// <summary>
        /// Gets a unique identifier for the type represented by this attribute. 
        /// The identifier is a composite string consisting of the names of 
        /// the containing classes and the attribute itself.
        /// </summary>
        /// <returns>
        /// A string that represents a unique identifier for the attribute 
        /// based on its type and the namespace structure it belongs to.
        /// </returns>
        public override object TypeId => $"{nameof(UCode)}.{nameof(ServiceBus)}.{nameof(CorrelationIdAttribute)}";
    }
}
