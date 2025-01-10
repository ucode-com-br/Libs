using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Polly.Retry;
using Polly;

namespace UCode.Apis.CnpjJa
{
    public partial class Client
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public Client(string apiKey, string baseUrl)
        {
            ArgumentNullException.ThrowIfNull(apiKey);
            ArgumentNullException.ThrowIfNull(baseUrl);


            this._httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };

            this._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(apiKey);

            // Define a retry policy using Polly
            this._retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        //Console.WriteLine($"Retry {retryCount} encountered an error. Waiting {timeSpan} before retrying.");
                    });
        }

        public async Task<OfficeDto?> GetOfficeAsync(RequestOffice parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);

            ArgumentNullException.ThrowIfNull(parameters.TaxId);

            var queryString = new StringBuilder($"/office/{parameters.TaxId}?");

            if (parameters.Simples.HasValue)
                queryString.Append($"simples={parameters.Simples.Value}&");

            if (parameters.SimplesHistory.HasValue)
                queryString.Append($"simplesHistory={parameters.SimplesHistory.Value}&");

            if (parameters.Registrations != null && parameters.Registrations.Count > 0)
                queryString.Append($"registrations={string.Join(",", parameters.Registrations)}&");

            if (parameters.Suframa.HasValue)
                queryString.Append($"suframa={parameters.Suframa.Value}&");

            if (parameters.Geocoding.HasValue)
                queryString.Append($"geocoding={parameters.Geocoding.Value}&");

            if (parameters.Links != null && parameters.Links.Count > 0)
                queryString.Append($"links={string.Join(",", parameters.Links)}&");

            if (!string.IsNullOrWhiteSpace(parameters.Strategy))
                queryString.Append($"strategy={parameters.Strategy}&");

            if (parameters.MaxAge.HasValue)
                queryString.Append($"maxAge={parameters.MaxAge.Value}&");

            if (parameters.MaxStale.HasValue)
                queryString.Append($"maxStale={parameters.MaxStale.Value}&");

            if (parameters.Sync.HasValue)
                queryString.Append($"sync={parameters.Sync.Value}&");

            // Remove trailing '&' if exists
            var endpoint = queryString.ToString().TrimEnd('&').TrimEnd('?');

            var response = await this._retryPolicy.ExecuteAsync(async () =>
            {
                return await this._httpClient.GetAsync(endpoint);
            });

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {response.ReasonPhrase}");
            }

            var content = await response.Content.ReadAsStringAsync();


            return JsonSerializer.Deserialize<OfficeDto>(content);
        }

    }

}

