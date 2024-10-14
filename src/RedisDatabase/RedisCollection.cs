using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace UCode.RedisDatabase
{
    public class RedisCollection
    {
        private readonly ILogger<RedisCollection> Logger;

        [NotNull]
        private RedisDatabase Database
        {
            get;
        }

        private IDatabase RedisDatabase => this.Database.Redis;

        /// <summary>
        /// Collection name from Redis
        /// </summary>
        [NotNull]
        public string CollectionName
        {
            get;
        }

        /// <summary>
        /// Default command flags for Redis
        /// </summary>
        public CommandFlags DefaultCommandFlags { get; set; } = CommandFlags.PreferMaster;

        internal RedisCollection([NotNull] ILogger<RedisCollection> logger, [NotNull] RedisDatabase redisDatabase, [NotNull] string collectionName)
        {
            this.Logger = logger;
            this.Database = redisDatabase;
            this.CollectionName = collectionName;
        }

        [return: NotNull]
        private RedisKey GetKey([NotNull] string id) => new($"{this.Database.Redis}({this.Database.DatabaseIndex}).{this.CollectionName}[{id}]");



        [return: MaybeNull]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private T? GetValue<T>([NotNull] RedisValue redisValue)
        {
            switch (typeof(T))
            {
                case null:
                    return default;
                case Type type
                    when type == typeof(string) || type == typeof(int) ||
                        type == typeof(uint) || type == typeof(double) ||
                        type == typeof(byte[]) || type == typeof(bool) ||
                        type == typeof(long) || type == typeof(ulong) ||
                        type == typeof(float) || type == typeof(ReadOnlyMemory<byte>) ||
                        type == typeof(Memory<byte>):
                    return (T?)redisValue.Box();
                default:
                    var bytes = (byte[]?)redisValue.Box();
                    var decompress = this.Database.RedisOptions.Compressor.Decompress(bytes);
                    var obj = this.Database.RedisOptions.Serializer.Deserialize<T?>(decompress);
                    return obj;
            }
        }

        [return: NotNull]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private RedisValue SetValue<T>(T? source)
        {
            switch (source)
            {
                case null:
                    return RedisValue.Null;
                case string v:
                    return v;
                case int v:
                    return v;
                case uint v:
                    return v;
                case double v:
                    return v;
                case byte[] v:
                    return v;
                case bool v:
                    return v;
                case long v:
                    return v;
                case ulong v:
                    return v;
                case float v:
                    return v;
                case ReadOnlyMemory<byte> v:
                    return v;
                case Memory<byte> v:
                    return v;
                case RedisValue v:
                    return v;
                default:
                    var json = this.Database.RedisOptions.Serializer.Serialize(source);
                    var bytes = this.Database.RedisOptions.Compressor.Compress(json);
                    return bytes;
            }
        }

        #region Increment Decrement
        /// <summary>
        /// Increment value from id
        /// </summary>
        /// <param name="id">Row key/id</param>
        /// <param name="increment"></param>
        /// <returns></returns>
        [return: NotNull]
        public async Task<long> IncrementAsync([NotNull] string id, long increment = 1)
        {
            var key = this.GetKey(id);

            try
            {
                return await this.RedisDatabase.StringIncrementAsync(key, increment, this.DefaultCommandFlags);
            }
            catch (Exception ex)
            {
                this.Logger.LogError("IncrementAsync(...) fail to StringIncrementAsync.", ex);

                throw;
            }
        }

        /// <summary>
        /// Decrement value from id
        /// </summary>
        /// <param name="id">Row key/id</param>
        /// <param name="increment"></param>
        /// <returns></returns>
        [return: NotNull]
        public async Task<long> DecrementAsync([NotNull] string id, long increment = 1)
        {
            try
            {

                return await this.RedisDatabase.StringDecrementAsync(this.GetKey(id), increment, this.DefaultCommandFlags);
            }
            catch (Exception ex)
            {
                this.Logger.LogError("DecrementAsync(...) fail to StringDecrementAsync.", ex);
                throw;
            }
        }
        #endregion Increment Decrement


        

        [return: MaybeNull]
        public async Task<T?> GetAsync<T>([NotNull] string id)
        {
            ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

            try
            {
                var redisValue = await this.RedisDatabase.StringGetAsync(this.GetKey(id), this.DefaultCommandFlags);

                return this.GetValue<T>(redisValue);
            }
            catch (Exception ex)
            {
                this.Logger.LogError("GetAsync(...) fail to StringGetAsync.", ex);
            }

            return default;
        }

        [return: NotNull]
        public async IAsyncEnumerable<T> GetAsync<T>([NotNull] IEnumerable<string> ids)
        {
            var keys = ids.Where(w => !string.IsNullOrWhiteSpace(w)).Select(this.GetKey).ToArray();

            if (keys.Length == 0)
            {
                throw new ArgumentException($"Ids is empty.");
            }

            var redisValue = Array.Empty<RedisValue>();

            try
            {
                redisValue = await this.RedisDatabase.StringGetAsync(keys, this.DefaultCommandFlags);
            }
            catch (Exception ex)
            {
                this.Logger.LogError("GetAsync(...) fail to StringGetAsync[multiple itens].", ex);
            }

            for (var i = 0; i < redisValue.Length; i++)
            {
                var result = this.GetValue<T>(redisValue[i]);

                if (result != null)
                {
                    yield return result;
                }
            }
        }

        /// <summary>
        /// Replace item in cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="timeToLive">time to life in cache</param>
        /// <returns></returns>
        [return: NotNull]
        public async Task<bool> ReplaceAsync<T>([NotNull] string id, [NotNull] T value, TimeToLive? timeToLive = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));


            var redisValue = this.SetValue(value);

            if (redisValue.HasValue)
            {
                try
                {

                    var result = await this.RedisDatabase.StringSetAsync(this.GetKey(id), redisValue, timeToLive?.Expiration, When.Exists, this.DefaultCommandFlags);

                    return result;
                }
                catch (Exception ex)
                {
                    this.Logger.LogError("ReplaceAsync(...) fail to StringSet.", ex);
                }
            }

            return false;
        }

        [return: NotNull]
        public async Task<bool> CreateAsync<T>([NotNull] string id, [NotNull] T value, TimeToLive? timeToLive = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

            var redisValue = this.SetValue(value);

            if (redisValue.HasValue)
            {
                try
                {
                    var result = await this.RedisDatabase.StringSetAsync(this.GetKey(id), redisValue, timeToLive?.Expiration, When.NotExists, this.DefaultCommandFlags);

                    return result;
                }
                catch (Exception ex)
                {
                    this.Logger.LogError("CreateAsync(...) fail to StringSet.", ex);
                }
            }

            return false;
        }

        [return: NotNull]
        public async Task<bool> CreateOrUpdateAsync<T>([NotNull] string id, [NotNull] T value, TimeToLive? timeToLive = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

            var redisValue = this.SetValue(value);

            if (redisValue.HasValue)
            {
                try
                {
                    var result = await this.RedisDatabase.StringSetAsync(this.GetKey(id), redisValue, timeToLive?.Expiration, When.Always, this.DefaultCommandFlags);

                    return result;
                }
                catch (Exception ex)
                {
                    this.Logger.LogError("CreateOrUpdateAsync(...) fail to StringSet.", ex);
                }
            }

            return false;
        }

        [return: NotNull]
        public async Task<bool> RemoveAsync([NotNull] string id)
        {
            ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

            try
            {
                var redisValue = await this.RedisDatabase.KeyDeleteAsync(this.GetKey(id), this.DefaultCommandFlags);

                return redisValue;
            }
            catch (Exception ex)
            {
                this.Logger.LogError("RemoveAsync(...) fail to KeyDeleteAsync.", ex);
            }

            return false;
        }

        [return: MaybeNull]
        public async Task<TimeToLive?> TimeToLiveAsync([NotNull] string id)
        {
            ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

            try
            {
                var redisValue = await this.RedisDatabase.KeyTimeToLiveAsync(this.GetKey(id), this.DefaultCommandFlags);

                return redisValue;
            }
            catch (Exception ex)
            {
                this.Logger.LogError("TimeToLiveAsync(...) fail to KeyTimeToLiveAsync.", ex);
            }

            return default;
        }

        /// <summary>
        /// Returns the time since the object stored at the specified id is idle (not requested by read or write operations)
        /// </summary>
        /// <param name="id">The id to get the time of.</param>
        /// <returns>The time since the object stored at the specified key is idle</returns>
        public async Task<TimeSpan?> IdleTimeAsync([NotNull] string id)
        {
            ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

            try
            {

                var redisValue = await this.RedisDatabase.KeyIdleTimeAsync(this.GetKey(id), this.DefaultCommandFlags);

                return redisValue;
            }
            catch (Exception ex)
            {
                this.Logger.LogError("IdleTimeAsync(...) fail to KeyIdleTimeAsync.", ex);
            }

            return null;
        }

        /// <summary>
        /// Set a timeout on id. After the timeout has expired, the key will automatically be deleted. A key with an associated timeout is said to be volatile in Redis terminology.
        /// </summary>
        /// <param name="id">The id to set the expiration for.</param>
        /// <param name="expiry">The timeout to set.</param>
        /// <returns>1 if the timeout was set. 0 if key does not exist or the timeout could not be set.</returns>
        /// <remarks>If key is updated before the timeout has expired, then the timeout is removed as if the PERSIST command was invoked on key.
        /// For Redis versions &lt; 2.1.3, existing timeouts cannot be overwritten. So, if key already has an associated timeout, it will do nothing and return 0. Since Redis 2.1.3, you can update the timeout of a key. It is also possible to remove the timeout using the PERSIST command. See the page on key expiry for more information.</remarks>
        public async Task<bool> ExpireAsync([NotNull] string id, TimeToLive? expiry = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

            try
            {
                var redisValue = await this.RedisDatabase.KeyExpireAsync(this.GetKey(id), expiry, this.DefaultCommandFlags);

                return redisValue;
            }
            catch (Exception ex)
            {
                this.Logger.LogError("ExpireAsync(...) fail to KeyExpireAsync.", ex);
            }

            return false;
        }


        [return: MaybeNull]
        public T? GetWithExpiry<T>([NotNull] string id, out TimeToLive? expiry)
        {
            ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

            expiry = null;

            try
            {
                var redisValue = this.RedisDatabase.StringGetWithExpiry(this.GetKey(id), this.DefaultCommandFlags);

                expiry = redisValue.Expiry;

                if (redisValue.Value.HasValue)
                {
                    try
                    {

                        var json = (string?)redisValue.Value;
                        if (json != null)
                        {
                            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError("GetWithExpiry(...) fail to json deserialize.", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError("GetWithExpiry(...) fail to StringGetWithExpiry.", ex);
            }

            return default;
        }



        //public async Task<List<T>> AppendList<T>(string id, T item)
        //{
        //    var jsonItem = System.Text.Json.JsonSerializer.Serialize<T>(item);

        //    jsonItem = jsonItem.Trim().TrimStart('{').TrimEnd('}');

        //    return await Database.StringAppendAsync(GetKey(id), jsonItem);
        //}
    }

}
