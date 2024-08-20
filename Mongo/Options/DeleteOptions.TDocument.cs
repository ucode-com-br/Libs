using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for a MongoDB delete operation.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document being queried.</typeparam>
    public record DeleteOptions<TDocument> : IOptions
    {
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
        /// Gets or sets a value indicating whether the delete operation should not be performed within a transaction.
        /// </summary>
        /// <value><c>true</c> if the delete operation should not be performed within a transaction; otherwise, <c>false</c>.</value>
        public bool NotPerformInTransaction
        {
            get; set;
        }

        /// <summary>
        /// Represents the implicit conversion from <see cref="DeleteOptions{TDocument}"/> to <see cref="DeleteOptions"/>.
        /// </summary>
        /// <param name="source">The source <see cref="DeleteOptions{TDocument}"/> object.</param>
        /// <returns>The converted <see cref="DeleteOptions"/> object.</returns>
        public static implicit operator DeleteOptions(DeleteOptions<TDocument> source)
        {
            // If the source object is null, return the default delete options
            if (source == default)
            {
                return default;
            }

            // Create a new instance of DeleteOptions
            var result = new DeleteOptions
            {
                // Set the Hint property
                Hint = source.Hint,

                // Set the Collation property
                Collation = source.Collation
            };

            return result;
        }

        /// <summary>
        /// Implicit conversion operator from <see cref="DeleteOptions{TDocument}"/> to <see cref="BulkWriteOptions"/>.
        /// </summary>
        /// <param name="source">The source <see cref="DeleteOptions{TDocument}"/> object.</param>
        /// <returns>The converted <see cref="BulkWriteOptions"/> object.</returns>
        public static implicit operator BulkWriteOptions(DeleteOptions<TDocument> source)
        {
            // If the source object is null, return the default bulk write options
            if (source == default)
            {
                return default;
            }

            // Create a new instance of BulkWriteOptions
            var result = new BulkWriteOptions
            {
                // Set the NotPerformInTransaction property
                NotPerformInTransaction = source.NotPerformInTransaction
            };

            return result;
        }
    }
}
