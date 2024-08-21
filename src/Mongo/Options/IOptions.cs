namespace UCode.Mongo.Options
{
    public interface IOptions
    {
        /// <summary>
        /// Represents whether the operation should be performed outside of a transaction.
        /// </summary>
        public bool NotPerformInTransaction
        {
            get; set;
        }
    }
}
