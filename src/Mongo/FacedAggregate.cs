using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace UCode.Mongo
{

    internal sealed record FacedAggregate<T>
    {
        /// <summary>
        /// Represents the result of a database operation, containing a collection of items.
        /// </summary>
        /// <typeparam name="T">The type of items in the result collection.</typeparam>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> collection containing the result items.
        /// </returns>
        [BsonRequired]
        [BsonElement("result")]
        public IEnumerable<T> Result
        {
            get; set;
        }

        /// <summary>
        /// Represents an array of total values.
        /// </summary>
        /// <remarks>
        /// This property is marked as required and is serialized to the BSON element named "total".
        /// </remarks>
        /// <returns>
        /// A dynamic array containing the total values.
        /// </returns>
        [BsonRequired]
        [BsonElement("total")]
        public dynamic[] Total
        {
            get; set;
        }

        /// <summary>
        /// Calculates the total number of rows based on the Total property.
        /// </summary>
        /// <returns>
        /// The total count of rows as an integer. Returns 0 if the Total property is null or empty.
        /// </returns>
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
