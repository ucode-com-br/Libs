using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using AngleSharp.Dom;
using HtmlAgilityPack;
using OpenQA.Selenium;
using UCode.Extensions;
using UCode.Scrapper.Tags;
using Cookie = System.Net.Cookie;

namespace UCode.Scrapper
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class Extensions
    {
        private static readonly Browser.Startup _browserStartup;
        static Extensions()
        {
            _browserStartup = new Browser.Startup();
        }

        public static HttpResponseMessage ReloadUsingBrowser(this HttpResponseMessage httpResponseMessage)
        {
            var tempFile = System.IO.Path.GetTempFileName();

            HttpContent httpContent = ((HttpContent)httpResponseMessage.Content);

            using (var fileStream = new System.IO.FileStream(tempFile, FileMode.Create))
            {
                httpContent.ReadAsStream().CopyTo(fileStream);
            }

            var url = $"file://{tempFile.Replace("\\", "/")}";

            var (Driver, Navigation) = _browserStartup.CreateDriverNavigator();

            Navigation.GoToUrl(url);

            var result = new HttpResponseMessage(httpResponseMessage.StatusCode);

            foreach (var head in httpResponseMessage.Headers)
            {
                result.Headers.Add(head.Key, head.Value);
            }


            result.ReasonPhrase = httpResponseMessage.ReasonPhrase;
            result.RequestMessage = httpResponseMessage.RequestMessage;
            result.Version = httpResponseMessage.Version;
            result.Content = new StringContent(Driver.PageSource, System.Text.Encoding.UTF8);

            //var element = Driver.FindElement(By.XPath(""));

            return result;
        }

        public static string GetSource(this HttpResponseMessage httpResponseMessage)
        {
            string result = null;

            var tempFile = System.IO.Path.GetTempFileName();

            HttpContent httpContent = ((HttpContent)httpResponseMessage.Content);

            using (var fileStream = new System.IO.FileStream(tempFile, FileMode.Create))
            {
                httpContent.ReadAsStream().CopyTo(fileStream);
            }

            var url = $"file://{tempFile.Replace("\\", "/")}";

            var (Driver, Navigation) = _browserStartup.CreateDriverNavigator();

            result = Driver.PageSource;


            Driver.Close();
            Driver.Dispose();

            return result;
        }

        public static string ProcessHtml(this string htmlSource)
        {
            string result = null;

            var tempFile = System.IO.Path.GetTempFileName();

            using (var fileStream = new FileStream(tempFile, FileMode.Create))
            using (var memory = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(htmlSource)))
            {
                memory.CopyTo(fileStream);
            }

            var url = $"file://{tempFile.Replace("\\", "/")}";

            var (Driver, Navigation) = _browserStartup.CreateDriverNavigator();

            result = Driver.PageSource;

            
            Driver.Close();
            Driver.Dispose();

            return result;
        }




        [return: NotNull]
        public static HtmlDocument ConvertToHtmlDocument([NotNull] this HttpResponseMessage httpResponseMessage)
        {

            var pageDocument = new HtmlDocument();

            pageDocument.LoadHtml(GetSource(httpResponseMessage));

            return pageDocument;

            //var readAsStringTask = httpResponseMessage.Content.ReadAsStringAsync();

            //var pageDocument = new HtmlDocument();

            //pageDocument.LoadHtml(readAsStringTask.Result);

            //return pageDocument;
        }

        [return: NotNull]
        public static Head GetHead([NotNull] this HtmlDocument htmlDocument)
        {
            var headDocument = htmlDocument.DocumentNode.SelectSingleNode("//head").InnerHtml.ConvertToHtmlDocument();

            var head = new Head(headDocument);

            head.Metas.AddMetas(headDocument);
            head.Links.AddLinks(headDocument);
            head.Bases.AddBases(headDocument);


            return head;
        }


        private static void AddBases(this List<Base> bases, HtmlDocument htmlDocument)
        {
            var metaNodes = htmlDocument.DocumentNode.SelectNodes("//base");

            var metaindex = 0;
            foreach (var meta in metaNodes)
            {
                bases.Add(new Base(metaindex++, meta.GetNodeAttribute("href"), meta.GetNodeAttribute("target"),
                    meta.GetNodeAttribute("accesskey"), meta.GetNodeAttribute("class"), meta.GetNodeAttribute("contenteditable"), GetNodeDataAttribute(meta), meta.GetNodeAttribute("dir"), meta.GetNodeAttribute("draggable"), meta.GetNodeAttribute("hidden"), meta.GetNodeAttribute("id"), meta.GetNodeAttribute("lang"), meta.GetNodeAttribute("spellcheck"), meta.GetNodeAttribute("style"), meta.GetNodeAttribute("tabindex"), meta.GetNodeAttribute("title"), meta.GetNodeAttribute("translate"),
                    meta.GetNodeAttribute("itemprop"), meta.GetNodeAttribute("itemid"), meta.GetNodeAttribute("itemref"), meta.GetNodeAttribute("itemscope"), meta.GetNodeAttribute("itemtype"), meta.GetNodeAttribute("nonce")));
            }
        }

        private static void AddLinks(this List<Link> links, HtmlDocument htmlDocument)
        {
            var metaNodes = htmlDocument.DocumentNode.SelectNodes("//link");

            var metaindex = 0;
            foreach (var meta in metaNodes)
            {

                links.Add(new Link(metaindex++, meta.GetNodeAttribute("crossorigin"), meta.GetNodeAttribute("href"), meta.GetNodeAttribute("hreflang"), meta.GetNodeAttribute("media"),
                    meta.GetNodeAttribute("referrerpolicy"), meta.GetNodeAttribute("rel"), meta.GetNodeAttribute("sizes"), meta.GetNodeAttribute("title"), meta.GetNodeAttribute("type"), meta.InnerHtml,
                    meta.GetNodeAttribute("accesskey"), meta.GetNodeAttribute("class"), meta.GetNodeAttribute("contenteditable"), GetNodeDataAttribute(meta), meta.GetNodeAttribute("dir"), meta.GetNodeAttribute("draggable"), meta.GetNodeAttribute("hidden"), meta.GetNodeAttribute("id"), meta.GetNodeAttribute("lang"), meta.GetNodeAttribute("spellcheck"), meta.GetNodeAttribute("style"), meta.GetNodeAttribute("tabindex"), meta.GetNodeAttribute("translate"),
                    meta.GetNodeAttribute("itemprop"), meta.GetNodeAttribute("itemid"), meta.GetNodeAttribute("itemref"), meta.GetNodeAttribute("itemscope"), meta.GetNodeAttribute("itemtype"), meta.GetNodeAttribute("nonce")));
            }
        }

        private static void AddMetas(this List<Meta> metas, HtmlDocument htmlDocument)
        {
            var metaNodes = htmlDocument.DocumentNode.SelectNodes("//meta");

            var metaindex = 0;
            foreach (var meta in metaNodes)
            {
                metas.Add(new Meta(metaindex++, meta.GetNodeAttribute("name"), meta.GetNodeAttribute("charset"), meta.GetNodeAttribute("http-equiv"), meta.GetNodeAttribute("property"),
                    meta.GetNodeAttribute("content"), meta.InnerHtml, meta.GetNodeAttribute("accesskey"), meta.GetNodeAttribute("class"), meta.GetNodeAttribute("contenteditable"), GetNodeDataAttribute(meta), meta.GetNodeAttribute("dir"), meta.GetNodeAttribute("draggable"), meta.GetNodeAttribute("hidden"), meta.GetNodeAttribute("id"), meta.GetNodeAttribute("lang"), meta.GetNodeAttribute("spellcheck"), meta.GetNodeAttribute("style"), meta.GetNodeAttribute("tabindex"), meta.GetNodeAttribute("title"), meta.GetNodeAttribute("translate"),
                    meta.GetNodeAttribute("itemprop"), meta.GetNodeAttribute("itemid"), meta.GetNodeAttribute("itemref"), meta.GetNodeAttribute("itemscope"), meta.GetNodeAttribute("itemtype"), meta.GetNodeAttribute("nonce")));
            }
        }

        private static Dictionary<string?, string?>? GetNodeDataAttribute(this HtmlNode htmlNode)
        {
            var dataAttr = htmlNode.GetDataAttributes();
            Dictionary<string?, string?>? dataDict = null;
            foreach (var data in dataAttr)
            {
                dataDict ??= new Dictionary<string?, string?>();

                dataDict.Add(data.Name, data.Value);
            }

            return dataDict;
        }

        private static string? GetNodeAttribute(this HtmlNode htmlNode, string attributeName)
        {
            var attribute = htmlNode.Attributes[attributeName];

            if (attribute == null)
            {
                return null;
            }

            return attribute.Value;
        }


        /*[return: NotNull]
        public static List<Cookie> List([NotNull] this CookieContainer container)
        {
            var cookies = new List<Cookie>();

            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable",
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.GetField |
                                                                    BindingFlags.Instance,
                                                                    null,
                                                                    container,
                                                                    new object[] { });

            foreach (var key in table.Keys)
            {

                Uri uri = null;

                var domain = key as string;

                if (domain == null)
                    continue;

                if (domain.StartsWith("."))
                    domain = domain.Substring(1);

                var address = string.Format("http://{0}/", domain);

                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out uri) == false)
                    continue;

                foreach (Cookie cookie in container.GetCookies(uri))
                {
                    cookies.Add(cookie);
                }
            }

            return cookies;
        }*/

        public static Stream ToStream(this CookieContainer cookieContainer)
        {
            var resultBytes = JsonSerializer.SerializeToUtf8Bytes(cookieContainer, new JsonSerializerOptions { WriteIndented = false, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Always });

            return new MemoryStream(resultBytes);
            /*var stream = new MemoryStream();
            var binF = new BinaryFormatter();


            binF.Serialize(stream, cookieContainer);

            return stream;*/
        }

        public static CookieContainer? ToCookieContainer(this Stream stream) =>
            //return (CookieContainer)new BinaryFormatter().Deserialize(stream);
            JsonSerializer.Deserialize<CookieContainer>(stream, new JsonSerializerOptions { WriteIndented = false, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Always });

        public static IEnumerable<Cookie> Get(this CookieContainer container)
        {
            if (container != default && container.Count > 0)
            {
                var table = (Hashtable)container
                    .GetType().InvokeMember("m_domainTable",
                        BindingFlags.NonPublic |
                        BindingFlags.GetField |
                        BindingFlags.Instance,
                        null,
                        container,
                        Array.Empty<object>());


                foreach (var key in table.Keys)
                {
                    // Look for http cookies.
                    if (container.GetCookies(new Uri(uriString: $"http://{key}/")).Count > 0)
                    {
                        foreach (var cookie in container.GetCookies(
                            new Uri(uriString: $"http://{key}/")).Cast<Cookie>())
                        {
                            yield return cookie;
                        }
                    }

                    // Look for https cookies
                    if (container.GetCookies(new Uri(uriString: $"https://{key}/")).Count > 0)
                    {
                        foreach (var cookie in container.GetCookies(
                            new Uri(uriString: $"https://{key}/")).Cast<Cookie>())
                        {
                            yield return cookie;
                        }
                    }
                }
            }
        }

        [return: NotNull]
        public static HtmlDocument ConvertToHtmlDocument([NotNull] this string @string)
        {
            var pageDocument = new HtmlDocument();

            pageDocument.LoadHtml(@string);

            return pageDocument;
        }

        [return: NotNull]
        public static string ConvertToString([NotNull] this HttpResponseMessage httpResponseMessage)
        {
            var readAsStringTask = httpResponseMessage.Content.ReadAsStringAsync();

            readAsStringTask.Wait();

            return readAsStringTask.Result;
        }

        [return: NotNull]
        public static byte[] ConvertToByteArray([NotNull] this HttpResponseMessage httpResponseMessage)
        {
            var readAsStringTask = httpResponseMessage.Content.ReadAsByteArrayAsync();

            readAsStringTask.Wait();

            return readAsStringTask.Result;
        }

        //public static Dictionary<string, string> MatchNamedCaptures(this string regex, string input)
        //{
        //    return MatchNamedCaptures(new Regex(regex), input);
        //}

        //public static Dictionary<string, string> MatchNamedCaptures(this Regex regex, string input)
        //{
        //    var namedCaptureDictionary = new Dictionary<string, string>();
        //    GroupCollection groups = regex.Match(input).Groups;
        //    string[] groupNames = regex.GetGroupNames();
        //    foreach (string groupName in groupNames)
        //        if (groups[groupName].Captures.Count > 0)
        //            namedCaptureDictionary.Add(groupName, groups[groupName].Value);
        //    return namedCaptureDictionary;
        //}

        [return: NotNull]
        public static ClientHttpForm[] GetForms([NotNull] this HtmlDocument htmlDocument,
            [NotNull] ClientHttpHandler clientHttpHandler)
        {
            var forms = htmlDocument.DocumentNode.SelectNodes("//form");

            var listClientHttpHandlerForm = new List<ClientHttpForm>();

            foreach (var form in forms)
            {
                var clientHttpHandlerForm = new ClientHttpForm(
                    clientHttpHandler,
                    form.GetAttributeValue("id", ""),
                    form.GetAttributeValue("name", ""),
                    form.GetAttributeValue("method", ""),
                    form.GetAttributeValue("action", ""));

                var inputs = form.SelectNodes("//input");


                //List<string> postContentArray = new List<string>(inputs.Count);
                foreach (var input in inputs)
                {
                    var inputName =
                        input.GetAttributeValue("name",
                            ""); //HttpUtility.UrlEncode(input.GetAttributeValue("name", ""));
                    var inputValue =
                        input.GetAttributeValue("value",
                            ""); //HttpUtility.UrlEncode(input.GetAttributeValue("value", ""));
                    var inputType = input.GetAttributeValue("type", "");


                    switch (inputType)
                    {
                        case "hidden":
                        case "text":
                        case "password":
                        case "checkbox":
                        case "submit":
                            //postContentArray.Add($"{inputName}={inputValue}");
                            clientHttpHandlerForm.inputByName.Add(inputName, inputValue);
                            break;
                        default:
                            break;
                    }


                    //string.Join('&', postContentArray.ToArray());
                }

                listClientHttpHandlerForm.Add(clientHttpHandlerForm);
            }

            return listClientHttpHandlerForm.ToArray();
        }
    }
}
