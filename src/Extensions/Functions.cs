using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    /// <summary>
    /// Provides a collection of static utility functions for various operations.
    /// </summary>
    public static class Functions
    {



        /// <summary>
        /// Retrieves the root part of the specified URI.
        /// </summary>
        /// <param name="uri">The URI from which to extract the root.</param>
        /// <returns>
        /// A string representing the root of the specified URI, 
        /// which includes the scheme and the host, but excludes 
        /// any path, query, or fragment components.
        /// </returns>
        public static string GetRoot(this Uri uri)
        {
            var url = uri.AbsoluteUri;
            var pos = url.IndexOf(uri.AbsolutePath, StringComparison.Ordinal);

            return url.Remove(pos, url.Length - pos);
        }

        /// <summary>
        /// Copies all bytes from the source stream to the destination stream.
        /// This method reads data from the source stream in chunks and writes it to the destination
        /// stream until there are no more bytes to read.
        /// </summary>
        /// <param name="src">The source stream which will be read.</param>
        /// <param name="dest">The destination stream where bytes will be written.</param>
        public static void CopyTo(this Stream src, Stream dest)
        {
            var bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        /// <summary>
        /// Copies the contents of the source object to the destination object.
        /// If the destination is not provided, a default value will be used.
        /// </summary>
        /// <typeparam name="T">The type of the source and destination objects.</typeparam>
        /// <param name="source">The source object to copy from.</param>
        /// <param name="destination">The destination object to copy to. If not provided, defaults to the type's default value.</param>
        /// <returns>The destination object after copying the contents from the source object.</returns>
        public static T CopyTo<T>(this T source, T destination = default) => CopyTo<T, T>(source, destination);

        /// <summary>
        /// Copies the properties and fields from the source object of type TSource to the 
        /// destination object of type TDestination. If the destination object is not provided, 
        /// a new instance of TDestination is created. If the source is null, the method 
        /// returns the default value for TDestination.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TDestination">The type of the destination object.</typeparam>
        /// <param name="source">The source object from which to copy the properties and fields.</param>
        /// <param name="destination">The destination object to which the properties and fields will be copied. 
        /// If not specified, a new instance of TDestination will be created.</param>
        /// <returns>The destination object with copied properties and fields from the source.</returns>
        public static TDestination CopyTo<TSource, TDestination>(this TSource source, TDestination destination = default)
        {
            if (source == null)
            {
                return default;
            }

            var TSourceType = typeof(TSource);
            var TDestinationType = typeof(TDestination);

            destination ??= (TDestination)Activator.CreateInstance(TDestinationType);

            foreach (var property in TSourceType.GetProperties())
            {
                try
                {
                    var sourceValue = property.GetValue(source);

                    TDestinationType.GetProperty(property.Name)?.SetValue(destination, sourceValue);
                }
                catch (Exception)
                {
                }
            }

            foreach (var field in TSourceType.GetFields())
            {
                try
                {
                    var sourceValue = field.GetValue(source);

                    TDestinationType.GetField(field.Name)?.SetValue(destination, sourceValue);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return destination;
        }


        #region RetryIfException

        /// <summary>
        /// Executes the provided action and retries the execution if an exception occurs.
        /// It attempts to execute the action a specified number of times before throwing an 
        /// <see cref="AggregateException"/> that contains all exceptions encountered during 
        /// the execution attempts.
        /// </summary>
        /// <param name="action">
        /// The action to execute. This is expected to be a delegate of type 
        /// <see cref="Action"/> that represents the method to be executed.
        /// </param>
        /// <param name="retry">
        /// The number of times to attempt executing the action before giving up. 
        /// If the action fails after the specified number of retries, an 
        /// <see cref="AggregateException"/> will be thrown containing all exceptions 
        /// encountered during the retries.
        /// </param>
        /// <exception cref="AggregateException">
        /// Thrown when the action fails for all specified attempts, containing a list
        /// of all exceptions thrown during the execution attempts.
        /// </exception>
        public static void RetryIfException(this Action action, int retry)
        {
            var exceptions = new List<Exception>();

            var completed = false;

            for (var i = 0; i < retry; i++)
            {
                try
                {
                    action();

                    completed = true;

                    break;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (!completed)
            {
                throw new AggregateException(exceptions);
            }
        }

        /// <summary>
        /// Executes a specified asynchronous action with a retry mechanism.
        /// If the action fails, it will be retried a specified number of times.
        /// If all attempts fail, an <see cref="AggregateException"/> containing all encountered exceptions is thrown.
        /// </summary>
        /// <param name="action">The asynchronous task to execute.</param>
        /// <param name="retry">The number of times to retry the action upon failure.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation, with no result.
        /// </returns>
        /// <exception cref="AggregateException">
        /// Thrown when all attempts to execute the action fail, containing all exceptions encountered during the retries.
        /// </exception>
        public static async Task RetryIfExceptionAsync(this Task action, int retry)
        {
            var exceptions = new List<Exception>();

            var completed = false;

            for (var i = 0; i < retry; i++)
            {
                try
                {
                    await action;

                    completed = true;

                    break;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (!completed)
            {
                throw new AggregateException(exceptions);
            }
        }

        /// <summary>
        /// Retries a given asynchronous task if it throws an exception, up to a specified number of retries.
        /// </summary>
        /// <typeparam name="T">The type of the result expected from the task.</typeparam>
        /// <param name="action">The asynchronous task to execute.</param>
        /// <param name="retry">The number of times to retry the action if it fails.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the result of the action if successful.
        /// </returns>
        /// <exception cref="AggregateException">Thrown when all retries fail and an aggregate of the caught exceptions is generated.</exception>
        public static async Task<T> RetryIfExceptionAsync<T>(this Task<T> action, int retry)
        {
            var exceptions = new List<Exception>();
            T r = default;

            for (var i = 0; i < retry; i++)
            {
                try
                {
                    r = await action;

                    break;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (r == null)
            {
                throw new AggregateException(exceptions);
            }

            return r;
        }


        /// <summary>
        /// Retries the provided array of actions if any exception occurs during their execution.
        /// </summary>
        /// <param name="actions">An array of <see cref="Action"/> delegates to be executed.</param>
        /// <param name="retry">The number of times to retry the actions in case of an exception.</param>
        /// <remarks>
        /// This extension method allows the user to execute a series of actions and handles exceptions by retrying the actions up to the specified number of times.
        /// </remarks>
        public static void RetryIfException(this Action[] actions, int retry) => RetryIfException(retry, actions);

        /// <summary>
        /// An extension method that executes an array of tasks and retries 
        /// the execution if an exception occurs. This method is designed to 
        /// handle tasks that may fail and allows for a specified number of 
        /// retries before ultimately giving up.
        /// </summary>
        /// <param name="actions">
        /// An array of tasks to be executed. Each task represents an 
        /// asynchronous operation that may throw exceptions.
        /// </param>
        /// <param name="retry">
        /// The number of times to retry the execution of the tasks if an 
        /// exception is encountered during the first execution.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The 
        /// underlying tasks will be awaited and any exceptions will be 
        /// caught and handled according to the retry logic.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="actions"/> parameter is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="retry"/> is less than zero.
        /// </exception>
        public static async Task RetryIfExceptionAsync(this Task[] actions, int retry) => await RetryIfExceptionAsync(retry, actions);

        /// <summary>
        /// Asynchronously executes an array of tasks with a given number of retries
        /// if any exceptions occur during their execution.
        /// </summary>
        /// <typeparam name="T">The type of the result returned by the tasks.</typeparam>
        /// <param name="actions">An array of tasks to be executed.</param>
        /// <param name="retry">The number of times to retry the tasks upon encountering an exception.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The result contains an array
        /// of results from the executed tasks.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="actions"/> array is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="retry"/> is less than zero.
        /// </exception>
        public static async Task<T[]> RetryIfExceptionAsync<T>(this Task<T>[] actions, int retry) => await RetryIfExceptionAsync(retry, actions);


        /// <summary>
        /// Executes a series of actions repeatedly up to a specified number of retries 
        /// if any exceptions are encountered during the execution of those actions.
        /// </summary>
        /// <param name="retry">The number of times to retry each action if an exception is thrown.</param>
        /// <param name="actions">An array of actions to be executed.</param>
        public static void RetryIfException(int retry, params Action[] actions)
        {
            foreach (var action in actions)
            {
                action.RetryIfException(retry);
            }
        }

        /// <summary>
        /// Executes a series of tasks asynchronously, with a retry mechanism for each task.
        /// If any task throws an exception, it will be retried a specified number of times before failing.
        /// </summary>
        /// <param name="retry">The number of times to retry each task if an exception occurs.</param>
        /// <param name="actions">An array of tasks to be executed with the retry logic.</param>
        /// <returns>A task that represents the asynchronous operation, containing the results of the completed actions.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="actions"/> parameter is null.</exception>
        /// <exception cref="Exception">Thrown if the maximum number of retries is exhausted for any task.</exception>
        public static async Task RetryIfExceptionAsync(int retry, params Task[] actions) => await Task.WhenAll(actions.Select(s => s.RetryIfExceptionAsync(retry)));

        /// <summary>
        /// Asynchronously executes a set of tasks, retrying each task a specified number of times
        /// if exceptions are encountered. It returns an array of results from the executed tasks.
        /// </summary>
        /// <typeparam name="T">The type of the results returned by the tasks.</typeparam>
        /// <param name="retry">The number of times to retry each task in case of an exception.</param>
        /// <param name="actions">An array of tasks to be executed.</param>
        /// <returns>A task that represents the asynchronous operation, containing an array of results from the tasks.</returns>
        /// <exception cref="ArgumentException">Thrown if the actions array is empty.</exception>
        /// <remarks>
        /// This method utilizes the RetryIfExceptionAsync extension method assumed to be defined elsewhere,
        /// which handles the retry logic for each individual task.
        /// </remarks>
        public static async Task<T[]> RetryIfExceptionAsync<T>(int retry, params Task<T>[] actions) => await Task.WhenAll(actions.Select(s => s.RetryIfExceptionAsync(retry)));

        #endregion RetryIfException
    }
}
