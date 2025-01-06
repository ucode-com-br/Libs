using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    [JsonConverter(typeof(JsonStringEnumConverter<UF>))]
    public enum UF
    {

        [System.Runtime.Serialization.EnumMember(Value = @"BR")]
        [JsonStringEnumMemberName(@"BR")]
        BR = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"AC")]
        [JsonStringEnumMemberName(@"AC")]
        AC = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"AL")]
        [JsonStringEnumMemberName(@"AL")]
        AL = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"AM")]
        [JsonStringEnumMemberName(@"AM")]
        AM = 3,

        [System.Runtime.Serialization.EnumMember(Value = @"AP")]
        [JsonStringEnumMemberName(@"AP")]
        AP = 4,

        [System.Runtime.Serialization.EnumMember(Value = @"BA")]
        [JsonStringEnumMemberName(@"BA")]
        BA = 5,

        [System.Runtime.Serialization.EnumMember(Value = @"CE")]
        [JsonStringEnumMemberName(@"CE")]
        CE = 6,

        [System.Runtime.Serialization.EnumMember(Value = @"DF")]
        [JsonStringEnumMemberName(@"DF")]
        DF = 7,

        [System.Runtime.Serialization.EnumMember(Value = @"ES")]
        [JsonStringEnumMemberName(@"ES")]
        ES = 8,

        [System.Runtime.Serialization.EnumMember(Value = @"GO")]
        [JsonStringEnumMemberName(@"GO")]
        GO = 9,

        [System.Runtime.Serialization.EnumMember(Value = @"MA")]
        [JsonStringEnumMemberName(@"MA")]
        MA = 10,

        [System.Runtime.Serialization.EnumMember(Value = @"MG")]
        [JsonStringEnumMemberName(@"MG")]
        MG = 11,

        [System.Runtime.Serialization.EnumMember(Value = @"MS")]
        [JsonStringEnumMemberName(@"MS")]
        MS = 12,

        [System.Runtime.Serialization.EnumMember(Value = @"MT")]
        [JsonStringEnumMemberName(@"MT")]
        MT = 13,

        [System.Runtime.Serialization.EnumMember(Value = @"PA")]
        [JsonStringEnumMemberName(@"PA")]
        PA = 14,

        [System.Runtime.Serialization.EnumMember(Value = @"PB")]
        [JsonStringEnumMemberName(@"PB")]
        PB = 15,

        [System.Runtime.Serialization.EnumMember(Value = @"PE")]
        [JsonStringEnumMemberName(@"PE")]
        PE = 16,

        [System.Runtime.Serialization.EnumMember(Value = @"PI")]
        [JsonStringEnumMemberName(@"PI")]
        PI = 17,

        [System.Runtime.Serialization.EnumMember(Value = @"PR")]
        [JsonStringEnumMemberName(@"PR")]
        PR = 18,

        [System.Runtime.Serialization.EnumMember(Value = @"RJ")]
        [JsonStringEnumMemberName(@"RJ")]
        RJ = 19,

        [System.Runtime.Serialization.EnumMember(Value = @"RN")]
        [JsonStringEnumMemberName(@"RN")]
        RN = 20,

        [System.Runtime.Serialization.EnumMember(Value = @"RO")]
        [JsonStringEnumMemberName(@"RO")]
        RO = 21,

        [System.Runtime.Serialization.EnumMember(Value = @"RR")]
        [JsonStringEnumMemberName(@"RR")]
        RR = 22,

        [System.Runtime.Serialization.EnumMember(Value = @"RS")]
        [JsonStringEnumMemberName(@"RS")]
        RS = 23,

        [System.Runtime.Serialization.EnumMember(Value = @"SC")]
        [JsonStringEnumMemberName(@"SC")]
        SC = 24,

        [System.Runtime.Serialization.EnumMember(Value = @"SP")]
        [JsonStringEnumMemberName(@"SP")]
        SP = 25,

        [System.Runtime.Serialization.EnumMember(Value = @"SE")]
        [JsonStringEnumMemberName(@"SE")]
        SE = 26,

        [System.Runtime.Serialization.EnumMember(Value = @"TO")]
        [JsonStringEnumMemberName(@"TO")]
        TO = 27,

    }

}

