namespace UCode.Repositories
{
    /// <summary>
    /// Represents the event arguments for an item-based event, encapsulating an item of a specified type.
    /// </summary>
    /// <typeparam name="T">The type of the item associated with the event.</typeparam>
    public class ItemEventArgs<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemEventArgs"/> class.
        /// </summary>
        /// <param name="item">The item associated with the event.</param>
        /// <param name="position">The position of the item.</param>
        /// <returns>
        /// An instance of the <see cref="ItemEventArgs"/> class.</returns>
        public ItemEventArgs(T item, int position)
        {
            this.Item = item;
            this.Position = position;
        }

        /// <summary>
        /// Represents an item of type T.
        /// The item can be accessed through its getter property,
        /// but cannot be set from outside of the class, ensuring
        /// it is read-only.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <value>The value of the item of type T.</value>
        public T Item
        {
            get;
        }
        /// <summary>
        /// Gets the position of the object as an integer value.
        /// </summary>
        /// <value>
        /// An integer representing the current position.
        /// </value>
        public int Position
        {
            get;
        }
    }
}
