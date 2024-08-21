using System;

namespace UCode.ServiceBus
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)
    ]
    public class MessageIdAttribute : Attribute
    {

        public MessageIdAttribute()
        {

        }

        public override object TypeId => $"{nameof(UCode)}.{nameof(ServiceBus)}.{nameof(MessageIdAttribute)}";
    }
}
