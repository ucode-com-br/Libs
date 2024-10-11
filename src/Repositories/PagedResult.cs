using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace UCode.Repositories
{
    /// <summary>
    /// Paged result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedResult<T> : IPagedResult<T>
    {
        [JsonConstructor]
        private PagedResult()
        {

        }

        public PagedResult(IEnumerable<T> results, int currentPage, int pageSize, int rowCount)
        {
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

        [JsonPropertyName("results")]
        public IReadOnlyList<T> Results
        {
            get => this._results;
            set
            {
                this._results = value;

                this.OnItem(this._results);
            }
        }

        [JsonPropertyName("currentPage")]
        public int CurrentPage
        {
            get; set;
        }

        [JsonIgnore]
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

        [JsonPropertyName("pageSize")]
        public int PageSize
        {
            get; set;
        }

        [JsonPropertyName("rowCount")]
        public int RowCount
        {
            get; set;
        }

        public T this[int index] => this._results[index];

        #endregion

        public IEnumerator<T> GetEnumerator() => this._results.GetEnumerator();

    }
}
