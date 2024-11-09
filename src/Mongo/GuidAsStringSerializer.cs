using System;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

namespace UCode.Mongo
{
    /// <summary>
    /// Provides methods to serialize and deserialize <see cref="Guid"/> values as lowercase strings in BSON format.
    /// </summary>
    public class GuidAsStringSerializer : SerializerBase<Guid>
    {
        /// <inheritdoc />
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Guid value) =>
            // Serializa o Guid como string
            context.Writer.WriteString(value.ToString().ToLower(System.Globalization.CultureInfo.InvariantCulture));

        /// <inheritdoc />
        public override Guid Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var currentBsonType = context.Reader.CurrentBsonType;

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

            throw new BsonSerializationException($"Cannot deserialize Guid from BsonType {currentBsonType}.");
        }
    }
}
