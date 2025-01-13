using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{

    public partial class SimplesSimeiHistoryDto
    {
        [JsonPropertyName("from")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string From
        {
            get; set;
        }
        [JsonPropertyName("until")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Until
        {
            get; set;
        }
        [JsonPropertyName("text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Text
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

