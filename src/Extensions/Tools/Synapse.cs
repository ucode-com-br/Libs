using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCode.Extensions.Tools
{
    public static class Synapse
    {
        public struct CosmosDbConfigView
        {
            public CosmosDbConfigView(string accountName, string database, string collection, string region, string secretKey)
            {
                this.AccountName = accountName;
                this.Database = database;
                this.Collection = collection;
                this.Region = region;
                this.SecretKey = secretKey;
            }

            /// <summary>
            /// CosmosDb name, only [name] like "[name].xxxx.cosmos.azure.com"
            /// </summary>
            public string AccountName
            {
                get; private set;
            }

            /// <summary>
            /// CosmosDb database name
            /// </summary>
            public string Database
            {
                get; private set;
            }

            /// <summary>
            /// CosmosDb collection name
            /// </summary>
            public string Collection
            {
                get; private set;
            }

            /// <summary>
            /// CosmosDb region widout spaces and lowercaase, like "brazilsouth"<
            /// </summary>
            public string Region
            {
                get; private set;
            }

            /// <summary>
            /// CosmosDb secret key, like "iOezRrwAu3r1MuYxBLZruJiPdsK3Vqo82BsTh0ZjJXNA7WeDerJPzYZoUYIis0d3Q102c0CB3RLDACDbul5apw==".
            /// </summary>
            public string SecretKey
            {
                get; private set;
            }
        }

        //https://github.com/MicrosoftDocs/azure-docs/blob/main/articles/cosmos-db/analytical-store-introduction.md


        /*

        Original data type	Suffix	Example
        Double	".float64"	24.99
        Array	".array"	["a", "b"]
        Binary	".binary"	0
        Boolean	".bool"	True
        Int32	".int32"	123
        Int64	".int64"	255486129307
        NULL	".NULL"	NULL
        String	".string"	"ABC"
        Timestamp	".timestamp"	Timestamp(0, 0)
        ObjectId	".objectId"	ObjectId("5f3f7b59330ec25c132623a2")
        Document	".object"	{"a": "a"}
         
         */


        /// <summary>
        /// Generate sql to create cosmosdb analytical view in synapse
        /// </summary>
        /// <param name="cosmosDb">CosmosDb required configurations</param>
        /// <param name="sqlViewName">Name for create view in azure synapse sql</param>
        /// <param name="instance">instance of object, if null create instance with parameter less constructor</param>
        /// <param name="notFoundItem">Each property or field not found call this delegate</param>
        /// <returns>SQL query</returns>
        public static string CosmosDbAnalyticalViewCreateSql<T>([NotNull] this CosmosDbConfigView cosmosDb, [NotNull] string sqlViewName, T? instance = default, Func<ReflectionExtensions.MemberInfoAction<T>, (string Suffix, string SqlType)?>? notFoundItem = default)
        {
            var cosmosdbConnection = $"account={cosmosDb.AccountName};database={cosmosDb.Database};region={cosmosDb.Region};key={cosmosDb.SecretKey}";

            List<string> lines = new List<string>();

            instance ??= (T?)Activator.CreateInstance(typeof(T));

            (string Suffix, string SqlType, Type[] Type)[] suffixTypes = [
                (".float64", "decimal(18, 0)", new Type[] { typeof(decimal), typeof(float) }),
                (".array", "varchar(max)", new Type[] {
                    typeof(string[]),
                    typeof(short[]),
                    typeof(int[]),
                    typeof(long[]),
                    typeof(float[]),
                    typeof(decimal[]),
                    typeof(object[]),

                    typeof(string?[]),
                    typeof(short?[]),
                    typeof(int?[]),
                    typeof(long?[]),
                    typeof(float?[]),
                    typeof(decimal?[]),
                    typeof(object?[]),

                    typeof(List<string>),
                    typeof(List<short>),
                    typeof(List<int>),
                    typeof(List<long>),
                    typeof(List<float>),
                    typeof(List<decimal>),
                    typeof(List<object>),

                    typeof(List<string?>),
                    typeof(List<short?>),
                    typeof(List<int?>),
                    typeof(List<long?>),
                    typeof(List<float?>),
                    typeof(List<decimal?>),
                    typeof(List<object?>),

                    typeof(List<string?>),
                    typeof(List<short?>),
                    typeof(List<int?>),
                    typeof(List<long?>),
                    typeof(List<float?>),
                    typeof(List<decimal?>),
                    typeof(List<object?>),
                }),
                (".array", "char(8000)", new Type[] { typeof(char[]), typeof(char?[]) }),
                (".binary", "varbinary(max)", new Type[] { typeof(byte[]), typeof(byte?[]) }),
                (".bool", "bit", new Type[] { typeof(bool) }),
                (".int32", "int", new Type[] { typeof(int), typeof(short) }),
                (".int64", "bigint", new Type[] { typeof(long) }),
                //(".NULL", "", new Type[] { typeof(string) }),
                (".string", "varchar(max)", new Type[] { typeof(string) }),
                //(".timestamp", "varchar(max)", new Type[] { typeof(string) }),
                //(".objectId", "varbinary(max)", new Type[] { typeof(ObjectId) }),
                (".object", "varchar(max)", new Type[] {
                    typeof(object),
                    typeof(Dictionary<string, string>),
                    typeof(Dictionary<string, short>),
                    typeof(Dictionary<string, int>),
                    typeof(Dictionary<string, long>),
                    typeof(Dictionary<string, float>),
                    typeof(Dictionary<string, decimal>),
                    typeof(Dictionary<string, object>),

                    typeof(Dictionary<string, string?>),
                    typeof(Dictionary<string, short?>),
                    typeof(Dictionary<string, int?>),
                    typeof(Dictionary<string, long?>),
                    typeof(Dictionary<string, float?>),
                    typeof(Dictionary<string, decimal?>),
                    typeof(Dictionary<string, object?>),

                    typeof(Dictionary<string, string[]>),
                    typeof(Dictionary<string, short[]>),
                    typeof(Dictionary<string, int[]>),
                    typeof(Dictionary<string, long[]>),
                    typeof(Dictionary<string, float[]>),
                    typeof(Dictionary<string, decimal[]>),
                    typeof(Dictionary<string, object[]>),

                    typeof(Dictionary<string, string?[]>),
                    typeof(Dictionary<string, short?[]>),
                    typeof(Dictionary<string, int?[]>),
                    typeof(Dictionary<string, long?[]>),
                    typeof(Dictionary<string, float?[]>),
                    typeof(Dictionary<string, decimal?[]>),
                    typeof(Dictionary<string, object?[]>),

                    typeof(Dictionary<string, string?[]?>),
                    typeof(Dictionary<string, short?[]?>),
                    typeof(Dictionary<string, int?[]?>),
                    typeof(Dictionary<string, long?[]?>),
                    typeof(Dictionary<string, float?[]?>),
                    typeof(Dictionary<string, decimal?[]?>),
                    typeof(Dictionary<string, object?[]?>),

                    typeof(Dictionary<string, List<string>>),
                    typeof(Dictionary<string, List<short>>),
                    typeof(Dictionary<string, List<int>>),
                    typeof(Dictionary<string, List<long>>),
                    typeof(Dictionary<string, List<float>>),
                    typeof(Dictionary<string, List<decimal>>),
                    typeof(Dictionary<string, List<object>>),

                    typeof(Dictionary<string, List<string?>>),
                    typeof(Dictionary<string, List<short?>>),
                    typeof(Dictionary<string, List<int?>>),
                    typeof(Dictionary<string, List<long?>>),
                    typeof(Dictionary<string, List<float?>>),
                    typeof(Dictionary<string, List<decimal?>>),
                    typeof(Dictionary<string, List<object?>>),

                    typeof(Dictionary<string, List<string?>?>),
                    typeof(Dictionary<string, List<short?>?>),
                    typeof(Dictionary<string, List<int?>?>),
                    typeof(Dictionary<string, List<long?>?>),
                    typeof(Dictionary<string, List<float?>?>),
                    typeof(Dictionary<string, List<decimal?>?>),
                    typeof(Dictionary<string, List<object?>?>),
                }),
            ];

            int countSucceed = 0;
            int countIgnored = 0;

            //&& (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
            instance.PublicMemberInfoAction((memberInfoAction) =>
            {
                if (memberInfoAction.CanGetValue && memberInfoAction.CanSetValue)
                {
                    var returnType = memberInfoAction.ReturnType!;

                    if (suffixTypes.FirstOrDefault(f => f.Type.Any(a => returnType == a || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == a))) is (string Suffix, string SqlType, Type[] Type) selected && selected != default)
                    {
                        lines.Add($"[{memberInfoAction.MemberInfo.Name}] {selected.SqlType} '$.{memberInfoAction.MemberInfo.Name}{(selected.Suffix.StartsWith('.') ? selected.Suffix : "." + selected.Suffix)}'");
                        countSucceed++;
                    }
                    else if (notFoundItem != null && notFoundItem.Invoke(memberInfoAction!) is (string, string) selected2 && selected2 != default)
                    {
                        lines.Add($"[{memberInfoAction.MemberInfo.Name}] {selected2.SqlType} '$.{memberInfoAction.MemberInfo.Name}{(selected2.Suffix.StartsWith('.') ? selected2.Suffix : "." + selected2.Suffix)}'");
                        countSucceed++;
                    }
                    else
                    {
                        countIgnored++;
                    }
                }
            });


            string sql = $"CREATE VIEW {sqlViewName} AS SELECT * FROM OPENROWSET ( 'CosmosDB', N'{cosmosdbConnection}', {cosmosDb.Collection}) WITH ({string.Join(", \n", lines)}) AS q1;";

            return sql;
        }

        

    }
}
