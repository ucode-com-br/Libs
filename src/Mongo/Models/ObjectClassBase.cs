using System;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using UCode.Mongo.Serializers;
using MongoDB.Bson;

namespace UCode.Mongo.Models
{
    public abstract class ObjectClassBase : IObjectBaseTenant
    {

        [JsonPropertyName("ref")]
        [BsonElement("ref")]
        //[BsonSerializer(typeof(GuidAsStringSerializer))]
        //[BsonGuidRepresentation(GuidRepresentation.Standard)]
        [BsonRepresentation(BsonType.String)]
        public Guid Ref
        {
            get;
            set;
        } = Guid.NewGuid();


        [JsonPropertyName("tenant")]
        [BsonElement("tenant")]
        //[BsonSerializer(typeof(GuidAsStringSerializer))]
        //[BsonGuidRepresentation(GuidRepresentation.Standard)]
        [BsonRepresentation(BsonType.String)]
        public Guid Tenant
        {
            get;
            set;
        } = Guid.NewGuid();

    }


}
