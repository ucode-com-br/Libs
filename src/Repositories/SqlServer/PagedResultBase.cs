using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UCode.Repositories.SqlServer
{
    /// <summary>
    /// Represents an abstract base class for paginated results that contain a collection of items of type <typeparamref name="T"/>.
    /// This class implements the <see cref="IPagedResult{T}"/> interface,
    /// enabling it to provide additional functionality for pagination.
    /// </summary>
    /// <typeparam name="T">The type of the items contained in the paginated results.</typeparam>
    public abstract class PagedResultBase<T> : IPagedResult<T>//, IReadOnlyList<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResultBase{T}"/> class.
        /// </summary>
        /// <param name="queryable">An optional <see cref="IQueryable{T}"/> source 
        /// from which to create the paged result. If null, no query is executed.</param>
        /// <param name="currentPage">The current page number. Defaults to 1 if not specified.</param>
        /// <param name="pageSize">The size of the page, which determines the maximum number 
        /// of items returned. Defaults to <see cref="int.MaxValue"/> if not specified.</param>
        protected PagedResultBase(IQueryable<T> queryable = null, int currentPage = 1, int pageSize = int.MaxValue)
        {
            this._queryable = queryable;
            this.CurrentPage = currentPage;
            this.PageSize = pageSize;
        }


        /// <summary>
        /// Executes a query against the provided IQueryable<T> and returns a paginated list of results.
        /// </summary>
        /// <param name="queryable">An IQueryable<T> representing the query to be executed.</param>
        /// <returns>
        /// A read-only list of type T containing the results of the query after applying pagination.
        /// </returns>
        protected virtual IReadOnlyList<T> RunQuery(IQueryable<T> queryable)
        {
            var result = queryable.Skip((this.CurrentPage - 1) * this.PageSize).Take(this.PageSize).ToArray();
            
            return result;
        }

        /// <summary>
        /// Gets the count of results available in the Results collection.
        /// </summary>
        /// <returns>
        /// The number of elements in the Results collection.
        /// </returns>
        public int CountResults() => this.Results.Count;

        #region private

        private readonly IQueryable<T> _queryable;
        private IReadOnlyList<T> _results;
        private int _rowCount = -1;
        private int _pageCount = -1;

        #endregion

        #region Event

        //public delegate void ItemEventHandler(object sender, ItemEventArgs<T> args);
        //public event ItemEventHandler ItemEvent;
        public event IPagedResult<T>.ItemEventHandler ItemEvent;

        /// <summary>
        /// Raises an event for each item in the provided IReadOnlyList.
        /// This method processes the items in parallel, invoking an event for each one.
        /// </summary>
        /// <param name="itens">A read-only list of items to process.</param>
        /// <remarks>
        /// This method is virtual, allowing derived classes to override its behavior.
        /// The event is invoked on a separate thread, which may lead to concurrent modifications.
        /// Ensure that event handlers are thread-safe.
        /// </remarks>
        protected virtual void OnItem(IReadOnlyList<T> itens) => Parallel.ForEach(itens,
                (item, _, position) => ItemEvent?.Invoke(this, new ItemEventArgs<T>(item, Convert.ToInt32(position))));

        //public IEnumerator<T> GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the results of the query as a read-only list.
        /// If the results are not yet initialized, it runs the query and stores the results.
        /// </summary>
        /// <value>
        /// A read-only list of type <typeparamref name="T"/> that contains the results of the query.
        /// </value>
        /// <remarks>
        /// The <c>get</c> accessor initializes the results by running the specified query if they are currently null.
        /// The <c>set</c> accessor allows for updating the results and also invokes the <c>OnItem</c> method with the new results.
        /// </remarks>
        public IReadOnlyList<T> Results
        {
            get
            {
                if (this._results == null)
                {
                    this._results = this.RunQuery(this._queryable);

                    this.OnItem(this._results);
                }

                return this._results;
            }
            set
            {
                this._results = value;

                this.OnItem(this._results);
            }
        }

        /// <summary>
        /// Represents the current page number in a paginated collection or interface.
        /// </summary>
        /// <value>
        /// An integer that gets or sets the current page number. 
        /// The value should be greater than or equal to 1, representing the first page.
        /// </value>
        public int CurrentPage
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the total number of pages based on the row count and page size.
        /// The total number of pages is calculated as the ceiling of the row count divided by the page size.
        /// If the page count is not already calculated (indicated by -1), it will compute the value 
        /// the first time the property is accessed.
        /// </summary>
        /// <value>
        /// An integer representing the total number of pages.
        /// </value>
        /// <remarks>
        /// Setting the value directly will update the page count without recalculation.
        /// </remarks>
        public int PageCount
        {
            get
            {
                if (this._pageCount == -1)
                {
                    var pageCount = (double)this.RowCount / this.PageSize;
                    this._pageCount = (int)Math.Ceiling(pageCount);
                }

                return this._pageCount;
            }
            set => this._pageCount = value;
        }

        /// <summary>
        /// Gets or sets the number of items to be displayed on a single page.
        /// </summary>
        /// <value>
        /// The number of items per page. This value must be a positive integer.
        /// </value>
        public int PageSize
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the count of rows.
        /// This property calculates the total number of rows based on the underlying queryable data.
        /// If the row count has not been set previously (indicated by a value of -1), it will calculate it
        /// by calling the Count() method on the queryable object, or fall back to returning an alternative
        /// count from the CountResults() method if the queryable is null.
        /// </summary>
        /// <returns>
        /// The current number of rows. If the count was not calculated before, it will compute it on demand.
        /// </returns>
        public int RowCount
        {
            get
            {
                if (this._rowCount == -1)
                {
                    if (this._queryable != null)
                    {
                        this._rowCount = this._queryable.Count();
                    }
                    else
                    {
                        return this.CountResults();
                    }
                }

                return this._rowCount;
            }
            set => this._rowCount = value;
        }

        /// <summary>
        /// Gets the count of results stored in the _results collection.
        /// </summary>
        /// <value>
        /// The number of elements in the _results collection.
        /// </value>
        public int Count => this._results.Count;

        public T this[int index] => this._results[index];

        //public int FirstRowOnPage
        //{
        //    get 
        //    { 
        //        return (CurrentPage - 1) * PageSize + 1; 
        //    }
        //}
        //public int LastRowOnPage
        //{
        //    get 
        //    { 
        //        return Math.Min(CurrentPage * PageSize, RowCount); 
        //    }
        //}

        #endregion
    }
}
