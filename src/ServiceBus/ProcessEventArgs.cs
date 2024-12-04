using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using UCode.Extensions;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Represents the event data for processes in an asynchronous context.
    /// This class is generic, meaning that it can handle different types of event data.
    /// </summary>
    /// <typeparam name="T">The type of data that this event will contain.</typeparam>
    public class ProcessEventArgs<T> : EventArgs, IAsyncDisposable
    {
        //private readonly Receiver _receiver;
        private readonly ProcessMessageEventArgs _processMessageEventArgs;
        private readonly ProcessSessionMessageEventArgs _processSessionMessageEventArgs;
        private readonly bool _session;
        private readonly bool _partitioned;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessEventArgs"/> class.
        /// </summary>
        /// <param name="processMessageEventArgs">The <see cref="ProcessMessageEventArgs"/> containing the message and receiver information.</param>
        /// <param name="session">A boolean indicating whether the message should be processed in a sessioned context.</param>
        /// <param name="partitioned">A boolean indicating whether the message processing should be partitioned.</param>
        public ProcessEventArgs(ProcessMessageEventArgs processMessageEventArgs, bool session = false, bool partitioned = false)
        {
            this._processMessageEventArgs = processMessageEventArgs;
            this._session = session;
            this._partitioned = partitioned;

            //this._receiver = new Receiver(this._processMessageEventArgs.GetPrivateField<ProcessMessageEventArgs, ServiceBusReceiver>("_receiver"), this._session, this._partitioned);

            var receiver = new Receiver(this._processMessageEventArgs.GetPrivateField<ProcessMessageEventArgs, ServiceBusReceiver>("_receiver"), this._session, this._partitioned);

            this.Message = new ReceiveMessage<T>(receiver, this._processMessageEventArgs.Message, this._session, this._partitioned);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessEventArgs"/> class.
        /// </summary>
        /// <param name="processMessageEventArgs">Event arguments containing information about the session message event.</param>
        /// <param name="session">Indicates whether the processing is in a session context. Defaults to false.</param>
        /// <param name="partitioned">Indicates whether the processing is in a partitioned context. Defaults to false.</param>
        /// <returns>
        /// A <see cref="ProcessEventArgs"/> instance that encapsulates session message event processing details.
        /// </returns>
        public ProcessEventArgs(ProcessSessionMessageEventArgs processMessageEventArgs, bool session = false, bool partitioned = false)
        {
            this._processSessionMessageEventArgs = processMessageEventArgs;
            this._session = session;
            this._partitioned = partitioned;

            //this._receiver = new Receiver(this._processSessionMessageEventArgs.GetPrivateField<ProcessSessionMessageEventArgs, ServiceBusReceiver>("_receiver"), this._session, this._partitioned);

            var receiver = new Receiver(this._processSessionMessageEventArgs.GetPrivateField<ProcessSessionMessageEventArgs, ServiceBusReceiver>("_receiver"), this._session, this._partitioned);

            this.Message = new ReceiveMessage<T>(receiver, this._processMessageEventArgs.Message, this._session, this._partitioned);
        }

        /// <summary>
        /// Gets the identifier associated with the current process message event arguments.
        /// </summary>
        /// <value>
        /// A string representing the identifier.
        /// </value>
        public string Identifier => this._processMessageEventArgs.Identifier;

        /// <summary>
        /// Gets the message of type <typeparamref name="T"/> that is received.
        /// </summary>
        /// <remarks>
        /// This property provides access to the received message. The message type is 
        /// generic, allowing for flexibility in handling different types of messages.
        /// </remarks>
        /// <typeparam name="T">The type of the message being received.</typeparam>
        public ReceiveMessage<T> Message
        {
            get;
        }

        /// <summary>
        /// Asynchronously disposes of the current instance's resources.
        /// </summary>
        /// <remarks>
        /// This method calls the DisposeAsync method on the Message property,
        /// which is responsible for cleaning up any resources used by the Message.
        /// It is intended to be called when the instance is no longer needed,
        /// ensuring that any asynchronous cleanup operations are completed.
        /// </remarks>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        public async ValueTask DisposeAsync() => await this.Message.DisposeAsync();
    }
}
