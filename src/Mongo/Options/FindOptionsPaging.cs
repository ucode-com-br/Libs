using System;
using System.Text.Json.Serialization;
using MongoDB.Driver;
using UCode.Mongo.Models;

namespace UCode.Mongo.Options
{

    /// <summary>
    /// Represents options for paging through the results of a find operation on a collection of documents of type <typeparamref name="TDocument"/>.
    /// This class derives from <see cref="FindOptionsPaging{TDocument, TKey, TEntity}"/> with a string as the key type.
    /// </summary>
    /// <typeparam name="TDocument">The type of the documents being paginated.</typeparam>
    public class FindOptionsPaging<TDocument> : FindOptionsPaging<TDocument, string, TDocument>
        where TDocument : IObjectBase<string, string>, IObjectBaseTenant
    {

    }

    /// <summary>
    /// Represents a pagination option for finding documents, allowing for generic 
    /// typing of the document and projection types.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document to be retrieved.</typeparam>
    /// <typeparam name="TProjection">The type of the projection to be created from the document.</typeparam>
    /// <remarks>
    /// This class serves as a specialized version of the FindOptionsPaging class,
    /// targeting a string type for ID management or other string-based operations.
    /// </remarks>
    public class FindOptionsPaging<TDocument, TProjection> : FindOptionsPaging<TDocument, string, TProjection>
        where TDocument : IObjectBase<string, string>, IObjectBaseTenant
    {

    }

    /// <summary>
    /// Represents pagination options for a find operation with generic types for document,
    /// object ID, and projection. This class inherits from a base version with a string type 
    /// used for additional configurations.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document being queried.</typeparam>
    /// <typeparam name="TObjectId">The type of the object ID used to identify documents.</typeparam>
    /// <typeparam name="TProjection">The type of the projection used in the find operation.</typeparam>
    public class FindOptionsPaging<TDocument, TObjectId, TProjection> : FindOptionsPaging<TDocument, TObjectId, TProjection, string>
        where TDocument : IObjectBase<TObjectId, string>, IObjectBaseTenant
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

    }

    /// <summary>
    /// Represents options for finding documents with support for pagination and user context.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document being handled.</typeparam>
    /// <typeparam name="TObjectId">The type of the identifier for the documents.</typeparam>
    /// <typeparam name="TProjection">The type of the projected result.</typeparam>
    /// <typeparam name="TUser">The type representing a user, for authorization or context purposes.</typeparam>
    public class FindOptionsPaging<TDocument, TObjectId, TProjection, TUser> : FindOptions<TDocument, TProjection>
        where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        private int _currentPage = 1;
        private int _pageSize = 1;

        /// <summary>
        /// Represents the number of items to be skipped in pagination.
        /// This property calculates the skip value based on the current page and page size.
        /// </summary>
        /// <returns>
        /// The number of items to skip, which is a nullable integer.
        /// </returns>
        /// <remarks>
        /// The property also uses the base class's Skip property for value assignment.
        /// </remarks>
        [JsonInclude()]
        [JsonPropertyName("skip")]
        internal new int? Skip
        {
            get
            {
                base.Skip = this.PageSize * (this.CurrentPage - 1);

                return base.Skip;
            }
            set => base.Skip = value;
        }

        /// <summary>
        /// Represents the limit of items to retrieve in a paginated response.
        /// This property is nullable and can be set or retrieved when 
        /// interacting with pagination. It overrides the base class limit 
        /// property to provide custom functionality pertaining to page size.
        /// </summary>
        /// <returns>
        /// Returns the current limit, which is an integer or null if not set.
        /// </returns>
        [JsonInclude()]
        [JsonPropertyName("limit")]
        internal new int? Limit
        {
            get
            {
                base.Limit = this.PageSize;

                return base.Limit;
            }
            set => base.Limit = value;
        }

        /// <summary>
        /// Gets or sets the current page number for pagination.
        /// The current page must be greater than zero; otherwise, an 
        /// InvalidOperationException will be thrown.
        /// </summary>
        /// <value>
        /// The current page number as an integer.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the value being set is less than or equal to zero.
        /// </exception>
        [JsonPropertyName("currentPage")]
        public int CurrentPage
        {
            get
            {
                return this._currentPage;
            }
            set
            {
                if (value <= 0)
                {
                    throw new InvalidOperationException($"Current page is {value}");
                }

                this._currentPage = value;

                base.Limit = this._pageSize;

                base.Skip = this._pageSize * (this._currentPage - 1);
            }
        }

        /// <summary>
        /// Represents the number of items per page in a paginated result set.
        /// This property gets or sets the page size, updating the base limit and skip values accordingly.
        /// </summary>
        /// <value>
        /// An integer representing the size of each page. Must be greater than zero.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to set the page size to a value less than or equal to zero.
        /// </exception>
        [JsonPropertyName("pageSize")]
        public int PageSize
        {
            get
            {
                return this._pageSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw new InvalidOperationException($"Page size is {value}");
                }

                this._pageSize = value;

                base.Limit = this._pageSize;

                base.Skip = this._pageSize * (this._currentPage - 1);
            }
        }


        /// <summary>
        /// Implicitly converts a <see cref="FindOptionsPaging{TDocument, TObjectId, TProjection, TUser}"/> 
        /// instance to a <see cref="FindOptions"/> instance. This operator allows for seamless 
        /// conversion between these two types without the need for an explicit cast.
        /// </summary>
        /// <param name="source">The source <see cref="FindOptionsPaging{TDocument, TObjectId, TProjection, TUser}"/> 
        /// instance to be converted.</param>
        /// <returns>
        /// Returns an instance of <see cref="FindOptions"/> that corresponds to the provided 
        /// <paramref name="source"/> instance.
        /// </returns>
        public static implicit operator FindOptions(FindOptionsPaging<TDocument, TObjectId, TProjection, TUser> source)
        {
            var json = System.Text.Json.JsonSerializer.Serialize<FindOptionsBase>(source);

            return System.Text.Json.JsonSerializer.Deserialize<FindOptions>(json)!;
        }


        public static implicit operator FindOptions<TDocument>(FindOptionsPaging<TDocument, TObjectId, TProjection, TUser> source)
        {
            var json = System.Text.Json.JsonSerializer.Serialize<FindOptionsBase>(source);

            return System.Text.Json.JsonSerializer.Deserialize<FindOptions<TDocument>>(json)!;
        }

        //public static implicit operator FindOptions<TDocument, TProjection>(FindOptionsPaging<TDocument, TObjectId, TProjection, TUser> source)
        //{
        //    var json = System.Text.Json.JsonSerializer.Serialize<FindOptionsBase>(source);
        //    return System.Text.Json.JsonSerializer.Deserialize<FindOptions<TDocument, TProjection>>(json)!;
        //}

    }
}
