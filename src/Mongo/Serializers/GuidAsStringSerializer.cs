using System;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

namespace UCode.Mongo.Serializers
{
    /// <summary>
    /// Provides methods to serialize and deserialize <see cref="Guid"/> values as lowercase strings in BSON format.
    /// </summary>
    public class GuidAsStringSerializer : SerializerBase<Guid>
    {
        /// <inheritdoc />
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Guid value)
        {
            if (value == Guid.Empty)
            {
                context.Writer.WriteString(null);
            }
            else
            {
                var guid = value.ToString().ToLower(System.Globalization.CultureInfo.InvariantCulture);

                context.Writer.WriteString(guid);
            }
        }

        /// <inheritdoc />
        public override Guid Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var currentBsonType = context.Reader.CurrentBsonType;

            try
            {

                if (currentBsonType == BsonType.Null)
                {
                    return Guid.Empty;
                }

                if (currentBsonType == BsonType.String)
                {
                    // Deserializa a string como Guid
                    var stringValue = context.Reader.ReadString();

                    return string.IsNullOrWhiteSpace(stringValue) ? Guid.Empty : Guid.Parse(stringValue.ToLower(System.Globalization.CultureInfo.InvariantCulture));
                }

                if (currentBsonType == BsonType.Binary)
                {
                    // Deserializa o binário como Guid
                    var binaryValue = context.Reader.ReadBinaryData();

                    if (binaryValue.IsGuid)
                        return binaryValue.AsGuid;

                    if (binaryValue.SubType == BsonBinarySubType.UuidLegacy)
                        return binaryValue.ToGuid();

                    if (binaryValue.SubType == BsonBinarySubType.UuidStandard)
                        return new Guid(binaryValue.Bytes);

                    throw new BsonSerializationException("Unsupported binary subtype for Guid.");
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            throw new BsonSerializationException($"Cannot deserialize Guid from BsonType {currentBsonType}.");
        }
    }
}
