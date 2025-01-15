using System;

namespace UCode.Apis.BigDataCorp.Models
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DatasetInfoAttribute : Attribute
    {
        public DatasetInfoAttribute(string name, string className)
        {
            Name = name;
            ClassName = className;
        }

        public string Name;

        public string ClassName;

        public static implicit operator DatasetInfo(DatasetInfoAttribute source)
        {
            return new DatasetInfo(source.Name, source.ClassName);
        }
    }
}
