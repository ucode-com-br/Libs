using System;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Specifies the usage of a custom attribute that can be applied 
    /// to properties and fields in a class. This attribute can only 
    /// be used once on each member it is applied to, and it can be 
    /// inherited by derived classes.
    /// </summary>
    /// <remarks>
    /// AllowMultiple is set to false, meaning that 
    /// the attribute can only be applied a single time 
    /// to any property or field. The Inherited property is 
    /// set to true, allowing derived classes to inherit the 
    /// attribute from base classes.
    /// </remarks>
    /// <example>
    /// [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, 
    /// AllowMultiple = false, Inherited = true)]
    /// 
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ScheduledEnqueueTimeAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledEnqueueTimeAttribute"/> class.
        /// </summary>
        public ScheduledEnqueueTimeAttribute()
        {

        }

        /// <summary>
        /// Gets a unique identifier for the type, constructed from the names of the 
        /// UCode, ServiceBus, and ScheduledEnqueueTimeAttribute components.
        /// </summary>
        /// <returns>
        /// A string representation of the unique identifier for the type.
        /// </returns>
        public override object TypeId => $"{nameof(UCode)}.{nameof(ServiceBus)}.{nameof(ScheduledEnqueueTimeAttribute)}";
    }
}
