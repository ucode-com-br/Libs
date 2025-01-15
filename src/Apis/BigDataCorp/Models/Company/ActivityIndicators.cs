using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UCode.Apis.BigDataCorp.Models.Company
{
    [DatasetInfo("activity_indicators", "ActivityIndicators")]
    public class ActivityIndicators
    {
        public ActivityIndicators()
        {
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? EmployeesRange
        {
            get; set;
        }



        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? IncomeRange
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? HasActivity
        {
            get; set;
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? ActivityLevel
        {
            get; set;
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? FirstLevelEconomicGroupAverageActivityLevel
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? FirstLevelEconomicGroupMaxActivityLevel
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? FirstLevelEconomicGroupMinActivityLevel
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? ShellCompanyLikelyhood
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? HasRecentAddress
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? HasRecentPhone
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? HasRecentEmail
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? HasRecentPassages
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? HasActiveDomain
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? HasActiveSSL
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? HasCorporateEmail
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? NumberOfBranches
        {
            get; set;
        }
    }
}
