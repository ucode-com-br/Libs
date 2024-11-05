using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace UCode.MongoTests
{
    public record IdStringCollectionRecord: Mongo.IObjectId
    {
        public string Id
        {
            get;
            set;
        }

        public string MyProperty1
        {
            get;
            set;
        }
        public int MyProperty2
        {
            get;
            set;
        }
        public byte MyProperty3
        {
            get;
            set;
        }

        public List<string> MyProperty4
        {
            get;
            set;
        } = new List<string>();


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
    }
}
