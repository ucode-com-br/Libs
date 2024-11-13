using System.Collections.Generic;
using MongoDB.Bson.Serialization;

namespace UCode.Mongo
{
    public struct ContextCollectionMetadata
    {
        internal ContextCollectionMetadata(string collectionName)
        {
            this.CollectionName = collectionName;
        }

        public string CollectionName
        {
            get;
        }

        public object IndexKeys
        {
            get; internal set;
        }

        public IEnumerable<BsonClassMap> BsonClassMaps
        {
            get; internal set;
        }

        public IndexDefinition<TDocument> GetIndexKeys<TDocument>()
        {
            return (IndexDefinition<TDocument>)IndexKeys;
        }

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
