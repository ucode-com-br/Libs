using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using UCode.Extensions;
using UCode.Mongo.Options;
using UCode.Repositories;

namespace UCode.Mongo
{
    //https://mongodb.github.io/mongo-csharp-driver/2.11/reference/driver/crud/writing/
    /// <summary>
    /// Represents a raw database set.
    /// </summary>
    public class RawDbSet : DbSet<BsonObjectId<BsonObjectId>, BsonObjectId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawDbSet"/> class.
        /// </summary>
        /// <param name="contextBase"></param>
        /// <param name="collectionName"></param>
        /// <param name="options"></param>
        public RawDbSet([NotNull] ContextBase contextBase, string? collectionName = null, TimerSeriesOptions? options = null) : base(contextBase, collectionName, options)
        {

        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string? ToString() => base.ToString();
    }
}
