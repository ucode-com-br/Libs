using System;
using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    public record AggregateOptions<TDocument> : IOptions
    {
        public Collation Collation
        {
            get; set;
        }
        public Query<TDocument> Hint
        {
            get; set;
        }
        public bool? AllowDiskUse
        {
            get; set;
        }
        public int? BatchSize
        {
            get; set;
        }
        public string Comment
        {
            get; set;
        }
        public TimeSpan? MaxAwaitTime
        {
            get; set;
        }
        public TimeSpan? MaxTime
        {
            get; set;
        }
        public bool? StringTranslationModeCodePoints
        {
            get; set;
        }
        public bool? BypassDocumentValidation
        {
            get; set;
        }

        public int? Skip
        {
            get; set;
        }
        public int? Limit
        {
            get; set;
        }
        public bool NotPerformInTransaction
        {
            get; set;
        }

        public static implicit operator AggregateOptions(AggregateOptions<TDocument> source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new AggregateOptions
            {
                Hint = source.Hint,
                Collation = source.Collation,
                AllowDiskUse = source.AllowDiskUse,
                BatchSize = source.BatchSize,
                BypassDocumentValidation = source.BypassDocumentValidation,
                Comment = source.Comment,
                MaxAwaitTime = source.MaxAwaitTime,
                MaxTime = source.MaxTime
            };


            if (source.StringTranslationModeCodePoints.HasValue)
            {
                result.TranslationOptions = new ExpressionTranslationOptions
                {
                    StringTranslationMode = source.StringTranslationModeCodePoints.Value
                        ? AggregateStringTranslationMode.CodePoints
                        : AggregateStringTranslationMode.Bytes
                };
            }

            return result;
        }
    }
}
