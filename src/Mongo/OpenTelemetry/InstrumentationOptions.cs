using System;
using MongoDB.Driver.Core.Events;

namespace UCode.Mongo.OpenTelemetry
{
    public class InstrumentationOptions
    {
        public bool CaptureCommandText
        {
            get; set;
        }

        public Func<CommandStartedEvent, bool> ShouldStartActivity
        {
            get; set;
        }
    }
}
