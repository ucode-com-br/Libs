using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    /// <summary>
    /// This class contains extension methods for implementing Polly resilience strategies.
    /// </summary>
    /// <remarks>
    /// The partial keyword indicates that other parts of this class may be defined in other files. 
    /// This class can be used to extend Polly's capabilities for enhancing application stability 
    /// through advanced fault-handling techniques such as retries, circuit breakers, and more.
    /// </remarks>
    public static partial class PollyExtension
    {
        /// <summary>
        /// Represents a collection that can be enumerated asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of elements in the asynchronous enumerable.</typeparam>
        public class AsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly IEnumerable<T>? _enumerable;
            private readonly IAsyncEnumerable<T>? _asyncEnumerable;
            private readonly Func<IEnumerable<T>>? _funcEnumerable;
            private readonly Task<IEnumerable<T>>? _taskEnumerable;
            private readonly ValueTask<IEnumerable<T>>? _valuetaskEnumerable;

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncEnumerable{T}"/> class 
            /// with the specified enumerable collection.
            /// </summary>
            /// <param name="enumerable">The enumerable collection to be wrapped in the async enumerable.</param>
            public AsyncEnumerable(IEnumerable<T> enumerable): this()
            {
                this._enumerable = enumerable;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncEnumerable{T}"/> class,
            /// using the specified asynchronous enumerable.
            /// </summary>
            /// <param name="asyncEnumerable">
            /// An instance of <see cref="IAsyncEnumerable{T}"/> to initialize the <see cref="AsyncEnumerable{T}"/>.
            /// </param>
            public AsyncEnumerable(IAsyncEnumerable<T> asyncEnumerable) : this()
            {
                this._asyncEnumerable = asyncEnumerable;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncEnumerable{T}"/> class 
            /// using a specified function that returns an <see cref="IEnumerable{T}"/>.
            /// </summary>
            /// <param name="funcEnumerable">
            /// A function that returns an <see cref="IEnumerable{T}"/> to be used 
            /// for asynchronous enumeration.
            /// </param>
            public AsyncEnumerable(Func<IEnumerable<T>> funcEnumerable) : this()
            {
                this._funcEnumerable = funcEnumerable;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncEnumerable{T}"/> class 
            /// with the specified task that returns an <see cref="IEnumerable{T}"/>.
            /// </summary>
            /// <param name="taskEnumerable">
            /// A <see cref="Task{IEnumerable{T}}"/> that represents the asynchronous operation 
            /// that returns an <see cref="IEnumerable{T}"/>.
            /// </param>
            public AsyncEnumerable(Task<IEnumerable<T>> taskEnumerable) : this()
            {
                this._taskEnumerable = taskEnumerable;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncEnumerable{T}"/> class 
            /// with a specified <see cref="ValueTask{IEnumerable{T}}"/>.
            /// </summary>
            /// <param name="valuetaskEnumerable">A <see cref="ValueTask{IEnumerable{T}}"/> that represents 
            /// the asynchronous operation returning an enumerable collection.</param>
            public AsyncEnumerable(ValueTask<IEnumerable<T>> valuetaskEnumerable) : this()
            {
                this._valuetaskEnumerable = valuetaskEnumerable;
            }


            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncEnumerable"/> class.
            /// This constructor sets up any necessary resources for the asynchronous 
            /// enumeration process, but no additional initialization logic is included.
            /// </summary>
            public AsyncEnumerable()
            {

            }

            /// <summary>
            /// Asynchronously enumerates over a sequence of values of type <typeparamref name="T"/>.
            /// The method can handle synchronous, asynchronous, func-based and task-based enumerables.
            /// </summary>
            /// <param name="cancellationToken">
            /// A <see cref="CancellationToken"/> that can be used to signal cancellation of the enumeration.
            /// </param>
            /// <returns>
            /// An <see cref="IAsyncEnumerator{T}"/> that allows asynchronous iteration over the sequence of values.
            /// </returns>
            public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                if (this._enumerable != null)
                {
                    foreach (T item in this._enumerable)
                    {
                        yield return item;

                        if (cancellationToken.IsCancellationRequested)
                        {
                            yield break;
                        }
                    }

                    yield break;
                }
                else if (this._asyncEnumerable != null)
                {
                    await foreach (T item in this._asyncEnumerable)
                    {
                        yield return item;

                        if (cancellationToken.IsCancellationRequested)
                        {
                            yield break;
                        }
                    }

                    yield break;
                }
                else if (this._funcEnumerable != null)
                {
                    foreach (T item in this._funcEnumerable())
                    {
                        yield return item;

                        if (cancellationToken.IsCancellationRequested)
                        {
                            yield break;
                        }
                    }

                    yield break;
                }
                else if (this._taskEnumerable != null)
                {
                    await this._taskEnumerable.WaitAsync(cancellationToken);

                    foreach (T item in this._taskEnumerable!.Result)
                    {
                        yield return item;

                        if (cancellationToken.IsCancellationRequested)
                        {
                            yield break;
                        }
                    }

                    this._taskEnumerable!.Dispose();

                    yield break;
                }
                else if (this._valuetaskEnumerable != null)
                {
                    var r = await this._valuetaskEnumerable!.Value;

                    foreach (T item in r)
                    {
                        yield return item;

                        if (cancellationToken.IsCancellationRequested)
                        {
                            yield break;
                        }
                    }

                    yield break;
                }
            }
        }






    }
}
