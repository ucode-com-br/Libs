using MongoDB.Driver;
using UCode.Extensions;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace UCode.Mongo
{
    public record MongoSettings
    {
        public MongoSettings(string connectionString)
        {
            this.ConnectionString = connectionString;
            this.Database =
                "mongodb\\+srv\\:\\/\\/.*\\:.*\\@.*\\/(?<DBNAME>.*)\\?.*".MatchNamedCaptures(connectionString)
                    ["DBNAME"];
        }

        public MongoSettings(string connectionString, string database = null)
        {
            this.ConnectionString = connectionString;
            this.Database = database;
        }

        public string ConnectionString
        {
            get;
        }

        public string Database
        {
            get;
        }

        public MongoClientSettings CreateClientSettings() => MongoClientSettings.FromConnectionString(this.ConnectionString);

        public static explicit operator MongoClientSettings(MongoSettings mongoSettings) => mongoSettings.CreateClientSettings();
    }
}
