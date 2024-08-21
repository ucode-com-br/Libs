using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    public static partial class PollyExtension
    {
        public class AsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IAsyncEnumerator<T>? _asyncEnumerator = null;
            private readonly IEnumerator<T>? _enumerator = null;
            private readonly Func<IEnumerator<T>>? _funcEnumerator = null;
            private readonly Task<IEnumerator<T>>? _taskEnumerator = null;
            private readonly ValueTask<IEnumerator<T>>? _valuetaskEnumerator = null;

            private readonly Lazy<IEnumerator<T>> __lazyEnumerator = null;
            private IEnumerator<T> Enumerator
            {
                get => __lazyEnumerator!.Value;
            }

            public AsyncEnumerator(IAsyncEnumerator<T> asyncEnumerator) : this()
            {
                _asyncEnumerator = asyncEnumerator;
            }

            public AsyncEnumerator(IEnumerator<T> enumerator) : this()
            {
                _enumerator = enumerator;
            }

            public AsyncEnumerator(Func<IEnumerator<T>> funcEnumerator) : this()
            {
                _funcEnumerator = funcEnumerator;
            }

            public AsyncEnumerator(Task<IEnumerator<T>> taskEnumerator) : this()
            {
                _taskEnumerator = taskEnumerator;
            }

            public AsyncEnumerator(ValueTask<IEnumerator<T>> valuetaskEnumerator): this()
            {
                _valuetaskEnumerator = valuetaskEnumerator;
            }

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

            private static IEnumerator<T> GetEnumerator([NotNull] IAsyncEnumerator<T> asyncEnumerator)
            {
                while (Wait(asyncEnumerator.MoveNextAsync()))
                {
                    var current = asyncEnumerator.Current;

                    if (current != null)
                        yield return asyncEnumerator.Current;
                }
            }
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

            private static IEnumerator<T>? WaitResult(Task<IEnumerator<T>>? enumerator)
            {
                if(enumerator == null)
                    return null;

                enumerator.Wait();

                return enumerator.Result;
            }

            private static IEnumerator<T>? WaitResult(ValueTask<IEnumerator<T>>? enumerator)
            {
                if (enumerator == null)
                    return null;


                return enumerator.Value.Result;
            }

            public T Current => this.Enumerator.Current;

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

            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(this.Enumerator.MoveNext());
        }






    }
}
