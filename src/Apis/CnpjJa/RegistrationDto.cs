using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class RegistrationDto
    {
        [JsonPropertyName("number")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        [System.ComponentModel.DataAnnotations.StringLength(14, MinimumLength = 8)]
        public string Number
        {
            get; set;
        }
        [JsonPropertyName("state")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public UF State
        {
            get; set;
        }
        [JsonPropertyName("enabled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool? Enabled
        {
            get; set;
        }
        [JsonPropertyName("statusDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string StatusDate
        {
            get; set;
        }
        [JsonPropertyName("status")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public RegistrationStatusDto Status { get; set; } = new RegistrationStatusDto();
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public RegistrationTypeDto Type { get; set; } = new RegistrationTypeDto();

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

