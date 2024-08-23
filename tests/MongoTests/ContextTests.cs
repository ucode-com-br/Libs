global using Xunit;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UCode.Mongo;
using UCode.Mongo.Options;

namespace UCode.MongoTests
{
    public class ContextTests
    {
        private readonly IConfigurationRoot _configurationRoot;
        private readonly ILoggerFactory _loggerFactory;

        private string ConnectionString => this._configurationRoot["MongoConnectionString"]!;

        public ContextTests(IConfigurationRoot configurationRoot, ILoggerFactory loggerFactory)
        {
            _configurationRoot = configurationRoot;
            _loggerFactory = loggerFactory;
        }

        [Fact]
        public async Task Test1()
        {
            var context = new ContextImpl(_loggerFactory, ConnectionString, nameof(ContextTests), false);

            var result = await context.IdStringCollection.InsertAsync(new IdStringCollectionRecord() { MyProperty1 = nameof(Test1), MyProperty2 = 1, MyProperty3 = 0x01 });

            Assert.True(result == 1);
        }

        [Fact]
        public async Task Test2()
        {
            var context = new ContextImpl(_loggerFactory, ConnectionString, nameof(ContextTests), false);

            var result = await context.IdStringCollection.InsertAsync(new IdStringCollectionRecord() { MyProperty1 = nameof(Test2), MyProperty2 = 1, MyProperty3 = 0x01 });

            Assert.True(result == 1);
        }

        [Fact]
        public async Task Test3()
        {
            var context = new ContextImpl(_loggerFactory, ConnectionString, nameof(ContextTests), false);

            var result1 = await context.IdStringCollection.InsertAsync(new IdStringCollectionRecord() { MyProperty1 = nameof(Test3), MyProperty2 = 1, MyProperty3 = 0x01 });
            var result2 = await context.IdStringCollection.InsertAsync(new IdStringCollectionRecord() { MyProperty1 = nameof(Test3), MyProperty2 = 2, MyProperty3 = 0x02 });
            var result3 = await context.IdStringCollection.InsertAsync(new IdStringCollectionRecord() { MyProperty1 = nameof(Test3), MyProperty2 = 3, MyProperty3 = 0x03 });

            Assert.True(result1 == 1 && result2 == 1 && result3 == 1);
        }

        [Fact]
        public async Task Test4()
        {
            //var context = new ContextImpl(_loggerFactory, ConnectionString, nameof(ContextTests), false);

            //var query = Query<IdStringCollectionRecord>.FromQuery("");
            //var options = new FindOptionsPaging<IdStringCollectionRecord>();

            //var result1 = await context.IdStringCollection.GetPagedAsync<IdStringCollectionRecord>(filter: query, findOptions: );

            //Assert.True(result1.Any());
        }
    }
}
