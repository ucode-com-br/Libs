using MongoDB.Driver;
using UCode.Extensions;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace UCode.Mongo
{
    /// <summary>
    /// Represents the settings for a MongoDB connection.
    /// </summary>
    public record MongoSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoSettings"/> class.
        /// </summary>
        /// <param name="connectionString">The MongoDB connection string.</param>
        public MongoSettings(string connectionString)
        {
            this.ConnectionString = connectionString;
            this.Database =
                "mongodb\\+srv\\:\\/\\/.*\\:.*\\@.*\\/(?<DBNAME>.*)\\?.*".MatchNamedCaptures(connectionString)
                    ["DBNAME"];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoSettings"/> class.
        /// </summary>
        /// <param name="connectionString">The MongoDB connection string.</param>
        /// <param name="database">The name of the database. If not provided, it will be extracted from the connection string.</param>
        public MongoSettings(string connectionString, string database = null)
        {
            this.ConnectionString = connectionString;
            this.Database = database;
        }

        /// <summary>
        /// Gets the MongoDB connection string.
        /// </summary>
        public string ConnectionString
        {
            get;
        }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        public string Database
        {
            get;
        }

        /// <summary>
        /// Creates a <see cref="MongoClientSettings"/> object from the connection string.
        /// </summary>
        /// <returns>A new <see cref="MongoClientSettings"/> object.</returns>
        public MongoClientSettings CreateClientSettings() => MongoClientSettings.FromConnectionString(this.ConnectionString);

        /// <summary>
        /// Converts a <see cref="MongoSettings"/> object to a <see cref="MongoClientSettings"/> object.
        /// </summary>
        /// <param name="mongoSettings">The <see cref="MongoSettings"/> object to convert.</param>
        /// <returns>A new <see cref="MongoClientSettings"/> object.</returns>
        public static explicit operator MongoClientSettings(MongoSettings mongoSettings) => mongoSettings.CreateClientSettings();
    }
}
