using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

// ReSharper disable NotResolvedInText

namespace UCode.RedisDatabase
{

    public class RedisDistributedCache
    {
        //public bool Compress = true;
        /*public bool UseBinnaryConverter = true;


        private byte[]? Serialize<T>(T source) where T : new()
        {
            byte[]? result = null;

            var bytes = UseBinnaryConverter ? BinaryConverter.Serialize<T>(source) : System.Text.Json.JsonSerializer.SerializeToUtf8Bytes<T>(source);

            if (Compress)
            {
                var buffer = new byte[K4os.Compression.LZ4.LZ4Codec.MaximumOutputSize(bytes.Length)];
                var encodedLength = K4os.Compression.LZ4.LZ4Codec.Encode(bytes, buffer, K4os.Compression.LZ4.LZ4Level.L00_FAST);

                result = new byte[encodedLength];
                buffer.CopyTo(result, 0);
            }
            else
            {
                result = bytes;
            }

            return result;
        }

        private T Deserialize<T>(byte[]? source) where T : new()
        {
            if (source == null)
                return default;

            byte[] bytes;

            if(Compress)
            {
                var buffer = new byte[K4os.Compression.LZ4.LZ4Codec.MaximumOutputSize(source.Length)];
                var decodedLength = K4os.Compression.LZ4.LZ4Codec.Decode(source, buffer);

                bytes = new byte[decodedLength];
                buffer.CopyTo(bytes, 0);
            }
            else
            {
                bytes = source;
            }

            T result = UseBinnaryConverter ? BinaryConverter.Deserialize<T>(bytes) : System.Text.Json.JsonSerializer.Deserialize<T>(bytes);

            return result;
        }
        */

        private readonly IDistributedCache _distributedCache;
        private readonly ILogger _logger;
        private readonly RedisOptions _options;

        public RedisDistributedCache(IDistributedCache distributedCache, RedisOptions options, ILogger<RedisDistributedCache> logger)
        {
            this._distributedCache = distributedCache;
            this._logger = logger;
            this._options = options;
        }


        public async Task<bool> ExistAsync([NotNull] string keyName, CancellationToken token = default)
        {
            var result = await this._distributedCache.GetAsync(keyName, token);

            return result != null || result.Length > 0;
        }

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

        public async Task RemoveAsync([NotNull] string keyName, CancellationToken token = default) => await this._distributedCache.RemoveAsync(keyName, token);

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
