using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace UCode.Apis.CnpjJa
{
    public partial class Client
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        private const string JsonMediaType = "application/json";
        private const string PdfMediaType = "application/pdf";

        public Client(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private string _baseUrl = "https://api.cnpja.com";
        private HttpClient _httpClient;

        public Client(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string BaseUrl
        {
            get => _baseUrl;
            set => _baseUrl = value;
        }

        private HttpClient GetHttpClient() => _httpClientFactory?.CreateClient() ?? _httpClient;

        private string BuildUrl(string endpoint, Dictionary<string, string> parameters = null)
        {
            var builder = new StringBuilder(BaseUrl.TrimEnd('/')).Append(endpoint);
            if (parameters != null && parameters.Count > 0)
            {
                builder.Append("?");
                builder.Append(string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}")));
            }
            return builder.ToString();
        }

        private HttpRequestMessage CreateHttpRequest(string method, string url, string authorization, string mediaType = JsonMediaType)
        {
            if (string.IsNullOrWhiteSpace(authorization))
                throw new ArgumentNullException(nameof(authorization));

            var request = new HttpRequestMessage(new HttpMethod(method), url);
            request.Headers.TryAddWithoutValidation("Authorization", authorization);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
            return request;
        }

        private async Task<T> ProcessResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            var statusCode = (int)response.StatusCode;
            if (statusCode == 200 || statusCode == 206)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return JsonSerializer.Deserialize<T>(responseContent, _jsonSerializerOptions);
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new ApiException($"Unexpected status code", statusCode, errorContent);
        }

        public virtual async Task<ZipDto> ZipCodeAsync(string code, string authorization, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(code, nameof(code));

            var url = BuildUrl($"/zip/{Uri.EscapeDataString(code)}");
            using var request = CreateHttpRequest("GET", url, authorization);
            using var client = GetHttpClient();
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return await ProcessResponseAsync<ZipDto>(response, cancellationToken);
        }

        public virtual async Task<SuframaDto> SuframaAsync(string taxId, string authorization, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(taxId, nameof(taxId));

            var url = BuildUrl("/suframa", new Dictionary<string, string> { { "taxId", taxId } });
            using var request = CreateHttpRequest("GET", url, authorization);
            using var client = GetHttpClient();
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return await ProcessResponseAsync<SuframaDto>(response, cancellationToken);
        }

        public virtual async Task<FileResponse> SuframaCertificateAsync(string taxId, string authorization, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(taxId, nameof(taxId));

            var url = BuildUrl("/suframa/certificate", new Dictionary<string, string> { { "taxId", taxId } });
            using var request = CreateHttpRequest("GET", url, authorization, PdfMediaType);
            using var client = GetHttpClient();
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if ((int)response.StatusCode == 200 || (int)response.StatusCode == 206)
            {
                var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                return new FileResponse((int)response.StatusCode, new Dictionary<string, IEnumerable<string>>((IEnumerable<KeyValuePair<string, IEnumerable<string>>>) response.Headers), responseStream, null, response);
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new ApiException($"Unexpected status code", (int)response.StatusCode, errorContent);
        }

        public virtual async Task<SimplesDto> CnpjSimplesMeiAsync(string taxId, string authorization, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(taxId, nameof(taxId));

            var url = BuildUrl("/simples", new Dictionary<string, string> { { "taxId", taxId } });
            using var request = CreateHttpRequest("GET", url, authorization);
            using var client = GetHttpClient();
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return await ProcessResponseAsync<SimplesDto>(response, cancellationToken);
        }

        public virtual async Task<SimplesDto> CnpjSimplesMeiAsync(string taxId, bool? history, Strategy? strategy, double? maxAge, double? maxStale, bool? sync, string authorization, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(taxId, nameof(taxId));

            var parameters = new Dictionary<string, string>
            {
                { "taxId", taxId }
            };

            if (history.HasValue)
                parameters["history"] = history.Value.ToString();
            if (strategy.HasValue)
                parameters["strategy"] = strategy.Value.ToString();
            if (maxAge.HasValue)
                parameters["maxAge"] = maxAge.Value.ToString();
            if (maxStale.HasValue)
                parameters["maxStale"] = maxStale.Value.ToString();
            if (sync.HasValue)
                parameters["sync"] = sync.Value.ToString();

            var url = BuildUrl("/simples", parameters);
            using var request = CreateHttpRequest("GET", url, authorization);
            using var client = GetHttpClient();
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return await ProcessResponseAsync<SimplesDto>(response, cancellationToken);
        }

        public virtual async Task<RfbDto> RfbAsync(string taxId, Strategy? strategy, double? maxAge, double? maxStale, bool? sync, string authorization, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(taxId, nameof(taxId));

            var parameters = new Dictionary<string, string>
            {
                { "taxId", taxId }
            };

            if (strategy.HasValue)
                parameters["strategy"] = strategy.Value.ToString();
            if (maxAge.HasValue)
                parameters["maxAge"] = maxAge.Value.ToString();
            if (maxStale.HasValue)
                parameters["maxStale"] = maxStale.Value.ToString();
            if (sync.HasValue)
                parameters["sync"] = sync.Value.ToString();

            var url = BuildUrl("/rfb", parameters);
            using var request = CreateHttpRequest("GET", url, authorization);
            using var client = GetHttpClient();
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return await ProcessResponseAsync<RfbDto>(response, cancellationToken);
        }

        public virtual async Task<PersonDto> PersonAsync(Guid personId, string authorization, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(personId, nameof(personId));

            var url = BuildUrl($"/person/{Uri.EscapeDataString(personId.ToString())}");
            using var request = CreateHttpRequest("GET", url, authorization);
            using var client = GetHttpClient();
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return await ProcessResponseAsync<PersonDto>(response, cancellationToken);
        }

        public virtual async Task<PersonPageDto> PersonAsync(string token, double? limit, IEnumerable<PersonType> type_in, IEnumerable<string> name_in, IEnumerable<string> name_nin, IEnumerable<string> taxId_in, IEnumerable<Age> age_in, IEnumerable<double> country_id_in, IEnumerable<double> country_id_nin, string authorization, CancellationToken cancellationToken)
        {
            var parameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(token))
                parameters["token"] = token;
            if (limit.HasValue)
                parameters["limit"] = limit.Value.ToString();

            void AddEnumerableParameter<T>(string key, IEnumerable<T> values)
            {
                if (values != null)
                {
                    foreach (var item in values)
                    {
                        parameters[$"{key}"] = item.ToString();
                    }
                }
            }

            AddEnumerableParameter("type.in", type_in);
            AddEnumerableParameter("name.in", name_in);
            AddEnumerableParameter("name.nin", name_nin);
            AddEnumerableParameter("taxId.in", taxId_in);
            AddEnumerableParameter("age.in", age_in);
            AddEnumerableParameter("country.id.in", country_id_in);
            AddEnumerableParameter("country.id.nin", country_id_nin);

            var url = BuildUrl("/person", parameters);
            using var request = CreateHttpRequest("GET", url, authorization);
            using var client = GetHttpClient();
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return await ProcessResponseAsync<PersonPageDto>(response, cancellationToken);
        }

        public virtual async Task<OfficeDto> CnpjAsync(string taxId, bool? simples, bool? simplesHistory, IEnumerable<UF> registrations, bool? suframa, bool? geocoding, IEnumerable<CertificateType>? links, Strategy? strategy, double? maxAge, double? maxStale, bool? sync, string authorization, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(taxId, nameof(taxId));

            var parameters = new Dictionary<string, string>
            {
                { "taxId", taxId }
            };

            if (simples.HasValue)
                parameters["simples"] = simples.Value.ToString();
            if (simplesHistory.HasValue)
                parameters["simplesHistory"] = simplesHistory.Value.ToString();
            if (suframa.HasValue)
                parameters["suframa"] = suframa.Value.ToString();
            if (geocoding.HasValue)
                parameters["geocoding"] = geocoding.Value.ToString();
            if (strategy.HasValue)
                parameters["strategy"] = strategy.Value.ToString();
            if (maxAge.HasValue)
                parameters["maxAge"] = maxAge.Value.ToString();
            if (maxStale.HasValue)
                parameters["maxStale"] = maxStale.Value.ToString();
            if (sync.HasValue)
                parameters["sync"] = sync.Value.ToString();

            void AddEnumerableParameter<T>(string key, IEnumerable<T> values)
            {
                if (values != null)
                {
                    foreach (var item in values)
                    {
                        parameters[$"{key}"] = item.ToString();
                    }
                }
            }

            AddEnumerableParameter("registrations", registrations);
            AddEnumerableParameter("links", links);

            var url = BuildUrl($"/office/{Uri.EscapeDataString(taxId)}", parameters);
            using var request = CreateHttpRequest("GET", url, authorization);
            using var client = GetHttpClient();
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return await ProcessResponseAsync<OfficeDto>(response, cancellationToken);
        }

        public virtual async Task<OfficePageDto> CnpjAsync(string token, double? limit, IEnumerable<string> names_in, IEnumerable<string> names_nin, IEnumerable<string> company_name_in, IEnumerable<string> company_name_nin, float? company_equity_gte, float? company_equity_lte, IEnumerable<double> company_nature_id_in, IEnumerable<double> company_nature_id_nin, IEnumerable<double> company_size_id_in, bool? company_simples_optant_eq, string company_simples_since_gte, string company_simples_since_lte, bool? company_simei_optant_eq, string company_simei_since_gte, string company_simei_since_lte, IEnumerable<string> taxId_nin, IEnumerable<string> alias_in, IEnumerable<string> alias_nin, string founded_gte, string founded_lte, bool? head_eq, string statusDate_gte, string statusDate_lte, IEnumerable<double> status_id_in, IEnumerable<double> reason_id_in, string specialDate_gte, string specialDate_lte, IEnumerable<double> special_id_in, IEnumerable<double> address_municipality_in, IEnumerable<double> address_municipality_nin, IEnumerable<string> address_district_in, IEnumerable<string> address_district_nin, IEnumerable<UF> address_state_in, IEnumerable<string> address_zip_in, string address_zip_gte, string address_zip_lte, IEnumerable<double> address_country_id_in, IEnumerable<double> address_country_id_nin, bool? phones_ex, IEnumerable<PhonesType> phones_type_in, IEnumerable<string> phones_area_in, string phones_area_gte, string phones_area_lte, bool? emails_ex, IEnumerable<EmailOwnership> emails_ownership_in, IEnumerable<string> emails_domain_in, IEnumerable<string> emails_domain_nin, IEnumerable<double> activities_id_in, IEnumerable<double> activities_id_nin, IEnumerable<double> mainActivity_id_in, IEnumerable<double> mainActivity_id_nin, IEnumerable<double> sideActivities_id_in, IEnumerable<double> sideActivities_id_nin, string authorization, CancellationToken cancellationToken)
        {
            var parameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(token))
                parameters["token"] = token;
            if (limit.HasValue)
                parameters["limit"] = limit.Value.ToString();
            if (company_equity_gte.HasValue)
                parameters["company.equity.gte"] = company_equity_gte.Value.ToString();
            if (company_equity_lte.HasValue)
                parameters["company.equity.lte"] = company_equity_lte.Value.ToString();
            if (company_simples_optant_eq.HasValue)
                parameters["company.simples.optant.eq"] = company_simples_optant_eq.Value.ToString();
            if (!string.IsNullOrEmpty(company_simples_since_gte))
                parameters["company.simples.since.gte"] = company_simples_since_gte;
            if (!string.IsNullOrEmpty(company_simples_since_lte))
                parameters["company.simples.since.lte"] = company_simples_since_lte;
            if (company_simei_optant_eq.HasValue)
                parameters["company.simei.optant.eq"] = company_simei_optant_eq.Value.ToString();
            if (!string.IsNullOrEmpty(company_simei_since_gte))
                parameters["company.simei.since.gte"] = company_simei_since_gte;
            if (!string.IsNullOrEmpty(company_simei_since_lte))
                parameters["company.simei.since.lte"] = company_simei_since_lte;
            if (!string.IsNullOrEmpty(founded_gte))
                parameters["founded.gte"] = founded_gte;
            if (!string.IsNullOrEmpty(founded_lte))
                parameters["founded.lte"] = founded_lte;
            if (head_eq.HasValue)
                parameters["head.eq"] = head_eq.Value.ToString();
            if (!string.IsNullOrEmpty(statusDate_gte))
                parameters["statusDate.gte"] = statusDate_gte;
            if (!string.IsNullOrEmpty(statusDate_lte))
                parameters["statusDate.lte"] = statusDate_lte;
            if (!string.IsNullOrEmpty(specialDate_gte))
                parameters["specialDate.gte"] = specialDate_gte;
            if (!string.IsNullOrEmpty(specialDate_lte))
                parameters["specialDate.lte"] = specialDate_lte;
            if (!string.IsNullOrEmpty(address_zip_gte))
                parameters["address.zip.gte"] = address_zip_gte;
            if (!string.IsNullOrEmpty(address_zip_lte))
                parameters["address.zip.lte"] = address_zip_lte;

            void AddEnumerableParameter<T>(string key, IEnumerable<T> values)
            {
                if (values != null)
                {
                    foreach (var item in values)
                    {
                        parameters[$"{key}"] = item.ToString();
                    }
                }
            }

            AddEnumerableParameter("names.in", names_in);
            AddEnumerableParameter("names.nin", names_nin);
            AddEnumerableParameter("company.name.in", company_name_in);
            AddEnumerableParameter("company.name.nin", company_name_nin);
            AddEnumerableParameter("company.nature.id.in", company_nature_id_in);
            AddEnumerableParameter("company.nature.id.nin", company_nature_id_nin);
            AddEnumerableParameter("company.size.id.in", company_size_id_in);
            AddEnumerableParameter("taxId.nin", taxId_nin);
            AddEnumerableParameter("alias.in", alias_in);
            AddEnumerableParameter("alias.nin", alias_nin);
            AddEnumerableParameter("status.id.in", status_id_in);
            AddEnumerableParameter("reason.id.in", reason_id_in);
            AddEnumerableParameter("special.id.in", special_id_in);
            AddEnumerableParameter("address.municipality.in", address_municipality_in);
            AddEnumerableParameter("address.municipality.nin", address_municipality_nin);
            AddEnumerableParameter("address.district.in", address_district_in);
            AddEnumerableParameter("address.district.nin", address_district_nin);
            AddEnumerableParameter("address.state.in", address_state_in);
            AddEnumerableParameter("address.zip.in", address_zip_in);
            AddEnumerableParameter("address.country.id.in", address_country_id_in);
            AddEnumerableParameter("address.country.id.nin", address_country_id_nin);
            AddEnumerableParameter("phones.type.in", phones_type_in);
            AddEnumerableParameter("phones.area.in", phones_area_in);
            AddEnumerableParameter("emails.ownership.in", emails_ownership_in);
            AddEnumerableParameter("emails.domain.in", emails_domain_in);
            AddEnumerableParameter("emails.domain.nin", emails_domain_nin);
            AddEnumerableParameter("activities.id.in", activities_id_in);
            AddEnumerableParameter("activities.id.nin", activities_id_nin);
            AddEnumerableParameter("mainActivity.id.in", mainActivity_id_in);
            AddEnumerableParameter("mainActivity.id.nin", mainActivity_id_nin);
            AddEnumerableParameter("sideActivities.id.in", sideActivities_id_in);
            AddEnumerableParameter("sideActivities.id.nin", sideActivities_id_nin);

            var url = BuildUrl("/office", parameters);
            using var request = CreateHttpRequest("GET", url, authorization);
            using var client = GetHttpClient();
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return await ProcessResponseAsync<OfficePageDto>(response, cancellationToken);
        }

        public virtual async Task<CompanyDto> CompanyAsync(double companyId, string authorization, CancellationToken cancellationToken)
        {
            var url = BuildUrl($"/company/{Uri.EscapeDataString(companyId.ToString())}");
            using var request = CreateHttpRequest("GET", url, authorization);
            using var client = GetHttpClient();
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return await ProcessResponseAsync<CompanyDto>(response, cancellationToken);
        }


    }
}
