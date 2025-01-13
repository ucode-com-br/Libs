using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UCode.Blob;

namespace UCode.Apis.Webhook
{
    public class Enqueue: IDisposable
    {
        private readonly Container _container;
        private readonly ServiceBus.Sender _servicebusSender;
        private bool _disposedValue;

        public Enqueue(Container container, ServiceBus.Sender servicebusSender)
        {
            this._container = container;
            this._servicebusSender = servicebusSender;
        }

        public async Task Send<T>(T instance, Func<T, string> funcMessageId, CancellationToken cancellationToken = default)
        {
            var messageId = funcMessageId(instance);

            await this._servicebusSender.SendOneAsync(instance,
                sendMessage =>
                {
                    sendMessage.MessageId = messageId;
                },
                cancellationToken);
        }

        public async Task Send<T>(T instance, string messageId, CancellationToken cancellationToken = default) => this.Send<T>(instance, ins => messageId, cancellationToken);



        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    this._servicebusSender.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this._disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Enqueue()
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
