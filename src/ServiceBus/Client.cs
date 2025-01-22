using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using UCode.Extensions;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Represents a client that implements IDisposable to free up resources.
    /// </summary>
    /// <remarks>
    /// This class is designed to manage resource cleanup when the instance is no longer needed.
    /// It implements the Dispose pattern to allow for proper resource management.
    /// </remarks>
    public class Client : IDisposable
    {
        private readonly ServiceBusAdministrationClient _serviceBusAdministrationClient;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly bool _session;
        private readonly bool _partitioned;
        private readonly bool _autoCreateQueue;
        private readonly Action<CreateQueueOptions> _defaultCreateQueueOptions;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// This constructor sets up the necessary configurations for connecting to the service bus
        /// and initializes clients for service bus administration and messaging.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string used to connect to the Azure Service Bus.
        /// </param>
        /// <param name="session">
        /// Indicates whether session-based messaging is enabled.
        /// </param>
        /// <param name="partitioned">
        /// Indicates whether the queue is partitioned.
        /// </param>
        /// <param name="autoCreateQueue">
        /// Specifies whether the queue should be automatically created if it does not exist.
        /// </param>
        /// <param name="defaultCreateQueueOptions">
        /// An optional action to configure the default options for creating queues.
        /// If not provided, an empty delegate will be used.
        /// </param>
        public Client(string connectionString, bool session, bool partitioned, bool autoCreateQueue, Action<CreateQueueOptions> defaultCreateQueueOptions = null)
        {
            var serviceBusClientOptions = new ServiceBusClientOptions()
            {
                RetryOptions = new ServiceBusRetryOptions()
                {
                    Mode = ServiceBusRetryMode.Fixed,
                    Delay = new TimeSpan(0, 0, 0, 10),
                    MaxRetries = 5
                },
                TransportType = ServiceBusTransportType.AmqpTcp
            };


            this._serviceBusAdministrationClient = new ServiceBusAdministrationClient(connectionString);

            this._serviceBusClient = new ServiceBusClient(connectionString, serviceBusClientOptions);

            this._session = session;
            this._partitioned = partitioned;
            this._autoCreateQueue = autoCreateQueue;
            this._defaultCreateQueueOptions = defaultCreateQueueOptions ?? ((q) => { });

            this.SetQueuesAsync().Wait();
        }

        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, QueueProperties> _dictQueueProperties = new();
        /// <summary>
        /// Gets the collection of keys from the dictionary of queue properties as a read-only list.
        /// </summary>
        /// <value>
        /// A read-only list of strings representing the keys of the queue properties stored in the dictionary.
        /// </value>
        /// <remarks>
        /// This property retrieves the keys from the <c>_dictQueueProperties</c> dictionary,
        /// which holds various properties related to queues. The keys in this case are of type 
        /// <c>string</c> and are converted to a list to ensure that the consumers of this API 
        /// receive a snapshot of the keys at the time of access, without allowing further modifications.
        /// </remarks>
        public IReadOnlyList<string> Queues => this._dictQueueProperties.Keys.ToList();

        /// <summary>
        /// Asynchronously checks if a queue exists with the specified name. If the queue does not exist and 
        /// automatic queue creation is enabled, it will create the queue.
        /// </summary>
        /// <param name="queueName">
        /// The name of the queue to check for existence.
        /// </param>
        /// <returns>
        /// A Task representing the asynchronous operation, containing true if the queue exists 
        /// or has been created, otherwise false.
        /// </returns>
        public async Task<bool> ExistQueueAsync(string queueName)
        {
            if (this._dictQueueProperties.ContainsKey(queueName))
            {
                return true;
            }

            await this.SetQueuesAsync();

            if (!this._dictQueueProperties.ContainsKey(queueName) && this._autoCreateQueue)
            {
                var defaultCreateQueueOptions = new CreateQueueOptions(queueName);

                this._defaultCreateQueueOptions(defaultCreateQueueOptions);

                var created = await this._serviceBusAdministrationClient.CreateQueueAsync(defaultCreateQueueOptions);

                this._dictQueueProperties.AddOrUpdate(created.Value.Name, created.Value, (key, value) => created.Value);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously retrieves queues from the service bus and updates the properties in the dictionary.
        /// </summary>
        /// <remarks>
        /// This method uses the Service Bus Administration Client to fetch all queues in pages.
        /// For each queue, it adds or updates the queue properties in a thread-safe dictionary.
        /// </remarks>
        /// <returns>
        /// A Task representing the asynchronous operation.
        /// </returns>
        private async Task SetQueuesAsync()
        {
            var pages = this._serviceBusAdministrationClient.GetQueuesAsync().AsPages();

            await foreach (var page in pages)
            {
                foreach (var item in page.Values)
                {
                    this._dictQueueProperties.AddOrUpdate(item.Name, item, (key, value) => item);
                }
            }
        }

        /// <summary>
        /// Asynchronously creates a sender for the specified queue if it exists.
        /// </summary>
        /// <param name="queueName">The name of the queue to create a sender for.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a <see cref="Sender"/> instance if the queue exists; otherwise, it returns null.
        /// </returns>
        public async Task<Sender> CreateSenderAsync(string queueName)
        {
            if (await this.ExistQueueAsync(queueName))
            {
                var serviceBusSender = this._serviceBusClient.CreateSender(queueName);

                return new(serviceBusSender, this._session, this._partitioned);
            }

            return null;
        }

        /// <summary>
        /// Asynchronously creates a Receiver for the specified queue if it exists.
        /// </summary>
        /// <param name="queueName">The name of the queue for which the Receiver should be created.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation. The task result contains
        /// an instance of the Receiver if the queue exists; otherwise, it returns null.
        /// </returns>
        public async Task<Receiver> CreateReceiverAsync(string queueName)
        {
            if (await this.ExistQueueAsync(queueName))
            {
                var receiver = this._serviceBusClient.CreateReceiver(queueName);

                return new Receiver(receiver, this._session, this._partitioned);
            }

            return null;
        }

        /// <summary>
        /// Asynchronously creates a message processor for the specified queue.
        /// This method checks for the existence of the queue and depending 
        /// on whether sessions are enabled, creates either a session processor or a regular processor.
        /// </summary>
        /// <typeparam name="T">
        /// The type of messages that the processor will handle.
        /// </typeparam>
        /// <param name="queueName">
        /// The name of the queue for which to create the processor.
        /// </param>
        /// <param name="identifierOfProcessor">
        /// An optional unique identifier for the processor. If not provided, a new GUID will be generated.
        /// </param>
        /// <param name="receiveAndDelete">
        /// A boolean value indicating whether messages should be received and deleted 
        /// or received in peek lock mode. The default is false, which means peek lock mode.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains 
        /// a <see cref="ProcessMessage{T}"/> that processes messages from the queue, or null 
        /// if the queue does not exist.
        /// </returns>
        public async Task<ProcessMessage<T>?> CreateProcessorAsync<T>(string queueName, string? identifierOfProcessor = null, bool receiveAndDelete = false)
        {
            if (await this.ExistQueueAsync(queueName))
            {
                var autoComplete = false;
                var receiveMode = receiveAndDelete ? ServiceBusReceiveMode.ReceiveAndDelete : ServiceBusReceiveMode.PeekLock;
                var identifier = identifierOfProcessor ?? Guid.NewGuid().ToString();

                if (this._session)
                {
                    var sessionOption = new ServiceBusSessionProcessorOptions()
                    {
                        AutoCompleteMessages = autoComplete,
                        Identifier = identifier,
                        ReceiveMode = receiveMode

                    };

                    var processor = this._serviceBusClient.CreateSessionProcessor(queueName, options: sessionOption);

                    return new ProcessMessage<T>(processor, this._session, this._partitioned);
                }
                else
                {
                    var option = new ServiceBusProcessorOptions()
                    {
                        AutoCompleteMessages = false,
                        Identifier = identifier,
                        ReceiveMode = receiveMode
                    };

                    var processor = this._serviceBusClient.CreateProcessor(queueName, option);

                    return new ProcessMessage<T>(processor, this._session, this._partitioned);
                }
            }

            return null;
        }

        /// <summary>
        /// Asynchronously resubmits messages from the dead-letter queue to the specified queue.
        /// This method allows for optional validation of messages before resubmission.
        /// </summary>
        /// <typeparam name="T">
        /// The type to which the message body will be deserialized. This could be a specific type
        /// or a generic object type.
        /// </typeparam>
        /// <param name="queueName">
        /// The name of the queue to which the messages should be resubmitted.
        /// </param>
        /// <param name="validate">
        /// An optional validation function that accepts a <see cref="ServiceBusReceivedMessage"/> 
        /// and the deserialized message object of type <typeparamref name="T"/>. 
        /// If the function returns <c>true</c>, the message will be resubmitted; 
        /// otherwise, it will be ignored.
        /// </param>
        /// <param name="sessionId">
        /// An optional session identifier for session-enabled queues. This parameter is required 
        /// if the <c>this._session</c> is true.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="sessionId"/> is null or empty when <c>this._session</c> is true.
        /// </exception>
        /// <returns>
        /// A task representing the asynchronous operation, with no result.
        /// </returns>
        public async Task ResubmitDeadLetterAsync<T>(string queueName, Func<ServiceBusReceivedMessage, T, bool> validate = null, string? sessionId = null)
        {
            if (this._session && string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException(nameof(sessionId));
            }

            var serviceBusSender = this._serviceBusClient.CreateSender(queueName);

            ServiceBusReceiver receiver;

            if (this._session)
            {
                receiver = await this._serviceBusClient.AcceptSessionAsync(queueName, sessionId);
            }
            else
            {
                receiver = this._serviceBusClient.CreateReceiver(queueName, new ServiceBusReceiverOptions
                {
                    ReceiveMode = ServiceBusReceiveMode.PeekLock,
                });
            }

            var dlqMessages = receiver.ReceiveMessagesAsync();

            await foreach (var msg in dlqMessages)
            {
                var obj = default(T);
                try
                {
                    if (typeof(T) == typeof(string))
                    {
                        obj = (T)(object)msg.Body.ToString();
                    }
                    else
                    {
                        if (typeof(T) == typeof(object))
                        {
                            obj = (T)(object)msg.Body.ToString().JsonObject<System.Text.Json.JsonElement>();
                        }
                        else
                        {
                            obj = msg.Body.ToString().JsonObject<T>();
                        }
                    }
                }
                catch { }


                if (validate == null || (validate?.Invoke(msg, obj) ?? true))
                {
                    var resubmittableMessage = new ServiceBusMessage(msg);

                    await serviceBusSender.SendMessageAsync(resubmittableMessage);

                    await receiver.CompleteMessageAsync(msg);
                }
            }
        }
        /// <summary>
        /// Asynchronously resubmits a message from the dead-letter queue to the specified queue.
        /// </summary>
        /// <param name="queueName">The name of the queue to which the message should be resubmitted.</param>
        /// <param name="sessionId">An optional parameter that specifies the session identifier for the message.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ResubmitDeadLetterAsync(string queueName, string? sessionId = null) => await this.ResubmitDeadLetterAsync<string>(queueName, null, sessionId);

        /// <summary>
        /// Asynchronously removes all messages from the specified service bus queue that satisfy the provided remove condition.
        /// This method processes messages either from a session-enabled queue or a regular queue, depending on the provided sessionId.
        /// </summary>
        /// <typeparam name="T">The type of the object to process from the message body.</typeparam>
        /// <param name="queueName">The name of the queue from which to remove messages.</param>
        /// <param name="remove">A function that takes a message and an object of type T, and returns a boolean indicating whether the message should be removed.</param>
        /// <param name="sessionId">Optional session identifier for session-enabled queues. Required if the queue is session-enabled.</param>
        /// <exception cref="ArgumentNullException">Thrown when sessionId is required but not provided for session-enabled queues.</exception>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task QueueRemoveAllAsync<T>(string queueName, Func<ServiceBusReceivedMessage, T, bool> remove, string? sessionId = null)
        {
            if (this._session && string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException(nameof(sessionId));
            }

            ServiceBusReceiver receiver;

            if (this._session)
            {
                receiver = await this._serviceBusClient.AcceptSessionAsync(queueName, sessionId);
            }
            else
            {
                receiver = this._serviceBusClient.CreateReceiver(queueName, new ServiceBusReceiverOptions
                {
                    ReceiveMode = ServiceBusReceiveMode.PeekLock,
                });
            }


            var dlqMessages = receiver.ReceiveMessagesAsync();

            await foreach (var msg in dlqMessages)
            {
                var obj = default(T);
                try
                {
                    if (typeof(T) == typeof(string))
                    {
                        obj = (T)(object)msg.Body.ToString();
                    }
                    else
                    {
                        if (typeof(T) == typeof(object))
                        {
                            obj = (T)(object)msg.Body.ToString().JsonObject<System.Text.Json.JsonElement>();
                        }
                        else
                        {
                            obj = msg.Body.ToString().JsonObject<T>();
                        }
                    }
                }
                catch { }


                if (remove.Invoke(msg, obj))
                {
                    await receiver.CompleteMessageAsync(msg);
                }
                else
                {
                    await receiver.AbandonMessageAsync(msg);
                }
            }
        }

        //internal static async ValueTask<ServiceBusMessage> ToServiceBusMessage<T>(T instance)
        //{
        //    var binaryData = new BinaryData(instance.ToJson().ToBytes());

        //    var result = new ServiceBusMessage(binaryData);

        //    return await ValueTask.FromResult(result);
        //}

        /// <summary>
        /// Converts an instance of type T to a <see cref="ServiceBusMessage"/> asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be converted.</typeparam>
        /// <returns>A <see cref="ValueTask{ServiceBusMessage}"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method serializes the instance of type T to JSON, then converts it to bytes and wraps it in a 
        /// <see cref="ServiceBusMessage"/>. This message can then be sent to a service bus.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the provided instance is null.</exception>
        /// 
        /// <summary>
        /// Releases resources used by the Service Bus client
        /// </summary>
        /// <param name="disposing">true to release managed resources; false to release only unmanaged resources</param>
        /// <exception cref="ServiceBusException">Thrown if error occurs during client disposal</exception>
        /// <remarks>
        /// <para>Releases connection resources and cleans up any pending operations</para>
        /// <seealso cref="ServiceBusClient.DisposeAsync"/>
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Client()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Finalizer for the <see cref="Client"/> class.
        /// <para>
        /// The finalizer is only defined if 'Dispose(bool disposing)' includes code to free unmanaged resources.
        /// </para>
        /// </summary>
        // ~Client() 
        // { 
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method 
        //     Dispose(disposing: false); 
        // } 
        
        /// <summary>
        /// Releases all resources used by the <see cref="Client"/> instance.
        /// <para>
        /// This method is called to dispose of both managed and unmanaged resources.
        /// If this method has already been called, it will not perform any action.
        /// </para>
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
