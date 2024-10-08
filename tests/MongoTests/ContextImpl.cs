using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using UCode.Mongo;

namespace UCode.MongoTests
{
    public class ContextImpl : UCode.Mongo.ContextBase
    {
        public ContextImpl([NotNull] ILoggerFactory loggerFactory, [NotNull] string connectionString, string? applicationName = null, bool forceTransaction = true) : base(loggerFactory, connectionString, applicationName, forceTransaction)
        {
        }

        public DbSet<IdStringCollectionRecord> IdStringCollection => GetDbSet<IdStringCollectionRecord>(nameof(IdStringCollectionRecord));


        public override async Task IndexAsync()
        {
            var index = new Dictionary<IndexKeysDefinition<IdStringCollectionRecord>, CreateIndexOptions>();

            index.Add(new IndexKeysDefinitionBuilder<IdStringCollectionRecord>().Ascending(x => x.MyProperty1),
                    new CreateIndexOptions()
                    {
                        Background = true,
                        Unique = false,
                        Name = "IDX_MYPROPERTY1"
                    });
            index.Add(new IndexKeysDefinitionBuilder<IdStringCollectionRecord>().Ascending(x => x.MyProperty2),
                    new CreateIndexOptions()
                    {
                        Background = true,
                        Unique = false,
                        Name = "IDX_MYPROPERTY2"
                    });
            index.Add(new IndexKeysDefinitionBuilder<IdStringCollectionRecord>().Ascending(x => x.MyProperty3),
                    new CreateIndexOptions()
                    {
                        Background = true,
                        Unique = false,
                        Name = "IDX_MYPROPERTY3"
                    });

            await IdStringCollection.IndexAsync(index);
        }


        public override async Task MapAsync()
        {
            _ = BsonClassMap.TryRegisterClassMap<IdStringCollectionRecord>(cm => {
                cm.AutoMap();

                cm.MapIdMember(c => c.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
            });
        }
    }
}
