using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UCode.Apis.BigDataCorp.Models
{

    public class ResponseData<TData>
    {
        [JsonPropertyName("Result")]
        public List<ResponseResult<TData>> Result { get; set; } = new List<ResponseResult<TData>>();

        [JsonPropertyName("QueryId")]
        [JsonInclude]
        private string _queryId;

        [JsonIgnore]
        public Guid QueryId
        {
            get => Guid.Parse(_queryId);
        }

        [JsonPropertyName("ElapsedMilliseconds")]
        [JsonInclude]
        private int _elapsedMilliseconds
        {
            get; set;
        }

        [JsonIgnore]
        public TimeSpan Elapsed
        {
            get => TimeSpan.FromMilliseconds(Convert.ToDouble(this._elapsedMilliseconds));
        }

        [JsonInclude]
        [JsonPropertyName("QueryDate")]
        public DateTime QueryDate
        {
            get; set;
        }

        /// <summary>
        /// Status list for each dataset.
        /// ex.: [{ "|+DATASET_NAME+|": [{"Code": 0, "Message": ""}] }]
        /// </summary>
        [JsonPropertyName("Status")]
        public Dictionary<string, List<Status>> Status { get; set; } = new Dictionary<string, List<Status>>();

        /// <summary>
        /// List of evidence used for each dataset.
        /// ex.: [{ "|+DATASET_NAME+|": [{"Id": "", "Source": ""}] }]
        /// </summary>
        [JsonPropertyName("Evidences")]
        public Dictionary<string, List<ResponseDataEvidences>> Evidences { get; set; } = new Dictionary<string, List<ResponseDataEvidences>>();

        /// <summary>
        /// Not mappedd properties
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> ExtensionData { get; set; } = new Dictionary<string, object>();
    }

}
