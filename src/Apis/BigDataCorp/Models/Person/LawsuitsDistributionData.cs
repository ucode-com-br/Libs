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
    [DatasetInfo("lawsuits_distribution_data", "LawsuitsDistributionData")]
    public class LawsuitsDistributionData
    {
        public LawsuitsDistributionData()
        {
        }


        [JsonExtensionData]
        public Dictionary<string, object>? JsonExtensionData { get; set; } = new Dictionary<string, object>();


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TotalLawsuits
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Last30DaysLawsuits
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Last90DaysLawsuits
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Last180DaysLawsuits
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Last365DaysLawsuits
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, int>? TypeDistribution
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, int>? CourtNameDistribution
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, int>? StatusDistribution
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, int>? StateDistribution
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, int>? PartyTypeDistribution
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, int>? CourtTypeDistribution
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, int>? CourtLevelDistribution
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, int>? CnjProcedureTypeDistribution
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, int>? CnjSubjectDistribution
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, int>? CnjBroadSubjectDistribution
        {
            get; set;
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? NextPageId
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? FirstLawsuitDate
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? LastLawsuitDate
        {
            get; set;
        }

    }
}
