using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class RfbMemberDto
    {
        [JsonPropertyName("since")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Since
        {
            get; set;
        }
        [JsonPropertyName("role")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public RoleDto Role { get; set; } = new RoleDto();
        [JsonPropertyName("person")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public PersonBaseDto Person { get; set; } = new PersonBaseDto();
        [JsonPropertyName("agent")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public MemberAgentDto Agent { get; set; } = new MemberAgentDto();

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

