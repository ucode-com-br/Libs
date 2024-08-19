using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace UCode.Scrapper
{
    public interface IResultSnapshot
    {
        IEnumerable<Cookie>? Cookies
        {
            get;
        }
        TimeSpan? Elapsed
        {
            get;
        }
        Exception? Exception
        {
            get;
        }
        int Index
        {
            get;
        }
        HttpRequestMessage? Request
        {
            get;
        }
        HttpResponseMessage? Response
        {
            get;
        }
    }
}
