using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.BigDataCorp.Models.Company
{
    public class Phone
    {
        public string? Number
        {
            get; set;
        }
        public string? AreaCode
        {
            get; set;
        }
        public string? CountryCode
        {
            get; set;
        }
        public string? Complement
        {
            get; set;
        }
        public string? Type
        {
            get; set;
        }
        public DateTime? LastUpdateDate
        {
            get; set;
        }
        public int? PhoneNumberOfEntities
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
