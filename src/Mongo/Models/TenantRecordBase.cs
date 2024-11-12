using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace UCode.Mongo.Models
{
    public abstract record TenantRecordBase<TObjectId, TUser> : ObjectRecordBase, IObjectBaseTenant, IObjectBase<TObjectId, TUser>
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

        [BsonId(IdGenerator = typeof(IdGenerator))]
        [BsonRepresentation(BsonType.String)]
        public TObjectId Id
        {
            get; set;
        }


        [BsonElement("created_by")]
        [JsonPropertyName("created_by")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        public TUser CreatedBy
        {
            get;
            set;
        }

        [BsonElement("created_at")]
        [JsonPropertyName("created_at")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        public DateTime CreatedAt
        {
            get;
            set;
        }

        [BsonElement("updated_by")]
        [JsonPropertyName("updated_by")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        public TUser? UpdatedBy
        {
            get;
            set;
        }

        [BsonElement("updated_at")]
        [JsonPropertyName("updated_at")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        public DateTime? UpdatedAt
        {
            get;
            set;
        }


        [BsonExtraElements]
        [BsonElement("extra_elements")]
        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        [BsonIgnoreIfNull]
        [JsonExtensionData]
        [JsonPropertyName("extra_elements")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object?>? ExtraElements
        {
            get; set;
        }


        [JsonPropertyName("disabled")]
        [BsonElement("disabled")]
        [BsonRequired]
        public bool Disabled
        {
            get; set;
        }



        public delegate void IdGeneratorCompletedEventHandler(IObjectBase<TObjectId, TUser> sender, IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs);


        public event IdGeneratorCompletedEventHandler IdGeneratorCompleted;


        public virtual void OnProcessCompleted(IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs) => IdGeneratorCompleted?.Invoke(this, eventArgs);

    }

    

    public abstract record TenantRecordBase<TObjectId> : TenantRecordBase<TObjectId, string>, IObjectBase<TObjectId>
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

    }

    public abstract record TenantRecordBase : TenantRecordBase<string, string>, IObjectBase, IObjectBaseTenant
    {

    }
}
