using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UCode.Apis.CnpjJa
{
    public partial class SimplesSimeiDto
    {
        [JsonPropertyName("optant")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool? Optant
        {
            get; set;
        }
        [JsonPropertyName("since")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Since
        {
            get; set;
        }
        [JsonPropertyName("history")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<SimplesSimeiHistoryDto> History
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
