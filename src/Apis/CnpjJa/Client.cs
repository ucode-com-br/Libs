using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{

    public partial class Client
    {
        private string _baseUrl = "https://api.cnpja.com";
        private System.Net.Http.HttpClient _httpClient;

        public Client(System.Net.Http.HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public string BaseUrl
        {
            get
            {
                return _baseUrl;
            }
            set
            {
                _baseUrl = value;
            }
        }



        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url);
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder);
        partial void ProcessResponse(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage response);

        /// <remarks>
        /// Adquire os dados de um código de endereçamento postal, incluindo município IBGE.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Correios 🡭](https://buscacepinter.correios.com.br/app/endereco/index.php)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são previamente armazenados em cache.  
        /// <br/>• Atualizações globais a cada 15 dias.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta.
        /// </remarks>
        /// <param name="code">Código de Endereçamento Postal.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<ZipDto> ZipCodeAsync(string code, string authorization)
        {
            return ZipCodeAsync(code, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Adquire os dados de um código de endereçamento postal, incluindo município IBGE.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Correios 🡭](https://buscacepinter.correios.com.br/app/endereco/index.php)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são previamente armazenados em cache.  
        /// <br/>• Atualizações globais a cada 15 dias.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta.
        /// </remarks>
        /// <param name="code">Código de Endereçamento Postal.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<ZipDto> ZipCodeAsync([NotNull] string code, [NotNull] string authorization, System.Threading.CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(code);

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/zip/{code}");
            urlBuilder_.Replace("{code}", Uri.EscapeDataString(ConvertToString(code, System.Globalization.CultureInfo.InvariantCulture)));

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ZipDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Adquire junto à SUFRAMA os dados cadastrais do estabelecimento, situação do projeto, atividades econômicas, e incentivos fiscais.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [SUFRAMA 🡭](https://www4.suframa.gov.br/cadsuf/#/menu-externo)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados podem ser obtidos online em tempo real, ou previamente armazenados em cache.  
        /// <br/>• O modo de resolução irá respeitar a defasagem máxima fornecida em `maxAge`.  
        /// <br/>• Atualizações individuais sob demanda.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta online.  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta em cache.
        /// </remarks>
        /// <param name="taxId">Número do CNPJ ou CPF sem pontuação.</param>
        /// <param name="strategy">Estratégia de cache utilizada na aquisição dos dados:  
        /// <br/>• `CACHE`: Entrega os dados do cache, evitando cobranças de créditos, se os dados não estiverem disponíveis resultará em um erro 404.  
        /// <br/>• `CACHE_IF_FRESH`: Retorna os dados do cache respeitando o limite em `maxAge`, se os dados estiverem desatualizados será consultado online.  
        /// <br/>• `CACHE_IF_ERROR`: Idem ao `CACHE_IF_FRESH`, mas se a consulta online falhar retorna os dados do cache respeitando o limite em `maxStale`.  
        /// <br/>• `ONLINE`: Consulta diretamente online, não recomendado pois ignora qualquer cache, sugerimos configurar `maxAge=1` como alternativa.</param>
        /// <param name="maxAge">Idade máxima, em dias, que um dado em cache é aceite, relevante para as estratégias `CACHE_IF_FRESH` e `CACHE_IF_ERROR`.</param>
        /// <param name="maxStale">Idade máxima, em dias, que um dado em cache é aceite em caso de erro na busca online, relevante apenas para a estratégia `CACHE_IF_FRESH`.</param>
        /// <param name="sync">Aguarda a compensação dos créditos de forma síncrona, retornando o cabeçalho `cnpja-request-cost`.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<SuframaDto> SuframaAsync(string taxId, Strategy? strategy, double? maxAge, double? maxStale, bool? sync, string authorization)
        {
            return SuframaAsync(taxId, strategy, maxAge, maxStale, sync, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Adquire junto à SUFRAMA os dados cadastrais do estabelecimento, situação do projeto, atividades econômicas, e incentivos fiscais.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [SUFRAMA 🡭](https://www4.suframa.gov.br/cadsuf/#/menu-externo)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados podem ser obtidos online em tempo real, ou previamente armazenados em cache.  
        /// <br/>• O modo de resolução irá respeitar a defasagem máxima fornecida em `maxAge`.  
        /// <br/>• Atualizações individuais sob demanda.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta online.  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta em cache.
        /// </remarks>
        /// <param name="taxId">Número do CNPJ ou CPF sem pontuação.</param>
        /// <param name="strategy">Estratégia de cache utilizada na aquisição dos dados:  
        /// <br/>• `CACHE`: Entrega os dados do cache, evitando cobranças de créditos, se os dados não estiverem disponíveis resultará em um erro 404.  
        /// <br/>• `CACHE_IF_FRESH`: Retorna os dados do cache respeitando o limite em `maxAge`, se os dados estiverem desatualizados será consultado online.  
        /// <br/>• `CACHE_IF_ERROR`: Idem ao `CACHE_IF_FRESH`, mas se a consulta online falhar retorna os dados do cache respeitando o limite em `maxStale`.  
        /// <br/>• `ONLINE`: Consulta diretamente online, não recomendado pois ignora qualquer cache, sugerimos configurar `maxAge=1` como alternativa.</param>
        /// <param name="maxAge">Idade máxima, em dias, que um dado em cache é aceite, relevante para as estratégias `CACHE_IF_FRESH` e `CACHE_IF_ERROR`.</param>
        /// <param name="maxStale">Idade máxima, em dias, que um dado em cache é aceite em caso de erro na busca online, relevante apenas para a estratégia `CACHE_IF_FRESH`.</param>
        /// <param name="sync">Aguarda a compensação dos créditos de forma síncrona, retornando o cabeçalho `cnpja-request-cost`.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<SuframaDto> SuframaAsync(string taxId, Strategy? strategy, double? maxAge, double? maxStale, bool? sync, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (taxId == null)
                throw new System.ArgumentNullException("taxId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/suframa?");
            urlBuilder_.Append(Uri.EscapeDataString("taxId") + "=").Append(Uri.EscapeDataString(ConvertToString(taxId, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            if (strategy != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("strategy") + "=").Append(Uri.EscapeDataString(ConvertToString(strategy, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (maxAge != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("maxAge") + "=").Append(Uri.EscapeDataString(ConvertToString(maxAge, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (maxStale != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("maxStale") + "=").Append(Uri.EscapeDataString(ConvertToString(maxStale, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (sync != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("sync") + "=").Append(Uri.EscapeDataString(ConvertToString(sync, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<SuframaDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 503)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorServiceUnavailableDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorServiceUnavailableDto>("Consulta online temporariamente indispon\u00edvel.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Emite o comprovante em PDF do registro na Suframa.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [SUFRAMA 🡭](https://www4.suframa.gov.br/cadsuf/#/menu-externo)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são obtidos online em tempo real.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Exemplos&lt;/span&gt;  
        /// <br/>• [Estabelecimento Ativo](/proxy/api/assets/docs/suframa_certificate_01.pdf) .  
        /// <br/>• [Estabelecimento Bloqueado](/proxy/api/assets/docs/suframa_certificate_02.pdf).  
        /// <br/>• [Estabelecimento com Múltiplos Incentivos](/proxy/api/assets/docs/suframa_certificate_03.pdf).
        /// </remarks>
        /// <param name="taxId">Número do CNPJ ou CPF sem pontuação.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<FileResponse> SuframaAsync(string taxId, string authorization)
        {
            return SuframaAsync(taxId, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Emite o comprovante em PDF do registro na Suframa.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [SUFRAMA 🡭](https://www4.suframa.gov.br/cadsuf/#/menu-externo)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são obtidos online em tempo real.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Exemplos&lt;/span&gt;  
        /// <br/>• [Estabelecimento Ativo](/proxy/api/assets/docs/suframa_certificate_01.pdf) .  
        /// <br/>• [Estabelecimento Bloqueado](/proxy/api/assets/docs/suframa_certificate_02.pdf).  
        /// <br/>• [Estabelecimento com Múltiplos Incentivos](/proxy/api/assets/docs/suframa_certificate_03.pdf).
        /// </remarks>
        /// <param name="taxId">Número do CNPJ ou CPF sem pontuação.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<FileResponse> SuframaAsync(string taxId, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (taxId == null)
                throw new System.ArgumentNullException("taxId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/suframa/certificate?");
            urlBuilder_.Append(Uri.EscapeDataString("taxId") + "=").Append(Uri.EscapeDataString(ConvertToString(taxId, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/pdf"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200 || status_ == 206)
                        {
                            var responseStream_ = response_.Content == null ? System.IO.Stream.Null : await response_.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            var fileResponse_ = new FileResponse(status_, headers_, responseStream_, null, response_);
                            disposeClient_ = false;
                            disposeResponse_ = false; // response and client are disposed by FileResponse
                            return fileResponse_;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 503)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorServiceUnavailableDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorServiceUnavailableDto>("Consulta online temporariamente indispon\u00edvel.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Adquire junto ao Simples Nacional a opção pelo regime, enquadramento no MEI, datas de inclusão, e histórico de períodos.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Simples Nacional 🡭](https://www8.receita.fazenda.gov.br/SimplesNacional/aplicacoes.aspx?id=21)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados podem ser obtidos online em tempo real, ou previamente armazenados em cache.  
        /// <br/>• O modo de resolução irá respeitar a defasagem máxima fornecida em `maxAge`.  
        /// <br/>• Atualizações individuais sob demanda.  
        /// <br/>• Atualizações globais a cada 30 dias.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta online.  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta em cache.  
        /// <br/>• Sujeito a adicionais conforme escolhas de parâmetros.
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="history">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona o histórico de períodos
        /// <br/>anteriores do Simples e MEI.</param>
        /// <param name="strategy">Estratégia de cache utilizada na aquisição dos dados:  
        /// <br/>• `CACHE`: Entrega os dados do cache, evitando cobranças de créditos, se os dados não estiverem disponíveis resultará em um erro 404.  
        /// <br/>• `CACHE_IF_FRESH`: Retorna os dados do cache respeitando o limite em `maxAge`, se os dados estiverem desatualizados será consultado online.  
        /// <br/>• `CACHE_IF_ERROR`: Idem ao `CACHE_IF_FRESH`, mas se a consulta online falhar retorna os dados do cache respeitando o limite em `maxStale`.  
        /// <br/>• `ONLINE`: Consulta diretamente online, não recomendado pois ignora qualquer cache, sugerimos configurar `maxAge=1` como alternativa.</param>
        /// <param name="maxAge">Idade máxima, em dias, que um dado em cache é aceite, relevante para as estratégias `CACHE_IF_FRESH` e `CACHE_IF_ERROR`.</param>
        /// <param name="maxStale">Idade máxima, em dias, que um dado em cache é aceite em caso de erro na busca online, relevante apenas para a estratégia `CACHE_IF_FRESH`.</param>
        /// <param name="sync">Aguarda a compensação dos créditos de forma síncrona, retornando o cabeçalho `cnpja-request-cost`.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<SimplesDto> CnpjSimplesMeiAsync(string taxId, bool? history, Strategy? strategy, double? maxAge, double? maxStale, bool? sync, string authorization)
        {
            return CnpjSimplesMeiAsync(taxId, history, strategy, maxAge, maxStale, sync, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Adquire junto ao Simples Nacional a opção pelo regime, enquadramento no MEI, datas de inclusão, e histórico de períodos.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Simples Nacional 🡭](https://www8.receita.fazenda.gov.br/SimplesNacional/aplicacoes.aspx?id=21)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados podem ser obtidos online em tempo real, ou previamente armazenados em cache.  
        /// <br/>• O modo de resolução irá respeitar a defasagem máxima fornecida em `maxAge`.  
        /// <br/>• Atualizações individuais sob demanda.  
        /// <br/>• Atualizações globais a cada 30 dias.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta online.  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta em cache.  
        /// <br/>• Sujeito a adicionais conforme escolhas de parâmetros.
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="history">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona o histórico de períodos
        /// <br/>anteriores do Simples e MEI.</param>
        /// <param name="strategy">Estratégia de cache utilizada na aquisição dos dados:  
        /// <br/>• `CACHE`: Entrega os dados do cache, evitando cobranças de créditos, se os dados não estiverem disponíveis resultará em um erro 404.  
        /// <br/>• `CACHE_IF_FRESH`: Retorna os dados do cache respeitando o limite em `maxAge`, se os dados estiverem desatualizados será consultado online.  
        /// <br/>• `CACHE_IF_ERROR`: Idem ao `CACHE_IF_FRESH`, mas se a consulta online falhar retorna os dados do cache respeitando o limite em `maxStale`.  
        /// <br/>• `ONLINE`: Consulta diretamente online, não recomendado pois ignora qualquer cache, sugerimos configurar `maxAge=1` como alternativa.</param>
        /// <param name="maxAge">Idade máxima, em dias, que um dado em cache é aceite, relevante para as estratégias `CACHE_IF_FRESH` e `CACHE_IF_ERROR`.</param>
        /// <param name="maxStale">Idade máxima, em dias, que um dado em cache é aceite em caso de erro na busca online, relevante apenas para a estratégia `CACHE_IF_FRESH`.</param>
        /// <param name="sync">Aguarda a compensação dos créditos de forma síncrona, retornando o cabeçalho `cnpja-request-cost`.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<SimplesDto> CnpjSimplesMeiAsync(string taxId, bool? history, Strategy? strategy, double? maxAge, double? maxStale, bool? sync, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (taxId == null)
                throw new System.ArgumentNullException("taxId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/simples?");
            urlBuilder_.Append(Uri.EscapeDataString("taxId") + "=").Append(Uri.EscapeDataString(ConvertToString(taxId, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            if (history != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("history") + "=").Append(Uri.EscapeDataString(ConvertToString(history, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (strategy != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("strategy") + "=").Append(Uri.EscapeDataString(ConvertToString(strategy, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (maxAge != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("maxAge") + "=").Append(Uri.EscapeDataString(ConvertToString(maxAge, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (maxStale != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("maxStale") + "=").Append(Uri.EscapeDataString(ConvertToString(maxStale, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (sync != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("sync") + "=").Append(Uri.EscapeDataString(ConvertToString(sync, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<SimplesDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 503)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorServiceUnavailableDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorServiceUnavailableDto>("Consulta online temporariamente indispon\u00edvel.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Emite o comprovante em PDF do registro no Simples Nacional.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Simples Nacional 🡭](https://www8.receita.fazenda.gov.br/SimplesNacional/aplicacoes.aspx?id=21)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são obtidos online em tempo real.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Exemplos&lt;/span&gt;  
        /// <br/>• [Empresa Optante Simples Nacional](/proxy/api/assets/docs/simples_certificate_01.pdf).  
        /// <br/>• [Empresa Enquadrada no MEI](/proxy/api/assets/docs/simples_certificate_02.pdf).  
        /// <br/>• [Empresa Desenquadrada](/proxy/api/assets/docs/simples_certificate_03.pdf).
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<FileResponse> CnpjSimplesMeiAsync(string taxId, string authorization)
        {
            return CnpjSimplesMeiAsync(taxId, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Emite o comprovante em PDF do registro no Simples Nacional.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Simples Nacional 🡭](https://www8.receita.fazenda.gov.br/SimplesNacional/aplicacoes.aspx?id=21)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são obtidos online em tempo real.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Exemplos&lt;/span&gt;  
        /// <br/>• [Empresa Optante Simples Nacional](/proxy/api/assets/docs/simples_certificate_01.pdf).  
        /// <br/>• [Empresa Enquadrada no MEI](/proxy/api/assets/docs/simples_certificate_02.pdf).  
        /// <br/>• [Empresa Desenquadrada](/proxy/api/assets/docs/simples_certificate_03.pdf).
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<FileResponse> CnpjSimplesMeiAsync(string taxId, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (taxId == null)
                throw new System.ArgumentNullException("taxId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/simples/certificate?");
            urlBuilder_.Append(Uri.EscapeDataString("taxId") + "=").Append(Uri.EscapeDataString(ConvertToString(taxId, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/pdf"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200 || status_ == 206)
                        {
                            var responseStream_ = response_.Content == null ? System.IO.Stream.Null : await response_.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            var fileResponse_ = new FileResponse(status_, headers_, responseStream_, null, response_);
                            disposeClient_ = false;
                            disposeResponse_ = false; // response and client are disposed by FileResponse
                            return fileResponse_;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 503)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorServiceUnavailableDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorServiceUnavailableDto>("Consulta online temporariamente indispon\u00edvel.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Adquire junto à Receita Federal os dados cadastrais do estabelecimento, atividades econômicas, e quadro de sócios e administradores.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados podem ser obtidos online em tempo real, ou previamente armazenados em cache.  
        /// <br/>• O modo de resolução irá respeitar a defasagem máxima fornecida em `maxAge`.  
        /// <br/>• Atualizações individuais sob demanda.  
        /// <br/>• Atualizações globais a cada 30 dias.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta online.  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta em cache.
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="strategy">Estratégia de cache utilizada na aquisição dos dados:  
        /// <br/>• `CACHE`: Entrega os dados do cache, evitando cobranças de créditos, se os dados não estiverem disponíveis resultará em um erro 404.  
        /// <br/>• `CACHE_IF_FRESH`: Retorna os dados do cache respeitando o limite em `maxAge`, se os dados estiverem desatualizados será consultado online.  
        /// <br/>• `CACHE_IF_ERROR`: Idem ao `CACHE_IF_FRESH`, mas se a consulta online falhar retorna os dados do cache respeitando o limite em `maxStale`.  
        /// <br/>• `ONLINE`: Consulta diretamente online, não recomendado pois ignora qualquer cache, sugerimos configurar `maxAge=1` como alternativa.</param>
        /// <param name="maxAge">Idade máxima, em dias, que um dado em cache é aceite, relevante para as estratégias `CACHE_IF_FRESH` e `CACHE_IF_ERROR`.</param>
        /// <param name="maxStale">Idade máxima, em dias, que um dado em cache é aceite em caso de erro na busca online, relevante apenas para a estratégia `CACHE_IF_FRESH`.</param>
        /// <param name="sync">Aguarda a compensação dos créditos de forma síncrona, retornando o cabeçalho `cnpja-request-cost`.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<RfbDto> RfbAsync(string taxId, Strategy? strategy, double? maxAge, double? maxStale, bool? sync, string authorization)
        {
            return RfbAsync(taxId, strategy, maxAge, maxStale, sync, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Adquire junto à Receita Federal os dados cadastrais do estabelecimento, atividades econômicas, e quadro de sócios e administradores.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados podem ser obtidos online em tempo real, ou previamente armazenados em cache.  
        /// <br/>• O modo de resolução irá respeitar a defasagem máxima fornecida em `maxAge`.  
        /// <br/>• Atualizações individuais sob demanda.  
        /// <br/>• Atualizações globais a cada 30 dias.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta online.  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta em cache.
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="strategy">Estratégia de cache utilizada na aquisição dos dados:  
        /// <br/>• `CACHE`: Entrega os dados do cache, evitando cobranças de créditos, se os dados não estiverem disponíveis resultará em um erro 404.  
        /// <br/>• `CACHE_IF_FRESH`: Retorna os dados do cache respeitando o limite em `maxAge`, se os dados estiverem desatualizados será consultado online.  
        /// <br/>• `CACHE_IF_ERROR`: Idem ao `CACHE_IF_FRESH`, mas se a consulta online falhar retorna os dados do cache respeitando o limite em `maxStale`.  
        /// <br/>• `ONLINE`: Consulta diretamente online, não recomendado pois ignora qualquer cache, sugerimos configurar `maxAge=1` como alternativa.</param>
        /// <param name="maxAge">Idade máxima, em dias, que um dado em cache é aceite, relevante para as estratégias `CACHE_IF_FRESH` e `CACHE_IF_ERROR`.</param>
        /// <param name="maxStale">Idade máxima, em dias, que um dado em cache é aceite em caso de erro na busca online, relevante apenas para a estratégia `CACHE_IF_FRESH`.</param>
        /// <param name="sync">Aguarda a compensação dos créditos de forma síncrona, retornando o cabeçalho `cnpja-request-cost`.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<RfbDto> RfbAsync(string taxId, Strategy? strategy, double? maxAge, double? maxStale, bool? sync, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (taxId == null)
                throw new System.ArgumentNullException("taxId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/rfb?");
            urlBuilder_.Append(Uri.EscapeDataString("taxId") + "=").Append(Uri.EscapeDataString(ConvertToString(taxId, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            if (strategy != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("strategy") + "=").Append(Uri.EscapeDataString(ConvertToString(strategy, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (maxAge != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("maxAge") + "=").Append(Uri.EscapeDataString(ConvertToString(maxAge, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (maxStale != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("maxStale") + "=").Append(Uri.EscapeDataString(ConvertToString(maxStale, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (sync != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("sync") + "=").Append(Uri.EscapeDataString(ConvertToString(sync, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<RfbDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 503)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorServiceUnavailableDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorServiceUnavailableDto>("Consulta online temporariamente indispon\u00edvel.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Emite o comprovante em PDF do registro na Receita Federal.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são obtidos online em tempo real.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Exemplos&lt;/span&gt;  
        /// <br/>• [Estabelecimento Ativo](/proxy/api/assets/docs/rfb_certificate_01.pdf).  
        /// <br/>• [Estabelecimento Baixado](/proxy/api/assets/docs/rfb_certificate_02.pdf).  
        /// <br/>• [Estabelecimento com Múltiplos CNAEs e Sócios](/proxy/api/assets/docs/rfb_certificate_03.pdf).  
        /// <br/>• [Estabelecimento Domiciliado no Exterior](/proxy/api/assets/docs/rfb_certificate_04.pdf).
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="pages">Páginas a incluir no comprovante separadas por vírgula.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<FileResponse> RfbAsync(string taxId, System.Collections.Generic.IEnumerable<Anonymous> pages, string authorization)
        {
            return RfbAsync(taxId, pages, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Emite o comprovante em PDF do registro na Receita Federal.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são obtidos online em tempo real.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Exemplos&lt;/span&gt;  
        /// <br/>• [Estabelecimento Ativo](/proxy/api/assets/docs/rfb_certificate_01.pdf).  
        /// <br/>• [Estabelecimento Baixado](/proxy/api/assets/docs/rfb_certificate_02.pdf).  
        /// <br/>• [Estabelecimento com Múltiplos CNAEs e Sócios](/proxy/api/assets/docs/rfb_certificate_03.pdf).  
        /// <br/>• [Estabelecimento Domiciliado no Exterior](/proxy/api/assets/docs/rfb_certificate_04.pdf).
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="pages">Páginas a incluir no comprovante separadas por vírgula.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<FileResponse> RfbAsync(string taxId, System.Collections.Generic.IEnumerable<Anonymous> pages, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (taxId == null)
                throw new System.ArgumentNullException("taxId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/rfb/certificate?");
            urlBuilder_.Append(Uri.EscapeDataString("taxId") + "=").Append(Uri.EscapeDataString(ConvertToString(taxId, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            if (pages != null)
            {
                foreach (var item_ in pages)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("pages") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/pdf"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200 || status_ == 206)
                        {
                            var responseStream_ = response_.Content == null ? System.IO.Stream.Null : await response_.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            var fileResponse_ = new FileResponse(status_, headers_, responseStream_, null, response_);
                            disposeClient_ = false;
                            disposeResponse_ = false; // response and client are disposed by FileResponse
                            return fileResponse_;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 503)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorServiceUnavailableDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorServiceUnavailableDto>("Consulta online temporariamente indispon\u00edvel.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Adquire os dados de uma pessoa, incluindo todos os quadros societários dos quais faz parte.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são previamente armazenados em cache.  
        /// <br/>• Atualizações individuais sob demanda.  
        /// <br/>• Atualizações globais a cada 30 dias.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta.
        /// </remarks>
        /// <param name="personId">Código da pessoa.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<PersonDto> PersonAsync(System.Guid personId, string authorization)
        {
            return PersonAsync(personId, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Adquire os dados de uma pessoa, incluindo todos os quadros societários dos quais faz parte.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são previamente armazenados em cache.  
        /// <br/>• Atualizações individuais sob demanda.  
        /// <br/>• Atualizações globais a cada 30 dias.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta.
        /// </remarks>
        /// <param name="personId">Código da pessoa.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<PersonDto> PersonAsync(System.Guid personId, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (personId == null)
                throw new System.ArgumentNullException("personId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/person/{personId}");
            urlBuilder_.Replace("{personId}", Uri.EscapeDataString(ConvertToString(personId, System.Globalization.CultureInfo.InvariantCulture)));

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<PersonDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Lista todas as pessoas que correspondem aos filtros configurados.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são previamente armazenados em cache.  
        /// <br/>• Atualizações individuais sob demanda.  
        /// <br/>• Atualizações globais a cada 30 dias.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por cada 10 pessoas retornadas, respeitando o `limit` fornecido.
        /// </remarks>
        /// <param name="token">Token de paginação, mutualmente exclusivo com as demais propriedades.</param>
        /// <param name="limit">Quantidade de registros a serem lidos por página.</param>
        /// <param name="type_in">Tipos das pessoas serem incluídos, separados por vírgula:  
        /// <br/>• `NATURAL`: Pessoa física.  
        /// <br/>• `LEGAL`: Pessoa jurídica.  
        /// <br/>• `FOREIGN`: Pessoa residente no exterior.  
        /// <br/>• `UNKNOWN`: Pessoa desconhecida.</param>
        /// <param name="name_in">Nomes a serem incluídos, separados por espaço para correspondência na mesma pessoa,
        /// <br/>ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="name_nin">Nomes a serem excluídos, separados por espaço para correspondência na mesma pessoa,
        /// <br/>ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="taxId_in">CPFs ou CNPJs a serem incluídos, separados por vírgula. A correspondência por CPF será feita pelos
        /// <br/>dígitos entre o quarto e nono, uma vez que não armazenamos CPFs completos em nossa plataforma.</param>
        /// <param name="age_in">Faixas etárias a serem incluídas, separadas por vírgula.</param>
        /// <param name="country_id_in">Códigos dos países a serem incluídos, separados por vírgula, conforme
        /// <br/>[M49 🡭](https://unstats.un.org/unsd/methodology/m49/).</param>
        /// <param name="country_id_nin">Códigos dos países a serem excluídos, separados por vírgula, conforme
        /// <br/>[M49 🡭](https://unstats.un.org/unsd/methodology/m49/).</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<PersonPageDto> PersonAsync(string token, double? limit, System.Collections.Generic.IEnumerable<Anonymous2> type_in, System.Collections.Generic.IEnumerable<string> name_in, System.Collections.Generic.IEnumerable<string> name_nin, System.Collections.Generic.IEnumerable<string> taxId_in, System.Collections.Generic.IEnumerable<Age> age_in, System.Collections.Generic.IEnumerable<double> country_id_in, System.Collections.Generic.IEnumerable<double> country_id_nin, string authorization)
        {
            return PersonAsync(token, limit, type_in, name_in, name_nin, taxId_in, age_in, country_id_in, country_id_nin, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Lista todas as pessoas que correspondem aos filtros configurados.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são previamente armazenados em cache.  
        /// <br/>• Atualizações individuais sob demanda.  
        /// <br/>• Atualizações globais a cada 30 dias.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por cada 10 pessoas retornadas, respeitando o `limit` fornecido.
        /// </remarks>
        /// <param name="token">Token de paginação, mutualmente exclusivo com as demais propriedades.</param>
        /// <param name="limit">Quantidade de registros a serem lidos por página.</param>
        /// <param name="type_in">Tipos das pessoas serem incluídos, separados por vírgula:  
        /// <br/>• `NATURAL`: Pessoa física.  
        /// <br/>• `LEGAL`: Pessoa jurídica.  
        /// <br/>• `FOREIGN`: Pessoa residente no exterior.  
        /// <br/>• `UNKNOWN`: Pessoa desconhecida.</param>
        /// <param name="name_in">Nomes a serem incluídos, separados por espaço para correspondência na mesma pessoa,
        /// <br/>ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="name_nin">Nomes a serem excluídos, separados por espaço para correspondência na mesma pessoa,
        /// <br/>ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="taxId_in">CPFs ou CNPJs a serem incluídos, separados por vírgula. A correspondência por CPF será feita pelos
        /// <br/>dígitos entre o quarto e nono, uma vez que não armazenamos CPFs completos em nossa plataforma.</param>
        /// <param name="age_in">Faixas etárias a serem incluídas, separadas por vírgula.</param>
        /// <param name="country_id_in">Códigos dos países a serem incluídos, separados por vírgula, conforme
        /// <br/>[M49 🡭](https://unstats.un.org/unsd/methodology/m49/).</param>
        /// <param name="country_id_nin">Códigos dos países a serem excluídos, separados por vírgula, conforme
        /// <br/>[M49 🡭](https://unstats.un.org/unsd/methodology/m49/).</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<PersonPageDto> PersonAsync(string token, double? limit, System.Collections.Generic.IEnumerable<Anonymous2> type_in, System.Collections.Generic.IEnumerable<string> name_in, System.Collections.Generic.IEnumerable<string> name_nin, System.Collections.Generic.IEnumerable<string> taxId_in, System.Collections.Generic.IEnumerable<Age> age_in, System.Collections.Generic.IEnumerable<double> country_id_in, System.Collections.Generic.IEnumerable<double> country_id_nin, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/person?");
            if (token != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("token") + "=").Append(Uri.EscapeDataString(ConvertToString(token, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (limit != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("limit") + "=").Append(Uri.EscapeDataString(ConvertToString(limit, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (type_in != null)
            {
                foreach (var item_ in type_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("type.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (name_in != null)
            {
                foreach (var item_ in name_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("name.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (name_nin != null)
            {
                foreach (var item_ in name_nin)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("name.nin") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (taxId_in != null)
            {
                foreach (var item_ in taxId_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("taxId.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (age_in != null)
            {
                foreach (var item_ in age_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("age.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (country_id_in != null)
            {
                foreach (var item_ in country_id_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("country.id.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (country_id_nin != null)
            {
                foreach (var item_ in country_id_nin)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("country.id.nin") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<PersonPageDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Adquire de forma centralizada múltiplas informações de um estabelecimento, incluindo acesso a diversos portais públicos e enriquecimentos externos.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)  
        /// <br/>• [Simples Nacional 🡭](https://www8.receita.fazenda.gov.br/SimplesNacional/aplicacoes.aspx?id=21)  
        /// <br/>• [Cadastro de Contribuintes 🡭](https://dfe-portal.svrs.rs.gov.br/NFE/CCC)  
        /// <br/>• [SUFRAMA 🡭](https://www4.suframa.gov.br/cadsuf/#/menu-externo)  
        /// <br/>• [Google Maps 🡭](https://developers.google.com/maps)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados podem ser obtidos online em tempo real, ou previamente armazenados em cache.  
        /// <br/>• O modo de resolução irá respeitar a defasagem máxima fornecida em `maxAge`.  
        /// <br/>• Atualizações individuais sob demanda.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta online.  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta em cache.  
        /// <br/>• Sujeito a adicionais conforme escolhas de parâmetros.
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="simples">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona as informações de opção pelo
        /// <br/>Simples e enquadramento no MEI.</param>
        /// <param name="simplesHistory">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona o histórico de períodos
        /// <br/>anteriores do Simples e MEI.</param>
        /// <param name="registrations">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona as Inscrições Estaduais para as selecionadas
        /// <br/>Unidades Federativas separadas por vírgula, utilize `BR` para considerar todas.</param>
        /// <param name="suframa">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona a inscrição na SUFRAMA.</param>
        /// <param name="geocoding">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona a latitude e longitude do endereço.</param>
        /// <param name="links">Adiciona links públicos para visualização dos arquivos selecionados separados por vírgula.</param>
        /// <param name="strategy">Estratégia de cache utilizada na aquisição dos dados:  
        /// <br/>• `CACHE`: Entrega os dados do cache, evitando cobranças de créditos, se os dados não estiverem disponíveis resultará em um erro 404.  
        /// <br/>• `CACHE_IF_FRESH`: Retorna os dados do cache respeitando o limite em `maxAge`, se os dados estiverem desatualizados será consultado online.  
        /// <br/>• `CACHE_IF_ERROR`: Idem ao `CACHE_IF_FRESH`, mas se a consulta online falhar retorna os dados do cache respeitando o limite em `maxStale`.  
        /// <br/>• `ONLINE`: Consulta diretamente online, não recomendado pois ignora qualquer cache, sugerimos configurar `maxAge=1` como alternativa.</param>
        /// <param name="maxAge">Idade máxima, em dias, que um dado em cache é aceite, relevante para as estratégias `CACHE_IF_FRESH` e `CACHE_IF_ERROR`.</param>
        /// <param name="maxStale">Idade máxima, em dias, que um dado em cache é aceite em caso de erro na busca online, relevante apenas para a estratégia `CACHE_IF_FRESH`.</param>
        /// <param name="sync">Aguarda a compensação dos créditos de forma síncrona, retornando o cabeçalho `cnpja-request-cost`.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<OfficeDto> CnpjAsync(string taxId, bool? simples, bool? simplesHistory, System.Collections.Generic.IEnumerable<UF> registrations, bool? suframa, bool? geocoding, System.Collections.Generic.IEnumerable<Anonymous5> links, Strategy? strategy, double? maxAge, double? maxStale, bool? sync, string authorization)
        {
            return CnpjAsync(taxId, simples, simplesHistory, registrations, suframa, geocoding, links, strategy, maxAge, maxStale, sync, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Adquire de forma centralizada múltiplas informações de um estabelecimento, incluindo acesso a diversos portais públicos e enriquecimentos externos.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)  
        /// <br/>• [Simples Nacional 🡭](https://www8.receita.fazenda.gov.br/SimplesNacional/aplicacoes.aspx?id=21)  
        /// <br/>• [Cadastro de Contribuintes 🡭](https://dfe-portal.svrs.rs.gov.br/NFE/CCC)  
        /// <br/>• [SUFRAMA 🡭](https://www4.suframa.gov.br/cadsuf/#/menu-externo)  
        /// <br/>• [Google Maps 🡭](https://developers.google.com/maps)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados podem ser obtidos online em tempo real, ou previamente armazenados em cache.  
        /// <br/>• O modo de resolução irá respeitar a defasagem máxima fornecida em `maxAge`.  
        /// <br/>• Atualizações individuais sob demanda.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta online.  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta em cache.  
        /// <br/>• Sujeito a adicionais conforme escolhas de parâmetros.
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="simples">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona as informações de opção pelo
        /// <br/>Simples e enquadramento no MEI.</param>
        /// <param name="simplesHistory">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona o histórico de períodos
        /// <br/>anteriores do Simples e MEI.</param>
        /// <param name="registrations">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona as Inscrições Estaduais para as selecionadas
        /// <br/>Unidades Federativas separadas por vírgula, utilize `BR` para considerar todas.</param>
        /// <param name="suframa">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona a inscrição na SUFRAMA.</param>
        /// <param name="geocoding">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona a latitude e longitude do endereço.</param>
        /// <param name="links">Adiciona links públicos para visualização dos arquivos selecionados separados por vírgula.</param>
        /// <param name="strategy">Estratégia de cache utilizada na aquisição dos dados:  
        /// <br/>• `CACHE`: Entrega os dados do cache, evitando cobranças de créditos, se os dados não estiverem disponíveis resultará em um erro 404.  
        /// <br/>• `CACHE_IF_FRESH`: Retorna os dados do cache respeitando o limite em `maxAge`, se os dados estiverem desatualizados será consultado online.  
        /// <br/>• `CACHE_IF_ERROR`: Idem ao `CACHE_IF_FRESH`, mas se a consulta online falhar retorna os dados do cache respeitando o limite em `maxStale`.  
        /// <br/>• `ONLINE`: Consulta diretamente online, não recomendado pois ignora qualquer cache, sugerimos configurar `maxAge=1` como alternativa.</param>
        /// <param name="maxAge">Idade máxima, em dias, que um dado em cache é aceite, relevante para as estratégias `CACHE_IF_FRESH` e `CACHE_IF_ERROR`.</param>
        /// <param name="maxStale">Idade máxima, em dias, que um dado em cache é aceite em caso de erro na busca online, relevante apenas para a estratégia `CACHE_IF_FRESH`.</param>
        /// <param name="sync">Aguarda a compensação dos créditos de forma síncrona, retornando o cabeçalho `cnpja-request-cost`.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<OfficeDto> CnpjAsync(string taxId, bool? simples, bool? simplesHistory, System.Collections.Generic.IEnumerable<UF> registrations, bool? suframa, bool? geocoding, System.Collections.Generic.IEnumerable<Anonymous5> links, Strategy? strategy, double? maxAge, double? maxStale, bool? sync, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (taxId == null)
                throw new System.ArgumentNullException("taxId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/office/{taxId}?");
            urlBuilder_.Replace("{taxId}", Uri.EscapeDataString(ConvertToString(taxId, System.Globalization.CultureInfo.InvariantCulture)));
            if (simples != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("simples") + "=").Append(Uri.EscapeDataString(ConvertToString(simples, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (simplesHistory != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("simplesHistory") + "=").Append(Uri.EscapeDataString(ConvertToString(simplesHistory, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (registrations != null)
            {
                foreach (var item_ in registrations)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("registrations") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (suframa != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("suframa") + "=").Append(Uri.EscapeDataString(ConvertToString(suframa, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (geocoding != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("geocoding") + "=").Append(Uri.EscapeDataString(ConvertToString(geocoding, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (links != null)
            {
                foreach (var item_ in links)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("links") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (strategy != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("strategy") + "=").Append(Uri.EscapeDataString(ConvertToString(strategy, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (maxAge != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("maxAge") + "=").Append(Uri.EscapeDataString(ConvertToString(maxAge, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (maxStale != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("maxStale") + "=").Append(Uri.EscapeDataString(ConvertToString(maxStale, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (sync != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("sync") + "=").Append(Uri.EscapeDataString(ConvertToString(sync, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<OfficeDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 503)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorServiceUnavailableDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorServiceUnavailableDto>("Consulta online temporariamente indispon\u00edvel.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Lista todos os estabelecimentos que correspondem aos filtros configurados.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são previamente armazenados em cache.  
        /// <br/>• Atualizações individuais sob demanda.  
        /// <br/>• Atualizações globais a cada 30 dias.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por cada 10 estabelecimentos retornados, respeitando o `limit` fornecido.
        /// </remarks>
        /// <param name="token">Token de paginação, mutualmente exclusivo com as demais propriedades.</param>
        /// <param name="limit">Quantidade de registros a serem lidos por página.</param>
        /// <param name="names_in">Termos a serem incluídos na razão social ou nome fantasia, separados por espaço para correspondência
        /// <br/>no mesmo estabelecimento, ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="names_nin">Termos a serem excluídos na razão social ou nome fantasia, separados por espaço para correspondência
        /// <br/>no mesmo estabelecimento, ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="company_name_in">Termos a serem incluídos na razão social, separados por espaço para correspondência na mesma empresa,
        /// <br/>ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="company_name_nin">Termos a serem excluídos na razão social, separados por espaço para correspondência na mesma empresa,
        /// <br/>ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="company_equity_gte">Capital social maior ou igual ao valor especificado.</param>
        /// <param name="company_equity_lte">Capital social menor ou igual ao valor especificado.</param>
        /// <param name="company_nature_id_in">Códigos das naturezas jurídicas a serem incluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/estrutura/natjur-estrutura/natureza-juridica-2021).</param>
        /// <param name="company_nature_id_nin">Códigos das naturezas jurídicas a serem excluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/estrutura/natjur-estrutura/natureza-juridica-2021).</param>
        /// <param name="company_size_id_in">Códigos dos portes a serem incluídos, separados por vírgula:  
        /// <br/>• `1`: Microempresa (ME).  
        /// <br/>• `3`: Empresa de Pequeno Porte (EPP).  
        /// <br/>• `5`: Demais.</param>
        /// <param name="company_simples_optant_eq">Indicador de opção pelo Simples Nacional:  
        /// <br/>• `true`: Apenas empresas optantes.  
        /// <br/>• `false`: Apenas empresas não optantes.</param>
        /// <param name="company_simples_since_gte">Data de opção pelo Simples Nacional maior ou igual a especificada.</param>
        /// <param name="company_simples_since_lte">Data de opção pelo Simples Nacional menor ou igual a especificada.</param>
        /// <param name="company_simei_optant_eq">Indicador de enquadramento no MEI:  
        /// <br/>• `true`: Apenas empresas enquadradas.  
        /// <br/>• `false`: Apenas empresas não enquadradas.</param>
        /// <param name="company_simei_since_gte">Data de enquadramento no MEI maior ou igual a especificada.</param>
        /// <param name="company_simei_since_lte">Data de enquadramento no MEI menor ou igual a especificada.</param>
        /// <param name="taxId_nin">Identificadores de listas de CNPJs a serem excluídos, separados por vírgula.</param>
        /// <param name="alias_in">Termos a serem incluídos no nome fantasia, separados por espaço para correspondência no mesmo
        /// <br/>estabelecimento, ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="alias_nin">Termos a serem excluídos no nome fantasia, separados por espaço para correspondência no mesmo
        /// <br/>estabelecimento, ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="founded_gte">Data de abertura maior ou igual a especificada.</param>
        /// <param name="founded_lte">Data de abertura menor ou igual a especificada.</param>
        /// <param name="head_eq">Indicador de estabelecimento matriz:  
        /// <br/>• `true`: Apenas matrizes.  
        /// <br/>• `false`: Apenas filiais.</param>
        /// <param name="statusDate_gte">Data da situação cadastral maior ou igual a especificada.</param>
        /// <param name="statusDate_lte">Data da situação cadastral menor ou igual a especificada.</param>
        /// <param name="status_id_in">Códigos das situações cadastrais a serem incluídos, separados por vírgula:  
        /// <br/>• `1`: Nula.  
        /// <br/>• `2`: Ativa.  
        /// <br/>• `3`: Suspensa.  
        /// <br/>• `4`: Inapta.  
        /// <br/>• `8`: Baixada.</param>
        /// <param name="reason_id_in">Códigos dos motivos das situações cadastrais a serem incluídos, separados por vírgula, conforme
        /// <br/>[Receita Federal 🡭](http://www.consultas.cge.rj.gov.br/scadastral.pdf).</param>
        /// <param name="specialDate_gte">Data da situação especial maior ou igual a especificada.</param>
        /// <param name="specialDate_lte">Data da situação especial menor ou igual a especificada.</param>
        /// <param name="special_id_in">Códigos das situações especiais a serem incluídos, separados por vírgula, conforme
        /// <br/>[Receita Federal 🡭](http://www38.receita.fazenda.gov.br/cadsincnac/jsp/coleta/ajuda/topicos/Eventos_de_Alteracao.htm).</param>
        /// <param name="address_municipality_in">Códigos dos municípios a serem incluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://www.ibge.gov.br/explica/codigos-dos-municipios.php).</param>
        /// <param name="address_municipality_nin">Códigos dos municípios a serem excluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://www.ibge.gov.br/explica/codigos-dos-municipios.php).</param>
        /// <param name="address_district_in">Termos a serem incluídos no bairro, separados por espaço para correspondência no mesmo
        /// <br/>estabelecimento, ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="address_district_nin">Termos a serem excluídos no bairro, separados por espaço para correspondência no mesmo
        /// <br/>estabelecimento, ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="address_state_in">Unidades federativas a serem incluídas, separadas por vírgula.</param>
        /// <param name="address_zip_in">Códigos de endereçamento postal a serem incluídos, separados por vírgula.</param>
        /// <param name="address_zip_gte">Código de endereçamento postal maior ou igual ao especificado.</param>
        /// <param name="address_zip_lte">Código de endereçamento postal menor ou igual ao especificado.</param>
        /// <param name="address_country_id_in">Códigos dos países a serem incluídos, separados por vírgula, conforme
        /// <br/>[M49 🡭](https://unstats.un.org/unsd/methodology/m49/).</param>
        /// <param name="address_country_id_nin">Códigos dos países a serem excluídos, separados por vírgula, conforme
        /// <br/>[M49 🡭](https://unstats.un.org/unsd/methodology/m49/).</param>
        /// <param name="phones_ex">Indicador de existência de telefone:  
        /// <br/>• `true`: Apenas estabelecimentos com telefone.  
        /// <br/>• `false`: Apenas estabelecimentos sem telefone.</param>
        /// <param name="phones_type_in">Tipos de telefone a serem incluídos, separados por vírgula.</param>
        /// <param name="phones_area_in">Códigos de DDD a serem incluídos, separados por vírgula.</param>
        /// <param name="phones_area_gte">Códigos de DDD maior ou igual ao especificado.</param>
        /// <param name="phones_area_lte">Códigos de DDD menor ou igual ao especificado.</param>
        /// <param name="emails_ex">Indicador de existência de e-mail:  
        /// <br/>• `true`: Apenas estabelecimentos com e-mail.  
        /// <br/>• `false`: Apenas estabelecimentos sem e-mail.</param>
        /// <param name="emails_ownership_in">Tipos de propriedade de e-mail a serem incluídos, separados por vírgula.</param>
        /// <param name="emails_domain_in">Domínios de e-mail a serem incluídos, separados por vírgula.</param>
        /// <param name="emails_domain_nin">Domínios de e-mail a serem excluídos, separados por vírgula.</param>
        /// <param name="activities_id_in">Códigos das atividades econômicas principais ou secundárias a serem incluídos, separados por vírgula,
        /// <br/>conforme [IBGE 🡭](https://concla.ibge.gov.br/busca-online-cnae.html?view=estrutura).</param>
        /// <param name="activities_id_nin">Códigos das atividades econômicas principais ou secundárias a serem excluídos, separados por vírgula,
        /// <br/>conforme [IBGE 🡭](https://concla.ibge.gov.br/busca-online-cnae.html?view=estrutura).</param>
        /// <param name="mainActivity_id_in">Códigos das atividades econômicas principais a serem incluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/busca-online-cnae.html?view=estrutura).</param>
        /// <param name="mainActivity_id_nin">Códigos das atividades econômicas principais a serem excluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/busca-online-cnae.html?view=estrutura).</param>
        /// <param name="sideActivities_id_in">Códigos das atividades econômicas secundárias a serem incluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/busca-online-cnae.html?view=estrutura).</param>
        /// <param name="sideActivities_id_nin">Códigos das atividades econômicas secundárias a serem excluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/busca-online-cnae.html?view=estrutura).</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<OfficePageDto> CnpjAsync(string token, double? limit, System.Collections.Generic.IEnumerable<string> names_in, System.Collections.Generic.IEnumerable<string> names_nin, System.Collections.Generic.IEnumerable<string> company_name_in, System.Collections.Generic.IEnumerable<string> company_name_nin, float? company_equity_gte, float? company_equity_lte, System.Collections.Generic.IEnumerable<double> company_nature_id_in, System.Collections.Generic.IEnumerable<double> company_nature_id_nin, System.Collections.Generic.IEnumerable<double> company_size_id_in, bool? company_simples_optant_eq, string company_simples_since_gte, string company_simples_since_lte, bool? company_simei_optant_eq, string company_simei_since_gte, string company_simei_since_lte, System.Collections.Generic.IEnumerable<string> taxId_nin, System.Collections.Generic.IEnumerable<string> alias_in, System.Collections.Generic.IEnumerable<string> alias_nin, string founded_gte, string founded_lte, bool? head_eq, string statusDate_gte, string statusDate_lte, System.Collections.Generic.IEnumerable<double> status_id_in, System.Collections.Generic.IEnumerable<double> reason_id_in, string specialDate_gte, string specialDate_lte, System.Collections.Generic.IEnumerable<double> special_id_in, System.Collections.Generic.IEnumerable<double> address_municipality_in, System.Collections.Generic.IEnumerable<double> address_municipality_nin, System.Collections.Generic.IEnumerable<string> address_district_in, System.Collections.Generic.IEnumerable<string> address_district_nin, System.Collections.Generic.IEnumerable<UF> address_state_in, System.Collections.Generic.IEnumerable<string> address_zip_in, string address_zip_gte, string address_zip_lte, System.Collections.Generic.IEnumerable<double> address_country_id_in, System.Collections.Generic.IEnumerable<double> address_country_id_nin, bool? phones_ex, System.Collections.Generic.IEnumerable<Anonymous7> phones_type_in, System.Collections.Generic.IEnumerable<string> phones_area_in, string phones_area_gte, string phones_area_lte, bool? emails_ex, System.Collections.Generic.IEnumerable<Anonymous8> emails_ownership_in, System.Collections.Generic.IEnumerable<string> emails_domain_in, System.Collections.Generic.IEnumerable<string> emails_domain_nin, System.Collections.Generic.IEnumerable<double> activities_id_in, System.Collections.Generic.IEnumerable<double> activities_id_nin, System.Collections.Generic.IEnumerable<double> mainActivity_id_in, System.Collections.Generic.IEnumerable<double> mainActivity_id_nin, System.Collections.Generic.IEnumerable<double> sideActivities_id_in, System.Collections.Generic.IEnumerable<double> sideActivities_id_nin, string authorization)
        {
            return CnpjAsync(token, limit, names_in, names_nin, company_name_in, company_name_nin, company_equity_gte, company_equity_lte, company_nature_id_in, company_nature_id_nin, company_size_id_in, company_simples_optant_eq, company_simples_since_gte, company_simples_since_lte, company_simei_optant_eq, company_simei_since_gte, company_simei_since_lte, taxId_nin, alias_in, alias_nin, founded_gte, founded_lte, head_eq, statusDate_gte, statusDate_lte, status_id_in, reason_id_in, specialDate_gte, specialDate_lte, special_id_in, address_municipality_in, address_municipality_nin, address_district_in, address_district_nin, address_state_in, address_zip_in, address_zip_gte, address_zip_lte, address_country_id_in, address_country_id_nin, phones_ex, phones_type_in, phones_area_in, phones_area_gte, phones_area_lte, emails_ex, emails_ownership_in, emails_domain_in, emails_domain_nin, activities_id_in, activities_id_nin, mainActivity_id_in, mainActivity_id_nin, sideActivities_id_in, sideActivities_id_nin, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Lista todos os estabelecimentos que correspondem aos filtros configurados.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são previamente armazenados em cache.  
        /// <br/>• Atualizações individuais sob demanda.  
        /// <br/>• Atualizações globais a cada 30 dias.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por cada 10 estabelecimentos retornados, respeitando o `limit` fornecido.
        /// </remarks>
        /// <param name="token">Token de paginação, mutualmente exclusivo com as demais propriedades.</param>
        /// <param name="limit">Quantidade de registros a serem lidos por página.</param>
        /// <param name="names_in">Termos a serem incluídos na razão social ou nome fantasia, separados por espaço para correspondência
        /// <br/>no mesmo estabelecimento, ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="names_nin">Termos a serem excluídos na razão social ou nome fantasia, separados por espaço para correspondência
        /// <br/>no mesmo estabelecimento, ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="company_name_in">Termos a serem incluídos na razão social, separados por espaço para correspondência na mesma empresa,
        /// <br/>ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="company_name_nin">Termos a serem excluídos na razão social, separados por espaço para correspondência na mesma empresa,
        /// <br/>ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="company_equity_gte">Capital social maior ou igual ao valor especificado.</param>
        /// <param name="company_equity_lte">Capital social menor ou igual ao valor especificado.</param>
        /// <param name="company_nature_id_in">Códigos das naturezas jurídicas a serem incluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/estrutura/natjur-estrutura/natureza-juridica-2021).</param>
        /// <param name="company_nature_id_nin">Códigos das naturezas jurídicas a serem excluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/estrutura/natjur-estrutura/natureza-juridica-2021).</param>
        /// <param name="company_size_id_in">Códigos dos portes a serem incluídos, separados por vírgula:  
        /// <br/>• `1`: Microempresa (ME).  
        /// <br/>• `3`: Empresa de Pequeno Porte (EPP).  
        /// <br/>• `5`: Demais.</param>
        /// <param name="company_simples_optant_eq">Indicador de opção pelo Simples Nacional:  
        /// <br/>• `true`: Apenas empresas optantes.  
        /// <br/>• `false`: Apenas empresas não optantes.</param>
        /// <param name="company_simples_since_gte">Data de opção pelo Simples Nacional maior ou igual a especificada.</param>
        /// <param name="company_simples_since_lte">Data de opção pelo Simples Nacional menor ou igual a especificada.</param>
        /// <param name="company_simei_optant_eq">Indicador de enquadramento no MEI:  
        /// <br/>• `true`: Apenas empresas enquadradas.  
        /// <br/>• `false`: Apenas empresas não enquadradas.</param>
        /// <param name="company_simei_since_gte">Data de enquadramento no MEI maior ou igual a especificada.</param>
        /// <param name="company_simei_since_lte">Data de enquadramento no MEI menor ou igual a especificada.</param>
        /// <param name="taxId_nin">Identificadores de listas de CNPJs a serem excluídos, separados por vírgula.</param>
        /// <param name="alias_in">Termos a serem incluídos no nome fantasia, separados por espaço para correspondência no mesmo
        /// <br/>estabelecimento, ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="alias_nin">Termos a serem excluídos no nome fantasia, separados por espaço para correspondência no mesmo
        /// <br/>estabelecimento, ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="founded_gte">Data de abertura maior ou igual a especificada.</param>
        /// <param name="founded_lte">Data de abertura menor ou igual a especificada.</param>
        /// <param name="head_eq">Indicador de estabelecimento matriz:  
        /// <br/>• `true`: Apenas matrizes.  
        /// <br/>• `false`: Apenas filiais.</param>
        /// <param name="statusDate_gte">Data da situação cadastral maior ou igual a especificada.</param>
        /// <param name="statusDate_lte">Data da situação cadastral menor ou igual a especificada.</param>
        /// <param name="status_id_in">Códigos das situações cadastrais a serem incluídos, separados por vírgula:  
        /// <br/>• `1`: Nula.  
        /// <br/>• `2`: Ativa.  
        /// <br/>• `3`: Suspensa.  
        /// <br/>• `4`: Inapta.  
        /// <br/>• `8`: Baixada.</param>
        /// <param name="reason_id_in">Códigos dos motivos das situações cadastrais a serem incluídos, separados por vírgula, conforme
        /// <br/>[Receita Federal 🡭](http://www.consultas.cge.rj.gov.br/scadastral.pdf).</param>
        /// <param name="specialDate_gte">Data da situação especial maior ou igual a especificada.</param>
        /// <param name="specialDate_lte">Data da situação especial menor ou igual a especificada.</param>
        /// <param name="special_id_in">Códigos das situações especiais a serem incluídos, separados por vírgula, conforme
        /// <br/>[Receita Federal 🡭](http://www38.receita.fazenda.gov.br/cadsincnac/jsp/coleta/ajuda/topicos/Eventos_de_Alteracao.htm).</param>
        /// <param name="address_municipality_in">Códigos dos municípios a serem incluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://www.ibge.gov.br/explica/codigos-dos-municipios.php).</param>
        /// <param name="address_municipality_nin">Códigos dos municípios a serem excluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://www.ibge.gov.br/explica/codigos-dos-municipios.php).</param>
        /// <param name="address_district_in">Termos a serem incluídos no bairro, separados por espaço para correspondência no mesmo
        /// <br/>estabelecimento, ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="address_district_nin">Termos a serem excluídos no bairro, separados por espaço para correspondência no mesmo
        /// <br/>estabelecimento, ou separados por vírgula para correspondência em diferentes.</param>
        /// <param name="address_state_in">Unidades federativas a serem incluídas, separadas por vírgula.</param>
        /// <param name="address_zip_in">Códigos de endereçamento postal a serem incluídos, separados por vírgula.</param>
        /// <param name="address_zip_gte">Código de endereçamento postal maior ou igual ao especificado.</param>
        /// <param name="address_zip_lte">Código de endereçamento postal menor ou igual ao especificado.</param>
        /// <param name="address_country_id_in">Códigos dos países a serem incluídos, separados por vírgula, conforme
        /// <br/>[M49 🡭](https://unstats.un.org/unsd/methodology/m49/).</param>
        /// <param name="address_country_id_nin">Códigos dos países a serem excluídos, separados por vírgula, conforme
        /// <br/>[M49 🡭](https://unstats.un.org/unsd/methodology/m49/).</param>
        /// <param name="phones_ex">Indicador de existência de telefone:  
        /// <br/>• `true`: Apenas estabelecimentos com telefone.  
        /// <br/>• `false`: Apenas estabelecimentos sem telefone.</param>
        /// <param name="phones_type_in">Tipos de telefone a serem incluídos, separados por vírgula.</param>
        /// <param name="phones_area_in">Códigos de DDD a serem incluídos, separados por vírgula.</param>
        /// <param name="phones_area_gte">Códigos de DDD maior ou igual ao especificado.</param>
        /// <param name="phones_area_lte">Códigos de DDD menor ou igual ao especificado.</param>
        /// <param name="emails_ex">Indicador de existência de e-mail:  
        /// <br/>• `true`: Apenas estabelecimentos com e-mail.  
        /// <br/>• `false`: Apenas estabelecimentos sem e-mail.</param>
        /// <param name="emails_ownership_in">Tipos de propriedade de e-mail a serem incluídos, separados por vírgula.</param>
        /// <param name="emails_domain_in">Domínios de e-mail a serem incluídos, separados por vírgula.</param>
        /// <param name="emails_domain_nin">Domínios de e-mail a serem excluídos, separados por vírgula.</param>
        /// <param name="activities_id_in">Códigos das atividades econômicas principais ou secundárias a serem incluídos, separados por vírgula,
        /// <br/>conforme [IBGE 🡭](https://concla.ibge.gov.br/busca-online-cnae.html?view=estrutura).</param>
        /// <param name="activities_id_nin">Códigos das atividades econômicas principais ou secundárias a serem excluídos, separados por vírgula,
        /// <br/>conforme [IBGE 🡭](https://concla.ibge.gov.br/busca-online-cnae.html?view=estrutura).</param>
        /// <param name="mainActivity_id_in">Códigos das atividades econômicas principais a serem incluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/busca-online-cnae.html?view=estrutura).</param>
        /// <param name="mainActivity_id_nin">Códigos das atividades econômicas principais a serem excluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/busca-online-cnae.html?view=estrutura).</param>
        /// <param name="sideActivities_id_in">Códigos das atividades econômicas secundárias a serem incluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/busca-online-cnae.html?view=estrutura).</param>
        /// <param name="sideActivities_id_nin">Códigos das atividades econômicas secundárias a serem excluídos, separados por vírgula, conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/busca-online-cnae.html?view=estrutura).</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<OfficePageDto> CnpjAsync(string token, double? limit, System.Collections.Generic.IEnumerable<string> names_in, System.Collections.Generic.IEnumerable<string> names_nin, System.Collections.Generic.IEnumerable<string> company_name_in, System.Collections.Generic.IEnumerable<string> company_name_nin, float? company_equity_gte, float? company_equity_lte, System.Collections.Generic.IEnumerable<double> company_nature_id_in, System.Collections.Generic.IEnumerable<double> company_nature_id_nin, System.Collections.Generic.IEnumerable<double> company_size_id_in, bool? company_simples_optant_eq, string company_simples_since_gte, string company_simples_since_lte, bool? company_simei_optant_eq, string company_simei_since_gte, string company_simei_since_lte, System.Collections.Generic.IEnumerable<string> taxId_nin, System.Collections.Generic.IEnumerable<string> alias_in, System.Collections.Generic.IEnumerable<string> alias_nin, string founded_gte, string founded_lte, bool? head_eq, string statusDate_gte, string statusDate_lte, System.Collections.Generic.IEnumerable<double> status_id_in, System.Collections.Generic.IEnumerable<double> reason_id_in, string specialDate_gte, string specialDate_lte, System.Collections.Generic.IEnumerable<double> special_id_in, System.Collections.Generic.IEnumerable<double> address_municipality_in, System.Collections.Generic.IEnumerable<double> address_municipality_nin, System.Collections.Generic.IEnumerable<string> address_district_in, System.Collections.Generic.IEnumerable<string> address_district_nin, System.Collections.Generic.IEnumerable<UF> address_state_in, System.Collections.Generic.IEnumerable<string> address_zip_in, string address_zip_gte, string address_zip_lte, System.Collections.Generic.IEnumerable<double> address_country_id_in, System.Collections.Generic.IEnumerable<double> address_country_id_nin, bool? phones_ex, System.Collections.Generic.IEnumerable<Anonymous7> phones_type_in, System.Collections.Generic.IEnumerable<string> phones_area_in, string phones_area_gte, string phones_area_lte, bool? emails_ex, System.Collections.Generic.IEnumerable<Anonymous8> emails_ownership_in, System.Collections.Generic.IEnumerable<string> emails_domain_in, System.Collections.Generic.IEnumerable<string> emails_domain_nin, System.Collections.Generic.IEnumerable<double> activities_id_in, System.Collections.Generic.IEnumerable<double> activities_id_nin, System.Collections.Generic.IEnumerable<double> mainActivity_id_in, System.Collections.Generic.IEnumerable<double> mainActivity_id_nin, System.Collections.Generic.IEnumerable<double> sideActivities_id_in, System.Collections.Generic.IEnumerable<double> sideActivities_id_nin, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/office?");
            if (token != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("token") + "=").Append(Uri.EscapeDataString(ConvertToString(token, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (limit != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("limit") + "=").Append(Uri.EscapeDataString(ConvertToString(limit, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (names_in != null)
            {
                foreach (var item_ in names_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("names.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (names_nin != null)
            {
                foreach (var item_ in names_nin)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("names.nin") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (company_name_in != null)
            {
                foreach (var item_ in company_name_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("company.name.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (company_name_nin != null)
            {
                foreach (var item_ in company_name_nin)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("company.name.nin") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (company_equity_gte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("company.equity.gte") + "=").Append(Uri.EscapeDataString(ConvertToString(company_equity_gte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (company_equity_lte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("company.equity.lte") + "=").Append(Uri.EscapeDataString(ConvertToString(company_equity_lte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (company_nature_id_in != null)
            {
                foreach (var item_ in company_nature_id_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("company.nature.id.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (company_nature_id_nin != null)
            {
                foreach (var item_ in company_nature_id_nin)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("company.nature.id.nin") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (company_size_id_in != null)
            {
                foreach (var item_ in company_size_id_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("company.size.id.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (company_simples_optant_eq != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("company.simples.optant.eq") + "=").Append(Uri.EscapeDataString(ConvertToString(company_simples_optant_eq, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (company_simples_since_gte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("company.simples.since.gte") + "=").Append(Uri.EscapeDataString(ConvertToString(company_simples_since_gte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (company_simples_since_lte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("company.simples.since.lte") + "=").Append(Uri.EscapeDataString(ConvertToString(company_simples_since_lte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (company_simei_optant_eq != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("company.simei.optant.eq") + "=").Append(Uri.EscapeDataString(ConvertToString(company_simei_optant_eq, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (company_simei_since_gte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("company.simei.since.gte") + "=").Append(Uri.EscapeDataString(ConvertToString(company_simei_since_gte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (company_simei_since_lte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("company.simei.since.lte") + "=").Append(Uri.EscapeDataString(ConvertToString(company_simei_since_lte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (taxId_nin != null)
            {
                foreach (var item_ in taxId_nin)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("taxId.nin") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (alias_in != null)
            {
                foreach (var item_ in alias_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("alias.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (alias_nin != null)
            {
                foreach (var item_ in alias_nin)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("alias.nin") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (founded_gte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("founded.gte") + "=").Append(Uri.EscapeDataString(ConvertToString(founded_gte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (founded_lte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("founded.lte") + "=").Append(Uri.EscapeDataString(ConvertToString(founded_lte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (head_eq != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("head.eq") + "=").Append(Uri.EscapeDataString(ConvertToString(head_eq, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (statusDate_gte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("statusDate.gte") + "=").Append(Uri.EscapeDataString(ConvertToString(statusDate_gte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (statusDate_lte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("statusDate.lte") + "=").Append(Uri.EscapeDataString(ConvertToString(statusDate_lte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (status_id_in != null)
            {
                foreach (var item_ in status_id_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("status.id.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (reason_id_in != null)
            {
                foreach (var item_ in reason_id_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("reason.id.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (specialDate_gte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("specialDate.gte") + "=").Append(Uri.EscapeDataString(ConvertToString(specialDate_gte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (specialDate_lte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("specialDate.lte") + "=").Append(Uri.EscapeDataString(ConvertToString(specialDate_lte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (special_id_in != null)
            {
                foreach (var item_ in special_id_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("special.id.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (address_municipality_in != null)
            {
                foreach (var item_ in address_municipality_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("address.municipality.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (address_municipality_nin != null)
            {
                foreach (var item_ in address_municipality_nin)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("address.municipality.nin") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (address_district_in != null)
            {
                foreach (var item_ in address_district_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("address.district.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (address_district_nin != null)
            {
                foreach (var item_ in address_district_nin)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("address.district.nin") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (address_state_in != null)
            {
                foreach (var item_ in address_state_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("address.state.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (address_zip_in != null)
            {
                foreach (var item_ in address_zip_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("address.zip.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (address_zip_gte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("address.zip.gte") + "=").Append(Uri.EscapeDataString(ConvertToString(address_zip_gte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (address_zip_lte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("address.zip.lte") + "=").Append(Uri.EscapeDataString(ConvertToString(address_zip_lte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (address_country_id_in != null)
            {
                foreach (var item_ in address_country_id_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("address.country.id.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (address_country_id_nin != null)
            {
                foreach (var item_ in address_country_id_nin)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("address.country.id.nin") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (phones_ex != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("phones.ex") + "=").Append(Uri.EscapeDataString(ConvertToString(phones_ex, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (phones_type_in != null)
            {
                foreach (var item_ in phones_type_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("phones.type.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (phones_area_in != null)
            {
                foreach (var item_ in phones_area_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("phones.area.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (phones_area_gte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("phones.area.gte") + "=").Append(Uri.EscapeDataString(ConvertToString(phones_area_gte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (phones_area_lte != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("phones.area.lte") + "=").Append(Uri.EscapeDataString(ConvertToString(phones_area_lte, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (emails_ex != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("emails.ex") + "=").Append(Uri.EscapeDataString(ConvertToString(emails_ex, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (emails_ownership_in != null)
            {
                foreach (var item_ in emails_ownership_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("emails.ownership.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (emails_domain_in != null)
            {
                foreach (var item_ in emails_domain_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("emails.domain.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (emails_domain_nin != null)
            {
                foreach (var item_ in emails_domain_nin)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("emails.domain.nin") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (activities_id_in != null)
            {
                foreach (var item_ in activities_id_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("activities.id.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (activities_id_nin != null)
            {
                foreach (var item_ in activities_id_nin)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("activities.id.nin") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (mainActivity_id_in != null)
            {
                foreach (var item_ in mainActivity_id_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("mainActivity.id.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (mainActivity_id_nin != null)
            {
                foreach (var item_ in mainActivity_id_nin)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("mainActivity.id.nin") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (sideActivities_id_in != null)
            {
                foreach (var item_ in sideActivities_id_in)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("sideActivities.id.in") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            if (sideActivities_id_nin != null)
            {
                foreach (var item_ in sideActivities_id_nin)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("sideActivities.id.nin") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
                }
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<OfficePageDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Gera o mapa aéreo referente ao endereço do estabelecimento.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Google Maps 🡭](https://developers.google.com/maps)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são obtidos online em tempo real.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;3 ₪&lt;/span&gt; por consulta.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Exemplos&lt;/span&gt;  
        /// <br/>• Tamanho: [640x640](/proxy/api/assets/docs/map_size_640x640.png), [320x320](/proxy/api/assets/docs/map_size_320x320.png), [160x160](/proxy/api/assets/docs/map_size_160x160.png).  
        /// <br/>• Densidade: [1](/proxy/api/assets/docs/map_size_640x640.png), [2](/proxy/api/assets/docs/map_size_640x640x2.png).  
        /// <br/>• Zoom: [2](/proxy/api/assets/docs/map_zoom_02.png), [5](/proxy/api/assets/docs/map_zoom_05.png), [8](/proxy/api/assets/docs/map_zoom_08.png), [11](/proxy/api/assets/docs/map_zoom_11.png), [14](/proxy/api/assets/docs/map_zoom_14.png), [17](/proxy/api/assets/docs/map_zoom_17.png), [20](/proxy/api/assets/docs/map_zoom_20.png).  
        /// <br/>• Tipo: [roadmap](/proxy/api/assets/docs/map_type_roadmap.png), [terrain](/proxy/api/assets/docs/map_type_terrain.png), [satellite](/proxy/api/assets/docs/map_type_satellite.png), [hybrid](/proxy/api/assets/docs/map_type_hybrid.png).
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="width">Largura em pixels.</param>
        /// <param name="height">Altura em pixels.</param>
        /// <param name="scale">Multiplicador de densidade de pixels.</param>
        /// <param name="zoom">Nível de ampliação.</param>
        /// <param name="type">Tipo do mapa:  
        /// <br/>• `roadmap`: Rodovias.  
        /// <br/>• `terrain`: Elevação.  
        /// <br/>• `satellite`: Satélite.  
        /// <br/>• `hybrid`: Rodovias e satélite.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<FileResponse> Mapa_AéreoAsync(string taxId, double? width, double? height, double? scale, double? zoom, MapType? type, string authorization)
        {
            return Mapa_AéreoAsync(taxId, width, height, scale, zoom, type, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Gera o mapa aéreo referente ao endereço do estabelecimento.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Google Maps 🡭](https://developers.google.com/maps)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são obtidos online em tempo real.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;3 ₪&lt;/span&gt; por consulta.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Exemplos&lt;/span&gt;  
        /// <br/>• Tamanho: [640x640](/proxy/api/assets/docs/map_size_640x640.png), [320x320](/proxy/api/assets/docs/map_size_320x320.png), [160x160](/proxy/api/assets/docs/map_size_160x160.png).  
        /// <br/>• Densidade: [1](/proxy/api/assets/docs/map_size_640x640.png), [2](/proxy/api/assets/docs/map_size_640x640x2.png).  
        /// <br/>• Zoom: [2](/proxy/api/assets/docs/map_zoom_02.png), [5](/proxy/api/assets/docs/map_zoom_05.png), [8](/proxy/api/assets/docs/map_zoom_08.png), [11](/proxy/api/assets/docs/map_zoom_11.png), [14](/proxy/api/assets/docs/map_zoom_14.png), [17](/proxy/api/assets/docs/map_zoom_17.png), [20](/proxy/api/assets/docs/map_zoom_20.png).  
        /// <br/>• Tipo: [roadmap](/proxy/api/assets/docs/map_type_roadmap.png), [terrain](/proxy/api/assets/docs/map_type_terrain.png), [satellite](/proxy/api/assets/docs/map_type_satellite.png), [hybrid](/proxy/api/assets/docs/map_type_hybrid.png).
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="width">Largura em pixels.</param>
        /// <param name="height">Altura em pixels.</param>
        /// <param name="scale">Multiplicador de densidade de pixels.</param>
        /// <param name="zoom">Nível de ampliação.</param>
        /// <param name="type">Tipo do mapa:  
        /// <br/>• `roadmap`: Rodovias.  
        /// <br/>• `terrain`: Elevação.  
        /// <br/>• `satellite`: Satélite.  
        /// <br/>• `hybrid`: Rodovias e satélite.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<FileResponse> Mapa_AéreoAsync(string taxId, double? width, double? height, double? scale, double? zoom, MapType? type, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (taxId == null)
                throw new System.ArgumentNullException("taxId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/office/{taxId}/map?");
            urlBuilder_.Replace("{taxId}", Uri.EscapeDataString(ConvertToString(taxId, System.Globalization.CultureInfo.InvariantCulture)));
            if (width != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("width") + "=").Append(Uri.EscapeDataString(ConvertToString(width, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (height != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("height") + "=").Append(Uri.EscapeDataString(ConvertToString(height, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (scale != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("scale") + "=").Append(Uri.EscapeDataString(ConvertToString(scale, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (zoom != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("zoom") + "=").Append(Uri.EscapeDataString(ConvertToString(zoom, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (type != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("type") + "=").Append(Uri.EscapeDataString(ConvertToString(type, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("image/png"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200 || status_ == 206)
                        {
                            var responseStream_ = response_.Content == null ? System.IO.Stream.Null : await response_.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            var fileResponse_ = new FileResponse(status_, headers_, responseStream_, null, response_);
                            disposeClient_ = false;
                            disposeResponse_ = false; // response and client are disposed by FileResponse
                            return fileResponse_;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Gera a visão da rua referente ao endereço do estabelecimento.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Google Maps 🡭](https://developers.google.com/maps)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são obtidos online em tempo real.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;10 ₪&lt;/span&gt; por consulta.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Exemplos&lt;/span&gt;  
        /// <br/>• Tamanho: [640x640](/proxy/api/assets/docs/street_size_640x640.png), [640x400](/proxy/api/assets/docs/street_size_640x400.png), [320x320](/proxy/api/assets/docs/street_size_320x320.png), [320x200](/proxy/api/assets/docs/street_size_320x200.png).  
        /// <br/>• Campo de visão: [120](/proxy/api/assets/docs/street_fov_120.png), [90](/proxy/api/assets/docs/street_fov_90.png), [60](/proxy/api/assets/docs/street_fov_60.png).
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="width">Largura em pixels.</param>
        /// <param name="height">Altura em pixels.</param>
        /// <param name="fov">Campo de visão em graus.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<FileResponse> Visão_da_RuaAsync(string taxId, double? width, double? height, double? fov, string authorization)
        {
            return Visão_da_RuaAsync(taxId, width, height, fov, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Gera a visão da rua referente ao endereço do estabelecimento.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Google Maps 🡭](https://developers.google.com/maps)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são obtidos online em tempo real.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;10 ₪&lt;/span&gt; por consulta.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Exemplos&lt;/span&gt;  
        /// <br/>• Tamanho: [640x640](/proxy/api/assets/docs/street_size_640x640.png), [640x400](/proxy/api/assets/docs/street_size_640x400.png), [320x320](/proxy/api/assets/docs/street_size_320x320.png), [320x200](/proxy/api/assets/docs/street_size_320x200.png).  
        /// <br/>• Campo de visão: [120](/proxy/api/assets/docs/street_fov_120.png), [90](/proxy/api/assets/docs/street_fov_90.png), [60](/proxy/api/assets/docs/street_fov_60.png).
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="width">Largura em pixels.</param>
        /// <param name="height">Altura em pixels.</param>
        /// <param name="fov">Campo de visão em graus.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<FileResponse> Visão_da_RuaAsync(string taxId, double? width, double? height, double? fov, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (taxId == null)
                throw new System.ArgumentNullException("taxId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/office/{taxId}/street?");
            urlBuilder_.Replace("{taxId}", Uri.EscapeDataString(ConvertToString(taxId, System.Globalization.CultureInfo.InvariantCulture)));
            if (width != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("width") + "=").Append(Uri.EscapeDataString(ConvertToString(width, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (height != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("height") + "=").Append(Uri.EscapeDataString(ConvertToString(height, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (fov != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("fov") + "=").Append(Uri.EscapeDataString(ConvertToString(fov, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("image/png"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200 || status_ == 206)
                        {
                            var responseStream_ = response_.Content == null ? System.IO.Stream.Null : await response_.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            var fileResponse_ = new FileResponse(status_, headers_, responseStream_, null, response_);
                            disposeClient_ = false;
                            disposeResponse_ = false; // response and client are disposed by FileResponse
                            return fileResponse_;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Adiciona uma nova lista em sua conta, é possível criar até mil listas.
        /// </remarks>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Criação bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<ListDto> Cria_ListaAsync(string authorization, ListCreateDto body)
        {
            return Cria_ListaAsync(authorization, body, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Adiciona uma nova lista em sua conta, é possível criar até mil listas.
        /// </remarks>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Criação bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<ListDto> Cria_ListaAsync(string authorization, ListCreateDto body, System.Threading.CancellationToken cancellationToken)
        {
            if (body == null)
                throw new System.ArgumentNullException("body");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/list");

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    var json_ = Newtonsoft.Json.JsonConvert.SerializeObject(body, _settings.Value);
                    var content_ = new System.Net.Http.StringContent(json_);
                    content_.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                    request_.Content = content_;
                    request_.Method = new System.Net.Http.HttpMethod("POST");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 201)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ListDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Encontra as listas que correspondem aos filtros configurados.
        /// </remarks>
        /// <param name="token">Token de paginação, mutualmente exclusivo com as demais propriedades.</param>
        /// <param name="limit">Quantidade de registros a serem lidos por página.</param>
        /// <param name="search">Termo a ser pesquisado no título ou descrição.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<ListPageDto> Pesquisa_ListasAsync(string token, double? limit, string search, string authorization)
        {
            return Pesquisa_ListasAsync(token, limit, search, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Encontra as listas que correspondem aos filtros configurados.
        /// </remarks>
        /// <param name="token">Token de paginação, mutualmente exclusivo com as demais propriedades.</param>
        /// <param name="limit">Quantidade de registros a serem lidos por página.</param>
        /// <param name="search">Termo a ser pesquisado no título ou descrição.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<ListPageDto> Pesquisa_ListasAsync(string token, double? limit, string search, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/list?");
            if (token != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("token") + "=").Append(Uri.EscapeDataString(ConvertToString(token, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (limit != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("limit") + "=").Append(Uri.EscapeDataString(ConvertToString(limit, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (search != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("search") + "=").Append(Uri.EscapeDataString(ConvertToString(search, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ListPageDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Adquire informações de uma lista através de seu identificador único.
        /// </remarks>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<ListDto> Consulta_Lista_Pelo_IDAsync(System.Guid listId, string authorization)
        {
            return Consulta_Lista_Pelo_IDAsync(listId, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Adquire informações de uma lista através de seu identificador único.
        /// </remarks>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<ListDto> Consulta_Lista_Pelo_IDAsync(System.Guid listId, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (listId == null)
                throw new System.ArgumentNullException("listId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/list/{listId}");
            urlBuilder_.Replace("{listId}", Uri.EscapeDataString(ConvertToString(listId, System.Globalization.CultureInfo.InvariantCulture)));

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ListDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Modifica as informações de uma lista através de seu identificador único.
        /// </remarks>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Atualização bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<ListDto> Atualiza_Lista_Pelo_IDAsync(System.Guid listId, string authorization, ListUpdateDto body)
        {
            return Atualiza_Lista_Pelo_IDAsync(listId, authorization, body, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Modifica as informações de uma lista através de seu identificador único.
        /// </remarks>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Atualização bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<ListDto> Atualiza_Lista_Pelo_IDAsync(System.Guid listId, string authorization, ListUpdateDto body, System.Threading.CancellationToken cancellationToken)
        {
            if (listId == null)
                throw new System.ArgumentNullException("listId");

            if (body == null)
                throw new System.ArgumentNullException("body");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/list/{listId}");
            urlBuilder_.Replace("{listId}", Uri.EscapeDataString(ConvertToString(listId, System.Globalization.CultureInfo.InvariantCulture)));

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    var json_ = Newtonsoft.Json.JsonConvert.SerializeObject(body, _settings.Value);
                    var content_ = new System.Net.Http.StringContent(json_);
                    content_.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                    request_.Content = content_;
                    request_.Method = new System.Net.Http.HttpMethod("PATCH");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ListDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Apaga uma lista através de seu identificador único.
        /// </remarks>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Remoção bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task Remove_Lista_Pelo_IDAsync(System.Guid listId, string authorization)
        {
            return Remove_Lista_Pelo_IDAsync(listId, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Apaga uma lista através de seu identificador único.
        /// </remarks>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Remoção bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task Remove_Lista_Pelo_IDAsync(System.Guid listId, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (listId == null)
                throw new System.ArgumentNullException("listId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/list/{listId}");
            urlBuilder_.Replace("{listId}", Uri.EscapeDataString(ConvertToString(listId, System.Globalization.CultureInfo.InvariantCulture)));

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("DELETE");

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 204)
                        {
                            return;
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Adquire de forma centralizada múltiplas informações de um estabelecimento, incluindo acesso a diversos portais públicos.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)  
        /// <br/>• [Simples Nacional 🡭](https://www8.receita.fazenda.gov.br/SimplesNacional/aplicacoes.aspx?id=21)  
        /// <br/>• [Cadastro de Contribuintes 🡭](https://dfe-portal.svrs.rs.gov.br/NFE/CCC)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados podem ser obtidos online em tempo real, ou previamente armazenados em cache.  
        /// <br/>• O modo de resolução irá respeitar a defasagem máxima fornecida em `maxAge`.  
        /// <br/>• Atualizações individuais sob demanda.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta online.  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta em cache.  
        /// <br/>• Sujeito a adicionais conforme escolhas de parâmetros.
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="enable_cache_fallback">Habilita retornar dados em cache caso a busca em tempo real falhe.</param>
        /// <param name="company_max_age">Idade máxima, em dias, que um dado em cache é aceite.</param>
        /// <param name="simples_max_age">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona as informações de opção pelo Simples e
        /// <br/>enquadramento no MEI.</param>
        /// <param name="sintegra_max_age">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona a lista de Inscrições Estaduais.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<LegacyCompanyDto> CnpjAsync(string taxId, bool? enable_cache_fallback, double? company_max_age, double? simples_max_age, double? sintegra_max_age, string authorization)
        {
            return CnpjAsync(taxId, enable_cache_fallback, company_max_age, simples_max_age, sintegra_max_age, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Adquire de forma centralizada múltiplas informações de um estabelecimento, incluindo acesso a diversos portais públicos.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)  
        /// <br/>• [Simples Nacional 🡭](https://www8.receita.fazenda.gov.br/SimplesNacional/aplicacoes.aspx?id=21)  
        /// <br/>• [Cadastro de Contribuintes 🡭](https://dfe-portal.svrs.rs.gov.br/NFE/CCC)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados podem ser obtidos online em tempo real, ou previamente armazenados em cache.  
        /// <br/>• O modo de resolução irá respeitar a defasagem máxima fornecida em `maxAge`.  
        /// <br/>• Atualizações individuais sob demanda.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta online.  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta em cache.  
        /// <br/>• Sujeito a adicionais conforme escolhas de parâmetros.
        /// </remarks>
        /// <param name="taxId">Número do CNPJ sem pontuação.</param>
        /// <param name="enable_cache_fallback">Habilita retornar dados em cache caso a busca em tempo real falhe.</param>
        /// <param name="company_max_age">Idade máxima, em dias, que um dado em cache é aceite.</param>
        /// <param name="simples_max_age">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona as informações de opção pelo Simples e
        /// <br/>enquadramento no MEI.</param>
        /// <param name="sintegra_max_age">+&lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; Adiciona a lista de Inscrições Estaduais.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<LegacyCompanyDto> CnpjAsync(string taxId, bool? enable_cache_fallback, double? company_max_age, double? simples_max_age, double? sintegra_max_age, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (taxId == null)
                throw new System.ArgumentNullException("taxId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/companies/{taxId}?");
            urlBuilder_.Replace("{taxId}", Uri.EscapeDataString(ConvertToString(taxId, System.Globalization.CultureInfo.InvariantCulture)));
            if (enable_cache_fallback != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("enable_cache_fallback") + "=").Append(Uri.EscapeDataString(ConvertToString(enable_cache_fallback, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (company_max_age != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("company_max_age") + "=").Append(Uri.EscapeDataString(ConvertToString(company_max_age, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (simples_max_age != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("simples_max_age") + "=").Append(Uri.EscapeDataString(ConvertToString(simples_max_age, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (sintegra_max_age != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("sintegra_max_age") + "=").Append(Uri.EscapeDataString(ConvertToString(sintegra_max_age, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<LegacyCompanyDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 503)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorServiceUnavailableDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorServiceUnavailableDto>("Consulta online temporariamente indispon\u00edvel.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Adquire a quantidade de créditos restantes em sua conta.
        /// </remarks>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<CreditoDto> BalanceAsync(string authorization)
        {
            return BalanceAsync(authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Adquire a quantidade de créditos restantes em sua conta.
        /// </remarks>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<CreditoDto> BalanceAsync(string authorization, System.Threading.CancellationToken cancellationToken)
        {
            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/credit");

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<CreditoDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Adquire os dados de uma empresa incluindo todos os sócios e estabelecimentos.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)  
        /// <br/>• [Simples Nacional 🡭](https://www8.receita.fazenda.gov.br/SimplesNacional/aplicacoes.aspx?id=21)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são previamente armazenados em cache.  
        /// <br/>• Atualizações individuais sob demanda.  
        /// <br/>• Atualizações globais a cada 30 dias.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta.
        /// </remarks>
        /// <param name="companyId">Código da empresa, idem aos oito primeiros caracteres do CNPJ.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<CompanyDto> CompanyAsync(double companyId, string authorization)
        {
            return CompanyAsync(companyId, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Adquire os dados de uma empresa incluindo todos os sócios e estabelecimentos.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Receita Federal 🡭](https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp)  
        /// <br/>• [Simples Nacional 🡭](https://www8.receita.fazenda.gov.br/SimplesNacional/aplicacoes.aspx?id=21)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são previamente armazenados em cache.  
        /// <br/>• Atualizações individuais sob demanda.  
        /// <br/>• Atualizações globais a cada 30 dias.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta.
        /// </remarks>
        /// <param name="companyId">Código da empresa, idem aos oito primeiros caracteres do CNPJ.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<CompanyDto> CompanyAsync(double companyId, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (companyId == null)
                throw new System.ArgumentNullException("companyId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/company/{companyId}");
            urlBuilder_.Replace("{companyId}", Uri.EscapeDataString(ConvertToString(companyId, System.Globalization.CultureInfo.InvariantCulture)));

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<CompanyDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Adquire junto ao Cadastro Centralizado de Contribuintes os números das inscrições estaduais e situações cadastrais. É possível informar o CNPJ de um estabelecimento, ou o CPF de um produtor rural.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Cadastro de Contribuintes 🡭](https://dfe-portal.svrs.rs.gov.br/NFE/CCC)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados podem ser obtidos online em tempo real, ou previamente armazenados em cache.  
        /// <br/>• O modo de resolução irá respeitar a defasagem máxima fornecida em `maxAge`.  
        /// <br/>• Atualizações individuais sob demanda.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta online.  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta em cache.
        /// </remarks>
        /// <param name="taxId">CNPJ ou CPF de produtor rural</param>
        /// <param name="states">Unidades Federativas para consulta separadas por vírgula, utilize `BR` para considerar todas.
        /// <br/>Consultas de CPF de produtor rural exigem informar a UF exata.</param>
        /// <param name="strategy">Estratégia de cache utilizada na aquisição dos dados:  
        /// <br/>• `CACHE`: Entrega os dados do cache, evitando cobranças de créditos, se os dados não estiverem disponíveis resultará em um erro 404.  
        /// <br/>• `CACHE_IF_FRESH`: Retorna os dados do cache respeitando o limite em `maxAge`, se os dados estiverem desatualizados será consultado online.  
        /// <br/>• `CACHE_IF_ERROR`: Idem ao `CACHE_IF_FRESH`, mas se a consulta online falhar retorna os dados do cache respeitando o limite em `maxStale`.  
        /// <br/>• `ONLINE`: Consulta diretamente online, não recomendado pois ignora qualquer cache, sugerimos configurar `maxAge=1` como alternativa.</param>
        /// <param name="maxAge">Idade máxima, em dias, que um dado em cache é aceite, relevante para as estratégias `CACHE_IF_FRESH` e `CACHE_IF_ERROR`.</param>
        /// <param name="maxStale">Idade máxima, em dias, que um dado em cache é aceite em caso de erro na busca online, relevante apenas para a estratégia `CACHE_IF_FRESH`.</param>
        /// <param name="sync">Aguarda a compensação dos créditos de forma síncrona, retornando o cabeçalho `cnpja-request-cost`.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<CccDto> Consulta_CCCAsync(string taxId, System.Collections.Generic.IEnumerable<UF> states, Strategy? strategy, double? maxAge, double? maxStale, bool? sync, string authorization)
        {
            return Consulta_CCCAsync(taxId, states, strategy, maxAge, maxStale, sync, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Adquire junto ao Cadastro Centralizado de Contribuintes os números das inscrições estaduais e situações cadastrais. É possível informar o CNPJ de um estabelecimento, ou o CPF de um produtor rural.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Cadastro de Contribuintes 🡭](https://dfe-portal.svrs.rs.gov.br/NFE/CCC)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados podem ser obtidos online em tempo real, ou previamente armazenados em cache.  
        /// <br/>• O modo de resolução irá respeitar a defasagem máxima fornecida em `maxAge`.  
        /// <br/>• Atualizações individuais sob demanda.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta online.  
        /// <br/>• &lt;span style="color: #DFE3E6"&gt;0 ₪&lt;/span&gt; por consulta em cache.
        /// </remarks>
        /// <param name="taxId">CNPJ ou CPF de produtor rural</param>
        /// <param name="states">Unidades Federativas para consulta separadas por vírgula, utilize `BR` para considerar todas.
        /// <br/>Consultas de CPF de produtor rural exigem informar a UF exata.</param>
        /// <param name="strategy">Estratégia de cache utilizada na aquisição dos dados:  
        /// <br/>• `CACHE`: Entrega os dados do cache, evitando cobranças de créditos, se os dados não estiverem disponíveis resultará em um erro 404.  
        /// <br/>• `CACHE_IF_FRESH`: Retorna os dados do cache respeitando o limite em `maxAge`, se os dados estiverem desatualizados será consultado online.  
        /// <br/>• `CACHE_IF_ERROR`: Idem ao `CACHE_IF_FRESH`, mas se a consulta online falhar retorna os dados do cache respeitando o limite em `maxStale`.  
        /// <br/>• `ONLINE`: Consulta diretamente online, não recomendado pois ignora qualquer cache, sugerimos configurar `maxAge=1` como alternativa.</param>
        /// <param name="maxAge">Idade máxima, em dias, que um dado em cache é aceite, relevante para as estratégias `CACHE_IF_FRESH` e `CACHE_IF_ERROR`.</param>
        /// <param name="maxStale">Idade máxima, em dias, que um dado em cache é aceite em caso de erro na busca online, relevante apenas para a estratégia `CACHE_IF_FRESH`.</param>
        /// <param name="sync">Aguarda a compensação dos créditos de forma síncrona, retornando o cabeçalho `cnpja-request-cost`.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<CccDto> Consulta_CCCAsync(string taxId, System.Collections.Generic.IEnumerable<UF> states, Strategy? strategy, double? maxAge, double? maxStale, bool? sync, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (taxId == null)
                throw new System.ArgumentNullException("taxId");

            if (states == null)
                throw new System.ArgumentNullException("states");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/ccc?");
            urlBuilder_.Append(Uri.EscapeDataString("taxId") + "=").Append(Uri.EscapeDataString(ConvertToString(taxId, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            foreach (var item_ in states)
            {
                urlBuilder_.Append(Uri.EscapeDataString("states") + "=").Append(Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (strategy != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("strategy") + "=").Append(Uri.EscapeDataString(ConvertToString(strategy, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (maxAge != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("maxAge") + "=").Append(Uri.EscapeDataString(ConvertToString(maxAge, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (maxStale != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("maxStale") + "=").Append(Uri.EscapeDataString(ConvertToString(maxStale, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (sync != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("sync") + "=").Append(Uri.EscapeDataString(ConvertToString(sync, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<CccDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 503)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorServiceUnavailableDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorServiceUnavailableDto>("Consulta online temporariamente indispon\u00edvel.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <remarks>
        /// Emite o comprovante em PDF do registro no Cadastro Centralizado de Contribuintes.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Cadastro de Contribuintes 🡭](https://dfe-portal.svrs.rs.gov.br/NFE/CCC)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são obtidos online em tempo real.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Exemplos&lt;/span&gt;  
        /// <br/>• [Estabelecimento Sem Restrição](/proxy/api/assets/docs/ccc_certificate_01.pdf).  
        /// <br/>• [Estabelecimento Bloqueado](/proxy/api/assets/docs/ccc_certificate_02.pdf).  
        /// <br/>• [Estabelecimento com Múltiplas Inscrições](/proxy/api/assets/docs/ccc_certificate_03.pdf).  
        /// <br/>• [Produtor Rural](/proxy/api/assets/docs/ccc_certificate_04.pdf).
        /// </remarks>
        /// <param name="taxId">Número do CNPJ ou CPF sem pontuação.</param>
        /// <param name="state">Unidade Federativa de origem. Consultas de CPF de produtor rural exigem informar a UF exata.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<FileResponse> Comprovante_CCCAsync(string taxId, UF? state, string authorization)
        {
            return Comprovante_CCCAsync(taxId, state, authorization, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// Emite o comprovante em PDF do registro no Cadastro Centralizado de Contribuintes.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Fontes de Dados&lt;/span&gt;  
        /// <br/>• [Cadastro de Contribuintes 🡭](https://dfe-portal.svrs.rs.gov.br/NFE/CCC)
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Frequência de Atualizações&lt;/span&gt;  
        /// <br/>• Os dados são obtidos online em tempo real.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Custo&lt;/span&gt;  
        /// <br/>• &lt;span style="color: #EAED37"&gt;1 ₪&lt;/span&gt; por consulta.
        /// <br/>
        /// <br/>&lt;span style="color: #DFE3E6"&gt;Exemplos&lt;/span&gt;  
        /// <br/>• [Estabelecimento Sem Restrição](/proxy/api/assets/docs/ccc_certificate_01.pdf).  
        /// <br/>• [Estabelecimento Bloqueado](/proxy/api/assets/docs/ccc_certificate_02.pdf).  
        /// <br/>• [Estabelecimento com Múltiplas Inscrições](/proxy/api/assets/docs/ccc_certificate_03.pdf).  
        /// <br/>• [Produtor Rural](/proxy/api/assets/docs/ccc_certificate_04.pdf).
        /// </remarks>
        /// <param name="taxId">Número do CNPJ ou CPF sem pontuação.</param>
        /// <param name="state">Unidade Federativa de origem. Consultas de CPF de produtor rural exigem informar a UF exata.</param>
        /// <param name="authorization">[Chave de API 🡭](https://cnpja.com/me/api-key)</param>
        /// <returns>Consulta bem sucedida.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<FileResponse> Comprovante_CCCAsync(string taxId, UF? state, string authorization, System.Threading.CancellationToken cancellationToken)
        {
            if (taxId == null)
                throw new System.ArgumentNullException("taxId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/ccc/certificate?");
            urlBuilder_.Append(Uri.EscapeDataString("taxId") + "=").Append(Uri.EscapeDataString(ConvertToString(taxId, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            if (state != null)
            {
                urlBuilder_.Append(Uri.EscapeDataString("state") + "=").Append(Uri.EscapeDataString(ConvertToString(state, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {

                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/pdf"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200 || status_ == 206)
                        {
                            var responseStream_ = response_.Content == null ? System.IO.Stream.Null : await response_.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            var fileResponse_ = new FileResponse(status_, headers_, responseStream_, null, response_);
                            disposeClient_ = false;
                            disposeResponse_ = false; // response and client are disposed by FileResponse
                            return fileResponse_;
                        }
                        else
                        if (status_ == 400)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorBadRequestDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorBadRequestDto>("Par\u00e2metro de consulta mal formatado ou faltante.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 401)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorUnauthorizedDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorUnauthorizedDto>("Chave de API ausente ou incorreta.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 404)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorNotFoundDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorNotFoundDto>("Entidade pesquisada n\u00e3o registrada no \u00f3rg\u00e3o.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 429)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorTooManyRequestsDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorTooManyRequestsDto>("Cr\u00e9ditos esgotados ou limite por minuto excedido.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 500)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorInternalServerErrorDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorInternalServerErrorDto>("Erro interno ou falha inesperada.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ == 503)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ErrorServiceUnavailableDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            throw new ApiException<ErrorServiceUnavailableDto>("Consulta online temporariamente indispon\u00edvel.", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        protected struct ObjectResponseResult<T>
        {
            public ObjectResponseResult(T responseObject, string responseText)
            {
                this.Object = responseObject;
                this.Text = responseText;
            }

            public T Object
            {
                get;
            }

            public string Text
            {
                get;
            }
        }

        public bool ReadResponseAsString
        {
            get; set;
        }

        protected virtual async System.Threading.Tasks.Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(System.Net.Http.HttpResponseMessage response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.Threading.CancellationToken cancellationToken)
        {
            if (response == null || response.Content == null)
            {
                return new ObjectResponseResult<T>(default(T), string.Empty);
            }

            if (ReadResponseAsString)
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var typedBody = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseText, JsonSerializerSettings);
                    return new ObjectResponseResult<T>(typedBody, responseText);
                }
                catch (Newtonsoft.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, responseText, headers, exception);
                }
            }
            else
            {
                try
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var streamReader = new System.IO.StreamReader(responseStream))
                    using (var jsonTextReader = new Newtonsoft.Json.JsonTextReader(streamReader))
                    {
                        var serializer = Newtonsoft.Json.JsonSerializer.Create(JsonSerializerSettings);
                        var typedBody = serializer.Deserialize<T>(jsonTextReader);
                        return new ObjectResponseResult<T>(typedBody, string.Empty);
                    }
                }
                catch (Newtonsoft.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
                }
            }
        }

        private string ConvertToString(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return "";
            }

            if (value is System.Enum)
            {
                var name = System.Enum.GetName(value.GetType(), value);
                if (name != null)
                {
                    var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                    if (field != null)
                    {
                        var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute))
                            as System.Runtime.Serialization.EnumMemberAttribute;
                        if (attribute != null)
                        {
                            return attribute.Value != null ? attribute.Value : name;
                        }
                    }

                    var converted = System.Convert.ToString(System.Convert.ChangeType(value, System.Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                    return converted == null ? string.Empty : converted;
                }
            }
            else if (value is bool)
            {
                return System.Convert.ToString((bool)value, cultureInfo).ToLowerInvariant();
            }
            else if (value is byte[])
            {
                return System.Convert.ToBase64String((byte[])value);
            }
            else if (value.GetType().IsArray)
            {
                var array = System.Linq.Enumerable.OfType<object>((System.Array)value);
                return string.Join(",", System.Linq.Enumerable.Select(array, o => ConvertToString(o, cultureInfo)));
            }

            var result = System.Convert.ToString(value, cultureInfo);
            return result == null ? "" : result;
        }
    }


    public partial class ErrorBadRequestDto
    {
        /// <summary>
        /// Código do status HTTP.
        /// </summary>
        [JsonPropertyName("code")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Code
        {
            get; set;
        }

        /// <summary>
        /// Mensagem de erro.
        /// </summary>
        [JsonPropertyName("message")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Message
        {
            get; set;
        }

        /// <summary>
        /// Lista com as falhas de validação.
        /// </summary>
        [JsonPropertyName("constraints")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<string> Constraints { get; set; } = new List<string>();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class ErrorUnauthorizedDto
    {
        /// <summary>
        /// Código do status HTTP.
        /// </summary>
        [JsonPropertyName("code")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Code
        {
            get; set;
        }

        /// <summary>
        /// Mensagem de erro.
        /// </summary>
        [JsonPropertyName("message")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Message
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class ErrorNotFoundDto
    {
        /// <summary>
        /// Código do status HTTP.
        /// </summary>
        [JsonPropertyName("code")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Code
        {
            get; set;
        }

        /// <summary>
        /// Mensagem de erro.
        /// </summary>
        [JsonPropertyName("message")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Message
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class ErrorTooManyRequestsDto
    {
        /// <summary>
        /// Código do status HTTP.
        /// </summary>
        [JsonPropertyName("code")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Code
        {
            get; set;
        }

        /// <summary>
        /// Mensagem de erro.
        /// </summary>
        [JsonPropertyName("message")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Message
        {
            get; set;
        }

        /// <summary>
        /// Créditos necessários para completar a consulta.
        /// </summary>
        [JsonPropertyName("required")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Required
        {
            get; set;
        }

        /// <summary>
        /// Créditos restantes em sua conta.
        /// </summary>
        [JsonPropertyName("remaining")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Remaining
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class ErrorInternalServerErrorDto
    {
        /// <summary>
        /// Código do status HTTP.
        /// </summary>
        [JsonPropertyName("code")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Code
        {
            get; set;
        }

        /// <summary>
        /// Mensagem de erro.
        /// </summary>
        [JsonPropertyName("message")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Message
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class ErrorServiceUnavailableDto
    {
        /// <summary>
        /// Código do status HTTP.
        /// </summary>
        [JsonPropertyName("code")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Code
        {
            get; set;
        }

        /// <summary>
        /// Mensagem de erro.
        /// </summary>
        [JsonPropertyName("message")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Message
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class ZipDto
    {
        /// <summary>
        /// Data da última atualização.
        /// </summary>
        [JsonPropertyName("updated")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Updated
        {
            get; set;
        }

        /// <summary>
        /// Código do município conforme
        /// <br/>[IBGE 🡭](https://www.ibge.gov.br/explica/codigos-dos-municipios.php).
        /// </summary>
        [JsonPropertyName("municipality")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Municipality
        {
            get; set;
        }

        /// <summary>
        /// Código de Endereçamento Postal.
        /// </summary>
        [JsonPropertyName("code")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        [System.ComponentModel.DataAnnotations.StringLength(8, MinimumLength = 8)]
        public string Code
        {
            get; set;
        }

        /// <summary>
        /// Logradouro.
        /// </summary>
        [JsonPropertyName("street")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Street
        {
            get; set;
        }

        /// <summary>
        /// Número.
        /// </summary>
        [JsonPropertyName("number")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Number
        {
            get; set;
        }

        /// <summary>
        /// Bairro ou distrito.
        /// </summary>
        [JsonPropertyName("district")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string District
        {
            get; set;
        }

        /// <summary>
        /// Município.
        /// </summary>
        [JsonPropertyName("city")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string City
        {
            get; set;
        }

        /// <summary>
        /// Sigla da Unidade Federativa.
        /// </summary>
        [JsonPropertyName("state")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public UF State
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class SuframaStatusDto
    {
        /// <summary>
        /// Código da situação cadastral:  
        /// <br/>• `1`: Ativa.  
        /// <br/>• `2`: Inativa.  
        /// <br/>• `3`: Bloqueada.  
        /// <br/>• `4`: Cancelada.  
        /// <br/>• `5`: Cancelada Ag. Rec.
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Descrição da situação cadastral.
        /// </summary>
        [JsonPropertyName("text")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Text
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class NatureDto
    {
        /// <summary>
        /// Código da natureza jurídica conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/estrutura/natjur-estrutura/natureza-juridica-2021).
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Descrição da natureza jurídica.
        /// </summary>
        [JsonPropertyName("text")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Text
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class CountryDto
    {
        /// <summary>
        /// Código do país conforme [M49 🡭](https://unstats.un.org/unsd/methodology/m49/).
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Nome do país.
        /// </summary>
        [JsonPropertyName("name")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Name
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class AddressDto
    {
        /// <summary>
        /// Código do município conforme
        /// <br/>[IBGE 🡭](https://www.ibge.gov.br/explica/codigos-dos-municipios.php).
        /// </summary>
        [JsonPropertyName("municipality")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Municipality
        {
            get; set;
        }

        /// <summary>
        /// Logradouro.
        /// </summary>
        [JsonPropertyName("street")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Street
        {
            get; set;
        }

        /// <summary>
        /// Número.
        /// </summary>
        [JsonPropertyName("number")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Number
        {
            get; set;
        }

        /// <summary>
        /// Bairro ou distrito.
        /// </summary>
        [JsonPropertyName("district")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string District
        {
            get; set;
        }

        /// <summary>
        /// Município.
        /// </summary>
        [JsonPropertyName("city")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string City
        {
            get; set;
        }

        /// <summary>
        /// Sigla da Unidade Federativa.
        /// </summary>
        [JsonPropertyName("state")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public UF State
        {
            get; set;
        }

        /// <summary>
        /// Complemento.
        /// </summary>
        [JsonPropertyName("details")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Details
        {
            get; set;
        }

        /// <summary>
        /// Código de Endereçamento Postal.
        /// </summary>
        [JsonPropertyName("zip")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        [System.ComponentModel.DataAnnotations.StringLength(8, MinimumLength = 8)]
        public string Zip
        {
            get; set;
        }

        /// <summary>
        /// Latitude.
        /// </summary>
        [JsonPropertyName("latitude")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float Latitude
        {
            get; set;
        }

        /// <summary>
        /// Longitude.
        /// </summary>
        [JsonPropertyName("longitude")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float Longitude
        {
            get; set;
        }

        /// <summary>
        /// Informações do país.
        /// </summary>
        [JsonPropertyName("country")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public CountryDto Country { get; set; } = new CountryDto();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class PhoneDto
    {
        /// <summary>
        /// Tipo do telefone:  
        /// <br/>• `LANDLINE`: Linha terrestre, telefone fixo.  
        /// <br/>• `MOBILE`: Linha móvel, telefone celular.
        /// </summary>
        [JsonPropertyName("type")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public PhoneDtoType Type
        {
            get; set;
        }

        /// <summary>
        /// Código de DDD.
        /// </summary>
        [JsonPropertyName("area")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        [System.ComponentModel.DataAnnotations.StringLength(2, MinimumLength = 2)]
        public string Area
        {
            get; set;
        }

        /// <summary>
        /// Número.
        /// </summary>
        [JsonPropertyName("number")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        [System.ComponentModel.DataAnnotations.StringLength(9, MinimumLength = 8)]
        public string Number
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class EmailDto
    {
        /// <summary>
        /// Tipo de propriedade do e-mail:  
        /// <br/>• `PERSONAL`: Pessoal, registrado em provedor gratuito.  
        /// <br/>• `CORPORATE`: Corporativo, registrado em provedor privado.  
        /// <br/>• `ACCOUNTING`: Contabilidade, domínio remete a empresas de contadores.
        /// </summary>
        [JsonPropertyName("ownership")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public EmailDtoOwnership Ownership
        {
            get; set;
        }

        /// <summary>
        /// Endereço de e-mail.
        /// </summary>
        [JsonPropertyName("address")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Address
        {
            get; set;
        }

        /// <summary>
        /// Domínio de registro.
        /// </summary>
        [JsonPropertyName("domain")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Domain
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class SuframaActivityDto
    {
        /// <summary>
        /// Código da atividade econômica conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/busca-online-cnae.html?view=estrutura).
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Descrição da atividade econômica.
        /// </summary>
        [JsonPropertyName("text")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Text
        {
            get; set;
        }

        /// <summary>
        /// Indica se a atividade econômica é exercida.
        /// </summary>
        [JsonPropertyName("performed")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Performed
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class SuframaIncentiveDto
    {
        /// <summary>
        /// Nome do tributo incentivado.
        /// </summary>
        [JsonPropertyName("tribute")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public SuframaIncentiveDtoTribute Tribute
        {
            get; set;
        }

        /// <summary>
        /// Benefício aplicado ao incentivo.
        /// </summary>
        [JsonPropertyName("benefit")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Benefit
        {
            get; set;
        }

        /// <summary>
        /// Finalidade do incentivo.
        /// </summary>
        [JsonPropertyName("purpose")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Purpose
        {
            get; set;
        }

        /// <summary>
        /// Base legal do incentivo.
        /// </summary>
        [JsonPropertyName("basis")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Basis
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class SuframaDto
    {
        /// <summary>
        /// Número do CNPJ ou CPF sem pontuação.
        /// </summary>
        [JsonPropertyName("taxId")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string TaxId
        {
            get; set;
        }

        /// <summary>
        /// Data da última atualização.
        /// </summary>
        [JsonPropertyName("updated")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Updated
        {
            get; set;
        }

        /// <summary>
        /// Número da inscrição SUFRAMA.
        /// </summary>
        [JsonPropertyName("number")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Number
        {
            get; set;
        }

        /// <summary>
        /// Razão social.
        /// </summary>
        [JsonPropertyName("name")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Data de inscrição na SUFRAMA.
        /// </summary>
        [JsonPropertyName("since")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Since
        {
            get; set;
        }

        /// <summary>
        /// Indica se o estabelecimento é a Matriz.
        /// </summary>
        [JsonPropertyName("head")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Head
        {
            get; set;
        }

        /// <summary>
        /// Indica se o projeto está aprovado.
        /// </summary>
        [JsonPropertyName("approved")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Approved
        {
            get; set;
        }

        /// <summary>
        /// Data de aprovação do projeto.
        /// </summary>
        [JsonPropertyName("approvalDate")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string ApprovalDate
        {
            get; set;
        }

        /// <summary>
        /// Informações da situação cadastral.
        /// </summary>
        [JsonPropertyName("status")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public SuframaStatusDto Status { get; set; } = new SuframaStatusDto();

        /// <summary>
        /// Informações da natureza jurídica.
        /// </summary>
        [JsonPropertyName("nature")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public NatureDto Nature { get; set; } = new NatureDto();

        /// <summary>
        /// Informações do endereço.
        /// </summary>
        [JsonPropertyName("address")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public AddressDto Address { get; set; } = new AddressDto();

        /// <summary>
        /// Lista de telefones.
        /// </summary>
        [JsonPropertyName("phones")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<PhoneDto> Phones { get; set; } = new List<PhoneDto>();

        /// <summary>
        /// Lista de e-mails.
        /// </summary>
        [JsonPropertyName("emails")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<EmailDto> Emails { get; set; } = new List<EmailDto>();

        /// <summary>
        /// Informações da atividade econômica principal.
        /// </summary>
        [JsonPropertyName("mainActivity")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public SuframaActivityDto MainActivity { get; set; } = new SuframaActivityDto();

        /// <summary>
        /// Lista de atividades econômicas secundárias.
        /// </summary>
        [JsonPropertyName("sideActivities")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<SuframaActivityDto> SideActivities { get; set; } = new List<SuframaActivityDto>();

        /// <summary>
        /// Lista de incentivos fiscais.
        /// </summary>
        [JsonPropertyName("incentives")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<SuframaIncentiveDto> Incentives { get; set; } = new List<SuframaIncentiveDto>();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class Buffer
    {

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class SimplesSimeiHistoryDto
    {
        /// <summary>
        /// Data de início do período.
        /// </summary>
        [JsonPropertyName("from")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string From
        {
            get; set;
        }

        /// <summary>
        /// Data de término do período.
        /// </summary>
        [JsonPropertyName("until")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Until
        {
            get; set;
        }

        /// <summary>
        /// Motivo de encerramento.
        /// </summary>
        [JsonPropertyName("text")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Text
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class SimplesSimeiDto
    {
        /// <summary>
        /// Indica se optante ou enquadrado.
        /// </summary>
        [JsonPropertyName("optant")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Optant
        {
            get; set;
        }

        /// <summary>
        /// Data de inclusão no período vigente.
        /// </summary>
        [JsonPropertyName("since")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Since
        {
            get; set;
        }

        /// <summary>
        /// Histórico de períodos anteriores.
        /// </summary>
        [JsonPropertyName("history")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<SimplesSimeiHistoryDto> History
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class SimplesDto
    {
        /// <summary>
        /// Número do CNPJ sem pontuação.
        /// </summary>
        [JsonPropertyName("taxId")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string TaxId
        {
            get; set;
        }

        /// <summary>
        /// Data da última atualização.
        /// </summary>
        [JsonPropertyName("updated")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Updated
        {
            get; set;
        }

        /// <summary>
        /// Informações da opção pelo Simples Nacional.
        /// </summary>
        [JsonPropertyName("simples")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public SimplesSimeiDto Simples { get; set; } = new SimplesSimeiDto();

        /// <summary>
        /// Informações do enquadramento no MEI.
        /// </summary>
        [JsonPropertyName("simei")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public SimplesSimeiDto Simei { get; set; } = new SimplesSimeiDto();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class CompanySizeDto
    {
        /// <summary>
        /// Código do porte:  
        /// <br/>• `1`: Microempresa (ME).  
        /// <br/>• `3`: Empresa de Pequeno Porte (EPP).  
        /// <br/>• `5`: Demais.
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Sigla do porte.
        /// </summary>
        [JsonPropertyName("acronym")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Acronym
        {
            get; set;
        }

        /// <summary>
        /// Descrição do porte.
        /// </summary>
        [JsonPropertyName("text")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Text
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class OfficeStatusDto
    {
        /// <summary>
        /// Código da situação cadastral:  
        /// <br/>• `1`: Nula.  
        /// <br/>• `2`: Ativa.  
        /// <br/>• `3`: Suspensa.  
        /// <br/>• `4`: Inapta.  
        /// <br/>• `8`: Baixada.
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Descrição da situação cadastral.
        /// </summary>
        [JsonPropertyName("text")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Text
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class OfficeReasonDto
    {
        /// <summary>
        /// Código do motivo da situação cadastral conforme
        /// <br/>[Receita Federal 🡭](http://www.consultas.cge.rj.gov.br/scadastral.pdf).
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Descrição do motivo da situação cadastral.
        /// </summary>
        [JsonPropertyName("text")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Text
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class OfficeSpecialDto
    {
        /// <summary>
        /// Código da situação especial conforme
        /// <br/>[Receita Federal 🡭](http://www38.receita.fazenda.gov.br/cadsincnac/jsp/coleta/ajuda/topicos/Eventos_de_Alteracao.htm).
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Descrição da situação especial.
        /// </summary>
        [JsonPropertyName("text")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Text
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class ActivityDto
    {
        /// <summary>
        /// Código da atividade econômica conforme
        /// <br/>[IBGE 🡭](https://concla.ibge.gov.br/busca-online-cnae.html?view=estrutura).
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Descrição da atividade econômica.
        /// </summary>
        [JsonPropertyName("text")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Text
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class RoleDto
    {
        /// <summary>
        /// Código da qualificação conforme
        /// <br/>[Receita Federal 🡭](http://www.consultas.cge.rj.gov.br/codsocio.pdf).
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Descrição da qualificação.
        /// </summary>
        [JsonPropertyName("text")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Text
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class PersonBaseDto
    {
        /// <summary>
        /// Código da pessoa.
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public System.Guid Id
        {
            get; set;
        }

        /// <summary>
        /// Tipo da pessoa:  
        /// <br/>• `NATURAL`: Pessoa física.  
        /// <br/>• `LEGAL`: Pessoa jurídica.  
        /// <br/>• `FOREIGN`: Pessoa residente no exterior.  
        /// <br/>• `UNKNOWN`: Pessoa desconhecida.
        /// </summary>
        [JsonPropertyName("type")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public PersonBaseDtoType Type
        {
            get; set;
        }

        /// <summary>
        /// Nome ou razão social.
        /// </summary>
        [JsonPropertyName("name")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `type == 'NATURAL' | 'LEGAL'`  
        /// <br/>CPF ou CNPJ.
        /// </summary>
        [JsonPropertyName("taxId")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string TaxId
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `type == 'NATURAL'`  
        /// <br/>Faixa etária.
        /// </summary>
        [JsonPropertyName("age")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        
        public Age Age
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `type == 'FOREIGN'`  
        /// <br/>País de origem.
        /// </summary>
        [JsonPropertyName("country")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CountryDto Country
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class MemberAgentDto
    {
        /// <summary>
        /// Informações da pessoa representante legal.
        /// </summary>
        [JsonPropertyName("person")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public PersonBaseDto Person { get; set; } = new PersonBaseDto();

        /// <summary>
        /// Informações da qualificação do representante legal.
        /// </summary>
        [JsonPropertyName("role")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public RoleDto Role { get; set; } = new RoleDto();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class RfbMemberDto
    {
        /// <summary>
        /// Data de entrada na sociedade.
        /// </summary>
        [JsonPropertyName("since")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Since
        {
            get; set;
        }

        /// <summary>
        /// Informações da qualificação.
        /// </summary>
        [JsonPropertyName("role")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public RoleDto Role { get; set; } = new RoleDto();

        /// <summary>
        /// Informações do sócio ou administrador.
        /// </summary>
        [JsonPropertyName("person")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public PersonBaseDto Person { get; set; } = new PersonBaseDto();

        /// <summary>
        /// Presente quando aplicável na qualificação  
        /// <br/>Informações do representante legal.
        /// </summary>
        [JsonPropertyName("agent")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public MemberAgentDto Agent { get; set; } = new MemberAgentDto();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class RfbDto
    {
        /// <summary>
        /// Número do CNPJ sem pontuação.
        /// </summary>
        [JsonPropertyName("taxId")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string TaxId
        {
            get; set;
        }

        /// <summary>
        /// Data da última atualização.
        /// </summary>
        [JsonPropertyName("updated")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Updated
        {
            get; set;
        }

        /// <summary>
        /// Razão social.
        /// </summary>
        [JsonPropertyName("name")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `nature.id &lt; 2000`  
        /// <br/>Ente federativo responsável.
        /// </summary>
        [JsonPropertyName("jurisdiction")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Jurisdiction
        {
            get; set;
        }

        /// <summary>
        /// Capital social
        /// </summary>
        [JsonPropertyName("equity")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public float Equity
        {
            get; set;
        }

        /// <summary>
        /// Informações da natureza jurídica.
        /// </summary>
        [JsonPropertyName("nature")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public NatureDto Nature { get; set; } = new NatureDto();

        /// <summary>
        /// Informações do porte.
        /// </summary>
        [JsonPropertyName("size")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public CompanySizeDto Size { get; set; } = new CompanySizeDto();

        /// <summary>
        /// Nome fantasia.
        /// </summary>
        [JsonPropertyName("alias")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Alias
        {
            get; set;
        }

        /// <summary>
        /// Data de abertura.
        /// </summary>
        [JsonPropertyName("founded")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Founded
        {
            get; set;
        }

        /// <summary>
        /// Indica se o estabelecimento é a Matriz.
        /// </summary>
        [JsonPropertyName("head")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Head
        {
            get; set;
        }

        /// <summary>
        /// Data da situação cadastral.
        /// </summary>
        [JsonPropertyName("statusDate")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string StatusDate
        {
            get; set;
        }

        /// <summary>
        /// Informações da situação cadastral.
        /// </summary>
        [JsonPropertyName("status")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public OfficeStatusDto Status { get; set; } = new OfficeStatusDto();

        /// <summary>
        /// Presente quando `status.id != 2`  
        /// <br/>Informações do motivo da situação cadastral.
        /// </summary>
        [JsonPropertyName("reason")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OfficeReasonDto Reason
        {
            get; set;
        }

        /// <summary>
        /// Data da situação especial.
        /// </summary>
        [JsonPropertyName("specialDate")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SpecialDate
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `specialDate != undefined`  
        /// <br/>Informações da situação especial.
        /// </summary>
        [JsonPropertyName("special")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OfficeSpecialDto Special
        {
            get; set;
        }

        /// <summary>
        /// Informações do endereço.
        /// </summary>
        [JsonPropertyName("address")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public AddressDto Address { get; set; } = new AddressDto();

        /// <summary>
        /// Lista de telefones.
        /// </summary>
        [JsonPropertyName("phones")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<PhoneDto> Phones { get; set; } = new List<PhoneDto>();

        /// <summary>
        /// Lista de e-mails.
        /// </summary>
        [JsonPropertyName("emails")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<EmailDto> Emails { get; set; } = new List<EmailDto>();

        /// <summary>
        /// Informações da atividade econômica principal.
        /// </summary>
        [JsonPropertyName("mainActivity")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public ActivityDto MainActivity { get; set; } = new ActivityDto();

        /// <summary>
        /// Lista de atividades econômicas secundárias.
        /// </summary>
        [JsonPropertyName("sideActivities")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<ActivityDto> SideActivities { get; set; } = new List<ActivityDto>();

        /// <summary>
        /// Quadro de sócios e administradores.
        /// </summary>
        [JsonPropertyName("members")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<RfbMemberDto> Members { get; set; } = new List<RfbMemberDto>();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class PersonMemberCompanyDto
    {
        /// <summary>
        /// Código da empresa, idem aos oito primeiros caracteres do CNPJ.
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Razão social.
        /// </summary>
        [JsonPropertyName("name")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `nature.id &lt; 2000`  
        /// <br/>Ente federativo responsável.
        /// </summary>
        [JsonPropertyName("jurisdiction")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Jurisdiction
        {
            get; set;
        }

        /// <summary>
        /// Capital social
        /// </summary>
        [JsonPropertyName("equity")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public float Equity
        {
            get; set;
        }

        /// <summary>
        /// Informações da natureza jurídica.
        /// </summary>
        [JsonPropertyName("nature")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public NatureDto Nature { get; set; } = new NatureDto();

        /// <summary>
        /// Informações do porte.
        /// </summary>
        [JsonPropertyName("size")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public CompanySizeDto Size { get; set; } = new CompanySizeDto();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class PersonMemberDto
    {
        /// <summary>
        /// Data de entrada na sociedade.
        /// </summary>
        [JsonPropertyName("since")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Since
        {
            get; set;
        }

        /// <summary>
        /// Informações da qualificação.
        /// </summary>
        [JsonPropertyName("role")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public RoleDto Role { get; set; } = new RoleDto();

        /// <summary>
        /// Presente quando aplicável na qualificação  
        /// <br/>Informações do representante legal.
        /// </summary>
        [JsonPropertyName("agent")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MemberAgentDto Agent
        {
            get; set;
        }

        /// <summary>
        /// Informações da empresa.
        /// </summary>
        [JsonPropertyName("company")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public PersonMemberCompanyDto Company { get; set; } = new PersonMemberCompanyDto();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class PersonDto
    {
        /// <summary>
        /// Código da pessoa.
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public System.Guid Id
        {
            get; set;
        }

        /// <summary>
        /// Tipo da pessoa:  
        /// <br/>• `NATURAL`: Pessoa física.  
        /// <br/>• `LEGAL`: Pessoa jurídica.  
        /// <br/>• `FOREIGN`: Pessoa residente no exterior.  
        /// <br/>• `UNKNOWN`: Pessoa desconhecida.
        /// </summary>
        [JsonPropertyName("type")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public PersonDtoType Type
        {
            get; set;
        }

        /// <summary>
        /// Nome ou razão social.
        /// </summary>
        [JsonPropertyName("name")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `type == 'NATURAL' | 'LEGAL'`  
        /// <br/>CPF ou CNPJ.
        /// </summary>
        [JsonPropertyName("taxId")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string TaxId
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `type == 'NATURAL'`  
        /// <br/>Faixa etária.
        /// </summary>
        [JsonPropertyName("age")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        
        public Age Age
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `type == 'FOREIGN'`  
        /// <br/>País de origem.
        /// </summary>
        [JsonPropertyName("country")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CountryDto Country
        {
            get; set;
        }

        /// <summary>
        /// Lista de sociedades participantes.
        /// </summary>
        [JsonPropertyName("membership")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<PersonMemberDto> Membership { get; set; } = new List<PersonMemberDto>();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class PersonPageDto
    {
        /// <summary>
        /// Token da próxima página.
        /// </summary>
        [JsonPropertyName("next")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        [System.ComponentModel.DataAnnotations.StringLength(32, MinimumLength = 32)]
        public string Next
        {
            get; set;
        }

        /// <summary>
        /// Quantidade de registros lidos.
        /// </summary>
        [JsonPropertyName("limit")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [System.ComponentModel.DataAnnotations.Range(1D, 100D)]
        public double Limit
        {
            get; set;
        }

        /// <summary>
        /// Quantidade de registros disponíveis.
        /// </summary>
        [JsonPropertyName("count")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
        public double Count
        {
            get; set;
        }

        /// <summary>
        /// Lista de pessoas que obedecem aos critérios de pesquisa.
        /// </summary>
        [JsonPropertyName("records")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<PersonDto> Records { get; set; } = new List<PersonDto>();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class MemberDto
    {
        /// <summary>
        /// Data de entrada na sociedade.
        /// </summary>
        [JsonPropertyName("since")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Since
        {
            get; set;
        }

        /// <summary>
        /// Informações do sócio ou administrador.
        /// </summary>
        [JsonPropertyName("person")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public PersonBaseDto Person { get; set; } = new PersonBaseDto();

        /// <summary>
        /// Informações da qualificação.
        /// </summary>
        [JsonPropertyName("role")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public RoleDto Role { get; set; } = new RoleDto();

        /// <summary>
        /// Presente quando aplicável na qualificação  
        /// <br/>Informações do representante legal.
        /// </summary>
        [JsonPropertyName("agent")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MemberAgentDto Agent
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class OfficeCompanyDto
    {
        /// <summary>
        /// Código da empresa, idem aos oito primeiros caracteres do CNPJ.
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Razão social.
        /// </summary>
        [JsonPropertyName("name")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `nature.id &lt; 2000`  
        /// <br/>Ente federativo responsável.
        /// </summary>
        [JsonPropertyName("jurisdiction")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Jurisdiction
        {
            get; set;
        }

        /// <summary>
        /// Capital social
        /// </summary>
        [JsonPropertyName("equity")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public float Equity
        {
            get; set;
        }

        /// <summary>
        /// Informações da natureza jurídica.
        /// </summary>
        [JsonPropertyName("nature")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public NatureDto Nature { get; set; } = new NatureDto();

        /// <summary>
        /// Informações do porte.
        /// </summary>
        [JsonPropertyName("size")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public CompanySizeDto Size { get; set; } = new CompanySizeDto();

        /// <summary>
        /// Informações da opção pelo Simples Nacional.
        /// </summary>
        [JsonPropertyName("simples")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SimplesSimeiDto Simples
        {
            get; set;
        }

        /// <summary>
        /// Informações do enquadramento no MEI.
        /// </summary>
        [JsonPropertyName("simei")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SimplesSimeiDto Simei
        {
            get; set;
        }

        /// <summary>
        /// Quadro de sócios e administradores.
        /// </summary>
        [JsonPropertyName("members")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<MemberDto> Members { get; set; } = new List<MemberDto>();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class RegistrationStatusDto
    {
        /// <summary>
        /// Código da situação cadastral:  
        /// <br/>• `1`: Sem restrição.  
        /// <br/>• `2`: Bloqueado como destinatário na UF.  
        /// <br/>• `3`: Vedada operação como destinatário na UF.
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Descrição da situação cadastral.
        /// </summary>
        [JsonPropertyName("text")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Text
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class RegistrationTypeDto
    {
        /// <summary>
        /// Código do tipo:  
        /// <br/>• `1`: IE Normal.  
        /// <br/>• `2`: IE Substituto Tributário.  
        /// <br/>• `3`: IE Não Contribuinte (Canteiro de Obras, IE Virtual, outros).  
        /// <br/>• `4`: IE de Produtor Rural.
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Descrição do tipo.
        /// </summary>
        [JsonPropertyName("text")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Text
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class RegistrationDto
    {
        /// <summary>
        /// Número da Inscrição Estadual.
        /// </summary>
        [JsonPropertyName("number")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        [System.ComponentModel.DataAnnotations.StringLength(14, MinimumLength = 8)]
        public string Number
        {
            get; set;
        }

        /// <summary>
        /// Unidade Federativa de registro.
        /// </summary>
        [JsonPropertyName("state")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public UF State
        {
            get; set;
        }

        /// <summary>
        /// Indica se habilitada como contribuinte.
        /// </summary>
        [JsonPropertyName("enabled")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Enabled
        {
            get; set;
        }

        /// <summary>
        /// Data da situação cadastral.
        /// </summary>
        [JsonPropertyName("statusDate")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string StatusDate
        {
            get; set;
        }

        /// <summary>
        /// Situação cadastral da inscrição.
        /// </summary>
        [JsonPropertyName("status")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public RegistrationStatusDto Status { get; set; } = new RegistrationStatusDto();

        /// <summary>
        /// Tipo da inscrição.
        /// </summary>
        [JsonPropertyName("type")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public RegistrationTypeDto Type { get; set; } = new RegistrationTypeDto();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class OfficeSuframaDto
    {
        /// <summary>
        /// Número da inscrição SUFRAMA.
        /// </summary>
        [JsonPropertyName("number")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Number
        {
            get; set;
        }

        /// <summary>
        /// Data de inscrição na SUFRAMA.
        /// </summary>
        [JsonPropertyName("since")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Since
        {
            get; set;
        }

        /// <summary>
        /// Indica se o projeto está aprovado.
        /// </summary>
        [JsonPropertyName("approved")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Approved
        {
            get; set;
        }

        /// <summary>
        /// Data de aprovação do projeto.
        /// </summary>
        [JsonPropertyName("approvalDate")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string ApprovalDate
        {
            get; set;
        }

        /// <summary>
        /// Informações da situação cadastral.
        /// </summary>
        [JsonPropertyName("status")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public SuframaStatusDto Status { get; set; } = new SuframaStatusDto();

        /// <summary>
        /// Lista de incentivos fiscais.
        /// </summary>
        [JsonPropertyName("incentives")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<SuframaIncentiveDto> Incentives { get; set; } = new List<SuframaIncentiveDto>();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class OfficeLinkDto
    {
        /// <summary>
        /// Tipo de arquivo a qual o link se refere.
        /// </summary>
        [JsonPropertyName("type")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public OfficeLinkDtoType Type
        {
            get; set;
        }

        /// <summary>
        /// URL pública de acesso ao arquivo.
        /// </summary>
        [JsonPropertyName("url")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Url
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class OfficeDto
    {
        /// <summary>
        /// Número do CNPJ sem pontuação.
        /// </summary>
        [JsonPropertyName("taxId")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string TaxId
        {
            get; set;
        }

        /// <summary>
        /// Data da última atualização.
        /// </summary>
        [JsonPropertyName("updated")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Updated
        {
            get; set;
        }

        /// <summary>
        /// Informações da empresa.
        /// </summary>
        [JsonPropertyName("company")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public OfficeCompanyDto Company { get; set; } = new OfficeCompanyDto();

        /// <summary>
        /// Nome fantasia.
        /// </summary>
        [JsonPropertyName("alias")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Alias
        {
            get; set;
        }

        /// <summary>
        /// Data de abertura.
        /// </summary>
        [JsonPropertyName("founded")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Founded
        {
            get; set;
        }

        /// <summary>
        /// Indica se o estabelecimento é a Matriz.
        /// </summary>
        [JsonPropertyName("head")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Head
        {
            get; set;
        }

        /// <summary>
        /// Data da situação cadastral.
        /// </summary>
        [JsonPropertyName("statusDate")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string StatusDate
        {
            get; set;
        }

        /// <summary>
        /// Informações da situação cadastral.
        /// </summary>
        [JsonPropertyName("status")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public OfficeStatusDto Status { get; set; } = new OfficeStatusDto();

        /// <summary>
        /// Presente quando `status.id != 2`  
        /// <br/>Informações do motivo da situação cadastral.
        /// </summary>
        [JsonPropertyName("reason")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OfficeReasonDto Reason
        {
            get; set;
        }

        /// <summary>
        /// Data da situação especial.
        /// </summary>
        [JsonPropertyName("specialDate")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SpecialDate
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `specialDate != undefined`  
        /// <br/>Informações da situação especial.
        /// </summary>
        [JsonPropertyName("special")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OfficeSpecialDto Special
        {
            get; set;
        }

        /// <summary>
        /// Informações do endereço.
        /// </summary>
        [JsonPropertyName("address")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public AddressDto Address { get; set; } = new AddressDto();

        /// <summary>
        /// Lista de telefones.
        /// </summary>
        [JsonPropertyName("phones")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<PhoneDto> Phones { get; set; } = new List<PhoneDto>();

        /// <summary>
        /// Lista de e-mails.
        /// </summary>
        [JsonPropertyName("emails")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<EmailDto> Emails { get; set; } = new List<EmailDto>();

        /// <summary>
        /// Informações da atividade econômica principal.
        /// </summary>
        [JsonPropertyName("mainActivity")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public ActivityDto MainActivity { get; set; } = new ActivityDto();

        /// <summary>
        /// Lista de atividades econômicas secundárias.
        /// </summary>
        [JsonPropertyName("sideActivities")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<ActivityDto> SideActivities { get; set; } = new List<ActivityDto>();

        /// <summary>
        /// Lista de Inscrições Estaduais.
        /// </summary>
        [JsonPropertyName("registrations")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<RegistrationDto> Registrations
        {
            get; set;
        }

        /// <summary>
        /// Lista de inscrições SUFRAMA
        /// </summary>
        [JsonPropertyName("suframa")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OfficeSuframaDto> Suframa
        {
            get; set;
        }

        /// <summary>
        /// Lista de links para arquivos.
        /// </summary>
        [JsonPropertyName("links")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OfficeLinkDto> Links
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class OfficePageRecordDto
    {
        /// <summary>
        /// Número do CNPJ sem pontuação.
        /// </summary>
        [JsonPropertyName("taxId")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string TaxId
        {
            get; set;
        }

        /// <summary>
        /// Data da última atualização.
        /// </summary>
        [JsonPropertyName("updated")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Updated
        {
            get; set;
        }

        /// <summary>
        /// Informações da empresa.
        /// </summary>
        [JsonPropertyName("company")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public OfficeCompanyDto Company { get; set; } = new OfficeCompanyDto();

        /// <summary>
        /// Nome fantasia.
        /// </summary>
        [JsonPropertyName("alias")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Alias
        {
            get; set;
        }

        /// <summary>
        /// Data de abertura.
        /// </summary>
        [JsonPropertyName("founded")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Founded
        {
            get; set;
        }

        /// <summary>
        /// Indica se o estabelecimento é a Matriz.
        /// </summary>
        [JsonPropertyName("head")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Head
        {
            get; set;
        }

        /// <summary>
        /// Data da situação cadastral.
        /// </summary>
        [JsonPropertyName("statusDate")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string StatusDate
        {
            get; set;
        }

        /// <summary>
        /// Informações da situação cadastral.
        /// </summary>
        [JsonPropertyName("status")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public OfficeStatusDto Status { get; set; } = new OfficeStatusDto();

        /// <summary>
        /// Presente quando `status.id != 2`  
        /// <br/>Informações do motivo da situação cadastral.
        /// </summary>
        [JsonPropertyName("reason")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OfficeReasonDto Reason
        {
            get; set;
        }

        /// <summary>
        /// Data da situação especial.
        /// </summary>
        [JsonPropertyName("specialDate")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SpecialDate
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `specialDate != undefined`  
        /// <br/>Informações da situação especial.
        /// </summary>
        [JsonPropertyName("special")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OfficeSpecialDto Special
        {
            get; set;
        }

        /// <summary>
        /// Informações do endereço.
        /// </summary>
        [JsonPropertyName("address")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public AddressDto Address { get; set; } = new AddressDto();

        /// <summary>
        /// Lista de telefones.
        /// </summary>
        [JsonPropertyName("phones")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<PhoneDto> Phones { get; set; } = new List<PhoneDto>();

        /// <summary>
        /// Lista de e-mails.
        /// </summary>
        [JsonPropertyName("emails")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<EmailDto> Emails { get; set; } = new List<EmailDto>();

        /// <summary>
        /// Informações da atividade econômica principal.
        /// </summary>
        [JsonPropertyName("mainActivity")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public ActivityDto MainActivity { get; set; } = new ActivityDto();

        /// <summary>
        /// Lista de atividades econômicas secundárias.
        /// </summary>
        [JsonPropertyName("sideActivities")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<ActivityDto> SideActivities { get; set; } = new List<ActivityDto>();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class OfficePageDto
    {
        /// <summary>
        /// Token da próxima página.
        /// </summary>
        [JsonPropertyName("next")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        [System.ComponentModel.DataAnnotations.StringLength(32, MinimumLength = 32)]
        public string Next
        {
            get; set;
        }

        /// <summary>
        /// Quantidade de registros lidos.
        /// </summary>
        [JsonPropertyName("limit")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [System.ComponentModel.DataAnnotations.Range(1D, 100D)]
        public double Limit
        {
            get; set;
        }

        /// <summary>
        /// Quantidade de registros disponíveis.
        /// </summary>
        [JsonPropertyName("count")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
        public double Count
        {
            get; set;
        }

        /// <summary>
        /// Lista de estabelecimentos que obedecem aos critérios de pesquisa.
        /// </summary>
        [JsonPropertyName("records")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<OfficePageRecordDto> Records { get; set; } = new List<OfficePageRecordDto>();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class ListCreateDto
    {
        /// <summary>
        /// Título da lista.
        /// </summary>
        [JsonPropertyName("title")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        [System.ComponentModel.DataAnnotations.StringLength(50)]
        public string Title
        {
            get; set;
        }

        /// <summary>
        /// Descrição da lista.
        /// </summary>
        [JsonPropertyName("description")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [System.ComponentModel.DataAnnotations.StringLength(2000)]
        public string Description
        {
            get; set;
        }

        /// <summary>
        /// Itens pertencentes a lista.
        /// </summary>
        [JsonPropertyName("items")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        [System.ComponentModel.DataAnnotations.MaxLength(100000)]
        public IList<string> Items { get; set; } = new List<string>();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class ListDto
    {
        /// <summary>
        /// Identificador único.
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public System.Guid Id
        {
            get; set;
        }

        /// <summary>
        /// Data de criação.
        /// </summary>
        [JsonPropertyName("created")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Created
        {
            get; set;
        }

        /// <summary>
        /// Data da última atualização.
        /// </summary>
        [JsonPropertyName("updated")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Updated
        {
            get; set;
        }

        /// <summary>
        /// Título da lista.
        /// </summary>
        [JsonPropertyName("title")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        [System.ComponentModel.DataAnnotations.StringLength(50)]
        public string Title
        {
            get; set;
        }

        /// <summary>
        /// Descrição da lista.
        /// </summary>
        [JsonPropertyName("description")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [System.ComponentModel.DataAnnotations.StringLength(2000)]
        public string Description
        {
            get; set;
        }

        /// <summary>
        /// Quantidade de itens.
        /// </summary>
        [JsonPropertyName("size")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Size
        {
            get; set;
        }

        /// <summary>
        /// Itens pertencentes a lista.
        /// </summary>
        [JsonPropertyName("items")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [System.ComponentModel.DataAnnotations.MaxLength(100000)]
        public IList<string> Items
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class ListSummaryDto
    {
        /// <summary>
        /// Identificador único.
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public System.Guid Id
        {
            get; set;
        }

        /// <summary>
        /// Data de criação.
        /// </summary>
        [JsonPropertyName("created")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Created
        {
            get; set;
        }

        /// <summary>
        /// Data da última atualização.
        /// </summary>
        [JsonPropertyName("updated")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Updated
        {
            get; set;
        }

        /// <summary>
        /// Título da lista.
        /// </summary>
        [JsonPropertyName("title")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        [System.ComponentModel.DataAnnotations.StringLength(50)]
        public string Title
        {
            get; set;
        }

        /// <summary>
        /// Descrição da lista.
        /// </summary>
        [JsonPropertyName("description")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [System.ComponentModel.DataAnnotations.StringLength(2000)]
        public string Description
        {
            get; set;
        }

        /// <summary>
        /// Quantidade de itens.
        /// </summary>
        [JsonPropertyName("size")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Size
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class ListPageDto
    {
        /// <summary>
        /// Token da próxima página.
        /// </summary>
        [JsonPropertyName("next")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        [System.ComponentModel.DataAnnotations.StringLength(32, MinimumLength = 32)]
        public string Next
        {
            get; set;
        }

        /// <summary>
        /// Quantidade de registros lidos.
        /// </summary>
        [JsonPropertyName("limit")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [System.ComponentModel.DataAnnotations.Range(1D, 100D)]
        public double Limit
        {
            get; set;
        }

        /// <summary>
        /// Quantidade de registros disponíveis.
        /// </summary>
        [JsonPropertyName("count")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
        public double Count
        {
            get; set;
        }

        /// <summary>
        /// Listas que obedecem aos critérios de pesquisa.
        /// </summary>
        [JsonPropertyName("records")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<ListSummaryDto> Records { get; set; } = new List<ListSummaryDto>();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class ListUpdateDto
    {
        /// <summary>
        /// Título da lista.
        /// </summary>
        [JsonPropertyName("title")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [System.ComponentModel.DataAnnotations.StringLength(50)]
        public string Title
        {
            get; set;
        }

        /// <summary>
        /// Descrição da lista.
        /// </summary>
        [JsonPropertyName("description")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [System.ComponentModel.DataAnnotations.StringLength(2000)]
        public string Description
        {
            get; set;
        }

        /// <summary>
        /// Itens pertencentes a lista.
        /// </summary>
        [JsonPropertyName("items")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [System.ComponentModel.DataAnnotations.MaxLength(100000)]
        public IList<string> Items
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class LegacyRegistrationDto
    {
        /// <summary>
        /// Situação cadastral: `NULA`, `ATIVA`, `SUSPENSA`, `INAPTA` ou `BAIXADA`.
        /// </summary>
        [JsonPropertyName("status")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public LegacyRegistrationDtoStatus Status
        {
            get; set;
        }

        /// <summary>
        /// Data da situação especial.
        /// </summary>
        [JsonPropertyName("status_date")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Status_date
        {
            get; set;
        }

        /// <summary>
        /// Motivo da situação cadastral.
        /// </summary>
        [JsonPropertyName("status_reason")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Status_reason
        {
            get; set;
        }

        /// <summary>
        /// Descrição da situação especial.
        /// </summary>
        [JsonPropertyName("special_status")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Special_status
        {
            get; set;
        }

        /// <summary>
        /// Data da situação especial.
        /// </summary>
        [JsonPropertyName("special_status_date")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Special_status_date
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class LegacyAddressDto
    {
        /// <summary>
        /// Logradouro.
        /// </summary>
        [JsonPropertyName("street")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Street
        {
            get; set;
        }

        /// <summary>
        /// Número.
        /// </summary>
        [JsonPropertyName("number")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Number
        {
            get; set;
        }

        /// <summary>
        /// Complemento.
        /// </summary>
        [JsonPropertyName("details")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Details
        {
            get; set;
        }

        /// <summary>
        /// Bairro ou distrito.
        /// </summary>
        [JsonPropertyName("neighborhood")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Neighborhood
        {
            get; set;
        }

        /// <summary>
        /// Município.
        /// </summary>
        [JsonPropertyName("city")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string City
        {
            get; set;
        }

        [JsonPropertyName("city_ibge")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string City_ibge
        {
            get; set;
        }

        /// <summary>
        /// Sigla da Unidade Federativa.
        /// </summary>
        [JsonPropertyName("state")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public UF State
        {
            get; set;
        }

        /// <summary>
        /// Código da Unidade Federativa conforme IBGE.
        /// </summary>
        [JsonPropertyName("state_ibge")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string State_ibge
        {
            get; set;
        }

        /// <summary>
        /// Código de Endereçamento Postal.
        /// </summary>
        [JsonPropertyName("zip")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        [System.ComponentModel.DataAnnotations.StringLength(8, MinimumLength = 8)]
        public string Zip
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class LegacyLegalNatureDto
    {
        [JsonPropertyName("code")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Code
        {
            get; set;
        }

        /// <summary>
        /// Descrição da natureza jurídica.
        /// </summary>
        [JsonPropertyName("description")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Description
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class LegacySimplesNacionalDto
    {
        /// <summary>
        /// Data da última atualização do Simples Nacional.
        /// </summary>
        [JsonPropertyName("last_update")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Last_update
        {
            get; set;
        }

        /// <summary>
        /// Indica se optante pelo Simples Nacional.
        /// </summary>
        [JsonPropertyName("simples_optant")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Simples_optant
        {
            get; set;
        }

        /// <summary>
        /// Data de inclusão no período vigente.
        /// </summary>
        [JsonPropertyName("simples_included")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Simples_included
        {
            get; set;
        }

        /// <summary>
        /// [Removido] Data de encerramento do último período.
        /// </summary>
        [JsonPropertyName("simples_excluded")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Simples_excluded
        {
            get; set;
        }

        /// <summary>
        /// Indica se enquadrado no MEI.
        /// </summary>
        [JsonPropertyName("simei_optant")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Simei_optant
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class LegacySintegraRegistrationDto
    {
        /// <summary>
        /// Número da Inscrição Estadual.
        /// </summary>
        [JsonPropertyName("number")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Number
        {
            get; set;
        }

        /// <summary>
        /// Unidade Federativa de registro.
        /// </summary>
        [JsonPropertyName("state")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public UF State
        {
            get; set;
        }

        /// <summary>
        /// Indica se habilitada como contribuinte.
        /// </summary>
        [JsonPropertyName("enabled")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Enabled
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class LegacySintegraDto
    {
        /// <summary>
        /// Data da última atualização do Cadastro de Contribuintes.
        /// </summary>
        [JsonPropertyName("last_update")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Last_update
        {
            get; set;
        }

        /// <summary>
        /// Número da Inscrição Estadual no estado de origem.
        /// </summary>
        [JsonPropertyName("home_state_registration")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Home_state_registration
        {
            get; set;
        }

        /// <summary>
        /// Lista de Inscrições Estaduais.
        /// </summary>
        [JsonPropertyName("registrations")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<LegacySintegraRegistrationDto> Registrations { get; set; } = new List<LegacySintegraRegistrationDto>();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class LegacyActivityDto
    {
        [JsonPropertyName("code")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Code
        {
            get; set;
        }

        /// <summary>
        /// Descrição da atividade econômica.
        /// </summary>
        [JsonPropertyName("description")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Description
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class LegacyRoleDto
    {
        [JsonPropertyName("code")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Code
        {
            get; set;
        }

        /// <summary>
        /// Descrição da qualificação.
        /// </summary>
        [JsonPropertyName("description")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Description
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class LegacyMemberAgentDto
    {
        /// <summary>
        /// Nome ou razão social.
        /// </summary>
        [JsonPropertyName("name")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// CPF ou CNPJ.
        /// </summary>
        [JsonPropertyName("tax_id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Tax_id
        {
            get; set;
        }

        /// <summary>
        /// Nome do país de origem.
        /// </summary>
        [JsonPropertyName("home_country")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Home_country
        {
            get; set;
        }

        /// <summary>
        /// Informações da qualificação.
        /// </summary>
        [JsonPropertyName("role")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public LegacyRoleDto Role { get; set; } = new LegacyRoleDto();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class LegacyMemberDto
    {
        /// <summary>
        /// Nome ou razão social.
        /// </summary>
        [JsonPropertyName("name")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// CPF ou CNPJ.
        /// </summary>
        [JsonPropertyName("tax_id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Tax_id
        {
            get; set;
        }

        /// <summary>
        /// Nome do país de origem.
        /// </summary>
        [JsonPropertyName("home_country")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Home_country
        {
            get; set;
        }

        /// <summary>
        /// Informações da qualificação.
        /// </summary>
        [JsonPropertyName("role")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public LegacyRoleDto Role { get; set; } = new LegacyRoleDto();

        /// <summary>
        /// Presente quando aplicável na qualificação  
        /// <br/>Informações do representante legal.
        /// </summary>
        [JsonPropertyName("legal_rep")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public LegacyMemberAgentDto Legal_rep { get; set; } = new LegacyMemberAgentDto();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class LegacyFilesDto
    {
        /// <summary>
        /// Comprovante de inscrição em PDF.
        /// </summary>
        [JsonPropertyName("registration")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Registration
        {
            get; set;
        }

        /// <summary>
        /// Quadro de sócios e administradores em PDF.
        /// </summary>
        [JsonPropertyName("membership")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Membership
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class LegacyMapsDto
    {
        /// <summary>
        /// Mapa aéreo de vias.
        /// </summary>
        [JsonPropertyName("roads")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Roads
        {
            get; set;
        }

        /// <summary>
        /// Mapa aéreo de satélite.
        /// </summary>
        [JsonPropertyName("satellite")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Satellite
        {
            get; set;
        }

        /// <summary>
        /// Visão da rua.
        /// </summary>
        [JsonPropertyName("street")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Street
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class LegacyCompanyDto
    {
        /// <summary>
        /// Data da última atualização.
        /// </summary>
        [JsonPropertyName("last_update")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Last_update
        {
            get; set;
        }

        /// <summary>
        /// Razão social.
        /// </summary>
        [JsonPropertyName("name")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Nome fantasia.
        /// </summary>
        [JsonPropertyName("alias")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Alias
        {
            get; set;
        }

        /// <summary>
        /// Número do CNPJ.
        /// </summary>
        [JsonPropertyName("tax_id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Tax_id
        {
            get; set;
        }

        /// <summary>
        /// Tipo do estabelecimento: `MATRIZ` ou `FILIAL`.
        /// </summary>
        [JsonPropertyName("type")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public LegacyCompanyDtoType Type
        {
            get; set;
        }

        /// <summary>
        /// Data de abertura.
        /// </summary>
        [JsonPropertyName("founded")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Founded
        {
            get; set;
        }

        /// <summary>
        /// Porte da empresa: `ME`, `EPP` ou `DEMAIS`.
        /// </summary>
        [JsonPropertyName("size")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public LegacyCompanyDtoSize Size
        {
            get; set;
        }

        /// <summary>
        /// Capital social.
        /// </summary>
        [JsonPropertyName("capital")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public float Capital
        {
            get; set;
        }

        /// <summary>
        /// Endereço de e-mail.
        /// </summary>
        [JsonPropertyName("email")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Email
        {
            get; set;
        }

        /// <summary>
        /// Número do telefone.
        /// </summary>
        [JsonPropertyName("phone")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Phone
        {
            get; set;
        }

        /// <summary>
        /// Número do telefone alternativo.
        /// </summary>
        [JsonPropertyName("phone_alt")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Phone_alt
        {
            get; set;
        }

        /// <summary>
        /// Ente federativo responsável.
        /// </summary>
        [JsonPropertyName("federal_entity")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Federal_entity
        {
            get; set;
        }

        /// <summary>
        /// Informações da situação cadastral.
        /// </summary>
        [JsonPropertyName("registration")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public LegacyRegistrationDto Registration { get; set; } = new LegacyRegistrationDto();

        /// <summary>
        /// Informações da situação cadastral.
        /// </summary>
        [JsonPropertyName("address")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public LegacyAddressDto Address { get; set; } = new LegacyAddressDto();

        /// <summary>
        /// Informações da natureza jurídica.
        /// </summary>
        [JsonPropertyName("legal_nature")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public LegacyLegalNatureDto Legal_nature { get; set; } = new LegacyLegalNatureDto();

        /// <summary>
        /// Informações do Simples Nacional.
        /// </summary>
        [JsonPropertyName("simples_nacional")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public LegacySimplesNacionalDto Simples_nacional { get; set; } = new LegacySimplesNacionalDto();

        /// <summary>
        /// Informações do Cadastro de Contribuintes.
        /// </summary>
        [JsonPropertyName("sintegra")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public LegacySintegraDto Sintegra { get; set; } = new LegacySintegraDto();

        /// <summary>
        /// Informações da atividade econômica principal.
        /// </summary>
        [JsonPropertyName("primary_activity")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public LegacyActivityDto Primary_activity { get; set; } = new LegacyActivityDto();

        /// <summary>
        /// Lista de atividades econômicas secundárias.
        /// </summary>
        [JsonPropertyName("secondary_activities")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<LegacyActivityDto> Secondary_activities { get; set; } = new List<LegacyActivityDto>();

        /// <summary>
        /// Quadro de sócios e administradores.
        /// </summary>
        [JsonPropertyName("membership")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<LegacyMemberDto> Membership { get; set; } = new List<LegacyMemberDto>();

        [JsonPropertyName("partnership")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public object Partnership { get; set; } = new object();

        /// <summary>
        /// Links para download de arquivos.
        /// </summary>
        [JsonPropertyName("files")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public LegacyFilesDto Files { get; set; } = new LegacyFilesDto();

        /// <summary>
        /// Links para download de mapas.
        /// </summary>
        [JsonPropertyName("maps")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public LegacyMapsDto Maps { get; set; } = new LegacyMapsDto();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class CreditoDto
    {
        /// <summary>
        /// Créditos acumulados de meses anteriores.
        /// </summary>
        [JsonPropertyName("perpetual")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Perpetual
        {
            get; set;
        }

        /// <summary>
        /// Créditos do mês atual.
        /// </summary>
        [JsonPropertyName("transient")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Transient
        {
            get; set;
        }

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class CompanyOfficeDto
    {
        /// <summary>
        /// Número do CNPJ sem pontuação.
        /// </summary>
        [JsonPropertyName("taxId")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string TaxId
        {
            get; set;
        }

        /// <summary>
        /// Nome fantasia.
        /// </summary>
        [JsonPropertyName("alias")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Alias
        {
            get; set;
        }

        /// <summary>
        /// Data de abertura.
        /// </summary>
        [JsonPropertyName("founded")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Founded
        {
            get; set;
        }

        /// <summary>
        /// Indica se o estabelecimento é a Matriz.
        /// </summary>
        [JsonPropertyName("head")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Head
        {
            get; set;
        }

        /// <summary>
        /// Data da situação cadastral.
        /// </summary>
        [JsonPropertyName("statusDate")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string StatusDate
        {
            get; set;
        }

        /// <summary>
        /// Informações da situação cadastral.
        /// </summary>
        [JsonPropertyName("status")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public OfficeStatusDto Status { get; set; } = new OfficeStatusDto();

        /// <summary>
        /// Presente quando `status.id != 2`  
        /// <br/>Informações do motivo da situação cadastral.
        /// </summary>
        [JsonPropertyName("reason")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OfficeReasonDto Reason
        {
            get; set;
        }

        /// <summary>
        /// Data da situação especial.
        /// </summary>
        [JsonPropertyName("specialDate")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SpecialDate
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `specialDate != undefined`  
        /// <br/>Informações da situação especial.
        /// </summary>
        [JsonPropertyName("special")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OfficeSpecialDto Special
        {
            get; set;
        }

        /// <summary>
        /// Informações da atividade econômica principal.
        /// </summary>
        [JsonPropertyName("mainActivity")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public ActivityDto MainActivity { get; set; } = new ActivityDto();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class CompanyDto
    {
        /// <summary>
        /// Código da empresa, idem aos oito primeiros caracteres do CNPJ.
        /// </summary>
        [JsonPropertyName("id")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double Id
        {
            get; set;
        }

        /// <summary>
        /// Razão social.
        /// </summary>
        [JsonPropertyName("name")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Presente quando `nature.id &lt; 2000`  
        /// <br/>Ente federativo responsável.
        /// </summary>
        [JsonPropertyName("jurisdiction")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Jurisdiction
        {
            get; set;
        }

        /// <summary>
        /// Capital social
        /// </summary>
        [JsonPropertyName("equity")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public float Equity
        {
            get; set;
        }

        /// <summary>
        /// Informações da natureza jurídica.
        /// </summary>
        [JsonPropertyName("nature")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public NatureDto Nature { get; set; } = new NatureDto();

        /// <summary>
        /// Informações do porte.
        /// </summary>
        [JsonPropertyName("size")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public CompanySizeDto Size { get; set; } = new CompanySizeDto();

        /// <summary>
        /// Informações da opção pelo Simples Nacional.
        /// </summary>
        [JsonPropertyName("simples")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SimplesSimeiDto Simples
        {
            get; set;
        }

        /// <summary>
        /// Informações do enquadramento no MEI.
        /// </summary>
        [JsonPropertyName("simei")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SimplesSimeiDto Simei
        {
            get; set;
        }

        /// <summary>
        /// Quadro de sócios e administradores.
        /// </summary>
        [JsonPropertyName("members")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<MemberDto> Members { get; set; } = new List<MemberDto>();

        /// <summary>
        /// Lista de estabelecimentos.
        /// </summary>
        [JsonPropertyName("offices")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<CompanyOfficeDto> Offices { get; set; } = new List<CompanyOfficeDto>();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    
    public partial class CccDto
    {
        /// <summary>
        /// Número do CNPJ ou CPF sem pontuação.
        /// </summary>
        [JsonPropertyName("taxId")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string TaxId
        {
            get; set;
        }

        /// <summary>
        /// Data da última atualização.
        /// </summary>
        [JsonPropertyName("updated")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public string Updated
        {
            get; set;
        }

        /// <summary>
        /// Unidade Federativa de origem.
        /// </summary>
        [JsonPropertyName("originState")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        
        public UF OriginState
        {
            get; set;
        }

        /// <summary>
        /// Inscrições Estaduais.
        /// </summary>
        [JsonPropertyName("registrations")] [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        
        public IList<RegistrationDto> Registrations { get; set; } = new List<RegistrationDto>();

        private IDictionary<string, object> _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>());
            }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    [JsonConverter(typeof(JsonStringEnumConverter<Strategy>))]
    public enum Strategy
    {

        [System.Runtime.Serialization.EnumMember(Value = @"ONLINE")]
        ONLINE = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"CACHE_IF_FRESH")]
        CACHE_IF_FRESH = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"CACHE_IF_ERROR")]
        CACHE_IF_ERROR = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"CACHE")]
        CACHE = 3,

    }


    [JsonConverter(typeof(JsonStringEnumConverter<Anonymous>))]
    public enum Anonymous
    {

        [System.Runtime.Serialization.EnumMember(Value = @"REGISTRATION")]
        REGISTRATION = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"MEMBERS")]
        MEMBERS = 1,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<Anonymous2>))]
    public enum Anonymous2
    {

        [System.Runtime.Serialization.EnumMember(Value = @"LEGAL")]
        LEGAL = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"NATURAL")]
        NATURAL = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"FOREIGN")]
        FOREIGN = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"UNKNOWN")]
        UNKNOWN = 3,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<Age>))]
    public enum Age
    {

        [System.Runtime.Serialization.EnumMember(Value = @"0-12")]
        [JsonStringEnumMemberName(@"0-12")]
        age0to12 = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"13-20")]
        [JsonStringEnumMemberName(@"13-20")]
        age13to20 = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"21-30")]
        [JsonStringEnumMemberName(@"21-30")]
        age21to30 = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"31-40")]
        [JsonStringEnumMemberName(@"31-40")]
        age31to40 = 3,

        [System.Runtime.Serialization.EnumMember(Value = @"41-50")]
        [JsonStringEnumMemberName(@"41-50")]
        age41to50 = 4,

        [System.Runtime.Serialization.EnumMember(Value = @"51-60")]
        [JsonStringEnumMemberName(@"")]
        age51to60 = 5,

        [System.Runtime.Serialization.EnumMember(Value = @"61-70")]
        [JsonStringEnumMemberName(@"61-70")]
        age61to70 = 6,

        [System.Runtime.Serialization.EnumMember(Value = @"71-80")]
        [JsonStringEnumMemberName(@"71-80")]
        age71to80 = 7,

        [System.Runtime.Serialization.EnumMember(Value = @"81+")]
        [JsonStringEnumMemberName(@"81+")]
        age81plus = 8,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<UF>))]
    public enum UF
    {

        [System.Runtime.Serialization.EnumMember(Value = @"BR")]
        [JsonStringEnumMemberName(@"BR")]
        BR = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"AC")]
        [JsonStringEnumMemberName(@"AC")]
        AC = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"AL")]
        [JsonStringEnumMemberName(@"AL")]
        AL = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"AM")]
        [JsonStringEnumMemberName(@"AM")]
        AM = 3,

        [System.Runtime.Serialization.EnumMember(Value = @"AP")]
        [JsonStringEnumMemberName(@"AP")]
        AP = 4,

        [System.Runtime.Serialization.EnumMember(Value = @"BA")]
        [JsonStringEnumMemberName(@"BA")]
        BA = 5,

        [System.Runtime.Serialization.EnumMember(Value = @"CE")]
        [JsonStringEnumMemberName(@"CE")]
        CE = 6,

        [System.Runtime.Serialization.EnumMember(Value = @"DF")]
        [JsonStringEnumMemberName(@"DF")]
        DF = 7,

        [System.Runtime.Serialization.EnumMember(Value = @"ES")]
        [JsonStringEnumMemberName(@"ES")]
        ES = 8,

        [System.Runtime.Serialization.EnumMember(Value = @"GO")]
        [JsonStringEnumMemberName(@"GO")]
        GO = 9,

        [System.Runtime.Serialization.EnumMember(Value = @"MA")]
        [JsonStringEnumMemberName(@"MA")]
        MA = 10,

        [System.Runtime.Serialization.EnumMember(Value = @"MG")]
        [JsonStringEnumMemberName(@"MG")]
        MG = 11,

        [System.Runtime.Serialization.EnumMember(Value = @"MS")]
        [JsonStringEnumMemberName(@"MS")]
        MS = 12,

        [System.Runtime.Serialization.EnumMember(Value = @"MT")]
        [JsonStringEnumMemberName(@"MT")]
        MT = 13,

        [System.Runtime.Serialization.EnumMember(Value = @"PA")]
        [JsonStringEnumMemberName(@"PA")]
        PA = 14,

        [System.Runtime.Serialization.EnumMember(Value = @"PB")]
        [JsonStringEnumMemberName(@"PB")]
        PB = 15,

        [System.Runtime.Serialization.EnumMember(Value = @"PE")]
        [JsonStringEnumMemberName(@"PE")]
        PE = 16,

        [System.Runtime.Serialization.EnumMember(Value = @"PI")]
        [JsonStringEnumMemberName(@"PI")]
        PI = 17,

        [System.Runtime.Serialization.EnumMember(Value = @"PR")]
        [JsonStringEnumMemberName(@"PR")]
        PR = 18,

        [System.Runtime.Serialization.EnumMember(Value = @"RJ")]
        [JsonStringEnumMemberName(@"RJ")]
        RJ = 19,

        [System.Runtime.Serialization.EnumMember(Value = @"RN")]
        [JsonStringEnumMemberName(@"RN")]
        RN = 20,

        [System.Runtime.Serialization.EnumMember(Value = @"RO")]
        [JsonStringEnumMemberName(@"RO")]
        RO = 21,

        [System.Runtime.Serialization.EnumMember(Value = @"RR")]
        [JsonStringEnumMemberName(@"RR")]
        RR = 22,

        [System.Runtime.Serialization.EnumMember(Value = @"RS")]
        [JsonStringEnumMemberName(@"RS")]
        RS = 23,

        [System.Runtime.Serialization.EnumMember(Value = @"SC")]
        [JsonStringEnumMemberName(@"SC")]
        SC = 24,

        [System.Runtime.Serialization.EnumMember(Value = @"SP")]
        [JsonStringEnumMemberName(@"SP")]
        SP = 25,

        [System.Runtime.Serialization.EnumMember(Value = @"SE")]
        [JsonStringEnumMemberName(@"SE")]
        SE = 26,

        [System.Runtime.Serialization.EnumMember(Value = @"TO")]
        [JsonStringEnumMemberName(@"TO")]
        TO = 27,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<Anonymous5>))]
    public enum Anonymous5
    {

        [System.Runtime.Serialization.EnumMember(Value = @"RFB_CERTIFICATE")]
        RFB_CERTIFICATE = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"SIMPLES_CERTIFICATE")]
        SIMPLES_CERTIFICATE = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"CCC_CERTIFICATE")]
        CCC_CERTIFICATE = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"SUFRAMA_CERTIFICATE")]
        SUFRAMA_CERTIFICATE = 3,

        [System.Runtime.Serialization.EnumMember(Value = @"OFFICE_MAP")]
        OFFICE_MAP = 4,

        [System.Runtime.Serialization.EnumMember(Value = @"OFFICE_STREET")]
        OFFICE_STREET = 5,

    }




    [JsonConverter(typeof(JsonStringEnumConverter<Anonymous7>))]
    public enum Anonymous7
    {

        [System.Runtime.Serialization.EnumMember(Value = @"LANDLINE")]
        LANDLINE = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"MOBILE")]
        MOBILE = 1,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<Anonymous8>))]
    public enum Anonymous8
    {

        [System.Runtime.Serialization.EnumMember(Value = @"ACCOUNTING")]
        ACCOUNTING = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"CORPORATE")]
        CORPORATE = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"PERSONAL")]
        PERSONAL = 2,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<MapType>))]
    public enum MapType
    {

        [System.Runtime.Serialization.EnumMember(Value = @"roadmap")]
        Roadmap = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"terrain")]
        Terrain = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"satellite")]
        Satellite = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"hybrid")]
        Hybrid = 3,

    }




    [JsonConverter(typeof(JsonStringEnumConverter<PhoneDtoType>))]
    public enum PhoneDtoType
    {

        [System.Runtime.Serialization.EnumMember(Value = @"LANDLINE")]
        LANDLINE = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"MOBILE")]
        MOBILE = 1,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<EmailDtoOwnership>))]
    public enum EmailDtoOwnership
    {

        [System.Runtime.Serialization.EnumMember(Value = @"ACCOUNTING")]
        ACCOUNTING = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"CORPORATE")]
        CORPORATE = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"PERSONAL")]
        PERSONAL = 2,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<SuframaIncentiveDtoTribute>))]
    public enum SuframaIncentiveDtoTribute
    {

        [System.Runtime.Serialization.EnumMember(Value = @"ICMS")]
        ICMS = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"IPI")]
        IPI = 1,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<PersonBaseDtoType>))]
    public enum PersonBaseDtoType
    {

        [System.Runtime.Serialization.EnumMember(Value = @"LEGAL")]
        LEGAL = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"NATURAL")]
        NATURAL = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"FOREIGN")]
        FOREIGN = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"UNKNOWN")]
        UNKNOWN = 3,

    }


    [JsonConverter(typeof(JsonStringEnumConverter<PersonDtoType>))]
    public enum PersonDtoType
    {

        [System.Runtime.Serialization.EnumMember(Value = @"LEGAL")]
        LEGAL = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"NATURAL")]
        NATURAL = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"FOREIGN")]
        FOREIGN = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"UNKNOWN")]
        UNKNOWN = 3,

    }



    [JsonConverter(typeof(JsonStringEnumConverter<OfficeLinkDtoType>))]
    public enum OfficeLinkDtoType
    {

        [System.Runtime.Serialization.EnumMember(Value = @"RFB_CERTIFICATE")]
        RFB_CERTIFICATE = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"SIMPLES_CERTIFICATE")]
        SIMPLES_CERTIFICATE = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"CCC_CERTIFICATE")]
        CCC_CERTIFICATE = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"SUFRAMA_CERTIFICATE")]
        SUFRAMA_CERTIFICATE = 3,

        [System.Runtime.Serialization.EnumMember(Value = @"OFFICE_MAP")]
        OFFICE_MAP = 4,

        [System.Runtime.Serialization.EnumMember(Value = @"OFFICE_STREET")]
        OFFICE_STREET = 5,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<LegacyRegistrationDtoStatus>))]
    public enum LegacyRegistrationDtoStatus
    {

        [System.Runtime.Serialization.EnumMember(Value = @"NULA")]
        NULA = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"ATIVA")]
        ATIVA = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"SUSPENSA")]
        SUSPENSA = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"INAPTA")]
        INAPTA = 3,

        [System.Runtime.Serialization.EnumMember(Value = @"BAIXADA")]
        BAIXADA = 4,

    }

    

    [JsonConverter(typeof(JsonStringEnumConverter<LegacyCompanyDtoType>))]
    public enum LegacyCompanyDtoType
    {

        [System.Runtime.Serialization.EnumMember(Value = @"MATRIZ")]
        MATRIZ = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"FILIAL")]
        FILIAL = 1,

    }

    [JsonConverter(typeof(JsonStringEnumConverter<LegacyCompanyDtoSize>))]
    public enum LegacyCompanyDtoSize
    {

        [System.Runtime.Serialization.EnumMember(Value = @"ME")]
        ME = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"EPP")]
        EPP = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"DEMAIS")]
        DEMAIS = 2,

    }

    

    public partial class FileResponse : System.IDisposable
    {
        private System.IDisposable _client;
        private System.IDisposable _response;

        public int StatusCode
        {
            get; private set;
        }

        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> Headers
        {
            get; private set;
        }

        public System.IO.Stream Stream
        {
            get; private set;
        }

        public bool IsPartial
        {
            get
            {
                return StatusCode == 206;
            }
        }

        public FileResponse(int statusCode, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.IO.Stream stream, System.IDisposable client, System.IDisposable response)
        {
            StatusCode = statusCode;
            Headers = headers;
            Stream = stream;
            _client = client;
            _response = response;
        }

        public void Dispose()
        {
            Stream.Dispose();
            if (_response != null)
                _response.Dispose();
            if (_client != null)
                _client.Dispose();
        }
    }


    
    public partial class ApiException : System.Exception
    {
        public int StatusCode
        {
            get; private set;
        }

        public string Response
        {
            get; private set;
        }

        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> Headers
        {
            get; private set;
        }

        public ApiException(string message, int statusCode, string response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.Exception innerException)
            : base(message + "\n\nStatus: " + statusCode + "\nResponse: \n" + ((response == null) ? "(null)" : response.Substring(0, response.Length >= 512 ? 512 : response.Length)), innerException)
        {
            StatusCode = statusCode;
            Response = response;
            Headers = headers;
        }

        public override string ToString()
        {
            return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
        }
    }

    public partial class ApiException<TResult> : ApiException
    {
        public TResult Result
        {
            get; private set;
        }

        public ApiException(string message, int statusCode, string response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, TResult result, System.Exception innerException)
            : base(message, statusCode, response, headers, innerException)
        {
            Result = result;
        }
    }

}

