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
    /// <summary>
    /// Provides extension methods for Polly, a .NET library used for resilience and fault-handling.
    /// </summary>
    /// <remarks>
    /// This class is defined as a static partial class, which means it can be extended in other parts of the codebase.
    /// </remarks>
    public static partial class PollyExtension
    {
        /// <summary>
        /// Executes an asynchronous action for each element in a given source collection 
        /// using the specified asynchronous policy. The execution can be configured 
        /// to either continue on error or not.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source collection.</typeparam>
        /// <typeparam name="TResult">The type of the result returned by the action.</typeparam>
        /// <param name="policy">The asynchronous policy to apply during the execution.</param>
        /// <param name="source">The collection of elements to process.</param>
        /// <param name="action">The asynchronous function to be executed on each element of the source.</param>
        /// <param name="continueOnCapturedContext">
        /// Specifies whether to capture the current context for the continuation. 
        /// Default is false.
        /// </param>
        /// <param name="onErrorContinue">
        /// Specifies whether to continue execution on error. If false, 
        /// the cancellation token will be triggered on the first error. Default is false.
        /// </param>
        /// <returns>A task representing the asynchronous operation, containing an array of 
        /// results of type TResult.</returns>
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



        /// <summary>
        /// Represents a method that creates a <see cref="Context"/> instance 
        /// based on the specified iteration parameter.
        /// </summary>
        /// <param name="iteration">An integer that specifies the iteration count.</param>
        /// <returns>
        /// A <see cref="Context"/> instance created for the given iteration.
        /// </returns>
        public delegate Context CreateContext(int iteration);
        /// <summary>
        /// Represents a delegate that defines a method signature for handling an 
        /// interaction context with a specific item and returning a result asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the item that will be processed.</typeparam>
        /// <typeparam name="TResult">The type of the result that will be returned.</typeparam>
        /// <param name="item">The item of type <typeparamref name="T"/> to be processed.</param>
        /// <param name="context">An instance of <see cref="Context"/> representing the context of the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation that returns a result of type <typeparamref name="TResult"/>.</returns>
        public delegate Task<TResult> IteractionContext<T, TResult>(T item, Context context);
        /// <summary>
        /// Represents a delegate that defines a method which processes an item of type <typeparamref name="T"/> 
        /// and returns a task that, when completed, yields a result of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="T">The type of the input item that the delegate processes.</typeparam>
        /// <typeparam name="TResult">The type of the result returned by the delegate after processing the input item.</typeparam>
        /// <returns>A task that represents the asynchronous operation, containing the result of type <typeparamref name="TResult"/>.</returns>
        public delegate Task<TResult> Iteraction<T, TResult>(T item);

        /// <summary>
        /// Executes an asynchronous operation on an IAsyncEnumerable<T> source using a specified IAsyncPolicy.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source.</typeparam>
        /// <param name="policy">The IAsyncPolicy to be applied during the execution.</param>
        /// <param name="source">The IAsyncEnumerable<T> to execute the operation on.</param>
        /// <param name="createContext">An optional parameter to create a context for the policy.</param>
        /// <param name="onErrorContinue">A boolean indicating whether to continue executing on error.</param>
        /// <returns>
        /// An IAsyncEnumerable<T> representing the asynchronous operation execution result.
        /// </returns>
        /// <remarks>
        /// This method allows for applying a resilience strategy to asynchronous enumerable operations,
        /// potentially handling errors gracefully depending on the specified policy and onErrorContinue flag.
        /// </remarks>
        public static IAsyncEnumerable<T> ExecuteAsync<T>(
            this IAsyncPolicy policy,
            IAsyncEnumerable<T> source,
            CreateContext? createContext = default,
            bool onErrorContinue = true) =>
                ExecuteAsync<T, T>(policy, source, (item, context) => Task.FromResult(item), createContext, onErrorContinue);

        /// <summary>
        /// Executes the provided async policy on an async enumerable source,
        /// transforming each element of the source using the specified interaction function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source async enumerable.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the interaction function.</typeparam>
        /// <param name="policy">
        /// An instance of <see cref="IAsyncPolicy"/> that defines the policy to be applied.
        /// This cannot be null.
        /// </param>
        /// <param name="source">
        /// The source async enumerable providing input items for processing.
        /// This may be null.
        /// </param>
        /// <param name="iteraction">
        /// The function that defines how each element of the source should be transformed.
        /// This may be null.
        /// </param>
        /// <param name="createContext">
        /// An optional function to create a context for the execution.
        /// This can be null, which implies the default behavior.
        /// </param>
        /// <param name="onErrorContinue">
        /// Indicates whether to continue execution on error. If true, execution will continue
        /// even if an error occurs during processing; otherwise, it will stop.
        /// </param>
        /// <returns>
        /// An <see cref="IAsyncEnumerable{TResult}"/> that provides the processed results
        /// as per the specified interaction function, using the provided policy.
        /// </returns>
        public static IAsyncEnumerable<TResult> ExecuteAsync<TSource, TResult>(
            [AllowNull] this IAsyncPolicy policy,
            [AllowNull] IAsyncEnumerable<TSource> source,
            [AllowNull] Iteraction<TSource, TResult> iteraction,
            CreateContext? createContext = default,
            bool onErrorContinue = true)
        {
            return ExecuteAsync<TSource, TResult>(policy, source, (item, context) => iteraction(item), createContext, onErrorContinue);
        }

        /// <summary>
        /// Executes an asynchronous operation on each item in the provided asynchronous enumerable collection,
        /// applying a specified policy and interaction context to produce results.
        /// </summary>
        /// <typeparam name="TSource">The type of the source elements in the asynchronous enumerable.</typeparam>
        /// <typeparam name="TResult">The type of the result produced for each element.</typeparam>
        /// <param name="policy">The IAsyncPolicy to apply to the execution of the asynchronous operation.</param>
        /// <param name="source">An asynchronous enumerable of type TSource from which items are taken.</param>
        /// <param name="iteraction">The interaction context that defines how to transform each item into a TResult.</param>
        /// <param name="createContext">An optional function to create a context for each operation; 
        /// if not provided, a new context will be created for each iteration.</param>
        /// <param name="onErrorContinue">A boolean value that determines whether the execution should continue
        /// on error (if false, execution will be canceled on the first error).</param>
        /// <returns>An asynchronous enumerable of TResult resulting from the applied operations on the source items.</returns>
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
        /// Represents a delegate that defines a method for calculating the delay between retry attempts.
        /// </summary>
        /// <param name="retry">The current retry attempt count, represented as an unsigned short.</param>
        /// <returns>
        /// A <see cref="TimeSpan"/> representing the duration of the delay after the current retry attempt.
        /// </returns>
        public delegate TimeSpan RetryDelay(ushort retry);


        /// <summary>
        /// Represents a method that defines a condition for retrying an operation based on the specified exception type.
        /// </summary>
        /// <typeparam name="TException">The type of exception to be handled. This must be a subclass of <see cref="Exception"/>.</typeparam>
        /// <param name="exception">The exception that has occurred during the operation.</param>
        /// <returns>
        /// A boolean value indicating whether the operation should be retried based on the given exception.
        /// </returns>
        /// <example>
        /// This delegate can be used in scenarios where operations that may fail need to be retried under certain conditions.
        /// For example:
        /// <code>
        /// void SomeOperation()
        /// {
        ///     try
        ///     {
        ///         // Attempt to execute a risky operation
        ///     }
        ///     catch (TException ex)
        ///     {
        ///         // Invoke the retry condition delegate
        ///         if (retryCondition(ex))
        ///         {
        ///             // Retry the operation
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public delegate bool RetryException<in TException>(TException exception) where TException : Exception;

        
        /// <summary>
        /// Represents a class designed to handle delegations involving a single generic type parameter.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter for the delegate.</typeparam>
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



            /// <summary>
            /// Initializes a new instance of the <see cref="CallDelegate"/> class,
            /// assigning the provided function reference.
            /// </summary>
            /// <param name="funcT1">
            /// A reference to a delegate of type <see cref="Func{T1}"/> that is assigned to the 
            public CallDelegate(ref Func<T1> funcT1)
            {
                this._funcT1 = funcT1;
            }


            /// <summary>
            /// Initializes a new instance of the <see cref="CallDelegate"/> class 
            /// with a reference to an Action delegate.
            /// </summary>
            /// <param name="actionT1">A reference to an Action delegate that takes 
            /// a single parameter of type T1.</param>
            public CallDelegate(ref Action<T1> actionT1)
            {
                this._actionT1 = actionT1;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CallDelegate"/> class.
            /// </summary>
            /// <param name="action">A reference to an <see cref="Action"/> delegate that will be used by the instance.</param>
            public CallDelegate(ref Action action)
            {
                this._action = action;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CallDelegate"/> class.
            /// </summary>
            /// <param name="taskT1">A reference to a Task of type T1 that will be assigned to the 
            public CallDelegate(ref Task<T1> taskT1)
            {
                this._taskT1 = taskT1;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CallDelegate"/> class, taking a reference to a Task.
            /// </summary>
            /// <param name="task">A reference to a <see cref="Task"/> object that the <see cref="CallDelegate"/> will operate on.</param>
            public CallDelegate(ref Task task)
            {
                this._task = task;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CallDelegate"/> class.
            /// </summary>
            /// <param name="funcTaskT1">
            /// A reference to a delegate of type <see cref="Func{Task{T1}}"/> that will be used
            /// to specify a method for executing a task that returns a value of type <typeparamref name="T1"/>.
            /// </param>
            public CallDelegate(ref Func<Task<T1>> funcTaskT1)
            {
                this._funcTaskT1 = funcTaskT1;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CallDelegate"/> class.
            /// </summary>
            /// <param name="funcTask">
            /// A reference to a function that returns a <see cref="Task"/>. This function will 
            /// be assigned to the 
            public CallDelegate(ref Func<Task> funcTask)
            {
                this._funcTask = funcTask;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CallDelegate"/> class.
            /// This constructor assigns the provided function delegate to the 
            public CallDelegate(ref Func<Action> funcAction)
            {
                this._funcAction = funcAction;
            }


            /// <summary>
            /// Initializes a new instance of the <see cref="CallDelegate"/> class.
            /// This constructor does not take any parameters and does not perform any special operations.
            /// </summary>
            public CallDelegate()
            {

            }

            #region FirstNotNull
            /// <summary>
            /// Returns the first non-null value from the provided array of nullable values.
            /// </summary>
            /// <typeparam name="T">The type of the values in the array.</typeparam>
            /// <param name="itens">An array of nullable values of type T.</param>
            /// <returns>The first non-null value of type T, or null if all values are null.</returns>
            /// <example>
            /// var result = FirstNotNull<int?>(null, null, 5, null); // result will be 5
            /// </example>
            public static T? FirstNotNull<T>(params T?[] itens) => FirstNotNull<T>(default, itens);
            /// <summary>
            /// Returns the first non-null value from a provided list of nullable values.
            /// If all provided values are null, it returns the specified default value.
            /// </summary>
            /// <typeparam name="T">The type of the values being evaluated.</typeparam>
            /// <param name="defaultValue">The value to return if all provided items are null.</param>
            /// <param name="itens">An array of nullable values to check for non-null values.</param>
            /// <returns>
            /// The first non-null value found in the list of items, or <paramref name="defaultValue"/> 
            /// if all items are null.
            /// </returns>
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

            /// <summary>
            /// Adds a function that returns the provided value if it is not null.
            /// </summary>
            /// <typeparam name="T">
            /// The type of the value to be checked for nullity and returned.
            /// </typeparam>
            /// <param name="act">
            /// The value to be wrapped in a function if it is not null.
            /// </param>
            /// <returns>
            /// A function that returns the value if it is not null; otherwise, returns null.
            /// </returns>
            public static Func<T>? AddFuncIfNotNull<T>(T? act)
            {
                if (act == null)
                    return null;

                return () => act;
            }

            /// <summary>
            /// Adds a function to a collection only if it is not null.
            /// </summary>
            /// <typeparam name="T">The type of the function's return value.</typeparam>
            /// <param name="act">The function to be added, or null.</param>
            /// <returns>
            /// Returns the provided function if it is not null; otherwise, returns null.
            /// </returns>
            public static Func<T>? AddFuncIfNotNull<T>(Func<T>? act)
            {
                if (act == null)
                    return null;

                return act;
            }

            /// <summary>
            /// Adds a function that converts an input of type TIn to an output of type TOut, 
            /// returning null if the input is null.
            /// </summary>
            /// <typeparam name="TIn">The type of the input parameter.</typeparam>
            /// <typeparam name="TOut">The type of the output result.</typeparam>
            /// <param name="act">The input value, which can be null.</param>
            /// <param name="convert">An optional function for converting TIn to TOut.</param>
            /// <returns>
            /// A function that returns the converted result if act is not null; otherwise, returns null.
            /// </returns>
            public static Func<TOut>? AddFuncIfNotNull<TIn, TOut>(TIn? act, Func<TIn, TOut>? convert = null)
            {
                if(act == null)
                    return null;

                var convertedResult = convert == null ? ((TOut)(object)act) : convert(act);

                return () => convertedResult;
            }

            /// <summary>
            /// Adds a function to the result if the provided function is not null.
            /// </summary>
            /// <typeparam name="TIn">The input type for the function.</typeparam>
            /// <typeparam name="TOut">The output type for the function.</typeparam>
            /// <param name="act">The function to add if it is not null.</param>
            /// <param name="convert">An optional conversion function that transforms the input function.</param>
            /// <returns>
            /// A nullable function of type <see cref="Func{TOut}"/> if <paramref name="act"/> is not null; otherwise, null.
            /// </returns>
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
        /// Calculates a delay for a backoff strategy that incorporates 
        /// a decorrelated jitter algorithm based on the number of retries.
        /// This method is designed to help manage retry attempts in 
        /// network or service calls.
        /// </summary>
        /// <param name="retry">The number of retry attempts that have been made.</param>
        /// <returns>
        /// A TimeSpan representing the calculated delay for the current retry. 
        /// The delay will be computed using the Decorrelated Jitter Backoff 
        /// algorithm, and in cases where the calculated delay array is null, 
        /// it will fallback to a standard exponential backoff 
        /// with additional random milliseconds jitter.
        /// </returns>
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

        /// <summary>
        /// Executes a given task with a specified retry policy. The method 
        /// allows for retries when a specific exception occurs, and it can 
        /// optionally evaluate the result of the task to determine whether 
        /// to retry based on the result.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the result produced by the task.
        /// </typeparam>
        /// <typeparam name="TException">
        /// The type of exception that will trigger a retry.
        /// Must derive from <see cref="Exception"/>.
        /// </typeparam>
        /// <param name="call">
        /// The task to execute and potentially retry.
        /// </param>
        /// <param name="retryException">
        /// An instance of <see cref="RetryException{TException}"/> 
        /// that defines the retry logic based on the exception.
        /// </param>
        /// <param name="retryResult">
        /// An optional function that receives the result of the task. 
        /// If not null, the result will be evaluated to decide 
        /// whether to retry based on a boolean return value.
        /// </param>
        /// <param name="retryCount">
        /// The maximum number of retry attempts. Default is 3.
        /// </param>
        /// <param name="retryDelay">
        /// An optional <see cref="RetryDelay"/> that defines the 
        /// delay strategy between retries. If not specified, defaults 
        /// to a standard delay pattern.
        /// </param>
        /// <returns>
        /// A Task representing the result of the asynchronous operation, 
        /// which may have retried if specified conditions were met. 
        /// The result type is <typeparamref name="T"/>.
        /// </returns>
        public static Task<T> RetryAsync<T, TException>(this Task<T> call, RetryException<TException> retryException, Func<T, bool>? retryResult = null, ushort retryCount = 3, RetryDelay? retryDelay = null) where TException : Exception
            => RetryAsync<T, TException>((CallDelegate<T>)call, retryException, retryResult, retryCount, retryDelay);

        /// <summary>
        /// Asynchronously retries a specified function call a given number of times 
        /// in the event of a particular exception being thrown.
        /// </summary>
        /// <typeparam name="T">The type of the result returned by the function.</typeparam>
        /// <typeparam name="TException">The type of exception to catch for retrying the operation.</typeparam>
        /// <param name="call">The function to execute and potentially retry.</param>
        /// <param name="retryException">An instance of <see cref="RetryException{TException}"/> to handle the retry logic.</param>
        /// <param name="retryResult">An optional function to determine if the result should trigger a retry.</param>
        /// <param name="retryCount">The number of times to retry the call in case of a failure. Defaults to 3.</param>
        /// <param name="retryDelay">An optional <see cref="RetryDelay"/> to specify the delay between retries.</param>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation, with the result of the function call.</returns>
        /// <remarks>
        /// This method allows for customizable retry behavior, including the ability to specify
        /// a condition for retrying based on the result of the function, and configurable delay
        /// between retries. The specified <typeparamref name="TException"/> will be caught and
        /// handled according to the provided <paramref name="retryException"/> logic.
        /// </remarks>
        public static Task<T> RetryAsync<T, TException>(this Func<T> call, RetryException<TException> retryException, Func<T, bool>? retryResult = null, ushort retryCount = 3, RetryDelay? retryDelay = null) where TException : Exception
            => RetryAsync<T, TException>((CallDelegate<T>)call, retryException, retryResult, retryCount, retryDelay);

        /// <summary>
        /// Asynchronously retries an action with a specified retry mechanism, allowing for custom exception handling and result evaluation.
        /// </summary>
        /// <typeparam name="T">The type of the result returned by the action.</typeparam>
        /// <typeparam name="TException">The type of exception that triggers a retry.</typeparam>
        /// <param name="call">The action delegate that will be executed and retried upon failure.</param>
        /// <param name="retryException">An instance of <see cref="RetryException{TException}"/> that defines the retry behavior on specific exceptions.</param>
        /// <param name="retryResult">An optional function that takes the result of the action and determines if a retry should be attempted.</param>
        /// <param name="retryCount">The maximum number of times to retry the action.</param>
        /// <param name="retryDelay">An optional <see cref="RetryDelay"/> object that defines the delay between retries.</param>
        /// <returns>A <see cref="Task{T}"/> that represents the asynchronous operation, containing the result of the action if successful.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="call"/> or <paramref name="retryException"/> are null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the maximum retry count is reached without a successful execution.</exception>
        public static Task<T> RetryAsync<T, TException>(this Action call, RetryException<TException> retryException, Func<T, bool>? retryResult = null, ushort retryCount = 3, RetryDelay? retryDelay = null) where TException : Exception
            => RetryAsync<T, TException>((CallDelegate<T>)call, retryException, retryResult, retryCount, retryDelay);

        /// <summary>
        /// Asynchronously retries a given task if it throws a specified type of exception.
        /// </summary>
        /// <typeparam name="T">The type of the result returned by the task.</typeparam>
        /// <typeparam name="TException">The type of the exception to catch. Must derive from <see cref="System.Exception"/>.</typeparam>
        /// <param name="call">The task to be executed and possibly retried.</param>
        /// <param name="retryException">An instance of <see cref="RetryException{TException}"/> that defines the exception to retry on.</param>
        /// <param name="retryResult">An optional predicate function that determines if the result should be retried based on the outcome.</param>
        /// <param name="retryCount">The number of times to retry the task. Default is 3.</param>
        /// <param name="retryDelay">An optional delay configuration to specify the delay between retries.</param>
        /// <returns>A <see cref="Task{T}"/> that represents the asynchronous operation, containing the result of the task.</returns>
        /// <exception cref="TException">Thrown if the task fails and the maximum retry count is reached.</exception>
        /// <remarks>
        /// This method provides a mechanism to handle transient failures by retrying the execution of a task that returns a result of type <typeparamref name="T"/>.
        /// </remarks>
        public static Task<T> RetryAsync<T, TException>(this Task call, RetryException<TException> retryException, Func<T, bool>? retryResult = null, ushort retryCount = 3, RetryDelay? retryDelay = null) where TException : Exception
            => RetryAsync<T, TException>((CallDelegate<T>)call, retryException, retryResult, retryCount, retryDelay);



        /// <summary>
        /// Executes a delegate asynchronously with a retry policy. If an exception of type <typeparamref name="TException"/> is thrown, or if the result fails the provided <paramref name="retryResult"/> predicate, the operation will automatically retry the specified number of times.
        /// </summary>
        /// <typeparam name="T">The type of the result returned by the delegate.</typeparam>
        /// <typeparam name="TException">The type of exception that will trigger a retry.</typeparam>
        /// <param name="call">The asynchronous delegate to execute with a retry policy.</param>
        /// <param name="retryException">An instance of <see cref="RetryException{TException}"/> that defines the behavior for when a retry is needed due to an exception.</param>
        /// <param name="retryResult">An optional predicate function that determines if the result should trigger a retry (defaults to always return true if not provided).</param>
        /// <param name="retryCount">The number of retry attempts to make (default is 3).</param>
        /// <param name="retryDelay">An optional function that defines the delay between retries (defaults to a jitter backoff policy if not provided).</param>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation that, on completion, contains the result of the delegate if successful.</returns>
        /// <exception cref="TException">Thrown if all retry attempts fail due to an exception.</exception>
        /// <exception cref="Exception">Thrown if all retry attempts fail due to result validation via the retryResult predicate.</exception>
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
