using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents an abstract class for an object identifier, inheriting from a generic base class 
    /// that accepts two parameters of type string.
    /// </summary>
    /// <remarks>
    /// This class serves as a base for specific implementations of object identifiers, 
    /// providing a cohesive structure for handling identifiers within derived classes.
    /// </remarks>
    public abstract class ObjectIdClass : ObjectIdClass<string, string>
    {

    }

    /// <summary>
    /// Represents an abstract base class for an ObjectId that is parameterized by a type.
    /// This class is a generic version that takes a type parameter for the ObjectId and
    /// inherits from a more specialized version that defines the ObjectId as a string.
    /// </summary>
    /// <typeparam name="TObjectId">
    /// The type of the object identifier. This type parameter allows for flexibility
    /// in defining what kind of identifiers are used as ObjectId.
    /// </typeparam>
    public abstract class ObjectIdClass<TObjectId> : ObjectIdClass<TObjectId, string>, IObjectId<TObjectId>
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

    }

    /// <summary>
    /// Represents an object identifier that is associated with a specific type of object
    /// and a user. This class implements the <see cref="IObjectId{TObjectId, TUser}"/> interface.
    /// </summary>
    /// <typeparam name="TObjectId">The type of the object identifier.</typeparam>
    /// <typeparam name="TUser">The type of the user associated with the identifier.</typeparam>
    public abstract class ObjectIdClass<TObjectId, TUser> : IObjectId<TObjectId, TUser>
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

        /// <summary>
        /// Represents a method that will handle the completion of an ID generation process.
        /// </summary>
        /// <typeparam name="TObjectId">The type of the object ID.</typeparam>
        /// <typeparam name="TUser">The type of the user associated with the ID generation.</typeparam>
        /// <param name="sender">The source of the event; typically, the object that initiated the ID generation.</param>
        /// <param name="eventArgs">An instance of <see cref="IdGeneratorCompletedEventArgs{TObjectId, TUser}"/> that contains the event data.</param>
        /// <remarks>
        /// This delegate is used to define the signature for methods that will respond to the completion
        /// of an ID generation operation, providing relevant context through the event arguments.
        /// </remarks>
        public delegate void IdGeneratorCompletedEventHandler(IObjectId<TObjectId, TUser> sender, IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs);


        /// <summary>
        /// Occurs when the ID generation process is completed.
        /// </summary>
        /// <remarks>
        /// This event can be subscribed to in order to perform actions after an ID has been generated.
        /// </remarks>
        public event IdGeneratorCompletedEventHandler IdGeneratorCompleted;


        /// <summary>
        /// Invoked when the process is completed.
        /// </summary>
        /// <param name="eventArgs">Event arguments containing information about the completed process.</param>
        public virtual void OnProcessCompleted(IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs) => IdGeneratorCompleted?.Invoke(this, eventArgs);


        /// <summary>
        /// Represents the unique identifier for the object.
        /// </summary>
        /// <remarks>
        /// This property is of type <typeparamref name="TObjectId"/> and allows
        /// for getting and setting the identifier value.
        /// </remarks>
        /// <value>
        /// The unique identifier of the object.
        /// </value>
        [BsonId(IdGenerator = typeof(IdGenerator))]
        [BsonRepresentation(BsonType.String)]
        public TObjectId Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user who created the current entity.
        /// </summary>
        /// <value>
        /// A <typeparamref name="TUser"/> object representing the user who created the entity.
        /// </value>
        [BsonElement("created_by")]
        [JsonPropertyName("created_by")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TUser CreatedBy
        {
            get;
            set;
        } = default!;

        /// <summary>
        /// Gets or sets the date and time when the object was created.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> representing the creation date and time.
        /// </value>
        [BsonElement("created_at")]
        [JsonPropertyName("created_at")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime CreatedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user who last updated the entity.
        /// </summary>
        /// <value>
        /// A nullable instance of the user type <typeparamref name="TUser"/>. 
        /// If no user has updated the entity, this property will be null.
        /// </value>
        /// <remarks>
        /// This property is useful for tracking changes and maintaining an audit trail 
        /// in applications that require user accountability.
        /// </remarks>
        [BsonElement("updated_by")]
        [JsonPropertyName("updated_by")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TUser? UpdatedBy
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the date and time when the entity was last updated.
        /// This property is nullable and can represent a point in time or
        /// be null if the entity has never been updated.
        /// </summary>
        /// <value>
        /// A nullable <see cref="DateTime"/> representing the last update time,
        /// or null if the entity has not been updated.
        /// </value>
        [BsonElement("updated_at")]
        [JsonPropertyName("updated_at")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? UpdatedAt
        {
            get;
            set;
        }



        /// <summary>
        /// Gets or sets a value indicating whether the current object is disabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the object is disabled; otherwise, <c>false</c>.
        /// </value>
        [BsonElement("disabled")]
        [JsonPropertyName("disabled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool Disabled
        {
            get;
            set;
        }


        /// <summary>
        /// Represents a unique identifier (GUID) for the reference.
        /// </summary>
        /// <value>
        /// A <see cref="Guid"/> value representing the reference identifier.
        /// </value>
        [BsonElement("ref")]
        [JsonPropertyName("ref")]
        [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]
        [BsonRepresentation(BsonType.String)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonSerializer(typeof(GuidAsStringSerializer))]
        public Guid Ref
        {
            get;
            set;
        } = new Guid();

        /// <summary>
        /// Represents a collection of additional elements or properties that may be associated 
        /// with an object, where each element is identified by a string key and can hold 
        /// a value of any type or be null.
        /// </summary>
        /// <value>
        /// A dictionary containing key-value pairs where the key is a string and the value 
        /// is an object (which can be null). If there are no extra elements, this property 
        /// may be null.
        /// </value>
        [BsonElement("extra_elements")]
        [JsonPropertyName("extra_elements")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object?>? ExtraElements
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user who created the object.
        /// </summary>
        /// <value>
        /// An instance of type TUser representing the user that created the object.
        /// Returns null if the creator is not set.
        /// </value>
        /// <returns>
        /// The user who created the object or null if not set.
        /// </returns>
        TUser? IObjectId<TObjectId, TUser>.CreatedBy
        {
            get => this.CreatedBy;
            set
            {
                if (value == null)
                {
                    this.CreatedBy = value;
                }
            }
        }

        /// <summary>
        /// Represents the creation date and time of the object.
        /// This property is a nullable DateTime that allows for the storage of 
        /// a date and time value indicating when the object was created.
        /// </summary>
        /// <value>
        /// A nullable DateTime value indicating the creation date and time.
        /// If no value has been set, it will be null.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to set the CreatedAt property to a non-nullable 
        /// DateTime if there is a business rule violation.
        /// </exception>
        DateTime? IObjectId<TObjectId, TUser>.CreatedAt
        {
            get => this.CreatedAt;
            set
            {
                if (value.HasValue)
                {
                    this.CreatedAt = value.Value;
                }
            }
        }


        /// <summary>
        /// Gets or sets the reference identifier of the object.
        /// </summary>
        /// <value>
        /// A nullable Guid representing the reference identifier. If set, the value will be assigned
        /// to the Ref property if it has a value; otherwise, it will be ignored.
        /// </value>
        /// <exception cref="NullReferenceException">
        /// Thrown when attempting to access the Ref if it has not been initialized.
        /// </exception>
        Guid? IObjectId<TObjectId, TUser>.Ref
        {
            get => this.Ref;
            set
            {
                if (value.HasValue)
                {
                    this.Ref = value.Value;
                }
            }
        }
    }




    /// <summary>
    /// Represents an abstract record that inherits from a generic ObjectIdRecord.
    /// The ObjectIdRecord class is a specialized type that handles identifiers 
    /// as a pair of strings, which can be used in various data handling 
    /// scenarios where a unique identifier is needed.
    /// </summary>
    /// <remarks>
    /// This class cannot be instantiated directly and is meant to be 
    /// subclassed by other records that require the behavior of ObjectIdRecord 
    /// with string identifiers.
    /// </remarks>
    public abstract record ObjectIdRecord : ObjectIdRecord<string, string>, IObjectId
    {

    }

    /// <summary>
    /// Represents an abstract record that defines an identity for an object based on a specific type of object identifier.
    /// This class extends the functionality of the generic <see cref="ObjectIdRecord{TObjectId, string}"/>.
    /// </summary>
    /// <typeparam name="TObjectId">
    /// The type of the object identifier. This type must implement the <see cref="IComparable{T}"/> and 
    /// <see cref="IEquatable{T}"/> interfaces to ensure that objects of this type can be compared and checked for equality.
    /// </typeparam>
    public abstract record ObjectIdRecord<TObjectId> : ObjectIdRecord<TObjectId, string>, IObjectId<TObjectId>
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

    }

    /// <summary>
    /// Represents an object identifier that is associated with a specific type of object
    /// and a user. This class implements the <see cref="IObjectId{TObjectId, TUser}"/> interface.
    /// </summary>
    /// <typeparam name="TObjectId">The type of the object identifier.</typeparam>
    /// <typeparam name="TUser">The type of the user associated with the identifier.</typeparam>
    public abstract record ObjectIdRecord<TObjectId, TUser> : IObjectId<TObjectId, TUser>
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {



        /// <summary>
        /// Represents the unique identifier for the object.
        /// </summary>
        /// <remarks>
        /// This property is of type <typeparamref name="TObjectId"/> and allows
        /// for getting and setting the identifier value.
        /// </remarks>
        /// <value>
        /// The unique identifier of the object.
        /// </value>
        //[BsonId]
        [BsonId(IdGenerator = typeof(IdGenerator))]
        [BsonRepresentation(BsonType.String)]
        public TObjectId Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user who created the current entity.
        /// </summary>
        /// <value>
        /// A <typeparamref name="TUser"/> object representing the user who created the entity.
        /// </value>
        [BsonElement("created_by")]
        [JsonPropertyName("created_by")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        public TUser CreatedBy
        {
            get;
            set;
        } = default!;

        /// <summary>
        /// Gets or sets the date and time when the object was created.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> representing the creation date and time.
        /// </value>
        [BsonElement("created_at")]
        [JsonPropertyName("created_at")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        public DateTime CreatedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user who last updated the entity.
        /// </summary>
        /// <value>
        /// A nullable instance of the user type <typeparamref name="TUser"/>. 
        /// If no user has updated the entity, this property will be null.
        /// </value>
        /// <remarks>
        /// This property is useful for tracking changes and maintaining an audit trail 
        /// in applications that require user accountability.
        /// </remarks>
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
        /// This property is nullable and can represent a point in time or
        /// be null if the entity has never been updated.
        /// </summary>
        /// <value>
        /// A nullable <see cref="DateTime"/> representing the last update time,
        /// or null if the entity has not been updated.
        /// </value>
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
        /// Gets or sets a value indicating whether the current object is disabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the object is disabled; otherwise, <c>false</c>.
        /// </value>
        [BsonElement("disabled")]
        [JsonPropertyName("disabled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        public bool Disabled
        {
            get;
            set;
        }


        /// <summary>
        /// Represents a unique identifier (GUID) for the reference.
        /// </summary>
        /// <value>
        /// A <see cref="Guid"/> value representing the reference identifier.
        /// </value>
        [BsonElement("ref")]
        [JsonPropertyName("ref")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        public Guid Ref
        {
            get;
            set;
        }

        /// <summary>
        /// Represents a collection of additional elements or properties that may be associated 
        /// with an object, where each element is identified by a string key and can hold 
        /// a value of any type or be null.
        /// </summary>
        /// <value>
        /// A dictionary containing key-value pairs where the key is a string and the value 
        /// is an object (which can be null). If there are no extra elements, this property 
        /// may be null.
        /// </value>
        [BsonElement("extra_elements")]
        [JsonPropertyName("extra_elements")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [BsonIgnoreIfNull]
        public Dictionary<string, object?>? ExtraElements
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user who created the object.
        /// </summary>
        /// <value>
        /// An instance of type TUser representing the user that created the object.
        /// Returns null if the creator is not set.
        /// </value>
        /// <returns>
        /// The user who created the object or null if not set.
        /// </returns>
        TUser? IObjectId<TObjectId, TUser>.CreatedBy
        {
            get => this.CreatedBy;
            set
            {
                if (value == null)
                {
                    this.CreatedBy = value;
                }
            }
        }

        /// <summary>
        /// Represents the creation date and time of the object.
        /// This property is a nullable DateTime that allows for the storage of 
        /// a date and time value indicating when the object was created.
        /// </summary>
        /// <value>
        /// A nullable DateTime value indicating the creation date and time.
        /// If no value has been set, it will be null.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to set the CreatedAt property to a non-nullable 
        /// DateTime if there is a business rule violation.
        /// </exception>
        DateTime? IObjectId<TObjectId, TUser>.CreatedAt
        {
            get => this.CreatedAt;
            set
            {
                if (value.HasValue)
                {
                    this.CreatedAt = value.Value;
                }
            }
        }


        /// <summary>
        /// Gets or sets the reference identifier of the object.
        /// </summary>
        /// <value>
        /// A nullable Guid representing the reference identifier. If set, the value will be assigned
        /// to the Ref property if it has a value; otherwise, it will be ignored.
        /// </value>
        /// <exception cref="NullReferenceException">
        /// Thrown when attempting to access the Ref if it has not been initialized.
        /// </exception>
        Guid? IObjectId<TObjectId, TUser>.Ref
        {
            get => this.Ref;
            set
            {
                if (value.HasValue)
                {
                    this.Ref = value.Value;
                }
            }
        }



        /// <summary>
        /// Represents a method that will handle the completion of an ID generation process.
        /// </summary>
        /// <typeparam name="TObjectId">The type of the object ID.</typeparam>
        /// <typeparam name="TUser">The type of the user associated with the ID generation.</typeparam>
        /// <param name="sender">The source of the event; typically, the object that initiated the ID generation.</param>
        /// <param name="eventArgs">An instance of <see cref="IdGeneratorCompletedEventArgs{TObjectId, TUser}"/> that contains the event data.</param>
        /// <remarks>
        /// This delegate is used to define the signature for methods that will respond to the completion
        /// of an ID generation operation, providing relevant context through the event arguments.
        /// </remarks>
        public delegate void IdGeneratorCompletedEventHandler(IObjectId<TObjectId, TUser> sender, IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs);


        /// <summary>
        /// Occurs when the ID generation process is completed.
        /// </summary>
        /// <remarks>
        /// This event can be subscribed to in order to perform actions after an ID has been generated.
        /// </remarks>
        public event IdGeneratorCompletedEventHandler IdGeneratorCompleted;


        /// <summary>
        /// Invoked when the process is completed.
        /// </summary>
        /// <param name="eventArgs">Event arguments containing information about the completed process.</param>
        public virtual void OnProcessCompleted(IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs) => IdGeneratorCompleted?.Invoke(this, eventArgs);
    }


}
