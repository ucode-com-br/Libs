//using System.Collections.Generic;
//using System.Text.Json.Serialization;

//namespace UCode.Apis.CnpjJa
//{
//    public partial class PersonBaseDto
//    {
//        private IDictionary<string, object> _additionalProperties;

//        [JsonPropertyName("id")]
//        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
//        public System.Guid Id
//        {
//            get; set;
//        }

//        [JsonPropertyName("type")]
//        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
//        public PersonBaseDtoType Type
//        {
//            get; set;
//        }

//        [JsonPropertyName("name")]
//        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
//        public string Name
//        {
//            get; set;
//        }

//        [JsonPropertyName("taxId")]
//        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
//        public string TaxId
//        {
//            get; set;
//        }

//        [JsonPropertyName("age")]
//        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
//        public Age Age
//        {
//            get; set;
//        }

//        [JsonPropertyName("country")]
//        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
//        public CountryDto Country
//        {
//            get; set;
//        }

        

//        [JsonExtensionData]
//        public IDictionary<string, object> AdditionalProperties
//        {
//            get
//            {
//                return _additionalProperties ?? (_additionalProperties = new Dictionary<string, object>());
//            }
//            set
//            {
//                _additionalProperties = value;
//            }
//        }

//    }


//}

