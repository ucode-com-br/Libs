using System;
using MongoDB.Bson;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;


namespace UCode.Mongo.Serializers
{
    /// <summary>
    /// Represents a converter that serializes and deserializes <see cref="BsonDocument"/> objects to and from JSON.
    /// This class inherits from <see cref="JsonConverter{T}"/> where T is <see cref="BsonDocument"/>.
    /// </summary>
    /// <remarks>
    /// This converter can be used with Newtonsoft.Json to customize the process 
    /// of converting MongoDB BSON documents to JSON format and vice versa.
    /// </remarks>
    public class BsonDocumentConverter : JsonConverter<BsonDocument>
    {
        /// <summary>
        /// Reads a BSON document from a JSON reader.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read the BSON data from.</param>
        /// <param name="typeToConvert">The <see cref="Type"/> of the object to convert.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> options for serialization.</param>
        /// <returns>
        /// A <see cref="BsonDocument"/> that contains the read BSON data.
        /// </returns>
        public override BsonDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = this.ReadBsonValue(ref reader);
            if (value is not null and BsonDocument doc)
            {
                return doc;
            }

            var json = "";
            using (var document = JsonDocument.ParseValue(ref reader))
            {
                json = document.RootElement.GetRawText();
            }
            return BsonDocument.Parse(json);
        }

        /// <summary>
        /// Writes the specified BsonDocument as a raw JSON value to the provided Utf8JsonWriter.
        /// </summary>
        /// <param name="writer">
        /// The Utf8JsonWriter to which the BsonDocument is written.
        /// </param>
        /// <param name="value">
        /// The BsonDocument that needs to be serialized to JSON.
        /// </param>
        /// <param name="options">
        /// Options that control the JSON serialization behavior.
        /// </param>
        public override void Write(Utf8JsonWriter writer, BsonDocument value, JsonSerializerOptions options) => writer.WriteRawValue(value.ToJson(), skipInputValidation: true);

        /// <summary>
        /// Reads a BSON value from the specified UTF-8 JSON reader.
        /// The method checks the current token type of the reader and processes it accordingly.
        /// It can handle various token types including objects, arrays, strings, numbers,
        /// booleans, and null values, returning the corresponding BsonValue.
        /// </summary>
        /// <param name="reader">The UTF-8 JSON reader from which to read the BSON value.</param>
        /// <returns>
        /// A <see cref="BsonValue"/> representing the BSON value read from the reader, 
        /// or null if the token type is not recognized or if no value is present.
        /// </returns>
        private BsonValue? ReadBsonValue(ref Utf8JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    return this.ReadBsonDocument(ref reader);
                case JsonTokenType.StartArray:
                    return this.ReadBsonArray(ref reader);
                case JsonTokenType.String:
                    return this.ReadStringValue(ref reader);
                case JsonTokenType.Number:
                    return this.ReadNumberValue(ref reader);
                case JsonTokenType.True:
                    reader.Read();
                    return BsonBoolean.True;
                case JsonTokenType.False:
                    reader.Read();
                    return BsonBoolean.False;
                case JsonTokenType.Null:
                    reader.Read();
                    return BsonNull.Value;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Reads a BSON document from the provided Utf8JsonReader.
        /// </summary>
        /// <param name="reader">The Utf8JsonReader to read from.</param>
        /// <returns>
        /// A BsonDocument containing the read key-value pairs,
        /// or null if an exception occurs during reading.
        /// </returns>
        private BsonDocument? ReadBsonDocument(ref Utf8JsonReader reader)
        {
            try
            {
                var doc = new BsonDocument();
                reader.Read();

                while (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var name = reader.GetString();
                    reader.Read();
                    var value = this.ReadBsonValue(ref reader);
                    doc.Add(name, value);
                }

                if (reader.TokenType != JsonTokenType.EndObject)
                    throw new JsonException("Spected end of JSON.");

                reader.Read(); // Avança além do EndObject
                return doc;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a BSON array from the provided <see cref="Utf8JsonReader"/> reference,
        /// parsing each element until the end of the array is reached.
        /// </summary>
        /// <param name="reader">
        /// The <see cref="Utf8JsonReader"/> used to read the BSON array from.
        /// </param>
        /// <returns>
        /// Returns a <see cref="BsonArray"/> containing the elements read from the JSON reader,
        /// or null if an exception occurs during the reading process.
        /// </returns>
        private BsonArray? ReadBsonArray(ref Utf8JsonReader reader)
        {
            try
            {

                var arr = new BsonArray();
                reader.Read();

                int counter = 0;
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    var value = this.ReadBsonValue(ref reader);
                    arr.Add(value);
                    counter++;
                }

                reader.Read();
                return arr;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a string value from the provided Utf8JsonReader and attempts to parse it into a BsonValue.
        /// If the string is null or whitespace, returns null. If the string represents a valid BsonDocument,
        /// it is parsed and returned. If it does not represent a valid BsonDocument but matches the ISODate pattern,
        /// it is converted to a BsonDateTime. Otherwise, the string is returned as a BsonString.
        /// </summary>
        /// <param name="reader">A reference to the Utf8JsonReader instance from which the string is read.</param>
        /// <returns>A BsonValue representing the parsed string, null if the string is null or whitespace.</returns>
        private BsonValue? ReadStringValue(ref Utf8JsonReader reader)
        {
            var str = reader.GetString();
            reader.Read();

            if (string.IsNullOrWhiteSpace(str))
                return null;

            try
            {
                return BsonDocument.Parse(str);
            }
            catch
            {
                // Se não for um documento válido, tenta tratar ISODate
                var isoMatch = Regex.Match(str, @"^ISODate\(\""(.*)\""\)$", RegexOptions.IgnoreCase);
                if (isoMatch.Success)
                {
                    var dateStr = isoMatch.Groups[1].Value;
                    if (DateTime.TryParse(dateStr, out var dt))
                    {
                        return new BsonDateTime(dt);
                    }
                }

                // Se não for ISODate, retorna como string normal
                return new BsonString(str);
            }
        }

        /// <summary>
        /// Reads a number value from the provided Utf8JsonReader. 
        /// Attempts to read the value as a long integer first, and if that fails, 
        /// tries to read it as a decimal. 
        /// </summary>
        /// <param name="reader">A reference to the Utf8JsonReader to read the number from.</param>
        /// <returns>
        /// Returns a BsonValue representing the number read, 
        /// which could be a BsonInt64 if an integer was successfully read, 
        /// a BsonDecimal128 if a decimal was successfully read, 
        /// or null if the value could not be read as either type.
        /// </returns>
        private BsonValue? ReadNumberValue(ref Utf8JsonReader reader)
        {
            // Tenta ler como inteiro, caso contrário como double
            if (reader.TryGetInt64(out var longValue))
            {
                reader.Read();
                return new BsonInt64(longValue);
            }

            if (reader.TryGetDecimal(out var doubleValue))
            {
                reader.Read();
                return new BsonDecimal128(doubleValue);
            }

            return null;
        }
    }
}
