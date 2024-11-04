using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    

    /// <summary>
    /// Represents a set of options for finding documents with pagination capabilities.
    /// This class extends the basic find options by adding properties related to paging,
    /// such as the number of documents to skip and the maximum number of documents to retrieve.
    /// </summary>
    /// <typeparam name="TDocument">
    /// The type of the documents to be retrieved. This allows the class to work with any 
    /// strongly-typed document, ensuring type safety when working with the results.
    /// </typeparam>
    public class FindOptionsPaging<TDocument> : FindOptions<TDocument>
    {
    }

    /// <summary>
    /// Represents the options for performing a find operation with pagination.
    /// This class inherits from <see cref="FindOptions{TDocument, TProjection}"/>.
    /// </summary>
    /// <typeparam name="TDocument">The type of the documents to be queried.</typeparam>
    /// <typeparam name="TProjection">The type of the projected result.</typeparam>
    public class FindOptionsPaging<TDocument, TProjection> : FindOptions<TDocument, TProjection>
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
        public int PageSize
        {
            get; set;
        }
    }
}
