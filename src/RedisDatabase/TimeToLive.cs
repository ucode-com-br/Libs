using System;
using Microsoft.Extensions.Caching.Distributed;

namespace UCode.RedisDatabase
{
    /// <summary>
    /// Represents a time-to-live (TTL) configuration for caching, which can be either sliding 
    /// or absolute. It encapsulates the duration for which a cached item is valid.
    /// </summary>
    public readonly struct TimeToLive
    {
        private readonly bool? _sliding;
        private readonly TimeSpan? _expiration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeToLive"/> class.
        /// </summary>
        /// <param name="sliding">A nullable boolean indicating whether the time to live is sliding.</param>
        /// <param name="expiration">A nullable <see cref="TimeSpan"/> indicating the expiration duration.</param>
        /// <returns>
        /// This constructor does not return a value, but initializes the properties of the <see cref="TimeToLive"/> instance.
        /// </returns>
        private TimeToLive(bool? sliding, TimeSpan? expiration)
        {
            this._sliding = sliding;
            this._expiration = expiration;
        }

        /// <summary>
        /// Converts a nullable <see cref="TimeSpan"/> to a <see cref="TimeToLive"/> 
        /// object with sliding behavior based on the specified offset.
        /// </summary>
        /// <param name="offset">A nullable <see cref="TimeSpan"/> that defines the duration 
        /// for which the <see cref="TimeToLive"/> is valid. If null, the <see cref="TimeToLive"/> 
        /// is considered to have no duration.</param>
        /// <returns>A <see cref="TimeToLive"/> instance representing a sliding duration 
        /// based on the specified <paramref name="offset"/>.</returns>
        public static TimeToLive ToSliding(TimeSpan? offset) => new(offset == null ? null : true, offset);

        /// <summary>
        /// Converts a nullable TimeSpan representing relative time 
        /// to a TimeToLive object, which can represent either an absolute 
        /// time span or be null based on the input.
        /// </summary>
        /// <param name="relative">A nullable TimeSpan that represents 
        /// the relative time span to be converted.</param>
        /// <returns>A TimeToLive object where the duration is derived 
        /// from the provided relative TimeSpan, or a null TimeToLive 
        /// if the input is null.</returns>
        public static TimeToLive ToAbsolute(TimeSpan? relative) => new(relative == null ? null : false, relative);

        /// <summary>
        /// Gets a value indicating whether the current object is in a sliding state.
        /// </summary>
        /// <value>
        /// A <see cref="Nullable{Boolean}"/> that represents the sliding state. 
        /// It can be <c>true</c> if the object is sliding, <c>false</c> if it is not, 
        /// or <c>null</c> if the state is unknown.
        /// </value>
        public bool? Sliding => this._sliding;
        /// <summary>
        /// Gets the expiration time as a nullable <see cref="TimeSpan"/>.
        /// </summary>
        /// <value>
        /// The expiration time. If it is not set, the value will be <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// This property returns the expiration duration that determines 
        /// when a certain item or event should expire.
        /// </remarks>
        public TimeSpan? Expiration => this._expiration;

        /// <summary>
        /// Returns a TimeToLive instance based on the absolute expiration date and time provided.
        /// If the expiration date is null, a new TimeToLive instance with null values is returned.
        /// </summary>
        /// <param name="expiration">The optional expiration date and time as a nullable DateTime.</param>
        /// <returns>
        /// A TimeToLive instance representing the time span from the specified expiration 
        /// date and time to the current UTC date and time, or a new instance with null values if expiration is null.
        /// </returns>
        public static TimeToLive Absolute(DateTime? expiration)
        {
            if (expiration != null)
            {
                var dateTimeU = expiration.Value.ToUniversalTime();

                var timeSpan = DateTime.UtcNow - dateTimeU;

                return ToAbsolute(timeSpan);
            }
            else
            {
                return new TimeToLive(null, null);
            }
        }



        public static implicit operator TimeToLive(DateTime expiration) => Absolute(expiration);

        public static implicit operator TimeToLive(TimeSpan sliding) => ToSliding(sliding);

        public static implicit operator TimeSpan?(TimeToLive source) => source._expiration;

        public static implicit operator TimeSpan(TimeToLive source) => source._expiration ?? TimeSpan.Zero;

        public static explicit operator DistributedCacheEntryOptions(TimeToLive timeToLive)
        {
            var result = new DistributedCacheEntryOptions();

            if (timeToLive._sliding.HasValue && timeToLive._expiration.HasValue)
            {
                if (timeToLive._sliding.Value)
                {
                    result.SetSlidingExpiration(timeToLive._expiration.Value);
                }
                else
                {
                    result.SetAbsoluteExpiration(timeToLive._expiration.Value);
                }
            }

            return result;
        }
    }
}
