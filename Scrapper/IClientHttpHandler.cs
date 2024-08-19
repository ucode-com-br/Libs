using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace UCode.Scrapper
{
    public interface IClientHttpHandler : IDisposable
    {
        bool AllowAutoRedirect
        {
            get;
        }
        Uri? BaseAddress
        {
            get;
            set;
        }
        HttpCompletionOption CompletionOption
        {
            get;
            set;
        }
        CookieContainer CookieContainer
        {
            get;
        }
        int CurrentRequests
        {
            get;
        }
        TimeSpan? Elapsed
        {
            get;
        }
        int MaxRequests
        {
            get;
        }

        IEnumerable<ResultSnapshot> RequestResponseSnapshots
        {
            get;
        }

        Uri? RequestUri
        {
            get;
        }

        Uri? ResponseUri
        {
            get;
        }

        TimeSpan Timeout
        {
            get;
            set;
        }

        TimeSpan? TotalElapsed
        {
            get;
        }

        bool UseCookies
        {
            get;
        }

        event EventHandler<ClientHttpExceptionEventArgs>? ExceptionEventHandler;

        event EventHandler<ClientHttpResponseEventArgs>? ResponseEventHandler;

        Task<ResultSnapshot> GetAsync([NotNull] string url, HttpHeaders httpHeaders = null);
        Task<ResultSnapshot> PostAsync([NotNull] string url, HttpContent httpContent, HttpHeaders httpHeaders = null);
        Task<ResultSnapshot> PostJsonAsync([NotNull] string url, object obj, HttpHeaders httpHeaders = null);
        Task<ResultSnapshot> PostJsonAsync([NotNull] string url, string utf8Json, HttpHeaders httpHeaders = null);
        Task<ResultSnapshot> PostStringAsync([NotNull] string url, string utf8String, string httpContentType = null, HttpHeaders httpHeaders = null);
        Task<ResultSnapshot> SendAsync([NotNull] HttpMethod httpMethod, [NotNull] string relativeOrAbsoluteUri, HttpContent? httpContent = null, HttpHeaders? httpHeaders = null);
    }
}
