using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace UCode.Mongo.Models
{

    /// <summary>
    /// Represents a base interface for objects that utilize a string identifier.
    /// This interface inherits from a generic interface IObjectBase, 
    /// enforcing the use of a string type parameter for the identifier.
    /// </summary>
    public interface IObjectBase : IObjectBase<string>
    {

    }

    /// <summary>
    /// Defines a generic interface for an object base that takes an identifier type.
    /// This interface is derived from another interface which requires an additional 
    /// identifier type parameter of type string.
    /// </summary>
    /// <typeparam name="TObjectId">The type of the object identifier.</typeparam>
    public interface IObjectBase<TObjectId> : IObjectBase<TObjectId, string>
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

    }


    /// <summary>
    /// Represents a base interface for objects that have a specified identifier type and a user type.
    /// </summary>
    /// <typeparam name="TObjectId">The type of the identifier for the object.</typeparam>
    /// <typeparam name="TUser">The type of the user associated with the object.</typeparam>
    public interface IObjectBase<TObjectId, TUser>
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        /// <summary>
        /// Represents a unique identifier for the object, stored as a BSON type string.
        /// </summary>
        /// <remarks>
        /// This property is decorated with attributes to specify how it should be serialized
        /// and represented in BSON format. The IdGenerator type is used for generating the ID.
        /// </remarks>
        /// <returns>
        /// A string representation of the unique object ID.
        /// </returns>
        [BsonId(IdGenerator = typeof(IdGenerator))]
        [BsonRepresentation(BsonType.String)]
        TObjectId Id 
        { 
            get; set; 
        }


        /// <summary>
        /// Represents the user who created the entity.
        /// </summary>
        /// <remarks>
        /// This property is decorated with attributes for serialization with BSON and JSON,
        /// indicating that it can be ignored when it is null.
        /// </remarks>
        /// <value>
        /// The user that created the entity. It can be null if not set.
        /// </value>
        [BsonElement("created_by")]
        [JsonPropertyName("created_by")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        TUser CreatedBy
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the date and time when the object was created.
        /// </summary>
        /// <remarks>
        /// This property is decorated with attributes for BSON and JSON serialization.
        /// The <c>BsonElement</c> attribute specifies the name of the field in the BSON document,
        /// while the <c>JsonPropertyName</c> attribute specifies the name of the property
        /// during JSON serialization. The <c>JsonIgnore</c> attribute indicates that this 
        /// property should be ignored when its value is null. The <c>BsonIgnoreIfNull</c> 
        /// attribute is used to skip this field in BSON serialization if its value is null.
        /// </remarks>
        /// <returns>
        /// A <c>DateTime</c> representing the creation date and time.
        /// </returns>
        [BsonElement("created_at")]
        [JsonPropertyName("created_at")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        DateTime CreatedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the user who last updated the entity.
        /// </summary>
        /// <value>
        /// The user who updated the entity. This property can be null.
        /// </value>
        /// <remarks>
        /// The property is decorated with attributes for serialization with BSON and JSON.
        /// If the property is null, it will be ignored during serialization.
        /// </remarks>
        [BsonElement("updated_by")] 
        [JsonPropertyName("updated_by")] 
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
        /// This property is serialized to and from BSON and JSON with the name "updated_at".
        /// It is nullable, indicating that the update time may not always be set.
        /// If null, the property will be ignored during serialization.
        /// </remarks>
        /// <value>
        /// A nullable <see cref="DateTime"/> representing the last update time. 
        /// If the value is not specified, it defaults to null.
        /// </value>
        [BsonElement("updated_at")]
        [JsonPropertyName("updated_at")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        DateTime? UpdatedAt
        {
            get;
            set;
        }


        /// <summary>
        /// Represents a collection of extra elements that may be included in a document. 
        /// This property allows for the storage of additional key-value pairs that are not 
        /// explicitly defined in the class model. The elements are optional and can be 
        /// serialized/deserialized with various libraries.
        /// </summary>
        /// <returns>
        /// A dictionary where the key is a string and the value is an object that can be nullable.
        /// </returns>
        /// <remarks>
        /// The extra elements can be serialized as part of a BSON document and also 
        /// supported for JSON serialization. This property will be ignored during serialization 
        /// if its value is null, ensuring that only meaningful data is included in the serialized output.
        /// </remarks>
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


        /// <summary>
        /// Represents the state of a feature, indicating whether it is disabled or enabled.
        /// </summary>
        /// <remarks>
        /// This property is marked with <c>JsonPropertyName</c> for JSON serialization and <c>BsonElement</c> for BSON serialization.
        /// It is also required in BSON documents.
        /// </remarks>
        /// <returns>
        /// A <c>bool</c> value indicating if the feature is disabled (<c>true</c>) or enabled (<c>false</c>).
        /// </returns>
        [JsonPropertyName("disabled")]
        [BsonElement("disabled")]
        [BsonRequired]
        bool Disabled 
        { 
            get; set; 
        }




        //public delegate void IdGeneratorCompletedEventHandler(IObjectId<TObjectId, TUser> sender, IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs);

        //event IdGeneratorCompletedEventHandler IdGeneratorCompleted;

        /// <summary>
        /// Represents a delegate for handling the completion of an ID generation process.
        /// </summary>
        /// <param name="sender">
        /// The object that raised the event, implementing <see cref="IObjectId{TObjectId, TUser}"/>.
        /// </param>
        /// <param name="eventArgs">
        /// An instance of <see cref="IdGeneratorCompletedEventArgs{TObjectId, TUser}"/> containing event data.
        /// </param>
        void OnProcessCompleted(IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs);
    }

}
