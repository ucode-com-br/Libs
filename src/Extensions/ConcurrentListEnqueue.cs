using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    /// <summary>
    /// Concorrent list enqueue using tasks
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentListEnqueue<T> : IList<T>, IReadOnlyList<T>, IDisposable
    {
        private readonly List<T> _list = new();


        /// <summary>
        /// Time limit
        /// </summary>
        public TimeSpan IndexOfTimeout { get; init; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Remove
        /// </summary>
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
        /// Is read only queue
        /// </summary>
        public bool IsReadOnly => false;

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
        /// Clean queue
        /// </summary>
        public void Clear()
        {
            this._addTask.Wait();

            lock (this)
            {
                this._list.Clear();
            }
        }

        /// <summary>
        /// Verify contains item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            var result = false;
            lock (this)
            {
                result = this._list.Contains(item);
            }
            return result;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (this)
            {
                this._list.CopyTo(array, arrayIndex);
            }
        }

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


        IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        #region Dispose
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        #endregion Dispose
    }
}
