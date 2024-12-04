using System;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Specifies the usage of the attribute, indicating that it can be applied 
    /// to properties and fields only, with no multiple instances allowed. 
    /// This attribute can be inherited by derived classes.
    /// </summary>
    /// <remarks>
    /// The <see cref="AttributeUsageAttribute"/> is utilized to control how 
    /// the custom attribute can be applied in your codebase.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ApplicationPropertyAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationPropertyAttribute"/> class.
        /// </summary>
        public ApplicationPropertyAttribute()
        {

        }

        /// <summary>
        /// Gets a unique identifier for the attribute that combines the names of the 
        /// UCode, ServiceBus, and ApplicationPropertyAttribute classes.
        /// </summary>
        /// <returns>
        /// A string that represents the unique type identifier for this attribute.
        /// </returns>
        public override object TypeId => $"{nameof(UCode)}.{nameof(ServiceBus)}.{nameof(ApplicationPropertyAttribute)}";
    }
}
