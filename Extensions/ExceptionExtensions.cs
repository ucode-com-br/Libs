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
    public static class TryExpression
    {
        #region Ignore Sync

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
