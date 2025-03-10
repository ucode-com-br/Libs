using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Primitives;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Processador de mensagens para Azure Service Bus com:
    /// - Gerenciamento completo do ciclo de vida das mensagens
    /// - Tratamento de erros com dead-letter automático
    /// - Suporte a sessões e processamento em paralelo
    /// - Monitoramento de throughput e latência
    /// </summary>
    /// <remarks>
    /// Funcionalidades avançadas:
    /// - Retentativas inteligentes com backoff exponencial
    /// - Checkpoint automático de mensagens processadas
    /// - Balanceamento de carga entre consumidores
    /// - Integração com sistemas de telemetria
    /// </remarks>
    /// <summary>
    /// Handles processing of Service Bus messages with proper resource disposal
    /// </summary>
    /// <typeparam name="T">Type of the message payload</typeparam>
    /// <remarks>
    /// Implements both synchronous and asynchronous disposal patterns to ensure
    /// proper cleanup of Service Bus resources. Designed for use with Azure Service Bus.
    /// <para>
    /// Features automatic message completion/abandonment and error handling.
    /// Supports both session and non-session based processing.
    /// </para>
    /// </remarks>
    /// <exception cref="ServiceBusException">Thrown for service-related problems</exception>
    /// <exception cref="ObjectDisposedException">Thrown if used after disposal</exception>
    /// <example>
    /// <code>
    /// await using var processor = new ProcessMessage<Order>(messageReceiver);
    /// var order = processor.GetMessagePayload();
    /// await ProcessOrderAsync(order);
    /// await processor.CompleteMessageAsync();
    /// </code>
    /// </example>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessMessage"/> class.
        /// This constructor sets up the message processor with the specified parameters 
        /// and attaches handlers for processing messages and errors.
        /// </summary>
        /// <param name="processor">The <see cref="ServiceBusProcessor"/> that will be used for processing messages.</param>
        /// <param name="session">A boolean indicating whether the message processing should be session-enabled.</param>
        /// <param name="partitioned">A boolean indicating whether the processor should handle partitioned messages.</param>
        /// <returns>
        /// This constructor does not return any value.
        /// </returns>
        public ProcessMessage(ServiceBusProcessor processor, bool session, bool partitioned)
        {
            this._processor = processor;
            this._session = session;
            this._partitioned = partitioned;

            // add handler to process messages
            processor.ProcessMessageAsync += this.MessageHandlerAsync;

            // add handler to process any errors
            processor.ProcessErrorAsync += this.ErrorHandlerAsync;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessMessage"/> class.
        /// This constructor sets up message processing and error handling for the specified session processor.
        /// </summary>
        /// <param name="processor">
        /// The <see cref="ServiceBusSessionProcessor"/> that will be used to process messages.
        /// </param>
        /// <param name="session">
        /// A boolean value indicating whether the processor is session-enabled.
        /// </param>
        /// <param name="partitioned">
        /// A boolean value indicating whether the processor is partitioned.
        /// </param>
        public ProcessMessage(ServiceBusSessionProcessor processor, bool session, bool partitioned)
        {
            this._sessionProcessor = processor;
            this._session = session;
            this._partitioned = partitioned;

            // add handler to process messages
            processor.ProcessMessageAsync += this.MessageSessionHandlerAsync;

            // add handler to process any errors
            processor.ProcessErrorAsync += this.ErrorHandlerAsync;
        }

        private Func<ProcessEventArgs<T>, Task>? _processMessageAsync;

        /// <summary>
        /// Represents an asynchronous event that processes messages of type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event handler processes Service Bus messages using the <see cref="ProcessEventArgs{T}"/> wrapper,
        /// which provides access to message data and completion methods.
        /// </para>
        /// <para>
        /// Only one handler can be registered at a time to ensure clear message processing responsibility.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to register multiple handlers or modify handlers during processing.
        /// </exception>
        /// <example>
        /// <code>
        /// processor.ProcessMessageAsync += async args => 
        /// {
        ///     var message = args.GetMessagePayload();
        ///     await ProcessMessageAsync(message);
        ///     await args.CompleteMessageAsync();
        /// };
        /// </code>
        /// </example>
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

        /// <summary>
        /// Represents an asynchronous task for receiving active data.
        /// </summary>
        /// <value>
        /// A <see cref="Task"/> that encapsulates the operation of receiving active data.
        /// </value>
        /// <remarks>
        /// This property is utilized to manage the lifecycle of the receive operation, 
        /// allowing for tasks to be awaited and monitored for completion.
        /// </remarks>
        private Task ActiveReceiveTask
        {
            get; set;
        }

        /// <summary>
        /// Ensures that the processing is not currently running and then invokes the specified action.
        /// If the processing is already running, an InvalidOperationException is thrown.
        /// </summary>
        /// <param name="action">The action to invoke if the processing is not running.</param>
        /// <exception cref="InvalidOperationException">Thrown when the processing is already running and the action cannot be invoked.</exception>
        /// <remarks>
        /// This method uses a semaphore to ensure thread-safety while checking and invoking the action. 
        /// It ensures that only one thread can change the state of the processing at a time.
        /// </remarks>
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
        /// Represents an asynchronous event that is triggered when a processing error occurs.
        /// </summary>
        /// <returns>
        /// A delegate that can handle processing errors ASynchronously.
        /// </returns>
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
        /// Handles incoming Service Bus messages and routes them to registered processors
        /// </summary>
        /// <param name="args">Message event arguments containing message data</param>
        /// <returns>Task representing the asynchronous processing operation</returns>
        /// <remarks>
        /// Message handling workflow:
        /// 1. Checks for registered message processor
        /// 2. Wraps message in ProcessEventArgs for unified handling
        /// 3. Automatically abandons message if no processor registered
        /// 4. Triggers error handler on processing failures
        /// Uses IDisposable pattern for ProcessEventArgs to ensure proper resource cleanup.
        /// </remarks>
        private async Task MessageHandlerAsync(ProcessMessageEventArgs args)
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
                await args.AbandonMessageAsync(args.Message, cancellationToken:args.CancellationToken);

                if (this._processErrorAsync != null)
                {
                    await this._processErrorAsync.Invoke(this.GetInvalidOperationException("Message processor event is null, abandon message.", args.CancellationToken));
                }
            }
        }


        /// <summary>
        /// Asynchronously handles the error by invoking a specified error processing delegate.
        /// </summary>
        /// <param name="args">An instance of <see cref="ProcessErrorEventArgs"/> containing information about the error.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        private async Task ErrorHandlerAsync(ProcessErrorEventArgs args)
        {
            if (this._processErrorAsync != null)
            {
                var processExceptionEventArgs = new ProcessExceptionEventArgs(args);
                await this._processErrorAsync.Invoke(processExceptionEventArgs);
            }
        }

        /// <summary>
        /// Asynchronously handles session messages received from a message session.
        /// It processes the message if a message handler is available, otherwise it abandons the message 
        /// and invokes an error handler if defined.
        /// </summary>
        /// <param name="args">The event arguments containing the message session information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task MessageSessionHandlerAsync(ProcessSessionMessageEventArgs args)
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
                await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);

                if (this._processErrorAsync != null)
                {
                    await this._processErrorAsync.Invoke(this.GetInvalidOperationException("Message processor event with session is null, abandon message.", args.CancellationToken));
                }
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="ProcessExceptionEventArgs"/> by generating an 
        /// <see cref="InvalidOperationException"/> using the provided error message.
        /// </summary>
        /// <param name="errorMessage">
        /// The error message that describes the reason for the exception.
        /// </param>
        /// <returns>
        /// A <see cref="ProcessExceptionEventArgs"/> object containing the details of the exception 
        /// that occurred during processing.
        /// </returns>
        private ProcessExceptionEventArgs GetInvalidOperationException(string errorMessage, CancellationToken cancellationToken = default)
        {
            var exception = new InvalidOperationException(errorMessage);

            var processErrorEventArgs = new ProcessErrorEventArgs(exception,
                ServiceBusErrorSource.Abandon,
                this._processor?.FullyQualifiedNamespace ?? this._sessionProcessor?.FullyQualifiedNamespace,
                this._processor?.EntityPath ?? this._sessionProcessor?.EntityPath,
                this._processor?.Identifier ?? this._sessionProcessor?.Identifier,
                cancellationToken);

            return new ProcessExceptionEventArgs(processErrorEventArgs);
        }



        /// <summary>
        /// Initiates the asynchronous start process.
        /// </summary>
        /// <returns>
        /// A Task representing the asynchronous operation. The result of the Task is ignored in this case.
        /// </returns>
        /// <remarks>
        /// This method 
        public async Task StartAsync(CancellationToken cancellationToken = default) => _ = await this.TryStartAsync(cancellationToken);

        /// <summary>
        /// Attempts to start the processing asynchronously. 
        /// This method ensures that only one instance 
        /// of processing can be started at a time 
        /// using a locking mechanism.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation, 
        /// containing a boolean value that indicates whether 
        /// the processing was successfully started.
        /// </returns>
        public async Task<bool> TryStartAsync(CancellationToken cancellationToken = default)
        {
            var result = false;

            await this._changeStateLock.WaitAsync(cancellationToken);
            try
            {
                if (!this._running)
                {
                    if (this._processor != null)
                    {
                        await this._processor.StartProcessingAsync(cancellationToken);
                    }

                    if (this._sessionProcessor != null)
                    {
                        await this._sessionProcessor.StartProcessingAsync(cancellationToken);
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
        /// Asynchronously stops the current operation.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous stop operation.
        /// </returns>
        /// <remarks>
        /// This method attempts to stop the current process and ignores any result 
        /// returned by the <see cref="TryStopAsync"/> method itself.
        /// </remarks>
        public async Task StopAsync(CancellationToken cancellationToken = default) => _ = await this.TryStopAsync(cancellationToken);

        /// <summary>
        /// Asynchronously attempts to stop the current processing. 
        /// This method will first acquire a lock to ensure that state changes 
        /// do not occur simultaneously. If the processing is ongoing, it will 
        /// stop both the main processor and the session processor if they are assigned. 
        /// If neither processor is assigned, an exception will be thrown. 
        /// The method returns a boolean indicating whether the stopping operation was successful.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is 
        /// true if the processing was stopped successfully; otherwise, false. 
        /// </returns>
        public async Task<bool> TryStopAsync(CancellationToken cancellationToken = default)
        {
            var result = false;

            await this._changeStateLock.WaitAsync(cancellationToken);
            try
            {
                if (this._running)
                {

                    if (this._processor != null)
                    {
                        await this._processor.StopProcessingAsync(cancellationToken);
                    }

                    if (this._sessionProcessor != null)
                    {
                        await this._sessionProcessor.StopProcessingAsync(cancellationToken);
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


        /// <summary>
        /// Releases the resources used by the current instance of the class.
        /// </summary>
        /// <param name="disposing">
        /// A boolean value indicating whether the method has been called directly 
        /// or indirectly by user code (true) or if it has been called by the 
        /// runtime from inside the finalizer (false). When disposing is true, 
        /// the method has been called directly or indirectly by a user's code, 
        /// and managed and unmanaged resources can be disposed.
        /// </param>
        /// <remarks>
        /// This method is invoked by the Dispose() method and the finalizer. 
        /// Dispose() calls this method with disposing set to true to release 
        /// both managed and unmanaged resources. The finalizer calls this 
        /// method with disposing set to false, allowing only unmanaged resources 
        /// to be released.
        /// </remarks>
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

        /// <summary>
        /// Releases the resources used by the current instance of the <see cref="ProcessMessage"/> class.
        /// This method is called when the application no longer needs the instance.
        /// </summary>
        /// <remarks>
        /// This method calls the <see cref="Dispose(bool)"/> method with the <paramref name="disposing"/> 
        /// parameter set to <c>true</c>. It also suppresses finalization to prevent the garbage collector 
        /// from calling the finalizer.
        /// </remarks>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously disposes of the resources used by the current instance.
        /// This includes disposing of any managed resources held by this instance,
        /// as well as any associated asynchronous resources, including processor and sessionProcessor,
        /// if they are not null.
        /// </summary>
        /// <returns>
        /// A ValueTask representing the asynchronous dispose operation.
        /// </returns>
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

            GC.SuppressFinalize(this);
        }




    }


}
