using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace UCode.Mongo.Models
{
    /// <summary>
    /// Represents an abstract base record for tenant records in a system.
    /// This record is generic over two types: TObjectId, which represents the identifier for the tenant,
    /// and TUser, which represents the user associated with the tenant.
    /// </summary>
    /// <typeparam name="TObjectId">The type of the object identifier, which must implement <see cref="IComparable{T}"/> and <see cref="IEquatable{T}"/>.</typeparam>
    /// <typeparam name="TUser">The type representing the user associated with the tenant.</typeparam>
    /// <remarks>
    /// This class implements <see cref="IObjectBaseTenant"/> and <see cref="IObjectBase{TObjectId, TUser}"/> 
    /// to enforce tenant-specific functionality and support for object base methods. 
    /// </remarks>
    /// <typeparamref name="TObjectId"/>
    /// <typeparamref name="TUser"/>
    public abstract record TenantRecordBase<TObjectId, TUser> : ObjectRecordBase, IObjectBaseTenant, IObjectBase<TObjectId, TUser>
                where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

        /// <summary>
        /// Represents the unique identifier of an object in BSON format.
        /// The identifier is generated using a specified IdGenerator.
        /// </summary>
        /// <value>
        /// The unique identifier of type <see cref="TObjectId"/>.
        /// </value>
        [BsonId(IdGenerator = typeof(IdGenerator))]
        [BsonRepresentation(BsonType.String)]
        public TObjectId Id
        {
            get; set;
        }


        /// <summary>
        /// Represents the user who created the entity. This property is serialized to JSON 
        /// with the name "created_by" and stored in the BSON format with the same name.
        /// If the property is not set, it will be ignored during JSON serialization.
        /// </summary>
        /// <value>
        /// The user who created the entity, or null if not specified.
        /// </value>
        /// <example>
        /// var entity = new MyEntity { CreatedBy = new User { Name = "John Doe" } };
        /// </example>
        [BsonElement("created_by")]
        [JsonPropertyName("created_by")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [BsonIgnoreIfNull]
        public TUser CreatedBy
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the date and time when the object was created.
        /// </summary>
        /// <remarks>
        /// This property is serialized with the name "created_at" in both BSON and JSON formats.
        /// The property will be ignored in JSON serialization if it holds the default value.
        /// It will also be ignored in BSON serialization if its value is null.
        /// </remarks>
        /// <value>
        /// A <see cref="DateTime"/> value representing the creation timestamp.
        /// </value>
        [BsonElement("created_at")]
        [JsonPropertyName("created_at")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [BsonIgnoreIfNull]
        public DateTime CreatedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the user who last updated the entity.
        /// </summary>
        /// <remarks>
        /// This property is decorated with attributes for serialization 
        /// in both BSON and JSON formats. It supports nullability,
        /// meaning it can be set to null if no user is assigned.
        /// </remarks>
        /// <returns>
        /// Returns an instance of type TUser or null if not assigned.
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
        /// Represents the date and time when the entity was last updated.
        /// This property is nullable and can hold a value of DateTime or be null.
        /// </summary>
        /// <remarks>
        /// The property is decorated with various attributes to facilitate compatibility
        /// with both MongoDB (Bson) and JSON serialization.
        /// </remarks>
        /// <value>
        /// A nullable <see cref="DateTime"/> representing the last update time.
        /// </value>
        /// <example>
        /// An example usage could be setting this property to the current date
        /// and time when an entity is updated.
        /// </example>
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
        /// Represents a collection of extra elements that can be serialized and deserialized 
        /// with additional attributes for handling JSON and BSON formats.
        /// </summary>
        /// <remarks>
        /// This property is used to store additional key-value pairs that are not defined 
        /// in the main class structure. It can be particularly useful for storing dynamic 
        /// data or when the structure of the data can change.
        /// </remarks>
        /// <returns>
        /// A dictionary containing extra elements, where the key is a string and the value 
        /// is an object that can be null.
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
        /// Represents the status of the feature, indicating whether it is disabled.
        /// </summary>
        /// <value>
        /// A boolean value where <c>true</c> indicates that the feature is disabled, and <c>false</c> indicates that it is enabled.
        /// </value>
        [JsonPropertyName("disabled")]
        [BsonElement("disabled")]
        [BsonRequired]
        public bool Disabled
        {
            get; set;
        }



        /// <summary>
        /// Represents a method that will handle the completion of an ID generation process.
        /// </summary>
        /// <typeparam name="TObjectId">The type of the object identifier.</typeparam>
        /// <typeparam name="TUser">The type of the user associated with the ID generation.</typeparam>
        /// <param name="sender">The source of the event, typically the object that generated the ID.</param>
        /// <param name="eventArgs">An object that contains the event data for the completion of the ID generation.</param>
        /// <remarks>
        /// This delegate is used to define a callback method that gets invoked when an ID generation operation is completed.
        /// </remarks>
        public delegate void IdGeneratorCompletedEventHandler(IObjectBase<TObjectId, TUser> sender, IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs);



        /// <summary>
        /// Occurs when the ID generation process is completed.
        /// </summary>
        /// <remarks>
        /// This event can be used to notify subscribers that the ID generation process has finished, 
        /// allowing them to handle the completion, such as updating UI elements or processing the generated ID.
        /// </remarks>
        /// <example>
        /// You can subscribe to this event like so:
        /// <code>
        /// idGenerator.IdGeneratorCompleted += new IdGeneratorCompletedEventHandler(OnIdGeneratorCompleted);
        /// </code>
        /// </example>
        public event IdGeneratorCompletedEventHandler IdGeneratorCompleted;


        /// <summary>
        /// Invoked when the process of generating an ID is completed.
        /// </summary>
        /// <param name="eventArgs">The event arguments containing details about the ID generation completion.</param>
        /// <remarks>
        /// This method is a virtual method allowing derived classes to provide specific 
        /// implementations for handling the completion of ID generation.
        /// </remarks>
        /// <example>
        /// <code>
        /// protected override void OnProcessCompleted(IdGeneratorCompletedEventArgs<int, string> eventArgs)
        /// {
        ///     // Custom logic here
        ///     base.OnProcessCompleted(eventArgs);
        /// }
        /// </code>
        /// </example>
        public virtual void OnProcessCompleted(IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs) => IdGeneratorCompleted?.Invoke(this, eventArgs);

    }



    /// <summary>
    /// Represents an abstract base class for tenant records with a generic identifier type.
    /// This class extends the functionality of <see cref="TenantRecordBase{TObjectId, TIdentifier}"/> 
    /// and implements the <see cref="IObjectBase{TObjectId}"/> interface.
    /// </summary>
    /// <typeparam name="TObjectId">
    /// The type of the object identifier, which must implement 
    /// <see cref="IComparable{T}"/> and <see cref="IEquatable{T}"/> 
    /// to ensure proper comparison and equality operations.
    /// </typeparam>
    public abstract record TenantRecordBase<TObjectId> : TenantRecordBase<TObjectId, string>, IObjectBase<TObjectId>
                    where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

    }

    /// <summary>
    /// Represents an abstract base class for tenant records.
    /// </summary>
    /// <remarks>
    /// This class is a generic extension of TenantRecordBase with string types for both key and value,
    /// and implements the IObjectBase and IObjectBaseTenant interfaces.
    /// </remarks>
    /// <typeparam name="TKey">The type of the key for the tenant record.</typeparam>
    /// <typeparam name="TValue">The type of the value for the tenant record.</typeparam>
    public abstract record TenantRecordBase : TenantRecordBase<string, string>, IObjectBase, IObjectBaseTenant
    {

    }
}
