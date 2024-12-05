using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace UCode.Mongo.Models
{
    /// <summary>
    /// Represents the base class for tenant-related objects, allowing for the identification 
    /// and management of tenant-specific data within a defined structure. This class serves 
    /// as a generic base class and has two type parameters: TObjectId for the type of identifier
    /// and TUser for the type of user associated with the tenant.
    /// </summary>
    /// <typeparam name="TObjectId">The type of the object identifier.</typeparam>
    /// <typeparam name="TUser">The type of the user associated with the tenant.</typeparam>
    /// <remarks>
    /// This abstract class implements the IObjectBaseTenant and IObjectBase interfaces, providing
    /// a foundation for creating tenant-aware objects in an application.
    /// </remarks>
    public abstract class TenantClassBase<TObjectId, TUser> : ObjectClassBase, IObjectBaseTenant, IObjectBase<TObjectId, TUser>
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

        /// <summary>
        /// Represents the unique identifier for an object in the database.
        /// This property uses BSON serialization and is represented as a string in the database.
        /// </summary>
        /// <value>
        /// An instance of <see cref="TObjectId"/> that uniquely identifies the object.
        /// </value>
        [BsonId(IdGenerator = typeof(IdGenerator))]
        [BsonRepresentation(BsonType.String)]
        public TObjectId Id
        {
            get; set;
        }


        /// <summary>
        /// Represents the user who created the entity.
        /// </summary>
        /// <remarks>
        /// This property uses attributes for serialization and deserialization with MongoDB and JSON.
        /// The property is ignored when the value is the default value for its type.
        /// </remarks>
        /// <typeparam name="TUser">
        /// The type of the user that created the entity.
        /// </typeparam>
        /// <returns>
        /// An instance of type <typeparamref name="TUser"/> representing the creator of the entity.
        /// </returns>
        [BsonElement("created_by")]
        [JsonPropertyName("created_by")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [BsonIgnoreIfDefault]
        public TUser CreatedBy
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the date and time when the entity was created.
        /// </summary>
        /// <remarks>
        /// This property is serialized with BSON and JSON, and it is ignored in serialization 
        /// if it is set to the default value. The default time is the epoch, specifically, 
        /// January 1, 0001 at 00:00:00 in UTC.
        /// </remarks>
        /// <value>
        /// A <see cref="DateTime"/> indicating the creation timestamp of the entity.
        /// </value>
        /// <returns>
        /// A <see cref="DateTime"/> representing the creation date and time.
        /// </returns>
        [BsonElement("created_at")]
        [JsonPropertyName("created_at")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [BsonIgnoreIfDefault]
        public DateTime CreatedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the user who updated the entity.
        /// </summary>
        /// <remarks>
        /// This property holds the information about the user responsible for the last update. 
        /// It may be null if the entity has not been updated by any user.
        /// </remarks>
        /// <returns>
        /// Returns an instance of the user type (TUser) who updated the entity.
        /// </returns>
        [BsonElement("updated_by")]
        [JsonPropertyName("updated_by")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        public TUser? UpdatedBy
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the date and time when the entity was last updated.
        /// This property is represented in both BSON (Binary JSON) and JSON formats, and it can be null,
        /// which indicates that there may not be an update timestamp available.
        /// </summary>
        /// <remarks>
        /// The property uses the following attributes:
        /// - BsonElement: Specifies the name of the property when serialized to BSON.
        /// - JsonPropertyName: Specifies the name of the property when serialized to JSON.
        /// - JsonIgnore: Specifies that the property should be ignored during serialization when its value is null.
        /// - BsonIgnoreIfNull: Specifies that the property should be ignored when serialized to BSON if its value is null.
        /// </remarks>
        /// <returns>
        /// A nullable <see cref="DateTime"/> representing the last updated timestamp, or null if it has not been set.
        /// </returns>
        [BsonElement("updated_at")]
        [JsonPropertyName("updated_at")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        public DateTime? UpdatedAt
        {
            get;
            set;
        }


        /// <summary>
        /// Represents additional elements that are not explicitly defined in the class.
        /// This property is serialized to and from a BSON document and JSON, allowing
        /// for flexibility in handling extra data in different formats.
        /// </summary>
        /// <remarks>
        /// The property utilizes various attributes to control serialization behavior,
        /// including BSON-specific attributes to manage dictionary representation and
        /// JSON attributes to ignore null values when serializing.
        /// </remarks>
        /// <returns>
        /// A dictionary containing key-value pairs where the key is a string and
        /// the value is an object that could potentially be null. If there are no
        /// extra elements, this property can be null.
        /// </returns>
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


        /// <summary>
        /// Gets or sets a value indicating whether the item is disabled.
        /// </summary>
        /// <remarks>
        /// This property is marked with JSON and BSON attributes to control serialization
        /// behavior in different formats. It is required in BSON representation.
        /// </remarks>
        /// <value>
        /// A boolean value where <c>true</c> indicates that the item is disabled,
        /// and <c>false</c> indicates that it is enabled.
        /// </value>
        [JsonPropertyName("disabled")]
        [BsonElement("disabled")]
        [BsonRequired]
        public bool Disabled
        {
            get; set;
        }



        /// <summary>
        /// Represents the method that will handle the completion of an ID generation process.
        /// </summary>
        /// <typeparam name="TObjectId">The type of the object ID.</typeparam>
        /// <typeparam name="TUser">The type of the user associated with the ID generation.</typeparam>
        /// <param name="sender">The source of the event, which is an instance of an object that implements <see cref="IObjectBase{TObjectId, TUser}"/>.</param>
        /// <param name="eventArgs">An instance of <see cref="IdGeneratorCompletedEventArgs{TObjectId, TUser}"/> that contains the event data.</param>
        public delegate void IdGeneratorCompletedEventHandler(IObjectBase<TObjectId, TUser> sender, IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs);



        /// <summary>
        /// This event is triggered when the ID generation is completed.
        /// </summary>
        /// <remarks>
        /// Subscribers to this event can handle the results of the ID generation process.
        /// </remarks>
        /// <example>
        /// The following example shows how to subscribe to the event:
        /// <code>
        /// idGenerator.IdGeneratorCompleted += (sender, e) => 
        /// {
        ///     // Handle the completed ID generation
        /// };
        /// </code>
        /// </example>
        public event IdGeneratorCompletedEventHandler IdGeneratorCompleted;

        /// <summary>
        /// Invoked when the process of generating an ID has been completed.
        /// </summary>
        /// <param name="eventArgs">An instance of <see cref="IdGeneratorCompletedEventArgs{TObjectId, TUser}"/> containing details about the completed event.</param>
        public virtual void OnProcessCompleted(IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs) => IdGeneratorCompleted?.Invoke(this, eventArgs);


    }

    /// <summary>
    /// Represents an abstract base class for tenant-specific object classes.
    /// This class extends the functionality of <see cref="TenantClassBase{TObjectId, TKey}"/> 
    /// while implementing both <see cref="IObjectBaseTenant"/> and <see cref="IObjectBase{TObjectId}"/> interfaces.
    /// </summary>
    /// <typeparam name="TObjectId">The type of the object ID used in the tenant class.</typeparam>
    /// <remarks>
    /// This is a generic abstract class that allows deriving classes to specify a different 
    /// ID type while using a predefined key type of string.
    /// </remarks>
    public abstract class TenantClassBase<TObjectId> : TenantClassBase<TObjectId, string>, IObjectBaseTenant, IObjectBase<TObjectId>
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

    }

    /// <summary>
    /// Represents the base class for tenant-related operations, providing
    /// a generic implementation that is specialized with string identifiers.
    /// This class is abstract and cannot be instantiated directly.
    /// It implements the IObjectBaseTenant and IObjectBase interfaces.
    /// </summary>
    /// <remarks>
    /// Derived classes must provide concrete implementations for 
    /// the abstract members of this class.
    /// </remarks>
    public abstract class TenantClassBase : TenantClassBase<string, string>, IObjectBaseTenant, IObjectBase
    {

    }
}
