using System;
using System.Linq;
using System.Threading.Tasks;

namespace UCode.Repositories.SqlServer
{
    /// <summary>
    /// Represents a paged result containing a collection of items along with pagination information.
    /// </summary>
    /// <typeparam name="T">The type of items contained in the paged result.</typeparam>
    public sealed class PagedResult<T> : PagedResultBase<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResult"/> class.
        /// </summary>
        public PagedResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResult{T}"/> class.
        /// </summary>
        /// <param name="queryable">An <see cref="IQueryable{T}"/> representing the data source to be paged.</param>
        /// <returns>
        /// This constructor does not return a value.
        /// </returns>
        public PagedResult(IQueryable<T> queryable) : base(queryable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResult{T}"/> class.
        /// </summary>
        /// <param name="queryable">The queryable data source to paginate.</param>
        /// <param name="currentPage">The current page number to display.</param>
        /// <param name="pageSize">The number of items to display on each page.</param>
        /// <returns>
        /// A <see cref="PagedResult{T}"/> containing the paginated results.
        /// </returns>
        public PagedResult(IQueryable<T> queryable, int currentPage, int pageSize) : base(queryable, currentPage,
            pageSize)
        {
        }


        /// <summary>
        /// Converts the results to a specified output type using an optional conversion function.
        /// </summary>
        /// <typeparam name="TOut">The type of the output result. Must be a class.</typeparam>
        /// <param name="convertFunc">An optional function to convert each item in the result set. If not provided, 
        /// a default serialization method is used.</param>
        /// <returns>A <see cref="PagedResult{TOut}"/> containing the converted results and pagination information.</returns>
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
