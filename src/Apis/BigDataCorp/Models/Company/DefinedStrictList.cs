using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.BigDataCorp.Models.Company
{
    public class DefinedStrictList<T>
    {
        public T? Primary
        {
            get; set;
        }
        public T? Secondary
        {
            get; set;
        }
        public T? Tertiary
        {
            get; set;
        }

        /// <summary>
        /// Not mappedd properties
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> ExtensionData { get; set; } = new Dictionary<string, object>();
    }
}
