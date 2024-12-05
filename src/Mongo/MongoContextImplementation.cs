using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCode.Extensions;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents an implementation of a MongoDB context with relevant properties for database connection.
    /// </summary>
    internal readonly struct MongoContextImplementation
    {
        /// <summary>
        /// Represents the full name of an individual. This property is read-only.
        /// </summary>
        /// <value>
        /// A string that contains the full name.
        /// </value>
        public string FullName
        {
            get;
        }

        /// <summary>
        /// Represents the hash of the connection string.
        /// </summary>
        /// <value>
        /// A string that holds the hashed version of the connection string.
        /// </value>
        /// <remarks>
        /// This property is read-only and is typically used for validation and security 
        /// purposes to ensure that the connection string has not been tampered with.
        /// </remarks>
        public string ConnectionStringHash
        {
            get;
        }

        /// <summary>
        /// Represents the name of the database.
        /// </summary>
        /// <remarks>
        /// This property is read-only and is intended to provide access to the database name stored within the object.
        /// </remarks>
        /// <value>
        /// A string representing the name of the database.
        /// </value>
        public string DatabaseName
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> of the current instance.
        /// </summary>
        /// <value>
        /// The <see cref="Type"/> representing the type of the current instance.
        /// </value>
        public Type Type
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoContextImplementation"/> class.
        /// </summary>
        /// <param name="fullName">The full name associated with the Mongo context.</param>
        /// <param name="type">The type of the Mongo context.</param>
        /// <param name="connectionStringHash">The hash of the connection string used for database connection.</param>
        /// <param name="databaseName">The name of the database.</param>
        internal MongoContextImplementation(string fullName, Type type, string connectionStringHash, string databaseName)
        {
            this.FullName = fullName;
            this.ConnectionStringHash = connectionStringHash;
            this.DatabaseName = databaseName;
            this.Type = type;
        }

        /// <summary>
        /// Overrides the default hash code function to provide a custom hash code for instances of this class.
        /// The hash code is computed by combining the hash codes of several fields: 
        /// <see cref="FullName"/>, <see cref="ConnectionStringHash"/>, 
        /// <see cref="DatabaseName"/>, and <see cref="Type"/>.
        /// </summary>
        /// <returns>
        /// An integer that serves as the hash code for the current instance,
        /// representing a unique value based on the specified fields.
        /// </returns>
        public override readonly int GetHashCode() => this.FullName.GetHashCode() ^ this.ConnectionStringHash.GetHashCode() ^ this.DatabaseName.GetHashCode() ^ this.Type.GetHashCode();

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// true if the specified object is equal to the current object; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is MongoContextImplementation other)
            {
                return this.FullName == other.FullName
                    && this.ConnectionStringHash == other.ConnectionStringHash
                    && this.DatabaseName == other.DatabaseName
                    && this.Type == other.Type;
            }
            return false;
        }

        /// <summary>
        /// Returns a string representation of the current object, which includes the 
        /// FullName, ConnectionStringHash, and DatabaseName properties, formatted 
        /// in a specific manner.
        /// </summary>
        /// <returns>
        /// A string that contains the FullName, ConnectionStringHash, and DatabaseName 
        /// of the current object, joined by a hyphen ('-').
        /// </returns>
        public override string ToString() => $"{this.FullName}-{this.ConnectionStringHash}-{this.DatabaseName}";
    }

}
