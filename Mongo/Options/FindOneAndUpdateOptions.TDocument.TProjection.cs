using System;
using MongoDB.Driver;

// ReSharper disable IdentifierTypo

namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for a MongoDB findOneAndUpdate operation.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document being queried.</typeparam>
    /// <typeparam name="TProjection">The type of the projection.</typeparam>
    public record FindOneAndUpdateOptions<TDocument, TProjection> : IOptions
    {
        //public IEnumerable<ArrayFilterDefinition> ArrayFilters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to bypass document validation.
        /// </summary>
        /// <value>True if document validation should be bypassed; otherwise, false.</value>
        public bool? BypassDocumentValidation
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the collation for the query.
        /// </summary>
        /// <value>The collation for the query.</value>
        public Collation Collation
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the hint for the query.
        /// </summary>
        /// <value>The hint for the query.</value>
        public Query<TDocument> Hint
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to perform an upsert operation.
        /// </summary>
        /// <value>True if an upsert operation should be performed; otherwise, false.</value>
        public bool IsUpsert
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the maximum time for the query.
        /// </summary>
        /// <value>The maximum time for the query.</value>
        public TimeSpan? MaxTime
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the projection for the query.
        /// </summary>
        /// <value>The projection for the query.</value>
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

        /// <summary>
        /// Gets or sets the sort order for the query.
        /// </summary>
        /// <value>The sort order for the query.</value>
        public Query<TDocument> Sort
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to perform the operation in a transaction.
        /// </summary>
        /// <value>True if the operation should be performed in a transaction; otherwise, false.</value>
        public bool NotPerformInTransaction
        {
            get; set;
        }

        /// <summary>
        /// Implicit conversion operator from <see cref="FindOneAndUpdateOptions{TDocument, TProjection}"/> to <see cref="MongoDB.Driver.FindOneAndUpdateOptions{TDocument, TProjection}"/>.
        /// </summary>
        /// <param name="source">The source <see cref="FindOneAndUpdateOptions{TDocument, TProjection}"/> object.</param>
        /// <returns>The converted <see cref="MongoDB.Driver.FindOneAndUpdateOptions{TDocument, TProjection}"/> object.</returns>
        public static implicit operator MongoDB.Driver.FindOneAndUpdateOptions<TDocument, TProjection>(
            FindOneAndUpdateOptions<TDocument, TProjection> source)
        {
            // If the source object is null, return the default find one and update options
            if (source == default)
            {
                return default;
            }

            // Create a new instance of FindOneAndUpdateOptions
            var result = new MongoDB.Driver.FindOneAndUpdateOptions<TDocument, TProjection>
            {
                BypassDocumentValidation = source.BypassDocumentValidation,
                Collation = source.Collation,
                Hint = source.Hint,
                IsUpsert = source.IsUpsert,
                MaxTime = source.MaxTime,
                Projection = source.Projection,
                ReturnDocument = source.ReturnDocumentAfter ? ReturnDocument.After : ReturnDocument.Before,  // Set the ReturnDocument property based on the value of ReturnDocumentAfter
                Sort = source.Sort
            };

            return result;
        }
    }
}
