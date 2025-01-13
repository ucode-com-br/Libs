using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class AddressDto
    {
        private IDictionary<string, object> _additionalProperties;

        [JsonPropertyName("municipality")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Municipality
        {
            get; set;
        }

        [JsonPropertyName("street")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Street
        {
            get; set;
        }

        [JsonPropertyName("number")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Number
        {
            get; set;
        }

        [JsonPropertyName("district")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string District
        {
            get; set;
        }

        [JsonPropertyName("city")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string City
        {
            get; set;
        }

        [JsonPropertyName("state")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string State
        {
            get; set;
        }

        [JsonPropertyName("details")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Details
        {
            get; set;
        }

        [JsonPropertyName("zip")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [System.ComponentModel.DataAnnotations.StringLength(8, MinimumLength = 8)]
        public string Zip
        {
            get; set;
        }

        [JsonPropertyName("latitude")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? Latitude
        {
            get; set;
        }

        [JsonPropertyName("longitude")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? Longitude
        {
            get; set;
        }

        [JsonPropertyName("country")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public CountryDto Country { get; set; } = new CountryDto();

        

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

