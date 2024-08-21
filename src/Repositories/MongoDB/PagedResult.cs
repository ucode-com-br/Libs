using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UCode.Repositories.MongoDB
{
    public class PagedResult<T> : IPagedResult<T>//, IReadOnlyList<T>
    {
        public PagedResult(IEnumerable<T> results, int currentPage, int pageSize, int rowCount)
        {
            //System.Convert.ChangeType(results, typeof(IReadOnlyList<T>))
            if (results is List<T> list)
            {
                this._results = list.AsReadOnly();
            }
            else if (results != default)
            {
                this._results = new List<T>(results).AsReadOnly();
            }
            else
            {
                this._results = new List<T>().AsReadOnly();
            }

            this.CurrentPage = currentPage;
            this.PageSize = pageSize;
            this.RowCount = rowCount;
        }


        #region private
        private IReadOnlyList<T> _results;
        private int _pageCount = -1;

        #endregion

        #region Event

        /// <summary>
        /// event to be called when an item is added to the result set
        /// </summary>
        public event IPagedResult<T>.ItemEventHandler ItemEvent;

        /// <summary>
        /// To call event when an item is added to the result set
        /// </summary>
        /// <param name="itens"></param>
        protected virtual void OnItem(IReadOnlyList<T> itens) => Parallel.ForEach(itens,
                (item, _, position) => ItemEvent?.Invoke(this, new ItemEventArgs<T>(item, Convert.ToInt32(position))));

        #endregion

        #region Public Properties

        /// <summary>
        /// Itens in the result set
        /// </summary>
        public IReadOnlyList<T> Results
        {
            get => this._results;
            set
            {
                this._results = value;

                this.OnItem(this._results);
            }
        }

        /// <summary>
        /// Current page in the result set
        /// </summary>
        public int CurrentPage
        {
            get; set;
        }

        /// <summary>
        /// Maszimum of pages in the result set
        /// </summary>
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
        /// Total of itens per page
        /// </summary>
        public int PageSize
        {
            get; set;
        }

        /// <summary>
        /// Total of itens in the reesult set
        /// </summary>
        public int RowCount
        {
            get; set;
        }


        /// <summary>
        /// Itnes in the result set
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index] => this._results[index];

        #endregion
    }
}
