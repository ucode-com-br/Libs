using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class PersonDto
    {
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public System.Guid Id
        {
            get; set;
        }
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public PersonDtoType Type
        {
            get; set;
        }
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Name
        {
            get; set;
        }
        [JsonPropertyName("taxId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string TaxId
        {
            get; set;
        }
        [JsonPropertyName("age")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

        public Age Age
        {
            get; set;
        }
        [JsonPropertyName("country")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CountryDto Country
        {
            get; set;
        }
        [JsonPropertyName("membership")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public IList<PersonMemberDto> Membership { get; set; } = new List<PersonMemberDto>();

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

