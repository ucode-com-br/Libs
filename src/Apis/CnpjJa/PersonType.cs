using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    

    [JsonConverter(typeof(JsonStringEnumConverter<PersonType>))]
    public enum PersonType
    {

        [System.Runtime.Serialization.EnumMember(Value = @"LEGAL")]
        LEGAL = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"NATURAL")]
        NATURAL = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"FOREIGN")]
        FOREIGN = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"UNKNOWN")]
        UNKNOWN = 3,

    }


}

