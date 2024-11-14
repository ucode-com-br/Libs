using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using UCode.Mongo.Models;

namespace UCode.Mongo.Options
{

    public class FindOptionsPaging<TDocument> : FindOptionsPaging<TDocument, string, TDocument>
        where TDocument : IObjectBase<string, string>, IObjectBaseTenant
    {

    }

    public class FindOptionsPaging<TDocument, TProjection> : FindOptionsPaging<TDocument, string, TProjection>
        where TDocument : IObjectBase<string, string>, IObjectBaseTenant
    {

    }

    public class FindOptionsPaging<TDocument, TObjectId, TProjection> : FindOptionsPaging<TDocument, TObjectId, TProjection, string>
        where TDocument : IObjectBase<TObjectId, string>, IObjectBaseTenant
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

    }

    public class FindOptionsPaging<TDocument, TObjectId, TProjection, TUser> : FindOptions<TDocument, TProjection>
        where TDocument : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        /// <summary>
        /// Gets or sets the number of items to skip based on the current page and page size.
        /// This property calculates the skip amount by multiplying the page size with the current page number 
        /// minus one. It is an override of an existing base property.
        /// </summary>
        /// <value>
        /// An integer that indicates the number of items to skip, or null if not set.
        /// </value>
        /// <returns>
        /// The calculated number of items to skip as an integer, or null if it is not applicable.
        /// </returns>
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
        /// Gets or sets the limit value, which is derived from the PageSize property.
        /// When accessed, it will update the base class's Limit property to the current PageSize.
        /// </summary>
        /// <value>
        /// The limit value as a nullable integer. Returns the base class's Limit when accessed,
        /// and can be set to change the base class's Limit value.
        /// </value>
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
        /// Gets or sets the current page number.
        /// </summary>
        /// <value>
        /// An integer representing the current page. 
        /// The value can be set to any positive integer to reflect the page number in a paginated context.
        /// </value>
        [JsonPropertyName("currentPage")]
        public int CurrentPage
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the number of items to be displayed on a single page.
        /// This property determines the size of the page when data is paginated,
        /// allowing for control over the amount of data presented to the user at one time.
        /// </summary>
        /// <value>
        /// An integer representing the size of the page. The default value is typically 0.
        /// </value>
        [JsonPropertyName("pageSize")]
        public int PageSize
        {
            get; set;
        }


    }
}
