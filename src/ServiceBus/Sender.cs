using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Represents a sender that implements both asynchronous and synchronous disposal patterns.
    /// </summary>
    /// <remarks>
    /// The Sender class is responsible for sending data and handling resources properly,
    /// ensuring that resources are released when no longer needed. It implements the 
    /// IAsyncDisposable and IDisposable interfaces for managing both asynchronous and 
    /// synchronous disposal, respectively.
    /// </remarks>
    public class Sender : IAsyncDisposable, IDisposable
    {
        private readonly ServiceBusSender _serviceBusSender;
        private readonly bool _session;
        private readonly bool _partitioned;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sender"/> class.
        /// </summary>
        /// <param name="serviceBusSender">The <see cref="ServiceBusSender"/> instance used for sending messages.</param>
        /// <param name="session">Indicates whether sessions are enabled for this sender.</param>
        /// <param name="partitioned">Indicates whether the sender is partitioned.</param>
        internal Sender(ServiceBusSender serviceBusSender, bool session, bool partitioned)
        {
            this._serviceBusSender = serviceBusSender;
            this._session = session;
            this._partitioned = partitioned;
        }


        /// <summary>
        /// Sends a message asynchronously to the service bus with the specified instance.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the instance being sent as a message.
        /// </typeparam>
        /// <param name="instance">
        /// The instance to be sent. This parameter must not be null.
        /// </param>
        /// <param name="configureMessage">
        /// An optional action that allows configuration of the message before sending.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to abort the operation. The default is a cancellation token that is not cancelable.
        /// </param>
        /// <returns>
        /// A value task representing the asynchronous operation, with no result.
        /// </returns>
        public async ValueTask SendOneAsync<T>([NotNull] T instance, Action<SendMessage<T>>? configureMessage = default, CancellationToken cancellationToken = default)
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


        /// <summary>
        /// Asynchronously sends a collection of instances to a service bus 
        /// and yields the results of the send operations.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the instances being sent.
        /// </typeparam>
        /// <param name="instances">
        /// The collection of instances to be sent.
        /// </param>
        /// <param name="configureMessage">
        /// An optional action to configure the message before sending. 
        /// This can be used to add custom properties or modify the message content.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token to observe while waiting for the asynchronous operation to complete.
        /// </param>
        /// <returns>
        /// An asynchronous enumerable of <see cref="SendResult{T}"/> 
        /// that represents the result of each send operation.
        /// </returns>
        public async IAsyncEnumerable<SendResult<T>> SendAsync<T>([NotNull] IEnumerable<T> instances, Action<SendMessage<T>>? configureMessage = default, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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

        /// <summary>
        /// Sends a collection of instances asynchronously and returns the results.
        /// </summary>
        /// <typeparam name="T">The type of the instances being sent.</typeparam>
        /// <param name="instances">A collection of instances to be sent.</param>
        /// <param name="configureMessage">Optional action to configure the message before sending.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// An enumerable collection of <see cref="SendResult{T}"/> representing the results of the send operation.
        /// </returns>
        public IEnumerable<SendResult<T>> Send<T>([NotNull] IEnumerable<T> instances, Action<SendMessage<T>>? configureMessage = default, CancellationToken cancellationToken = default)
        {
            var task = this.PrivateSendAsync(instances, configureMessage, cancellationToken);

            task.AsTask().Wait(cancellationToken);

            return task.Result;
        }

        /// <summary>
        /// Asynchronously sends a collection of instances and returns the results of sending each instance.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the instances to be sent.
        /// </typeparam>
        /// <param name="instances">
        /// A collection of instances of type <typeparamref name="T"/> to be sent.
        /// </param>
        /// <param name="configureMessage">
        /// An optional action to configure the message before sending. Defaults to null.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to cancel the asynchronous operation, if needed. Defaults to CancellationToken.None.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask"/> that represents the asynchronous operation, containing an enumerable of <see cref="SendResult{T}"/>
        /// representing the results of sending the instances.
        /// </returns>
        private async ValueTask<IEnumerable<SendResult<T>>> PrivateSendAsync<T>([NotNull] IEnumerable<T> instances, Action<SendMessage<T>>? configureMessage = default, CancellationToken cancellationToken = default)
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

        /// <summary>
        /// Asynchronously sends a batch of messages to a service bus. The messages are created from 
        /// an enumerable collection and can be configured with an optional action before sending.
        /// </summary>
        /// <typeparam name="T">The type of the messages to be sent.</typeparam>
        /// <param name="instances">An enumerable collection of instances to be sent as messages.</param>
        /// <param name="configureMessage">An optional action to configure each message.</param>
        /// <param name="chunkSize">The maximum number of messages to include in a single batch.</param>
        /// <param name="maxSizeInBytes">The maximum size in bytes for the message batch.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation of sending the messages.</returns>
        /// <exception cref="Exception">Thrown if there is a failure to add a message to the batch.</exception>
        public async ValueTask SendBatchAsync<T>(
            [NotNull] IEnumerable<T> instances,
            Action<SendMessage<T>>? configureMessage = default,
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

        /// <summary>
        /// Asynchronously disposes of the resources used by the current instance.
        /// This method will dispose of the Service Bus sender if it has been created.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous dispose operation.
        /// The value of the task indicates completion of the DisposeAsync method.
        /// </returns>
        public async ValueTask DisposeAsync()
        {
            if (this._serviceBusSender != null)
            {
                await this._serviceBusSender.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the resources used by the object, providing an option to release both managed and unmanaged resources.
        /// This method is intended to be overridden in derived classes to provide custom disposal logic.
        /// </summary>
        /// <param name="disposing">
        /// A boolean value indicating whether the method has been called directly or indirectly
        /// by a user's code (true) or by the runtime from inside the finalizer (false).
        /// If true, the method should release both managed and unmanaged resources.
        /// If false, it should only release unmanaged resources.
        /// </param>
        /// <remarks>
        /// This method is called by the Dispose() methods and the finalizer. 
        /// Derived classes should provide implementation that manages the disposal of their own resources.
        /// </remarks>
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
                        // Handle any exceptions that occur during disposal of managed resources
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

        /// <summary>
        /// Finalizer should be overridden only if the 'Dispose(bool disposing)' method 
        /// contains code to free unmanaged resources. This finalizer is provided as 
        /// a placeholder and is not activated unless needed.
        /// </summary>
        /// <remarks>
        /// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method.
        /// </remarks>
        /// <example>
        /// <code>
        /// ~Sender()  
        /// {  
        ///     // Only call Dispose with 'disposing' set to false.  
        ///     Dispose(disposing: false);  
        /// }
        /// </code>
        /// </example>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
