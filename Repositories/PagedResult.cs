using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UCode.Repositories
{
    public class PagedResult<T> : IPagedResult<T>//, IEnumerable<T>
    {
        public PagedResult(IEnumerable<T> results, int currentPage, int pageSize, int rowCount)
        {
            //System.Convert.ChangeType(results, typeof(IReadOnlyList<T>))
            if (results is List<T> list)
            {
                this._results = list.AsReadOnly();
            }
            else if (results is not default(object?) and IEnumerable<T> enu)
            {
                this._results = new List<T>(enu).AsReadOnly();
            }
            else if (results is not default(object?) and T[] arr)
            {
                this._results = new List<T>(arr).AsReadOnly();
            }
            else if (results is not default(object?) and IList<T> ilist)
            {
                this._results = ilist.AsReadOnly();
            }
            else if (results is not default(object?) and IReadOnlyList<T> rlist)
            {
                this._results = rlist;
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

        public event IPagedResult<T>.ItemEventHandler ItemEvent;

        protected virtual void OnItem(IReadOnlyList<T> itens) => Parallel.ForEach(itens,
                (item, _, position) => ItemEvent?.Invoke(this, new ItemEventArgs<T>(item, Convert.ToInt32(position))));



        #endregion

        #region Public Properties

        public IReadOnlyList<T> Results
        {
            get => this._results;
            set
            {
                this._results = value;

                this.OnItem(this._results);
            }
        }

        public int CurrentPage
        {
            get; set;
        }

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

        public int PageSize
        {
            get; set;
        }

        public int RowCount
        {
            get; set;
        }

        public T this[int index] => this._results[index];

        #endregion

        public IEnumerator<T> GetEnumerator() => this._results.GetEnumerator();

    }
}
