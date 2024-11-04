using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MongoDB.Driver;

namespace UCode.Mongo
{
    //https://mongodb.github.io/mongo-csharp-driver/2.11/reference/driver/crud/writing/
    /// <summary>
    /// Represents a set of documents of type TDocument.
    /// </summary>
    /// <typeparam name="TDocument">The type of document in the set.</typeparam>
    public class DbSet<TDocument> : DbSet<TDocument, string>
        where TDocument : IObjectId<string>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DbSet"/> class.
        /// </summary>
        /// <param name="contextBase">The context base that is used to interact with the database.</param>
        /// <param name="collectionName">An optional name of the collection. If <c>null</c>, a default name is used.</param>
        /// <param name="createCollectionOptionsAction">An optional action to configure the collection creation options.</param>
        /// <param name="mongoCollectionSettingsAction">An optional action to configure the MongoDB collection settings.</param>
        /// <param name="useTransaction">Specifies whether to use transactions for operations on this collection.</param>
        /// <returns>
        /// A new instance of the <see cref="DbSet"/> class.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet([NotNull] ContextBase contextBase, string? collectionName = null,
                                    Action<CreateCollectionOptions>? createCollectionOptionsAction = null,
                                    Action<MongoCollectionSettings>? mongoCollectionSettingsAction = null,
                                    bool useTransaction = false) : base(contextBase, collectionName, createCollectionOptionsAction, mongoCollectionSettingsAction)
        {

        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string? ToString() => base.ToString();
    }
}
