using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class OfficeDto
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
        [JsonPropertyName("company")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public OfficeCompanyDto Company { get; set; } = new OfficeCompanyDto();
        [JsonPropertyName("alias")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Alias
        {
            get; set;
        }
        [JsonPropertyName("founded")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Founded
        {
            get; set;
        }
        [JsonPropertyName("head")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool? Head
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

        public OfficeStatusDto Status { get; set; } = new OfficeStatusDto();
        [JsonPropertyName("reason")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OfficeReasonDto Reason
        {
            get; set;
        }
        [JsonPropertyName("specialDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SpecialDate
        {
            get; set;
        }
        [JsonPropertyName("special")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OfficeSpecialDto Special
        {
            get; set;
        }
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

        public ActivityDto MainActivity { get; set; } = new ActivityDto();
        [JsonPropertyName("sideActivities")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public IList<ActivityDto> SideActivities { get; set; } = new List<ActivityDto>();
        [JsonPropertyName("registrations")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<RegistrationDto> Registrations
        {
            get; set;
        }
        [JsonPropertyName("suframa")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OfficeSuframaDto> Suframa
        {
            get; set;
        }
        [JsonPropertyName("links")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OfficeLinkDto> Links
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

