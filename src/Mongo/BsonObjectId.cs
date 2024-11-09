using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents a BSON ObjectId type for MongoDB documents.
    /// This generic class allows for flexibility in defining the type 
    /// of the ObjectId, providing a strongly-typed usage within the 
    /// context of MongoDB operations.
    /// </summary>
    /// <typeparam name="TObjectId">
    /// The type of the ObjectId. It must adhere to the constraints 
    /// of the IObjectId interface, ensuring compatibility with BSON 
    /// serialization and MongoDB operations.
    /// </typeparam>
    public class BsonObjectId<TObjectId> : TenantClassBase<TObjectId>
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        /// <summary>
        /// Represents a BSON document that is used to store data in a structured format.
        /// The document is a read-only instance, ensuring that its state cannot be modified
        /// after initialization, promoting immutability and thread safety.
        /// </summary>
        private readonly BsonDocument _bsonDocument;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonObjectId"/> class using the specified BsonDocument.
        /// </summary>
        /// <param name="bsonDocument">The BsonDocument from which to initialize the BsonObjectId.</param>
        /// <returns>
        /// This constructor does not return a value. It instantiates the BsonObjectId with the provided BsonDocument.
        /// </returns>
        public BsonObjectId(BsonDocument bsonDocument)
        {
            _bsonDocument = bsonDocument;
        }

        /// <summary>
        /// Gets or sets the identifier for the object represented by this instance,
        /// stored in a BSON document. The identifier is retrieved from and set to
        /// the "_id" field of the BSON document.
        /// </summary>
        /// <returns>
        /// The identifier of type <typeparamref name="TObjectId"/>.
        /// </returns>
        /// <value>
        /// The value to set the identifier, of type <typeparamref name="TObjectId"/>.
        /// </value>
        public TObjectId Id
        {
            get => ConvertBson(this._bsonDocument.GetElement("_id").Value);
            set => _bsonDocument.SetElement(new BsonElement("_id", BsonValue.Create(value)));
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
        public string? CreatedBy
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
        public DateTime? CreatedAt
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
        public string? UpdatedBy
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
        public DateTime? UpdatedAt
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
        public Dictionary<string, object?>? ExtraElements
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is disabled.
        /// </summary>
        /// <value>
        /// True if the entity is disabled; otherwise, false.
        /// </value>
        public bool Disabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the reference identifier.
        /// </summary>
        /// <remarks>
        /// This property may hold a unique identifier of type <see cref="Guid"/> that can be null.
        /// It is intended to be used for categorizing or linking to another entity.
        /// </remarks>
        /// <value>
        /// A nullable <see cref="Guid"/> representing the reference identifier. 
        /// A value of <c>null</c> indicates that there is no reference identifier set.
        /// </value>
        public Guid? Ref
        {
            get;
            set;
        }


        /// <summary>
        /// Implicitly converts a <see cref="BsonObjectId{TObjectId}"/> to a <see cref="BsonDocument"/>.
        /// </summary>
        /// <param name="objectId">The <see cref="BsonObjectId{TObjectId}"/> instance to be converted.</param>
        /// <returns>A <see cref="BsonDocument"/> representation of the provided <see cref="BsonObjectId{TObjectId}"/>.</returns>
        public static implicit operator BsonDocument(BsonObjectId<TObjectId> objectId) => objectId._bsonDocument;

        /// <summary>
        /// Implicitly converts a <see cref="BsonDocument"/> to a <see cref="BsonObjectId{TObjectId}"/>.
        /// </summary>
        /// <param name="bsonDocument">The <see cref="BsonDocument"/> to be converted.</param>
        /// <returns>A new instance of <see cref="BsonObjectId{TObjectId}"/> representing the provided <see cref="BsonDocument"/>.</returns>
        public static implicit operator BsonObjectId<TObjectId>(BsonDocument bsonDocument) => new(bsonDocument);

        /// <summary>
        /// Converts a BsonValue to the target type TObjectId.
        /// </summary>
        /// <param name="bsonValue">The BsonValue to be converted.</param>
        /// <returns>
        /// The converted value of type TObjectId.
        /// Returns the default value of TObjectId if the bsonValue is null or represents a null value.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the bsonValue cannot be converted to TObjectId, 
        /// or if the BSON type is unsupported.
        /// </exception>
        private TObjectId ConvertBson(BsonValue bsonValue)
        {
            // Obter o tipo de destino
            Type targetType = typeof(TObjectId);

            // Verificar se o BsonValue é nulo ou representa um valor nulo
            if (bsonValue == null || bsonValue.IsBsonNull)
            {
                return default(TObjectId);
            }

            // Realizar a conversão baseada no tipo de destino
            if (targetType == typeof(string))
            {
                return (TObjectId)(object)bsonValue.AsString;
            }
            if (targetType == typeof(int))
            {
                return (TObjectId)(object)bsonValue.AsInt32;
            }
            if (targetType == typeof(long))
            {
                return (TObjectId)(object)bsonValue.AsInt64;
            }
            if (targetType == typeof(double))
            {
                return (TObjectId)(object)bsonValue.AsDouble;
            }
            if (targetType == typeof(bool))
            {
                return (TObjectId)(object)bsonValue.AsBoolean;
            }
            if (targetType == typeof(DateTime))
            {
                return (TObjectId)(object)bsonValue.ToUniversalTime();
            }
            if (targetType == typeof(Guid))
            {
                return (TObjectId)(object)bsonValue.AsGuid;
            }
            if (targetType == typeof(ObjectId))
            {
                return (TObjectId)(object)bsonValue.AsObjectId;
            }

            try
            {
                var json = bsonValue.ToJson();
                var converted = System.Text.Json.JsonSerializer.Deserialize<TObjectId>(json);

                if (converted == null)
                {
                    throw new InvalidOperationException($"Json convertion returned null: {json}");
                }

                return converted;
            }
            catch (Exception ex)
            {
                // Caso o tipo não seja suportado, lançar uma exceção
                throw new InvalidOperationException($"Not supported conversion type: {targetType.Name}", ex);
            }
        }
    }
}
