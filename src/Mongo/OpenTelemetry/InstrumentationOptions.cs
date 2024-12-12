using System;
using MongoDB.Driver.Core.Events;

namespace UCode.Mongo.OpenTelemetry
{
    /// <summary>
    /// Represents the configuration options for instrumentation in the application.
    /// This class allows for specifying various settings related to the behavior
    /// and performance monitoring of the application.
    /// </summary>
    public class InstrumentationOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the command text should be captured.
        /// </summary>
        /// <value>
        /// <c>true</c> if the command text should be captured; otherwise, <c>false</c>.
        /// </value>
        public bool CaptureCommandText
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>
        /// A string representing the name of the application.
        /// </value>
        public string ApplicationName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a function that determines whether to start an activity 
        /// based on the given <see cref="CommandStartedEvent"/>.
        /// </summary>
        /// <value>
        /// A <see cref="Func{CommandStartedEvent, bool}"/> delegate that takes a 
        /// <see cref="CommandStartedEvent"/> as a parameter and returns a 
        /// boolean value indicating whether the activity should start.
        /// </value>
        public Func<CommandStartedEvent, bool> ShouldStartActivity
        {
            get; set;
        }
    }
}
