using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using UCode.Extensions;

namespace UCode.ServiceBus
{
    public class ReceiveMessage<T> : IAsyncDisposable
    {
        private readonly ServiceBusReceivedMessage _serviceBusReceivedMessage;

        // Never dispose this object (its a reference from the receiver inside the ProcessEventArgs)
        private readonly Receiver _receiver;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private T? _body;

        /// <summary>
        /// Using session
        /// </summary>
        public bool UseSession
        {
            get;
        }

        /// <summary>
        /// Using partition
        /// </summary>
        public bool UsePartition
        {
            get;
        }

        public bool AutoComplete
        {
            get; set;
        } = true;

        public bool AutoRenewLock
        {
            get; set;
        } = true;

        public bool IsCompleted
        {
            get;
            private set;
        }

        public bool IsAbandoned
        {
            get; private set;
        }

        public async ValueTask CompletedAsync() => _ = this.TryCompletedAsync();
        public async ValueTask<bool> TryCompletedAsync()
        {
            var result = false;
            await this._semaphore.WaitAsync();

            try
            {
                if (!this.IsCompleted && !this.IsAbandoned)
                {
                    await this._receiver.CompletedAsync(this._serviceBusReceivedMessage);

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

        public async ValueTask AbandonAsync() => _ = this.TryAbandonAsync();
        public async ValueTask<bool> TryAbandonAsync()
        {
            var result = false;
            await this._semaphore.WaitAsync();

            try
            {
                if (!this.IsCompleted && !this.IsAbandoned)
                {
                    await this._receiver.AbandonAsync(this._serviceBusReceivedMessage);

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

        public async ValueTask RenewLockAsync() => _ = this.TryRenewLockAsync();
        public async ValueTask<bool> TryRenewLockAsync()
        {
            var result = false;
            await this._semaphore.WaitAsync();

            try
            {
                if (!this.IsCompleted && !this.IsAbandoned)
                {
                    await this._receiver.RenewLockAsync(this._serviceBusReceivedMessage);
                    result = true;
                }
            }
            finally
            {
                this._semaphore.Release();
            }
            return result;
        }


        public ReceiveMessage(Receiver receiver, ServiceBusReceivedMessage serviceBusMessage, bool session = false, bool partitioned = false)
        {
            this._serviceBusReceivedMessage = serviceBusMessage;

            this.UseSession = session;
            this.UsePartition = partitioned;
            this._receiver = receiver;
        }


        public IReadOnlyDictionary<string, object> ApplicationProperties => this._serviceBusReceivedMessage.ApplicationProperties;

        public string MessageId => this._serviceBusReceivedMessage.MessageId;

        public string PartitionKey => this._serviceBusReceivedMessage.PartitionKey;
        public string TransactionPartitionKey => this._serviceBusReceivedMessage.TransactionPartitionKey;

        /// <summary>
        /// Usado para agrupar multiplas mensagens
        /// </summary>
        public string SessionId => this._serviceBusReceivedMessage.SessionId;
        public string ReplyToSessionId => this._serviceBusReceivedMessage.ReplyToSessionId;
        public string Subject => this._serviceBusReceivedMessage.Subject;
        public string To => this._serviceBusReceivedMessage.To;
        public string ReplyTo => this._serviceBusReceivedMessage.ReplyTo;
        public string CorrelationId => this._serviceBusReceivedMessage.CorrelationId;

        public TimeSpan? TimeToLive => this._serviceBusReceivedMessage.TimeToLive;
        public string LockToken => this._serviceBusReceivedMessage.LockToken;
        public int DeliveryCount => this._serviceBusReceivedMessage.DeliveryCount;
        public DateTimeOffset LockedUntil => this._serviceBusReceivedMessage.LockedUntil;
        public long SequenceNumber => this._serviceBusReceivedMessage.SequenceNumber;
        public long EnqueuedSequenceNumber => this._serviceBusReceivedMessage.EnqueuedSequenceNumber;
        public DateTimeOffset EnqueuedTime => this._serviceBusReceivedMessage.EnqueuedTime;
        public string DeadLetterSource => this._serviceBusReceivedMessage.DeadLetterSource;
        public string DeadLetterReason => this._serviceBusReceivedMessage.DeadLetterReason;
        public string DeadLetterErrorDescription => this._serviceBusReceivedMessage.DeadLetterErrorDescription;

        public DateTimeOffset ExpiresAt => this._serviceBusReceivedMessage.ExpiresAt;
        public string? State => System.Enum.GetName(this._serviceBusReceivedMessage.State);
        public string ContentType => this._serviceBusReceivedMessage.ContentType ?? "application/json";

        public DateTimeOffset? ScheduledEnqueueTime => this._serviceBusReceivedMessage.ScheduledEnqueueTime;

        /// <summary>
        /// Body instance deserialized using json
        /// </summary>
        public T Body
        {
            get
            {
                if (this.AutoRenewLock)
                {
                    var r = this.RenewLockAsync();
                    r.AsTask().Wait();
                }

                return this._body ??= this._serviceBusReceivedMessage.Body.ToObjectFromJson<T>();
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

        public override string ToString() => this.JsonString();


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

        }
    }
}
