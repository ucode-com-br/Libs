using System;
using Azure.Messaging.ServiceBus;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Represents the event data for process exception events.
    /// </summary>
    public class ProcessExceptionEventArgs
    {
        private readonly ProcessErrorEventArgs _processErrorEventArgs;



        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessExceptionEventArgs"/> class
        /// with the specified <see cref="ProcessErrorEventArgs"/> instance.
        /// </summary>
        /// <param name="processErrorEventArgs">
        /// The <see cref="ProcessErrorEventArgs"/> instance containing error details.
        /// </param>
        public ProcessExceptionEventArgs(ProcessErrorEventArgs processErrorEventArgs) => this._processErrorEventArgs = processErrorEventArgs;


        /// <summary>
        /// Gets the name of the error source associated with the current instance of the process error event arguments.
        /// </summary>
        /// <value>
        /// A string representing the name of the error source. This value is retrieved from the 
        /// <see cref="Enum.GetName(Enum)"/> method based on the <c>ErrorSource</c> property of the 
        /// <see cref="_processErrorEventArgs"/> object.
        /// </value>
        public string ErrorSource => Enum.GetName(this._processErrorEventArgs.ErrorSource);

        /// <summary>
        /// Gets the fully qualified namespace from the process error event arguments.
        /// </summary>
        /// <value>
        /// A string representing the fully qualified namespace.
        /// </value>
        public string FullyQualifiedNamespace => this._processErrorEventArgs.FullyQualifiedNamespace;

        /// <summary>
        /// Gets the identifier associated with the current process error event arguments.
        /// </summary>
        /// <value>
        /// A string that represents the identifier. This value is retrieved from the 
        /// <see cref="_processErrorEventArgs"/> instance.
        /// </value>
        /// <remarks>
        /// This property provides a read-only access to the identifier, which can be used
        /// to identify the specific error event being processed.
        /// </remarks>
        public string Identifier => this._processErrorEventArgs.Identifier;

        /// <summary>
        /// Gets the path of the entity associated with the current instance.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that represents the entity path.
        /// </value>
        /// <remarks>
        /// This property retrieves the entity path from the 
        /// <see cref="_processErrorEventArgs"/> instance.
        /// </remarks>
        public string EntityPath => this._processErrorEventArgs.EntityPath;
    }
}
