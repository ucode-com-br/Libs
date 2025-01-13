using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class SuframaActivityDto
    {
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }
        [JsonPropertyName("text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Text
        {
            get; set;
        }
        [JsonPropertyName("performed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool? Performed
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

