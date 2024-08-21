using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace UCode.Mongo
{
    internal sealed class FacedAggregate<T>
    {
        [BsonRequired]
        [BsonElement("result")]
        public IEnumerable<T> Result
        {
            get; set;
        }

        [BsonRequired]
        [BsonElement("total")]
        public dynamic[] Total
        {
            get; set;
        }

        public int TotalRows()
        {
            if (this.Total == null || this.Total.Length <= 0)
            {
                return 0;
            }

            return (int)this.Total[0].total;
        }
    }
}
