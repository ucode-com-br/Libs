using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace UCode.ServiceBus
{
    public class Receiver : IAsyncDisposable
    {
        private readonly ServiceBusReceiver _serviceBusReceiver;
        private readonly bool _session;
        private readonly bool _partitioned;

        internal Receiver(ServiceBusReceiver serviceBusReceiver, bool session, bool partitioned)
        {
            this._serviceBusReceiver = serviceBusReceiver;
            this._session = session;
            this._partitioned = partitioned;
        }

        /*public struct ReceiverResult<T>
        {
            public ReceiverResult(T @value, Exception exception)
            {
                this.Value = @value;
                this.Exception = exception;
            }

            public ReceiverResult(T @value) => this.Value = @value;

            /// <summary>
            /// Deserialized value with Json
            /// </summary>
            public T Value
            {
                get; set;
            }

            /// <summary>
            /// Exception if any
            /// </summary>
            public Exception? Exception { get; set; } = null;
        }*/
        public async Task CloseAsync() => await this._serviceBusReceiver.CloseAsync();

        public bool IsClosed => this._serviceBusReceiver.IsClosed;

        public async Task<ReceiveMessage<T>> ReceiveMessageAsync<T>(TimeSpan? maxWaitTime = null)
        {
            var message = await this._serviceBusReceiver.ReceiveMessageAsync(maxWaitTime);

            return new ReceiveMessage<T>(this, message, this._session, this._partitioned);
        }

        public async Task<IEnumerable<ReceiveMessage<T>>> ReceiveMessagesAsync<T>(int maxMessage, TimeSpan? timeOut = null) => (await this._serviceBusReceiver.ReceiveMessagesAsync(maxMessage, timeOut)).Select(s => new ReceiveMessage<T>(this, s, this._session, this._partitioned));

        internal async ValueTask CompletedAsync(ServiceBusReceivedMessage serviceBusReceivedMessage) => await this._serviceBusReceiver.CompleteMessageAsync(serviceBusReceivedMessage);

        internal async ValueTask AbandonAsync(ServiceBusReceivedMessage serviceBusReceivedMessage) => await this._serviceBusReceiver.AbandonMessageAsync(serviceBusReceivedMessage);

        internal async ValueTask RenewLockAsync(ServiceBusReceivedMessage serviceBusReceivedMessage) => await this._serviceBusReceiver.RenewMessageLockAsync(serviceBusReceivedMessage);
        public async ValueTask DisposeAsync() => await this._serviceBusReceiver.DisposeAsync();

        //public async ValueTask<IEnumerable<ServiceBusReceivedMessage>> ReceiveMessagesAsync(int maxMessage, TimeSpan? timeOut = null) => await this._serviceBusReceiver.ReceiveMessagesAsync(maxMessage, timeOut);
    }
}
