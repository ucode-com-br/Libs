using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Text.Json;

namespace UCode.Apis.CnpjJa
{

    [JsonConverter(typeof(JsonStringEnumConverter<PhonesType>))]
    public enum PhonesType
    {

        [System.Runtime.Serialization.EnumMember(Value = @"LANDLINE")]
        LANDLINE = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"MOBILE")]
        MOBILE = 1,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<EmailOwnership>))]
    public enum EmailOwnership
    {

        [System.Runtime.Serialization.EnumMember(Value = @"ACCOUNTING")]
        ACCOUNTING = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"CORPORATE")]
        CORPORATE = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"PERSONAL")]
        PERSONAL = 2,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<MapType>))]
    public enum MapType
    {

        [System.Runtime.Serialization.EnumMember(Value = @"roadmap")]
        Roadmap = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"terrain")]
        Terrain = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"satellite")]
        Satellite = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"hybrid")]
        Hybrid = 3,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<PhoneDtoType>))]
    public enum PhoneDtoType
    {

        [System.Runtime.Serialization.EnumMember(Value = @"LANDLINE")]
        LANDLINE = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"MOBILE")]
        MOBILE = 1,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<EmailDtoOwnership>))]
    public enum EmailDtoOwnership
    {

        [System.Runtime.Serialization.EnumMember(Value = @"ACCOUNTING")]
        ACCOUNTING = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"CORPORATE")]
        CORPORATE = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"PERSONAL")]
        PERSONAL = 2,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<SuframaIncentiveDtoTribute>))]
    public enum SuframaIncentiveDtoTribute
    {

        [System.Runtime.Serialization.EnumMember(Value = @"ICMS")]
        ICMS = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"IPI")]
        IPI = 1,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<PersonBaseDtoType>))]
    public enum PersonBaseDtoType
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

    [JsonConverter(typeof(JsonStringEnumConverter<PersonDtoType>))]
    public enum PersonDtoType
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
    [JsonConverter(typeof(JsonStringEnumConverter<OfficeLinkDtoType>))]
    public enum OfficeLinkDtoType
    {

        [System.Runtime.Serialization.EnumMember(Value = @"RFB_CERTIFICATE")]
        RFB_CERTIFICATE = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"SIMPLES_CERTIFICATE")]
        SIMPLES_CERTIFICATE = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"CCC_CERTIFICATE")]
        CCC_CERTIFICATE = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"SUFRAMA_CERTIFICATE")]
        SUFRAMA_CERTIFICATE = 3,

        [System.Runtime.Serialization.EnumMember(Value = @"OFFICE_MAP")]
        OFFICE_MAP = 4,

        [System.Runtime.Serialization.EnumMember(Value = @"OFFICE_STREET")]
        OFFICE_STREET = 5,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<LegacyRegistrationDtoStatus>))]
    public enum LegacyRegistrationDtoStatus
    {

        [System.Runtime.Serialization.EnumMember(Value = @"NULA")]
        NULA = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"ATIVA")]
        ATIVA = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"SUSPENSA")]
        SUSPENSA = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"INAPTA")]
        INAPTA = 3,

        [System.Runtime.Serialization.EnumMember(Value = @"BAIXADA")]
        BAIXADA = 4,

    }
    [JsonConverter(typeof(JsonStringEnumConverter<LegacyCompanyDtoType>))]
    public enum LegacyCompanyDtoType
    {

        [System.Runtime.Serialization.EnumMember(Value = @"MATRIZ")]
        MATRIZ = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"FILIAL")]
        FILIAL = 1,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<LegacyCompanyDtoSize>))]
    public enum LegacyCompanyDtoSize
    {

        [System.Runtime.Serialization.EnumMember(Value = @"ME")]
        ME = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"EPP")]
        EPP = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"DEMAIS")]
        DEMAIS = 2,

    }

}

