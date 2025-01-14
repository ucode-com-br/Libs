using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static UCode.Apis.BigDataCorp.Models.Company.MediaProfileAndExposure;

namespace UCode.Apis.BigDataCorp.Models.Company
{
    [DatasetInfo("media_profile_and_exposure", "MediaProfileAndExposure")]
    public class MediaProfileAndExposure
    {
        #region Class


        public class EntityStatistic
        {
            public NewsByRangeDate? NewsByRangeDate
            {
                get; set;
            }
        }

        public class NewsByRangeDate
        {
            public int? TotalNews
            {
                get; set;
            }
            public int? TotalNewsOnLast7Days
            {
                get; set;
            }
            public int? TotalNewsOnLast30Days
            {
                get; set;
            }
            public int? TotalNewsOnLast90Days
            {
                get; set;
            }
            public int? TotalNewsOnLast180Days
            {
                get; set;
            }
            public int? TotalNewsOnLast365Days
            {
                get; set;
            }
        }

        public class NewsComment
        {
            public string? Comment
            {
                get; set;
            }
            public string? Author
            {
                get; set;
            }
            public DateTime? PostDate
            {
                get; set;
            }
            public List<NewsComment> Replies
            {
                get; set;
            }
        }

        public class NewsItem
        {
            public string? Title
            {
                get; set;
            }
            public string? SourceName
            {
                get; set;
            }
            public string? Url
            {
                get; set;
            }
            public SentimentAnalysis? SentimentAnalysis
            {
                get; set;
            }
            public List<string> Categories { get; set; } = new List<string>();
            public List<NewsComment> NewsComments { get; set; } = new List<NewsComment>();
            public DateTime? CaptureDate
            {
                get; set;
            }
            public DateTime? PublicationDate
            {
                get; set;
            }
        }



        public class SearchLabel
        {
            public string? FullName
            {
                get; set;
            }
            public decimal? FullNameUniquenessScore
            {
                get; set;
            }

            public string? ShortName
            {
                get; set;
            }
            public decimal? ShortNameUniquenessScore
            {
                get; set;
            }

            public string? OfficialName
            {
                get; set;
            }
            public decimal? OfficialNameUniquenessScore
            {
                get; set;
            }

            public string? TradeName
            {
                get; set;
            }
            public decimal? TradeNameUniquenessScore
            {
                get; set;
            }
        }

        public class SentimentAnalysis
        {
            public string? Label
            {
                get; set;
            }
            public int? PeopleCount
            {
                get; set;
            }
            public int? PlacesCount
            {
                get; set;
            }
            public int? OrganizationsCount
            {
                get; set;
            }
            public Dictionary<string, object>? Entities
            {
                get; set;
            }
        }

        #endregion




        public MediaProfileAndExposure()
        {
        }

        public string? MediaExposureLevel
        {
            get; set;
        }
        public string? CelebrityLevel
        {
            get; set;
        }
        public string? UnpopularityLevel
        {
            get; set;
        }
        public List<NewsItem>? NewsItems { get; set; } = new List<NewsItem>();
        public DateTime? CreationDate
        {
            get; set;
        }
        public DateTime? LastUpdateDate
        {
            get; set;
        }
        public SearchLabel? SearchLabels
        {
            get; set;
        }
        public string? Next
        {
            get; set;
        }
        public int? TotalPages
        {
            get; set;
        }
        public EntityStatistic? EntityStatistics
        {
            get; set;
        }
    }
}
