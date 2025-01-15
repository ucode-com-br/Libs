using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UCode.Apis.BigDataCorp.Models.Company
{
    [DatasetInfo("partner_boavista_owner_participation_data_company", "OwnerParticipationData")]
    public class OwnerParticipationData
    {
        public int NumberOfOwners
        {
            get; set;
        }
        public int NumberOfPeopleAsOwners
        {
            get; set;
        }
        public int NumberOfCompaniesAsOwners
        {
            get; set;
        }
        public bool HasMajorityStakeHolder
        {
            get; set;
        }
        public int AverageParticipationPercentage
        {
            get; set;
        }
        public int MaxParticipationPercentage
        {
            get; set;
        }
        public int MinParticipationPercentage
        {
            get; set;
        }
        public DateTime FirstOwnerEntryDate
        {
            get; set;
        }
        public DateTime LastOwnerEntryDate
        {
            get; set;
        }
        public DateTime QueryDate
        {
            get; set;
        }

        public List<Participation> OwnerParticipations { get; set; } = new List<Participation>();
    }

    public struct Participation
    {
        [JsonPropertyName("DocNumber")]
        /// <summary>
        /// BR - Número de documento do sócio, sendo CPF para pessoa fisica ou CNPJ para pessoa juridica
        /// </summary>
        public string TaxId
        {
            get; set;
        }

        /// <summary>
        /// Nome do sócio
        /// </summary>
        public string EntityName
        {
            get; set;
        }

        /// <summary>
        /// BR - Tipo de entidade (CPF ou CNPJ) do sócio
        /// </summary>
        public string EntityType
        {
            get; set;
        }

        /// <summary>
        /// BR - Tipo de participação desse sócio
        /// </summary>
        public string ParticipationType
        {
            get; set;
        }

        /// <summary>
        /// BR - Status na Receita Federal do documento do sócio
        /// </summary>
        public string EntityTaxIdStatus
        {
            get; set;
        }

        /// <summary>
        /// BR - Percentual de participação societária desse sócio
        /// </summary>
        public int ParticipationPercentage
        {
            get; set;
        }

        /// <summary>
        /// BR - Flag indicando se o sócio tem poderes para agir em nome da empresa
        /// </summary>
        public bool CanSignForCompany
        {
            get; set;
        }

        /// <summary>
        /// BR - Flag indicando se o sócio tem algum registro de negativação em seu nome
        /// </summary>
        public bool HasDebtIndicator
        {
            get; set;
        }

        /// <summary>
        /// BR - Flag indicando se o sócio tem algum registro de fraude em seu nome
        /// </summary>
        public bool HasFraudIndicator
        {
            get; set;
        }

        /// <summary>
        /// Data de entrada do sócio na empresa ou data de entrada na Junta
        /// </summary>
        public DateTime EntryDate
        {
            get; set;
        }
    }

}
