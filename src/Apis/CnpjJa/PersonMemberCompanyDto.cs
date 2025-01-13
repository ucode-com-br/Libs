using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class PersonMemberCompanyDto
    {
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Name
        {
            get; set;
        }
        [JsonPropertyName("jurisdiction")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Jurisdiction
        {
            get; set;
        }
        [JsonPropertyName("equity")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public float Equity
        {
            get; set;
        }
        [JsonPropertyName("nature")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public NatureDto Nature { get; set; } = new NatureDto();
        [JsonPropertyName("size")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public CompanySizeDto Size { get; set; } = new CompanySizeDto();

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

