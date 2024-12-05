using System.Collections.Generic;
using MongoDB.Bson.Serialization;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents metadata for a collection context, including the collection name, index keys, and BSON class maps.
    /// </summary>
    public struct ContextCollectionMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContextCollectionMetadata"/> class.
        /// </summary>
        /// <param name="collectionName">
        /// The name of the collection associated with the context metadata.
        /// </param>
        internal ContextCollectionMetadata(string collectionName)
        {
            this.CollectionName = collectionName;
        }

        /// <summary>
        /// Gets the name of the collection.
        /// </summary>
        /// <value>
        /// A string representing the name of the collection.
        /// </value>
        public string CollectionName
        {
            get;
        }

        /// <summary>
        /// Gets or sets the index keys.
        /// </summary>
        /// <value>
        /// An object representing the index keys. The property is 
        public object IndexKeys
        {
            get; internal set;
        }

        /// <summary>
        /// Represents a collection of BSON class mappings for the BSON serialization.
        /// </summary>
        /// <value>
        /// An enumerable collection of <see cref="BsonClassMap"/> instances.
        /// </value>
        /// <remarks>
        /// This property is exposed with an 
        public IEnumerable<BsonClassMap> BsonClassMaps
        {
            get; internal set;
        }

        /// <summary>
        /// Retrieves the index keys associated with the current index definition.
        /// </summary>
        /// <typeparam name="TDocument">
        /// The type of the document for which the index keys are defined.
        /// </typeparam>
        /// <returns>
        /// An instance of <see cref="IndexDefinition{TDocument}"/> that represents the index keys for the specified document type.
        /// </returns>
        public IndexDefinition<TDocument> GetIndexKeys<TDocument>()
        {
            return (IndexDefinition<TDocument>)IndexKeys;
        }

        /// <summary>
        /// Retrieves a collection of BSON class maps for a specified document type.
        /// </summary>
        /// <typeparam name="TDocument">
        /// The type of the document for which the BSON class maps are being retrieved.
        /// </typeparam>
        /// <returns>
        /// A collection of BSON class maps for the specified document type.
        /// If there are no maps available, returns null.
        /// </returns>
        public IEnumerable<BsonClassMap<TDocument>>? GetBsonClassMaps<TDocument>()
        {
            var x = new List<BsonClassMap<TDocument>>();

            foreach (var item in BsonClassMaps)
            {
                x.Add((BsonClassMap<TDocument>)item);
            }

            return x;
        }
    }
}
