using MongoDB.Driver;

namespace UCode.Mongo.Options
{
    public class QueryBulkWriteOptions<TDocument>
    {
        private readonly BulkWriteOptions _options;

        public QueryBulkWriteOptions()
        {
            this._options = new BulkWriteOptions();
        }

        public QueryBulkWriteOptions(BulkWriteOptions options)
        {
            this._options = options;
        }

        public SortDefinition<TDocument> Sort
        {
            get;
            set;
        }

        public bool IsUpsert
        {
            get;
            set;
        }

        public void Populate(ref ReplaceOneModel<TDocument> replaceOneModel)
        {
            replaceOneModel.IsUpsert = this.IsUpsert;
            replaceOneModel.Sort = this.Sort;
        }






        public static implicit operator BulkWriteOptions(QueryBulkWriteOptions<TDocument> source) => source._options;

        public static implicit operator QueryBulkWriteOptions<TDocument>(BulkWriteOptions source) => new QueryBulkWriteOptions<TDocument>(source);




    }
}
