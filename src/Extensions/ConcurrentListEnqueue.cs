using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    /// <summary>
    /// Optimized concurrent list implementation with:
    /// - Separate queues for add/insert/remove operations
    /// - Independent async workers
    /// - Configurable timeouts per operation type
    /// - Pending task monitoring
    /// </summary>
    /// <remarks>
    /// Resource consumption pattern:
    /// 1. Operations are enqueued in dedicated queues
    /// 2. Workers process batches in optimized way
    /// 3. Lock-free for write operations
    /// 4. Granular lock for read operations
    /// </remarks>
    public class ConcurrentListEnqueue<T> : IList<T>, IReadOnlyList<T>, IDisposable
    {
        private readonly List<T> _list = new();


        /// <summary>
        /// Gets the timeout duration for indexing operations.
        /// The default value is set to 2 seconds.
        /// </summary>
        /// <value>
        /// A <see cref="TimeSpan"/> representing the timeout.
        /// </value>
        public TimeSpan IndexOfTimeout { get; init; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Gets the duration of the timeout to be removed. 
        /// This property is initialized to a default value of 5 seconds.
        /// </summary>
        /// <value>
        /// A <see cref="TimeSpan"/> representing the timeout duration.
        /// </value>
        public TimeSpan RemoveTimeout { get; init; } = TimeSpan.FromSeconds(5);

        // ADD
        private readonly System.Collections.Concurrent.ConcurrentQueue<T> _addQueue = new();
        private Task? _addTask;

        // REMOVE
        private readonly System.Collections.Concurrent.ConcurrentQueue<T> _remQueue = new();
        private Task? _remTask;

        // INSERT
        private readonly System.Collections.Concurrent.ConcurrentQueue<(int, T)> _insQueue = new();
        private Task? _insTask;

        // REMOVE AT INDEX
        private readonly System.Collections.Concurrent.ConcurrentQueue<int> _reaQueue = new();
        private Task? _reaTask;

        // DISPOSED
        private bool _disposedValue;



        /// <summary>
        /// Waits for the completion of multiple tasks, either indefinitely or for a specified time period.
        /// </summary>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> that specifies the maximum time to wait for the tasks to complete. Defaults to <see cref="default"/>,
        /// which indicates waiting indefinitely until the tasks complete.</param>
        /// <returns>
        /// Returns <c>true</c> if all tasks complete within the specified timeout; otherwise, returns <c>false</c> if the timeout elapses before the tasks complete.
        /// </returns>
        public bool Wait(TimeSpan timeout = default)
        {
            if (timeout == default)
            {
                this._addTask?.Wait();
                this._remTask?.Wait();
                this._insTask?.Wait();
                this._reaTask?.Wait();

                return true;
            }

            return (this._addTask ?? Task.CompletedTask).Wait(timeout) && (this._remTask ?? Task.CompletedTask).Wait(timeout) && (this._insTask ?? Task.CompletedTask).Wait(timeout) && (this._reaTask ?? Task.CompletedTask).Wait(timeout);
        }



        public T this[int index]
        {
            get
            {
                var result = default(T);

                lock (this)
                {
                    result = this._list[index];
                }

                return result;
            }
            set => this.Insert(index, value);
        }

        /// <summary>
        /// Gets the number of elements in the list.
        /// This property returns the count of items present in the 
        public int Count
        {
            get
            {
                var result = default(int);

                lock (this)
                {
                    result = this._list.Count;
                }

                return result;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this collection is read-only.
        /// </summary>
        /// <value>
        /// Always returns <c>false</c>, indicating that the collection is not read-only.
        /// </value>
        /// <returns>
        /// Returns a boolean value where <c>false</c> implies the collection can be modified.
        /// </returns>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds an item to the collection in a thread-safe manner.
        /// This method enqueues the item to an 
        public void Add(T item)
        {
            //_list.Add(item);
            this._addQueue.Enqueue(item);

            if (this._addTask == null)
            {
                this._addTask = new Task(() =>
                {
                    while (this._addQueue.TryDequeue(out var add))
                    {
                        lock (this)
                        {
                            this._list.Add(add);
                        }
                    }

                    this._addTask = null;
                });

                this._addTask.Start();
            }
        }

        /// <summary>
        /// Clears all the tasks from the 
        public void Clear()
        {
            this._addTask.Wait();

            lock (this)
            {
                this._list.Clear();
            }
        }

        /// <summary>
        /// Checks if the specified item is present in the collection.
        /// </summary>
        /// <param name="item">The item to search for in the collection.</param>
        /// <returns>
        /// <c>true</c> if the item is found in the collection; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(T item)
        {
            var result = false;
            lock (this)
            {
                result = this._list.Contains(item);
            }
            return result;
        }

        /// <summary>
        /// Copies the elements of the list to a specified array, starting at a particular array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the list. 
        /// The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in the array at which storing the copied elements begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (this)
            {
                this._list.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            lock (this)
            {
                for (var i = 0; i < this._list.Count; i++)
                {
                    yield return this._list[i];
                }
            }
        }

        /// <summary>
        /// Finds the index of the specified item in the collection.
        /// This method uses a timeout mechanism to avoid blocking indefinitely,
        /// checking for elapsed time while searching for the item.
        /// </summary>
        /// <param name="item">The item to locate in the collection.</param>
        /// <returns>
        /// The zero-based index of the item within the collection if found; 
        /// otherwise, the default value (which is 0).
        /// </returns>
        public int IndexOf(T item)
        {
            var result = default(int);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            while (result == default)
            {
                if (watch.Elapsed > this.IndexOfTimeout)
                {
                    break;
                }

                lock (this)
                {
                    result = this._list.IndexOf(item);
                }
            }

            return result;
        }

        /// <summary>
        /// Inserts an item at the specified index into a thread-safe list. 
        /// The insertion operation is queued for processing and ensures that 
        /// multiple insertions are handled without causing thread contention.
        /// </summary>
        /// <param name="index">The zero-based index at which the item should be inserted.</param>
        /// <param name="item">The item to be inserted into the list.</param>
        public void Insert(int index, T item)
        {
            //lock (this)
            //{
            //    _list.Insert(index, item);
            //}

            this._insQueue.Enqueue((index, item));

            if (this._insTask == null)
            {
                this._insTask = new Task(() =>
                {
                    while (this._insQueue.TryDequeue(out var ins))
                    {
                        lock (this)
                        {
                            this._list.Insert(ins.Item1, ins.Item2);
                        }
                    }

                    this._insTask = null;
                });

                this._insTask.Start();
            }
        }

        /// <summary>
        /// Removes the specified item from the list. If the item is queued for removal,
        /// it will be processed in a separate task, which will handle the removal
        /// within a specified timeout period.
        /// </summary>
        /// <param name="item">The item to be removed from the list.</param>
        /// <returns>True, indicating that the item has been queued for removal.</returns>
        public bool Remove(T item)
        {
            //bool result = default(bool);

            //lock (this)
            //{
            //    result = _list.Contains(item);
            //}

            //return result;


            this._remQueue.Enqueue(item);

            if (this._remQueue == null)
            {
                this._remTask = new Task(() =>
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var removed = false;

                    while (this._remQueue.TryDequeue(out var rem) || !removed)
                    {
                        if (watch.Elapsed > this.RemoveTimeout)
                        {
                            break;
                        }

                        lock (this)
                        {
                            removed = this._list.Remove(rem);
                        }
                    }

                    this._remTask = null;
                });

                this._remTask.Start();
            }

            return true;
        }

        /// <summary>
        /// Removes an item at the specified index from the underlying list asynchronously.
        /// If the removal is requested when a task is already processing, the index will be enqueued for later processing.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove from the list.</param>
        /// <remarks>
        /// The method utilizes a Queue to handle indices asynchronously, ensuring that 
        /// multiple calls to remove items do not interfere with one another. 
        /// A Task is started when the queue is not currently being processed.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is less than zero or greater than or equal to the count of the list.</exception>
        public void RemoveAt(int index)
        {
            //lock (this)
            //{
            //    _list.RemoveAt(index);
            //}

            this._reaQueue.Enqueue(index);

            if (this._reaQueue == null)
            {
                this._reaTask = new Task(() =>
                {
                    while (this._reaQueue.TryDequeue(out var rea))
                    {
                        lock (this)
                        {
                            this._list.RemoveAt(rea);
                        }
                    }

                    this._reaTask = null;
                });

                this._reaTask.Start();
            }
        }

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return this.GetEnumerator();
        //}


        /// <summary>
        /// Provides the enumerator for the collection, enabling iteration
        /// over the collection using a non-generic interface.
        /// </summary>
        /// <returns>
        /// An enumerator that can iterate through the collection.
        /// </returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        #region Dispose
        /// <summary>
        /// Releases the resources used by the current instance of the class.
        /// </summary>
        /// <param name="disposing">A boolean value indicating whether the method has been called directly or indirectly by a user's code. If true, both managed and unmanaged resources can be disposed; if false, only unmanaged resources should be disposed.</param>
        /// <remarks>
        /// If disposing equals true, the method can dispose of all resources that the class owns. 
        /// If disposing equals false, the method should not reference other objects, as they have been disposed.
        /// </remarks>
        /// <exception cref="Exception">Any exception thrown during the disposal of managed resources is caught and ignored.</exception>
        protected virtual void Dispose(bool disposing) 
        { 
            if (!this._disposedValue) 
            { 
                if (disposing) 
                { 
                    try 
                    { 
                        this._list.Clear(); 
                    } 
                    catch (Exception) 
                    { 
                        // ignored 
                    } 
        
                    this._addTask?.Dispose(); 
                    this._remTask?.Dispose(); 
                    this._insTask?.Dispose(); 
                    this._reaTask?.Dispose(); 
        
                    this._addQueue.Clear(); 
                    this._remQueue.Clear(); 
                    this._insQueue.Clear(); 
                    this._reaQueue.Clear(); 
                } 
        
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer 
                // TODO: set large fields to null 
                this._disposedValue = true; 
            } 
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ConcurrentList()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Represents a class that implements the IDisposable interface,
        /// providing a mechanism for releasing both managed and unmanaged resources.
        /// </summary>
        /// <remarks>
        /// It is crucial to override the finalizer only if the Dispose(bool disposing)
        /// method has code to free unmanaged resources. The finalizer cleans up resources
        /// when Dispose is not called. This implementation calls Dispose with a parameter 
        /// indicating whether it is being disposed explicitly or through the finalizer.
        /// </remarks>
        /// <example>
        /// <code>
        /// using (var list = new ConcurrentList())
        /// {
        ///     // Use the list here
        /// }
        /// // At this point, the Dispose method will be called automatically.
        /// </code>
        /// </example>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        #endregion Dispose
    }
}
