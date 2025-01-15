using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UCode.Apis.BigDataCorp.Models
{
    public struct Status
    {
        [JsonPropertyName("Code")]
        public int Code
        {
            get; set;
        }

        [JsonPropertyName("Message")]
        public string Message
        {
            get; set;
        }
    }
}
