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
    //https://docs.bigdatacorp.com.br/plataforma/reference/marketplace_partner_datarisk_income_prediction_person
    [DatasetInfo("partner_datarisk_income_prediction_person", "DatariskIncomePrediction")]
    public class DatariskIncomePrediction
    {
        //OnlineQueries
        public DatariskIncomePrediction()
        {
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Origin
        {
            get; set;
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? InputParameters
        {
            get; set;
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? QueryRawHTMLResult
        {
            get; set;
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object>? QueryResultData { get; set; } = new Dictionary<string, object>();


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? QueryDate
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
