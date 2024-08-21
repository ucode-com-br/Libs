using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace UCode.ServiceBus
{
    public class Sender : IAsyncDisposable, IDisposable
    {
        private readonly ServiceBusSender _serviceBusSender;
        private readonly bool _session;
        private readonly bool _partitioned;
        private bool disposedValue;

        internal Sender(ServiceBusSender serviceBusSender, bool session, bool partitioned)
        {
            this._serviceBusSender = serviceBusSender;
            this._session = session;
            this._partitioned = partitioned;
        }


        public async ValueTask SendOneAsync<T>([NotNull] T instance, Action<SendMessage<T>> configureMessage = default, CancellationToken cancellationToken = default)
        {
            var msg = new SendMessage<T>(instance);

            configureMessage?.Invoke(msg);

            try
            {
                await this._serviceBusSender.SendMessageAsync(msg, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public struct SendResult<T>
        {
            public SendResult(T @value, Exception exception)
            {
                this.Value = @value;
                this.Exception = exception;
            }

            public SendResult(T @value) => this.Value = @value;

            public T Value
            {
                get; set;
            }
            public Exception? Exception { get; set; } = null;
        }

        public async IAsyncEnumerable<SendResult<T>> SendAsync<T>([NotNull] IEnumerable<T> instances, Action<SendMessage<T>> configureMessage = default, CancellationToken cancellationToken = default)
        {
            foreach (var instance in instances)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var message = new SendMessage<T>(instance);

                configureMessage?.Invoke(message);

                SendResult<T> sr;
                try
                {
                    await this._serviceBusSender.SendMessageAsync(message, cancellationToken);
                    sr = new SendResult<T>(instance);
                }
                catch (Exception ex)
                {
                    sr = new SendResult<T>(instance, ex);
                }
                yield return sr;
            }
        }
        public IEnumerable<SendResult<T>> Send<T>([NotNull] IEnumerable<T> instances, Action<SendMessage<T>> configureMessage = default, CancellationToken cancellationToken = default)
        {
            var task = this.PrivateSendAsync(instances, configureMessage, cancellationToken);

            task.AsTask().Wait(cancellationToken);

            return task.Result;
        }

        private async ValueTask<IEnumerable<SendResult<T>>> PrivateSendAsync<T>([NotNull] IEnumerable<T> instances, Action<SendMessage<T>> configureMessage = default, CancellationToken cancellationToken = default)
        {
            var list = new List<SendResult<T>>();

            await foreach (var item in this.SendAsync(instances, configureMessage, cancellationToken))
            {
                list.Add(item);
            }

            return list;
        }

        //public async ValueTask SendBatchAsync<T>([NotNull] IAsyncEnumerable<T> instances, Action<Message<T>> configureMessage = default, CancellationToken cancellationToken = default)
        //{
        //    var messages = await _serviceBusSender.CreateMessageBatchAsync(cancellationToken);

        //    await foreach (var instance in instances.WithCancellation(cancellationToken))
        //    {
        //        var msg = new Message<T>(instance);

        //        configureMessage?.Invoke(msg);

        //        if (!messages.TryAddMessage(msg))
        //        {
        //            throw new Exception($"Fail add message to batch.");
        //        }
        //    }
        //    await _serviceBusSender.SendMessagesAsync(messages, cancellationToken);
        //}

        public async ValueTask SendBatchAsync<T>(
        [NotNull] IEnumerable<T> instances,
        Action<SendMessage<T>> configureMessage = default,
        int chunkSize = 4000,
        long? maxSizeInBytes = null,
        CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task>();

            var count = 0;

            foreach (var itens in instances.Chunk(chunkSize))
            {

                var messages = await this._serviceBusSender.CreateMessageBatchAsync(
                    new CreateMessageBatchOptions() { MaxSizeInBytes = maxSizeInBytes }, cancellationToken);

                foreach (var instance in itens)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var msg = new SendMessage<T>(instance, this._session, this._partitioned);

                    configureMessage?.Invoke(msg);

                    if (!messages.TryAddMessage(msg))
                    {
                        if (messages.Count > 0)
                        {
                            tasks.Add(this._serviceBusSender.SendMessagesAsync(messages, cancellationToken));

                            messages = await this._serviceBusSender.CreateMessageBatchAsync(
                                new CreateMessageBatchOptions() { MaxSizeInBytes = maxSizeInBytes }, cancellationToken);


                            if (!messages.TryAddMessage(msg))
                            {
                                throw new Exception($"Fail add message to batch (retry).", new Exception($"Total of messages {count}"));
                            }
                        }
                        else
                        {
                            throw new Exception($"Fail add message to batch.", new Exception($"Total of messages {count}"));
                        }
                    }
                    else
                    {
                        Interlocked.Increment(ref count);
                    }
                }

                if (messages.Count > 0)
                {
                    tasks.Add(this._serviceBusSender.SendMessagesAsync(messages, cancellationToken));
                }
            }

            await Task.WhenAll(tasks);
            //await _serviceBusSender.SendMessagesAsync(messages, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            if (this._serviceBusSender != null)
            {
                await this._serviceBusSender.DisposeAsync();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        this.DisposeAsync().AsTask().Wait();
                    }
                    catch (Exception)
                    {
                    }
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Sender()
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
