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
                (item, _, position) => ItemEvent?.Invoke(this, new ItemEventArgs<T>(item, System.Convert.ToInt32(position))));



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


        public async ValueTask<PagedResult<TOut>> ConvertAsync<TOut>(Func<T, TOut>? convertFunc, bool parallel) =>
            await ValueTask.FromResult(this.Convert(convertFunc, parallel));

        public PagedResult<TOut> Convert<TOut>() => Convert<TOut>(false);

        public PagedResult<TOut> Convert<TOut>(bool parallel)
        {
            List<TOut>? tout = default;

            Func<T, TOut> func = static (T obj) =>
            {
                if (obj == null)
                {
                    return default;
                }

                TOut? result = default;

                try
                {
                    result = (TOut)(object)obj;
                }
                catch (Exception ex0)
                {
                    try
                    {
                        result = (TOut?)System.Convert.ChangeType(obj, typeof(TOut));

                        if (result == null)
                        {
                            throw new NullReferenceException($"System.Convert.ChangeType result is null.");
                        }
                    }
                    catch (Exception ex1)
                    {
                        try
                        {
                            var options = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web) { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };

                            var json = System.Text.Json.JsonSerializer.Serialize(obj, options);

                            result = System.Text.Json.JsonSerializer.Deserialize<TOut>(json, options);
                        }
                        catch (Exception ex3)
                        {
                            throw new AggregateException(ex0, ex1, ex3);
                        }
                    }
                }

                return result;
            };

            return Convert<TOut>(func, parallel);
        }

        public PagedResult<TOut> Convert<TOut>(Func<T, TOut>? convertFunc, bool parallel)
        {
            List<TOut>? tout = default;

            if (convertFunc != null)
            {
                var toutArray = new TOut[this.Results.Count];

                if (parallel)
                {
                    Parallel.ForEach(this.Results,
                    (item, _, pos) =>
                    {
                        var it = convertFunc(item);

                        if (item != null && it == null)
                        {
                            throw new NullReferenceException($"Convert result is null.");
                        }

                        toutArray[System.Convert.ToInt32(pos)] = it;
                    });


                }
                else
                {
                    for (var i = 0; i < this.Results.Count; i++)
                    {
                        var it = convertFunc(this.Results[i]);

                        if (this.Results[i] != null && it == null)
                        {
                            throw new NullReferenceException($"Convert result is null.");
                        }

                        toutArray[System.Convert.ToInt32(i)] = it;
                    }
                }

                tout = new List<TOut>(toutArray);
            }
            else
            {
                var options = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web) { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };

                var json = System.Text.Json.JsonSerializer.Serialize(this.Results, options);

                tout = System.Text.Json.JsonSerializer.Deserialize<List<TOut>>(json, options);
            }

            return new PagedResult<TOut>(tout ?? new List<TOut>(), this.CurrentPage, this.PageSize, this.RowCount);
        }
    }
}
