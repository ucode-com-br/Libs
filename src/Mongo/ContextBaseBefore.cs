using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using UCode.Mongo.Models;
using UCode.Mongo.Options;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents an abstract base class for a context that is utilized 
    /// before a specific operation or event takes place. This class 
    /// provides a foundation for deriving specific context types 
    /// related to pre-operation scenarios.
    /// </summary>
    /// <remarks>
    /// Derived classes should implement the necessary logic for 
    /// managing the context state and handling related functionality 
    /// specific to their context requirements.
    /// </remarks>
    public abstract class ContextBaseBefore
    {
        /// <summary>
        /// Executes pre-insertion logic for a document, modifying the original document and its replacement options 
        /// based on the specified sender.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document being processed, which must implement IObjectBase.</typeparam>
        /// <typeparam name="TObjectId">The type of the document's identifier, which must be comparable and equatable.</typeparam>
        /// <typeparam name="TProjection">The type used for projecting the document.</typeparam>
        /// <typeparam name="TUser">The type representing the user associated with the document.</typeparam>
        /// <param name="sender">The DbSet from which the document is derived, cannot be null.</param>
        /// <param name="original">A reference to the original document before insertion.</param>
        /// <param name="replaceOptions">A reference to the options that dictate how the document can be replaced.</param>
        internal void BeforeInsertInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref TDocument original, ref ReplaceOptions<TDocument> replaceOptions)
                    where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
                    where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);

            OptionObject<TDocument, TProjection> option = replaceOptions;

            this.BeforeInsert<TDocument, TObjectId, TProjection, TUser>(sender, ref original, ref option);

            replaceOptions = option;
        }

        /// <summary>
        /// Handles operations required before inserting a document into the database.
        /// This method is responsible for performing necessary validations and preparations
        /// on the <paramref name="original"/> document before it is inserted.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document to be inserted.</typeparam>
        /// <typeparam name="TObjectId">The type of the identifier for the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection for this document.</typeparam>
        /// <typeparam name="TUser">The type representing the user associated with the document.</typeparam>
        /// <param name="sender">The sender which triggers the insert operation, represented by a DbSet instance.</param>
        /// <param name="original">The original document that is being inserted, passed by reference to allow modifications.</param>
        /// <param name="insertOneOptions">Options related to the insert operation, passed by reference to allow modifications.</param>
        internal void BeforeInsertInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref TDocument original, ref InsertOneOptions insertOneOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);

            OptionObject<TDocument, TProjection> option = insertOneOptions;

            this.BeforeInsert<TDocument, TObjectId, TProjection, TUser>(sender, ref original, ref option);

            insertOneOptions = option;
        }

        /// <summary>
        /// Prepares for an 
        internal void BeforeUpdateInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref UpdateDefinition<TDocument> updateDefinition, ref UpdateOptions<TDocument> updateOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);

            OptionObject<TDocument, TProjection> option = updateOptions;

            this.BeforeUpdate<TDocument, TObjectId, TProjection, TUser>(sender, ref filterDefinition, ref updateDefinition, ref option);
        }

        /// <summary>
        /// Handles operations that must occur before updating a document.
        /// This method may modify the provided filter and update definitions,
        /// as well as the options for the find and update operation.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document that is to be updated.</typeparam>
        /// <typeparam name="TObjectId">The type of the identifier for the document.</typeparam>
        /// <typeparam name="TProjection">The type of projected result returned by the update operation.</typeparam>
        /// <typeparam name="TUser">The type representing the user or owner of the document.</typeparam>
        /// <param name="sender">The DbSet instance that is responsible for the update operation.</param>
        /// <param name="filterDefinition">The filter definition used to find the document to update.</param>
        /// <param name="updateDefinition">The update definition that specifies the changes to apply to the document.</param>
        /// <param name="findOneAndUpdateOptions">Options for the find and update operations.</param>
        internal void BeforeUpdateInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref UpdateDefinition<TDocument> updateDefinition, ref FindOneAndUpdateOptions<TDocument, TProjection> findOneAndUpdateOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);

            OptionObject<TDocument, TProjection> option = findOneAndUpdateOptions;

            this.BeforeUpdate<TDocument, TObjectId, TProjection, TUser>(sender, ref filterDefinition, ref updateDefinition, ref option);
        }

        /// <summary>
        /// Performs actions needed before a document is replaced in the database.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document being replaced.</typeparam>
        /// <typeparam name="TObjectId">The type of the identifier for the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection associated with the document.</typeparam>
        /// <typeparam name="TUser">The type of the user associated with the document.</typeparam>
        /// <param name="sender">The DbSet instance that is performing the replace operation.</param>
        /// <param name="original">The original document before the replace operation.</param>
        /// <param name="filterDefinition">A reference to a filter definition that specifies which document to replace.</param>
        /// <param name="replaceOptions">A reference to the options for the replace operation.</param>
        internal void BeforeReplaceInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, TDocument original, ref FilterDefinition<TDocument> filterDefinition, ref ReplaceOptions replaceOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);
            OptionObject<TDocument, TProjection> option = replaceOptions;
            this.BeforeReplace<TDocument, TObjectId, TProjection, TUser>(sender, ref original, ref filterDefinition, ref option);
            replaceOptions = option;
        }


        /// <summary>
        /// Prepares the aggregation process by performing any necessary operations
        /// before the aggregation takes place.
        /// </summary>
        /// <typeparam name="TDocument">The type of document being processed.</typeparam>
        /// <typeparam name="TObjectId">The type of the object's identifier.</typeparam>
        /// <typeparam name="TProjection">The type of the projection result.</typeparam>
        /// <typeparam name="TUser">The type of the user associated with the document.</typeparam>
        /// <param name="sender">The DbSet sending the aggregation request.</param>
        /// <param name="original">An array of BsonDocuments representing the original data.</param>
        /// <param name="aggregateOptions">Options that modify the aggregation process.</param>
        internal void BeforeAggregateInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref BsonDocument[] original, ref AggregateOptions aggregateOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);
            OptionObject<TDocument, TProjection> option = aggregateOptions;
            this.BeforeAggregate<TDocument, TObjectId, TProjection, TUser>(sender, ref original, ref option);
            aggregateOptions = option;
        }

        /// <summary>
        /// Executes pre-processing logic before finding documents in the specified DbSet.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document being processed, which must implement IObjectBase.</typeparam>
        /// <typeparam name="TObjectId">The type of the identifier for the document, which must implement IComparable and IEquatable.</typeparam>
        /// <typeparam name="TProjection">The type of the projection to be applied to the results.</typeparam>
        /// <typeparam name="TUser">The type of the user associated with the document.</typeparam>
        /// <param name="sender">The DbSet from which documents are to be retrieved.</param>
        /// <param name="filterDefinition">The filter definition that specifies which documents to find.</param>
        /// <param name="countOptions">Options that determine the counting behavior during the find operation.</param>
        internal void BeforeFindInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref CountOptions countOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);
            OptionObject<TDocument, TProjection> option = countOptions;
            this.BeforeFind<TDocument, TObjectId, TProjection, TUser>(sender, ref filterDefinition, ref option);
            countOptions = option;
        }


        /// <summary>
        /// Executes processing before a find operation is performed on the given DbSet.
        /// </summary>
        /// <typeparam name="TDocument">The type of document being queried.</typeparam>
        /// <typeparam name="TObjectId">The type of object ID, which must be comparable and equatable.</typeparam>
        /// <typeparam name="TProjection">The type of projection for the find operation.</typeparam>
        /// <typeparam name="TUser">The type of user associated with the document.</typeparam>
        /// <param name="sender">The DbSet from which the documents are being queried.</param>
        /// <param name="filterDefinition">The filter definition used to refine the query, passed by reference.</param>
        /// <param name="findOptions">The options for the find operation, passed by reference.</param>
        internal void BeforeFindInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref FindOptions<TDocument, TProjection> findOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);
            OptionObject<TDocument, TProjection> option = findOptions;
            this.BeforeFind<TDocument, TObjectId, TProjection, TUser>(sender, ref filterDefinition, ref option);
            findOptions = option;
        }


        /// <summary>
        /// Prepares the queryable object for 
        internal void BeforeQueryableInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref IQueryable<TDocument> queryable, ref AggregateOptions aggregateOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);

            OptionObject<TDocument, TProjection> option = aggregateOptions;
            this.BeforeQueryable<TDocument, TObjectId, TProjection, TUser>(sender, ref queryable, ref option);
            aggregateOptions = option;
        }

        /// <summary>
        /// This method processes actions before the deletion of a document.
        /// It validates necessary parameters and performs any required pre-delete operations.
        /// </summary>
        /// <typeparam name="TDocument">
        /// The type of the document being deleted, which must implement 
        /// the <see cref="IObjectBase{TObjectId, TUser}"/> and 
        /// <see cref="IObjectBaseTenant"/> interfaces.
        /// </typeparam>
        /// <typeparam name="TObjectId">
        /// The type of the object identifier, which must implement 
        /// <see cref="IComparable{TObjectId}"/> and 
        /// <see cref="IEquatable{TObjectId}"/> interfaces.
        /// </typeparam>
        /// <typeparam name="TProjection">
        /// The type of the projection that can be used 
        /// during the delete operation.
        /// </typeparam>
        /// <typeparam name="TUser">
        /// The type representing the user associated with the document.
        /// </typeparam>
        /// <param name="sender">
        /// The <see cref="DbSet{TDocument, TObjectId, TUser}"/> instance 
        /// from which the delete operation is initiated, cannot be null.
        /// </param>
        /// <param name="filterDefinition">
        /// A reference to the <see cref="FilterDefinition{TDocument}"/> 
        /// that specifies the criteria for the delete operation.
        /// </param>
        /// <param name="deleteOptions">
        /// A reference to the <see cref="DeleteOptions"/> that holds additional 
        /// options for the delete operation, which may be modified by this method.
        /// </param>
        internal void BeforeDeleteInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref FilterDefinition<TDocument> filterDefinition, ref DeleteOptions deleteOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);
            OptionObject<TDocument, TProjection> option = deleteOptions;
            this.BeforeDelete<TDocument, TObjectId, TProjection, TUser>(sender, ref filterDefinition, ref option);
            deleteOptions = option;
        }

        //BulkWriteOptions
        /// <summary>
        /// Prepares the 
        internal void BeforeBulkWriteInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref IEnumerable<WriteModel<TDocument>> writeModels, ref BulkWriteOptions bulkWriteOptions)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);
            OptionObject<TDocument, TProjection> option = bulkWriteOptions;
            this.BeforeBulkWrite<TDocument, TObjectId, TProjection, TUser>(sender, ref writeModels, ref option);
            bulkWriteOptions = option;
        }


        /// <summary>
        /// Handles 
        internal void ResultInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref IEnumerable<TProjection> results)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);

            this.Result<TDocument, TObjectId, TProjection, TUser>(sender, ref results);
        }

        /// <summary>
        /// Processes the result 
        internal void ResultInternal<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref TProjection result)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
            ArgumentNullException.ThrowIfNull(sender);

            this.Result<TDocument, TObjectId, TProjection, TUser>(sender, ref result);
        }




        /// <summary>
        /// This method is called before inserting a document into the database.
        /// It can be overridden in derived classes to implement custom logic.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document being inserted.</typeparam>
        /// <typeparam name="TObjectId">The type of the object identifier.</typeparam>
        /// <typeparam name="TProjection">The type of the projection object.</typeparam>
        /// <typeparam name="TUser">The type of the user associated with the document.</typeparam>
        /// <param name="sender">The DbSet responsible for the document operation.</param>
        /// <param name="original">A reference to the original document object before insertion.</param>
        /// <param name="option">A reference to an optional object containing additional data for the insert operation.</param>
        protected virtual void BeforeInsert<TDocument, TObjectId, TProjection, TUser>(
            DbSet<TDocument, TObjectId, TUser> sender, 
            ref TDocument original, 
            ref OptionObject<TDocument, TProjection> option)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }

        /// <summary>
        /// This method is called before an update operation is performed on a DbSet.
        /// It allows for customization of the filter and update definitions 
        /// as well as any options specific to the projection of the document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document to be updated, which must implement IObjectBase.</typeparam>
        /// <typeparam name="TObjectId">The type of the identifier used for the document, which must be comparable and equatable.</typeparam>
        /// <typeparam name="TProjection">The type used for the projection of the document.</typeparam>
        /// <typeparam name="TUser">The type that represents the user associated with the document.</typeparam>
        /// <param name="sender">The DbSet that is sending the update request.</param>
        /// <param name="filterDefinition">A reference to the filter definition that specifies which documents to update.</param>
        /// <param name="updateDefinition">A reference to the update definition that specifies how to update the documents.</param>
        /// <param name="option">A reference to an optional object that defines further specifics for the update operation.</param>
        protected virtual void BeforeUpdate<TDocument, TObjectId, TProjection, TUser>(
            DbSet<TDocument, TObjectId, TUser> sender, 
            ref FilterDefinition<TDocument> filterDefinition, 
            ref UpdateDefinition<TDocument> updateDefinition, 
            ref OptionObject<TDocument, TProjection> option)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }

        /// <summary>
        /// This method is called before replacing a document in the database.
        /// It provides an opportunity to modify or validate the original document
        /// and the filter definition before the replacement occurs.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document being replaced.</typeparam>
        /// <typeparam name="TObjectId">The type of the identifier used for the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection to apply to the document.</typeparam>
        /// <typeparam name="TUser">The type of the user associated with the document.</typeparam>
        /// <param name="sender">The DbSet that is sending the replace request.</param>
        /// <param name="original">The original document that is to be replaced, passed by reference.</param>
        /// <param name="filterDefinition">The filter definition used to identify the document in the database, passed by reference.</param>
        /// <param name="option">Options related to the replacement operation, passed by reference.</param>
        protected virtual void BeforeReplace<TDocument, TObjectId, TProjection, TUser>(
            DbSet<TDocument, TObjectId, TUser> sender, 
            ref TDocument original, 
            ref FilterDefinition<TDocument> filterDefinition, 
            ref OptionObject<TDocument, TProjection> option)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }

        /// <summary>
        /// Executes operations that should be performed before aggregating documents.
        /// The method allows customization of the aggregation process for specific document types.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document that is being aggregated.</typeparam>
        /// <typeparam name="TObjectId">The type of the identifier used for document identification.</typeparam>
        /// <typeparam name="TProjection">The type of the projection result of the aggregation.</typeparam>
        /// <typeparam name="TUser">The type representing the user associated with the document.</typeparam>
        /// <param name="sender">The DbSet instance from which the documents are aggregated.</param>
        /// <param name="bsonDocuments">An array of BSON documents to be processed during aggregation.</param>
        /// <param name="option">Options for the aggregation process, which can include filters or projections.</param>
        protected virtual void BeforeAggregate<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref BsonDocument[] bsonDocuments, ref OptionObject<TDocument, TProjection> option) 
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId> 
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant 
        { 
        }

        /// <summary>
        /// This method is called before a deletion operation is executed on a document.
        /// It allows for modifications to the filter definition and options before the delete operation occurs.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document being deleted.</typeparam>
        /// <typeparam name="TObjectId">The type of the identifier for the document.</typeparam>
        /// <typeparam name="TProjection">The type used for projection in the deletion context.</typeparam>
        /// <typeparam name="TUser">The type representing the user context or actor performing the deletion.</typeparam>
        /// <param name="sender">The DbSet instance that is attempting to delete the document.</param>
        /// <param name="filterDefinition">A reference to the filter definition applied to the deletion operation, allowing it to be modified.</param>
        /// <param name="option">A reference to an option object that can provide additional context or configuration for the deletion operation.</param>
        protected virtual void BeforeDelete<TDocument, TObjectId, TProjection, TUser>(
            DbSet<TDocument, TObjectId, TUser> sender, 
            ref FilterDefinition<TDocument> filterDefinition, 
            ref OptionObject<TDocument, TProjection> option)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }

        /// <summary>
        /// Executes logic prior to finding documents in the database.
        /// This method allows custom filtering and options to be applied before a retrieval operation.
        /// </summary>
        /// <typeparam name="TDocument">
        /// The type of the document being retrieved.
        /// Must implement <see cref="IObjectBase{TObjectId, TUser}"/> and <see cref="IObjectBaseTenant"/>.
        /// </typeparam>
        /// <typeparam name="TObjectId">
        /// The type of the identifier for the document.
        /// Must implement <see cref="IComparable{TObjectId}"/> and <see cref="IEquatable{TObjectId}"/>.
        /// </typeparam>
        /// <typeparam name="TProjection">
        /// The type used for the projection of the retrieved documents.
        /// </typeparam>
        /// <typeparam name="TUser">
        /// The type representing the user related to the document.
        /// </typeparam>
        /// <param name="sender">
        /// The DbSet from which the documents are retrieved.
        /// </param>
        /// <param name="filterDefinition">
        /// A reference to the FilterDefinition that defines the criteria for finding documents,
        /// allowing modifications to be made to the filtering logic.
        /// </param>
        /// <param name="option">
        /// A reference to an OptionObject containing settings or options related to the retrieval operation.
        /// </param>
        protected virtual void BeforeFind<TDocument, TObjectId, TProjection, TUser>(
            DbSet<TDocument, TObjectId, TUser> sender, 
            ref FilterDefinition<TDocument> filterDefinition, 
            ref OptionObject<TDocument, TProjection> option) 
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId> 
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant 
        {
        }

        /// <summary>
        /// Executes operations before queryable execution on the specified DbSet.
        /// This method is intended to be overridden in derived classes to provide 
        /// additional functionality or modifications to the querying process.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document being queried. Must implement IObjectBase.</typeparam>
        /// <typeparam name="TObjectId">The type of the document ID. Must be comparable and equatable.</typeparam>
        /// <typeparam name="TProjection">The type of the projection for the query.</typeparam>
        /// <typeparam name="TUser">The type of the user associated with the document.</typeparam>
        /// <param name="sender">The DbSet instance that is executing the query.</param>
        /// <param name="queryable">The IQueryable collection to be modified before execution.</param>
        /// <param name="option">An object holding options for the querying process.</param>
        protected virtual void BeforeQueryable<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref IQueryable<TDocument> queryable, ref OptionObject<TDocument, TProjection> option) 
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId> 
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant 
        { 
        }

        /// <summary>
        /// Executes logic before a bulk write operation for a specified document type in the database.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document being written to the database.</typeparam>
        /// <typeparam name="TObjectId">The type of the identifier for the document, which must be comparable and equatable.</typeparam>
        /// <typeparam name="TProjection">The projection type used for the document.</typeparam>
        /// <typeparam name="TUser">The type representing the user associated with the document.</typeparam>
        /// <param name="sender">The DbSet instance that is sending the bulk write request.</param>
        /// <param name="writeModels">A reference to an enumerable collection of write models that define the write operations to be performed.</param>
        /// <param name="option">A reference to an optional object that can hold additional options or parameters related to the bulk write operation.</param>
        protected virtual void BeforeBulkWrite<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref IEnumerable<WriteModel<TDocument>> writeModels, ref OptionObject<TDocument, TProjection> option) 
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId> 
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant 
        { 
        }


        /// <summary>
        /// This method processes the results of a query, projecting the entities of type TDocument 
        /// into a collection of type TProjection. It is designed to be overridden in derived classes.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document being processed, which must implement 
        /// IObjectBase and IObjectBaseTenant interfaces.</typeparam>
        /// <typeparam name="TObjectId">The type of the identifier for the document, which must implement 
        /// IComparable and IEquatable interfaces.</typeparam>
        /// <typeparam name="TProjection">The type of the projection resulting from the processing of TDocument.</typeparam>
        /// <typeparam name="TUser">The type representing the user who owns or interacts with the document.</typeparam>
        /// <param name="sender">The DbSet from which documents are queried.</param>
        /// <param name="items">A reference to an enumerable collection of TProjection 
        /// that will be populated with the results of the projection.</param>
        protected virtual void Result<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref IEnumerable<TProjection> items) 
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId> 
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant 
        {
        }

        /// <summary>
        /// This method processes the result of an operation on a database set.
        /// It is a generic method that works with various types of documents, object IDs, projections, and users.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document being processed, which must implement IObjectBase.</typeparam>
        /// <typeparam name="TObjectId">The type of the object ID, which must implement IComparable and IEquatable.</typeparam>
        /// <typeparam name="TProjection">The type of the projected item being worked with.</typeparam>
        /// <typeparam name="TUser">The type representing a user in the context of this operation.</typeparam>
        /// <param name="sender">The database set from which the document is retrieved or operated on.</param>
        /// <param name="item">A reference to the projection item to be processed.</param>
        protected virtual void Result<TDocument, TObjectId, TProjection, TUser>(DbSet<TDocument, TObjectId, TUser> sender, ref TProjection item)
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
            where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        {
        }
    }
}
