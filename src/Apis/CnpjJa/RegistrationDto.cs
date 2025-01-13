using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class RegistrationDto
    {
        private IDictionary<string, object> _additionalProperties;

        [JsonPropertyName("number")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [System.ComponentModel.DataAnnotations.StringLength(14, MinimumLength = 8)]
        public string Number
        {
            get; set;
        }

        [JsonPropertyName("state")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string State
        {
            get; set;
        }

        [JsonPropertyName("enabled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Enabled
        {
            get; set;
        }

        [JsonPropertyName("statusDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string StatusDate
        {
            get; set;
        }

        [JsonPropertyName("status")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public RegistrationStatusDto Status { get; set; } = new RegistrationStatusDto();

        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public RegistrationTypeDto Type { get; set; } = new RegistrationTypeDto();

        

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

