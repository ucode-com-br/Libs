using System;
using System.Collections.Generic;

namespace UCode.Apis.CnpjJa
{
    /// <summary>
    /// Represents a typed API exception containing both the error details and the result object
    /// </summary>
    /// <typeparam name="TResult">Type of the result returned from the API call</typeparam>
    public partial class ApiException<TResult> : ApiException
    {
        /// <summary>
        /// Gets the result object returned by the API (if available)
        /// </summary>
        public TResult Result
        {
            get; private set;
        }

        /// <summary>
        /// Initializes a new instance of the ApiException with detailed error information
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        /// <param name="statusCode">HTTP status code of the response</param>
        /// <param name="response">HTTP response content</param>
        /// <param name="headers">HTTP response headers</param>
        /// <param name="result">Result object returned from the API</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public ApiException(string message, int statusCode, string response, IReadOnlyDictionary<string, IEnumerable<string>> headers, TResult result, Exception innerException)
            : base(message, statusCode, response, headers, innerException)
        {
            Result = result;
        }
    }

    /// <summary>
    /// Represents a base class for API exceptions containing HTTP response details
    /// </summary>
    public partial class ApiException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code returned by the API
        /// </summary>
        public int StatusCode
        {
            get; private set;
        }

        public string Response
        {
            get; private set;
        }

        public IReadOnlyDictionary<string, IEnumerable<string>>? Headers
        {
            get; private set;
        }

        /// <summary>
        /// Initializes a new instance of the ApiException with basic error information
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        /// <param name="statusCode">HTTP status code of the response</param>
        /// <param name="response">HTTP response content</param>
        /// <param name="headers">HTTP response headers</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public ApiException(string message, int statusCode, string response, IReadOnlyDictionary<string, IEnumerable<string>>? headers = default, Exception? innerException = default)
            : base(message + "\n\nStatus: " + statusCode + "\nResponse: \n" + ((response == null) ? "(null)" : response.Substring(0, response.Length >= 512 ? 512 : response.Length)), innerException)
        {
            StatusCode = statusCode;
            Response = response;
            Headers = headers;
        }

        /// <summary>
        /// Creates and returns a string representation of the current exception
        /// </summary>
        /// <returns>A string representation of the exception including response details</returns>
        public override string ToString()
        {
            return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
        }
    }

}

