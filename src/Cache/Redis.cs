using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace UCode.Cache
{
    public class Redis
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger _logger;

        public Redis(IDistributedCache distributedCache, ILogger<Redis> logger)
        {
            this._distributedCache = distributedCache;
            this._logger = logger;
        }

        public async Task<TResult> GetOrSetCacheSlidingAsync<TResult>([NotNull] string keyName,
            Func<Task<TResult>> funcSetCache, TimeSpan ttl) where TResult : class => await this.GetOrSetCacheAsync(keyName, funcSetCache, ttl);

        public async Task<TResult> GetOrSetCacheAbsoluteAsync<TResult>([NotNull] string keyName,
            Func<Task<TResult>> funcSetCache, TimeSpan ttl) where TResult : class => await this.GetOrSetCacheAsync(keyName, funcSetCache, null, ttl);

        public async Task<bool> ExistAsync([NotNull] string keyName, CancellationToken token = default)
        {
            var result = await this._distributedCache.GetAsync(keyName, token);

            return result != null || result.Length > 0;
        }

        public async Task<TResult> GetAsync<TResult>([NotNull] string keyName,
            CancellationToken token = default) where TResult : class
        {
            TResult result = default;

            var jsonBytes = await this._distributedCache.GetAsync(keyName, token);

            if (jsonBytes != null)
            {
                string json = null;
                try
                {
                    json = LZStringCSharp.LZString.DecompressFromUint8Array(jsonBytes);
                }
                catch
                {
                    json = Encoding.UTF8.GetString(jsonBytes);
                }

                if (json != null)
                {
                    result = System.Text.Json.JsonSerializer.Deserialize<TResult>(json);
                }
            }

            return result;
        }

        public async Task RemoveAsync([NotNull] string keyName, CancellationToken token = default) => await this._distributedCache.RemoveAsync(keyName, token);

        public async Task SetAsync<TResult>([NotNull] string keyName, TResult content,
            TimeSpan? ttlSliding = null, TimeSpan? ttlAbsoluteToNow = null,
            CancellationToken token = default) where TResult : class
        {
            if (!ttlSliding.HasValue && !ttlAbsoluteToNow.HasValue)
            {
                throw new ArgumentNullException("Time to live can't be null.");
            }

            var options = new DistributedCacheEntryOptions();
            if (ttlSliding.HasValue)
            {
                options.SetSlidingExpiration(ttlSliding.Value);
            }
            else
            {
                options.SetAbsoluteExpiration(ttlAbsoluteToNow.Value);
            }

            var json = System.Text.Json.JsonSerializer.Serialize(content);

            var jsonCompressed = LZStringCSharp.LZString.CompressToUint8Array(json);

            //await _distributedCache.SetAsync(keyName, Encoding.UTF8.GetBytes(json), token);
            await this._distributedCache.SetAsync(keyName, jsonCompressed, token);
        }

        [return: MaybeNull]
        public async Task<TResult> GetOrSetCacheAsync<TResult>([NotNull] string keyName,
            Func<Task<TResult>> funcSetCache, TimeSpan? ttlSliding = null, TimeSpan? ttlAbsoluteToNow = null) where TResult : class
        {
            if (!ttlSliding.HasValue && !ttlAbsoluteToNow.HasValue)
            {
                throw new ArgumentNullException("Time to live can't be null.");
            }

            TResult result = default;

            var jsonBytes = await this._distributedCache.GetAsync(keyName);

            if (jsonBytes == null)
            {
                try
                {
                    result = await funcSetCache();
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                    {
                        throw;
                    }
                    else
                    {
                        this._logger.LogError(ex.Message, ex);
                    }
                }

                if (result == default)
                {
                    try
                    {
                        var options = new DistributedCacheEntryOptions();
                        if (ttlSliding.HasValue)
                        {
                            options.SetSlidingExpiration(ttlSliding.Value);
                        }
                        else
                        {
                            options.SetAbsoluteExpiration(ttlAbsoluteToNow.Value);
                        }

                        var json = System.Text.Json.JsonSerializer.Serialize(result);

                        var jsonCompressed = LZStringCSharp.LZString.CompressToUint8Array(json);

                        //await _distributedCache.SetAsync(keyName, Encoding.UTF8.GetBytes(json), options);
                        await this._distributedCache.SetAsync(keyName, jsonCompressed, options);
                    }
                    catch (Exception ex)
                    {
                        if (Debugger.IsAttached)
                        {
                            throw;
                        }
                        else
                        {
                            this._logger.LogError(ex.Message, ex);
                        }
                    }
                }

            }
            else
            {
                try
                {
                    string json = null;
                    try
                    {
                        json = LZStringCSharp.LZString.DecompressFromUint8Array(jsonBytes);
                    }
                    catch
                    {
                        json = Encoding.UTF8.GetString(jsonBytes);
                    }

                    if (json != null)
                    {
                        result = System.Text.Json.JsonSerializer.Deserialize<TResult>(json);
                    }
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                    {
                        throw;
                    }
                    else
                    {
                        this._logger.LogError(ex.Message, ex);
                    }

                    await this._distributedCache.RemoveAsync(keyName);

                    result = await funcSetCache();
                }
            }


            return result;
        }
    }
}
