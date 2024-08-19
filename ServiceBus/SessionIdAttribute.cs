using System;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Attribute used to mark a property or field as the session ID for a Service Bus message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SessionIdAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionIdAttribute"/> class.
        /// </summary>
        public SessionIdAttribute()
        {
        }

        /// <summary>
        /// Gets the type ID of the attribute.
        /// </summary>
        /// <remarks>
        /// This property is used to uniquely identify the attribute type.
        /// The type ID is a string in the format "{Namespace}.{ClassName}".
        /// </remarks>
        public override object TypeId => $"{nameof(UCode)}.{nameof(ServiceBus)}.{nameof(SessionIdAttribute)}";
    }
}
