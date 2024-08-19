using System;
using Microsoft.Extensions.Caching.Distributed;

namespace UCode.RedisDatabase
{
    public readonly struct TimeToLive
    {
        private readonly bool? _sliding;
        private readonly TimeSpan? _expiration;
        private readonly DateTime _created;

        private TimeToLive(bool? sliding, TimeSpan? expiration)
        {
            this._created = DateTime.UtcNow;
            this._sliding = sliding;
            this._expiration = expiration;
        }

        public static TimeToLive ToSliding(TimeSpan? offset) => new(offset == null ? null : true, offset);

        public static TimeToLive ToAbsolute(TimeSpan? relative) => new(relative == null ? null : false, relative);

        public bool? Sliding => this._sliding;
        public TimeSpan? Expiration => this._expiration;

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
