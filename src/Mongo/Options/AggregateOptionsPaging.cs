using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the options for paginating aggregate results.
    /// Inherits from <see cref="AggregateOptions"/> and includes properties
    /// to manage pagination such as current page, page size, items to skip,
    /// and limit for the number of items to return.
    /// </summary>
    public class AggregateOptionsPaging : AggregateOptions
    {

        /// <summary>
        /// Represents an optional field that can be used to specify the number of items to skip in a collection or sequence,
        /// typically for pagination or filtering purposes.
        /// </summary>
        /// <remarks>
        /// This field is nullable, allowing its value to be set to null when skipping is not applicable.
        /// </remarks>
        private int? _skip;

        /// <summary>
        /// A nullable integer field that represents a limit value.
        /// This field can be used to define constraints in various operations,
        /// allowing for an optional specification of the limit.
        /// </summary>
        private int? _limit;

        /// <summary>
        /// Gets or sets the number of items to skip.
        /// </summary>
        /// <remarks>
        /// This property is overridden to calculate the number of items to skip based on the current page and page size.
        /// </remarks>
        public int? Skip
        {
            get
            {
                this._skip = this.PageSize * (this.CurrentPage - 1);

                return this._skip;
            }
            set => this._skip = value;
        }

        /// <summary>
        /// Gets or sets the maximum number of items to return.
        /// </summary>
        /// <remarks>
        /// This property is overridden to set the maximum number of items to return to the page size.
        /// </remarks>
        public int? Limit
        {
            get
            {
                this._limit = this.PageSize;

                return this._limit;
            }
            set => this._limit = value;
        }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int CurrentPage
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the number of items to display per page.
        /// </summary>
        public int PageSize
        {
            get; set;
        }
    }
}
