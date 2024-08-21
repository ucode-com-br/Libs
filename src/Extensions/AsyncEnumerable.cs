using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    public static partial class PollyExtension
    {
        public class AsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly IEnumerable<T>? _enumerable;
            private readonly IAsyncEnumerable<T>? _asyncEnumerable;
            private readonly Func<IEnumerable<T>>? _funcEnumerable;
            private readonly Task<IEnumerable<T>>? _taskEnumerable;
            private readonly ValueTask<IEnumerable<T>>? _valuetaskEnumerable;

            public AsyncEnumerable(IEnumerable<T> enumerable): this()
            {
                this._enumerable = enumerable;
            }

            public AsyncEnumerable(IAsyncEnumerable<T> asyncEnumerable) : this()
            {
                this._asyncEnumerable = asyncEnumerable;
            }

            public AsyncEnumerable(Func<IEnumerable<T>> funcEnumerable) : this()
            {
                this._funcEnumerable = funcEnumerable;
            }

            public AsyncEnumerable(Task<IEnumerable<T>> taskEnumerable) : this()
            {
                this._taskEnumerable = taskEnumerable;
            }

            public AsyncEnumerable(ValueTask<IEnumerable<T>> valuetaskEnumerable) : this()
            {
                this._valuetaskEnumerable = valuetaskEnumerable;
            }


            public AsyncEnumerable()
            {

            }

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
