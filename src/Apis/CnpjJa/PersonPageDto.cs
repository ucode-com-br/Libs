using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class PersonPageDto
    {
        [JsonPropertyName("next")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [System.ComponentModel.DataAnnotations.StringLength(32, MinimumLength = 32)]
        public string Next
        {
            get; set;
        }

        [JsonPropertyName("limit")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [System.ComponentModel.DataAnnotations.Range(1D, 100D)]
        public double Limit
        {
            get; set;
        }

        [JsonPropertyName("count")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
        public double Count
        {
            get; set;
        }

        [JsonPropertyName("records")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public IList<PersonDto> Records { get; set; } = new List<PersonDto>();

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

