using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Dynamic.Core.Parser;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Contrib.WaitAndRetry;
using static UCode.Extensions.PollyExtension;

namespace UCode.Extensions
{
    public static partial class PollyExtension
    {
        public static Task<TResult[]> ExecuteAsync<TSource, TResult>(
            this IAsyncPolicy policy,
            IEnumerable<TSource> source,
            Func<TSource, Task<TResult>> action,
            bool continueOnCapturedContext = false,
            bool onErrorContinue = false)
        {
            // Arguments validation omitted
            var cts = new CancellationTokenSource();
            var token = !onErrorContinue ? cts.Token : default;
            var tasks = source.Select(async (item) =>
            {
                try
                {
                    return await policy.ExecuteAsync(async _ =>
                    {
                        return await action(item);
                    }, token, continueOnCapturedContext);
                }
                catch
                {
                    if (!onErrorContinue)
                        cts.Cancel();
                    throw;
                }
            }).ToArray();
            var whenAll = Task.WhenAll(tasks);
            _ = whenAll.ContinueWith(_ => cts.Dispose(), TaskScheduler.Default);
            return whenAll;
        }



        public delegate Context CreateContext(int iteration);
        public delegate Task<TResult> IteractionContext<T, TResult>(T item, Context context);
        public delegate Task<TResult> Iteraction<T, TResult>(T item);

        public static IAsyncEnumerable<T> ExecuteAsync<T>(
            this IAsyncPolicy policy,
            IAsyncEnumerable<T> source,
            CreateContext? createContext = default,
            bool onErrorContinue = true) =>
                ExecuteAsync<T, T>(policy, source, (item, context) => Task.FromResult(item), createContext, onErrorContinue);

        public static IAsyncEnumerable<TResult> ExecuteAsync<TSource, TResult>(
            [AllowNull] this IAsyncPolicy policy,
            [AllowNull] IAsyncEnumerable<TSource> source,
            [AllowNull] Iteraction<TSource, TResult> iteraction,
            CreateContext? createContext = default,
            bool onErrorContinue = true)
        {
            return ExecuteAsync<TSource, TResult>(policy, source, (item, context) => iteraction(item), createContext, onErrorContinue);
        }

        public static async IAsyncEnumerable<TResult> ExecuteAsync<TSource, TResult>(
            [AllowNull] this IAsyncPolicy policy,
            [AllowNull] IAsyncEnumerable<TSource> source,
            [AllowNull] IteractionContext<TSource, TResult> iteraction,
            CreateContext? createContext = default,
            bool onErrorContinue = true)
        {
            // Arguments validation omitted
            var cts = new CancellationTokenSource();
            var token = !onErrorContinue ? cts.Token : default;
            int count = 0;
            // ?? new Context(Guid.NewGuid().ToString(), contextData ?? new Dictionary<string, object>());

            await foreach (var item in source)
            {
                TResult? result;
                try
                {
                    var context = (createContext == null)? (new Context(Guid.NewGuid().ToString())) : createContext(Interlocked.Add(ref count, 1));

                    result = await policy.ExecuteAsync<TResult>(async (context, token) =>
                    {
                        token.ThrowIfCancellationRequested();

                        return await iteraction.Invoke(item, context);
                    }, context, token);
                }
                catch
                {
                    if (!onErrorContinue)
                        cts.Cancel();
                    throw;
                }
                

                if (result != null)
                    yield return result;
            }

            cts.Dispose();
        }



        /// <summary>
        /// Retry handler
        /// </summary>
        /// <param name="retry">number of ret</param>
        /// <returns>Time for delay</returns>
        public delegate TimeSpan RetryDelay(ushort retry);


        public delegate bool RetryException<in TException>(TException exception) where TException : Exception;

        
        public class CallDelegate<T1>
        {
            protected readonly Func<T1>? _funcT1;
            protected readonly Action<T1>? _actionT1;
            protected readonly Action? _action;
            protected readonly Task _task;
            protected readonly Task<T1> _taskT1;
            protected readonly Func<Task> _funcTask;
            protected readonly Func<Task<T1>> _funcTaskT1;
            protected readonly Func<Action> _funcAction;



            public CallDelegate(ref Func<T1> funcT1)
            {
                this._funcT1 = funcT1;
            }


            public CallDelegate(ref Action<T1> actionT1)
            {
                this._actionT1 = actionT1;
            }

            public CallDelegate(ref Action action)
            {
                this._action = action;
            }

            public CallDelegate(ref Task<T1> taskT1)
            {
                this._taskT1 = taskT1;
            }

            public CallDelegate(ref Task task)
            {
                this._task = task;
            }

            public CallDelegate(ref Func<Task<T1>> funcTaskT1)
            {
                this._funcTaskT1 = funcTaskT1;
            }

            public CallDelegate(ref Func<Task> funcTask)
            {
                this._funcTask = funcTask;
            }

            public CallDelegate(ref Func<Action> funcAction)
            {
                this._funcAction = funcAction;
            }


            public CallDelegate()
            {

            }

            #region FirstNotNull
            public static T? FirstNotNull<T>(params T?[] itens) => FirstNotNull<T>(default, itens);
            public static T? FirstNotNull<T>(T? defaultValue, params T?[] itens)
            {
                foreach (var item in itens)
                {
                    if (item != null)
                        return item;
                }

                return defaultValue;
            }
            #endregion FirstNotNull

            #region AddFuncIfNotNull

            public static Func<T>? AddFuncIfNotNull<T>(T? act)
            {
                if (act == null)
                    return null;

                return () => act;
            }

            public static Func<T>? AddFuncIfNotNull<T>(Func<T>? act)
            {
                if (act == null)
                    return null;

                return act;
            }

            public static Func<TOut>? AddFuncIfNotNull<TIn, TOut>(TIn? act, Func<TIn, TOut>? convert = null)
            {
                if(act == null)
                    return null;

                var convertedResult = convert == null ? ((TOut)(object)act) : convert(act);

                return () => convertedResult;
            }

            public static Func<TOut>? AddFuncIfNotNull<TIn, TOut>(Func<TIn>? act, Func<Func<TIn>, Func<TOut>>? convert = null)
            {
                if (act == null)
                    return null;

                return convert == null ? ((Func<TOut>)(object)act) : convert(act);
            }
            #endregion AddFuncIfNotNull

            public static implicit operator Func<T1>?(CallDelegate<T1>? source)
            {
                if (source == null)
                {
                    return default;
                }


                return FirstNotNull<Func<T1>>(
                    source._funcT1,
                    AddFuncIfNotNull<Func<Action>,T1>(source._funcAction),
                    AddFuncIfNotNull<Func<Task>, T1>(source._funcTask),
                    AddFuncIfNotNull<Action, T1>(source._action));
            }

            public static implicit operator CallDelegate<T1>?(Func<T1>? source)
            {
                if (source == null)
                {
                    return default;
                }

                return new CallDelegate<T1>(ref source);
            }


            public static implicit operator Action<T1>?(CallDelegate<T1>? source)
            {
                if (source == null)
                {
                    return default;
                }

                return source?._actionT1;
            }

            public static implicit operator CallDelegate<T1>?(Action<T1>? source)
            {
                if (source == null)
                {
                    return default;
                }

                return new CallDelegate<T1>(ref source);
            }


            public static implicit operator Action?(CallDelegate<T1>? source)
            {
                if (source == null)
                {
                    return default;
                }

                return source?._action;
            }

            public static implicit operator CallDelegate<T1>?(Action? source)
            {
                if (source == null)
                {
                    return default;
                }

                return new CallDelegate<T1>(ref source);
            }


            public static implicit operator CallDelegate<T1>?(Task<T1>? source)
            {
                if (source == null)
                {
                    return default;
                }

                return new CallDelegate<T1>(ref source);
            }

            public static implicit operator Task<T1>?(CallDelegate<T1>? source)
            {
                if (source == null)
                {
                    return default;
                }

                return source._taskT1 ?? Task.Run<T1>(source._funcT1);
            }



            public static implicit operator CallDelegate<T1>?(Task? source)
            {
                if (source == null)
                {
                    return default;
                }

                return new CallDelegate<T1>(ref source);
            }

            public static implicit operator Task?(CallDelegate<T1>? source)
            {
                if (source == null)
                {
                    return default;
                }

                return source._task ?? Task.Run(source._action);
            }



            public static implicit operator CallDelegate<T1>?(Func<Task>? source)
            {
                if (source == null)
                {
                    return default;
                }

                return new CallDelegate<T1>(ref source);
            }

            public static implicit operator Func<Task>?(CallDelegate<T1>? source)
            {
                if (source == null)
                {
                    return default;
                }

                return () => (Task)source;
            }



            public static implicit operator CallDelegate<T1>?(Func<Task<T1>>? source)
            {
                if (source == null)
                {
                    return default;
                }

                return new CallDelegate<T1>(ref source);
            }

            public static implicit operator Func<Task<T1>>?(CallDelegate<T1>? source)
            {
                if (source == null)
                {
                    return default;
                }

                return () => (Task<T1>)source;
            }




            public static implicit operator CallDelegate<T1>?(Func<Action>? source)
            {
                if (source == null)
                {
                    return default;
                }

                return new CallDelegate<T1>(ref source);
            }

            public static implicit operator Func<Action>?(CallDelegate<T1>? source)
            {
                if (source == null)
                {
                    return default;
                }

                return () => (Action)source;
            }


        }

        /// <summary>
        /// private static TimeSpan DecorrelatedJitterBackoff(int retry) => DecorrelatedJitterBackoff(Convert.ToUInt16(retry));
        /// </summary>
        /// <param name="retry"></param>
        /// <returns></returns>
        private static TimeSpan DecorrelatedJitterBackoff(ushort retry)
        {
            int retryInt = Convert.ToInt32(retry);

            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: retryInt, Random.GetInt32(0, 1000), false).ToArray();
            if (delay != null)
            {
                return delay[retryInt];
            }

            return TimeSpan.FromSeconds(Math.Pow(2, retryInt)) + TimeSpan.FromMilliseconds(Random.GetDouble(0, 1000));
        }


        //public static async Task xxxx<T>(Func<Task> func)
        //{
        //    var policy = Policy
        //      .Handle<Exception>()
        //      .WaitAndRetryAsync(new[]
        //      {
        //        TimeSpan.FromSeconds(1),
        //        TimeSpan.FromSeconds(2),
        //        TimeSpan.FromSeconds(3)
        //      });

        //    await policy.ExecuteAsync(func);

        //}

        public static Task<T> RetryAsync<T, TException>(this Task<T> call, RetryException<TException> retryException, Func<T, bool>? retryResult = null, ushort retryCount = 3, RetryDelay? retryDelay = null) where TException : Exception
            => RetryAsync<T, TException>((CallDelegate<T>)call, retryException, retryResult, retryCount, retryDelay);

        public static Task<T> RetryAsync<T, TException>(this Func<T> call, RetryException<TException> retryException, Func<T, bool>? retryResult = null, ushort retryCount = 3, RetryDelay? retryDelay = null) where TException : Exception
            => RetryAsync<T, TException>((CallDelegate<T>)call, retryException, retryResult, retryCount, retryDelay);

        public static Task<T> RetryAsync<T, TException>(this Action call, RetryException<TException> retryException, Func<T, bool>? retryResult = null, ushort retryCount = 3, RetryDelay? retryDelay = null) where TException : Exception
            => RetryAsync<T, TException>((CallDelegate<T>)call, retryException, retryResult, retryCount, retryDelay);

        public static Task<T> RetryAsync<T, TException>(this Task call, RetryException<TException> retryException, Func<T, bool>? retryResult = null, ushort retryCount = 3, RetryDelay? retryDelay = null) where TException : Exception
            => RetryAsync<T, TException>((CallDelegate<T>)call, retryException, retryResult, retryCount, retryDelay);



        public static async Task<T> RetryAsync<T, TException>(this CallDelegate<T> call, RetryException<TException> retryException, Func<T, bool>? retryResult = null, ushort retryCount = 3, RetryDelay? retryDelay = null)
             where TException : Exception
        {
            var retryCountInt = Convert.ToInt32(retryCount);

            retryDelay ??= PollyExtension.DecorrelatedJitterBackoff;

            Func<int, TimeSpan> retryDelayInt = (pos) => retryDelay(Convert.ToUInt16(pos));

            PolicyBuilder<T> policyBuilder = Policy<T>.Handle<TException>((ex) => retryException(ex)).OrResult(retryResult ?? ((item) => true));

            var policy = policyBuilder.WaitAndRetryAsync(retryCountInt, retryDelayInt);


            

            return await policy.ExecuteAsync((Func<Task<T>>)call);

        }

        //public static IAsyncEnumerable<T> RetryAsync<T, TException>(this IAsyncEnumerable<T> itens, RetryException<TException> retryException, ushort retryCount = 3, RetryDelay? retryDelay = null)
        //     where TException : Exception
        //{

        //    var retryCountInt = Convert.ToInt32(retryCount);

        //    retryDelay ??= PollyExtension.DecorrelatedJitterBackoff;

        //    Func<int, TimeSpan> retryDelayInt = (pos) => retryDelay(Convert.ToUInt16(pos));

        //    PolicyBuilder policyBuilder = Policy.Handle<TException>((ex) => retryException(ex));

        //    var policy = policyBuilder.WaitAndRetryAsync(retryCountInt, retryDelayInt);


        //    return policy.ExecuteAsync(itens, false, true);

        //    //return await policy.ExecuteAsync((AsyncEnumerable<T>)callDelegate);
        //}

        //public static T Retry<T, TException>(this Func<T> call, RetryException<TException> retryException, ushort retryCount = 3, RetryDelay? retryDelay = null)
        //     where TException : Exception
        //{
        //    var retryCountInt = Convert.ToInt32(retryCount);

        //    retryDelay ??= PollyExtension.DecorrelatedJitterBackoff;

        //    Func<int, TimeSpan> retryDelayInt = (pos) => retryDelay(Convert.ToUInt16(pos));

        //    PolicyBuilder policyBuilder = Policy.Handle<TException>((ex) => retryException(ex));

        //    //var policy = policyBuilder.WaitAndRetryAsync(retryCountInt, retryDelayInt);
        //    var policy = policyBuilder.WaitAndRetry(retryCountInt, retryDelayInt);

        //    return policy.Execute(call);
        //}
    }
}
