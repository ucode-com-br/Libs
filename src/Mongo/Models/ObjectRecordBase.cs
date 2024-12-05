using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UCode.Mongo.Models
{
    public abstract record ObjectRecordBase : IObjectBaseTenant
    {
        /// <summary>
        /// Represents a unique reference identifier as a GUID.
        /// This property is serialized using JSON with the specified name "ref",
        /// and it is also mapped to a BSON element with the same name.
        /// </summary>
        /// <returns>
        /// A <see cref="Guid"/> representing the reference identifier.
        /// </returns>
        /// <remarks>
        /// The property is initialized with a new GUID by default, which ensures
        /// that it has a unique value unless otherwise specified.
        /// </remarks>
        [JsonPropertyName("ref")]
        [BsonElement("ref")]
        [BsonRepresentation(BsonType.String)]
        public Guid Ref
        {
            get;
            set;
        } = Guid.NewGuid();


        /// <summary>
        /// Represents the tenant identifier for an entity, using a globally unique identifier (GUID).
        /// This property is serialized with the JSON property name "tenant" and stored in MongoDB 
        /// with the element name "tenant". It is represented as a string in BSON format.
        /// </summary>
        /// <returns>
        /// A <see cref="Guid"/> representing the tenant.
        /// </returns>
        [JsonPropertyName("tenant")]
        [BsonElement("tenant")]
        [BsonRepresentation(BsonType.String)]
        public Guid Tenant
        {
            get;
            set;
        } = Guid.NewGuid();

    }


}
