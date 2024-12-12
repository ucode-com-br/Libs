using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace UCode.RedisDatabase
{

    /// <summary>
    /// Represents a distributed cache implementation that uses Redis as its storage backend.
    /// This class provides methods for setting, getting, and removing cache entries.
    /// </summary>
    /// <remarks>
    /// The RedisDistributedCache can be configured with options such as expiration time and connection details.
    /// Ensure the appropriate Redis server is available and accessible for the cache to function correctly.
    /// </remarks>
    public class RedisDistributedCache
    {

        private readonly IDistributedCache _distributedCache;
        private readonly ILogger _logger;
        private readonly RedisOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisDistributedCache"/> class.
        /// </summary>
        /// <param name="distributedCache">An instance of <see cref="IDistributedCache"/> that provides access to a distributed cache.</param>
        /// <param name="options">An instance of <see cref="RedisOptions"/> containing configuration options for Redis.</param>
        /// <param name="logger">An instance of <see cref="ILogger{RedisDistributedCache}"/> used for logging.</param>
        /// <returns>
        /// A new instance of <see cref="RedisDistributedCache"/>
        /// </returns>
        public RedisDistributedCache(IDistributedCache distributedCache, RedisOptions options, ILogger<RedisDistributedCache> logger)
        {
            this._distributedCache = distributedCache;
            this._logger = logger;
            this._options = options;
        }


        /// <summary>
        /// Asynchronously checks if a value associated with the specified key exists in the distributed cache.
        /// </summary>
        /// <param name="keyName">The key for which to check the existence of a value in the cache.</param>
        /// <param name="token">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean value 
        /// indicating whether the value associated with the specified key exists in the cache.
        /// </returns>
        public async Task<bool> ExistAsync([NotNull] string keyName, CancellationToken token = default)
        {
            var result = await this._distributedCache.GetAsync(keyName, token);

            return result != null || result.Length > 0;
        }

        /// <summary>
        /// Asynchronously retrieves an object of type TResult from a distributed cache 
        /// using the specified key.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the result object that is expected to be retrieved from the cache. 
        /// It must be a class with a parameterless constructor.
        /// </typeparam>
        /// <param name="keyName">
        /// The key associated with the cached object to retrieve. This parameter cannot be null.
        /// </param>
        /// <param name="token">
        /// A cancellation token that can be used to signal cancellation of the asynchronous operation. 
        /// The default value is CancellationToken.None.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing the retrieved object of 
        /// type TResult, or null if no object with the specified key exists in the cache.
        /// </returns>
        public async Task<TResult?> GetAsync<TResult>([NotNull] string keyName,
            CancellationToken token = default) where TResult : class, new()
        {
            TResult? result = default;

            var jsonBytes = await this._distributedCache.GetAsync(keyName, token);

            if (jsonBytes != null)
            {
                result = this._options.Serializer.Deserialize<TResult>(this._options.Compressor.Decompress(jsonBytes));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously removes the cache entry with the specified key from the distributed cache.
        /// </summary>
        /// <param name="keyName">The key of the cache entry to remove. The key must not be null.</param>
        /// <param name="token">A cancellation token that can be used to cancel the operation (optional).</param>
        /// <returns>A task representing the asynchronous remove operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyName"/> is null.</exception>
        public async Task RemoveAsync([NotNull] string keyName, CancellationToken token = default) => await this._distributedCache.RemoveAsync(keyName, token);

        /// <summary>
        /// Asynchronously sets the specified content in the distributed cache with the given key name.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the content to be stored in the cache. Must be a class type and have a parameterless constructor.
        /// </typeparam>
        /// <param name="keyName">
        /// The key under which the content will be stored in the cache. This value cannot be null.
        /// </param>
        /// <param name="content">
        /// The content to be stored in the cache. If this is null, the method will return without making any changes.
        /// </param>
        /// <param name="timeToLive">
        /// Specifies the duration for which the content should remain in the cache. The Sliding property must not be null.
        /// </param>
        /// <param name="token">
        /// An optional <see cref="CancellationToken"/> to observe while waiting for the task to complete. The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="timeToLive"/> parameter's Sliding property is null.
        /// </exception>
        /// <remarks>
        /// This method uses the specified serializer to convert the content into a byte array, which is then stored in the distributed cache.
        /// If the <paramref name="content"/> is null, the method will exit without performing any operations.
        /// </remarks>
        public async Task SetAsync<TResult>([NotNull] string keyName, TResult? content,
            TimeToLive timeToLive,
            CancellationToken token = default) where TResult : class, new()
        {
            if (timeToLive.Sliding == null)
            {
                throw new ArgumentNullException("Time to live can't be null.");
            }

            if (content == default)
            {
                return;
            }

            var options = (DistributedCacheEntryOptions)timeToLive;

            //await _distributedCache.SetAsync(keyName, Encoding.UTF8.GetBytes(json), token);
            await this._distributedCache.SetAsync(keyName, this._options.Compressor.Compress(this._options.Serializer.Serialize(content)), token);
        }

        /// <summary>
        /// Asynchronously retrieves a cached value by its key. If the value is not in cache,
        /// it executes the provided function to get the value, caches it, and returns it.
        /// The method supports sliding expiration for cache entries.
        /// </summary>
        /// <typeparam name="TResult">The type of the result that can be cached, which must be a class type with a parameterless constructor.</typeparam>
        /// <param name="keyName">The key under which the value is stored in the cache.</param>
        /// <param name="funcSetCache">A function that retrieves the value to cache if it is not found in the cache.</param>
        /// <param name="timeToLive">An instance of <see cref="TimeToLive"/> that defines the expiration policy for the cache entry.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the cached value if found, or the value retrieved by <paramref name="funcSetCache"/> if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeToLive"/> is null.</exception>
        /// <remarks>
        /// This method will log errors using a logger if exceptions occur during the cache retrieval or setting process.
        /// If the data in the cache is not valid or cannot be deserialized, it will remove the corrupted entry and retry to get the value from the provided function.
        /// </remarks>
        [return: MaybeNull]
        public async Task<TResult?> GetOrSetCacheAsync<TResult>([NotNull] string keyName,
            Func<Task<TResult?>> funcSetCache, TimeToLive timeToLive) where TResult : class, new()
        {
            if (timeToLive.Sliding == null)
            {
                throw new ArgumentNullException("Time to live can't be null.");
            }

            TResult? result = default;

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
                        var options = (DistributedCacheEntryOptions)timeToLive;

                        await this._distributedCache.SetAsync(keyName, this._options.Compressor.Compress(this._options.Serializer.Serialize<TResult>(result)), options);
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
                    result = this._options.Serializer.Deserialize<TResult>(this._options.Compressor.Decompress(jsonBytes));
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
