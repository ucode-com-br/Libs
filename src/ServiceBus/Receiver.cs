using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Represents a receiver that implements asynchronous disposal 
    /// capabilities through the IAsyncDisposable interface.
    /// </summary>
    /// <remarks>
    /// This class is responsible for handling the reception of messages or data
    /// and includes mechanisms to clean up resources asynchronously when
    /// they are no longer needed.
    /// </remarks>
    public class Receiver : IAsyncDisposable
    {
        private readonly ServiceBusReceiver _serviceBusReceiver;
        private readonly bool _session;
        private readonly bool _partitioned;

        /// <summary>
        /// Initializes a new instance of the <see cref="Receiver"/> class.
        /// </summary>
        /// <param name="serviceBusReceiver">
        /// The Service Bus receiver instance that will be used for receiving messages.
        /// </param>
        /// <param name="session">
        /// A boolean value indicating whether the receiver should be session-aware.
        /// </param>
        /// <param name="partitioned">
        /// A boolean value indicating whether the receiver is for a partitioned queue or topic.
        /// </param>
        internal Receiver(ServiceBusReceiver serviceBusReceiver, bool session, bool partitioned)
        {
            this._serviceBusReceiver = serviceBusReceiver;
            this._session = session;
            this._partitioned = partitioned;
        }


        /// <summary>
        /// Represents the result of a receiver operation, encapsulating a value and any potential exceptions that may have occurred.
        /// </summary>
        /// <typeparam name="T">The type of the value being received.</typeparam>
        public async Task CloseAsync() => await this._serviceBusReceiver.CloseAsync();

        /// <summary>
        /// Gets a value indicating whether the service bus receiver is closed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the service bus receiver is closed; otherwise, <c>false</c>.
        /// </value>
        public bool IsClosed => this._serviceBusReceiver.IsClosed;

        /// <summary>
        /// Asynchronously receives a message from the service bus.
        /// </summary>
        /// <typeparam name="T">The type of the message to be received.</typeparam>
        /// <param name="maxWaitTime">An optional <see cref="TimeSpan"/> that specifies the maximum wait time for receiving the message.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the received <see cref="ReceiveMessage{T}"/> object.
        /// </returns>
        public async Task<ReceiveMessage<T>> ReceiveMessageAsync<T>(TimeSpan? maxWaitTime = null)
        {
            var message = await this._serviceBusReceiver.ReceiveMessageAsync(maxWaitTime);

            return new ReceiveMessage<T>(this, message, this._session, this._partitioned);
        }

        /// <summary>
        /// Asynchronously receives a specified number of messages from the service bus receiver.
        /// </summary>
        /// <typeparam name="T">The type of the message to be received.</typeparam>
        /// <param name="maxMessage">The maximum number of messages to receive.</param>
        /// <param name="timeOut">An optional timeout period for receiving messages.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing an enumerable collection of 
        /// <see cref="ReceiveMessage{T}"/> objects which represent the received messages.
        /// </returns>
        /// <remarks>
        /// This method uses the service bus receiver to fetch messages from the queue or topic. The 
        /// messages are then wrapped in the <see cref="ReceiveMessage{T}"/> class before being returned.
        /// </remarks>
        public async Task<IEnumerable<ReceiveMessage<T>>> ReceiveMessagesAsync<T>(int maxMessage, TimeSpan? timeOut = null) => (await this._serviceBusReceiver.ReceiveMessagesAsync(maxMessage, timeOut)).Select(s => new ReceiveMessage<T>(this, s, this._session, this._partitioned));

        /// <summary>
        /// Asynchronously completes the processing of a specified Service Bus message.
        /// </summary>
        /// <param name="serviceBusReceivedMessage">
        /// The Service Bus message that needs to be marked as completed.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask"/> that represents the asynchronous operation.
        /// </returns>
        /// <remarks>
        /// This method is intended to be used when a message has been successfully processed
        /// and should be removed from the Service Bus queue or subscription. It awaits the
        /// completion of the message processing by calling the 
        /// <see cref="_serviceBusReceiver.CompleteMessageAsync"/> method.
        /// </remarks>
        internal async ValueTask CompletedAsync(ServiceBusReceivedMessage serviceBusReceivedMessage) => await this._serviceBusReceiver.CompleteMessageAsync(serviceBusReceivedMessage);

        /// <summary>
        /// Asynchronously abandons a received message from the Service Bus.
        /// This method allows the message to be retried or processed by another receiver,
        /// which indicates that the current processing of the message should not be considered successful.
        /// </summary>
        /// <param name="serviceBusReceivedMessage">
        /// The Service Bus message that is to be abandoned. This parameter cannot be null.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask"/> that represents the asynchronous operation. The task result is 
        /// the completion of the abandon operation.
        /// </returns>
        /// <remarks>
        /// This method is intended to be used when a message cannot be processed successfully,
        /// allowing it to remain in the queue for future processing attempts.
        /// </remarks>
        internal async ValueTask AbandonAsync(ServiceBusReceivedMessage serviceBusReceivedMessage) => await this._serviceBusReceiver.AbandonMessageAsync(serviceBusReceivedMessage);

        /// <summary>
        /// Asynchronously renews the lock on the specified <see cref="ServiceBusReceivedMessage"/>.
        /// This is particularly useful for maintaining the message lock for messages that require longer processing times.
        /// </summary>
        /// <param name="serviceBusReceivedMessage">
        /// The <see cref="ServiceBusReceivedMessage"/> for which the lock needs to be renewed. 
        /// This message must be received from a Service Bus queue or topic subscription.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask"/> representing the asynchronous operation. This will complete once the message lock is successfully renewed.
        /// </returns>
        /// <remarks>
        /// This method should be awaited to ensure that the lock renewal operation is completed.
        /// It is important to renew the lock periodically while processing the message to prevent it from being 
        /// automatically unlocked by the Service Bus before processing has completed.
        /// </remarks>
        internal async ValueTask RenewLockAsync(ServiceBusReceivedMessage serviceBusReceivedMessage) => await this._serviceBusReceiver.RenewMessageLockAsync(serviceBusReceivedMessage);
        /// <summary>
        /// Asynchronously disposes of the resources used by the service bus receiver.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous dispose operation.</returns>
        /// <remarks>
        /// This method calls the <see cref="_serviceBusReceiver.DisposeAsync"/> method to release resources.
        /// </remarks>
        public async ValueTask DisposeAsync() => await this._serviceBusReceiver.DisposeAsync();

        //public async ValueTask<IEnumerable<ServiceBusReceivedMessage>> ReceiveMessagesAsync(int maxMessage, TimeSpan? timeOut = null) => await this._serviceBusReceiver.ReceiveMessagesAsync(maxMessage, timeOut);
    }
}
