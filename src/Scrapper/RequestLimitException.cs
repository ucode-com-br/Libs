using System;

namespace UCode.Scrapper
{
    public class RequestLimitException : Exception
    {
        public RequestLimitException(int maxRequests) : base($"The defined limit of {maxRequests} requests has been reached.") => this.MaxRequests = maxRequests;

        public int MaxRequests
        {
            get;
        }
    }
}
