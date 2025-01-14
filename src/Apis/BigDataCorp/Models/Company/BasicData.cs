using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.BigDataCorp.Models.Company
{
    public struct BasicData
    {
        public BasicData()
        {
        }

        public object AlternativeIdNumbers
        {
            get; set;
        }
        public string? TaxIdNumber
        {
            get; set;
        }
        public string? TaxIdCountry
        {
            get; set;
        }
        public string? OfficialName
        {
            get; set;
        }
        public string? TradeName
        {
            get; set;
        }
        public string[]? Aliases
        {
            get; set;
        }
        public decimal? NameUniquenessScore
        {
            get; set;
        }
        public decimal? OfficialNameUniquenessScore
        {
            get; set;
        }
        public decimal? TradeNameUniquenessScore
        {
            get; set;
        }
        public decimal? OfficialNameInputNameMatchPercentage
        {
            get; set;
        }
        public decimal? TradeNameInputNameMatchPercentage
        {
            get; set;
        }
        public DateTime? ClosedDate
        {
            get; set;
        }
        public decimal? Age
        {
            get; set;
        }
        public bool? IsHeadquarter
        {
            get; set;
        }
        public string? HeadquarterState
        {
            get; set;
        }
        public bool? IsConglomerate
        {
            get; set;
        }
        public string? TaxIdOrigin
        {
            get; set;
        }
        public DateTime? TaxIdStatusRegistrationDate
        {
            get; set;
        }

        /// <summary>
        /// Date of registration of the company's status with the Internal Revenue Service.
        /// </summary>
        public string? TaxRegime
        {
            get; set;
        }

        /// <summary>
        /// Type of company in the IRS.
        /// </summary>
        public string? CompanyType_ReceitaFederal
        {
            get; set;
        }

        /// <summary>
        /// Type of company in the IRS.
        /// </summary>
        public object? TaxRegimes
        {
            get; set;
        }

        public DateTime? FoundedDate
        {
            get; set;
        }
        public string? TaxIdStatus
        {
            get; set;
        }
        public DateTime? TaxIdStatusDate
        {
            get; set;
        }
        public List<Activity> Activities { get; set; } = new List<Activity>();

        /// <summary>
        /// Object detailing description of the legal nature of the company.
        /// + Code string required
        ///   > Code of legal nature.
        /// + Activity string required
        ///   > Description of the legal nature.
        /// </summary>
        public Dictionary<string, string> LegalNature { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Descriptor of the special situation of the CNPJ.
        /// </summary>
        public string? SpecialSituation
        {
            get; set;
        }

        /// <summary>
        /// Date on which the company entered into a special situation.
        /// </summary>
        public DateTime? SpecialSituationDate
        {
            get; set;
        }

        /// <summary>
        /// Date of creation of the record in BigDataCorp internal data.
        /// </summary>
        public DateTime? CreationDate
        {
            get; set;
        }

        /// <summary>
        /// Last update date for this record.
        /// </summary>
        public DateTime? LastUpdateDate
        {
            get; set;
        }

        /// <summary>
        /// Dictionary containing possible additional data.
        /// </summary>
        public Dictionary<string, object> AdditionalOutputData
        {
            get; set;
        }

        /// <summary>
        /// Dictionary containing possible additional data.
        /// + HasChangedTradeName boolean required
        ///   > Indicates whether the company has changed its corporate name.
        /// + HasChangedTaxRegime boolean required
        ///   > Indicates whether the company has changed tax regime.
        /// + HistoricalDataEvolution  object required
        ///   > Dictionary with the historical evolution of the data.
        /// </summary>
        public Dictionary<string, object> HistoricalData
        {
            get; set;
        }

        /// <summary>
        /// Not mappedd properties
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> ExtensionData { get; set; } = new Dictionary<string, object>();
    }
}
