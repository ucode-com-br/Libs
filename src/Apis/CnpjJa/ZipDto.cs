using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class ZipDto
    {
        [JsonPropertyName("updated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Updated
        {
            get; set;
        }
        [JsonPropertyName("municipality")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Municipality
        {
            get; set;
        }
        [JsonPropertyName("code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        [System.ComponentModel.DataAnnotations.StringLength(8, MinimumLength = 8)]
        public string Code
        {
            get; set;
        }
        [JsonPropertyName("street")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Street
        {
            get; set;
        }
        [JsonPropertyName("number")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Number
        {
            get; set;
        }
        [JsonPropertyName("district")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string District
        {
            get; set;
        }
        [JsonPropertyName("city")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string City
        {
            get; set;
        }
        [JsonPropertyName("state")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public UF State
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

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

