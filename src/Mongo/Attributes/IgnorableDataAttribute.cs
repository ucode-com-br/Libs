using System;


namespace UCode.Mongo.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IgnorableDataAttribute : Attribute
    {
        private readonly object? _defaultValue;


        public IgnorableDataAttribute() : this(null) { }

        public IgnorableDataAttribute(object? defaultValue) => this._defaultValue = defaultValue;


        public object? DefaultValue => _defaultValue;
    }



}
