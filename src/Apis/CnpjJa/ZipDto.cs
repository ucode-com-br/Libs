using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class ZipDto
    {
        private IDictionary<string, object> _additionalProperties;

        [JsonPropertyName("updated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Updated
        {
            get; set;
        }

        [JsonPropertyName("municipality")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Municipality
        {
            get; set;
        }

        [JsonPropertyName("code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [System.ComponentModel.DataAnnotations.StringLength(8, MinimumLength = 8)]
        public string Code
        {
            get; set;
        }

        [JsonPropertyName("street")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Street
        {
            get; set;
        }

        [JsonPropertyName("number")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Number
        {
            get; set;
        }

        [JsonPropertyName("district")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string District
        {
            get; set;
        }

        [JsonPropertyName("city")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string City
        {
            get; set;
        }

        [JsonPropertyName("state")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string State
        {
            get; set;
        }

        

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

}

