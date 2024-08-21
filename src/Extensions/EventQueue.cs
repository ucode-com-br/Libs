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
    public sealed class EventQueue<T>: IAsyncDisposable, IDisposable
    {
        public enum QueueDirection
        {
            Enqueue, Dequeue
        }

        public class QueueEvent : EventArgs
        {
            public T Item
            {
                get; set;
            }

            public QueueDirection Direction
            {
                get; set;
            }

            public int Count
            {
                get; set;
            }
        }


        public delegate void EnqueuedEventHandler(object sender, QueueEvent e);
        public event EnqueuedEventHandler EnqueuedEvent;
        public event EnqueuedEventHandler DequeuedEvent;
        public event EventHandler<Exception> ExceptionsEvent;
        private void OnEnqueued(QueueEvent e) => EnqueuedEvent?.Invoke(this, e);
        private void OnDequeued(QueueEvent e) => DequeuedEvent?.Invoke(this, e);
        private void OnException(Exception e) => ExceptionsEvent?.Invoke(this, e);


        private readonly ConcurrentQueue<T> _queue;
        private bool _DisposedValue;
        private int _count;
        private (Thread, CancellationTokenSource, Func<CancellationToken>) _backgroundDequeueThread;

        private int Increment() => Interlocked.Add(ref this._count, 1);

        private int Decrement() => Interlocked.Add(ref this._count, -1);


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


        public EventQueue(IEnumerable<T> itens): this()
        {
            Enqueue(itens, CancellationToken.None);
        }

        public EventQueue()
        {
            this._count = 0;
            this._queue = new ConcurrentQueue<T>();

            RestartBackgroundDequeue();
        }



        public Task EnqueueAsync(T item)
        {
            return Task.Run(() => { this.Enqueue(item); });
        }

        public void Enqueue(T item)
        {
            Enqueue(new T[1] { item }, CancellationToken.None);
        }

        public Task<T?> DequeueAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run<T>(() => { return this.Dequeue(); }, cancellationToken);
        }

        public T? Dequeue() => this.Dequeue(CancellationToken.None);
        

        public int Count
        {
            get => this._count;
        }


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

        public bool IsEventDequeueRunning
        {
            get => this._backgroundDequeueThread.Item1.IsAlive && !this._backgroundDequeueThread.Item3().IsCancellationRequested;
        }

        public void StopEventDequeue(TimeSpan? timeout = null)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            if (timeout.HasValue)
            {
                cancellationTokenSource.CancelAfter(timeout.Value);
            }
            
            this.StopEventDequeue(cancellationTokenSource.Token);
        }

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

        public void Wait(TimeSpan? timeout)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            if (timeout.HasValue)
            {
                cancellationTokenSource.CancelAfter(timeout.Value);
            }

            Wait(cancellationTokenSource.Token);
        }

        public void Wait(CancellationToken cancellationToken = default)
        {
            while ((this._count > 0 || !this._queue.IsEmpty) && !cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(100);
            }
        }


















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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync() => new(Task.Run(this.Dispose));
    }
}
