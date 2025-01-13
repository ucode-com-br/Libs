using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class MemberAgentDto
    {
        private IDictionary<string, object> _additionalProperties;

        [JsonPropertyName("person")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public PersonDto Person { get; set; } = new PersonDto();

        [JsonPropertyName("role")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public RoleDto Role { get; set; } = new RoleDto();

        

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

