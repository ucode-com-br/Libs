using System;
using Azure.Messaging.ServiceBus;

namespace UCode.ServiceBus
{
    public class ProcessExceptionEventArgs
    {
        private readonly ProcessErrorEventArgs _processErrorEventArgs;

        /// <summary>Gets the exception that triggered the call to the error event handler.</summary>
        public Exception Exception => this._processErrorEventArgs.Exception;


        public ProcessExceptionEventArgs(ProcessErrorEventArgs processErrorEventArgs) => this._processErrorEventArgs = processErrorEventArgs;


        /// <summary>Gets the source associated with the error.</summary>
        ///
        /// <value>The source associated with the error.</value>
        public string ErrorSource => Enum.GetName(this._processErrorEventArgs.ErrorSource);

        /// <summary>
        /// Gets the namespace name associated with the error event.
        /// This is likely to be similar to <c>{yournamespace}.servicebus.windows.net</c>.
        /// </summary>
        public string FullyQualifiedNamespace => this._processErrorEventArgs.FullyQualifiedNamespace;

        /// <summary>
        /// Gets the identifier of the processor that raised this event.
        ///
        /// </summary>
        public string Identifier => this._processErrorEventArgs.Identifier;

        /// <summary>
        /// Gets the entity path associated with the error event.
        /// </summary>
        public string EntityPath => this._processErrorEventArgs.EntityPath;
    }
}
