using System;

namespace UCode.Mongo.Models
{
    /// <summary>
    /// Represents the event arguments for completion of an ID generation process.
    /// This class is used to pass information about the generated ID and the user
    /// who triggered the event.
    /// </summary>
    /// <typeparam name="TObjectId">The type representing the object ID.</typeparam>
    /// <typeparam name="TUser">The type representing the user associated with the ID generation.</typeparam>
    public sealed class IdGeneratorCompletedEventArgs<TObjectId, TUser> : EventArgs
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        /// <summary>
        /// Gets or sets the container object.
        /// </summary>
        /// <remarks>
        /// This property serves as a generic container that can hold any object type. 
        /// It allows users to store and retrieve objects dynamically.
        /// </remarks>
        public object Container
        {
            get;
            set;
        }

        /// <summary>
        /// Represents a document associated with the object.
        /// </summary>
        /// <value>
        /// An object implementing the <see cref="IObjectBase{TObjectId, TUser}"/> interface,
        /// which encapsulates the functionality and data associated with the document.
        /// </value>
        /// <typeparam name="TObjectId">The type of the object identifier.</typeparam>
        /// <typeparam name="TUser">The type of the user identifier.</typeparam>
        public IObjectBase<TObjectId, TUser> Document
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the result identifier of type <see cref="TObjectId"/>.
        /// </summary>
        /// <value>
        /// The result identifier.
        /// </value>
        public TObjectId Result
        {
            get;
            set;
        }
    }

}
