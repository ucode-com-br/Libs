using System;
using System.Diagnostics.CodeAnalysis;

namespace UCode.Scrapper
{
    /// <summary>
    /// Client exceptions arguments for the event handler
    /// </summary>
    public class ClientHttpExceptionEventArgs : EventArgs
    {
        public ClientHttpExceptionEventArgs([NotNull] string message, [MaybeNull] Exception? exception, [MaybeNull] TimeSpan? elapsed, [MaybeNull] ResultSnapshot? resultSnapshot)
        {
            this.Message = message;
            this.Exception = exception;
            this.Elapsed = elapsed;
            this.ResultSnapshot = resultSnapshot;
        }

        /// <summary>
        /// Messa of the exception
        /// </summary>
        [NotNull]
        public string Message
        {
            get;
        }

        /// <summary>
        /// Exception raised
        /// </summary>
        [MaybeNull]
        public Exception? Exception
        {
            get;
        }

        /// <summary>
        /// Maybe stopwatch used to measured the time
        /// </summary>
        [MaybeNull]
        public TimeSpan? Elapsed
        {
            get;
        }

        /// <summary>
        /// Http request message
        /// </summary>
        [MaybeNull]
        public ResultSnapshot? ResultSnapshot
        {
            get;
        }

    }
}
