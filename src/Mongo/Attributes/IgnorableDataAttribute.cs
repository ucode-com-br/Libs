using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.Formula.Functions;

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
