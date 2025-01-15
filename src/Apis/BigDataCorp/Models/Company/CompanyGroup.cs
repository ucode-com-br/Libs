
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace UCode.Apis.BigDataCorp.Models.Company
{

    public class CompanyGroup
    {
        /// <summary>
        /// CNPJ da empresa principal ou da primeira empresa
        /// </summary>
        public string? MainCompanyTaxId
        {
            get; set;
        }

        public string? MatchKeyTaxId
        {
            get; set;
        }

        public int? TotalUniqueStates
        {
            get; set;
        }

        public int? TotalUniqueCities
        {
            get; set;
        }

        public int? TotalSites
        {
            get; set;
        }
        public int? TotalSimplesCompanies
        {
            get; set;
        }
        public int? TotalNumberOfOwners
        {
            get; set;
        }
        public int? TotalMEIs
        {
            get; set;
        }
        public int? TotalMarketplaceStores
        {
            get; set;
        }
        public decimal? TotalDeclaredValue
        {
            get; set;
        }
        public int? MinSites
        {
            get; set;
        }
        public int? MinNumberOfOwners
        {
            get; set;
        }
        public int? MinMarketplaceStores
        {
            get; set;
        }
        public int? MaxSites
        {
            get; set;
        }
        public int? MaxNumberOfOwners
        {
            get; set;
        }
        public int? MaxMarketplaceStores
        {
            get; set;
        }
        public decimal? MaxDeclaredValue
        {
            get; set;
        }
        public decimal? AverageNumberOfOwners
        {
            get; set;
        }
        public decimal? AverageMarkerplaceStores
        {
            get; set;
        }
        public int? TotalUniqueCNAES
        {
            get; set;
        }
        public decimal? AverageDeclaredValue
        {
            get; set;
        }


        public decimal? AverageSites
        {
            get; set;
        }

        public string? AverageEmployeeRange
        {
            get; set;
        }


        public Dictionary<string, int> CNAEDistribution { get; set; } = new Dictionary<string, int>();




        public string? CompanyGroupType
        {
            get; set;
        }

        public decimal? MinDeclaredValue
        {
            get; set;
        }

        /// <summary>
        /// Tipo do grupo econômico (first-level, third-level, extended, etc...).
        /// </summary>
        public string? EconomicGroupType
        {
            get; set;
        }

        /// <summary>
        /// Quantidade de empresas marcadas como 'sede'.
        /// </summary>
        public int? TotalHeadquarter
        {
            get; set;
        }

        /// <summary>
        /// Total de empresas relacionadas.
        /// </summary>
        public int? TotalCompanies
        {
            get; set;
        }

        /// <summary>
        /// Quantidade de empresas marcadas como 'filial'.
        /// </summary>
        public int? TotalBranches
        {
            get; set;
        }

        /// <summary>
        /// Quantidade de estados em que a empresa se encontra.
        /// </summary>
        public int? TotalStates
        {
            get; set;
        }

        /// <summary>
        /// Quantidade de cidades em que a empresa se encontra.
        /// </summary>
        public int? TotalCities
        {
            get; set;
        }

        /// <summary>
        /// Total de empresas ativas.
        /// </summary>
        public int? TotalActiveCompanies
        {
            get; set;
        }

        /// <summary>
        /// Total de empresas inativas.
        /// </summary>
        public int? TotalInactiveCompanies
        {
            get; set;
        }

        /// <summary>
        /// Idade mínima das empresas.
        /// </summary>
        public decimal? MinCompanyAge
        {
            get; set;
        }

        /// <summary>
        /// Idade máxima das empresas.
        /// </summary>
        public decimal? MaxCompanyAge
        {
            get; set;
        }

        /// <summary>
        /// Idade média das empresas.
        /// </summary>
        public decimal? AverageCompanyAge
        {
            get; set;
        }

        /// <summary>
        /// Mínimo do nível de atividade das empresas.
        /// </summary>
        public decimal? MinActivityLevel
        {
            get; set;
        }

        /// <summary>
        /// Máximo do nível de atividade das empresas.
        /// </summary>
        public decimal? MaxActivityLevel
        {
            get; set;
        }

        /// <summary>
        /// Média do nível de atividade das empresas.
        /// </summary>
        public decimal? AverageActivityLevel
        {
            get; set;
        }

        /// <summary>
        /// Faixa total de renda das entidades.
        /// </summary>
        public string? TotalIncomeRange
        {
            get; set;
        }

        /// <summary>
        /// Faixa mínima de renda das entidades.
        /// </summary>
        public string? MinIncomeRange
        {
            get; set;
        }

        /// <summary>
        /// Faixa máxima de renda das entidades.
        /// </summary>
        public string? MaxIncomeRange
        {
            get; set;
        }

        /// <summary>
        /// Faixa média de renda das entidades.
        /// </summary>
        public string? AverageIncomeRange
        {
            get; set;
        }

        /// <summary>
        /// Faixa total da quantidade de funcionários.
        /// </summary>
        public string? TotalEmployeesRange
        {
            get; set;
        }

        /// <summary>
        /// Faixa mínima da quantidade de funcionários.
        /// </summary>
        public string? MinEmployeesRange
        {
            get; set;
        }

        /// <summary>
        /// Faixa máxima da quantidade de funcionários.
        /// </summary>
        public string? MaxEmployeesRange
        {
            get; set;
        }

        /// <summary>
        /// Faixa média da quantidade de funcionários.
        /// </summary>
        public string? AverageEmployeesRange
        {
            get; set;
        }

        /// <summary>
        /// Total de pessoas na empresa.
        /// </summary>
        public int? TotalPeople { get; set; } = 0;

        /// <summary>
        /// Total de donos da empresa.
        /// </summary>
        public int? TotalOwners
        {
            get; set;
        }

        /// <summary>
        /// Total de pessoas politicamente expostas (PEPs) na empresa.
        /// </summary>
        public int? TotalPEPs
        {
            get; set;
        }

        /// <summary>
        /// Total de pessoas sancionadas na empresa.
        /// </summary>
        public int? TotalSanctioned
        {
            get; set;
        }

        /// <summary>
        /// Total de ações judiciais relacionadas à entidade.
        /// </summary>
        public int? TotalLawsuits
        {
            get; set;
        }

        /// <summary>
        /// Total de websites relacionados à entidade.
        /// </summary>
        public int? TotalWebsites
        {
            get; set;
        }

        /// <summary>
        /// Total de endereços relacionados à entidade.
        /// </summary>
        public int TotalAddresses
        {
            get; set;
        }

        /// <summary>
        /// Total de telefones relacionados à entidade.
        /// </summary>
        public int? TotalPhones
        {
            get; set;
        }

        /// <summary>
        /// Total de emails relacionados à entidade.
        /// </summary>
        public int? TotalEmails
        {
            get; set;
        }

        /// <summary>
        /// Total de passagens relacionadas à entidade.
        /// </summary>
        public int? TotalPassages
        {
            get; set;
        }

        /// <summary>
        /// Total de passagens suspeitas relacionadas à entidade.
        /// </summary>
        public int? TotalBadPassages
        {
            get; set;
        }

        /// <summary>
        /// Média de passagens do mês relacionadas à entidade.
        /// </summary>
        public decimal? MonthAveragePassages
        {
            get; set;
        }

        /// <summary>
        /// Data da primeira passagem identificada para uma empresa ou pessoa do grupo.
        /// </summary>
        public string? FirstPassageDate
        {
            get; set;
        }

        /// <summary>
        /// Data da ultima passagem identificada para uma empresa ou pessoa do grupo.
        /// </summary>
        public string? LastPassageDate
        {
            get; set;
        }

        /// <summary>
        /// Total de passagens nos últimos 3 meses relacionadas à entidade.
        /// </summary>
        public int? Last3MonthsPassages
        {
            get; set;
        }

        /// <summary>
        /// Total de passagens nos últimos 6 meses relacionadas à entidade.
        /// </summary>
        public int? Last6MonthsPassages
        {
            get; set;
        }

        /// <summary>
        /// Total de passagens nos últimos 12 meses relacionadas à entidade.
        /// </summary>
        public int? Last12MonthsPassages
        {
            get; set;
        }

        /// <summary>
        /// Total de passagens nos últimos 18 meses relacionadas à entidade.
        /// </summary>
        public int? Last18MonthsPassages
        {
            get; set;
        }

        /// <summary>
        /// Lista das atividades econômicas (CNAE) identificadas para as empresas do grupo.
        /// </summary>
        public List<string> EconomicActivities { get; set; } = new List<string>();

        /// <summary>
        /// Distribuição da quantidade de entidades identificadas por nível do grupo.
        /// </summary>
        public object? EntitiesByLevel
        {
            get; set;
        }

        /// <summary>
        /// Distribuição da quantidade de entidades identificadas por faixa de faturamento.
        /// </summary>
        public object? IncomeRangeDistribution
        {
            get; set;
        }

        /// <summary>
        /// Distribuição da quantidade de entidades identificadas por número de funcionários.
        /// </summary>
        public object? EmployeeRangeDistribution
        {
            get; set;
        }

        /// <summary>
        /// Distribuição da quantidade de entidades identificadas por estado de localização.
        /// </summary>
        public Dictionary<string, int> StateDistribution { get; set; } = new Dictionary<string, int>();



        /// <summary>
        /// Distribuição da quantidade de entidades identificadas por cidade de localização.
        /// </summary>
        public Dictionary<string, int> CityDistribution { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Lista de CNPJs associados à entidade.
        /// </summary>
        [JsonPropertyName("CompanyDocNumbers")]
        public string[] CompanyTaxIds { get; set; } = new string[0];


        public Dictionary<string, int> TaxRegimeDistribution { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, int> TaxIdStatusDistribution { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, int> LegalNatureDistribution { get; set; } = new Dictionary<string, int>();

        #region Political
        /// <summary>
        /// Distribuição do valor doado a cada partido por ano.
        /// </summary>
        public object? PartyDonationDistribution
        {
            get; set;
        }

        /// <summary>
        /// Distribuição do valor doado a cada candidato por ano.
        /// </summary>
        public object? CandidateDonationDistribution
        {
            get; set;
        }

        /// <summary>
        /// Valor total de doações a partidos na última eleição.
        /// </summary>
        public decimal? TotalValueInPartyDonationsInLastElection
        {
            get; set;
        }

        /// <summary>
        /// Valor total de doações a partidos na penúltima eleição.
        /// </summary>
        public decimal? TotalValueInPartyDonationsInPenultimateElection
        {
            get; set;
        }


        /// <summary>
        /// Valor total de doações a candidatos na última eleição.
        /// </summary>
        public decimal? TotalValueInCandidateDonationsInLastElection
        {
            get; set;
        }


        /// <summary>
        /// Valor total de doações a candidatos na penúltima eleição.
        /// </summary>
        public decimal? TotalValueInCandidateDonationsInPenultimateElection
        {
            get; set;
        }

        /// <summary>
        /// Número de partidos distintos doados para na última eleição.
        /// </summary>
        public int? TotalDistinctPartyDonatedToInLastElection
        {
            get; set;
        }

        /// <summary>
        /// Número de partidos distintos doados para na penúltima eleição.
        /// </summary>
        public int? TotalDistinctPartyDonatedToInPenultimateElection
        {
            get; set;
        }


        /// <summary>
        /// Número de candidatos distintos doados para na última eleição.
        /// </summary>
        public int? TotalDistinctCandidateDonatedToInLastElection
        {
            get; set;
        }


        /// <summary>
        /// Número de candidatos distintos doados para na penúltima eleição.
        /// </summary>
        public int? TotalDistinctCandidateDonatedToInPenultimateElection
        {
            get; set;
        }


        /// <summary>
        /// Número de empresas que fizeram alguma doação eleitoral na ultima eleição.
        /// </summary>
        public int? TotalCompaniesWithElectoralDonationInLastElection
        {
            get; set;
        }


        /// <summary>
        /// Número de empresas que fizeram alguma doação eleitoral na penúltima eleição.
        /// </summary>
        public int? TotalCompaniesWithElectoralDonationInPenultimateElection
        {
            get; set;
        }

        /// <summary>
        /// Distribuição do valor recebido por fornecimento de cada partido por ano.
        /// </summary>
        public object? PartyProviderPaymentDistribution
        {
            get; set;
        }

        /// <summary>
        /// Distribuição do valor recebido por fornecimento de cada candidato por ano.
        /// </summary>
        public object? CandidateProviderPaymentDistribution
        {
            get; set;
        }

        /// <summary>
        /// Valor total recebido por fornecimento de partidos na última eleição.
        /// </summary>
        public decimal? TotalValueInPartyProviderPaymentsInLastElection
        {
            get; set;
        }

        /// <summary>
        /// Valor total recebido por fornecimento de partidos na penúltima eleição.
        /// </summary>
        public decimal? TotalValueInPartyProviderPaymentsInPenultimateElection
        {
            get; set;
        }

        /// <summary>
        /// Valor total recebido por fornecimento de candidatos na última eleição.
        /// </summary>
        public decimal? TotalValueInCandidateProviderPaymentsInLastElection
        {
            get; set;
        }

        /// <summary>
        /// Valor total recebido por fornecimento de candidatos na penúltima eleição.
        /// </summary>
        public decimal? TotalValueInCandidateProviderPaymentsInPenultimateElection
        {
            get; set;
        }


        /// <summary>
        /// Número de partidos distintos que fizeram pagamento por fornecimento na última eleição.
        /// </summary>
        public int? TotalDistinctPartyProviderPaymentsInLastElection
        {
            get; set;
        }

        /// <summary>
        /// Número de partidos distintos que fizeram pagamento por fornecimento na penúltima eleição.
        /// </summary>
        public int? TotalDistinctPartyProviderPaymentsInPenultimateElection
        {
            get; set;
        }


        /// <summary>
        /// Número de candidatos distintos que fizeram pagamento por fornecimento na última eleição.
        /// </summary>
        public int? TotalDistinctCandidateProviderPaymentsInLastElection
        {
            get; set;
        }


        /// <summary>
        /// Número de candidatos distintos doados para na penúltima eleição.
        /// </summary>
        public int? TotalDistinctCandidateProviderPaymentsInPenultimateElection
        {
            get; set;
        }


        /// <summary>
        /// Número de empresas que receberam por fornecimento eleitoral na ultima eleição.
        /// </summary>
        public int? TotalCompaniesWithProviderPaymentsInLastElection
        {
            get; set;
        }


        /// <summary>
        /// Número de empresas que receberam por fornecimento eleitoral na penúltima eleição.
        /// </summary>
        public int? TotalCompaniesWithProviderPaymentsInPenultimateElection
        {
            get; set;
        }

        #endregion Political

        [BsonExtraElements]
        [JsonExtensionData]
        public Dictionary<string, object> ExtensionData { get; set; } = new Dictionary<string, object>();
    }
}
