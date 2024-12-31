using MongoDB.Driver;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents the definition of an index for a document type in a database.
    /// </summary>
    /// <typeparam name="TDocument">
    /// The type of the document that this index definition is associated with.
    /// </typeparam>
    public struct IndexDefinition<TDocument>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexDefinition{TDocument}"/> class.
        /// </summary>
        /// <param name="indexKeysDefinition">
        /// The definition of the index keys for the index. This specifies which fields in the document will be indexed.
        /// </param>
        /// <param name="options">
        /// The options for creating the index, such as the index name and whether it should be unique or not.
        /// </param>
        /// <returns>
        /// A new instance of the <see cref="IndexDefinition{TDocument}"/> class.
        /// </returns>
        public IndexDefinition(IndexKeysDefinition<TDocument> indexKeysDefinition, CreateIndexOptions<TDocument> options)
        {
            this.IndexKeysDefinition = indexKeysDefinition;
            this.IndexOptions = options;
        }

        /// <summary>
        /// Gets or initializes the definition of index keys applicable to the document type.
        /// </summary>
        /// <typeparam name="TDocument">
        /// The type of the document that the index definition is associated with.
        /// </typeparam>
        /// <returns>
        /// An instance of <see cref="IndexKeysDefinition{TDocument}"/> that defines the keys 
        /// to be used for the index.
        /// </returns>
        public IndexKeysDefinition<TDocument> IndexKeysDefinition
        {
            get; set;
        }

        /// <summary>
        /// Gets or initializes the options for creating an index.
        /// </summary>
        /// <value>
        /// An instance of <see cref="CreateIndexOptions"/> representing the options for index creation.
        /// </value>
        public CreateIndexOptions<TDocument> IndexOptions
        {
            get; set;
        }


        /// <summary>
        /// Defines an implicit conversion operator from an <see cref="IndexDefinition{TDocument}"/> to a 
        /// <see cref="CreateIndexOptions"/> object. 
        /// </summary>
        /// <param name="source">
        /// The <see cref="IndexDefinition{TDocument}"/> instance to convert.
        /// </param>
        /// <returns>
        /// A <see cref="CreateIndexOptions"/> obtained from the <paramref name="source"/>. 
        /// If the <paramref name="source"/> does not have associated index options, 
        /// a new instance of <see cref="CreateIndexOptions"/> is returned.
        /// </returns>
        public static implicit operator CreateIndexOptions(IndexDefinition<TDocument> source) => source.IndexOptions ?? new CreateIndexOptions() { };

        /// <summary>
        /// Implicitly converts an <see cref="IndexDefinition{TDocument}"/> to an 
        /// <see cref="IndexKeysDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="source">The source index definition to convert from.</param>
        /// <returns>An equivalent <see cref="IndexKeysDefinition{TDocument}"/>.</returns>
        public static implicit operator IndexKeysDefinition<TDocument>(IndexDefinition<TDocument> source) => source.IndexKeysDefinition;

        /// <summary>
        /// Implicitly converts an instance of <see cref="IndexDefinition{TDocument}"/> 
        /// to an instance of <see cref="CreateIndexModel{TDocument}"/>.
        /// </summary>
        /// <param name="source">
        /// The <see cref="IndexDefinition{TDocument}"/> instance to be converted.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="CreateIndexModel{TDocument}"/> 
        /// initialized with the provided <paramref name="source"/>.
        /// </returns>
        public static implicit operator CreateIndexModel<TDocument>(IndexDefinition<TDocument> source) => new CreateIndexModel<TDocument>(source, source);
    }
}
