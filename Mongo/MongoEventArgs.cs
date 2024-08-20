using System;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents the event arguments for a MongoDB event.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class MongoEventArgs<TEvent> : EventArgs
    {
        /// <summary>
        /// Gets the event.
        /// </summary>
        public TEvent Event
        {
            get; private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoEventArgs{TEvent}"/> class.
        /// </summary>
        /// <param name="ev">The event.</param>
        public MongoEventArgs(TEvent ev) => this.Event = ev;

        /// <summary>
        /// Implicitly converts a <see cref="MongoEventArgs{TEvent}"/> to a <typeparamref name="TEvent"/>.
        /// </summary>
        /// <param name="source">The source <see cref="MongoEventArgs{TEvent}"/>.</param>
        /// <returns>The event.</returns>
        public static implicit operator TEvent(MongoEventArgs<TEvent> source) => source.Event;
    }
}
