using Microsoft.AspNetCore.Mvc.Formatters;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace UCode.Mongo.Serializers
{
    /// <summary>
    /// Represents a formatter for BSON (Binary JSON) output in an ASP.NET application.
    /// This class is responsible for serializing objects into BSON format for HTTP responses.
    /// </summary>
    /// <remarks>
    /// This class inherits from the OutputFormatter base class, which provides the functionality 
    /// to format the HTTP response body.
    /// </remarks>
    public class BsonOutputFormatter : OutputFormatter
    {
        public BsonOutputFormatter()
        {
            this.SupportedMediaTypes.Add("application/bson");
            this.SupportedMediaTypes.Add("text/bson");
        }

        /// <summary>
        /// Verifica se o tipo de objeto é um dos tipos Bson suportados
        /// </summary>
        /// <param name="type">O tipo do objeto a ser escrito</param>
        /// <returns>true se suportado, caso contrário false</returns>
        protected override bool CanWriteType(Type type) => type == typeof(BsonDocument)
                || type == typeof(BsonValue)
                || type == typeof(BsonElement)
                || type == typeof(BsonArray);

        /// <summary>
        /// Retorna a lista de Content-Types suportados, dado um contentType e um tipo de objeto.
        /// </summary>
        public override IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            if (this.CanWriteType(objectType))
            {
                if (string.IsNullOrEmpty(contentType) ||
                    contentType.Contains("application/bson", StringComparison.OrdinalIgnoreCase) ||
                    contentType.Contains("text/bson", StringComparison.OrdinalIgnoreCase))
                {
                    return ["application/bson", "text/bson"];
                }
            }

            return [];
        }

        /// <summary>
        /// Serializa o objeto fornecido pelo contexto em BSON e grava no corpo da resposta.
        /// </summary>
        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var response = context.HttpContext.Response;

            // Determina o content-type a partir do contexto
            var selectedMediaType = context.ContentType.Value;
            if (string.IsNullOrEmpty(selectedMediaType))
            {
                selectedMediaType = "application/bson";
            }

            var obj = context.Object;

            using (var ms = new MemoryStream())
            using (var writer = new BsonBinaryWriter(ms))
            {
                // Serializa o objeto com base no seu tipo
                if (obj is BsonDocument doc)
                {
                    BsonSerializer.Serialize(writer, doc);
                }
                else if (obj is BsonArray arr)
                {
                    BsonSerializer.Serialize(writer, arr);
                }
                else if (obj is BsonValue val)
                {
                    // BsonValue não necessariamente é um documento, 
                    // mas podemos tentar converter em documento
                    if (val.IsBsonDocument)
                        BsonSerializer.Serialize(writer, val.AsBsonDocument);
                    else
                        BsonSerializer.Serialize(writer, val.ToBsonDocument());
                }
                else if (obj is BsonElement elem)
                {
                    // BsonElement não é serializável diretamente,
                    // serializamos o valor do elemento
                    if (elem.Value != null)
                        BsonSerializer.Serialize(writer, elem.Value);
                    else
                        BsonSerializer.Serialize(writer, new BsonDocument());
                }
                else
                {
                    // Caso não seja nenhum dos tipos específicos, tentamos serializar genericamente
                    BsonSerializer.Serialize(writer, obj);
                }

                response.ContentType = selectedMediaType;
                writer.Close();

                ms.Position = 0;
                await ms.CopyToAsync(response.Body);
            }
        }
    }

    /// <summary>
    /// Represents a BSON input formatter for processing incoming BSON data in ASP.NET applications.
    /// Derived from the InputFormatter class to provide custom behavior for reading BSON formatted data.
    /// </summary>
    /// <remarks>
    /// The <see cref="BsonInputFormatter"/> class is typically used in conjunction with ASP.NET Core MVC
    /// to handle BSON formatted request bodies, enabling seamless integration with applications that utilize BSON.
    /// </remarks>
    public class BsonInputFormatter : InputFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonInputFormatter"/> class.
        /// The constructor adds support for media types associated with BSON data.
        /// </summary>
        /// <remarks>
        /// This constructor sets up the formatter to recognize and handle 
        /// BSON content by adding the appropriate media types to the 
        /// <see cref="SupportedMediaTypes"/> collection.
        /// </remarks>
        /// <returns>
        /// A new instance of <see cref="BsonInputFormatter"/> with media types 
        /// "application/bson" and "text/bson" supported.
        /// </returns>
        public BsonInputFormatter()
        {
            this.SupportedMediaTypes.Add("application/bson");
            this.SupportedMediaTypes.Add("text/bson");
        }

        /// <summary>
        /// Determines whether the input formatter can read the given content based on the content type of the request.
        /// </summary>
        /// <param name="context">The context for the input formatter, containing information about the HTTP request.</param>
        /// <returns>
        /// Returns true if the content type is either "application/bson" or "text/bson"; otherwise, false.
        /// </returns>
        public override bool CanRead(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var contentType = request.ContentType;

            return !string.IsNullOrEmpty(contentType) &&
                (contentType.Contains("application/bson", StringComparison.OrdinalIgnoreCase) || contentType.Contains("text/bson", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Reads the request body and deserializes it into a <see cref="BsonDocument"/>.
        /// </summary>
        /// <param name="context">The <see cref="InputFormatterContext"/> that contains the HTTP context for the request.</param>
        /// <returns>
        /// A <see cref="Task{InputFormatterResult}"/> that represents the asynchronous operation,
        /// containing the result of deserializing the request body into a <see cref="BsonDocument"/>.
        /// </returns>
        public override async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            InputFormatterResult? result = null;
            var request = context.HttpContext.Request;

            using (var ms = new MemoryStream())
            {
                await request.Body.CopyToAsync(ms);
                ms.Position = 0;

                using (var reader = new BsonBinaryReader(ms))
                {
                    var doc = BsonSerializer.Deserialize<BsonDocument>(reader);

                    result = await InputFormatterResult.SuccessAsync(doc);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a list of supported content types based on the specified content type and object type.
        /// </summary>
        /// <param name="contentType">The content type to check against, may be null or empty.</param>
        /// <param name="objectType">The type of the object to check for BSON support.</param>
        /// <returns>
        /// A read-only list of supported content types. Returns a list containing "application/bson" and "text/bson"
        /// if the objectType is a BSON-related type and the contentType matches the corresponding media types.
        /// Otherwise, returns an empty list.
        /// </returns>
        public override IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            if (objectType == typeof(BsonDocument) ||
                objectType == typeof(BsonValue) ||
                objectType == typeof(BsonElement) ||
                objectType == typeof(BsonArray))
            {
                if (string.IsNullOrEmpty(contentType) ||
                    contentType.Contains("application/bson", StringComparison.OrdinalIgnoreCase) ||
                    contentType.Contains("text/bson", StringComparison.OrdinalIgnoreCase))
                {
                    return ["application/bson", "text/bson"];
                }
            }

            return [];
        }

        /// <summary>
        /// Asynchronously reads the request body and deserializes it into the specified model type.
        /// The deserialization supports BsonDocument, BsonValue, BsonElement, and BsonArray types.
        /// </summary>
        /// <param name="context">The context for the input formatter that provides access to the HTTP request and model type.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing an 
        /// <see cref="InputFormatterResult"/> which indicates the result of the read operation.
        /// </returns>
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            InputFormatterResult? result = null;

            var request = context.HttpContext.Request;

            using (var ms = new MemoryStream())
            {
                await request.Body.CopyToAsync(ms);
                ms.Position = 0;

                BsonDocument? doc = null;
                using (var reader = new BsonBinaryReader(ms))
                {
                    if (context.ModelType == typeof(BsonDocument))
                    {
                        doc = BsonSerializer.Deserialize<BsonDocument>(reader);
                    }
                    else if (context.ModelType == typeof(BsonValue))
                    {
                        doc = BsonSerializer.Deserialize<BsonValue>(reader).AsBsonDocument;
                    }
                    else if (context.ModelType == typeof(BsonDocument))
                    {
                        doc = BsonSerializer.Deserialize<BsonElement>(reader).Value?.AsBsonDocument;
                    }
                    else if (context.ModelType == typeof(BsonArray))
                    {
                        doc = BsonSerializer.Deserialize<BsonArray>(reader).AsBsonDocument;
                    }

                    result = await InputFormatterResult.SuccessAsync(doc);
                }
            }

            return result!;
        }
    }
}
