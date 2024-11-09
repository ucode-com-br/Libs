using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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
        public IndexDefinition(IndexKeysDefinition<TDocument> indexKeysDefinition, CreateIndexOptions options)
        {
            IndexKeysDefinition = indexKeysDefinition;
            IndexOptions = options;
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
        public CreateIndexOptions IndexOptions
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



    /// <summary>
    /// Represents a collection of index keys for documents of type <typeparamref name="TDocument"/>.
    /// This class is typically used in database operations where documents need to be indexed for efficient 
    /// retrieval and search capabilities.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document that the index keys are associated with.</typeparam>
    public sealed class IndexKeys<TDocument>
    {
        #region private fields
        private readonly IndexKeysDefinitionBuilder<TDocument> _indexKeysDefinitionBuilder;

        private readonly List<IndexDefinition<TDocument>> _indexDefinitions = new List<IndexDefinition<TDocument>>();
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexKeys"/> class.
        /// This constructor takes an <see cref="IndexKeysDefinitionBuilder{TDocument}"/> as a parameter
        /// and passes it to the base constructor.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="IndexKeys"/> class.
        /// </returns>
        public IndexKeys() : this(new IndexKeysDefinitionBuilder<TDocument>())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexKeys"/> class.
        /// </summary>
        /// <param name="indexKeysDefinitionBuilder">
        /// The builder used to define the index keys for the document of type <typeparamref name="TDocument"/>.
        /// </param>
        /// <returns>
        /// This constructor does not return a value.
        /// </returns>
        internal IndexKeys(IndexKeysDefinitionBuilder<TDocument> indexKeysDefinitionBuilder)
        {
            _indexKeysDefinitionBuilder = indexKeysDefinitionBuilder;
        }

        /// <summary>
        /// Initializes a new instance of the IndexKeys class.
        /// </summary>
        /// <param name="indexKeysDefinitionBuilder">
        /// The builder used to define the index keys for the document.
        /// </param>
        /// <param name="indexDefinitions">
        /// A list of index definitions associated with the document.
        /// </param>
        /// <returns>
        /// This constructor does not return a value.
        /// </returns>
        internal IndexKeys(IndexKeysDefinitionBuilder<TDocument> indexKeysDefinitionBuilder, List<IndexDefinition<TDocument>> indexDefinitions)
        {
            _indexKeysDefinitionBuilder = indexKeysDefinitionBuilder;
            _indexDefinitions = indexDefinitions;
        }
        #endregion constructors

        #region private method
        /// <summary>
        /// Creates an instance of <see cref="CreateIndexOptions"/> and applies
        /// the specified configurations through an optional action delegate.
        /// </summary>
        /// <param name="action">
        /// An optional <see cref="Action{CreateIndexOptions}"/> that can be used to modify
        /// the <see cref="CreateIndexOptions"/> instance before it is returned. If no action
        /// is provided, default options will be returned.
        /// </param>
        /// <returns>
        /// A configured instance of <see cref="CreateIndexOptions"/> based on the provided action.
        /// </returns>
        private CreateIndexOptions CreateIndexOptions(Action<CreateIndexOptions>? action = default)
        {
            var options = new CreateIndexOptions();

            action?.Invoke(options);

            return options;
        }

        /// <summary>
        /// Creates an index definition for the specified document type.
        /// </summary>
        /// <param name="id">The index keys definition to be used for the index.</param>
        /// <param name="action">An optional action to configure the index options.</param>
        /// <returns>An instance of <see cref="IndexKeys{TDocument}"/> containing the defined index keys.</returns>
        private IndexKeys<TDocument> GetIndexDefinition(IndexKeysDefinition<TDocument> id, Action<CreateIndexOptions>? action = default)
        {
            var item = new IndexDefinition<TDocument>() { IndexKeysDefinition = id, IndexOptions = CreateIndexOptions(action) };

            _indexDefinitions.Add(item);
            //var newList = new List<IndexDefinition<TDocument>>();
            //newList.AddRange(_indexDefinitions);
            //newList.Add(item);
            //return new IndexKeys<TDocument>(this._indexKeysDefinitionBuilder, newList);

            return this;
        }
        #endregion private method


        /// <summary>
        /// Creates an ascending index definition for the specified field of a document.
        /// </summary>
        /// <param name="field">
        /// The field definition for which the ascending index should be created.
        /// </param>
        /// <param name="action">
        /// An optional action to configure the index options.
        /// </param>
        /// <returns>
        /// An instance of <see cref="IndexDefinition{TDocument}"/> representing the ascending index definition.
        /// </returns>
        public IndexKeys<TDocument> Ascending(FieldDefinition<TDocument> field, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Ascending(field);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Defines an ascending sort order for the specified field in the index.
        /// </summary>
        /// <param name="field">
        /// An expression representing the field for which the ascending order is defined.
        /// </param>
        /// <param name="action">
        /// An optional action to configure additional index creation options.
        /// </param>
        /// <returns>
        /// An <see cref="IndexDefinition{TDocument}"/> instance representing the index definition
        /// with the specified ascending sort order.
        /// </returns>
        public IndexKeys<TDocument> Ascending(Expression<Func<TDocument, object>> field, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Ascending(field);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Combines an array of index key definitions into a single index definition.
        /// </summary>
        /// <param name="keys">An array of <see cref="IndexKeysDefinition{TDocument}"/> elements to combine.</param>
        /// <param name="action">An optional action to configure the <see cref="CreateIndexOptions"/> for the combined index.</param>
        /// <returns>
        /// A combined <see cref="IndexDefinition{TDocument}"/> that represents the combined index keys and options.
        /// </returns>
        public IndexKeys<TDocument> Combine(IndexKeysDefinition<TDocument>[] keys, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Combine(keys);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Combines multiple index key definitions into a single index definition for the specified document type.
        /// </summary>
        /// <typeparam name="TDocument">
        /// The type of the document for which the index is defined.
        /// </typeparam>
        /// <param name="keys">
        /// A collection of index key definitions to combine.
        /// </param>
        /// <param name="action">
        /// An optional action that can be used to configure additional options for the index creation.
        /// </param>
        /// <returns>
        /// The combined <see cref="IndexDefinition{TDocument}"/> that includes all the specified index keys and options.
        /// </returns>
        public IndexKeys<TDocument> Combine(IEnumerable<IndexKeysDefinition<TDocument>> keys, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Combine(keys);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Configures the specified field to be indexed in descending order.
        /// </summary>
        /// <param name="field">The field to be indexed in descending order, represented as a <see cref="FieldDefinition{TDocument}"/>.</param>
        /// <param name="action">An optional action to customize the index creation options, represented as a <see cref="Action{CreateIndexOptions}"/>. If no action is provided, the default is used.</param>
        /// <returns>
        /// An <see cref="IndexDefinition{TDocument}"/> that represents the created index definition for the specified field configured in descending order.
        /// </returns>
        public IndexKeys<TDocument> Descending(FieldDefinition<TDocument> field, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Descending(field);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Defines a field in the index to be sorted in descending order.
        /// </summary>
        /// <typeparam name="TDocument">The type of the documents in the index.</typeparam>
        /// <param name="field">An expression that specifies the field to sort by.</param>
        /// <param name="action">An optional action to configure additional index options.</param>
        /// <returns>
        /// An <see cref="IndexDefinition{TDocument}"/> that represents the new index definition with the specified field sorted in descending order.
        /// </returns>
        public IndexKeys<TDocument> Descending(Expression<Func<TDocument, object>> field, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Descending(field);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Creates a 2D geospatial index definition for the specified field.
        /// </summary>
        /// <param name="field">
        /// The field definition for which the 2D geospatial index is created.
        /// </param>
        /// <param name="action">
        /// An optional action to further customize the index options.
        /// </param>
        /// <returns>
        /// Returns an <see cref="IndexDefinition{TDocument}"/> representing the 
        /// newly created 2D geospatial index.
        /// </returns>
        public IndexKeys<TDocument> Geo2D(FieldDefinition<TDocument> field, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Geo2D(field);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Creates a 2D Geospatial index for the specified field in the document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document being indexed.</typeparam>
        /// <param name="field">A lambda expression that identifies the field to index.</param>
        /// <param name="action">An optional action to configure additional index options.</param>
        /// <returns>An instance of <see cref="IndexDefinition{TDocument}"/> that represents the configured index.</returns>
        public IndexKeys<TDocument> Geo2D(Expression<Func<TDocument, object>> field, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Geo2D(field);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Creates a 2D sphere geospatial index definition for the specified field on a document of type <typeparamref name="TDocument"/>.
        /// </summary>
        /// <param name="field">The field definition representing the field to which the geospatial index will be applied.</param>
        /// <param name="action">An optional action to further customize the index creation options.</param>
        /// <returns>
        /// An <see cref="IndexDefinition{TDocument}"/> object representing the created geospatial index definition.
        /// </returns>
        public IndexKeys<TDocument> Geo2DSphere(FieldDefinition<TDocument> field, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Geo2DSphere(field);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Creates a 2dsphere index for the specified field of the document.
        /// This index is used for supporting queries that include geo-spatial data.
        /// </summary>
        /// <param name="field">An expression specifying the document field to index for 2dsphere queries.</param>
        /// <param name="action">An optional action to configure additional index options.</param>
        /// <returns>
        /// An <see cref="IndexDefinition{TDocument}"/> that describes the created 2dsphere index.
        /// </returns>
        public IndexKeys<TDocument> Geo2DSphere(Expression<Func<TDocument, object>> field, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Geo2DSphere(field);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Creates a hashed index for the specified field in the document type.
        /// </summary>
        /// <param name="field">
        /// The field for which the hashed index is to be created. This should be a 
        /// definition of the field represented by <see cref="FieldDefinition{TDocument}"/>.
        /// </param>
        /// <param name="action">
        /// An optional action that can be used to configure additional index options 
        /// during creation, encapsulated as a delegate of type <see cref="Action{CreateIndexOptions}"/>.
        /// The default value is null.
        /// </param>
        /// <returns>
        /// Returns an <see cref="IndexDefinition{TDocument}"/> that represents 
        /// the created hashed index for the specified field.
        /// </returns>
        public IndexKeys<TDocument> Hashed(FieldDefinition<TDocument> field, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Hashed(field);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Defines a hashed index on the specified field of the document.
        /// </summary>
        /// <param name="field">A lambda expression that specifies the field to be indexed.</param>
        /// <param name="action">An optional action to configure additional options for creating the index.</param>
        /// <returns>
        /// Returns an <see cref="IndexDefinition{TDocument}"/> that represents the hashed index definition.
        /// </returns>
        public IndexKeys<TDocument> Hashed(Expression<Func<TDocument, object>> field, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Hashed(field);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Creates a text index on the specified field of the document.
        /// </summary>
        /// <param name="field">
        /// The field definition for the document to create a text index on.
        /// </param>
        /// <param name="action">
        /// An optional action to apply additional options when creating the index.
        /// </param>
        /// <returns>
        /// Returns an <see cref="IndexDefinition{TDocument}"/> representing the text index that was created.
        /// </returns>
        public IndexKeys<TDocument> Text(FieldDefinition<TDocument> field, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Hashed(field);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Creates a text index definition for the specified field on the document type.
        /// </summary>
        /// <param name="field">
        /// An expression that specifies the field of the document to be indexed as text.
        /// </param>
        /// <param name="action">
        /// An optional action to customize the index creation options.
        /// </param>
        /// <returns>
        /// An IndexDefinition object representing the text index definition for the specified field.
        /// </returns>
        public IndexKeys<TDocument> Text(Expression<Func<TDocument, object>> field, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Text(field);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Creates an index definition using a wildcard for the specified field.
        /// </summary>
        /// <param name="field">
        /// The field definition on which the wildcard index is created.
        /// If null, defaults may be applied.
        /// </param>
        /// <param name="action">
        /// Optional action to customize the index creation options.
        /// This can be used to set additional parameters for the index.
        /// </param>
        /// <returns>
        /// An <see cref="IndexDefinition{TDocument}"/> representing the created index definition.
        /// </returns>
        public IndexKeys<TDocument> Wildcard(FieldDefinition<TDocument> field = null, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Text(field);

            return GetIndexDefinition(id, action);
        }

        /// <summary>
        /// Creates an index definition that includes a wildcard for the specified field.
        /// </summary>
        /// <param name="field">
        /// An expression that specifies the field to include in the index using a wildcard.
        /// </param>
        /// <param name="action">
        /// An optional action that allows for further customization of index creation options.
        /// </param>
        /// <returns>
        /// An <see cref="IndexDefinition{TDocument}"/> instance that represents the index definition
        /// including the specified wildcard field.
        /// </returns>
        public IndexKeys<TDocument> Wildcard(Expression<Func<TDocument, object>> field, Action<CreateIndexOptions>? action = default)
        {
            var id = _indexKeysDefinitionBuilder.Wildcard(field);

            return GetIndexDefinition(id, action);
        }


        #region implicit operator
        public static implicit operator IndexKeysDefinitionBuilder<TDocument>(IndexKeys<TDocument> source) => source._indexKeysDefinitionBuilder;

        public static implicit operator IndexKeys<TDocument>(IndexKeysDefinitionBuilder<TDocument> source) => new IndexKeys<TDocument>(source);

        public static implicit operator List<IndexDefinition<TDocument>>(IndexKeys<TDocument> source) => source._indexDefinitions;

        public static implicit operator List<CreateIndexModel<TDocument>>(IndexKeys<TDocument> source) => ((List<IndexDefinition<TDocument>>)source).Select(s => (CreateIndexModel<TDocument>)s).ToList();
        #endregion implicit operator

    }
}
