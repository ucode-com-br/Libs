using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace UCode.RedisDatabase
{
    public abstract class RegistedDatabase
    {
        [NotNull]
        private readonly RedisOptions RedisOptions;

        [NotNull]
        private readonly ILoggerFactory LoggerFactory;

        [NotNull]
        private readonly Lazy<ILogger<RegistedDatabase>> logger;

        [NotNull]
        protected ILogger<RegistedDatabase> Logger => this.logger.Value;

        protected RegistedDatabase([NotNull] ILoggerFactory loggerFactory, [NotNull] RedisOptions redisOptions)
        {
            this.LoggerFactory = loggerFactory;
            this.RedisOptions = redisOptions;

            this.logger = new Lazy<ILogger<RegistedDatabase>>(this.LoggerFactory.CreateLogger<RegistedDatabase>());
        }

        private string _connectionPoolId;

        [NotNull]
        public string ConnectionPoolId
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._connectionPoolId))
                {

                    var hash = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(this.ConnectionString + this.DatabaseName));

                    this._connectionPoolId = Convert.ToBase64String(hash);
                }

                return this._connectionPoolId;
            }
        }

        [NotNull]
        public abstract string DatabaseName
        {
            get;
        }

        [NotNull]
        public abstract string ConnectionString
        {
            get;
        }

        [return: NotNull]
        internal static (DateTime Created, long Index) GetDbIndex([NotNull] ConnectionMultiplexer connectionMultiplexer, [NotNull] string databaseName)
        {
            ArgumentException.ThrowIfNullOrEmpty(databaseName, nameof(databaseName));

            var shemaDatabase = connectionMultiplexer.GetDatabase(0);

            var redisValue = shemaDatabase.StringGet($"redis:database:{databaseName}.created");


            (DateTime Created, long Index) result;
            if (redisValue.IsNull)
            {
                var now = DateTime.UtcNow;

                shemaDatabase.StringSet($"redis:database:{databaseName}.created", now.Ticks);
                var index = shemaDatabase.StringIncrement($"redis:databases") + 2;
                result = (now, index);
            }
            else
            {
                var createdDatabase = new DateTime(Convert.ToInt64(redisValue, System.Globalization.CultureInfo.InvariantCulture), DateTimeKind.Utc);

                var index = Convert.ToInt64(shemaDatabase.StringGet($"redis:databases"), System.Globalization.CultureInfo.InvariantCulture);

                result = (createdDatabase, index);
            }


            shemaDatabase.StringSet($"redis:database:{databaseName}.index", result.Index);

            return result;
        }

        [return: NotNull]
        internal static ConnectionMultiplexer GetConnectionMultiplexer(string clientId, string connectionString, int defaultDatabase)
        {
            var config = ConfigurationOptions.Parse(connectionString);

            config.ClientName = clientId;
            config.DefaultDatabase = defaultDatabase;
            //config.AbortOnConnectFail = false;

            var connect = ConnectionMultiplexer.Connect(config);


            return connect;
        }

        private readonly object _getDatabaseLock = new();
        private readonly Dictionary<string, ConnectionMultiplexer> _connectionCache = new();
        private readonly Dictionary<string, RedisDatabase> _redisDatabaseCache = new();

        [return: NotNull]
        public RedisDatabase GetDatabase(string? clientId = null)
        {
            lock (this._getDatabaseLock)
            {
                ConnectionMultiplexer? connection = null;

                if (string.IsNullOrWhiteSpace(clientId))
                {
                    clientId = this.ConnectionPoolId;
                }

                if (!this._connectionCache.ContainsKey(clientId))
                {
                    connection = GetConnectionMultiplexer(clientId, this.ConnectionString, 0);

                    this._connectionCache.Add(clientId, connection);
                }
                else
                {
                    connection = this._connectionCache[clientId];
                }

                if (!this._redisDatabaseCache.ContainsKey(clientId))
                {
                    var dbIndex = GetDbIndex(connection, this.DatabaseName);

                    var database = connection.GetDatabase(Convert.ToInt32(dbIndex.Index));

                    this._redisDatabaseCache.Add(clientId, new RedisDatabase(this.LoggerFactory, this.LoggerFactory.CreateLogger<RedisDatabase>(), this.DatabaseName, dbIndex.Created, database, this.RedisOptions));
                }
            }

            return this._redisDatabaseCache[clientId];
        }


        public static implicit operator RedisDatabase([NotNull] RegistedDatabase source) => source.GetDatabase();
    }

}
