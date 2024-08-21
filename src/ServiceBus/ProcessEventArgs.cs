using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using UCode.Extensions;

namespace UCode.ServiceBus
{
    public class ProcessEventArgs<T> : EventArgs, IAsyncDisposable
    {
        //private readonly Receiver _receiver;
        private readonly ProcessMessageEventArgs _processMessageEventArgs;
        private readonly ProcessSessionMessageEventArgs _processSessionMessageEventArgs;
        private readonly bool _session;
        private readonly bool _partitioned;

        public ProcessEventArgs(ProcessMessageEventArgs processMessageEventArgs, bool session = false, bool partitioned = false)
        {
            this._processMessageEventArgs = processMessageEventArgs;
            this._session = session;
            this._partitioned = partitioned;

            //this._receiver = new Receiver(this._processMessageEventArgs.GetPrivateField<ProcessMessageEventArgs, ServiceBusReceiver>("_receiver"), this._session, this._partitioned);

            var receiver = new Receiver(this._processMessageEventArgs.GetPrivateField<ProcessMessageEventArgs, ServiceBusReceiver>("_receiver"), this._session, this._partitioned);

            this.Message = new ReceiveMessage<T>(receiver, this._processMessageEventArgs.Message, this._session, this._partitioned);
        }

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
        /// The identifier of the processor that raised this event.
        /// </summary>
        public string Identifier => this._processMessageEventArgs.Identifier;

        public ReceiveMessage<T> Message
        {
            get;
        }

        public async ValueTask DisposeAsync() => await this.Message.DisposeAsync();
    }
}
