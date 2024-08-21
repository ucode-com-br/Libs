using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCode.Extensions;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents an implementation of <see cref="MongoContext"/>.
    /// </summary>
    internal struct MongoContextImplementation
    {
        /// <summary>
        /// The full name of the context.
        /// </summary>
        public string FullName
        {
            get;
        }

        /// <summary>
        /// The hash of the connection string.
        /// </summary>
        public string ConnectionStringHash
        {
            get;
        }

        /// <summary>
        /// The name of the database.
        /// </summary>
        public string DatabaseName
        {
            get;
        }

        /// <summary>
        /// The type of the context.
        /// </summary>
        public Type Type
        {
            get;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MongoContextImplementation"/> struct.
        /// </summary>
        /// <param name="fullName"></param>
        /// <param name="type"></param>
        /// <param name="connectionStringHash"></param>
        /// <param name="databaseName"></param>
        internal MongoContextImplementation(string fullName, Type type, string connectionStringHash, string databaseName)
        {
            FullName = fullName;
            ConnectionStringHash = connectionStringHash;
            DatabaseName = databaseName;
            Type = type;
        }

        /// <summary>
        /// Hashcode from ToString
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => FullName.GetHashCode() ^ ConnectionStringHash.GetHashCode() ^ DatabaseName.GetHashCode() ^ Type.GetHashCode();

        /// <summary>
        /// Determines whether the specified object is equal to the current object. 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is MongoContextImplementation other)
            {
                return FullName == other.FullName
                    && ConnectionStringHash == other.ConnectionStringHash
                    && DatabaseName == other.DatabaseName
                    && Type == other.Type;
            }
            return false;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{FullName}-{ConnectionStringHash}-{DatabaseName}";
    }

}
