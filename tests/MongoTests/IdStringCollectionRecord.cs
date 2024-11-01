using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCode.MongoTests
{
    public record IdStringCollectionRecord: Mongo.IObjectId
    {
        public string Id
        {
            get;
            set;
        }

        public string MyProperty1
        {
            get;
            set;
        }
        public int MyProperty2
        {
            get;
            set;
        }
        public byte MyProperty3
        {
            get;
            set;
        }

        public List<string> MyProperty4
        {
            get;
            set;
        } = new List<string>();
    }
}
