using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
        /// Initializes a new instance of the <see cref="DbSet{TDocument}"/> class.
        /// </summary>
        /// <param name="contextBase">The context base.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="options">The options.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DbSet([NotNull] ContextBase contextBase, string? collectionName = null, MongoDB.Driver.TimeSeriesOptions? options = null) : base(contextBase,
            collectionName, options)
        {

        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string? ToString() => base.ToString();
    }
}
