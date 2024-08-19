using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace UCode.ServiceBus
{
    public class ProcessMessage<T> : IDisposable, IAsyncDisposable
    {
        private bool _disposedValue;
        private readonly ServiceBusProcessor _processor;
        private readonly ServiceBusSessionProcessor _sessionProcessor;
        private readonly bool _session;
        private readonly bool _partitioned;
        private bool _running;
        private readonly SemaphoreSlim _changeStateLock = new(1, 1);
        private readonly SemaphoreSlim _processingStartStopSemaphore = new(1, 1);

        public ProcessMessage(ServiceBusProcessor processor, bool session, bool partitioned)
        {
            this._processor = processor;
            this._session = session;
            this._partitioned = partitioned;

            // add handler to process messages
            processor.ProcessMessageAsync += this.MessageHandler;

            // add handler to process any errors
            processor.ProcessErrorAsync += this.ErrorHandler;
        }

        /// <summary>
        /// ServiceBusSessionProcessor
        /// </summary>
        public ProcessMessage(ServiceBusSessionProcessor processor, bool session, bool partitioned)
        {
            this._sessionProcessor = processor;
            this._session = session;
            this._partitioned = partitioned;

            // add handler to process messages
            processor.ProcessMessageAsync += this.MessageSessionHandler;

            // add handler to process any errors
            processor.ProcessErrorAsync += this.ErrorHandler;
        }

        private Func<ProcessEventArgs<T>, Task>? _processMessageAsync;
        /// <summary>
        /// The handler responsible for processing messages received from the Queue
        /// or Subscription.
        /// Implementation is mandatory.
        /// </summary>
        /// <remarks>
        /// It is not recommended that the state of the processor be managed directly from within this handler; requesting to start or stop the processor may result in
        /// a deadlock scenario.
        /// </remarks>
        public event Func<ProcessEventArgs<T>, Task> ProcessMessageAsync
        {
            add
            {
                //ArgumentException.AssertNotNull(value, nameof(this.ProcessMessageAsync));
                if (this._processMessageAsync != default)
                {
                    throw new InvalidOperationException("Only one handler is allowed");
                }

                this.EnsureNotRunningAndInvoke(() => this._processMessageAsync = value);
            }
            remove
            {
                //ArgumentException.AssertNotNull(value, nameof(this.ProcessMessageAsync));
                if (this._processMessageAsync != default)
                {
                    throw new InvalidOperationException("Only one handler is allowed");
                }

                this.EnsureNotRunningAndInvoke(() => this._processMessageAsync = default);
            }
        }
        private Task ActiveReceiveTask
        {
            get; set;
        }

        internal void EnsureNotRunningAndInvoke(Action action)
        {
            if (this.ActiveReceiveTask == null)
            {
                try
                {
                    this._processingStartStopSemaphore.Wait();

                    if (this.ActiveReceiveTask == null)
                    {
                        action?.Invoke();
                    }
                    else
                    {
                        throw new InvalidOperationException("Running message processor cannot perform operation.");
                    }
                }
                finally
                {
                    this._processingStartStopSemaphore.Release();
                }
            }
            else
            {
                throw new InvalidOperationException("Running message processor cannot perform operation.");
            }
        }


        private Func<ProcessExceptionEventArgs, Task> _processErrorAsync;
        /// <summary>
        /// The handler responsible for processing messages received from the Queue
        /// or Subscription. Implementation is mandatory.
        /// </summary>
        public event Func<ProcessExceptionEventArgs, Task> ProcessErrorAsync
        {
            add
            {
                //Argument.AssertNotNull(value, nameof(ProcessErrorAsync));

                if (this._processErrorAsync != default)
                {
                    throw new NotSupportedException("Handler has already been assigned.");
                }

                this.EnsureNotRunningAndInvoke(() => this._processErrorAsync = value);
            }
            remove
            {
                //Argument.AssertNotNull(value, nameof(ProcessErrorAsync));

                if (this._processErrorAsync != value)
                {
                    throw new ArgumentException("Handler Has Not Been Assigned");
                }

                this.EnsureNotRunningAndInvoke(() => this._processErrorAsync = default);
            }
        }



        /// <summary>
        /// handle any meessage when receiving messages
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            if (this._processMessageAsync != null)
            {
                await using (var processEventArgs = new ProcessEventArgs<T>(args, this._session, this._partitioned))
                {
                    await this._processMessageAsync.Invoke(processEventArgs);
                }
            }
            else
            {
                await args.AbandonMessageAsync(args.Message);

                if (this._processErrorAsync != null)
                {
                    await this._processErrorAsync.Invoke(this.GetInvalidOperationException("Message processor event is null, abandon message."));
                }
            }
        }


        /// <summary>
        /// handle any errors when receiving messages
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task ErrorHandler(ProcessErrorEventArgs args)
        {
            if (this._processErrorAsync != null)
            {
                var processExceptionEventArgs = new ProcessExceptionEventArgs(args);
                await this._processErrorAsync.Invoke(processExceptionEventArgs);
            }
        }

        /// <summary>
        /// handle any meessage when receiving messages
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task MessageSessionHandler(ProcessSessionMessageEventArgs args)
        {
            if (this._processMessageAsync != null)
            {
                await using (var processEventArgs = new ProcessEventArgs<T>(args, this._session, this._partitioned))
                {
                    await this._processMessageAsync.Invoke(processEventArgs);
                }
            }
            else
            {
                await args.AbandonMessageAsync(args.Message);

                if (this._processErrorAsync != null)
                {
                    await this._processErrorAsync.Invoke(this.GetInvalidOperationException("Message processor event with session is null, abandon message."));
                }
            }
        }

        private ProcessExceptionEventArgs GetInvalidOperationException(string errorMessage)
        {
            var exception = new InvalidOperationException(errorMessage);
            CancellationToken cancellationToken = default;

            var processErrorEventArgs = new ProcessErrorEventArgs(exception,
                ServiceBusErrorSource.Abandon,
                this._processor?.FullyQualifiedNamespace ?? this._sessionProcessor?.FullyQualifiedNamespace,
                this._processor?.EntityPath ?? this._sessionProcessor?.EntityPath,
                this._processor?.Identifier ?? this._sessionProcessor?.Identifier,
                cancellationToken);

            return new ProcessExceptionEventArgs(processErrorEventArgs);
        }



        /// <summary>
        /// Start ignoring if succeed or not
        /// </summary>
        public async Task StartAsync() => _ = await this.TryStartAsync();

        /// <summary>
        /// Start with result
        /// </summary>
        /// <returns>succeed to start</returns>
        public async Task<bool> TryStartAsync()
        {
            var result = false;

            await this._changeStateLock.WaitAsync();
            try
            {
                if (!this._running)
                {
                    if (this._processor != null)
                    {
                        await this._processor.StartProcessingAsync();
                    }

                    if (this._sessionProcessor != null)
                    {
                        await this._sessionProcessor.StartProcessingAsync();
                    }

                    if (this._processor == null && this._sessionProcessor == null)
                    {
                        throw new InvalidOperationException("No processor has been assigned");
                    }

                    this._running = true;
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            finally
            {
                this._changeStateLock.Release();
            }

            return result;
        }

        /// <summary>
        /// Stop ignoring if succeed or not
        /// </summary>
        public async Task StopAsync() => _ = await this.TryStopAsync();

        /// <summary>
        /// Stop with result
        /// </summary>
        /// <returns>succeed to stop</returns>
        public async Task<bool> TryStopAsync()
        {
            var result = false;

            await this._changeStateLock.WaitAsync();
            try
            {
                if (this._running)
                {

                    if (this._processor != null)
                    {
                        await this._processor.StopProcessingAsync();
                    }

                    if (this._sessionProcessor != null)
                    {
                        await this._sessionProcessor.StopProcessingAsync();
                    }

                    if (this._processor == null && this._sessionProcessor == null)
                    {
                        throw new InvalidOperationException("No processor has been assigned");
                    }

                    this._running = false;
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            finally
            {
                this._changeStateLock.Release();
            }

            return result;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this._disposedValue = true;
            }

            try
            {
                this.DisposeAsync().AsTask().Wait();
            }
            catch (Exception)
            {

            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ProcessMessage()
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

        public async ValueTask DisposeAsync()
        {
            if (!this._disposedValue)
            {
                this.Dispose();

                if (this._processor != null)
                {
                    await this._processor.DisposeAsync();
                }

                if (this._sessionProcessor != null)
                {
                    await this._sessionProcessor.DisposeAsync();
                }
            }
        }




    }


}
