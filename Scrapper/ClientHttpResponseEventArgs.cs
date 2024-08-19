using System;

namespace UCode.Scrapper
{
    public class ClientHttpResponseEventArgs : EventArgs
    {

        public ClientHttpResponseEventArgs(TimeSpan elapsed, ResultSnapshot resultSnapshot)
        {
            this.Elapsed = elapsed;
            this.ResultSnapshot = resultSnapshot;
        }

        public ResultSnapshot ResultSnapshot
        {
            get;
        }


        public TimeSpan Elapsed
        {
            get;
        }
    }
}
