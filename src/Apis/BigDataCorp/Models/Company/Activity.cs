using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.BigDataCorp.Models.Company
{
    public class Activity
    {
        public bool IsMain
        {
            get; set;
        }

        public string Code
        {
            get; set;
        }

        [JsonPropertyName("Activity")]
        public string Name
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
