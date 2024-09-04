//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using MongoDB.Bson;

//namespace UCode.Mongo
//{
//    public record SeedCollections<TContext, TDocument, TObjectId> : IReadOnlyDictionary<string, IList<TDocument>>
//        where TDocument : IObjectId<TObjectId>
//        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
//        where TContext : ContextBase
//    {

//        private IDictionary<DbSet<TDocument, TObjectId>, IList<TDocument>>? collection = new Dictionary<DbSet<TDocument, TObjectId>, IList<TDocument>>();

//        internal SeedCollections(TContext contextBase)
//        {
//            var contextType = typeof(TContext);

//            var contextFields = contextType.GetFields();

//            foreach (var field in contextFields)
//            {
//                var fieldType = field.FieldType;

//                if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(DbSet<>))
//                {
//                    var tDocument = fieldType.GetGenericArguments()[0];
//                    var tObject = fieldType.GetGenericArguments()[1];

//                    var fieldValue = field.GetValue(contextBase);

//                    collection.Add(fieldValue as DbSet<TDocument, TObjectId>, new List<TDocument>());
//                }
//            }



//            var contextProperties = contextType.GetProperties();

//            foreach (var prop in contextProperties)
//            {
//                var propType = prop.PropertyType;

//                if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(DbSet<>))
//                {
//                    var tDocument = propType.GetGenericArguments()[0];
//                    var tObject = propType.GetGenericArguments()[1];

//                    var propValue = prop.GetValue(contextBase);

//                    collection.Add(propValue as DbSet<TDocument, TObjectId>, new List<TDocument>());
//                }
//            }

//        }

//        public IList<TDocument> this[string key] => this.collection.First(f => f.Key.CollectionName == key).Value;

//        public IEnumerable<string> Keys => this.collection.Keys.Select(s => s.CollectionName);

//        public IEnumerable<IList<TDocument>> Values => this.collection.Values;

//        public int Count => this.collection.Count;

//        public bool ContainsKey(string key) => this.collection.Any(f => f.Key.CollectionName == key);

//        public IEnumerator<KeyValuePair<string, IList<TDocument>>> GetEnumerator() =>
//            this.collection.Select(s => new KeyValuePair<string, IList<TDocument>>(s.Key.CollectionName, s.Value)).AsEnumerable().GetEnumerator();

//        public bool TryGetValue(string key, [MaybeNullWhen(false)] out IList<TDocument> value) => this.collection.TryGetValue(key, out value);

//        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
//    }
//}
