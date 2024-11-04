using System;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Driver;

namespace UCode.Mongo
{
    //https://mongodb.github.io/mongo-csharp-driver/2.11/reference/driver/crud/writing/
    /// <summary>
    /// Represents a set of entities of type <typeparamref name="BsonObjectId"/> 
    /// for the context of a MongoDB database. This class inherits from 
    /// DbSet allowing interaction with a collection of documents within 
    /// a MongoDB database.
    /// </summary>
    /// <remarks>
    /// This class is specifically tailored to handle BsonObjectId types 
    /// as both the entity type and the key type. It provides functionalities 
    /// for querying, adding, updating, and deleting BsonObjectId entities 
    /// within the database.
    /// </remarks>
    public class RawDbSet : DbSet<BsonObjectId<BsonObjectId>, BsonObjectId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawDbSet"/> class.
        /// </summary>
        /// <param name="contextBase">
        /// The context base that this database set will be associated with.
        /// This parameter cannot be null.
        /// </param>
        /// <param name="collectionName">
        /// An optional name of the collection. If not provided, a default collection name will be used.
        /// </param>
        /// <param name="createCollectionOptionsAction">
        /// An optional action to configure the creation options for the collection.
        /// </param>
        /// <param name="mongoCollectionSettingsAction">
        /// An optional action to configure the settings for the MongoDB collection.
        /// </param>
        /// <param name="useTransaction">
        /// A boolean flag indicating whether to use a transaction when performing operations on the database.
        /// Default is false.
        /// </param>
        public RawDbSet([NotNull] ContextBase contextBase,
            string? collectionName = null,
            Action<CreateCollectionOptions>? createCollectionOptionsAction = null,
            Action<MongoCollectionSettings>? mongoCollectionSettingsAction = null,
            bool useTransaction = false) : base(contextBase, collectionName, createCollectionOptionsAction, mongoCollectionSettingsAction, useTransaction)
        {

        }

        /// <summary>
        /// Overrides the default implementation of the ToString method 
        /// to provide a string representation of the current object.
        /// Calls the base class's ToString method to retrieve the default string representation.
        /// </summary>
        /// <returns>
        /// A string that represents the current object. 
        /// Returns null if the base ToString method returns null.
        /// </returns>
        public override string? ToString() => base.ToString();
    }
}
