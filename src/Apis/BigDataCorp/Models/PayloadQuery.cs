using System.Collections.Generic;
using System.Reflection.Metadata;

namespace UCode.Apis.BigDataCorp.Models
{
    public class PayloadQuery
    {
        private readonly List<string> _queries;


        private PayloadQuery(string query) : this(new List<string>(), query)
        {

        }

        private PayloadQuery(List<string> parentQueries, string query)
        {
            _queries = parentQueries;

            _queries.Add(query);
        }



        public static PayloadQuery Doc(string document) => new PayloadQuery($"doc{{{document}}}");

    }
}
