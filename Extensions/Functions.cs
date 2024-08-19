using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UCode.Extensions.FederalCode;

namespace UCode.Extensions
{
    public static class Functions
    {



        public static string GetRoot(this Uri uri)
        {
            var url = uri.AbsoluteUri;
            var pos = url.IndexOf(uri.AbsolutePath, StringComparison.Ordinal);

            return url.Remove(pos, url.Length - pos);
        }

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
        ///     Copy properties and fields
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="source">Source object for copy properties and fields</param>
        /// <param name="destination">Destination object for copy properties and fields</param>
        /// <returns>based destination</returns>
        public static T CopyTo<T>(this T source, T destination = default) => CopyTo<T, T>(source, destination);

        /// <summary>
        ///     Copy properties and fields with same name, ignoring errors.
        /// </summary>
        /// <returns></returns>
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


        [Obsolete("Mudar o nome de \"UCode.Extensions.Functions.IsCNPJ\" para \"UCode.Extensions.FederalCode.BR.IsCnpj\".",
            true)]
        public static bool IsCNPJ(this string cnpj) => cnpj.IsCnpj();

        [Obsolete("Mudar o nome de \"UCode.Extensions.Functions.IsCPF\" para \"UCode.Extensions.FederalCode.BR.IsCpf\".",
            true)]
        public static bool IsCPF(this string cpf) => cpf.IsCpf();

        #region RetryIfException

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


        public static void RetryIfException(this Action[] actions, int retry) => RetryIfException(retry, actions);

        public static async Task RetryIfExceptionAsync(this Task[] actions, int retry) => await RetryIfExceptionAsync(retry, actions);

        public static async Task<T[]> RetryIfExceptionAsync<T>(this Task<T>[] actions, int retry) => await RetryIfExceptionAsync(retry, actions);


        public static void RetryIfException(int retry, params Action[] actions)
        {
            foreach (var action in actions)
            {
                action.RetryIfException(retry);
            }
        }

        public static async Task RetryIfExceptionAsync(int retry, params Task[] actions) => await Task.WhenAll(actions.Select(s => s.RetryIfExceptionAsync(retry)));

        public static async Task<T[]> RetryIfExceptionAsync<T>(int retry, params Task<T>[] actions) => await Task.WhenAll(actions.Select(s => s.RetryIfExceptionAsync(retry)));

        #endregion RetryIfException
    }
}
