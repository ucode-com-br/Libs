using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public partial class CompanyOfficeDto
    {
        /// <summary>
        /// Número do CNPJ sem pontuação.
        /// </summary>
        [JsonPropertyName("taxId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string TaxId
        {
            get; set;
        }

        /// <summary>
        /// Nome fantasia.
        /// </summary>
        [JsonPropertyName("alias")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Alias
        {
            get; set;
        }

        /// <summary>
        /// Data de abertura.
        /// </summary>
        [JsonPropertyName("founded")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Founded
        {
            get; set;
        }

        /// <summary>
        /// Indica se o estabelecimento é a Matriz.
        /// </summary>
        [JsonPropertyName("head")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Head
        {
            get; set;
        }

        /// <summary>
        /// Data da situação cadastral.
        /// </summary>
        [JsonPropertyName("statusDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string StatusDate
        {
            get; set;
        }

        /// <summary>
        /// Informações da situação cadastral.
        /// </summary>
        [JsonPropertyName("status")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public OfficeStatusDto Status { get; set; } = new OfficeStatusDto();

        /// <summary>
        /// Presente quando `status.id != 2`  
        /// <br/>Informações do motivo da situação cadastral.
        /// </summary>
        [JsonPropertyName("reason")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OfficeReasonDto Reason
        {
            get; set;
        }

        /// <summary>
        /// Data da situação especial.
        /// </summary>
        [JsonPropertyName("specialDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SpecialDate
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `specialDate != undefined`  
        /// <br/>Informações da situação especial.
        /// </summary>
        [JsonPropertyName("special")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OfficeSpecialDto Special
        {
            get; set;
        }

        /// <summary>
        /// Informações da atividade econômica principal.
        /// </summary>
        [JsonPropertyName("mainActivity")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public ActivityDto MainActivity { get; set; } = new ActivityDto();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

}

