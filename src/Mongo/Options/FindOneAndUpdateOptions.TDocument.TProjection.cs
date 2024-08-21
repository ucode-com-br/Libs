using System;
using MongoDB.Driver;

// ReSharper disable IdentifierTypo

namespace UCode.Mongo.Options
{
    public record FindOneAndUpdateOptions<TDocument, TProjection> : IOptions
    {
        //public IEnumerable<ArrayFilterDefinition> ArrayFilters { get; set; }
        public bool? BypassDocumentValidation
        {
            get; set;
        }
        public Collation Collation
        {
            get; set;
        }
        public Query<TDocument> Hint
        {
            get; set;
        }

        public bool IsUpsert
        {
            get; set;
        }
        public TimeSpan? MaxTime
        {
            get; set;
        }
        public Query<TDocument, TProjection> Projection
        {
            get; set;
        }

        /// <summary>
        ///     Gets or sets which version of the document to return.
        ///     True = ReturnDocument.Before
        ///     False = ReturnDocument.After
        /// </summary>
        public bool ReturnDocumentAfter
        {
            get; set;
        }

        public Query<TDocument> Sort
        {
            get; set;
        }
        public bool NotPerformInTransaction
        {
            get; set;
        }


        public static implicit operator MongoDB.Driver.FindOneAndUpdateOptions<TDocument, TProjection>(
            FindOneAndUpdateOptions<TDocument, TProjection> source)
        {
            if (source == default)
            {
                return default;
            }

            var result = new MongoDB.Driver.FindOneAndUpdateOptions<TDocument, TProjection>
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                Collation = source.Collation,
                Hint = source.Hint,
                IsUpsert = source.IsUpsert,
                MaxTime = source.MaxTime,
                Projection = source.Projection,
                ReturnDocument = source.ReturnDocumentAfter ? ReturnDocument.After : ReturnDocument.Before,
                Sort = source.Sort
            };

            return result;
        }
    }
}
