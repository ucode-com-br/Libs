namespace UCode.Mongo.Options
{
    public abstract record FindOptionsPagingBase : FindOptionsBase
    {
        internal new int? Skip
        {
            get
            {
                base.Skip = this.PageSize * (this.CurrentPage - 1);

                return base.Skip;
            }
            set => base.Skip = value;
        }

        internal new int? Limit
        {
            get
            {
                base.Limit = this.PageSize;

                return base.Limit;
            }
            set => base.Limit = value;
        }


        public int CurrentPage
        {
            get; set;
        }

        public int PageSize
        {
            get; set;
        }
    }
}
