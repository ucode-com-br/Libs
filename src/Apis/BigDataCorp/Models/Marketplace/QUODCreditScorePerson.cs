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
    [DatasetInfo("partner_quod_credit_score_person", "QUODCreditScorePerson")]
    public class QUODCreditScorePerson
    {
        public QUODCreditScorePerson()
        {
        }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Score
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? PaymentCommitmentScore
        {
            get; set;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? PaymentCapacityScore
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
