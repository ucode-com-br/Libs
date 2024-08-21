using System;

namespace UCode.Mongo
{
    public class MongoEventArgs<TEvent> : EventArgs
    {
        public TEvent Event
        {
            get; private set;
        }

        public MongoEventArgs(TEvent ev) => this.Event = ev;

        public static implicit operator TEvent(MongoEventArgs<TEvent> source) => source.Event;
    }
}
