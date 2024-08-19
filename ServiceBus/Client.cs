using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using UCode.Extensions;

namespace UCode.ServiceBus
{
    public class Client : IDisposable
    {


        private readonly ServiceBusAdministrationClient _serviceBusAdministrationClient;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly bool _session;
        private readonly bool _partitioned;
        private readonly bool _autoCreateQueue;
        private readonly Action<CreateQueueOptions> _defaultCreateQueueOptions;
        private bool disposedValue;

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
        public IReadOnlyList<string> Queues => this._dictQueueProperties.Keys.ToList();

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

        public async Task<Sender> CreateSenderAsync(string queueName)
        {
            if (await this.ExistQueueAsync(queueName))
            {
                var serviceBusSender = this._serviceBusClient.CreateSender(queueName);

                return new(serviceBusSender, this._session, this._partitioned);
            }

            return null;
        }

        public async Task<Receiver> CreateReceiverAsync(string queueName)
        {
            if (await this.ExistQueueAsync(queueName))
            {
                var receiver = this._serviceBusClient.CreateReceiver(queueName);

                return new Receiver(receiver, this._session, this._partitioned);
            }

            return null;
        }

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
        /// Transfer dead letter sub queue to principal
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="validate"></param>
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
        public async Task ResubmitDeadLetterAsync(string queueName, string? sessionId = null) => await this.ResubmitDeadLetterAsync<string>(queueName, null, sessionId);

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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
