using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace UCode.RedisDatabase
{
    /// <summary>
    /// Represents an abstract base class for registered databases.
    /// This class may contain common functionality and properties 
    /// that can be inherited by derived database classes.
    /// </summary>
    /// <remarks>
    /// Derived classes must implement the required functionality 
    /// specific to their database type.
    /// </remarks>
    public abstract class RegistedDatabase
    {
        [NotNull]
        private readonly RedisOptions RedisOptions;

        [NotNull]
        private readonly ILoggerFactory LoggerFactory;

        [NotNull]
        private readonly Lazy<ILogger<RegistedDatabase>> logger;

        /// <summary>
        /// Gets the logger instance for the <see cref="RegistedDatabase"/> class.
        /// </summary>
        /// <value>
        /// The <see cref="ILogger{RegistedDatabase}"/> instance used for logging.
        /// </value>
        /// <remarks>
        /// The logger is accessed lazily to improve performance and resource management.
        /// </remarks>
        [NotNull]
        protected ILogger<RegistedDatabase> Logger => this.logger.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistedDatabase"/> class.
        /// </summary>
        /// <param name="loggerFactory">
        /// The <see cref="ILoggerFactory"/> used to create loggers.
        /// Must not be null.
        /// </param>
        /// <param name="redisOptions">
        /// The options for configuring Redis.
        /// Must not be null.
        /// </param>
        protected RegistedDatabase([NotNull] ILoggerFactory loggerFactory, [NotNull] RedisOptions redisOptions)
        {
            this.LoggerFactory = loggerFactory;
            this.RedisOptions = redisOptions;
        
            this.logger = new Lazy<ILogger<RegistedDatabase>>(this.LoggerFactory.CreateLogger<RegistedDatabase>());
        }

        private string _connectionPoolId;

        /// <summary>
        /// Gets the unique identifier for the connection pool.
        /// The identifier is generated based on the connection string and database name 
        /// if it has not been set previously.
        /// </summary>
        /// <value>
        /// A base64 string representing the unique connection pool identifier.
        /// </value>
        /// <remarks>
        /// The identifier is computed using the SHA256 hash of the concatenation 
        /// of the ConnectionString and DatabaseName properties.
        /// </remarks>
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

        /// <summary>
        /// Gets the name of the database associated with the implementing class.
        /// </summary>
        /// <remarks>
        /// This property must not return null and is implemented in derived classes.
        /// </remarks>
        /// <returns>
        /// A string representing the database name.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the database name cannot be determined.
        /// </exception>
        [NotNull]
        public abstract string DatabaseName
        {
            get;
        }

        /// <summary>
        /// Represents the connection string to the database.
        /// This property must be implemented by derived classes,
        /// and it cannot be null.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that contains the connection string.
        /// </value>
        /// <remarks>
        /// The derived class must provide an implementation for this property,
        /// ensuring that it returns a valid and non-null connection string.
        /// </remarks>
        [NotNull]
        public abstract string ConnectionString
        {
            get;
        }

        /// <summary>
        /// Retrieves the creation date and index of a specified Redis database.
        /// </summary>
        /// <param name="connectionMultiplexer">
        /// The connection multiplexer used to interact with the Redis server.
        /// </param>
        /// <param name="databaseName">
        /// The name of the Redis database whose index and creation date are to be retrieved.
        /// </param>
        /// <returns>
        /// A tuple containing the creation date and the index of the specified Redis database.
        /// The 'Created' field indicates when the database was created, 
        /// and the 'Index' field is a unique long value associated with the database.
        /// </returns>
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

        /// <summary>
        /// Retrieves a <see cref="ConnectionMultiplexer"/> instance configured with the specified client ID, 
        /// connection string, and default database number.
        /// </summary>
        /// <param name="clientId">A unique identifier for the client, used in connection configuration.</param>
        /// <param name="connectionString">The connection string used to configure the connection multiplexer.</param>
        /// <param name="defaultDatabase">The default database number to use when connecting.</param>
        /// <returns>
        /// A <see cref="ConnectionMultiplexer"/> instance that represents the connection to the Redis server.
        /// </returns>
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

        /// <summary>
        /// Retrieves a RedisDatabase instance associated with the specified client ID.
        /// If no client ID is provided, it uses the default connection pool ID. 
        /// It employs locking to ensure thread safety when accessing the connection and database caches.
        /// </summary>
        /// <param name="clientId">Optional. The ID of the client requesting the database. If null or whitespace, 
        /// the default connection pool ID is used.</param>
        /// <returns>A RedisDatabase instance that is not null and is associated with the specified or default client ID.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the underlying Redis connection cannot be established.</exception>
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
