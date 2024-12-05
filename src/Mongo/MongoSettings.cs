using MongoDB.Driver;
using UCode.Extensions;


namespace UCode.Mongo
{
    /// <summary>
    /// This class encapsulates the settings required for connecting to a MongoDB database.
    /// It includes properties for the connection string and database name, and provides methods to create client settings.
    /// </summary>
    public record MongoSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoSettings"/> class with a specified connection string.
        /// Extracts the database name from the MongoDB connection string using a regular expression.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the MongoDB database.</param>
        /// <returns>
        /// This constructor does not return a value. It sets the <see cref="ConnectionString"/> and <see cref="Database"/> properties based on the provided connection string.
        /// </returns>
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
        /// <param name="connectionString">The connection string to the MongoDB database.</param>
        /// <param name="database">The name of the database to use. If not provided, defaults to null.</param>
        /// <returns>
        /// Returns a new instance of the <see cref="MongoSettings"/> class with the specified connection string and database name.
        /// </returns>
        public MongoSettings(string connectionString, string database = null)
        {
            this.ConnectionString = connectionString;
            this.Database = database;
        }

        /// <summary>
        /// Gets the connection string used to connect to the database.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the database connection string.
        /// </value>
        public string ConnectionString
        {
            get;
        }

        /// <summary>
        /// Represents the name of the database.
        /// </summary>
        /// <value>
        /// A string that contains the name of the database. This property is read-only.
        /// </value>
        public string Database
        {
            get;
        }

        /// <summary>
        /// Creates a new instance of <see cref="MongoClientSettings"/> using the specified connection string.
        /// </summary>
        /// <returns>
        /// A <see cref="MongoClientSettings"/> object configured with the provided connection string.
        /// </returns>
        /// <remarks>
        /// This method calls <see cref="MongoClientSettings.FromConnectionString(string)"/>
        /// to parse the connection string and generate the settings.
        /// </remarks>
        public MongoClientSettings CreateClientSettings() => MongoClientSettings.FromConnectionString(this.ConnectionString);


        /// <summary>
        /// Performs an explicit conversion from <see cref="MongoSettings"/> to <see cref="MongoClientSettings"/>.
        /// </summary>
        /// <param name="mongoSettings">The <see cref="MongoSettings"/> instance to convert.</param>
        /// <returns>
        /// A <see cref="MongoClientSettings"/> that corresponds to the provided <see cref="MongoSettings"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="mongoSettings"/> is null or invalid for conversion.
        /// </exception>
        public static explicit operator MongoClientSettings(MongoSettings mongoSettings) => mongoSettings.CreateClientSettings();
    }

}
