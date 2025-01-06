using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class EmailDto
    {
        [JsonPropertyName("ownership")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public EmailDtoOwnership Ownership
        {
            get; set;
        }
        [JsonPropertyName("address")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Address
        {
            get; set;
        }
        [JsonPropertyName("domain")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Domain
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

