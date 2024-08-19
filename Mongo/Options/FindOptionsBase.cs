using System;

// ReSharper disable CommentTypo

namespace UCode.Mongo.Options
{
    public abstract record FindOptionsBase : IOptions
    {
        public Collation Collation
        {
            get; set;
        }

        public bool AllowPartialResults
        {
            get; set;
        }

        public bool? AllowDiskUse
        {
            get; set;
        }

        public TimeSpan? MaxTime
        {
            get; set;
        }

        public int? BatchSize
        {
            get; set;
        }

        public virtual int? Skip
        {
            get; set;
        }

        public virtual int? Limit
        {
            get; set;
        }

        public string Comment
        {
            get; set;
        }

        public TimeSpan? MaxAwaitTime
        {
            get; set;
        }

        public bool? NoCursorTimeout
        {
            get; set;
        }

        public bool? ReturnKey
        {
            get; set;
        }

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

        public bool NotPerformInTransaction
        {
            get; set;
        }


    }
}
