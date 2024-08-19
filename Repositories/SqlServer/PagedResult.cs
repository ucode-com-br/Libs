using System;
using System.Linq;
using System.Threading.Tasks;

namespace UCode.Repositories.SqlServer
{
    public sealed class PagedResult<T> : PagedResultBase<T>
    {
        public PagedResult()
        {
        }

        public PagedResult(IQueryable<T> queryable) : base(queryable)
        {
        }

        public PagedResult(IQueryable<T> queryable, int currentPage, int pageSize) : base(queryable, currentPage,
            pageSize)
        {
        }


        public PagedResult<TOut> Convert<TOut>(Func<T, TOut> convertFunc = null) where TOut : class
        {
            var tout = new TOut[this.Results.Count];

            if (convertFunc != null)
            {
                Parallel.ForEach(this.Results,
                    (item, _, pos) => tout[System.Convert.ToInt32(pos)] = convertFunc(item));
            }
            else
            {
                var options = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web) { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };

                var json = System.Text.Json.JsonSerializer.Serialize(this.Results, options);

                tout = System.Text.Json.JsonSerializer.Deserialize<TOut[]>(json, options);
            }

            return new PagedResult<TOut>
            {
                CurrentPage = CurrentPage,
                PageCount = PageCount,
                PageSize = PageSize,
                RowCount = RowCount,
                Results = tout
            };
        }
    }
}
