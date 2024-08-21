using System;
using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for a MongoDB aggregate operation.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document being queried.</typeparam>
    public record AggregateOptions<TDocument> : IOptions
    {
        /// <summary>
        /// Gets or sets the collation for the aggregate operation.
        /// </summary>
        public Collation Collation
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the hint for the aggregate operation.
        /// </summary>
        public Query<TDocument> Hint
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the aggregate operation should use disk.
        /// </summary>
        public bool? AllowDiskUse
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the batch size for the aggregate operation.
        /// </summary>
        public int? BatchSize
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the comment for the aggregate operation.
        /// </summary>
        public string Comment
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the maximum await time for the aggregate operation.
        /// </summary>
        public TimeSpan? MaxAwaitTime
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the maximum time for the aggregate operation.
        /// </summary>
        public TimeSpan? MaxTime
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use string translation mode code points for the aggregate operation.
        /// </summary>
        public bool? StringTranslationModeCodePoints
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether document validation should be bypassed for the aggregate operation.
        /// </summary>
        public bool? BypassDocumentValidation
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the number of documents to skip for the aggregate operation.
        /// </summary>
        public int? Skip
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the maximum number of documents to return for the aggregate operation.
        /// </summary>
        public int? Limit
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the aggregate operation should not be performed in a transaction.
        /// </summary>
        public bool NotPerformInTransaction
        {
            get; set;
        }

        /// <summary>
        /// Represents the implicit conversion from <see cref="AggregateOptions{TDocument}"/> to <see cref="AggregateOptions"/>.
        /// </summary>
        /// <param name="source">The source <see cref="AggregateOptions{TDocument}"/> object.</param>
        /// <returns>The converted <see cref="AggregateOptions"/> object.</returns>
        public static implicit operator AggregateOptions(AggregateOptions<TDocument> source)
        {
            // If the source object is null, return the default aggregate options
            if (source == default)
            {
                return default;
            }

            // Create a new instance of AggregateOptions
            var result = new AggregateOptions
            {
                // Copy the properties from the source object
                Hint = source.Hint,
                Collation = source.Collation,
                AllowDiskUse = source.AllowDiskUse,
                BatchSize = source.BatchSize,
                BypassDocumentValidation = source.BypassDocumentValidation,
                Comment = source.Comment,
                MaxAwaitTime = source.MaxAwaitTime,
                MaxTime = source.MaxTime
            };

            // If the StringTranslationModeCodePoints property has a value, create a new ExpressionTranslationOptions object
            // and set the StringTranslationMode property based on the value of StringTranslationModeCodePoints
            if (source.StringTranslationModeCodePoints.HasValue)
            {
                result.TranslationOptions = new ExpressionTranslationOptions
                {
                    StringTranslationMode = source.StringTranslationModeCodePoints.Value
                        ? AggregateStringTranslationMode.CodePoints
                        : AggregateStringTranslationMode.Bytes
                };
            }
            // Return the converted aggregate options
            return result;
        }
    }
}
