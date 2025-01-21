using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UCode.Mongo.Models
{
    /// <summary>
    /// Interface base para modelos MongoDB com:
    /// - Suporte a multi-tenancy
    /// - Identificação universal (GUID)
    /// - Separação lógica de dados por tenant
    /// - Serialização padronizada para operações CRUD
    /// </summary>
    /// <remarks>
    /// Implementação obrigatória para:
    /// - Coleções particionadas por cliente
    /// - Controle de acesso a nível de registro
    /// - Auditoria de dados transversal
    /// </remarks>
    public interface IObjectBaseTenant
    {
        /// <summary>
        /// Represents a reference identifier for an entity. 
        /// This property is decorated with attributes for JSON and BSON 
        /// serialization, indicating how it should be serialized to 
        /// different formats.
        /// </summary>
        /// <value>
        /// A <see cref="Guid"/> that uniquely identifies the reference.
        /// </value>
        /// <remarks>
        /// The property is serialized as a string in BSON and JSON formats.
        /// </remarks>
        [JsonPropertyName("ref")]
        [BsonElement("ref")]
        [BsonRepresentation(BsonType.String)]
        Guid Ref
        {
            get; set;
        }

        /// <summary>
        /// Represents the tenant identifier for a specific entity or record.
        /// This property is decorated with attributes for serialization 
        /// and mapping to both JSON and BSON formats.
        /// </summary>
        /// <value>
        /// A <see cref="Guid"/> that uniquely identifies the tenant.
        /// </value>
        [JsonPropertyName("tenant")]
        [BsonElement("tenant")]
        [BsonRepresentation(BsonType.String)]
        Guid Tenant 
        { 
            get; set; 
        }

    }

}
