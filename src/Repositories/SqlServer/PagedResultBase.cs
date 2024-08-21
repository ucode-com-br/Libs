using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UCode.Repositories.SqlServer
{
    public abstract class PagedResultBase<T> : IPagedResult<T>//, IReadOnlyList<T>
    {
        protected PagedResultBase(IQueryable<T> queryable = null, int currentPage = 1, int pageSize = int.MaxValue)
        {
            this._queryable = queryable;
            this.CurrentPage = currentPage;
            this.PageSize = pageSize;
        }


        protected virtual IReadOnlyList<T> RunQuery(IQueryable<T> queryable)
        {
            var result = queryable.Skip((this.CurrentPage - 1) * this.PageSize).Take(this.PageSize).ToArray();

            return result;
        }

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
