using System;

namespace UCode.ServiceBus
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)
    ]
    public class CorrelationIdAttribute : Attribute
    {

        public CorrelationIdAttribute()
        {

        }

        public override object TypeId => $"{nameof(UCode)}.{nameof(ServiceBus)}.{nameof(CorrelationIdAttribute)}";
    }
}
