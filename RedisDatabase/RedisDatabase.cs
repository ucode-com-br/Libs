using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace UCode.RedisDatabase
{
    /// <summary>
    /// Redis database reference
    /// </summary>
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

        [NotNull]
        public string DatabaseName
        {
            get;
        }

        [NotNull]
        public int DatabaseIndex
        {
            get;
        }

        [NotNull]
        public DateTime Created
        {
            get;
        }

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
