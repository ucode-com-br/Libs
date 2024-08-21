using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    public class ConcurrentList<T> : IList<T>, IReadOnlyList<T>, IDisposable
    {
        private readonly List<T> _list;
        private readonly System.Threading.CancellationToken _cancellationToken;
        private bool _disposedValue;

        private readonly TimeSpan _clearTimeout = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _removeTimeout = TimeSpan.FromSeconds(1);

        private readonly System.Collections.Concurrent.ConcurrentDictionary<Task, System.Threading.CancellationTokenSource> _taskAdd = new();
        private readonly System.Collections.Concurrent.ConcurrentDictionary<Task, System.Threading.CancellationTokenSource> _taskAddRange = new();
        private readonly System.Collections.Concurrent.ConcurrentDictionary<Task, System.Threading.CancellationTokenSource> _taskInsert = new();



        public delegate void AddHandler(ConcurrentList<T> sender, T item);
        public event AddHandler AddEvent;
        public virtual void OnAdd(T item) => AddEvent?.Invoke(this, item);

        public delegate void AddRangeHandler(ConcurrentList<T> sender, IEnumerable<T> collection);
        public event AddRangeHandler AddRangeEvent;
        public virtual void OnAddRange(IEnumerable<T> collection) => AddRangeEvent?.Invoke(this, collection);

        public delegate void RemoveHandler(ConcurrentList<T> sender, T item);
        public event RemoveHandler RemoveEvent;
        public virtual void OnRemove(T item) => RemoveEvent?.Invoke(this, item);

        public delegate void RemoveAtHandler(ConcurrentList<T> sender, int index);
        public event RemoveAtHandler RemoveAtEvent;
        public virtual void OnRemoveAt(int index) => RemoveAtEvent?.Invoke(this, index);

        public delegate void InsertHandler(ConcurrentList<T> sender, int index, T item);
        public event InsertHandler InsertEvent;
        public virtual void OnInsert(int index, T item) => InsertEvent?.Invoke(this, index, item);

        public delegate void ClearHandler(ConcurrentList<T> sender);
        public event ClearHandler ClearEvent;
        public virtual void OnClear() => ClearEvent?.Invoke(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConcurrentList(IEnumerable<T> collection, System.Threading.CancellationToken cancellationToken = default) : this(new List<T>(collection), cancellationToken)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConcurrentList(int capacity, System.Threading.CancellationToken cancellationToken = default) : this(new List<T>(capacity), cancellationToken)
        {

        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConcurrentList(System.Threading.CancellationToken cancellationToken = default) : this(new List<T>(), cancellationToken)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConcurrentList(List<T> list, System.Threading.CancellationToken cancellationToken = default)
        {
            this._list = list;
            this._cancellationToken = cancellationToken;
            this._cancellationToken.Register(this.Canceled);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Canceled()
        {
            foreach (var task in this._taskAdd.Concat(this._taskAddRange).Concat(this._taskInsert))
            {
                task.Value.Cancel();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfDisposed()
        {
            if (this._disposedValue)
            {
                throw new ObjectDisposedException(nameof(ConcurrentList<T>));
            }
        }

        public T this[int index]
        {
            get
            {
                this.ThrowIfDisposed();

                var result = default(T);

                lock (this)
                {
                    result = this._list[index];
                }

                return result;
            }
            set => this.InsertForget(index, value);
        }

        public int Count
        {
            get
            {
                this.ThrowIfDisposed();

                var result = default(int);

                lock (this)
                {
                    result = this._list.Count;
                }

                return result;
            }
        }

        public bool IsReadOnly => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task AddRangeAsync(IEnumerable<T> collection, System.Threading.CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();

            await Task.Run(() => this.AddRange(collection), cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRangeForget(IEnumerable<T> collection)
        {
            this.ThrowIfDisposed();

            var cancellationTokenSource = new System.Threading.CancellationTokenSource();

            var task = this.AddRangeAsync(collection, cancellationTokenSource.Token).ContinueWith((task) => this._taskAdd.Remove(task, out var cancellationToken));

            while (!this._taskAddRange.TryAdd(task, cancellationTokenSource))
            {
                if (task.IsCompleted)
                {
                    break;
                }

                System.Threading.Thread.Sleep(1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(IEnumerable<T> collection)
        {
            this.ThrowIfDisposed();

            lock (this)
            {
                this._list.AddRange(collection);

                this.OnAddRange(collection);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task AddAsync(T item, System.Threading.CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();

            await Task.Run(() => this.Add(item), cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddForget(T item)
        {
            this.ThrowIfDisposed();

            var cancellationTokenSource = new System.Threading.CancellationTokenSource();

            var task = this.AddAsync(item, cancellationTokenSource.Token).ContinueWith((task) => this._taskAdd.Remove(task, out var cancellationToken));

            while (!this._taskAdd.TryAdd(task, cancellationTokenSource))
            {
                if (task.IsCompleted)
                {
                    break;
                }

                System.Threading.Thread.Sleep(1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            this.ThrowIfDisposed();

            lock (this)
            {
                this._list.Add(item);
                this.OnAdd(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => this.Clear(this._clearTimeout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(TimeSpan timeout)
        {
            this.ThrowIfDisposed();

            var timer = System.Diagnostics.Stopwatch.StartNew();
            while (!this._taskAdd.IsEmpty || !this._taskInsert.IsEmpty || !this._taskAddRange.IsEmpty)
            {
                if (timer.Elapsed > timeout)
                {
                    break;
                }

                System.Threading.Thread.Sleep(1);
            }
            timer.Stop();

            lock (this)
            {
                this._list.Clear();
                this.OnClear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            this.ThrowIfDisposed();

            var result = false;
            lock (this)
            {
                result = this._list.Contains(item);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.ThrowIfDisposed();

            lock (this)
            {
                this._list.CopyTo(array, arrayIndex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
        {
            this.ThrowIfDisposed();

            lock (this)
            {
                for (var i = 0; i < this._list.Count; i++)
                {
                    yield return this._list[i];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item)
        {
            this.ThrowIfDisposed();

            var result = default(int);

            lock (this)
            {
                result = this._list.IndexOf(item);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task InsertAsync(int index, T item, System.Threading.CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();

            await Task.Run(() => this.Insert(index, item), cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertForget(int index, T item)
        {
            this.ThrowIfDisposed();

            var cancellationTokenSource = new System.Threading.CancellationTokenSource();

            var task = this.InsertAsync(index, item, cancellationTokenSource.Token).ContinueWith((task) => this._taskInsert.Remove(task, out var cancellationToken));

            while (!this._taskInsert.TryAdd(task, cancellationTokenSource))
            {
                if (task.IsCompleted)
                {
                    break;
                }

                System.Threading.Thread.Sleep(1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, T item)
        {
            this.ThrowIfDisposed();

            lock (this)
            {
                this._list.Insert(index, item);
                this.OnInsert(index, item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item) => this.Remove(item, this._removeTimeout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item, TimeSpan timeout)
        {
            this.ThrowIfDisposed();

            var result = false;

            var timer = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                lock (this)
                {
                    result = this._list.Remove(item);
                    this.OnRemove(item);
                }

                if (timer.Elapsed > timeout || result)
                {
                    break;
                }

                System.Threading.Thread.Sleep(1);

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            } while (!result && !this._taskAdd.IsEmpty && !this._taskInsert.IsEmpty);
            timer.Stop();

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index) => this.RemoveAt(index, this._removeTimeout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, TimeSpan timeout)
        {
            this.ThrowIfDisposed();

            var timer = System.Diagnostics.Stopwatch.StartNew();
            while (!this._taskAdd.IsEmpty || !this._taskInsert.IsEmpty)
            {
                if (timer.Elapsed > timeout)
                {
                    break;
                }

                System.Threading.Thread.Sleep(1);
            }
            timer.Stop();

            lock (this)
            {
                this._list.RemoveAt(index);
                this.OnRemoveAt(index);
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    ThrowIfDisposed();

        //    return this.GetEnumerator();
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IReadOnlyList<T> ReadOnlyList()
        {
            this.ThrowIfDisposed();

            lock (this)
            {
                return this._list.AsReadOnly();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<IReadOnlyList<T>> ToListAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();

            return await Task.Run(this.ReadOnlyList, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            this.ThrowIfDisposed();
            return this.GetEnumerator();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator List<T>(ConcurrentList<T> source) => source.ToList();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ConcurrentList<T>(List<T> source) => new(source);

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {

                    foreach (var task in this._taskAdd.Concat(this._taskAddRange).Concat(this._taskInsert))
                    {
                        if (!task.Value.IsCancellationRequested)
                        {
                            task.Value.Cancel();
                        }

                        task.Value.Dispose();
                        task.Key.Dispose();
                    }
                    this._taskAdd.Clear();
                    this._taskAddRange.Clear();
                    this._taskInsert.Clear();


                    try
                    {
                        this._list.Clear();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

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
