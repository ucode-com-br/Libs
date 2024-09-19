using System;
using MongoDB.Bson;

namespace UCode.Mongo
{
    public class BsonObjectId<TObjectId> : IObjectId<TObjectId>
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        private readonly BsonDocument _bsonDocument;

        /// <summary>
        /// Initializes a new instance of the BsonObjectId class.
        /// </summary>
        /// <param name="bsonDocument"></param>
        public BsonObjectId(BsonDocument bsonDocument)
        {
            _bsonDocument = bsonDocument;
        }

        public TObjectId Id
        {
            get => ConvertBson(this._bsonDocument.GetElement("_id").Value);
            set => _bsonDocument.SetElement(new BsonElement("_id", BsonValue.Create(value)));
        }

        /// <summary>
        /// Implicit operator that converts a BsonObjectId to a BsonDocument.
        /// </summary>
        /// <param name="objectId"></param>
        public static implicit operator BsonDocument(BsonObjectId<TObjectId> objectId) => objectId._bsonDocument;

        /// <summary>
        /// Implicit operator that converts a BsonDocument to a BsonObjectId.
        /// </summary>
        /// <param name="bsonDocument"></param>
        public static implicit operator BsonObjectId<TObjectId>(BsonDocument bsonDocument) => new(bsonDocument);

        /// <summary>
        /// Converts a BsonValue to an ObjectId.
        /// </summary>
        /// <param name="bsonValue"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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
