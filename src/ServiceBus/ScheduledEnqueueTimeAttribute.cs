using System;

namespace UCode.ServiceBus
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)
    ]
    public class ScheduledEnqueueTimeAttribute : Attribute
    {

        public ScheduledEnqueueTimeAttribute()
        {

        }

        public override object TypeId => $"{nameof(UCode)}.{nameof(ServiceBus)}.{nameof(ScheduledEnqueueTimeAttribute)}";
    }
}
