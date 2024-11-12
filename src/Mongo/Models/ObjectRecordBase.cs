using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SharpCompress.Compressors.Xz;
using UCode.Extensions;
using UCode.Mongo.Serializers;

namespace UCode.Mongo.Models
{
    public abstract record ObjectRecordBase : IObjectBaseTenant
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
