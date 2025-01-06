using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class SuframaIncentiveDto
    {
        [JsonPropertyName("tribute")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public SuframaIncentiveDtoTribute Tribute
        {
            get; set;
        }
        [JsonPropertyName("benefit")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Benefit
        {
            get; set;
        }
        [JsonPropertyName("purpose")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Purpose
        {
            get; set;
        }
        [JsonPropertyName("basis")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Basis
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

