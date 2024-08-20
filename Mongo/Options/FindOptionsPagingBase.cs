namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the base options for a MongoDB find operation with paging.
    /// </summary
    public abstract record FindOptionsPagingBase : FindOptionsBase
    {
        /// <summary>
        /// Gets or sets the number of items to skip.
        /// </summary>
        /// <remarks>
        /// This property is overridden to calculate the number of items to skip based on the current page and page size.
        /// </remarks>
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
        /// Gets or sets the maximum number of items to return.
        /// </summary>
        /// <remarks>
        /// This property is overridden to set the maximum number of items to return to the page size.
        /// </remarks>
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
