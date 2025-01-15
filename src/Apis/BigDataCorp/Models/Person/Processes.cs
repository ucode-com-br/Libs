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





    [DatasetInfo("processes", "Processes")]
    public class Processes
    {
        #region Classes

        public class Decision
        {
            public string? DecisionContent
            {
                get; set;
            }
            public DateTime? DecisionDate
            {
                get; set;
            }

            [JsonExtensionData]
            public Dictionary<string, object>? JsonExtensionData { get; set; } = new Dictionary<string, object>();
        }

        public class Party
        {
            public string? Doc
            {
                get; set;
            }
            public bool? IsPartyActive
            {
                get; set;
            }
            public string? Name
            {
                get; set;
            }
            public string? Polarity
            {
                get; set;
            }
            public string? Type
            {
                get; set;
            }
            public PartyDetails? PartyDetails
            {
                get; set;
            }
            public DateTime? LastCaptureDate
            {
                get; set;
            }

            [JsonExtensionData]
            public Dictionary<string, object>? JsonExtensionData { get; set; } = new Dictionary<string, object>();
        }

        public class PartyDetails
        {
            public string? SpecificType
            {
                get; set;
            }
            public string? OAB
            {
                get; set;
            }
            public string? State
            {
                get; set;
            }

            [JsonExtensionData]
            public Dictionary<string, object>? JsonExtensionData { get; set; } = new Dictionary<string, object>();
        }

        public class Lawsuit
        {
            public List<string>? OtherSubjects { get; set; } = new List<string>();
            public string? Number
            {
                get; set;
            }
            public string? Type
            {
                get; set;
            }
            public string? MainSubject
            {
                get; set;
            }
            public string? CourtName
            {
                get; set;
            }
            public string? CourtLevel
            {
                get; set;
            }
            public string? CourtType
            {
                get; set;
            }
            public string? CourtDistrict
            {
                get; set;
            }
            public string? JudgingBody
            {
                get; set;
            }
            public string? State
            {
                get; set;
            }
            public string? Status
            {
                get; set;
            }
            public string? LawsuitHostService
            {
                get; set;
            }
            public string? InferredCNJSubjectName
            {
                get; set;
            }
            public int? InferredCNJSubjectNumber
            {
                get; set;
            }
            public string? InferredCNJProcedureTypeName
            {
                get; set;
            }
            public string? InferredBroadCNJSubjectName
            {
                get; set;
            }
            public int? InferredBroadCNJSubjectNumber
            {
                get; set;
            }
            public int? NumberOfVolumes
            {
                get; set;
            }
            public int? NumberOfPages
            {
                get; set;
            }
            public decimal? Value
            {
                get; set;
            }
            public DateTime? ResJudicataDate
            {
                get; set;
            }
            public DateTime? CloseDate
            {
                get; set;
            }
            public DateTime? RedistributionDate
            {
                get; set;
            }
            public DateTime? PublicationDate
            {
                get; set;
            }
            public DateTime? NoticeDate
            {
                get; set;
            }
            public DateTime? LastMovementDate
            {
                get; set;
            }
            public DateTime? CaptureDate
            {
                get; set;
            }
            public DateTime? LastUpdate
            {
                get; set;
            }
            public int? NumberOfParties
            {
                get; set;
            }
            public int? NumberOfUpdates
            {
                get; set;
            }
            public int? LawSuitAge
            {
                get; set;
            }
            public int? AverageNumberOfUpdatesPerMonth
            {
                get; set;
            }
            public int? ReasonForConcealedData
            {
                get; set;
            }
            public List<Party>? Parties { get; set; } = new List<Party>();
            public List<Update>? Updates { get; set; } = new List<Update>();
            public List<Petition>? Petitions { get; set; } = new List<Petition>();
            public List<Decision>? Decisions { get; set; } = new List<Decision>();

            [JsonExtensionData]
            public Dictionary<string, object>? JsonExtensionData { get; set; } = new Dictionary<string, object>();
        }

        public class Update
        {
            public string? Content
            {
                get; set;
            }
            public DateTime? PublishDate
            {
                get; set;
            }
            public DateTime? CaptureDate
            {
                get; set;
            }

            [JsonExtensionData]
            public Dictionary<string, object>? JsonExtensionData { get; set; } = new Dictionary<string, object>();
        }

        public class Petition
        {
            public string? Type
            {
                get; set;
            }
            public string? Author
            {
                get; set;
            }
            public DateTime? CreationDate
            {
                get; set;
            }
            public DateTime? JoinedDate
            {
                get; set;
            }

            [JsonExtensionData]
            public Dictionary<string, object>? JsonExtensionData { get; set; } = new Dictionary<string, object>();
        }

        #endregion Classes




        public Processes()
        {
        }


        [JsonExtensionData]
        public Dictionary<string, object>? JsonExtensionData { get; set; } = new Dictionary<string, object>();


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Lawsuit>? Lawsuits { get; set; } = new List<Lawsuit>();


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TotalLawsuits
        {
            get; set;
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TotalLawsuitsAsAuthor
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TotalLawsuitsAsDefendant
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TotalLawsuitsAsOther
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
