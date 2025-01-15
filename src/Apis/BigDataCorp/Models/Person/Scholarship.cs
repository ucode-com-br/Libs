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
    [DatasetInfo("university_student_data", "Scholarship")]
    public class Scholarship
    {
        public class ScholarshipHistoryData
        {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Level
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Institution
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? SpecializationArea
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? StartYear
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? EndYear
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? EducationalLevel
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? CourseType
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? CourseShift
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? ScholarshipType
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? AcademicTrainingType
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? InstitutionCampus
            {
                get; set;
            }
        }

        public class PublicationHistoryData
        {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Type
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Title
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Summary
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Abstract
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Source
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Role
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? YearPublished
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Keywords
            {
                get; set;
            }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Authors
            {
                get; set;
            }
        }

        public Scholarship()
        {
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ScholarshipHistoryData>? ScholarshipHistory { get; set; } = new List<ScholarshipHistoryData>();

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PublicationHistoryData>? PublicationHistory { get; set; } = new List<PublicationHistoryData>();

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ScholarshipLevel
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? EducationalLevel
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? NumberOfUndergraduateCourses
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsUniversityStudent
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsCurrentlyOnAcademicField
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? LastUpdateDate
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? CreationDate
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
