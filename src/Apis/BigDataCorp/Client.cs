using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UCode.Extensions;
using UCode.Scrapper;

namespace UCode.Apis.BigDataCorp
{

    public class Client
    {
        private Models.Authentication Authentication
        {
            get;
        }




        public Client(string accessToken, string tokenId) : this(new Models.Authentication() { AccessToken = accessToken, TokenId = tokenId })
        {

        }

        public Client(Models.Authentication authentication)
        {
            Authentication = authentication;
        }

        private async Task<Models.ResponseData<TData>?> MarketplaceAsync<TData>(UCode.Scrapper.ClientHttpHandler clientHttpHandler, Models.Payload<TData> payload)
        //where TResult : Models.ResponseResult<TData>
        {
            return await RequestAsync("https://plataforma.bigdatacorp.com.br/marketplace", clientHttpHandler, payload);
        }

        private async Task<Models.ResponseData<TData>?> PessoasAsync<TData>(UCode.Scrapper.ClientHttpHandler clientHttpHandler, Models.Payload<TData> payload)
        //where TResult : Models.ResponseResult<TData>
        {
            return await RequestAsync("https://plataforma.bigdatacorp.com.br/pessoas", clientHttpHandler, payload);
        }

        private async Task<Models.ResponseData<TData>?> RequestAsync<TData>(string url, UCode.Scrapper.ClientHttpHandler clientHttpHandler, Models.Payload<TData> payload)
        //where TResult : Models.ResponseResult<TData>
        {
            Models.ResponseData<TData>? resp = default;

            var header = new Header();

            header.Add("Content-Type", "application/json");
            header.Add("Accept", "application/json");
            header.Add("AccessToken", Authentication.AccessToken);
            header.Add("TokenId", Authentication.TokenId);

            var resultSnapshot = await clientHttpHandler.PostJsonAsync(url, payload, header);

            if (resultSnapshot.Response != null)
            {
                var json = await resultSnapshot.Response.Content.ReadAsStringAsync();

                if (resultSnapshot.Response.StatusCode == HttpStatusCode.OK)
                {
                    // Existe situaçoes excepcionais que mesmo com status code 200 o objeto é um erro
                    try
                    {
                        resp = System.Text.Json.JsonSerializer.Deserialize<Models.ResponseData<TData>?>(json);
                    }
                    catch (Exception tResponseSerializeExcemption)
                    {

                    }
                }
                else
                {

                }


            }


            return resp;
        }


        private async Task<Models.ResponseData<TData>?> CompanyAsync<TData>(UCode.Scrapper.ClientHttpHandler clientHttpHandler, Models.Payload<TData> payload)
        //where TResult : Models.ResponseResult<TData>
        {
            return await RequestAsync("https://plataforma.bigdatacorp.com.br/empresas", clientHttpHandler, payload);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regionInfo"></param>
        /// <param name="taxId"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<Models.Company.ActivityIndicators> ActivityIndicatorsAsync(RegionInfo regionInfo, string taxId)
        {

            ClientHttpHandler clientHttpHandler = new ClientHttpHandler();

            var payload = new Models.Payload<Models.Company.ActivityIndicators>();


            switch (regionInfo)
            {
                case { TwoLetterISORegionName: "BR" }:

                    var cnpj = taxId.Replace(".", "").Replace("-", "").Replace("/", "").Replace("\\", "").Replace(" ", "").Trim();

                    //payload.Q = $"doc{{{tax}}}";
                    payload.Query.Doc = cnpj;

                    var responseData = await CompanyAsync<Models.Company.ActivityIndicators>(clientHttpHandler, payload);

                    if (responseData != null)
                    {
                        foreach (var item in responseData.Result)
                        {
                            yield return item.Value;
                        }
                    }
                    break;
                default:
                    yield break;
            }

        }

        public async IAsyncEnumerable<Models.Marketplace.QUODCreditScorePerson> QUODCreditScorePersonAsync(RegionInfo regionInfo, string taxId)
        {

            ClientHttpHandler clientHttpHandler = new ClientHttpHandler();

            var payload = new Models.Payload<Models.Marketplace.QUODCreditScorePerson>();


            switch (regionInfo)
            {
                case { TwoLetterISORegionName: "BR" }:

                    var tax = taxId.Replace(".", "").Replace("-", "").Replace("/", "").Replace("\\", "").Replace(" ", "").Trim();

                    //payload.Q = $"doc{{{tax}}}";
                    payload.Query.Doc = tax;

                    var responseData = await CompanyAsync<Models.Marketplace.QUODCreditScorePerson>(clientHttpHandler, payload);

                    if (responseData != null)
                    {
                        foreach (var item in responseData.Result)
                        {
                            yield return item.Value;
                        }
                    }
                    break;
                default:
                    yield break;
            }

        }

        public async IAsyncEnumerable<Models.Person.FinancialRisk> PersonFinancialRiskAsync(RegionInfo regionInfo, string taxId)
        {

            ClientHttpHandler clientHttpHandler = new ClientHttpHandler();

            var payload = new Models.Payload<Models.Person.FinancialRisk>();


            switch (regionInfo)
            {
                case { TwoLetterISORegionName: "BR" }:

                    var tax = taxId.Replace(".", "").Replace("-", "").Replace("/", "").Replace("\\", "").Replace(" ", "").Trim();

                    //payload.Q = $"doc{{{tax}}}";
                    payload.Query.Doc = tax;

                    var responseData = await PessoasAsync<Models.Person.FinancialRisk>(clientHttpHandler, payload);

                    if (responseData != null)
                    {
                        foreach (var item in responseData.Result)
                        {
                            yield return item.Value;
                        }
                    }
                    break;
                default:
                    yield break;
            }
        }

        public async IAsyncEnumerable<Models.Person.Scholarship> PersonScholarshipAsync(RegionInfo regionInfo, string taxId)
        {

            ClientHttpHandler clientHttpHandler = new ClientHttpHandler();

            var payload = new Models.Payload<Models.Person.Scholarship>();


            switch (regionInfo)
            {
                case { TwoLetterISORegionName: "BR" }:

                    var tax = taxId.Replace(".", "").Replace("-", "").Replace("/", "").Replace("\\", "").Replace(" ", "").Trim();

                    //payload.Q = $"doc{{{tax}}}";
                    payload.Query.Doc = tax;

                    var responseData = await PessoasAsync<Models.Person.Scholarship>(clientHttpHandler, payload);

                    if (responseData != null)
                    {
                        foreach (var item in responseData.Result)
                        {
                            yield return item.Value;
                        }
                    }
                    break;
                default:
                    yield break;
            }
        }

        public async IAsyncEnumerable<Models.Person.IndebtednessQuestion> PersonIndebtednessQuestionAsync(RegionInfo regionInfo, string taxId)
        {

            ClientHttpHandler clientHttpHandler = new ClientHttpHandler();

            var payload = new Models.Payload<Models.Person.IndebtednessQuestion>();


            switch (regionInfo)
            {
                case { TwoLetterISORegionName: "BR" }:

                    var tax = taxId.Replace(".", "").Replace("-", "").Replace("/", "").Replace("\\", "").Replace(" ", "").Trim();

                    //payload.Q = $"doc{{{tax}}}";
                    payload.Query.Doc = tax;

                    var responseData = await PessoasAsync<Models.Person.IndebtednessQuestion>(clientHttpHandler, payload);

                    if (responseData != null)
                    {
                        foreach (var item in responseData.Result)
                        {
                            yield return item.Value;
                        }
                    }
                    break;
                default:
                    yield break;
            }

        }

        public async IAsyncEnumerable<Models.Company.MediaProfileAndExposure> CompanyMediaProfileAndExposureAsync(RegionInfo regionInfo, string taxId)
        {

            ClientHttpHandler clientHttpHandler = new ClientHttpHandler();

            var payload = new Models.Payload<Models.Company.MediaProfileAndExposure>();


            switch (regionInfo)
            {
                case { TwoLetterISORegionName: "BR" }:

                    var tax = taxId.Replace(".", "").Replace("-", "").Replace("/", "").Replace("\\", "").Replace(" ", "").Trim();

                    //payload.Q = $"doc{{{tax}}}";
                    payload.Query.Doc = tax;

                    var responseData = await CompanyAsync<Models.Company.MediaProfileAndExposure>(clientHttpHandler, payload);

                    if (responseData != null)
                    {
                        foreach (var item in responseData.Result)
                        {
                            yield return item.Value;
                        }
                    }
                    break;
                default:
                    yield break;
            }

        }




        public async IAsyncEnumerable<Models.Marketplace.DatariskIncomePrediction> PersonIncomePredictionAsync(RegionInfo regionInfo, string taxId)
        {

            ClientHttpHandler clientHttpHandler = new ClientHttpHandler();

            var payload = new Models.Payload<Models.Marketplace.DatariskIncomePrediction>();


            switch (regionInfo)
            {
                case { TwoLetterISORegionName: "BR" }:

                    var tax = taxId.Replace(".", "").Replace("-", "").Replace("/", "").Replace("\\", "").Replace(" ", "").Trim();

                    //payload.Q = $"doc{{{tax}}}";
                    payload.Query.Doc = tax;

                    var responseData = await MarketplaceAsync<Models.Marketplace.DatariskIncomePrediction>(clientHttpHandler, payload);

                    if (responseData != null)
                    {
                        foreach (var item in responseData.Result)
                        {
                            yield return item.Value;
                        }
                    }
                    break;
                default:
                    yield break;
            }

        }


        public async IAsyncEnumerable<Models.Person.LawsuitsDistributionData> PersonLawsuitsDistributionDataAsync(RegionInfo regionInfo, string taxId)
        {

            ClientHttpHandler clientHttpHandler = new ClientHttpHandler();

            var payload = new Models.Payload<Models.Person.LawsuitsDistributionData>();


            switch (regionInfo)
            {
                case { TwoLetterISORegionName: "BR" }:

                    var tax = taxId.Replace(".", "").Replace("-", "").Replace("/", "").Replace("\\", "").Replace(" ", "").Trim();

                    //payload.Q = $"doc{{{tax}}}";
                    payload.Query.Doc = tax;

                    var responseData = await PessoasAsync<Models.Person.LawsuitsDistributionData>(clientHttpHandler, payload);

                    if (responseData != null)
                    {
                        foreach (var item in responseData.Result)
                        {
                            yield return item.Value;
                        }
                    }
                    break;
                default:
                    yield break;
            }

        }

        public async IAsyncEnumerable<Models.Person.Processes> PersonLawsuitsAsync(RegionInfo regionInfo, string taxId)
        {
            ClientHttpHandler clientHttpHandler = new ClientHttpHandler();

            var payload = new Models.Payload<Models.Person.Processes>();


            switch (regionInfo)
            {
                case { TwoLetterISORegionName: "BR" }:

                    var tax = taxId.Replace(".", "").Replace("-", "").Replace("/", "").Replace("\\", "").Replace(" ", "").Trim();

                    //payload.Q = $"doc{{{tax}}}";
                    payload.Query.Doc = tax;

                    var responseData = await PessoasAsync<Models.Person.Processes>(clientHttpHandler, payload);

                    if (responseData != null)
                    {
                        foreach (var item in responseData.Result)
                        {
                            yield return item.Value;
                        }
                    }
                    break;
                default:
                    yield break;
            }
        }

        public async IAsyncEnumerable<Models.Person.PersonCollections> PersonCollectionsAsync(RegionInfo regionInfo, string taxId)
        {

            ClientHttpHandler clientHttpHandler = new ClientHttpHandler();

            var payload = new Models.Payload<Models.Person.PersonCollections>();


            switch (regionInfo)
            {
                case { TwoLetterISORegionName: "BR" }:

                    var tax = taxId.Replace(".", "").Replace("-", "").Replace("/", "").Replace("\\", "").Replace(" ", "").Trim();

                    //payload.Q = $"doc{{{tax}}}";
                    payload.Query.Doc = tax;

                    var responseData = await PessoasAsync<Models.Person.PersonCollections>(clientHttpHandler, payload);

                    if (responseData != null)
                    {
                        foreach (var item in responseData.Result)
                        {
                            yield return item.Value;
                        }
                    }
                    break;
                default:
                    yield break;
            }

        }


        public async IAsyncEnumerable<Models.Company.CompanyGroups> CompanyGroupsAsync(RegionInfo regionInfo, string taxId)
        {
            ClientHttpHandler clientHttpHandler = new ClientHttpHandler();

            var payload = new Models.Payload<Models.Company.CompanyGroups>();

            switch (regionInfo)
            {
                case { TwoLetterISORegionName: "BR" }:

                    var cnpj = taxId.Replace(".", "").Replace("-", "").Replace("/", "").Replace("\\", "").Replace(" ", "").Trim();

                    //payload.Q = $"doc{{{tax}}}";
                    payload.Query.Doc = cnpj;

                    var responseData = await CompanyAsync<Models.Company.CompanyGroups>(clientHttpHandler, payload);

                    if (responseData != null)
                    {
                        foreach (var rows in responseData.Result)
                        {
                            var captures = "doc\\{(?<doc>.*)\\}".MatchNamedCaptures(rows.MatchKeys);
                            var doc = captures.ContainsKey("doc") ? captures["doc"] : taxId;

                            var rowsValues = rows.Value;

                            for (int i = 0; i < rowsValues.Count; i++)
                            {
                                var record = rowsValues[i];

                                record.MatchKeyTaxId = doc;

                                if (record.MainCompanyTaxId == null && record.CompanyTaxIds.Length > 0)
                                {
                                    record.MainCompanyTaxId = record.CompanyTaxIds[0];
                                }
                            }

                            yield return rowsValues;
                        }
                    }
                    break;
                default:
                    yield break;
            }

        }

        [return: NotNull]
        public async IAsyncEnumerable<Models.Company.RegistrationData> RegistrationDataAsync([NotNull] RegionInfo regionInfo, [NotNull] string taxId)
        {
            ClientHttpHandler clientHttpHandler = new ClientHttpHandler();

            var payload = new Models.Payload<Models.Company.RegistrationData>();


            switch (regionInfo)
            {
                case { TwoLetterISORegionName: "BR" }:

                    var cnpj = taxId.Replace(".", "").Replace("-", "").Replace("/", "").Replace("\\", "").Replace(" ", "").Trim();

                    //payload.Q = $"doc{{{tax}}}";
                    payload.Query.Doc = cnpj;

                    var responseData = await CompanyAsync<Models.Company.RegistrationData>(clientHttpHandler, payload);

                    if (responseData != null)
                    {
                        foreach (var item in responseData.Result)
                        {
                            yield return item.Value;
                        }
                    }
                    break;
                default:
                    yield break;
            }

        }

        [return: NotNull]
        public async IAsyncEnumerable<Models.Marketplace.OwnerParticipationData> CompanyOwnersParticipationsDataAsync([NotNull] RegionInfo regionInfo, [NotNull] string taxId)
        {
            ClientHttpHandler clientHttpHandler = new ClientHttpHandler();

            var payload = new Models.Payload<Models.Marketplace.OwnerParticipationData>();


            switch (regionInfo)
            {
                case { TwoLetterISORegionName: "BR" }:

                    var cnpj = taxId.Replace(".", "").Replace("-", "").Replace("/", "").Replace("\\", "").Replace(" ", "").Trim();

                    //payload.Q = $"doc{{{tax}}}";
                    payload.Query.Doc = cnpj;

                    var responseData = await MarketplaceAsync<Models.Marketplace.OwnerParticipationData>(clientHttpHandler, payload);

                    if (responseData != null)
                    {
                        foreach (var item in responseData.Result)
                        {
                            yield return item.Value;
                        }
                    }
                    break;
                default:
                    yield break;
            }

        }



    }

}
