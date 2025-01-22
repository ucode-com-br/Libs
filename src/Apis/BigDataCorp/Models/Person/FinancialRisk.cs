using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UCode.Apis.BigDataCorp.Models.Person
{
    [DatasetInfo("financial_risk", "FinancialRisk")]
    public class FinancialRisk
    {
        public FinancialRisk()
        {
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TotalAssets
        {
            get; set;
        }

        /// <summary>
        /// Estimated annual income range in BRL (R$)
        /// </summary>
        /// <remarks>
        /// Categorized into ranges by Brazilian central bank regulations.
        /// Possible values: "Até R$ 24.400,00", "R$ 24.401,00 a R$ 73.200,00",
        /// "R$ 73.201,00 a R$ 122.000,00", "Acima de R$ 122.000,00"
        /// </remarks>
        /// <example>"R$ 24.401,00 a R$ 73.200,00"</example>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? EstimatedIncomeRange
        {
            get; set;
        }

        /// <summary>
        /// Indicates if the person has current formal employment
        /// </summary>
        /// <remarks>
        /// Based on RAIS/CAGED government employment registries.
        /// Null indicates employment status unknown.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsCurrentlyEmployed
        {
            get; set;
        }

        /// <summary>
        /// Indicates if the person owns business equity
        /// </summary>
        /// <remarks>
        /// Checks REDESIM registry for business ownership.
        /// Null indicates ownership status unknown.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsCurrentlyOwner
        {
            get; set;
        }

        /// <summary>
        /// Start date of current/last employment
        /// </summary>
        /// <remarks>
        /// Uses the most recent employment record from eSocial system.
        /// Null if no employment history found.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? LastOccupationStartDate
        {
            get; set;
        }

        /// <summary>
        /// Indicates if the person has active debt collections
        /// </summary>
        /// <remarks>
        /// Checks SERASA/Boletos registry for open collections.
        /// Null indicates collection status unknown.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsCurrentlyOnCollection
        {
            get; set;
        }

        /// <summary>
        /// Number of collection incidents in last 12 months
        /// </summary>
        /// <remarks>
        /// Counts both financial and non-financial (utility) collections.
        /// Null indicates no collection data available.
        /// </remarks>
        /// <example>3</example>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Last365DaysCollectionOccurrences
        {
            get; set;
        }

        /// <summary>
        /// Current streak of months with active collections
        /// </summary>
        /// <remarks>
        /// Continuous months with at least one active collection.
        /// Null indicates no active collections.
        /// </remarks>
        /// <example>2</example>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? CurrentConsecutiveCollectionMonths
        {
            get; set;
        }

        /// <summary>
        /// Indicates receipt of government assistance (Bolsa Família)
        /// </summary>
        /// <remarks>
        /// Based on CadÚnico social program registry.
        /// Null indicates assistance status unknown.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsCurrentlyReceivingAssistance
        {
            get; set;
        }

        /// <summary>
        /// Comprehensive risk score (300-850)
        /// </summary>
        /// <remarks>
        /// Calculated using Serasa/Experian scoring model:
        /// <para>- 300-579: Poor</para>
        /// <para>- 580-669: Fair</para>
        /// <para>- 670-739: Good</para>
        /// <para>- 740-799: Very Good</para>
        /// <para>- 800-850: Excellent</para>
        /// Null indicates insufficient data for scoring.
        /// </remarks>
        /// <example>720</example>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? FinancialRiskScore
        {
            get; set;
        }

        /// <summary>
        /// Categorical risk classification
        /// </summary>
        /// <remarks>
        /// <para>Possible values:</para>
        /// <para>- "Baixo" (Low Risk)</para>
        /// <para>- "Médio" (Medium Risk)</para> 
        /// <para>- "Alto" (High Risk)</para>
        /// <para>- "Muito Alto" (Very High Risk)</para>
        /// Null indicates risk classification unavailable.
        /// </remarks>
        /// <example>"Médio"</example>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FinancialRiskLevel
        {
            get; set;
        }







        [JsonExtensionData]
        public Dictionary<string, object>? ExtensionData
        {
            get; set;
        }

    }
}
