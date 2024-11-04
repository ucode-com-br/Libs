global using Xunit;
using System.Collections.Specialized;
using System.Linq;
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

            var result1 = await context.IdStringCollection.InsertAsync(new IdStringCollectionRecord() { MyProperty1 = nameof(Test3), MyProperty2 = 1, MyProperty3 = 0x01, MyProperty4 = new System.Collections.Generic.List<string>() { "1", "2", "3" } });
            var result2 = await context.IdStringCollection.InsertAsync(new IdStringCollectionRecord() { MyProperty1 = nameof(Test3), MyProperty2 = 2, MyProperty3 = 0x02, MyProperty4 = new System.Collections.Generic.List<string>() { "4", "5", "6" } });
            var result3 = await context.IdStringCollection.InsertAsync(new IdStringCollectionRecord() { MyProperty1 = nameof(Test3), MyProperty2 = 3, MyProperty3 = 0x03, MyProperty4 = new System.Collections.Generic.List<string>() { "7", "8", "9" } });

            Assert.True(result1 == 1 && result2 == 1 && result3 == 1);
        }

        [Fact]
        public async Task Test4()
        {
            var context = new ContextImpl(_loggerFactory, ConnectionString, nameof(ContextTests), false);

            var query = Query<IdStringCollectionRecord>.FromExpression(d => d.MyProperty4.Any(a => a == "4"));
            var options = new FindOptionsPaging<IdStringCollectionRecord>() { PageSize = 100, CurrentPage = 0 };
            

            var result1 = await context.IdStringCollection.GetPagedAsync<IdStringCollectionRecord>(filter: query, findOptions: options);

        }
    }
}
