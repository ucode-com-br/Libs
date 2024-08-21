using System;

// ReSharper disable CommentTypo

namespace UCode.Mongo.Options
{
    /// <summary>
    /// Represents the base options for a MongoDB find operation.
    /// </summary>
    public abstract record FindOptionsBase : IOptions
    {
        /// <summary>
        /// Gets or sets the collation for the find operation.
        /// </summary>
        public Collation Collation
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether partial results are allowed for the find operation.
        /// </summary>
        public bool AllowPartialResults
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow disk use for the find operation.
        /// </summary>
        public bool? AllowDiskUse
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the maximum time for the find operation.
        /// </summary>
        public TimeSpan? MaxTime
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the batch size for the find operation.
        /// </summary>
        public int? BatchSize
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the number of documents to skip for the find operation.
        /// </summary>
        public virtual int? Skip
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the maximum number of documents to return for the find operation.
        /// </summary>
        public virtual int? Limit
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a comment for the find operation.
        /// </summary>
        public string Comment
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the maximum await time for the find operation.
        /// </summary>
        public TimeSpan? MaxAwaitTime
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cursor timeout should be disabled for the find operation.
        /// </summary>
        public bool? NoCursorTimeout
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to return the record id for the find operation.
        /// </summary>
        public bool? ReturnKey
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the record id for the find operation.
        /// </summary>
        public bool? ShowRecordId
        {
            get; set;
        }

        /// <summary>
        ///     Gets or sets the type of the cursor.
        ///     0 = NonTailable : A non-tailable cursor - This is sufficient for a vast majority of uses.
        ///     1 = Tailable : A tailable cursor.
        ///     2 = TailableAwait : A tailable cursor with a built-in server sleep.
        /// </summary>
        public int CursorType
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the operation should not be performed within a transaction.
        /// </summary>
        public bool NotPerformInTransaction
        {
            get; set;
        }
    }
}
