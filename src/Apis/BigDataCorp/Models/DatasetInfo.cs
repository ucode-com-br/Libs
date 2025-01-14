using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UCode.Apis.BigDataCorp.Models
{
    public struct DatasetInfo
    {
        internal DatasetInfo(string name, string className)
        {
            Name = name;
            ClassName = className;
        }
        public string Name;
        public string ClassName;

        public static bool operator ==(DatasetInfo a, DatasetInfo b) => a.Name == b.Name && a.ClassName == b.ClassName;

        public static bool operator !=(DatasetInfo a, DatasetInfo b) => a.Name != b.Name && a.ClassName != b.ClassName;


        //public static bool operator ==(DatasetInfo? a, DatasetInfo? b) => (a == null && b == null) || (a?.Name == b?.Name && a?.ClassName == b?.ClassName);

        //public static bool operator !=(DatasetInfo? a, DatasetInfo? b) => (a == null && b != null) || (a != null && b == null) || (a != null && b != null && a.Value.Name != b.Value.Name && a.Value.ClassName != b.Value.ClassName);

    }
}
