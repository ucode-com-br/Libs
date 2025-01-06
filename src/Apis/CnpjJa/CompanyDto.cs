using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UCode.Apis.CnpjJa
{
    public partial class CompanyDto
    {
        /// <summary>
        /// Código da empresa, idem aos oito primeiros caracteres do CNPJ.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Razão social.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `nature.id &lt; 2000`  
        /// <br/>Ente federativo responsável.
        /// </summary>
        [JsonPropertyName("jurisdiction")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Jurisdiction
        {
            get; set;
        }

        /// <summary>
        /// Capital social
        /// </summary>
        [JsonPropertyName("equity")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public float Equity
        {
            get; set;
        }

        /// <summary>
        /// Informações da natureza jurídica.
        /// </summary>
        [JsonPropertyName("nature")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public NatureDto Nature { get; set; } = new NatureDto();

        /// <summary>
        /// Informações do porte.
        /// </summary>
        [JsonPropertyName("size")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public CompanySizeDto Size { get; set; } = new CompanySizeDto();

        /// <summary>
        /// Informações da opção pelo Simples Nacional.
        /// </summary>
        [JsonPropertyName("simples")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SimplesSimeiDto Simples
        {
            get; set;
        }

        /// <summary>
        /// Informações do enquadramento no MEI.
        /// </summary>
        [JsonPropertyName("simei")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SimplesSimeiDto Simei
        {
            get; set;
        }

        /// <summary>
        /// Quadro de sócios e administradores.
        /// </summary>
        [JsonPropertyName("members")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public IList<MemberDto> Members { get; set; } = new List<MemberDto>();

        /// <summary>
        /// Lista de estabelecimentos.
        /// </summary>
        [JsonPropertyName("offices")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]

        public IList<CompanyOfficeDto> Offices { get; set; } = new List<CompanyOfficeDto>();

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
