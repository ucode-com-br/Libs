using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UCode.Apis.BigDataCorp.Models.Marketplace
{
    [DatasetInfo("partner_boavista_owner_participation_data_company", "OwnerParticipationData")]
    public class OwnerParticipationData
    {
        public class OwnerParticipation
        {
            public string? DocNumber
            {
                get; set;
            }

            public string? EntityName
            {
                get; set;
            }

            public string? EntityType
            {
                get; set;
            }

            public string? ParticipationType
            {
                get; set;
            }

            public string? EntityTaxIdStatus
            {
                get; set;
            }

            public decimal? ParticipationPercentage
            {
                get; set;
            }

            public bool? CanSignForCompany
            {
                get; set;
            }

            public bool? HasDebtIndicator
            {
                get; set;
            }

            public bool? HasFraudIndicator
            {
                get; set;
            }

            public DateTime? EntryDate
            {
                get; set;
            }
        }


        public OwnerParticipationData()
        {
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? NumberOfOwners
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? NumberOfPeopleAsOwners
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? NumberOfCompaniesAsOwners
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? HasMajorityStakeHolder
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? AverageParticipationPercentage
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? MaxParticipationPercentage
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? MinParticipationPercentage
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? FirstOwnerEntryDate
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? LastOwnerEntryDate
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? QueryDate
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<OwnerParticipation> OwnerParticipations
        {
            get; set;
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? ReferenceDate
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
