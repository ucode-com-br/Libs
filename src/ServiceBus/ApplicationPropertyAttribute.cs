using System;

namespace UCode.ServiceBus
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ApplicationPropertyAttribute : Attribute
    {

        public ApplicationPropertyAttribute()
        {

        }

        public override object TypeId => $"{nameof(UCode)}.{nameof(ServiceBus)}.{nameof(ApplicationPropertyAttribute)}";
    }
}
