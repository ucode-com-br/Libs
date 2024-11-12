using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using UCode.Mongo.Serializers;

namespace UCode.Mongo.Models
{
    /// <summary>
    /// 
    /// </summary>
    public interface IObjectBaseTenant
    {
        [JsonPropertyName("ref")]
        [BsonElement("ref")]
        //[BsonSerializer(typeof(GuidAsStringSerializer))]
        //[BsonGuidRepresentation(GuidRepresentation.Standard)]
        [BsonRepresentation(BsonType.String)]
        Guid Ref
        {
            get; set;
        }

        [JsonPropertyName("tenant")]
        [BsonElement("tenant")]
        //[BsonSerializer(typeof(GuidAsStringSerializer))]
        //[BsonGuidRepresentation(GuidRepresentation.Standard)]
        [BsonRepresentation(BsonType.String)]
        Guid Tenant
        {
            get; set;
        }

    }

}
