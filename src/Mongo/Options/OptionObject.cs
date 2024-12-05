using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace UCode.Mongo.Options
{


    /// <summary>
    /// Represents a generic option object that takes two type parameters,
    /// TDocument and TProjection. This class can be used to handle options
    /// that involve a document type and its associated projection type.
    /// </summary>
    /// <typeparam name="TDocument">
    /// The type of the document that this option is associated with.
    /// </typeparam>
    /// <typeparam name="TProjection">
    /// The type of the projection that this option will project the document to.
    /// </typeparam>
    public class OptionObject<TDocument, TProjection>
    {
        protected readonly BulkWriteOptions? _bulkWriteOptions;

        protected readonly AggregateOptions? _aggregateOptions;

        protected readonly InsertManyOptions? _insertOption1;
        protected readonly InsertOneOptions? _insertOption2;

        protected readonly CountOptions? _countOptions;

        protected readonly UpdateOptions? _updateOptions1;
        protected readonly FindOneAndUpdateOptions<TDocument>? _updateOptions2;
        protected readonly FindOneAndUpdateOptions<TDocument, TProjection>? _updateOptions3;

        protected readonly DeleteOptions? _deleteOption1;
        protected readonly FindOneAndDeleteOptions<TDocument>? _deleteOption2;
        protected readonly FindOneAndDeleteOptions<TDocument, TProjection>? _deleteOption3;

        protected readonly ReplaceOptions? _replaceOption1;
        protected readonly FindOneAndReplaceOptions<TDocument>? _replaceOption2;
        protected readonly FindOneAndReplaceOptions<TDocument, TProjection>? _replaceOption3;

        protected readonly FindOptions<TDocument, TProjection>? _findOptions1;
        protected readonly FindOptions<TDocument>? _findOptions2;
        protected readonly FindOptions? _findOptions3;

        /// <summary>
        /// Gets a value indicating whether the current instance is considered an aggregate.
        /// </summary>
        /// <value>
        /// <c>true</c> if the instance has aggregate options defined; otherwise, <c>false</c>.
        /// </value>
        public bool IsAggregate => _aggregateOptions != default;
        /// <summary>
        /// Determines whether an insert operation is valid based on the 
        /// state of two insertion options.
        /// </summary>
        /// <value>
        /// Returns true if either <c>_insertOption1</c> or <c>_insertOption2</c> 
        /// is not the default value; otherwise, false.
        /// </value>
        public bool IsInsert => _insertOption1 != default || _insertOption2 != default;
        /// <summary>
        /// Gets a value indicating whether the _countOptions is different from its default value.
        /// </summary>
        /// <value>
        /// <c>true</c> if the _countOptions is not the default value; otherwise, <c>false</c>.
        /// </value>
        public bool IsCount => _countOptions != default;
        /// <summary>
        /// Gets a value indicating whether any of the update options have been set to a value 
        /// other than their default.<br />
        /// This property checks three individual update options and determines if at least one 
        /// of them is not equal to its default value.
        /// </summary>
        /// <value>
        /// <c>true</c> if any of the update options (_updateOptions1, _updateOptions2, 
        /// or _updateOptions3) is set to a value different from its default; otherwise, <c>false</c>.
        /// </value>
        public bool IsUpdate => _updateOptions1 != default || _updateOptions2 != default || _updateOptions3 != default;
        /// <summary>
        /// Gets a value indicating whether any of the delete options are set to a value other than their default.
        /// </summary>
        /// <value>
        /// True if at least one of the delete options (_deleteOption1, _deleteOption2, or _deleteOption3) is not 
        /// equal to its default value; otherwise, false.
        /// </value>
        public bool IsDelete => _deleteOption1 != default || _deleteOption2 != default || _deleteOption3 != default;
        /// <summary>
        /// Gets a value indicating whether any of the replacement options are set to non-default values.
        /// </summary>
        /// <value>
        /// Returns <c>true</c> if at least one of the replacement options (_replaceOption1, _replaceOption2, or _replaceOption3) is not 
        /// equal to its default value; otherwise, returns <c>false</c>.
        /// </value>
        public bool IsReplace => _replaceOption1 != default || _replaceOption2 != default || _replaceOption3 != default;
        /// <summary>
        /// Gets a value indicating whether any of the find options are not set to their default value.
        /// </summary>
        /// <returns>
        /// True if at least one of the find options (_findOptions1, _findOptions2, or _findOptions3) 
        /// is not equal to its default value; otherwise, false.
        /// </returns>
        public bool IsFind => _findOptions1 != default || _findOptions2 != default || _findOptions3 != default;

        

        /// <summary>
        /// Returns a string representation of the current object, prioritizing the string representations
        /// of various option properties in a specific order. If none of the properties have a string
        /// representation, the base class's string representation is returned.
        /// </summary>
        /// <returns>
        /// A string representing the current object, or the base string representation if no options are available.
        /// </returns>
        public override string ToString() =>
            this._bulkWriteOptions?.ToString() ??
            this._aggregateOptions?.ToString() ??
            this._insertOption1?.ToString() ??
            this._insertOption2?.ToString() ??
            this._countOptions?.ToString() ??
            this._updateOptions1?.ToString() ??
            this._updateOptions2?.ToString() ??
            this._updateOptions3?.ToString() ??
            this._deleteOption1?.ToString() ??
            this._deleteOption2?.ToString() ??
            this._deleteOption3?.ToString() ??
            this._replaceOption1?.ToString() ??
            this._replaceOption2?.ToString() ??
            this._replaceOption3?.ToString() ??
            this._findOptions1?.ToString() ??
            this._findOptions2?.ToString() ??
            this._findOptions3?.ToString() ?? base.ToString();

        /// <summary>
        /// Overrides the default Equals method to provide a custom equality comparison 
        /// for the current instance. This method checks equality against various options 
        /// held in the instance, returning true if any of them match the provided object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// Returns true if the specified object is equal to the current instance; 
        /// otherwise, returns false. The comparison includes a series of options 
        /// that are part of the current instance.
        /// </returns>
        public override bool Equals(object? obj) =>
            this._bulkWriteOptions?.Equals(obj) ??
            this._aggregateOptions?.Equals(obj) ??
            this._insertOption1?.Equals(obj) ??
            this._insertOption2?.Equals(obj) ??
            this._countOptions?.Equals(obj) ??
            this._updateOptions1?.Equals(obj) ??
            this._updateOptions2?.Equals(obj) ??
            this._updateOptions3?.Equals(obj) ??
            this._deleteOption1?.Equals(obj) ??
            this._deleteOption2?.Equals(obj) ??
            this._deleteOption3?.Equals(obj) ??
            this._replaceOption1?.Equals(obj) ??
            this._replaceOption2?.Equals(obj) ??
            this._replaceOption3?.Equals(obj) ??
            this._findOptions1?.Equals(obj) ??
            this._findOptions2?.Equals(obj) ??
            this._findOptions3?.Equals(obj) ??
            base.Equals(obj);

        /// <summary>
        /// Returns a hash code for the current instance.
        /// The hash code is computed by calling the GetHashCode method 
        /// on a series of optional properties. The first non-null 
        /// value's hash code will be returned. If all properties are 
        /// null, the base class's GetHashCode method is called.
        /// </summary>
        /// <returns>
        /// An integer that represents the hash code for the current instance.
        /// </returns>
        public override int GetHashCode() =>
            this._bulkWriteOptions?.GetHashCode() ??
            this._aggregateOptions?.GetHashCode() ??
            this._insertOption1?.GetHashCode() ??
            this._insertOption2?.GetHashCode() ??
            this._countOptions?.GetHashCode() ??
            this._updateOptions1?.GetHashCode() ??
            this._updateOptions2?.GetHashCode() ??
            this._updateOptions3?.GetHashCode() ??
            this._deleteOption1?.GetHashCode() ??
            this._deleteOption2?.GetHashCode() ??
            this._deleteOption3?.GetHashCode() ??
            this._replaceOption1?.GetHashCode() ??
            this._replaceOption2?.GetHashCode() ??
            this._replaceOption3?.GetHashCode() ??
            this._findOptions1?.GetHashCode() ??
            this._findOptions2?.GetHashCode() ??
            this._findOptions3?.GetHashCode() ??
            base.GetHashCode();



        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class with the specified bulk write options.
        /// </summary>
        /// <param name="bulkWriteOptions">An instance of <see cref="BulkWriteOptions"/> that specifies the options for bulk write operations.</param>
        protected OptionObject(BulkWriteOptions bulkWriteOptions)
        {
            _bulkWriteOptions = bulkWriteOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class.
        /// </summary>
        /// <param name="countOptions">An instance of <see cref="CountOptions"/> that specifies the counting options to be used.</param>
        /// <returns>
        /// This constructor does not return a value.
        /// </returns>
        protected OptionObject(CountOptions countOptions)
        {
            _countOptions = countOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class.
        /// </summary>
        /// <param name="updateOptions">
        /// An instance of <see cref="UpdateOptions"/> that contains the options for the update.
        /// </param>
        protected OptionObject(UpdateOptions updateOptions)
        {
            _updateOptions1 = updateOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class.
        /// </summary>
        /// <param name="aggregateOptions">The aggregate options to be associated with this instance.</param>
        protected OptionObject(AggregateOptions aggregateOptions) 
        { 
            _aggregateOptions = aggregateOptions; 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class
        /// with the specified find options.
        /// </summary>
        /// <param name="findOptions">
        /// An instance of <see cref="FindOptions{TDocument,TProjection}"/> that contains 
        /// the options for finding documents.
        /// </param>
        protected OptionObject(FindOptions<TDocument, TProjection> findOptions)
        {
            _findOptions1 = findOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class with the specified find options.
        /// </summary>
        /// <param name="findOptions">The options used to find documents of type <typeparamref name="TDocument"/>.</param>
        protected OptionObject(FindOptions<TDocument> findOptions)
        {
            _findOptions2 = findOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class.
        /// </summary>
        /// <param name="findOptions">An instance of <see cref="FindOptions"/> that specifies the options 
        /// for finding objects.</param>
        protected OptionObject(FindOptions findOptions) 
        { 
            _findOptions3 = findOptions; 
        }

        // Insert Options Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class.
        /// </summary>
        /// <param name="insertManyOptions">The options for inserting multiple documents.</param>
        protected OptionObject(InsertManyOptions insertManyOptions) 
        { 
            _insertOption1 = insertManyOptions; 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class with specified insert options.
        /// </summary>
        /// <param name="insertOneOptions">The options to use for the insert operation.</param>
        protected OptionObject(InsertOneOptions insertOneOptions)
        {
            _insertOption2 = insertOneOptions;
        }

        // Delete Options Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class
        /// with the specified delete options.
        /// </summary>
        /// <param name="deleteOptions">The delete options to be assigned to this instance.</param>
        /// <returns>
        /// This constructor does not return a value.
        /// </returns>
        protected OptionObject(DeleteOptions deleteOptions)  
        {  
            _deleteOption1 = deleteOptions;  
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class with the specified 
        /// options for the FindOneAndDelete operation.
        /// </summary>
        /// <param name="deleteOptions">
        /// The options for the FindOneAndDelete operation, represented by 
        /// an instance of <see cref="FindOneAndDeleteOptions{TDocument}"/>.
        /// </param>
        protected OptionObject(FindOneAndDeleteOptions<TDocument> deleteOptions)
        {
            _deleteOption2 = deleteOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class 
        /// with the specified options for the FindOneAndDelete operation.
        /// </summary>
        /// <param name="deleteOptions">The options to apply for the FindOneAndDelete operation.</param>
        protected OptionObject(FindOneAndDeleteOptions<TDocument, TProjection> deleteOptions)
        {
            _deleteOption3 = deleteOptions;
        }

        // Replace Options Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class 
        /// with the specified replace options.
        /// </summary>
        /// <param name="replaceOptions">An instance of <see cref="ReplaceOptions"/> 
        /// that contains the options to be used for replacing.</param>
        protected OptionObject(ReplaceOptions replaceOptions) 
        { 
            _replaceOption1 = replaceOptions; 
        }

        /// <summary>
        /// Constructor for the OptionObject class that initializes the instance with specified replace options.
        /// </summary>
        /// <param name="replaceOptions">An instance of FindOneAndReplaceOptions<TDocument> that contains options for replacing a document.</param>
        protected OptionObject(FindOneAndReplaceOptions<TDocument> replaceOptions)
        {
            _replaceOption2 = replaceOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class with specified replace options.
        /// </summary>
        /// <param name="replaceOptions">
        /// The options to use for the find one and replace operation. 
        /// This parameter is of type <see cref="FindOneAndReplaceOptions{TDocument, TProjection}"/>.
        /// </param>
        protected OptionObject(FindOneAndReplaceOptions<TDocument, TProjection> replaceOptions)
        {
            _replaceOption3 = replaceOptions;
        }

        // Update Options Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class.
        /// </summary>
        /// <param name="updateOptions">
        /// The options to use when finding and updating a document.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="OptionObject"/> configured with the provided update options.
        /// </returns>
        protected OptionObject(FindOneAndUpdateOptions<TDocument> updateOptions)
        {
            _updateOptions2 = updateOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionObject"/> class.
        /// </summary>
        /// <param name="updateOptions">
        /// The options to be used for the find one and update operation.
        /// </param>
        protected OptionObject(FindOneAndUpdateOptions<TDocument, TProjection> updateOptions)
        { 
            _updateOptions3 = updateOptions; 
        }

        public static implicit operator BulkWriteOptions(OptionObject<TDocument, TProjection> source) => source._bulkWriteOptions;
        public static implicit operator OptionObject<TDocument, TProjection>(BulkWriteOptions source) => new OptionObject<TDocument, TProjection>(source);

        // Implicit Conversion Operators
        public static implicit operator UpdateOptions(OptionObject<TDocument, TProjection> source) => source._updateOptions1;
        public static implicit operator OptionObject<TDocument, TProjection>(UpdateOptions source) => new OptionObject<TDocument, TProjection>(source);

        public static implicit operator CountOptions(OptionObject<TDocument, TProjection> source) => source._countOptions;
        public static implicit operator OptionObject<TDocument, TProjection>(CountOptions source) => new OptionObject<TDocument, TProjection>(source);

        public static implicit operator AggregateOptions(OptionObject<TDocument, TProjection> source) => source._aggregateOptions;
        public static implicit operator OptionObject<TDocument, TProjection>(AggregateOptions source) => new OptionObject<TDocument, TProjection>(source);

        public static implicit operator FindOptions<TDocument, TProjection>(OptionObject<TDocument, TProjection> source) => source._findOptions1;
        public static implicit operator OptionObject<TDocument, TProjection>(FindOptions<TDocument, TProjection> source) => new OptionObject<TDocument, TProjection>(source);

        public static implicit operator FindOptions<TDocument>(OptionObject<TDocument, TProjection> source) => source._findOptions2;
        public static implicit operator OptionObject<TDocument, TProjection>(FindOptions<TDocument> source) => new OptionObject<TDocument, TProjection>(source);

        public static implicit operator FindOptions(OptionObject<TDocument, TProjection> source) => source._findOptions3;
        public static implicit operator OptionObject<TDocument, TProjection>(FindOptions source) => new OptionObject<TDocument, TProjection>(source);

        // Insert Options Implicit Conversion Operators
        public static implicit operator InsertManyOptions(OptionObject<TDocument, TProjection> source) => source._insertOption1;
        public static implicit operator OptionObject<TDocument, TProjection>(InsertManyOptions source) => new OptionObject<TDocument, TProjection>(source);

        public static implicit operator InsertOneOptions(OptionObject<TDocument, TProjection> source) => source._insertOption2;
        public static implicit operator OptionObject<TDocument, TProjection>(InsertOneOptions source) => new OptionObject<TDocument, TProjection>(source);

        // Delete Options Implicit Conversion Operators
        public static implicit operator DeleteOptions(OptionObject<TDocument, TProjection> source) => source._deleteOption1;
        public static implicit operator OptionObject<TDocument, TProjection>(DeleteOptions source) => new OptionObject<TDocument, TProjection>(source);

        public static implicit operator FindOneAndDeleteOptions<TDocument>(OptionObject<TDocument, TProjection> source) => source._deleteOption2;
        public static implicit operator OptionObject<TDocument, TProjection>(FindOneAndDeleteOptions<TDocument> source) => new OptionObject<TDocument, TProjection>(source);

        public static implicit operator FindOneAndDeleteOptions<TDocument, TProjection>(OptionObject<TDocument, TProjection> source) => source._deleteOption3;
        public static implicit operator OptionObject<TDocument, TProjection>(FindOneAndDeleteOptions<TDocument, TProjection> source) => new OptionObject<TDocument, TProjection>(source);

        // Replace Options Implicit Conversion Operators
        public static implicit operator ReplaceOptions(OptionObject<TDocument, TProjection> source) => source._replaceOption1;
        public static implicit operator OptionObject<TDocument, TProjection>(ReplaceOptions source) => new OptionObject<TDocument, TProjection>(source);

        public static implicit operator FindOneAndReplaceOptions<TDocument>(OptionObject<TDocument, TProjection> source) => source._replaceOption2;
        public static implicit operator OptionObject<TDocument, TProjection>(FindOneAndReplaceOptions<TDocument> source) => new OptionObject<TDocument, TProjection>(source);

        public static implicit operator FindOneAndReplaceOptions<TDocument, TProjection>(OptionObject<TDocument, TProjection> source) => source._replaceOption3;
        public static implicit operator OptionObject<TDocument, TProjection>(FindOneAndReplaceOptions<TDocument, TProjection> source) => new OptionObject<TDocument, TProjection>(source);

        // Update Options Implicit Conversion Operators
        public static implicit operator FindOneAndUpdateOptions<TDocument>(OptionObject<TDocument, TProjection> source) => source._updateOptions2;
        public static implicit operator OptionObject<TDocument, TProjection>(FindOneAndUpdateOptions<TDocument> source) => new OptionObject<TDocument, TProjection>(source);

        public static implicit operator FindOneAndUpdateOptions<TDocument, TProjection>(OptionObject<TDocument, TProjection> source) => source._updateOptions3;
        public static implicit operator OptionObject<TDocument, TProjection>(FindOneAndUpdateOptions<TDocument, TProjection> source) => new OptionObject<TDocument, TProjection>(source);
    }

}
