using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    [JsonConverter(typeof(JsonStringEnumConverter<CertificateType>))]
    public enum CertificateType
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

}

