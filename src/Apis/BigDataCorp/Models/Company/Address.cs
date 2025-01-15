using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.BigDataCorp.Models.Company
{
    public class Address
    {
        public string? Typology
        {
            get; set;
        }
        public string? Title
        {
            get; set;
        }
        public string? AddressMain
        {
            get; set;
        }
        public string? Number
        {
            get; set;
        }
        public string? Complement
        {
            get; set;
        }
        public string? Neighborhood
        {
            get; set;
        }
        public string? ZipCode
        {
            get; set;
        }
        public string? City
        {
            get; set;
        }
        public string? State
        {
            get; set;
        }
        public string? Country
        {
            get; set;
        }
        public string? Type
        {
            get; set;
        }
        public string? ComplementType
        {
            get; set;
        }
        public DateTime? LastUpdateDate
        {
            get; set;
        }

        /// <summary>
        /// Not mappedd properties
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> ExtensionData { get; set; } = new Dictionary<string, object>();



        //public bool? OwnHeadquarters { get; set; }
        //public bool? AddressCurrentlyInRFSite { get; set; }
        //public string? BuildCode { get; set; }
        //public string? BuildingCode { get; set; }
        //public string? HouseholdCode { get; set; }
        //public int? AddressEntityAge { get; set; }
        //public int? AddressEntityTotalPassages { get; set; }
        //public int? AddressEntityBadPassages { get; set; }
        //public int? AddressEntityCrawlingPassages { get; set; }
        //public int? AddressEntityValidationPassages { get; set; }
        //public int? AddressEntityQueryPassages { get; set; }
        //public int? AddressEntityMonthAveragePassages { get; set; }
        //public int? AddressGlobalAge { get; set; }
        //public int? AddressGlobalTotalPassages { get; set; }
        //public int? AddressGlobalBadPassages { get; set; }
        //public int? AddressGlobalCrawlingPassages { get; set; }
        //public int? AddressGlobalValidationPassages { get; set; }
        //public int? AddressGlobalQueryPassages { get; set; }
        //public string? AddressGlobalMonthAveragePassages { get; set; }
        //public int? AddressNumberOfEntities { get; set; }
        //public int? Priority { get; set; }
        //public bool? IsMainForEntity { get; set; }
        //public bool? IsRecentForEntity { get; set; }
        //public bool? IsMainForOtherEntity { get; set; }
        //public bool? IsRecentForOtherEntity { get; set; }
        //public bool? IsActive { get; set; }
        //public bool? IsRatified { get; set; }
        //public bool? IsLikelyFromAccountant { get; set; }
        //public DateTime? LastValidationDate { get; set; }
        //public DateTime? EntityFirstPassageDate { get; set; }
        //public DateTime? EntityLastPassageDate { get; set; }
        //public DateTime? GlobalFirstPassageDate { get; set; }
        //public DateTime? GlobalLastPassageDate { get; set; }
        //public DateTime? CreationDate { get; set; }
        //public bool? HasOptIn { get; set; }
        //public string? Latitude { get; set; }
        //public string? Longitude { get; set; }
    }
}
