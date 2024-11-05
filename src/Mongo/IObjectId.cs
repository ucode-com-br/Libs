using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents a unique identifier for an object of type string.
    /// This interface extends a generic interface IObjectId with a type parameter of string.
    /// </summary>
    /// <remarks>
    /// The IObjectId interface is typically used in scenarios where objects need to be uniquely identified,
    /// such as in data storage or retrieval. By extending IObjectId<string>, it provides a strongly-typed
    /// contract ensuring that the identifier is consistently of the string type.
    /// </remarks>
    public interface IObjectId : IObjectId<string>
    {
    }

    /// <summary>
    /// Defines a generic interface for an object identifier that extends another
    /// interface, allowing for a specific type of object ID and a string representation.
    /// </summary>
    /// <typeparam name="TObjectId">The type of the object identifier which must be 
    /// comparable and equatable.</typeparam>
    /// <remarks>
    /// This interface can be used to enforce that types implementing it provide 
    /// specific behaviors for comparing and equating object IDs. The requirement 
    /// for <typeparamref name="TObjectId"/> to implement both <see cref="IComparable{T}"/> 
    /// and <see cref="IEquatable{T}"/> ensures that users of this interface can 
    /// perform these operations on object IDs of the specified type.
    /// </remarks>
    public interface IObjectId<TObjectId> : IObjectId<TObjectId, string>
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

    }


    /// <summary>
    /// Defines an interface for objects that have a unique ID and track user creation and modification information.
    /// </summary>
    /// <typeparam name="TObjectId">The type of the object's ID, which must be comparable and equatable.</typeparam>
    /// <typeparam name="TUser">The type of the user who created and updated the object.</typeparam>
    public interface IObjectId<TObjectId, TUser>
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        /// <summary>
        /// Gets or sets the ID of the object.
        /// </summary>
        [BsonId]
        TObjectId Id
        {
            get; set;
        }



        /// <summary>
        /// Gets or sets the user who created the object.
        /// </summary>
        /// <remarks>
        /// This property is decorated with BsonElement and JsonPropertyName attributes, 
        /// indicating how this property should be serialized/deserialized 
        /// when working with MongoDB and JSON.
        /// </remarks>
        /// <typeparam name="TUser">The type of the user who created the object.</typeparam>
        /// <returns>
        /// The user who created the object.
        /// </returns>
        [BsonElement("createdBy")]
        [JsonPropertyName("createdBy")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        TUser? CreatedBy
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the date and time when the object was created.
        /// </summary>
        /// <remarks>
        /// This property is decorated with both BsonElement and JsonPropertyName attributes
        /// to ensure compatibility with BSON (used in MongoDB) and JSON serialization/deserialization, respectively.
        /// </remarks>
        /// <returns>
        /// A DateTime value representing the creation timestamp.
        /// </returns>
        [BsonElement("createdAt")]
        [JsonPropertyName("createdAt")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        DateTime? CreatedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the user who updated the entity.
        /// </summary>
        /// <remarks>
        /// This property is decorated with attributes for serialization to BSON and JSON formats.
        /// </remarks>
        /// <typeparam name="TUser">
        /// The type of the user who performed the update. It can be any class that conforms to the expected user representation.
        /// </typeparam>
        /// <returns>
        /// An instance of <typeparamref name="TUser"/> representing the user who updated the entity.
        /// </returns>
        [BsonElement("updatedBy")]
        [JsonPropertyName("updatedBy")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        TUser? UpdatedBy
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the date and time when the entity was last updated.
        /// </summary>
        /// <remarks>
        /// The UpdatedAt property is mapped to the "updatedAt" field in BSON and JSON formats.
        /// It facilitates data interchange between the database and the application.
        /// </remarks>
        /// <returns>
        /// A DateTime value representing the last update timestamp.
        /// </returns>
        [BsonElement("updatedAt")]
        [JsonPropertyName("updatedAt")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        DateTime? UpdatedAt
        {
            get;
            set;
        }






        /// <summary>
        /// Represents the extra BSON elements associated with this object.
        /// </summary>
        [BsonExtraElements]
        [BsonElement("extraElements")]
        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        [BsonIgnoreIfNull]
        [JsonExtensionData]
        [JsonPropertyName("extraElements")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        Dictionary<string, object?>? ExtraElements
        {
            get; set;
        }




        /// <summary>
        /// Soft delete, indicating whether the feature is disabled.
        /// </summary>
        /// <value>
        /// Returns true if the feature is disabled; otherwise, false.
        /// </value>
        [JsonPropertyName("disabled")]
        [BsonElement("disabled")]
        [BsonRequired]
        bool Disabled
        {
            get; set;
        }


        [JsonPropertyName("ref")]
        [BsonElement("ref")]
        [BsonIgnoreIfNull]
        [BsonSerializer(typeof(GuidAsStringSerializer))]
        Guid? Ref { get; set; }


    }
}
