using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UCode.Extensions;

namespace UCode.Scrapper
{
    public class ClientHttpHandler : IClientHttpHandler
    {
        [NotNull]
        private readonly HttpClientHandler _httpClientHandler;

        [NotNull]
        private readonly HttpClient _httpClient;

        [NotNull]
        private readonly List<ResultSnapshot> _requestResponseSnapshots;

        [NotNull]
        private int _currentRequests;

        [NotNull]
        private readonly SemaphoreSlim _semaphore = new(1, 1);


        private static HttpClientHandler GetHttpClientHandler(bool useCookies, bool allowAutoRedirect) => new()
        {
            CookieContainer = new CookieContainer(),
            UseCookies = useCookies,
            AllowAutoRedirect = allowAutoRedirect
        };


        /// <summary>
        /// Constructor for http client
        /// </summary>
        /// <param name="useCookies"></param>
        /// <param name="allowAutoRedirect"></param>
        /// <param name="maxRequests"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ClientHttpHandler(bool useCookies = true, bool allowAutoRedirect = true, int maxRequests = int.MaxValue)
        {
            if (maxRequests <= 0)
            {
                throw new ArgumentOutOfRangeException($"maxRequests must be greater than 0.");
            }

            if (maxRequests < int.MaxValue)
            {
                this._requestResponseSnapshots = new List<ResultSnapshot>(maxRequests);
            }
            else
            {
                this._requestResponseSnapshots = new List<ResultSnapshot>();
            }


            this.MaxRequests = maxRequests;

            
            this._httpClientHandler = GetHttpClientHandler(useCookies, allowAutoRedirect);

            this._httpClient = new HttpClient(this._httpClientHandler, true);

        }

        /// <summary>
        /// Proxy for requests
        /// </summary>
        public IWebProxy? Proxy
        {
            get => this._httpClientHandler.Proxy;
            set => this._httpClientHandler.Proxy = value;
        }


        /// <summary>
        /// Base address for requests
        /// </summary>
        public Uri? BaseAddress
        {
            get => this._httpClient.BaseAddress;
            set => this._httpClient.BaseAddress = value;
        }

        /// <summary>
        /// Use cookies in request
        /// </summary>
        public bool UseCookies
        {
            get => this._httpClientHandler.UseCookies;
        }

        /// <summary>
        /// Allow request redirect
        /// </summary>
        public bool AllowAutoRedirect => this._httpClientHandler.AllowAutoRedirect;

        /// <summary>
        /// Last request Uri
        /// </summary>
        public Uri? RequestUri => this._requestResponseSnapshots?.LastOrDefault().Request?.RequestUri;

        /// <summary>
        /// Last response Uri
        /// </summary>
        public Uri? ResponseUri => this._requestResponseSnapshots?.LastOrDefault().Response?.RequestMessage?.RequestUri;

        /// <summary>
        /// Timeout for requests
        /// </summary>
        [NotNull]
        public TimeSpan Timeout
        {
            get => this._httpClient.Timeout;
            set => this._httpClient.Timeout = value;
        }

        /// <summary>
        /// Last request/response elapsed time
        /// </summary>
        [MaybeNull]
        public TimeSpan? Elapsed => this._requestResponseSnapshots.Count > 0 ? this._requestResponseSnapshots.Last().Elapsed : TimeSpan.Zero;

        /// <summary>
        /// Total elapsed time for all requests
        /// </summary>
        [MaybeNull]
        public TimeSpan? TotalElapsed => this._requestResponseSnapshots.Count > 0 ? new TimeSpan(this._requestResponseSnapshots.Sum(s => s.Elapsed?.Ticks ?? 0)) : TimeSpan.Zero;

        /// <summary>
        /// Current request count
        /// </summary>
        [NotNull]
        public int CurrentRequests => this._currentRequests;

        /// <summary>
        /// Max of allowed request
        /// </summary>
        [NotNull]
        public int MaxRequests
        {
            get;
        }


        /// <summary>
        /// Request and Response Snapshots
        /// </summary>
        public IEnumerable<ResultSnapshot> RequestResponseSnapshots => this._requestResponseSnapshots;

        /// <summary>
        /// Cookie container reference
        /// </summary>
        public CookieContainer CookieContainer => this._httpClientHandler.CookieContainer;

        /// <summary>
        /// Event handler for http responses
        /// </summary>
        public event EventHandler<ClientHttpResponseEventArgs>? ResponseEventHandler;

        /// <summary>
        /// Event handler for exceptions
        /// </summary>
        public event EventHandler<ClientHttpExceptionEventArgs>? ExceptionEventHandler;

        /// <summary>
        /// Call attached events in ResponseEventHandler
        /// </summary>
        /// <param name="stopwatch"></param>
        /// <param name="httpResponseMessage"></param>
        /// <param name="cookies"></param>
        protected virtual void OnResponseEvent(TimeSpan elapsed, ResultSnapshot resultSnapshot)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // Event will be null if there are no subscribers
                // Call to raise the event.
                ResponseEventHandler?.Invoke(this, new ClientHttpResponseEventArgs(elapsed, resultSnapshot));
            }
            catch (Exception ex)
            {
                this.OnExceptionEvent("Error in event call ResponseEventHandler", ex, stopwatch.Elapsed, resultSnapshot);
            }
            finally
            {

                stopwatch.Stop();
            }
        }

        /// <summary>
        /// Call attached events in ExceptionEventHandler
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="elapsed"></param>
        /// <param name="httpRequestMessage"></param>
        /// <param name="httpResponseMessage"></param>
        /// <param name="cookies"></param>
        protected virtual void OnExceptionEvent(string message, Exception? exception, TimeSpan? elapsed, ResultSnapshot? resultSnapshot) =>
            // Event will be null if there are no subscribers
            // Call to raise the event.
            ExceptionEventHandler?.Invoke(this, new ClientHttpExceptionEventArgs(message, exception, elapsed, resultSnapshot));


        public HttpCompletionOption CompletionOption
        {
            get; set;
        } = HttpCompletionOption.ResponseHeadersRead;

        [return: NotNull]
        public async Task<ResultSnapshot> SendAsync([NotNull] HttpMethod httpMethod, [NotNull] string relativeOrAbsoluteUri,
            HttpContent? httpContent = null, HttpHeaders? httpHeaders = null)
        {
            if (this._currentRequests >= this.MaxRequests)
            {
                var ex = new RequestLimitException(this.MaxRequests);

                this.OnExceptionEvent("Error in SendAsync", ex, null, null);

                throw ex;
            }

            #region Uri
            Uri? uri = null;
            if (Uri.IsWellFormedUriString(relativeOrAbsoluteUri, UriKind.Relative))
            {
                if (this.BaseAddress == null)
                {
                    var ex = new ArgumentNullException($"Relative url is not supported, the base address is null for compose full address. Base:{this.BaseAddress} Address: {relativeOrAbsoluteUri}", nameof(this.BaseAddress));

                    this.OnExceptionEvent("Error in SendAsync", ex, null, null);

                    throw ex;
                }

                uri = new Uri(this.BaseAddress, relativeOrAbsoluteUri);
            }
            else
            {
                uri = new Uri(relativeOrAbsoluteUri);
            }
            #endregion Uri

            // Increment request counter
            Interlocked.Increment(ref this._currentRequests);

            // Wait for semaphore
            await this._semaphore.WaitAsync();

            // Start timer
            var timer = Stopwatch.StartNew();

            ResultSnapshot? resultSnapshot = null;
            HttpRequestMessage? httpRequestMessage = null;
            HttpResponseMessage? httpResponseMessage = null;
            IEnumerable<Cookie>? cookies = null;
            try
            {
                httpRequestMessage = new HttpRequestMessage(httpMethod, uri);

                if (httpContent != null)
                {
                    httpRequestMessage.Content = httpContent;
                }

                if (httpHeaders != null)
                {
                    foreach (var header in httpHeaders)
                    {
                        if (header.Key.Equals("Content-Type", StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }
                        //if ((httpRequestMessage.Content?.Headers?.Count() ?? 0) > 0)
                        //{
                        //    var contentHeader = httpRequestMessage.Content!.Headers;
                        //    var contentHeaderCount = contentHeader.Count();
                        //    var contentHeaderHeaders = contentHeader.ToArray();



                        //    if (contentHeader.Contains(header.Key))
                        //    {
                        //        contentHeader.Remove(header.Key);
                        //    }

                        //    contentHeader.Add(header.Key, header.Value);


                        //}



                        try
                        {

                            //if (httpRequestMessage.Content != null && httpRequestMessage.Content!.Headers.Contains(header.Key))
                            //{
                            //    httpRequestMessage.Content!.Headers.Remove(header.Key);

                            //    httpRequestMessage.Content!.Headers.Add(header.Key, header.Value);
                            //}
                            //else
                            //{
                            //if (httpRequestMessage.Headers.Contains(header.Key))
                            //{
                            //    httpRequestMessage.Headers.Remove(header.Key);
                            //}

                            //httpRequestMessage.Headers.Add(header.Key, header.Value);
                            //}

                            if (httpRequestMessage.Headers.Contains(header.Key))
                            {
                                httpRequestMessage.Headers.Remove(header.Key);
                            }

                            httpRequestMessage.Headers.Add(header.Key, header.Value);
                        }
                        catch { }
                    }
                }

                //httpRequestMessage.Headers.Add("Host", "test.com");
                //httpRequestMessage.Headers.Add("Connection", "keep-alive");
                //httpRequestMessage.Headers.Add("Content-Length", "138");
                //httpRequestMessage.Headers.Add("Pragma", "no-cache");
                //httpRequestMessage.Headers.Add("Cache-Control", "no-cache");
                //httpRequestMessage.Headers.Add("Origin", "test.com");
                //httpRequestMessage.Headers.Add("Upgrade-Insecure-Requests", "1");
                //httpRequestMessage.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                //httpRequestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36");
                //httpRequestMessage.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                //httpRequestMessage.Headers.Add("Referer", "http://www.translationdirectory.com/");
                //httpRequestMessage.Headers.Add("Accept-Encoding", "gzip, deflate");
                //httpRequestMessage.Headers.Add("Accept-Language", "en-GB,en-US;q=0.9,en;q=0.8");
                //httpRequestMessage.Headers.Add("Cookie", "__utmc=266643403; __utmz=266643403.1537352460.3.3.utmccn=(referral)|utmcsr=google.co.uk|utmcct=/|utmcmd=referral; __utma=266643403.817561753.1532012719.1537357162.1537361568.5; __utmb=266643403; __atuvc=0%7C34%2C0%7C35%2C0%7C36%2C0%7C37%2C48%7C38; __atuvs=5ba2469fbb02458f002");


                //public delegate bool RetryException<in TException>(TException exception) where TException : Exception;

                httpResponseMessage = await (this._httpClient.SendAsync(httpRequestMessage, this.CompletionOption)).RetryAsync((Exception exception) =>
                {
                    return false;
                },
                (HttpResponseMessage result) =>
                {
                    return false;
                });

                cookies = this.CookieContainer.Get();
            }
            catch (Exception ex)
            {
                timer.Stop();

                resultSnapshot = new ResultSnapshot(this._currentRequests, httpRequestMessage, httpResponseMessage, cookies, timer.Elapsed, ex);

                this.OnExceptionEvent("Error in SendAsync", ex, timer.Elapsed, resultSnapshot);
            }
            finally
            {
                if (timer.IsRunning)
                {
                    timer.Stop();
                }

                resultSnapshot ??= new ResultSnapshot(this._currentRequests, httpRequestMessage, httpResponseMessage, cookies, timer.Elapsed, null);

                // Add snapshot
                this._requestResponseSnapshots.Add(resultSnapshot.Value);

                // Release semaphore
                this._semaphore.Release();


                var stopwatchOnResponseEvent = Stopwatch.StartNew();
                try
                {
                    // raise event after releasing semaphore for allow new requests
                    this.OnResponseEvent(timer.Elapsed, resultSnapshot.Value);
                }
                catch (Exception ex)
                {
                    this.OnExceptionEvent("Error in event call ResponseEventHandler", ex, stopwatchOnResponseEvent.Elapsed, resultSnapshot);
                }
                finally
                {
                    stopwatchOnResponseEvent.Stop();
                }
            }

            return resultSnapshot.Value;
        }

        public async Task<ResultSnapshot> PostAsync([NotNull] string url, HttpContent httpContent,
            HttpHeaders httpHeaders = null) => await this.SendAsync(HttpMethod.Post, url, httpContent, httpHeaders);


        public async Task<ResultSnapshot> PostJsonAsync([NotNull] string url, object obj,
            HttpHeaders? httpHeaders = null)
        {
            var jsonString = obj.JsonString();
            return await this.PostStringAsync(url, jsonString, ((httpHeaders?.Contains("Content-Type") ?? false) ? httpHeaders!.GetValues("Content-Type").FirstOrDefault() : "application/json"), httpHeaders);
        }

        public async Task<ResultSnapshot> PostJsonAsync([NotNull] string url, string utf8Json,
            HttpHeaders? httpHeaders = null) => await this.PostStringAsync(url, utf8Json, ((httpHeaders?.Contains("Content-Type") ?? false) ? httpHeaders!.GetValues("Content-Type").FirstOrDefault() : "application/json"), httpHeaders);

        public async Task<ResultSnapshot> PostStringAsync([NotNull] string url, string utf8String,
            string httpContentType = null, HttpHeaders? httpHeaders = null)
        {
            var stringContent = new StringContent(utf8String, Encoding.UTF8, httpContentType);

            if (httpHeaders?.Contains("Content-Type") ?? false)
            {
                var mediaContent = new MediaTypeHeaderValue(httpHeaders!.GetValues("Content-Type").FirstOrDefault());

                stringContent = new StringContent(utf8String, mediaContent);
            }


            return await this.PostAsync(url, stringContent, httpHeaders);
        }

        public async Task<ResultSnapshot> GetAsync([NotNull] string url, HttpHeaders httpHeaders = null) => await this.SendAsync(HttpMethod.Get, url, null, httpHeaders);
        


        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this._semaphore?.Dispose();
                    this._httpClient?.Dispose();
                    this._httpClientHandler?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                this.disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Client()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
