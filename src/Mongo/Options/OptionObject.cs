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
    //public class OptionObject : OptionObject<BsonDocument>
    //{
    //    protected OptionObject(UpdateOptions updateOptions) : base(updateOptions) { }

    //    protected OptionObject(CountOptions countOptions) : base(countOptions)
    //    {
    //    }

    //    protected OptionObject(AggregateOptions aggregateOptions) : base(aggregateOptions)
    //    {
    //    }


    //    protected OptionObject(FindOptions findOptions) : base(findOptions)
    //    {
    //    }

    //    public static implicit operator CountOptions(OptionObject source) => source._countOptions;
    //    public static implicit operator OptionObject(CountOptions source) => new OptionObject(source);

    //    public static implicit operator AggregateOptions(OptionObject source) => source._aggregateOptions;
    //    public static implicit operator OptionObject(AggregateOptions source) => new OptionObject(source);

    //    public static implicit operator FindOptions(OptionObject source) => source._findOptions3;
    //    public static implicit operator OptionObject(FindOptions source) => new OptionObject(source);

    //}


    //public class OptionObject<TDocument> : OptionObject<TDocument, TDocument>
    //{
    //    protected OptionObject(UpdateOptions updateOptions) : base(updateOptions) { }

    //    protected OptionObject(CountOptions countOptions) : base(countOptions)
    //    {
    //    }

    //    protected OptionObject(AggregateOptions aggregateOptions) : base(aggregateOptions)
    //    {
    //    }


    //    protected OptionObject(FindOptions<TDocument> findOptions): base(findOptions)
    //    {
    //    }

    //    protected OptionObject(FindOptions findOptions) : base(findOptions)
    //    {
    //    }

    //    public static implicit operator CountOptions(OptionObject<TDocument> source) => source._countOptions;
    //    public static implicit operator OptionObject<TDocument>(CountOptions source) => new OptionObject<TDocument>(source);

    //    public static implicit operator AggregateOptions(OptionObject<TDocument> source) => source._aggregateOptions;
    //    public static implicit operator OptionObject<TDocument>(AggregateOptions source) => new OptionObject<TDocument>(source);

    //    public static implicit operator FindOptions<TDocument>(OptionObject<TDocument> source) => source._findOptions2;
    //    public static implicit operator OptionObject<TDocument>(FindOptions<TDocument> source) => new OptionObject<TDocument>(source);

    //    public static implicit operator FindOptions(OptionObject<TDocument> source) => source._findOptions3;
    //    public static implicit operator OptionObject<TDocument>(FindOptions source) => new OptionObject<TDocument>(source);

    //}


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

        public bool IsAggregate => _aggregateOptions != default;
        public bool IsInsert => _insertOption1 != default || _insertOption2 != default;
        public bool IsCount => _countOptions != default;
        public bool IsUpdate => _updateOptions1 != default || _updateOptions2 != default || _updateOptions3 != default;
        public bool IsDelete => _deleteOption1 != default || _deleteOption2 != default || _deleteOption3 != default;
        public bool IsReplace => _replaceOption1 != default || _replaceOption2 != default || _replaceOption3 != default;
        public bool IsFind => _findOptions1 != default || _findOptions2 != default || _findOptions3 != default;

        

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



        protected OptionObject(BulkWriteOptions bulkWriteOptions)
        {
            _bulkWriteOptions = bulkWriteOptions;
        }

        protected OptionObject(CountOptions countOptions)
        {
            _countOptions = countOptions;
        }

        protected OptionObject(UpdateOptions updateOptions)
        {
            _updateOptions1 = updateOptions;
        }

        protected OptionObject(AggregateOptions aggregateOptions)
        {
            _aggregateOptions = aggregateOptions;
        }

        protected OptionObject(FindOptions<TDocument, TProjection> findOptions)
        {
            _findOptions1 = findOptions;
        }

        protected OptionObject(FindOptions<TDocument> findOptions)
        {
            _findOptions2 = findOptions;
        }

        protected OptionObject(FindOptions findOptions)
        {
            _findOptions3 = findOptions;
        }

        // Insert Options Constructors
        protected OptionObject(InsertManyOptions insertManyOptions)
        {
            _insertOption1 = insertManyOptions;
        }

        protected OptionObject(InsertOneOptions insertOneOptions)
        {
            _insertOption2 = insertOneOptions;
        }

        // Delete Options Constructors
        protected OptionObject(DeleteOptions deleteOptions)
        {
            _deleteOption1 = deleteOptions;
        }

        protected OptionObject(FindOneAndDeleteOptions<TDocument> deleteOptions)
        {
            _deleteOption2 = deleteOptions;
        }

        protected OptionObject(FindOneAndDeleteOptions<TDocument, TProjection> deleteOptions)
        {
            _deleteOption3 = deleteOptions;
        }

        // Replace Options Constructors
        protected OptionObject(ReplaceOptions replaceOptions)
        {
            _replaceOption1 = replaceOptions;
        }

        protected OptionObject(FindOneAndReplaceOptions<TDocument> replaceOptions)
        {
            _replaceOption2 = replaceOptions;
        }

        protected OptionObject(FindOneAndReplaceOptions<TDocument, TProjection> replaceOptions)
        {
            _replaceOption3 = replaceOptions;
        }

        // Update Options Constructors
        protected OptionObject(FindOneAndUpdateOptions<TDocument> updateOptions)
        {
            _updateOptions2 = updateOptions;
        }

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
