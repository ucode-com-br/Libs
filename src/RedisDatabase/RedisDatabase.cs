using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace UCode.RedisDatabase
{
    /// <summary>
    /// Represents a connection to a Redis database.
    /// </summary>
    /// <remarks>
    /// This class provides methods for interacting with the Redis database,
    /// allowing for operations such as storing and retrieving data.
    /// </remarks>
    public class RedisDatabase
    {
        [NotNull]
        private readonly ILoggerFactory LoggerFactory;

        [NotNull]
        private readonly ILogger<RedisDatabase> Logger;

        [NotNull]
        internal readonly IDatabase Redis;

        [NotNull]
        internal readonly RedisOptions RedisOptions;

        /// <summary>
        /// Represents the name of the database.
        /// This property is marked with the [NotNull] attribute,
        /// indicating that it should never be null.
        /// </summary>
        /// <value>
        /// A non-null string that contains the name of the database.
        /// </value>
        [NotNull]
        public string DatabaseName
        {
            get;
        }

        /// <summary>
        /// Represents the index of the database. 
        /// This property is expected to be a non-null value.
        /// </summary>
        /// <value>
        /// An integer representing the index of the database.
        /// </value>
        /// <remarks>
        /// The DatabaseIndex is a read-only property; 
        /// it can only be set initially and cannot be modified thereafter.
        /// </remarks>
        [NotNull]
        public int DatabaseIndex
        {
            get;
        }

        /// <summary>
        /// Represents the date and time when the object was created.
        /// This property is read-only and cannot be modified after the object is instantiated.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> indicating the creation date and time of the object.
        /// </value>
        /// <remarks>
        /// The Created property is marked with the <see cref="NotNullAttribute"/> to indicate that it should
        /// never be null and will always contain a valid date and time.
        /// </remarks>
        [NotNull]
        public DateTime Created
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisDatabase"/> class.
        /// </summary>
        /// <param name="loggerFactory">An instance of <see cref="ILoggerFactory"/> used for logging.</param>
        /// <param name="logger">An instance of <see cref="ILogger{RedisDatabase}"/> used for logging specific to the RedisDatabase instance.</param>
        /// <param name="databaseName">The name of the Redis database.</param>
        /// <param name="created">The date and time when the Redis database was created.</param>
        /// <param name="redis">An instance of <see cref="IDatabase"/> representing the Redis database.</param>
        /// <param name="redisOptions">An instance of <see cref="RedisOptions"/> containing the configuration options for Redis.</param>
        /// <returns>
        /// A new instance of <see cref="RedisDatabase"/> initialized with the given parameters.
        /// </returns>
        internal RedisDatabase([NotNull] ILoggerFactory loggerFactory, [NotNull] ILogger<RedisDatabase> logger, [NotNull] string databaseName, [NotNull] DateTime created, [NotNull] IDatabase redis, [NotNull] RedisOptions redisOptions)
        {
            this.LoggerFactory = loggerFactory;
            this.Logger = logger;
            this.DatabaseName = databaseName;
            this.DatabaseIndex = redis.Database;
            this.Redis = redis;
            this.Created = created;
            this.RedisOptions = redisOptions;
        }



        [NotNull]
        public RedisCollection this[[NotNull] string collectionName] => new(this.LoggerFactory.CreateLogger<RedisCollection>(), this, collectionName);
    }

}
