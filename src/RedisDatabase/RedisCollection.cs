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
    /// <summary>
    /// Represents a collection that interacts with a Redis data store.
    /// </summary>
    /// <remarks>
    /// This class provides methods to perform various operations on Redis collections, such as adding, removing,
    /// and retrieving items. It is designed to work with the StackExchange.Redis client to facilitate communication
    /// with a Redis server.
    /// </remarks>
    public class RedisCollection
    {
        private readonly ILogger<RedisCollection> Logger;

        /// <summary>
        /// Represents a Redis database instance.
        /// </summary>
        /// <remarks>
        /// The Database property is marked with the <see cref="NotNull"/> attribute,
        /// indicating that it should never be null when accessed.
        /// </remarks>
        [NotNull]
        private RedisDatabase Database
        {
            get;
        }

        /// <summary>
        /// Gets the Redis database instance from the current database.
        /// </summary>
        /// <value>
        /// An instance of <see cref="IDatabase"/> representing the Redis database.
        /// </value>
        private IDatabase RedisDatabase => this.Database.Redis;

        /// <summary>
        /// Represents the name of the collection that cannot be null.
        /// </summary>
        /// <value>
        /// A string that represents the name of the collection. The value is intended to be 
        /// set to ensure that it is not null, enforced by the [NotNull] attribute.
        /// </value>
        [NotNull]
        public string CollectionName
        {
            get;
        }

        /// <summary>
        /// Gets or sets the default command flags for the operation.
        /// </summary>
        /// <value>
        /// The default command flags, which determine the behavior of commands when executed. 
        /// The default value is <see cref="CommandFlags.PreferMaster"/>.
        /// </value>
        public CommandFlags DefaultCommandFlags { get; set; } = CommandFlags.PreferMaster;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCollection"/> class.
        /// </summary>
        /// <param name="logger">An instance of <see cref="ILogger{T}"/> used for logging operations.</param>
        /// <param name="redisDatabase">An instance of <see cref="RedisDatabase"/> that represents the Redis database.</param>
        /// <param name="collectionName">The name of the collection in the Redis database.</param>
        internal RedisCollection([NotNull] ILogger<RedisCollection> logger, [NotNull] RedisDatabase redisDatabase, [NotNull] string collectionName)
        {
            this.Logger = logger;
            this.Database = redisDatabase;
            this.CollectionName = collectionName;
        }

        /// <summary>
        /// Constructs a Redis key based on the provided identifier.
        /// The key is formatted to include the Redis database connection 
        /// details, database index, collection name, and the unique identifier.
        /// </summary>
        /// <param name="id">
        /// The unique identifier used to generate the Redis key. 
        /// This parameter must not be null.
        /// </param>
        /// <returns>
        /// Returns a RedisKey object that represents the constructed key 
        /// based on the combination of database information and the provided id.
        /// </returns>
        [return: NotNull]
        private RedisKey GetKey([NotNull] string id) => new($"{this.Database.Redis}({this.Database.DatabaseIndex}).{this.CollectionName}[{id}]");



        /// <summary>
        /// Retrieves a value of type <typeparamref name="T"/> from the provided RedisValue.
        /// This method supports a specific set of primitive types as well as more complex types
        /// by utilizing serialization and compression methods defined in the Redis options.
        /// </summary>
        /// <typeparam name="T">
        /// The type of value to retrieve. It can be a primitive type or a complex type
        /// that can be handled by the defined serializer.
        /// </typeparam>
        /// <param name="redisValue">
        /// The RedisValue from which to extract the value.
        /// It must not be null, as denoted by the <see cref="NotNullAttribute"/>.
        /// </param>
        /// <returns>
        /// The value of type <typeparamref name="T"/> extracted from the RedisValue, 
        /// or null if the extraction was unsuccessful or the type is unsupported.
        /// The result is marked with <see cref="MaybeNullAttribute"/> indicating 
        /// that the return value can be null.
        /// </returns>
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

        /// <summary>
        /// Sets the specified value to a RedisValue, handling various types
        /// and serializing complex types into a compressed format if necessary.
        /// </summary>
        /// <typeparam name="T">The type of the source value.</typeparam>
        /// <param name="source">The source value to set, which can be of various types.</param>
        /// <returns>A RedisValue representing the input value, 
        /// or a serialized and compressed version if it is of a complex type.</returns>
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
        /// Increments the value of a specified key in the Redis database asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the key to increment.</param>
        /// <param name="increment">The amount by which to increment the value (default is 1).</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing the new value of the key after the increment.
        /// </returns>
        /// <exception cref="Exception">
        /// Throws an exception if the increment operation fails.
        /// </exception>
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
        /// Decrements the value of a Redis key specified by the provided identifier.
        /// </summary>
        /// <param name="id">The identifier of the Redis key to decrement.</param>
        /// <param name="increment">The amount to decrement the value by. Defaults to 1.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, 
        /// containing the new value of the key after the decrement operation.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when the decrement operation fails.
        /// </exception>
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


        

        /// <summary>
        /// Asynchronously retrieves an object of type <typeparamref name="T"/> from the Redis database using the specified identifier.
        /// </summary>
        /// <typeparam name="T">The type of the object to retrieve from the database.</typeparam>
        /// <param name="id">The identifier of the object to retrieve. It must not be null or empty.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing the retrieved object of type <typeparamref name="T"/> if found; otherwise, <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="id"/> is null or empty.</exception>
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

        /// <summary>
        /// Asynchronously retrieves a collection of values from a Redis database based on a list of identifiers.
        /// This method yields each value as it is retrieved, allowing for efficient processing of potentially large datasets.
        /// </summary>
        /// <typeparam name="T">The type of values to be returned.</typeparam>
        /// <param name="ids">An enumerable collection of string identifiers used to retrieve values from the Redis database.</param>
        /// <returns>
        /// An asynchronous sequence of values of type <typeparamref name="T"/> retrieved from the Redis database.
        /// Each value is yielded individually as it is retrieved, allowing for deferred processing.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="ids"/> collection is empty or contains only whitespace strings.</exception>
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
        /// Replaces the value associated with the specified identifier in the Redis database asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the value being replaced.</typeparam>
        /// <param name="id">The identifier for the value to be replaced.</param>
        /// <param name="value">The new value that will replace the existing value.</param>
        /// <param name="timeToLive">An optional time-to-live setting for the new value.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean indicating
        /// whether the value was successfully replaced.
        /// </returns>
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

        /// <summary>
        /// Asynchronously creates a new entry with the specified id and value in the database.
        /// </summary>
        /// <typeparam name="T">The type of the value to be stored.</typeparam>
        /// <param name="id">The identifier for the entry. Must not be null or empty.</param>
        /// <param name="value">The value to be stored in the database. Must not be null.</param>
        /// <param name="timeToLive">Optional. The time-to-live for the entry. If specified, the entry will expire after this duration.</param>
        /// <returns>
        /// Returns a task that represents the asynchronous operation. The task result contains 
        /// a boolean value indicating whether the entry was successfully created (true) or not (false).
        /// </returns>
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

        /// <summary>
        /// Asynchronously creates or updates a value in a Redis database associated with the specified id.
        /// </summary>
        /// <typeparam name="T">The type of the value being stored.</typeparam>
        /// <param name="id">The unique identifier for the value in the database. It should not be null or empty.</param>
        /// <param name="value">The value to be stored in the database. It should not be null.</param>
        /// <param name="timeToLive">An optional value specifying the expiration time for the entry. If not specified, the entry will not expire.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains <c>true</c> if the value was created or updated successfully; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Asynchronously removes an entry from the Redis database identified by the specified key.
        /// </summary>
        /// <param name="id">The identifier of the entry to be removed. Cannot be null or empty.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains 
        /// true if the entry was successfully removed; otherwise, false.
        /// </returns>
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

        /// <summary>
        /// Asynchronously retrieves the time to live (TTL) for a specified key in the Redis database.
        /// If the key does not exist or an error occurs, it will return null.
        /// </summary>
        /// <param name="id">
        /// The identifier of the key for which to retrieve the time to live.
        /// This parameter must not be null or empty.
        /// </param>
        /// <returns>
        /// A <see cref="TimeToLive"/> instance representing the TTL of the key, 
        /// or <c>null</c> if the key does not exist or if an error occurs during retrieval.
        /// </returns>
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
        /// Asynchronously retrieves the idle time of a given key identified by the specified identifier.
        /// The method performs an operation on a Redis database to obtain the idle time associated 
        /// with the given key. If the key does not exist or an error occurs during the operation, 
        /// it logs the error and returns null.
        /// </summary>
        /// <param name="id">
        /// The identifier of the key whose idle time is to be retrieved. 
        /// This parameter cannot be null or an empty string.
        /// </param>
        /// <returns>
        /// A <see cref="TimeSpan"/> representing the idle time of the key if successful; 
        /// otherwise, null if the key does not exist or an error occurred.
        /// </returns>
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
        /// Asynchronously sets an expiration time for the specified key in the Redis database.
        /// </summary>
        /// <param name="id">The identifier for the key that needs to expire.</param>
        /// <param name="expiry">An optional parameter representing the time-to-live for the key. If null, no expiration is set.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean value indicating 
        /// whether the expiration was successfully set (true) or not (false).
        /// </returns>
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


        /// <summary>
        /// Retrieves an object of type T from a Redis database using the specified identifier
        /// and also retrieves its associated expiration time.
        /// </summary>
        /// <typeparam name="T">The type of the object to be retrieved.</typeparam>
        /// <param name="id">The identifier for the object in the Redis database.</param>
        /// <param name="expiry">Output parameter that will store the time to live (TTL) of the object, if it exists.</param>
        /// <returns>
        /// Returns the object of type T if found; otherwise, returns null. 
        /// The return value is also marked as nullable to indicate that it can be null.
        /// </returns>
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


    }

}
