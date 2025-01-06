using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UCode.Apis.CnpjJa
{
    [JsonConverter(typeof(JsonStringEnumConverter<Strategy>))]
    public enum Strategy
    {

        [System.Runtime.Serialization.EnumMember(Value = @"ONLINE")]
        ONLINE = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"CACHE_IF_FRESH")]
        CACHE_IF_FRESH = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"CACHE_IF_ERROR")]
        CACHE_IF_ERROR = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"CACHE")]
        CACHE = 3,

    }
}
