using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class PhoneDto
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public PhoneDtoType Type
        {
            get; set;
        }
        [JsonPropertyName("area")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        [System.ComponentModel.DataAnnotations.StringLength(2, MinimumLength = 2)]
        public string Area
        {
            get; set;
        }
        [JsonPropertyName("number")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        [System.ComponentModel.DataAnnotations.StringLength(9, MinimumLength = 8)]
        public string Number
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

