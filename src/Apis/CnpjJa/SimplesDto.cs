using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UCode.Apis.CnpjJa
{
    public partial class SimplesDto
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
        [JsonPropertyName("simples")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public SimplesSimeiDto Simples { get; set; } = new SimplesSimeiDto();
        [JsonPropertyName("simei")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public SimplesSimeiDto Simei { get; set; } = new SimplesSimeiDto();

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
