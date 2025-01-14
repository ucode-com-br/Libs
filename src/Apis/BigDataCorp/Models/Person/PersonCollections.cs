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
    [DatasetInfo("collections", "Collections")]
    public class PersonCollections
    {
        public PersonCollections()
        {
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsCurrentlyOnCollection
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TotalCollectionOccurrences
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TotalCollectionOrigins
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Last30DaysCollectionOccurrences
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Last90DaysCollectionOccurrences
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Last180DaysCollectionOccurrences
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Last365DaysCollectionOccurrences
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Last30DaysCollectionOrigins
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Last90DaysCollectionOrigins
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Last180DaysCollectionOrigins
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Last365DaysCollectionOrigins
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TotalCollectionMonths
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? CurrentConsecutiveCollectionMonths
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxConsecutiveCollectionMonths
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? CollectionOccurrences
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? CollectionOrigins
        {
            get; set;
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? FirstCollectionDate
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? LastCollectionDate
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? CreationDate
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? LastUpdateDate
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
