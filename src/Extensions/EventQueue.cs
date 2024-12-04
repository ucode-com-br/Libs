using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    /// <summary>
    /// Represents a thread-safe queue for handling events asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of events that the queue will handle.</typeparam>
    /// <remarks>
    /// This class implements both <see cref="IAsyncDisposable"/> and <see cref="IDisposable"/> 
    /// interfaces, allowing for asynchronous as well as synchronous resource management.
    /// It ensures that events can be enqueued and dequeued in a thread-safe manner, making 
    /// it suitable for use in concurrent applications.
    /// </remarks>
    public sealed class EventQueue<T>: IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Represents the direction of operations in a queue.
        /// </summary>
        /// <remarks>
        /// This enumeration provides options for either adding an item to the queue (Enqueue)
        /// or removing an item from the queue (Dequeue).
        /// </remarks>
        public enum QueueDirection
        {
            Enqueue, Dequeue
        }

        /// <summary>
        /// Represents the event arguments for queue-related events.
        /// Inherits from the <see cref="EventArgs"/> class to provide event data
        /// for handling events in a queue system.
        /// </summary>
        public class QueueEvent : EventArgs
        {
            /// <summary>
            /// Represents a generic item property that can get or set a value of type T.
            /// </summary>
            /// <typeparam name="T">
            /// The type of the item. This can be any data type.
            /// </typeparam>
            /// <value>
            /// The current value of the item.
            /// </value>
            /// <example>
            /// var instance = new ClassName();
            /// instance.Item = new YourType(); // Setting the item
            /// YourType value = instance.Item; // Getting the item
            /// </example>
            public T Item
            {
                get; set;
            }

            /// <summary>
            /// Represents the direction of the queue.
            /// This property defines whether the queue operates in a first-in-first-out (FIFO) 
            /// or last-in-first-out (LIFO) manner, allowing for flexible queue management and processing.
            /// </summary>
            /// <value>
            /// A <see cref="QueueDirection"/> value indicating the current direction of the queue.
            /// </value>
            public QueueDirection Direction
            {
                get; set;
            }

            /// <summary>
            /// Gets or sets the count value.
            /// </summary>
            /// <value>
            /// An int representing the count. The default value is 0.
            /// </value>
            public int Count
            {
                get; set;
            }
        }


        /// <summary>
        /// Represents the method that will handle event notifications 
        /// related to queue events.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An instance of <see cref="QueueEvent"/> containing the event data.</param>
        public delegate void EnqueuedEventHandler(object sender, QueueEvent e);
        public event EnqueuedEventHandler EnqueuedEvent;
        public event EnqueuedEventHandler DequeuedEvent;
        public event EventHandler<Exception> ExceptionsEvent;
        /// <summary>
        /// Invoked when an event is enqueued.
        /// </summary>
        /// <param name="e">The queue event that has been enqueued.</param>
        /// <remarks>
        /// This method uses a null-conditional operator to invoke the EnqueuedEvent
        /// if it is not null, passing in the current instance and the enqueued event instance.
        /// </remarks>
        private void OnEnqueued(QueueEvent e) => EnqueuedEvent?.Invoke(this, e);
        /// <summary>
        /// Invoked when an item is dequeued from the queue.
        /// </summary>
        /// <param name="e">An instance of <see cref="QueueEvent"/> containing the event data associated with the dequeue operation.</param>
        /// <remarks>
        /// This method handles the event signaling by invoking the <c>DequeuedEvent</c> delegate,
        /// passing the current instance and the event data to any subscribed event handlers.
        /// </remarks>
        private void OnDequeued(QueueEvent e) => DequeuedEvent?.Invoke(this, e);
        /// <summary>
        /// Invoked when an exception occurs.
        /// </summary>
        /// <param name="e">The exception that occurred.</param>
        /// <remarks>
        /// This method raises the <see cref="ExceptionsEvent"/> event, 
        /// allowing subscribers to handle the exception as necessary.
        /// </remarks>
        private void OnException(Exception e) => ExceptionsEvent?.Invoke(this, e);


        private readonly ConcurrentQueue<T> _queue;
        private bool _DisposedValue;
        private int _count;
        private (Thread, CancellationTokenSource, Func<CancellationToken>) _backgroundDequeueThread;

        /// <summary>
        /// Increments the value of the <c>_count</c> field by one in a thread-safe manner.
        /// </summary>
        /// <returns>
        /// The new value of the <c>_count</c> field after the increment operation.
        /// </returns>
        /// <remarks>
        /// This method utilizes <see cref="System.Threading.Interlocked.Add"/> 
        /// to ensure that the increment operation is atomic, preventing race conditions 
        /// in multi-threaded environments.
        /// </remarks>
        private int Increment() => Interlocked.Add(ref this._count, 1);

        /// <summary>
        /// Decrements the value of the <c>_count</c> field atomically by one.
        /// </summary>
        /// <returns>
        /// The new value of the <c>_count</c> field after the decrement operation.
        /// </returns>
        /// <remarks>
        /// This method uses <see cref="Interlocked.Add"/> to ensure that the 
        /// decrement operation is thread-safe, meaning that it can be safely 
        /// called from multiple threads simultaneously without encountering race 
        /// conditions.
        /// </remarks>
        private int Decrement() => Interlocked.Add(ref this._count, -1);


        /// <summary>
        /// Enqueues a collection of items into the queue.
        /// </summary>
        /// <param name="itens">The collection of items to be enqueued.</param>
        /// <param name="cancellationToken">A cancellation token to observe while enqueuing the items. Defaults to <see cref="CancellationToken.None"/>.</param>
        /// <exception cref="ObjectDisposedException">Thrown when the method is called on a disposed instance.</exception>
        /// <exception cref="Exception">Thrown when an exception occurs during the enqueuing process.</exception>
        private void Enqueue(IEnumerable<T> itens, CancellationToken cancellationToken = default)
        {
            if(this._DisposedValue)
                throw new ObjectDisposedException($"Disposed request.");

            foreach (var item in itens)
            {
                try
                {
                    this._queue.Enqueue(item);
                    this.OnEnqueued(new QueueEvent() { Count = this.Increment(), Direction = QueueDirection.Enqueue, Item = item });
                }
                catch (Exception ex)
                {
                    this.OnException(new Exception($"Exception in \"{nameof(Enqueue)}\" method, see inner for details.", ex));
                }

                if(cancellationToken.IsCancellationRequested)
                    break;
            }
        }

        /// <summary>
        /// Attempts to dequeue an item from the queue, providing cancellation support.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to signal cancellation of the operation.</param>
        /// <returns>
        /// The dequeued item of type T, or null if the queue was empty
        /// or the operation was canceled before an item could be dequeued.
        /// </returns>
        private T? Dequeue(CancellationToken cancellationToken = default)
        {
            T? result = default;

            while (!this._queue.IsEmpty)
            {
                try
                {
                    if (this._queue.TryDequeue(out result))
                    {
                        this.OnEnqueued(new QueueEvent() { Count = this.Decrement(), Direction = QueueDirection.Dequeue, Item = result });
                        break;
                    }
                }
                catch (Exception ex)
                {
                    this.OnException(new Exception($"Exception in \"{nameof(Dequeue)}\" method, see inner for details.", ex));
                }

                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            return result;
        }

        /// <summary>
        /// Continuously processes items from the queue in the background.
        /// The method checks for cancellation requests, dequeues items if available,
        /// and pauses when there are no items to process.
        /// </summary>
        /// <remarks>
        /// This method runs in a background thread and checks for cancellation
        /// in each iteration of the loop. If there are items in the queue, it dequeues them
        /// until the queue is empty or a cancellation request is received.
        /// If the queue is empty, the method sleeps for a short duration
        /// to avoid busy waiting.
        /// </remarks>
        /// <exception cref="ThreadInterruptedException">
        /// Thrown if the thread is interrupted while sleeping.
        /// </exception>
        private void BackgroundDequeue()
        {
            while (!this._backgroundDequeueThread.Item3().IsCancellationRequested)
            {
                if (this._count > 0)
                {
                    _ = this.Dequeue();
                }
                else if (!this._queue.IsEmpty)
                {
                    _ = this.Dequeue();
                }
                else
                {
                    if (this._backgroundDequeueThread.Item3().IsCancellationRequested)
                        break;

                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// Restarts the background dequeue process by creating a new thread
        /// that executes the BackgroundDequeue method. This thread is set as 
        /// a background thread with a below-normal priority and uses invariant 
        /// culture for its current culture settings.
        /// </summary>
        /// <remarks>
        /// The method initializes a new thread for dequeuing background tasks,
        /// ensuring that the previous thread, if any, is replaced. The thread 
        /// is configured to operate in the background so it does not prevent 
        /// the application from shutting down. It also handles thread culture 
        /// settings to avoid issues with localization in the processing.
        /// </remarks>
        /// <exception cref="ThreadStateException">
        /// Thrown when the thread is in a state that does not allow it to be 
        /// started.
        /// </exception>
        private void RestartBackgroundDequeue()
        {
            this._backgroundDequeueThread = (new Thread(this.BackgroundDequeue)
            {
                IsBackground = true,
                Name = nameof(BackgroundDequeue),
                Priority = ThreadPriority.BelowNormal,
                CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture,
                CurrentCulture = System.Globalization.CultureInfo.InvariantCulture
            }, new CancellationTokenSource(), () => this._backgroundDequeueThread.Item2.Token);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="EventQueue{T}"/> class
        /// with a specified collection of items.
        /// </summary>
        /// <param name="itens">
        /// An <see cref="IEnumerable{T}"/> containing the items to enqueue in the event queue.
        /// </param>
        /// <remarks>
        /// This constructor calls the parameterless constructor and then enqueues the provided items 
        /// using the <see cref="Enqueue(IEnumerable{T}, CancellationToken)"/> method with a default cancellation token.
        /// </remarks>
        /// <returns>
        /// A new instance of the <see cref="EventQueue{T}"/> class.
        /// </returns>
        public EventQueue(IEnumerable<T> itens): this()
        {
            Enqueue(itens, CancellationToken.None);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventQueue{T}"/> class.
        /// This constructor sets the initial count to zero and creates a new concurrent queue for storing events.
        /// It also starts the background dequeue process to handle events asynchronously.
        /// </summary>
        public EventQueue()
        {
            this._count = 0;
            this._queue = new ConcurrentQueue<T>();

            RestartBackgroundDequeue();
        }



        /// <summary>
        /// Asynchronously enqueues an item of type T.
        /// </summary>
        /// <param name="item">The item to be enqueued.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public Task EnqueueAsync(T item)
        {
            return Task.Run(() => { this.Enqueue(item); });
        }

        /// <summary>
        /// Adds an item to the end of the queue.
        /// </summary>
        /// <param name="item">The item to be added to the queue.</param>
        public void Enqueue(T item)
        {
            Enqueue(new T[1] { item }, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously dequeues an item from the queue.
        /// </summary>
        /// <typeparam name="T">The type of the item to be dequeued.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous dequeue operation. 
        /// The value of the task is the item dequeued from the queue, or null if the queue is empty.
        /// </returns>
        public Task<T?> DequeueAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run<T>(() => { return this.Dequeue(); }, cancellationToken);
        }

        /// <summary>
        /// Dequeues an item from the queue.
        /// </summary>
        /// <returns>
        /// The item that has been dequeued, or null if the queue is empty.
        /// </returns>
        /// <remarks>
        /// This method does not take any cancellation token parameters and defaults to using a 
        /// <see cref="CancellationToken"/> with no cancellation.
        /// </remarks>
        public T? Dequeue() => this.Dequeue(CancellationToken.None);
        

        /// <summary>
        /// Gets the current count value.
        /// </summary>
        /// <value>
        /// An integer representing the current count.
        /// </value>
        public int Count
        {
            get => this._count;
        }


        /// <summary>
        /// Starts the event dequeue process. 
        /// Checks if the dequeued event is properly set and manages the state of the background dequeue thread.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the <see cref="DequeuedEvent"/> is null, indicating that an event needs to be attached before dispatching.
        /// </exception>
        /// <exception cref="ThreadStateException">
        /// Thrown when the background dequeue thread is currently running or alive when an attempt is made to start the event dequeue process.
        /// </exception>
        public void StartEventDequeue()
        {
            if (this.DequeuedEvent == null)
                throw new InvalidOperationException("DequeuedEvent is null for dispatch, try attach event.");

            if (this._backgroundDequeueThread.Item1.ThreadState == ThreadState.Running || this._backgroundDequeueThread.Item1.IsAlive)
                throw new ThreadStateException("Background thread for dequeue is running or alive.");

            if (this._backgroundDequeueThread.Item1.ThreadState == ThreadState.Stopped || this._backgroundDequeueThread.Item1.ThreadState == ThreadState.Aborted ||
                this._backgroundDequeueThread.Item1.ThreadState == ThreadState.StopRequested || this._backgroundDequeueThread.Item1.ThreadState == ThreadState.AbortRequested)
                this.RestartBackgroundDequeue();

            if (this._backgroundDequeueThread.Item1.IsAlive)
                

            if (this._backgroundDequeueThread.Item1.ThreadState == ThreadState.Unstarted)
                this._backgroundDequeueThread.Item1.Start();
        }

        /// <summary>
        /// Gets a value indicating whether the event dequeue process is currently running.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the background dequeue thread is alive and the cancellation is not requested; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEventDequeueRunning
        {
            get => this._backgroundDequeueThread.Item1.IsAlive && !this._backgroundDequeueThread.Item3().IsCancellationRequested;
        }

        /// <summary>
        /// Stops the event dequeue process, allowing an optional timeout for cancellation.
        /// </summary>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> that specifies the duration after which the operation should be canceled.</param>
        /// <remarks>
        /// If a timeout value is provided, the method will cancel the operation after the specified duration.
        /// </remarks>
        public void StopEventDequeue(TimeSpan? timeout = null)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            if (timeout.HasValue)
            {
                cancellationTokenSource.CancelAfter(timeout.Value);
            }
            
            this.StopEventDequeue(cancellationTokenSource.Token);
        }

        /// <summary>
        /// Stops the event dequeue process, ensuring that the background thread responsible
        /// for dequeuing events is properly terminated.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to signal that the operation should be canceled.
        /// </param>
        /// <exception cref="ThreadStateException">
        /// Thrown when the background dequeue thread is not currently running.
        /// </exception>
        /// <remarks>
        /// This method checks if the event dequeue process is running. If it is not running,
        /// a <see cref="ThreadStateException"/> is thrown. If the background dequeue thread has not 
        /// already been canceled, a cancellation is issued to stop the operations. Afterwards,
        /// the method enters a loop where it will sleep for a specified duration until the
        /// provided <paramref name="cancellationToken"/> signals that the operation is canceled.
        /// </remarks>
        public void StopEventDequeue(CancellationToken cancellationToken)
        {
            if (!this.IsEventDequeueRunning)
                throw new ThreadStateException("Background thread for dequeue is not running.");

            if (!this._backgroundDequeueThread.Item3().IsCancellationRequested)
            {
                this._backgroundDequeueThread.Item2.Cancel();
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Waits for a specified amount of time, or until cancellation is requested.
        /// </summary>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> that specifies the time to wait before canceling the operation.</param>
        /// <remarks>
        /// If the <paramref name="timeout"/> parameter is provided, the method will automatically cancel 
        /// waiting after the specified time has elapsed. If no timeout is specified, the wait will continue 
        /// indefinitely until a cancellation is requested.
        /// </remarks>
        public void Wait(TimeSpan? timeout)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            if (timeout.HasValue)
            {
                cancellationTokenSource.CancelAfter(timeout.Value);
            }

            Wait(cancellationTokenSource.Token);
        }

        /// <summary>
        /// Waits until the count is zero and the queue is empty or until the provided 
        /// cancellation token is canceled.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to signal cancellation 
        /// of the wait operation. Default value is <see cref="CancellationToken.None"/>.
        /// </param>
        public void Wait(CancellationToken cancellationToken = default)
        {
            while ((this._count > 0 || !this._queue.IsEmpty) && !cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(100);
            }
        }


















        /// <summary>
        /// Releases the resources used by the current instance of the class.
        /// This method is called by the Dispose method and by the finalizer. 
        /// It accepts a boolean parameter that indicates whether to release both managed and unmanaged resources, 
        /// or only unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// A boolean value that specifies whether the method is being called 
        /// to release both managed and unmanaged resources (true), 
        /// or just unmanaged resources (false).
        /// </param>
        protected void Dispose(bool disposing) 
        { 
            if (!this._DisposedValue) 
            { 
                if (disposing) 
                { 
                    // TODO: dispose managed state (managed objects) 
                } 
        
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer 
                // TODO: set large fields to null 
                this._DisposedValue = true; 
            } 
        }


        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~EventQueue()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Finalizer for the <see cref="EventQueue"/> class. 
        /// This finalizer is responsible for cleaning up unmanaged resources 
        /// when the object is no longer needed, only if the <see cref="Dispose(bool)"/> 
        /// method has code to free those resources.
        /// </summary>
        /// <remarks>
        /// It is a good practice not to change this code.
        /// Cleanup code should be placed in the <see cref="Dispose(bool)"/> method.
        /// </remarks>
        /// <returns>
        /// An instance of <see cref="EventQueue"/> class.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously disposes of the resources used by this instance.
        /// This method starts the disposal on a separate task, allowing it to be awaited
        /// without blocking the calling thread.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public ValueTask DisposeAsync() => new(Task.Run(this.Dispose));
    }
}
