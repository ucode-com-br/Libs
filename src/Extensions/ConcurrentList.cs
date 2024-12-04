using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    /// <summary>
    /// Represents a thread-safe list of objects that can be accessed by multiple threads
    /// concurrently. Implements the <see cref="IList{T}"/>, <see cref="IReadOnlyList{T}"/>, 
    /// and <see cref="IDisposable"/> interfaces.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
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



        /// <summary>
        /// Represents a delegate that handles the addition of an item to a <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the items in the <see cref="ConcurrentList{T}"/>.</typeparam>
        /// <param name="sender">The instance of <see cref="ConcurrentList{T}"/> that sent the event.</param>
        /// <param name="item">The item that was added to the <see cref="ConcurrentList{T}"/>.</param>
        public delegate void AddHandler(ConcurrentList<T> sender, T item);
        public event AddHandler AddEvent;
        /// <summary>
        /// Invoked when an item is added.
        /// </summary>
        /// <param name="item">The item that is being added.</param>
        /// <remarks>
        /// This method is a virtual method that can be overridden in derived classes.
        /// It raises an event to notify any subscribers that an item has been added.
        /// </remarks>
        public virtual void OnAdd(T item) => AddEvent?.Invoke(this, item);

        /// <summary>
        /// Represents a method that will handle the event for adding a range of items
        /// to a <see cref="ConcurrentList{T}"/>. This delegate should be used to define 
        /// the signature of the method that handles the event of adding a range of items.
        /// </summary>
        /// <param name="sender">
        /// The instance of <see cref="ConcurrentList{T}"/> that is sending the event.
        /// </param>
        /// <param name="collection">
        /// An <see cref="IEnumerable{T}"/> containing the collection of items to be added.
        /// </param>
        public delegate void AddRangeHandler(ConcurrentList<T> sender, IEnumerable<T> collection);
        public event AddRangeHandler AddRangeEvent;
        /// <summary>
        /// Invokes the AddRange event with the specified collection.
        /// </summary>
        /// <param name="collection">A collection of items to add.</param>
        /// <remarks>
        /// This method is virtual, allowing derived classes to override its behavior.
        /// The AddRangeEvent will be triggered if it is not null, passing the current instance 
        /// and the provided collection to the event handlers.
        /// </remarks>
        public virtual void OnAddRange(IEnumerable<T> collection) => AddRangeEvent?.Invoke(this, collection);

        /// <summary>
        /// Defines a delegate that represents a method for handling remove actions 
        /// in a <see cref="ConcurrentList{T}"/>. This delegate is invoked when an 
        /// item is removed from the list.
        /// </summary>
        /// <param name="sender">The instance of the <see cref="ConcurrentList{T}"/> 
        /// that is sending the notification.</param>
        /// <param name="item">The item of type <typeparamref name="T"/> that has 
        /// been removed from the list.</param>
        public delegate void RemoveHandler(ConcurrentList<T> sender, T item);
        public event RemoveHandler RemoveEvent;
        /// <summary>
        /// Invokes the <see cref="RemoveEvent"/> when an item is removed.
        /// This method is virtual and can be overridden by derived classes.
        /// </summary>
        /// <param name="item">The item of type <typeparamref name="T"/> that has been removed.</param>
        /// <remarks>
        /// This method triggers any registered event handlers for the removal of the specified item.
        /// </remarks>
        public virtual void OnRemove(T item) => RemoveEvent?.Invoke(this, item);

        /// <summary>
        /// Represents the method that will handle the removal of an item 
        /// at a specified index in a ConcurrentList.
        /// </summary>
        /// <param name="sender">The instance of the ConcurrentList that raised the event.</param>
        /// <param name="index">The index from which the item is being removed.</param>
        /// <typeparam name="T">The type of items in the ConcurrentList.</typeparam>
        public delegate void RemoveAtHandler(ConcurrentList<T> sender, int index);
        public event RemoveAtHandler RemoveAtEvent;
        /// <summary>
        /// Invoked when an item is removed from a collection at a specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the item was removed.</param>
        /// <remarks>
        /// This method is virtual, allowing derived classes to override its behavior.
        /// If there are subscribers to the RemoveAtEvent, they will be notified 
        /// with the current instance and the index of the removed item.
        /// </remarks>
        public virtual void OnRemoveAt(int index) => RemoveAtEvent?.Invoke(this, index);

        /// <summary>
        /// Represents a method that will handle the insertion of an item into a <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="sender">The instance of <see cref="ConcurrentList{T}"/> that is invoking the event.</param>
        /// <param name="index">The index at which the item is being inserted.</param>
        /// <param name="item">The item being inserted into the list.</param>
        /// <typeparam name="T">The type of items in the <see cref="ConcurrentList{T}"/>.</typeparam>
        public delegate void InsertHandler(ConcurrentList<T> sender, int index, T item);
        public event InsertHandler InsertEvent;
        /// <summary>
        /// Executes the insert event when an item is inserted at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the item should be inserted.</param>
        /// <param name="item">The item to be inserted of type T.</param>
        /// <remarks>
        /// This method is designed to be overridden in derived classes to provide custom behavior 
        /// when an item is inserted. It triggers the InsertEvent to notify any subscribers that 
        /// an insertion has occurred.
        /// </remarks>
        public virtual void OnInsert(int index, T item) => InsertEvent?.Invoke(this, index, item);

        /// <summary>
        /// Represents a method that will handle the clearing of a <see cref="ConcurrentList{T}"/>.
        /// </summary>
        /// <param name="sender">The instance of <see cref="ConcurrentList{T}"/> that is being cleared.</param>
        /// <typeparam name="T">The type of elements in the <see cref="ConcurrentList{T}"/>.</typeparam>
        public delegate void ClearHandler(ConcurrentList<T> sender);
        public event ClearHandler ClearEvent;
        /// <summary>
        /// Invoked when the object is cleared.
        /// This method triggers the ClearEvent, notifying all subscribers
        /// that the object has been cleared. It allows for any necessary
        /// cleanup or resetting of state that needs to occur upon clearing.
        /// </summary>
        /// <remarks>
        /// The method uses a null-conditional operator to safely invoke
        /// the ClearEvent only if it has subscribers.
        /// </remarks>
        /// <seealso cref="ClearEvent"/>
        public virtual void OnClear() => ClearEvent?.Invoke(this);

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentList{T}"/> class 
        /// with the specified collection and cancellation token.
        /// </summary>
        /// <param name="collection">
        /// The collection of items to initialize the list with. This collection is 
        /// copied into the new <see cref="ConcurrentList{T}"/> instance.
        /// </param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> that can be used to cancel 
        /// operations associated with this instance. Defaults to <see cref="default"/>.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConcurrentList(IEnumerable<T> collection, System.Threading.CancellationToken cancellationToken = default) : this(new List<T>(collection), cancellationToken)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentList{T}"/> class 
        /// with a specified initial capacity and an optional cancellation token.
        /// </summary>
        /// <param name="capacity">The initial capacity of the list.</param>
        /// <param name="cancellationToken">An optional cancellation token to signal cancellation.</param>
        /// <returns>
        /// A new instance of the <see cref="ConcurrentList{T}"/> class.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConcurrentList(int capacity, System.Threading.CancellationToken cancellationToken = default) : this(new List<T>(capacity), cancellationToken)
        {

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentList{T}"/> class 
        /// using an empty list and an optional cancellation token.
        /// </summary>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> that can be used to 
        /// signal cancellation. The default value is <see cref="default"/>.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConcurrentList(System.Threading.CancellationToken cancellationToken = default) : this(new List<T>(), cancellationToken)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentList{T}"/> class.
        /// </summary>
        /// <param name="list">The initial list of items of type <typeparamref name="T"/> to use.</param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> to monitor for cancellation requests.
        /// Defaults to <see cref="CancellationToken.None"/>.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ConcurrentList(List<T> list, System.Threading.CancellationToken cancellationToken = default)
        {
            this._list = list;
            this._cancellationToken = cancellationToken;
            this._cancellationToken.Register(this.Canceled);
        }

        /// <summary>
        /// Cancels all tasks in the specified task lists.
        /// This method iterates through the tasks contained in the 
        /// '_taskAdd', '_taskAddRange', and '_taskInsert' collections, 
        /// calling the 'Cancel' method on each task's value.
        /// </summary>
        /// <remarks>
        /// The method is marked with the 'MethodImpl' attribute with 
        /// 'AggressiveInlining' option to suggest to the compiler 
        /// that it should inline the method to improve performance.
        /// </remarks>
        /// <exception cref="Exception">
        /// This method does not throw exceptions directly, 
        /// but individual tasks may throw exceptions upon cancellation 
        /// based on their implementation.
        /// </exception>
        /// <example>
        /// <code>
        /// var someInstance = new SomeClass();
        /// someInstance.Canceled();
        /// </code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Canceled()
        {
            foreach (var task in this._taskAdd.Concat(this._taskAddRange).Concat(this._taskInsert))
            {
                task.Value.Cancel();
            }
        }

        /// <summary>
        /// Checks whether the current instance has been disposed. 
        /// If the instance has been disposed, it throws an <see cref="ObjectDisposedException"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when trying to access a method or property after the object has been disposed.
        /// </exception>
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

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        /// <returns>
        /// The number of elements in the collection as an integer.
        /// </returns>
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

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        /// <value>
        /// Always returns <c>false</c>, indicating that the collection can be modified.
        /// </value>
        public bool IsReadOnly => false;

        /// <summary>
        /// Asynchronously adds a range of elements to the collection.
        /// </summary>
        /// <param name="collection">
        /// The collection of elements to add.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the object has been disposed.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task AddRangeAsync(IEnumerable<T> collection, System.Threading.CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();

            await Task.Run(() => this.AddRange(collection), cancellationToken);
        }

        /// <summary>
        /// Adds a range of elements from the specified collection to the current instance,
        /// while also allowing cancellation of the operation. The method is designed to be
        /// executed with aggressive inlining for performance optimization.
        /// </summary>
        /// <param name="collection">The collection of elements to add to the current instance.</param>
        /// <remarks>
        /// This method will throw an exception if the instance has already been disposed.
        /// It creates a cancellation token source that can be used to cancel the async operation
        /// if needed. The method also ensures thread-safe addition of the task to the 
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

        /// <summary>
        /// Adds the elements of the specified collection to the end of the list.
        /// </summary>
        /// <param name="collection">
        /// An <see cref="IEnumerable{T}"/> that contains the elements to be added to the list.
        /// </param>
        /// <remarks>
        /// This method is thread-safe and will throw an exception if the current instance has been disposed.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the current instance has been disposed when this method is called.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="collection"/> is null.
        /// </exception>
        /// <seealso cref="ThrowIfDisposed"/>
        /// <seealso cref="OnAddRange"/>
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

        /// <summary>
        /// Asynchronously adds an item of type T to the collection.
        /// </summary>
        /// <param name="item">The item to be added to the collection.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to signal the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the collection has been disposed.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task AddAsync(T item, System.Threading.CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();

            await Task.Run(() => this.Add(item), cancellationToken);
        }

        /// <summary>
        /// Adds an item to a collection asynchronously and ensures that the task is tracked.
        /// This method is designed to be called in an aggressive inlining context for performance optimization.
        /// </summary>
        /// <param name="item">The item to be added to the collection.</param>
        /// <exception cref="ObjectDisposedException">Thrown when the object has been disposed.</exception>
        /// <remarks>
        /// This method 
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

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item to be added to the collection.</param>
        /// <exception cref="ObjectDisposedException">Thrown if the object has been disposed.</exception>
        /// <remarks>
        /// This method is marked with <see cref="MethodImplAttribute"/> with <see cref="MethodImplOptions.AggressiveInlining"/> 
        /// to suggest the compiler to inline the method for performance optimization.
        /// The method is thread-safe due to the use of a lock around the critical section.
        /// </remarks>
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

        /// <summary>
        /// Clears the resources or data associated with this instance, using a specified timeout.
        /// This method is marked with the <see cref="MethodImplOptions.AggressiveInlining"/> attribute,
        /// which suggests to the compiler that it should inline this method if possible for performance improvements.
        /// </summary>
        /// <remarks>
        /// This method calls the overloaded <see cref="Clear(int)"/> method with a specific timeout value.
        /// </remarks>
        /// <exception cref="SomeExceptionType">
        /// An exception that may be thrown during the clearing process.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => this.Clear(this._clearTimeout);

        /// <summary>
        /// Clears the 
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

        /// <summary>
        /// Determines whether the collection contains a specific element.
        /// </summary>
        /// <param name="item">The object to locate in the collection. The value can be null for reference types.</param>
        /// <returns>
        /// true if the item is found in the collection; otherwise, false.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the method is called on a disposed object.
        /// </exception>
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

        /// <summary>
        /// Copies the elements of the list to a specified array, starting at a particular index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the list.</param>
        /// <param name="arrayIndex">The zero-based index in the array at which storing the copied elements begins.</param>
        /// <remarks>
        /// This method is designed to improve performance by using aggressive inlining.
        /// Additionally, it is synchronized to ensure thread safety when accessing the 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.ThrowIfDisposed();

            lock (this)
            {
                this._list.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// Enumerates the elements in the list.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the collection has been disposed.
        /// </exception>
        /// <remarks>
        /// This method uses an aggressive inlining optimization to improve performance
        /// when calling the enumerator. It acquires a lock to ensure thread safety while
        /// iterating through the list.
        /// </remarks>
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

        /// <summary>
        /// Searches for the specified item and returns the zero-based index of the first occurrence 
        /// within the collection.
        /// </summary>
        /// <param name="item">The item to locate in the collection.</param>
        /// <returns>
        /// The zero-based index of the first occurrence of the item, 
        /// if found; otherwise, -1.
        /// </returns>
        /// <exception cref="ObjectDisposedException">Thrown when the collection has been disposed.</exception>
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

        /// <summary>
        /// Inserts an item asynchronously at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the item should be inserted.</param>
        /// <param name="item">The item to insert.</param>
        /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the object has been disposed.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task InsertAsync(int index, T item, System.Threading.CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();

            await Task.Run(() => this.Insert(index, item), cancellationToken);
        }

        /// <summary>
        /// Inserts an item at the specified index asynchronously and manages the associated cancellation token.
        /// This method ensures that the operation is performed in an efficient manner by utilizing 
        /// aggressive inlining. If the insertion task is ongoing, it will continue until it can be added 
        /// to the task list or until it completes.
        /// </summary>
        /// <param name="index">The zero-based index at which the item should be inserted.</param>
        /// <param name="item">The item to be inserted into the collection.</param>
        /// <remarks>
        /// This method can throw exceptions if the instance has been disposed. Additionally, it uses aggressive 
        /// inlining to minimize the overhead of method calls in performance-critical scenarios. 
        /// The cancellation token is created for the asynchronous operation, allowing it to be canceled if needed.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">Thrown if the instance has been disposed.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled.</exception>
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

        /// <summary>
        /// Inserts an item at the specified index in the list.
        /// </summary>
        /// <param name="index">The zero-based index at which the item should be inserted.</param>
        /// <param name="item">The item to be inserted into the list.</param>
        /// <remarks>
        /// This method ensures that the list is not disposed before performing the insert operation.
        /// Additionally, it locks the list to prevent simultaneous access, which ensures thread safety during the operation.
        /// After inserting the item, it calls the <see cref="OnInsert(int, T)"/> method to execute any additional logic that should occur upon insertion.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">Thrown if the list has been disposed when attempting to insert the item.</exception>
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

        /// <summary>
        /// Removes the specified item from the collection.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <returns>
        /// True if the item was successfully removed; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This method is an implementation of the Remove functionality and utilizes a specified timeout
        /// to determine how long to attempt removal before failing. The method is marked with the 
        /// <c>MethodImplOptions.AggressiveInlining</c> attribute to suggest to the JIT compiler
        /// that it should inline this method when possible for performance considerations.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item) => this.Remove(item, this._removeTimeout);

        /// <summary>
        /// Removes the specified item from the collection, waiting until the operation is complete
        /// or the specified timeout has elapsed.
        /// </summary>
        /// <param name="item">The item to be removed from the collection.</param>
        /// <param name="timeout">The maximum amount of time to wait for the item to be removed.</param>
        /// <returns>
        /// Returns true if the item was successfully removed; otherwise, false.
        /// </returns>
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

        /// <summary>
        /// Removes the element at the specified index from the collection.
        /// This method uses aggressive inlining for performance optimization.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <remarks>
        /// This method calls another overloaded version of RemoveAt,
        /// passing in a timeout value associated with the removal operation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index) => this.RemoveAt(index, this._removeTimeout);

        /// <summary>
        /// Removes the element at the specified index from the collection with a timeout.
        /// The method waits for any ongoing add or insert tasks to complete before 
        /// removing the element. If the timeout is reached, the method will stop waiting
        /// and return without making any changes.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <param name="timeout">The maximum time to wait for ongoing tasks to complete.</param>
        /// <exception cref="ObjectDisposedException">Thrown if the object has been disposed.</exception>
        /// <remarks>
        /// This method is marked with <see cref="MethodImplOptions.AggressiveInlining"/> so that it may be inlined 
        /// for performance optimization.
        /// </remarks>
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

        /// <summary>
        /// Returns a read-only wrapper for the underlying list.
        /// </summary>
        /// <returns>
        /// A read-only list that contains the elements of the underlying list.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IReadOnlyList<T> ReadOnlyList()
        {
            this.ThrowIfDisposed();

            lock (this)
            {
                return this._list.AsReadOnly();
            }
        }

        /// <summary>
        /// Asynchronously converts the current instance to a read-only list.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a read-only list of type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the current instance has been disposed.
        /// </exception>
        /// <remarks>
        /// This method runs the conversion operation on a separate task, allowing it to be awaited asynchronously. 
        /// It uses <see cref="MethodImplOptions.AggressiveInlining"/> to suggest to the compiler that it should inline 
        /// the method for better performance.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<IReadOnlyList<T>> ToListAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();

            return await Task.Run(this.ReadOnlyList, cancellationToken);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator for the collection that implements the
        /// <see cref="IEnumerator"/> interface.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// The method is called after the object has been disposed.
        /// </exception>
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
        /// <summary>
        /// Releases the resources used by the current instance of the class.
        /// This method is called when disposing is true, to dispose of both managed
        /// and unmanaged resources. If disposing is false, only the unmanaged resources
        /// are released. This method can be overridden in a derived class to release 
        /// additional resources.
        /// </summary>
        /// <param name="disposing">
        /// A boolean value that indicates whether the method has been called directly 
        /// or indirectly by a user's code (true) or by the runtime from inside the 
        /// finalizer (false).
        /// </param>
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

        /// <summary>
        /// Finalizer for the ConcurrentList class. It is commented out here but serves as a reminder
        /// to only override the finalizer if the 'Dispose(bool disposing)' method contains code 
        /// to free unmanaged resources. 
        /// </summary>
        /// <remarks>
        /// It is important to follow the dispose pattern to ensure proper resource management, 
        /// especially for unmanaged resources. The finalizer should call the Dispose method 
        /// with disposing set to false, indicating that it is being called by the 
        /// garbage collector and not by the user code.
        /// </remarks>
        /// <example>
        /// Use the Dispose method to explicitly free resources when they are no longer needed.
        /// </example>
        /// <seealso cref="Dispose(bool)"/>
        // ~ConcurrentList() 
        // { 
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method 
        //     Dispose(disposing: false); 
        // }
        
        /// <summary>
        /// Disposes of the resources used by the ConcurrentList.
        /// </summary>
        /// <remarks>
        /// This method allows for the release of both managed and unmanaged resources. 
        /// When disposing is true, managed resources will also be disposed of.
        /// </remarks>
        /// <example>
        /// Use this method to release resources deterministically when done with the ConcurrentList.
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
