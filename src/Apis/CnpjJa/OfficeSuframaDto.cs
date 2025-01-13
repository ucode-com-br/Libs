using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class OfficeSuframaDto
    {
        [JsonPropertyName("number")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Number
        {
            get; set;
        }
        [JsonPropertyName("since")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Since
        {
            get; set;
        }
        [JsonPropertyName("approved")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool? Approved
        {
            get; set;
        }
        [JsonPropertyName("approvalDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string ApprovalDate
        {
            get; set;
        }
        [JsonPropertyName("status")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public SuframaStatusDto Status { get; set; } = new SuframaStatusDto();
        [JsonPropertyName("incentives")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public IList<SuframaIncentiveDto> Incentives { get; set; } = new List<SuframaIncentiveDto>();

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

