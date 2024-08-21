using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace UCode.RedisDatabase
{
    /// <summary>
    /// Default redis database when you using single database
    /// </summary>
    public sealed class RedisDefaultDatabase : RegistedDatabase
    {
        private readonly string _databaseName;
        private readonly string _connectionString;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="connectionString"></param>
        /// <param name="databaseName"></param>
        /// <param name="redisOptions"></param>
        internal RedisDefaultDatabase([NotNull] ILoggerFactory loggerFactory, [NotNull] string connectionString, [NotNull] string databaseName, [NotNull] RedisOptions redisOptions) : base(loggerFactory, redisOptions)
        {
            this._databaseName = databaseName;
            this._connectionString = connectionString;
        }

        /// <summary>
        /// Redis database name
        /// </summary>
        [NotNull]
        public override string DatabaseName => this._databaseName;

        /// <summary>
        /// Redis connection string
        /// </summary>
        [NotNull]
        public override string ConnectionString => this._connectionString;
    }
}
