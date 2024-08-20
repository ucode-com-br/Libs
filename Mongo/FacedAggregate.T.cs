using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents the result of an aggregation operation on a collection.
    /// </summary>
    /// <typeparam name="T">The type of the projection.</typeparam>
    internal sealed class FacedAggregate<T>
    {
        /// <summary>
        /// Gets or sets the result of the aggregation operation.
        /// </summary>
        [BsonRequired]
        [BsonElement("result")]
        public IEnumerable<T> Result
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the total count of documents matching the query.
        /// </summary>
        [BsonRequired]
        [BsonElement("total")]
        public dynamic[] Total
        {
            get; set;
        }

        /// <summary>
        /// Returns the total number of rows in the result.
        /// </summary>
        /// <returns>The total number of rows.</returns>
        public int TotalRows()
        {
            // If the Total property is null or empty, return 0.
            if (this.Total == null || this.Total.Length <= 0)
            {
                return 0;
            }

            // Otherwise, return the total count from the first element of the Total array.
            return (int)this.Total[0].total;
        }
    }
}
