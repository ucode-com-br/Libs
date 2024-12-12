using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace UCode.RedisDatabase
{
    /// <summary>
    /// Represents a sealed class for a default Redis database.
    /// This class inherits from the <see cref="RegistedDatabase"/> class.
    /// </summary>
    /// <remarks>
    /// This class is used to encapsulate the behavior and properties 
    /// specific to the default Redis database implementation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var redisDatabase = new RedisDefaultDatabase();
    /// </code>
    /// </example>
    public sealed class RedisDefaultDatabase : RegistedDatabase
    {
        private readonly string _databaseName;
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisDefaultDatabase"/> class.
        /// </summary>
        /// <param name="loggerFactory">
        /// An instance of <see cref="ILoggerFactory"/> used for logging.
        /// Must not be <c>null</c>.</param>
        /// <param name="connectionString">
        /// A string representing the connection details to the Redis database.
        /// Must not be <c>null</c>.</param>
        /// <param name="databaseName">
        /// The name of the Redis database.
        /// Must not be <c>null</c>.</param>
        /// <param name="redisOptions">
        /// An instance of <see cref="RedisOptions"/> containing the configuration options for Redis.
        /// Must not be <c>null</c>.</param>
        internal RedisDefaultDatabase([NotNull] ILoggerFactory loggerFactory, [NotNull] string connectionString, [NotNull] string databaseName, [NotNull] RedisOptions redisOptions) : base(loggerFactory, redisOptions)
        {
            this._databaseName = databaseName;
            this._connectionString = connectionString;
        }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <value>
        /// A string representing the name of the database. 
        /// This property overrides the base class property and 
        /// ensures that it will always return a non-null value 
        /// as indicated by the <see cref="NotNull"/> attribute.
        /// </value>
        [NotNull]
        public override string DatabaseName => this._databaseName;

        /// <summary>
        /// Gets the connection string for the current instance of the class.
        /// </summary>
        /// <value>
        /// A string representing the connection string.
        /// </value>
        /// <remarks>
        /// This property overrides the ConnectionString property 
        /// from the base class and ensures that it is never null.
        /// </remarks>
        [NotNull]
        public override string ConnectionString => this._connectionString;
    }
}
