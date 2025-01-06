using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class MemberDto
    {
        [JsonPropertyName("since")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Since
        {
            get; set;
        }
        [JsonPropertyName("person")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public PersonBaseDto Person { get; set; } = new PersonBaseDto();
        [JsonPropertyName("role")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public RoleDto Role { get; set; } = new RoleDto();
        [JsonPropertyName("agent")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MemberAgentDto Agent
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

