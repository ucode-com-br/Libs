using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace UCode.Mongo.Models
{

    public interface IObjectBase : IObjectBase<string>
    {

    }

    public interface IObjectBase<TObjectId> : IObjectBase<TObjectId, string>
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

    }


    public interface IObjectBase<TObjectId, TUser>
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        [BsonId(IdGenerator = typeof(IdGenerator))]
        [BsonRepresentation(BsonType.String)]
        TObjectId Id
        {
            get; set;
        }


        [BsonElement("created_by")]
        [JsonPropertyName("created_by")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        TUser CreatedBy
        {
            get;
            set;
        }

        [BsonElement("created_at")]
        [JsonPropertyName("created_at")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        DateTime CreatedAt
        {
            get;
            set;
        }

        [BsonElement("updated_by")]
        [JsonPropertyName("updated_by")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        TUser? UpdatedBy
        {
            get;
            set;
        }

        [BsonElement("updated_at")]
        [JsonPropertyName("updated_at")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        DateTime? UpdatedAt
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
        Dictionary<string, object?>? ExtraElements
        {
            get; set;
        }


        [JsonPropertyName("disabled")]
        [BsonElement("disabled")]
        [BsonRequired]
        bool Disabled
        {
            get; set;
        }




        //public delegate void IdGeneratorCompletedEventHandler(IObjectId<TObjectId, TUser> sender, IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs);

        //event IdGeneratorCompletedEventHandler IdGeneratorCompleted;

        void OnProcessCompleted(IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs);
    }

}
