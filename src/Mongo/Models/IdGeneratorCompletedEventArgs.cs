using System;

namespace UCode.Mongo.Models
{
    public sealed class IdGeneratorCompletedEventArgs<TObjectId, TUser> : EventArgs
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        public object Container
        {
            get;
            set;
        }

        public IObjectBase<TObjectId, TUser> Document
        {
            get;
            set;
        }

        public TObjectId Result
        {
            get;
            set;
        }
    }

}
