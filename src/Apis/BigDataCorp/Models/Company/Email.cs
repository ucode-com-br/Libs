using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.BigDataCorp.Models.Company
{
    public struct Email
    {
        public Email()
        {
        }

        public string? EmailAddress
        {
            get; set;
        }
        public string? Domain
        {
            get; set;
        }
        public string? UserName
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

        /// <summary>
        /// Not mappedd properties
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> ExtensionData { get; set; } = new Dictionary<string, object>();
    }
}
