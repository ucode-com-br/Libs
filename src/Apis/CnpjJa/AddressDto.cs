using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class AddressDto
    {
        [JsonPropertyName("municipality")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Municipality
        {
            get; set;
        }
        [JsonPropertyName("street")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Street
        {
            get; set;
        }
        [JsonPropertyName("number")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Number
        {
            get; set;
        }
        [JsonPropertyName("district")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string District
        {
            get; set;
        }
        [JsonPropertyName("city")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string City
        {
            get; set;
        }
        [JsonPropertyName("state")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public UF State
        {
            get; set;
        }
        [JsonPropertyName("details")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Details
        {
            get; set;
        }
        [JsonPropertyName("zip")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        [System.ComponentModel.DataAnnotations.StringLength(8, MinimumLength = 8)]
        public string Zip
        {
            get; set;
        }
        [JsonPropertyName("latitude")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float Latitude
        {
            get; set;
        }
        [JsonPropertyName("longitude")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float Longitude
        {
            get; set;
        }
        [JsonPropertyName("country")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public CountryDto Country { get; set; } = new CountryDto();

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

