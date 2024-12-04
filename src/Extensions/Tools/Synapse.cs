using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCode.Extensions.Tools
{
    /// <summary>
    /// Represents a synapse, providing methods and properties to manage and interact with neural connections.
    /// </summary>
    /// <remarks>
    /// This class serves as a static utility for handling operations related to synapses in a neural network simulation.
    /// </remarks>
    public static class Synapse
    {
        /// <summary>
        /// Represents the configuration settings for connecting to a Cosmos DB instance.
        /// </summary>
        /// <remarks>
        /// This struct holds the essential parameters necessary for establishing a connection 
        /// to a Cosmos DB, including account name, database name, collection name, region, 
        /// and the secret key for authentication.
        /// </remarks>
        public struct CosmosDbConfigView
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CosmosDbConfigView"/> class.
            /// </summary>
            /// <param name="accountName">The name of the Cosmos DB account.</param>
            /// <param name="database">The name of the database within the Cosmos DB account.</param>
            /// <param name="collection">The name of the collection within the specified database.</param>
            /// <param name="region">The region where the Cosmos DB account is hosted.</param>
            /// <param name="secretKey">The secret key used for authentication with the Cosmos DB account.</param>
            public CosmosDbConfigView(string accountName, string database, string collection, string region, string secretKey)
            {
                this.AccountName = accountName;
                this.Database = database;
                this.Collection = collection;
                this.Region = region;
                this.SecretKey = secretKey;
            }

            /// <summary>
            /// Gets the name of the account.
            /// </summary>
            /// <value>
            /// A string representing the account name.
            /// This property is read-only outside of the class, as the setter is 
            public string AccountName
            {
                get; private set;
            }

            /// <summary>
            /// Gets the name of the database.
            /// </summary>
            /// <value>
            /// A string representing the name of the database.
            /// </value>
            /// <remarks>
            /// This property is read-only outside of the class it is defined in,
            /// since it has a 
            public string Database
            {
                get; private set;
            }

            /// <summary>
            /// Gets the collection as a string.
            /// The Collection property is read-only outside the class and can only be set 
            public string Collection
            {
                get; private set;
            }

            /// <summary>
            /// Gets the region associated with the object.
            /// </summary>
            /// <value>
            /// A string that represents the region.
            /// This property is read-only outside of the class.
            /// </value>
            public string Region
            {
                get; private set;
            }

            /// <summary>
            /// Gets the secret key associated with the instance of the class.
            /// The secret key is a string that is used for secure operations.
            /// </summary>
            /// <value>
            /// A string representing the secret key. This property can only be set 
            /// 
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
        /// Creates a SQL view in Azure Cosmos DB based on the provided instance and its properties.
        /// This method generates a SQL query to create the view using the member information from the specified instance.
        /// </summary>
        /// <typeparam name="T">The type of the instance used to derive the SQL view structure.</typeparam>
        /// <param name="cosmosDb">The configuration settings required to connect to the Cosmos DB.</param>
        /// <param name="sqlViewName">The name of the SQL view to create.</param>
        /// <param name="instance">An optional instance of type <typeparamref name="T"/> from which the view properties will be derived.</param>
        /// <param name="notFoundItem">An optional function that defines handling for member types that do not match predefined suffix types.</param>
        /// <returns>A string that represents the SQL command to create the specified view in Cosmos DB.</returns>
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
