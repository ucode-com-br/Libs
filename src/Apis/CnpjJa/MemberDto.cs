using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class MemberDto
    {
        private IDictionary<string, object> _additionalProperties;

        [JsonPropertyName("since")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string Since
        {
            get; set;
        }

        [JsonPropertyName("person")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public PersonDto Person { get; set; } = new PersonDto();

        [JsonPropertyName("role")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public RoleDto Role { get; set; } = new RoleDto();

        [JsonPropertyName("agent")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public MemberAgentDto Agent
        {
            get; set;
        }

        

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

