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

        /// <summary>
        /// Initializes a new instance of the <see cref="Redis"/> class.
        /// </summary>
        /// <param name="distributedCache">The distributed cache implementation to use.</param>
        /// <param name="logger">The logger to use for logging errors.</param>
        public Redis(IDistributedCache distributedCache, ILogger<Redis> logger)
        {
            this._distributedCache = distributedCache;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves a cached value or sets a new value with a sliding expiration.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="keyName">The key name of the cached value.</param>
        /// <param name="funcSetCache">The function to set the cache value if it doesn't exist.</param>
        /// <param name="ttl">The time to live for the cached value.</param>
        /// <returns>The cached value or the result of the set function.</returns>
        public async Task<TResult> GetOrSetCacheSlidingAsync<TResult>([NotNull] string keyName,
            Func<Task<TResult>> funcSetCache, TimeSpan ttl) where TResult : class => await this.GetOrSetCacheAsync(keyName, funcSetCache, ttl);

        /// <summary>
        /// Retrieves a cached value or sets a new value with an absolute expiration.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="keyName">The key name of the cached value.</param>
        /// <param name="funcSetCache">The function to set the cache value if it doesn't exist.</param>
        /// <param name="ttl">The time to live for the cached value.</param>
        /// <returns>The cached value or the result of the set function.</returns>
        public async Task<TResult> GetOrSetCacheAbsoluteAsync<TResult>([NotNull] string keyName,
            Func<Task<TResult>> funcSetCache, TimeSpan ttl) where TResult : class => await this.GetOrSetCacheAsync(keyName, funcSetCache, null, ttl);

        /// <summary>
        /// Checks if a cached value exists.
        /// </summary>
        /// <param name="keyName">The key name of the cached value.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>True if the cached value exists, false otherwise.</returns>
        public async Task<bool> ExistAsync([NotNull] string keyName, CancellationToken token = default)
        {
            var result = await this._distributedCache.GetAsync(keyName, token);

            return result != null || result.Length > 0;
        }

        /// <summary>
        /// Retrieves a cached value from the distributed cache.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="keyName">The key name of the cached value.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The cached value or the default value of <typeparamref name="TResult"/> if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="keyName"/> is null.</exception>
        public async Task<TResult> GetAsync<TResult>([NotNull] string keyName,
            CancellationToken token = default) where TResult : class
        {
            // Initialize the result variable with the default value
            TResult result = default;

            // Get the cached value as a byte array
            var jsonBytes = await this._distributedCache.GetAsync(keyName, token);

            if (jsonBytes != null)
            {
                string json = null;
                try
                {
                    // Try to decompress the byte array using LZStringCSharp
                    json = LZStringCSharp.LZString.DecompressFromUint8Array(jsonBytes);
                }
                catch
                {
                    // If decompression fails, try to convert the byte array to a string using UTF8 encoding
                    json = Encoding.UTF8.GetString(jsonBytes);
                }

                if (json != null)
                {
                    // Deserialize the JSON string to an object of type TResult
                    result = System.Text.Json.JsonSerializer.Deserialize<TResult>(json);
                }
            }
            // Return the result
            return result;
        }

        /// <summary>
        /// Removes a cached value from the distributed cache.
        /// </summary>
        /// <param name="keyName">The key name of the cached value.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="keyName"/> is null.</exception>
        public async Task RemoveAsync([NotNull] string keyName, CancellationToken token = default) => await this._distributedCache.RemoveAsync(keyName, token);

        /// <summary>
        /// Sets a cached value in the distributed cache with the specified time to live.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="keyName">The key name of the cached value.</param>
        /// <param name="content">The content to be cached.</param>
        /// <param name="ttlSliding">The sliding time to live for the cached value. If null, <paramref name="ttlAbsoluteToNow"/> must be specified.</param>
        /// <param name="ttlAbsoluteToNow">The absolute expiration time to live for the cached value. If null, <paramref name="ttlSliding"/> must be specified.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if both <paramref name="ttlSliding"/> and <paramref name="ttlAbsoluteToNow"/> are null.</exception>
        public async Task SetAsync<TResult>([NotNull] string keyName, TResult content,
            TimeSpan? ttlSliding = null, TimeSpan? ttlAbsoluteToNow = null,
            CancellationToken token = default) where TResult : class
        {
            if (!ttlSliding.HasValue && !ttlAbsoluteToNow.HasValue)
            {
                throw new ArgumentNullException("Time to live can't be null.");
            }

            // Create a new distributed cache entry options object
            var options = new DistributedCacheEntryOptions();

            // Set the time to live for the cached value
            if (ttlSliding.HasValue)
            {
                options.SetSlidingExpiration(ttlSliding.Value);
            }
            else
            {
                options.SetAbsoluteExpiration(ttlAbsoluteToNow.Value);
            }

            // Serialize the content to JSON
            var json = System.Text.Json.JsonSerializer.Serialize(content);

            // Compress the JSON using LZStringCSharp
            var jsonCompressed = LZStringCSharp.LZString.CompressToUint8Array(json);

            //await _distributedCache.SetAsync(keyName, Encoding.UTF8.GetBytes(json), token);
            // Set the cached value in the distributed cache using the key name and compressed JSON
            await this._distributedCache.SetAsync(keyName, jsonCompressed, token);
        }

        /// <summary>
        /// Retrieves a cached value or sets a new value with a sliding expiration.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="keyName">The key name of the cached value.</param>
        /// <param name="funcSetCache">The function to set the cache value if it doesn't exist.</param>
        /// <param name="ttlSliding">The sliding time to live for the cached value. If null, <paramref name="ttlAbsoluteToNow"/> must be specified.</param>
        /// <param name="ttlAbsoluteToNow">The absolute expiration time to live for the cached value. If null, <paramref name="ttlSliding"/> must be specified.</param>
        /// <returns>The cached value or the result of the set function.</returns>
        /// <exception cref="ArgumentNullException">Thrown if both <paramref name="ttlSliding"/> and <paramref name="ttlAbsoluteToNow"/> are null.</exception>
        [return: MaybeNull]
        public async Task<TResult> GetOrSetCacheAsync<TResult>([NotNull] string keyName,
            Func<Task<TResult>> funcSetCache, TimeSpan? ttlSliding = null, TimeSpan? ttlAbsoluteToNow = null) where TResult : class
        {
            // Check if both ttlSliding and ttlAbsoluteToNow are null
            if (!ttlSliding.HasValue && !ttlAbsoluteToNow.HasValue)
            {
                throw new ArgumentNullException("Time to live can't be null.");
            }

            TResult result = default;

            // Retrieve the cached value as a byte array
            var jsonBytes = await this._distributedCache.GetAsync(keyName);

            if (jsonBytes == null)
            {
                try
                {
                    // Set the cache value using the provided function
                    result = await funcSetCache();
                }
                catch (Exception ex)
                {
                    // Log the exception if not in debug mode
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
                        // Create a new distributed cache entry options object
                        var options = new DistributedCacheEntryOptions();

                        // Set the time to live for the cached value
                        if (ttlSliding.HasValue)
                        {
                            options.SetSlidingExpiration(ttlSliding.Value);
                        }
                        else
                        {
                            options.SetAbsoluteExpiration(ttlAbsoluteToNow.Value);
                        }

                        // Serialize the result to JSON
                        var json = System.Text.Json.JsonSerializer.Serialize(result);

                        // Compress the JSON using LZStringCSharp
                        var jsonCompressed = LZStringCSharp.LZString.CompressToUint8Array(json);

                        //await _distributedCache.SetAsync(keyName, Encoding.UTF8.GetBytes(json), options);
                        // Set the compressed JSON in the distributed cache
                        await this._distributedCache.SetAsync(keyName, jsonCompressed, options);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception if not in debug mode
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
                        // Try to decompress the byte array using LZStringCSharp
                        json = LZStringCSharp.LZString.DecompressFromUint8Array(jsonBytes);
                    }
                    catch
                    {
                        // If decompression fails, try to convert the byte array to a string using UTF8 encoding
                        json = Encoding.UTF8.GetString(jsonBytes);
                    }

                    if (json != null)
                    {
                        // Deserialize the JSON string to an object of type TResult
                        result = System.Text.Json.JsonSerializer.Deserialize<TResult>(json);
                    }
                }
                catch (Exception ex)
                {
                    // If the debugger is attached, rethrow the exception
                    if (Debugger.IsAttached)
                    {
                        throw;
                    }
                    else
                    {
                        // Log the exception message and stack trace
                        this._logger.LogError(ex.Message, ex);
                    }

                    // Remove the cached value
                    await this._distributedCache.RemoveAsync(keyName);

                    // Set a new value if necessary
                    result = await funcSetCache();
                }
            }


            return result;
        }
    }
}
