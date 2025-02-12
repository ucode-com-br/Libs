using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using UCode.Extensions;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Represents a message receiver that processes messages asynchronously.
    /// This class is designed to work with generic types, allowing for flexibility 
    /// in the types of messages it can handle.
    /// </summary>
    /// <typeparam name="T">The type of the message that the receiver will process.</typeparam>
    /// <interface>IAsyncDisposable</interface>
    /// <remarks>
    /// Implementing IAsyncDisposable allows for asynchronous cleanup of resources 
    /// when the operation is complete.
    /// </remarks>
    public class ReceiveMessage<T> : IAsyncDisposable
    {
        private readonly ServiceBusReceivedMessage _serviceBusReceivedMessage;

        // Never dispose this object (its a reference from the receiver inside the ProcessEventArgs)
        private readonly Receiver _receiver;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private T? _body;

        public bool UseSession
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether partitioning is used.
        /// </summary>
        /// <returns>
        /// True if partitioning is used; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This property is read-only and returns the current setting of partition usage.
        /// </remarks>
        public bool UsePartition
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the auto-complete feature is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if auto-complete is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool AutoComplete
        {
            get; set;
        } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the lock should automatically renew.
        /// </summary>
        /// <value>
        /// <c>true</c> if the lock should automatically renew; otherwise, <c>false</c>.
        /// </value>
        public bool AutoRenewLock
        {
            get; set;
        } = true;

        /// <summary>
        /// Gets a value indicating whether the operation is completed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the operation is completed; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property can only be set 
        public bool IsCompleted
        {
            get;
            private set;
        }

        /// <summary>
        /// Represents whether an entity is considered abandoned.
        /// </summary>
        /// <value>
        /// True if the entity is abandoned; otherwise, false.
        /// </value>
        /// <remarks>
        /// This property is read-only from outside the class, meaning that it can only be set 
        public bool IsAbandoned
        {
            get; private set;
        }

        /// <summary>
        /// Asynchronously marks the operation as completed. 
        /// This method does not return a value but instead returns a 
        /// ValueTask representing the ongoing operation. The operation 
        /// completion status can be checked using the TryCompletedAsync method.
        /// </summary>
        /// <returns>A ValueTask that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method is designed to be called when the operation has 
        /// finished, allowing for any necessary cleanup or finalization 
        /// activities to be performed.
        /// </remarks>
        public async ValueTask CompletedAsync(CancellationToken cancellationToken = default) => _ = await this.TryCompletedAsync(cancellationToken);


        /// <summary>
        /// Attempts to mark the operation as completed asynchronously.
        /// </summary>
        /// <returns>
        /// A <see cref="ValueTask{bool}"/> that represents the asynchronous operation, 
        /// with a boolean value indicating whether the operation was completed successfully.
        /// </returns>
        /// <remarks>
        /// This method uses a semaphore to ensure that only one thread can mark the operation as completed at a time.
        /// If the operation is already completed or has been abandoned, the completion will not take place.
        /// </remarks>
        public async ValueTask<bool> TryCompletedAsync(CancellationToken cancellationToken = default)
        {
            var result = false;
            await this._semaphore.WaitAsync(cancellationToken);

            try
            {
                if (!this.IsCompleted && !this.IsAbandoned)
                {
                    await this._receiver.CompletedAsync(this._serviceBusReceivedMessage, cancellationToken);

                    this.IsCompleted = true;

                    result = true;
                }
            }
            finally
            {

                this._semaphore.Release();
            }
            return result;
        }

        /// <summary>
        /// Asynchronously attempts to abandon the current operation.
        /// </summary>
        /// <remarks>
        /// This method executes the abandonment operation without awaiting its completion.
        /// It returns a ValueTask which allows the caller to await the operation if needed.
        /// </remarks>
        /// <returns>
        /// A ValueTask representing the asynchronous operation of abandoning the task.
        /// </returns>
        public async ValueTask AbandonAsync(CancellationToken cancellationToken = default) => _ = await this.TryAbandonAsync(cancellationToken);

        /// <summary>
        /// Attempts to abandon the current message asynchronously.
        /// </summary>
        /// <remarks>
        /// This method will check if the message has not already been completed or abandoned.
        /// If these conditions are met, it will invoke the abandonment process for the message 
        /// and update the state of the object accordingly.
        /// </remarks>
        /// <returns>
        /// A <see cref="ValueTask{Boolean}"/> that represents the asynchronous operation.
        /// The result is true if the abandonment was successful; otherwise, false.
        /// </returns>
        public async ValueTask<bool> TryAbandonAsync(CancellationToken cancellationToken = default)
        {
            var result = false;
            await this._semaphore.WaitAsync(cancellationToken);

            try
            {
                if (!this.IsCompleted && !this.IsAbandoned)
                {
                    await this._receiver.AbandonAsync(this._serviceBusReceivedMessage, cancellationToken);

                    this.IsAbandoned = true;
                    result = true;
                }
            }
            finally
            {

                this._semaphore.Release();
            }
            return result;
        }

        /// <summary>
        /// Asynchronously attempts to renew the lock held by the current instance.
        /// </summary>
        /// <returns>
        /// A <see cref="ValueTask"/> that represents the asynchronous operation,
        /// which can return a ValueTask 
        /// indicating the completion of the lock renewal process.
        /// </returns>
        public async ValueTask RenewLockAsync(CancellationToken cancellationToken = default) => _ = await this.TryRenewLockAsync(cancellationToken);

        /// <summary>
        /// Attempts to renew the lock on a message asynchronously.
        /// This method ensures that the lock is renewed if the message is not completed or abandoned.
        /// </summary>
        /// <returns>
        /// A <see cref="ValueTask{Boolean}"/> representing the asynchronous operation. 
        /// The result indicates whether the lock was successfully renewed.
        /// </returns>
        public async ValueTask<bool> TryRenewLockAsync(CancellationToken cancellationToken = default)
        {
            var result = false;
            await this._semaphore.WaitAsync();

            try
            {
                if (!this.IsCompleted && !this.IsAbandoned)
                {
                    await this._receiver.RenewLockAsync(this._serviceBusReceivedMessage, cancellationToken);
                    result = true;
                }
            }
            finally
            {
                this._semaphore.Release();
            }
            return result;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveMessage"/> class.
        /// </summary>
        /// <param name="receiver">The <see cref="Receiver"/> instance used to receive messages.</param>
        /// <param name="serviceBusMessage">The <see cref="ServiceBusReceivedMessage"/> that was received.</param>
        /// <param name="session">A boolean indicating whether session support is used (default is false).</param>
        /// <param name="partitioned">A boolean indicating whether partitioning support is used (default is false).</param>
        /// <returns></returns>
        public ReceiveMessage(Receiver receiver, ServiceBusReceivedMessage serviceBusMessage, bool session = false, bool partitioned = false)
        {
            this._serviceBusReceivedMessage = serviceBusMessage;

            this.UseSession = session;
            this.UsePartition = partitioned;
            this._receiver = receiver;
        }

        /// <summary>
        /// Verify if message is received
        /// </summary>
        public bool MessageReceived
        {
            get => this._serviceBusReceivedMessage != null;
        }


        /// <summary>
        /// Gets the application properties of the received message from the service bus.
        /// This property returns an <see cref="IReadOnlyDictionary{TKey, TValue}"/> 
        /// containing the application properties associated with the message.
        /// </summary>
        /// <value>
        /// An <see cref="IReadOnlyDictionary{string, object}"/> that contains the application properties. 
        /// The dictionary is read-only and cannot be modified.
        /// </value>
        /// <remarks>
        /// Application properties are custom properties that can be added to the message 
        /// to provide additional context or metadata.
        /// </remarks>
        public IReadOnlyDictionary<string, object> ApplicationProperties => this._serviceBusReceivedMessage.ApplicationProperties;

        /// <summary>
        /// Gets the message ID of the received message from the service bus.
        /// </summary>
        /// <value>
        /// A string representing the unique identifier of the message.
        /// </value>
        public string MessageId => this._serviceBusReceivedMessage.MessageId;

        /// <summary>
        /// Gets the partition key of the received message from the service bus.
        /// </summary>
        /// <value>
        /// A string representing the partition key of the Service Bus message.
        /// </value>
        public string PartitionKey => this._serviceBusReceivedMessage.PartitionKey;

        /// <summary>
        /// Gets the partition key for the transaction associated with the received message.
        /// </summary>
        /// <value>
        /// A string representing the transaction partition key.
        /// </value>
        /// <remarks>
        /// This property retrieves the transaction partition key from the 
        /// underlying service bus message that was received.
        /// </remarks>
        public string TransactionPartitionKey => this._serviceBusReceivedMessage.TransactionPartitionKey;


        /// <summary>
        /// Gets the session ID of the service bus received message.
        /// </summary>
        /// <value>
        /// A string representing the session ID. If the session ID is not set, this value may be null.
        /// </value>
        public string SessionId => this._serviceBusReceivedMessage.SessionId;

        /// <summary>
        /// Gets the ReplyToSessionId property from the associated service bus received message.
        /// This property is used to get the session identifier for the reply address
        /// specified in the message, allowing for session-based message handling.
        /// </summary>
        /// <value>
        /// A string representing the ReplyToSessionId of the received message.
        /// If the message does not have a reply session identifier, this property returns null.
        /// </value>
        public string ReplyToSessionId => this._serviceBusReceivedMessage.ReplyToSessionId;

        /// <summary>
        /// Gets the subject of the received message from the service bus.
        /// </summary>
        /// <value>
        /// A string representing the subject of the service bus received message.
        /// </value>
        public string Subject => this._serviceBusReceivedMessage.Subject;

        /// <summary>
        /// Gets the "To" address of the message received from the service bus.
        /// </summary>
        /// <value>
        /// A string representing the "To" address of the received message.
        /// </value>
        /// <remarks>
        /// This property accesses the "To" field of the 
        /// <see cref="_serviceBusReceivedMessage"/> object, 
        /// which encapsulates the details of the message received.
        /// </remarks>
        public string To => this._serviceBusReceivedMessage.To;

        /// <summary>
        /// Gets the 'ReplyTo' property from the received message using the service bus.
        /// </summary>
        /// <value>
        /// A string representing the address to which replies to the message should be sent.
        /// </value>
        public string ReplyTo => this._serviceBusReceivedMessage.ReplyTo;

        /// <summary>
        /// Gets the correlation identifier of the received message from the service bus.
        /// </summary>
        /// <value>
        /// A string representing the correlation ID of the message.
        /// </value>
        public string CorrelationId => this._serviceBusReceivedMessage.CorrelationId;

        /// <summary>
        /// Gets the Time-to-Live (TTL) property of the service bus received message.
        /// This property represents the duration for which the message will be retained in the service bus.
        /// </summary>
        /// <value>
        /// A nullable <see cref="TimeSpan"/> representing the time-to-live of the message. 
        /// If the message does not have a TTL defined, it will return null.
        /// </value>
        public TimeSpan? TimeToLive => this._serviceBusReceivedMessage.TimeToLive;

        /// <summary>
        /// Gets the lock token of the received message from the Service Bus.
        /// The lock token is a unique identifier for the message, which can be used
        /// for message processing operations, such as completing or abandoning the message.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the lock token associated with the message.
        /// </value>
        public string LockToken => this._serviceBusReceivedMessage.LockToken;

        /// <summary>
        /// Gets the number of times the message has been delivered.
        /// </summary>
        /// <value>
        /// An integer representing the delivery count of the message. The delivery count indicates the number of times 
        /// the message has been received by the consumer. This value increments each time the message is delivered to a receiver
        /// and not completed or abandoned.
        /// </value>
        public int DeliveryCount => this._serviceBusReceivedMessage.DeliveryCount;

        /// <summary>
        /// Gets the DateTimeOffset indicating when the message is locked until.
        /// </summary>
        /// <value>
        /// A <see cref="DateTimeOffset"/> representing the lock expiration time for the service bus message.
        /// </value>
        /// <remarks>
        /// This property retrieves the LockedUntil property from the associated service bus received message.
        /// Ensure that the message has been received and is currently locked before accessing this property.
        /// </remarks>
        public DateTimeOffset LockedUntil => this._serviceBusReceivedMessage.LockedUntil;

        /// <summary>
        /// Gets the sequence number of the received message from the service bus.
        /// </summary>
        /// <value>
        /// The sequence number as a <see cref="long"/> which uniquely identifies the message 
        /// within the service bus queue or topic.
        /// </value>
        public long SequenceNumber => this._serviceBusReceivedMessage.SequenceNumber;

        /// <summary>
        /// Gets the sequence number of the message as it was enqueued in the service bus.
        /// </summary>
        /// <value>
        /// A <see cref="long"/> representing the enqueued sequence number of the message.
        /// </value>
        /// <remarks>
        /// This property retrieves the enqueued sequence number from the associated 
        /// <see cref="ServiceBusReceivedMessage"/>. This number represents the 
        /// order in which the message was enqueued in the service bus queue or subscription.
        /// </remarks>
        public long EnqueuedSequenceNumber => this._serviceBusReceivedMessage.EnqueuedSequenceNumber;

        /// <summary>
        /// Gets the time at which the message was enqueued in the service bus.
        /// </summary>
        /// <value>
        /// A <see cref="DateTimeOffset"/> that represents the enqueue time of the message.
        /// </value>
        /// <remarks>
        /// This property retrieves the `EnqueuedTime` from the underlying 
        /// <see cref="_serviceBusReceivedMessage"/> object, providing the time 
        /// when the message was added to the service bus queue.
        /// </remarks>
        public DateTimeOffset EnqueuedTime => this._serviceBusReceivedMessage.EnqueuedTime;

        /// <summary>
        /// Gets the source of the dead letter message from the service bus received message.
        /// </summary>
        /// <value>
        /// A string representing the dead letter source of the message. 
        /// This indicates where the message was originally sent from before being moved to the dead letter queue.
        /// </value>
        /// <remarks>
        /// Dead letters are messages that cannot be delivered to any receiver due to various reasons.
        /// This property allows you to trace the origin of the message that ended up in the dead letter queue.
        /// </remarks>
        public string DeadLetterSource => this._serviceBusReceivedMessage.DeadLetterSource;

        /// <summary>
        /// Gets the reason why the message was dead-lettered in the service bus.
        /// </summary>
        /// <value>
        /// A string that specifies the reason why the message was dead-lettered.
        /// </value>
        public string DeadLetterReason => this._serviceBusReceivedMessage.DeadLetterReason;

        /// <summary>
        /// Gets the error description associated with a dead-lettered message in the service bus.
        /// </summary>
        /// <value>
        /// A string that contains the error description for the dead-lettered message.
        /// </value>
        /// <remarks>
        /// This property retrieves the <see cref="DeadLetterErrorDescription"/> from the underlying 
        /// <see cref="ServiceBusReceivedMessage"/> instance. If the message has not been dead-lettered, 
        /// this will return a null or empty string.
        /// </remarks>
        public string DeadLetterErrorDescription => this._serviceBusReceivedMessage.DeadLetterErrorDescription;

        /// <summary>
        /// Gets the expiration date and time of the message received from the service bus.
        /// </summary>
        /// <value>
        /// A <see cref="DateTimeOffset"/> representing the date and time when the message expires.
        /// </value>
        public DateTimeOffset ExpiresAt => this._serviceBusReceivedMessage.ExpiresAt;

        /// <summary>
        /// Gets the name of the state of the received message from the service bus.
        /// This property returns the string representation of the current state 
        /// of the message which is an enumeration value.
        /// </summary>
        /// <returns>
        /// A string representing the name of the state, or null if 
        /// the state is not defined in the enumeration.
        /// </returns>
        public string? State => System.Enum.GetName(this._serviceBusReceivedMessage.State);

        /// <summary>
        /// Gets the content type of the message. If the content type is not defined,
        /// it defaults to "application/json".
        /// </summary>
        /// <value>
        /// A string representing the content type of the message. It returns the 
        /// content type from the message if available; otherwise, it returns "application/json".
        /// </value>
        public string ContentType => this._serviceBusReceivedMessage.ContentType ?? "application/json";

        /// <summary>
        /// Gets the scheduled enqueue time for the service bus message.
        /// This property returns the time when the message is scheduled to be enqueued,
        /// or null if the message was not scheduled.
        /// </summary>
        /// <value>
        /// A <see cref="DateTimeOffset"/> representing the scheduled enqueue time,
        /// or <c>null</c> if no scheduled enqueue time exists.
        /// </value>
        public DateTimeOffset? ScheduledEnqueueTime => this._serviceBusReceivedMessage.ScheduledEnqueueTime;

        /// <summary>
        /// Gets the body of the message. If the AutoRenewLock property is set to true,
        /// it attempts to renew the lock on the message before accessing the body.
        /// </summary>
        /// <returns>
        /// The body of the message deserialized into type T.
        /// </returns>
        public T Body
        {
            get
            {
                if (this.AutoRenewLock)
                {
                    var r = this.RenewLockAsync();
                    r.AsTask().Wait();
                }

                this._body ??= this._serviceBusReceivedMessage.Body.ToObjectFromJson<T>();

                return this._body!;
            }
        }









        public static implicit operator ServiceBusReceivedMessage(ReceiveMessage<T> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return source._serviceBusReceivedMessage;
        }

        public static implicit operator Receiver(ReceiveMessage<T> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return source._receiver;
        }

        public static implicit operator T(ReceiveMessage<T> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return source.Body;
        }





        /// <summary>
        /// Returns a string that represents the current object.
        /// This method overrides the default implementation of the 
        /// ToString method to return the JSON representation of 
        /// the object by calling the JsonString method.
        /// </summary>
        /// <returns>
        /// A string that represents the current object in JSON format.
        /// </returns>
        public override string ToString() => this.JsonString();


        /// <summary>
        /// Asynchronously releases the resources used by the current instance.
        /// </summary>
        /// <returns>
        /// A ValueTask indicating the completion of the asynchronous operation.
        /// </returns>
        /// <remarks>
        /// This method checks the state of the AutoComplete property. 
        /// If AutoComplete is true, it calls CompletedAsync to finalize the operation.
        /// Otherwise, it calls AbandonAsync to clean up without completing.
        /// </remarks>
        public async ValueTask DisposeAsync()
        {
            if (this.AutoComplete)
            {
                await this.CompletedAsync();
            }
            else
            {
                await this.AbandonAsync();
            }

            GC.SuppressFinalize(this);

        }
    }
}
