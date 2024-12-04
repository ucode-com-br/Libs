using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    /// <summary>
    /// Provides extension methods for the Polly resilience library.
    /// </summary>
    /// <remarks>
    /// This class is marked as partial, allowing other parts of the class to be defined in separate files.
    /// </remarks>
    public static partial class PollyExtension
    {
        /// <summary>
        /// Represents an asynchronous enumerator that can iterate over a collection of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements to enumerate.</typeparam>
        public class AsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IAsyncEnumerator<T>? _asyncEnumerator = null;
            private readonly IEnumerator<T>? _enumerator = null;
            private readonly Func<IEnumerator<T>>? _funcEnumerator = null;
            private readonly Task<IEnumerator<T>>? _taskEnumerator = null;
            private readonly ValueTask<IEnumerator<T>>? _valuetaskEnumerator = null;

            private readonly Lazy<IEnumerator<T>> __lazyEnumerator = null;
            /// <summary>
            /// Gets the enumerator for the lazy collection.
            /// </summary>
            /// <returns>An enumerator of type <typeparamref name="T"/> that can be used to iterate through the lazy collection.</returns>
            /// <exception cref="InvalidOperationException">Thrown when the enumerator is accessed and the underlying collection is not initialized.</exception>
            private IEnumerator<T> Enumerator
            {
                get => __lazyEnumerator!.Value;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncEnumerator{T}"/> class 
            /// with the specified asynchronous enumerator.
            /// </summary>
            /// <param name="asyncEnumerator">The asynchronous enumerator to use.</param>
            /// <remarks>
            /// The constructor initializes the 
            public AsyncEnumerator(IAsyncEnumerator<T> asyncEnumerator) : this()
            {
                _asyncEnumerator = asyncEnumerator;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncEnumerator{T}"/> class 
            /// with the specified enumerator.
            /// </summary>
            /// <param name="enumerator">An instance of <see cref="IEnumerator{T}"/> to be used by the AsyncEnumerator.</param>
            public AsyncEnumerator(IEnumerator<T> enumerator) : this()
            {
                _enumerator = enumerator;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncEnumerator{T}"/> class 
            /// using the specified function to create an enumerator.
            /// </summary>
            /// <param name="funcEnumerator">
            /// A function that returns an <see cref="IEnumerator{T}"/>. This function is called 
            /// to create a new enumerator.
            public AsyncEnumerator(Func<IEnumerator<T>> funcEnumerator) : this()
            {
                _funcEnumerator = funcEnumerator;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncEnumerator{T}"/> class 
            /// with the specified task enumerator.
            /// </summary>
            /// <param name="taskEnumerator">
            /// A <see cref="Task{IEnumerator{T}}"/> representing the asynchronous enumerator 
            /// that will be used by this instance.
            /// </param>
            public AsyncEnumerator(Task<IEnumerator<T>> taskEnumerator) : this()
            {
                _taskEnumerator = taskEnumerator;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncEnumerator{T}"/> class
            /// using the specified <see cref="ValueTask{IEnumerator{T}}"/>.
            /// </summary>
            /// <param name="valuetaskEnumerator">
            /// A <see cref="ValueTask{IEnumerator{T}}"/> representing the asynchronous operation 
            /// that returns an enumerator of type <typeparamref name="T"/>.
            /// </param>
            public AsyncEnumerator(ValueTask<IEnumerator<T>> valuetaskEnumerator): this()
            {
                _valuetaskEnumerator = valuetaskEnumerator;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncEnumerator"/> class.
            /// This constructor sets up a lazy enumerator that can yield elements 
            /// from various types of enumerators such as asynchronous enumerators, 
            /// value task enumerators, or function delegates that produce enumerators.
            /// </summary>
            /// <remarks>
            /// This constructor employs a lazy initialization pattern, ensuring that 
            /// the enumerator is only created when it is first accessed. Furthermore, 
            /// it handles multiple types of enumerators, providing flexibility 
            /// in how enumerables are used within asynchronous programming contexts.
            /// </remarks>
            /// <exception cref="ArgumentNullException">
            /// Thrown when none of the stored enumerators are available, indicating 
            /// that an enumerator could not be found to yield values.
            /// </exception>
            public AsyncEnumerator()
            {
                __lazyEnumerator = new Lazy<IEnumerator<T>>(() => {
                    if (_asyncEnumerator != null)
                    {
                        return GetEnumerator(_asyncEnumerator);
                    }

                    if (_valuetaskEnumerator != null)
                    {
                        return Wait(_valuetaskEnumerator.Value);
                    }

                    if (_enumerator != null)
                    {
                        return _enumerator;
                    }

                    if (_funcEnumerator != null)
                    {
                        return _funcEnumerator();
                    }

                    if (_taskEnumerator != null)
                    {

                        _taskEnumerator.Wait();

                        var r = _taskEnumerator.Result;

                        _taskEnumerator.Dispose();

                        return r;
                    }

                    throw new ArgumentNullException("unable find enumerator.");
                }, LazyThreadSafetyMode.ExecutionAndPublication);
            }

            /// <summary>
            /// Asynchronously enumerates the items produced by the provided asynchronous enumerator.
            /// </summary>
            /// <param name="asyncEnumerator">
            /// An instance of <see cref="IAsyncEnumerator{T}"/> used to retrieve the elements asynchronously.
            /// This parameter cannot be null.
            /// </param>
            /// <returns>
            /// An <see cref="IEnumerator{T}"/> that allows for the asynchronous enumeration of the items.
            /// </returns>
            /// <remarks>
            /// This method uses the <see cref="Wait"/> function to pause execution until the next item is available. 
            /// It yields each non-null item produced by the asynchronous enumerator.
            /// </remarks>
            private static IEnumerator<T> GetEnumerator([NotNull] IAsyncEnumerator<T> asyncEnumerator)
            {
                while (Wait(asyncEnumerator.MoveNextAsync()))
                {
                    var current = asyncEnumerator.Current;

                    if (current != null)
                        yield return asyncEnumerator.Current;
                }
            }
            /// <summary>
            /// Waits for the completion of the given ValueTask and returns the result.
            /// If the ValueTask is already completed, its result is obtained directly.
            /// Otherwise, it converts the ValueTask to a Task and waits for it to complete,
            /// then retrieves the result from the Task.
            /// </summary>
            /// <typeparam name="TResult">The type of the result returned by the ValueTask.</typeparam>
            /// <param name="valueTask">The ValueTask to wait for.</param>
            /// <returns>
            /// The result of the ValueTask, or the default value of TResult if the
            /// ValueTask is not completed successfully.
            /// </returns>
            private static TResult? Wait<TResult>(ValueTask<TResult> valueTask)
            {
                TResult? result = default;

                if (valueTask.IsCompleted)
                {
                    result = valueTask.Result;
                }
                else
                {
                    using (var task = valueTask.AsTask())
                    {
                        task.Wait();

                        result = task.Result;
                    }
                }
                
                return result;
            }

            /// <summary>
            /// Waits for the completion of a task that produces an enumerator and returns the result.
            /// </summary>
            /// <param name="enumerator">
            /// The task producing an enumerator of type <typeparamref name="T"/>. This can be null.
            /// </param>
            /// <returns>
            /// Returns the result of the completed task if it is not null; otherwise, returns null.
            /// </returns>
            private static IEnumerator<T>? WaitResult(Task<IEnumerator<T>>? enumerator)
            {
                if(enumerator == null)
                    return null;

                enumerator.Wait();

                return enumerator.Result;
            }

            /// <summary>
            /// Asynchronously waits for a ValueTask that returns an IEnumerator of type T.
            /// </summary>
            /// <param name="enumerator">A nullable ValueTask that, when awaited, yields an IEnumerator of type T.</param>
            /// <returns>
            /// Returns the Result of the ValueTask as an IEnumerator of type T, or null if the provided ValueTask is null.
            /// </returns>
            private static IEnumerator<T>? WaitResult(ValueTask<IEnumerator<T>>? enumerator)
            {
                if (enumerator == null)
                    return null;


                return enumerator.Value.Result;
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            /// <value>
            /// The element at the current position of the enumerator, 
            /// which is of type <typeparamref name="T"/>.
            /// </value>
            /// <remarks>
            /// This property is read-only and provides access to the current element
            /// during enumeration. If the enumerator is positioned before the first 
            /// element or after the last element, accessing this property will throw 
            /// an exception. Ensure that the enumerator is valid before trying to 
            /// access the Current property.
            /// </remarks>
            public T Current => this.Enumerator.Current;

            /// <summary>
            /// Asynchronously disposes of resources used by the object, including any 
            /// asynchronous enumerators and regular enumerators. This method ensures 
            /// that all resources are properly released, preventing resource leaks.
            /// </summary>
            /// <returns>
            /// A <see cref="ValueTask"/> representing the asynchronous operation of 
            /// disposing resources. The result of the task indicates the completion 
            /// of the dispose operation.
            /// </returns>
            public async ValueTask DisposeAsync()
            {
                if (this._asyncEnumerator != null)
                {
                    try
                    {
                        await this._asyncEnumerator!.DisposeAsync();
                    }
                    catch
                    {
                    }
                }

                if (this._enumerator != null)
                {
                    try
                    {
                        this._enumerator.Dispose();
                    }
                    catch
                    {
                    }
                }


                if (this._taskEnumerator != null)
                {
                    try
                    {
                        this._taskEnumerator.Dispose();
                    }
                    catch
                    {
                    }
                }

            }

            /// <summary>
            /// Asynchronously advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="ValueTask{T}"/> that represents the asynchronous operation,
            /// containing a boolean value that indicates whether the enumerator was 
            /// successfully advanced to the next element (<c>true</c>) or the enumerator 
            /// has passed the end of the collection (<c>false</c>).
            /// </returns>
            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(this.Enumerator.MoveNext());
        }






    }
}
