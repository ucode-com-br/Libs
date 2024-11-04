using System;
using MongoDB.Bson;

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
    public class BsonObjectId<TObjectId> : IObjectId<TObjectId>
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
