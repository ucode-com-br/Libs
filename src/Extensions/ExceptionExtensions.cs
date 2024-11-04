using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    /// <summary>
    /// Represents a class containing methods to create and work with 
    /// try expressions in the context of expression trees. Try 
    /// expressions are used to handle exceptions in more 
    /// functional-style programming constructs.
    /// </summary>
    /// <remarks>
    /// This class provides static methods for creating try 
    /// expression trees, allowing developers to represent 
    /// try-catch-finally blocks in a syntactically 
    /// abstract manner. 
    /// </remarks>
    public static class TryExpression
    {
        #region Ignore Sync

        /// <summary>
        /// Executes a given function, ignoring any exceptions that may occur,
        /// and optionally executes a final action after the function call.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the result returned by the <paramref name="tryBody"/> function.
        /// </typeparam>
        /// <param name="tryBody">
        /// A <see cref="Func{TResult?}"/> delegate that represents the function 
        /// to be executed. Any exceptions thrown by this function will be ignored.
        /// </param>
        /// <param name="finallyBody">
        /// An optional <see cref="Action"/> delegate that will be executed after
        /// the try block, regardless of whether an exception was thrown.
        /// </param>
        /// <returns>
        /// Returns the result of the <paramref name="tryBody"/> function, or 
        /// <c>default</c> if an exception was caught. The return value can be 
        /// <c>null</c> if <typeparamref name="TResult"/> is a reference type.
        /// </returns>
        [return: MaybeNull]
        public static TResult? Ignore<TResult>([NotNull] Func<TResult?> tryBody, Action? finallyBody = null)
        {
            TResult? result = default;

            try
            {
                result = tryBody.Invoke();
            }
            catch
            {

            }
            finally
            {
                finallyBody?.Invoke();
            }

            return result;
        }

        /// <summary>
        /// Executes a specified action while ignoring any exceptions that may occur.
        /// Optionally, a final action can be executed after the try block is completed.
        /// </summary>
        /// <param name="tryBody">The action to execute that may throw exceptions.</param>
        /// <param name="finallyBody">An optional action to execute after the try block finishes.</param>
        /// <returns>
        /// Returns nothing. The return type is void.
        /// </returns>
        /// <remarks>
        /// The method uses a try-catch-finally pattern to ensure that if an exception is thrown
        /// during the execution of <paramref name="tryBody"/>, it is caught and ignored. The
        /// <paramref name="finallyBody"/> is executed regardless of whether an exception was thrown.
        /// </remarks>
        /// <exception cref="System.Exception">Any exception thrown during the execution of <paramref name="tryBody"/> is ignored.</exception>
        /// <seealso cref="System.Action"/>
        [return: MaybeNull]
        public static void Ignore([NotNull] Action tryBody, Action? finallyBody = null)
        {
            try
            {
                tryBody.Invoke();
            }
            catch
            {

            }
            finally
            {
                finallyBody?.Invoke();
            }
        }

        #endregion Ignore Sync

        #region Ignore Async Task

        /// <summary>
        /// Executes a provided asynchronous function and ignores any exceptions that may be thrown. 
        /// After the execution of the function, an optional finalization function can also be executed.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the result that the asynchronous function is expected to return.
        /// </typeparam>
        /// <param name="tryBody">
        /// The asynchronous function to execute. This function returns a Task that produces a result of type TResult.
        /// </param>
        /// <param name="finallyBody">
        /// An optional asynchronous function to execute after the try body. 
        /// This function does not take any parameters and returns a Task.
        /// </param>
        /// <returns>
        /// A Task that represents the asynchronous operation. 
        /// The result is of type TResult?, which can be null if no value is returned, 
        /// or if an exception is thrown and caught.
        /// </returns>
        [return: MaybeNull]
        public async static Task<TResult?> IgnoreAsync<TResult>([NotNull] Func<Task<TResult?>> tryBody, Func<Task>? finallyBody = null)
        {
            TResult? result = default;

            try
            {
                result = await tryBody.Invoke();
            }
            catch
            {

            }
            finally
            {
                if (finallyBody != null)
                {
                    await finallyBody.Invoke();
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously executes a specified action while ignoring any exceptions.
        /// Optionally, a final action can be provided to be executed after the main action
        /// has completed, regardless of whether it was successful or resulted in an exception.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the result produced by the function, although it is not utilized
        /// in this method since the return value of the action is ignored.
        /// </typeparam>
        /// <param name="tryBody">
        /// A function that returns a Task to be executed. Any exceptions thrown by this
        /// function will be caught and ignored.
        /// </param>
        /// <param name="finallyBody">
        /// An optional function that returns a Task to be executed after the main action
        /// has completed, regardless of its outcome. If this parameter is null, no final 
        /// action is performed.
        /// </param>
        /// <returns>
        /// A Task representing the asynchronous operation of the tryBody and finallyBody
        /// executions. The method itself does not return a value, as the return value
        /// of tryBody is ignored.
        /// </returns>
        [return: MaybeNull]
        public async static Task IgnoreAsync<TResult>([NotNull] Func<Task> tryBody, Func<Task>? finallyBody = null)
        {
            try
            {
                await tryBody.Invoke();
            }
            catch
            {

            }
            finally
            {
                if (finallyBody != null)
                {
                    await finallyBody.Invoke();
                }
            }
        }

        #endregion Ignore Async Task

        #region Ignore Async ValueTask

        /// <summary>
        /// Executes a provided asynchronous function and returns its result, 
        /// while ensuring that a final cleanup function is invoked after execution,
        /// regardless of whether the try function succeeded or failed.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the result produced by the tryBody function.
        /// </typeparam>
        /// <param name="tryBody">
        /// An asynchronous function that returns a <see cref="ValueTask{TResult?}"/> and is invoked in a try block.
        /// This function is expected to execute the main logic and may potentially throw an exception.
        /// </param>
        /// <param name="finallyBody">
        /// An optional asynchronous function that returns a <see cref="ValueTask"/>.
        /// It is invoked in the finally block to perform cleanup operations, if provided.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult?}"/> that represents the result of the tryBody function.
        /// It returns <c>null</c> if the tryBody function fails or if it did not produce a result.
        /// </returns>
        [return: MaybeNull]
        public async static ValueTask<TResult?> IgnoreAsync<TResult>([NotNull] Func<ValueTask<TResult?>> tryBody, Func<ValueTask>? finallyBody = null)
        {
            TResult? result = default;

            try
            {
                result = await tryBody.Invoke();
            }
            catch
            {

            }
            finally
            {
                if (finallyBody != null)
                {
                    await finallyBody.Invoke();
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously executes a specified action and ensures that a final action is executed,
        /// regardless of whether the first action succeeds or fails.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the <paramref name="tryBody"/> function, which is ignored in this method.</typeparam>
        /// <param name="tryBody">The asynchronous function to execute, which may yield a result of type <typeparamref name="TResult"/>.</param>
        /// <param name="finallyBody">An optional asynchronous function to execute after <paramref name="tryBody"/>,
        /// regardless of the outcome of <paramref name="tryBody"/>.</param>
        /// <returns>A <see cref="ValueTask"/> representing the completion of the asynchronous operation.</returns>
        /// <remarks>
        /// This method is useful for executing a block of code that may throw exceptions while ensuring that cleanup code is always run.
        /// The <paramref name="finallyBody"/> is optional and can be omitted if no additional action is needed after <paramref name="tryBody"/>.
        /// </remarks>
        [return: MaybeNull]
        public async static ValueTask IgnoreAsync<TResult>([NotNull] Func<ValueTask> tryBody, Func<ValueTask>? finallyBody = null)
        {
            try
            {
                await tryBody.Invoke();
            }
            catch
            {

            }
            finally
            {
                if (finallyBody != null)
                {
                    await finallyBody.Invoke();
                }
            }
        }

        #endregion Ignore Async ValueTask

        #region HasException Sync

        /// <summary>
        /// Determines whether the provided function <paramref name="tryBody"/> throws an exception when invoked.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the result returned by the <paramref name="tryBody"/> function.
        /// </typeparam>
        /// <param name="tryBody">
        /// A function that is invoked to perform the intended operation which may throw an exception.
        /// It should return a result of type <typeparamref name="TResult"/>.
        /// </param>
        /// <param name="bodyResult">
        /// When this method returns, contains the value returned by <paramref name="tryBody"/> if it succeeded,
        /// or <c>null</c> if an exception was thrown.
        /// This parameter is passed uninitialized; any value it holds when the method returns is irrelevant.
        /// </param>
        /// <param name="finallyBody">
        /// An optional action to be executed after the <paramref name="tryBody"/> runs, regardless of whether it succeeded or threw an exception.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether an exception was thrown by the <paramref name="tryBody"/> function.
        /// </returns>
        [return: NotNull]
        public static bool IsThrowingException<TResult>([NotNull] Func<TResult?> tryBody, out TResult? bodyResult, Action? finallyBody = null)
        {
            var result = false;

            try
            {
                bodyResult = tryBody.Invoke();
            }
            catch
            {
                result = true;
                bodyResult = default;
            }
            finally
            {
                finallyBody?.Invoke();
            }

            return result;
        }

        /// <summary>
        /// Evaluates whether an exception is thrown during the execution of a specified action,
        /// allowing for optional finalization actions to be executed thereafter.
        /// </summary>
        /// <param name="tryBody">
        /// An action delegate that represents the body of code to be executed, which may throw an exception.
        /// This parameter cannot be null.
        /// </param>
        /// <param name="finallyBody">
        /// An optional action delegate that represents cleanup or finalization code to be executed 
        /// after the tryBody action, regardless of whether an exception was thrown.
        /// This parameter can be null.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether an exception was thrown during the execution of the tryBody action.
        /// True indicates that an exception was thrown; otherwise, false.
        /// </returns>
        [return: NotNull]
        public static bool IsThrowingException([NotNull] Action tryBody, Action? finallyBody = null)
        {
            var result = false;

            try
            {
                tryBody.Invoke();

                
            }
            catch
            {
                result = true;
            }
            finally
            {
                finallyBody?.Invoke();
            }

            return result;
        }

        #endregion IsThrowException Sync

    }
}
