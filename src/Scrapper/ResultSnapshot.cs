using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;

namespace UCode.Scrapper
{
    /// <summary>
    /// Request and response client snapshot.
    /// </summary>
    public readonly struct ResultSnapshot : IResultSnapshot
    {
        public ResultSnapshot(int index, [MaybeNull] HttpRequestMessage? request, [MaybeNull] HttpResponseMessage? response, [MaybeNull] IEnumerable<Cookie>? cookies, [NotNull] TimeSpan elapsed, [MaybeNull] Exception? exception)
        {
            this.Index = index;
            this.Request = request;
            this.Response = response;
            this.Cookies = cookies;
            this.Elapsed = elapsed;
            this.Exception = exception;
        }

        /// <summary>
        /// Current position
        /// </summary>
        public int Index
        {
            get;
        }

        /// <summary>
        /// Request message.
        /// </summary>
        public HttpRequestMessage? Request
        {
            get;
        }

        /// <summary>
        /// Response message.
        /// </summary>
        public HttpResponseMessage? Response
        {
            get;
        }

        /// <summary>
        /// Container for cookies.
        /// </summary>
        public IEnumerable<Cookie>? Cookies
        {
            get;
        }

        /// <summary>
        /// Gets the elapsed time since the request was sent.
        /// </summary>
        public TimeSpan? Elapsed
        {
            get;
        }

        /// <summary>
        /// Raised exception
        /// </summary>
        public Exception? Exception
        {
            get;
        }
    }
}
