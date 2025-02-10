using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Represents the result of a send operation, encapsulating a value of type T and an optional exception.
    /// </summary>
    /// <typeparam name="T">The type of the value being sent.</typeparam>
    public struct SendResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendResult{T}"/> class.
        /// </summary>
        /// <param name="value">The value of type <typeparamref name="T"/> to be assigned to the <see cref="Value"/> property.</param>
        /// <param name="exception">An <see cref="Exception"/> that may have occurred during the operation.</param>
        /// <returns>
        /// A new instance of <see cref="SendResult{T}"/> with the specified value and exception.
        /// </returns>
        public SendResult(T @value, Exception exception)
        {
            this.Value = @value;
            this.Exception = exception;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendResult{T}"/> class 
        /// with the specified value.
        /// </summary>
        /// <param name="value">The value to be assigned to the <see cref="Value"/> property.</param>
        public SendResult(T @value) => this.Value = @value;

        /// <summary>
        /// Represents a generic property that can get or set a value of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value represented by the property.</typeparam>
        /// <value>
        /// The current value of the type <typeparamref name="T"/>, which can be retrieved or assigned.
        /// </value>
        public T Value
        {
            get; set;
        }
        /// <summary>
        /// Gets or sets the exception that has occurred.
        /// </summary>
        /// <value>
        /// The exception instance or null if no exception has occurred.
        /// </value>
        public Exception? Exception { get; set; } = null;
    }
}
