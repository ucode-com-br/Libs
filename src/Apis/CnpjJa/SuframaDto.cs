using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class SuframaDto
    {
        [JsonPropertyName("taxId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string TaxId
        {
            get; set;
        }
        [JsonPropertyName("updated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Updated
        {
            get; set;
        }
        [JsonPropertyName("number")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Number
        {
            get; set;
        }
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Name
        {
            get; set;
        }
        [JsonPropertyName("since")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Since
        {
            get; set;
        }
        [JsonPropertyName("head")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool? Head
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
        [JsonPropertyName("nature")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public NatureDto Nature { get; set; } = new NatureDto();
        [JsonPropertyName("address")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public AddressDto Address { get; set; } = new AddressDto();
        [JsonPropertyName("phones")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public IList<PhoneDto> Phones { get; set; } = new List<PhoneDto>();
        [JsonPropertyName("emails")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public IList<EmailDto> Emails { get; set; } = new List<EmailDto>();
        [JsonPropertyName("mainActivity")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public SuframaActivityDto MainActivity { get; set; } = new SuframaActivityDto();
        [JsonPropertyName("sideActivities")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public IList<SuframaActivityDto> SideActivities { get; set; } = new List<SuframaActivityDto>();
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

