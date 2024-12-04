using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace UCode.Repositories
{
    /// <summary>
    /// Represents a paginated result set containing a collection of items of type <typeparamref name="T"/>.
    /// Implements the <see cref="IPagedResult{T}"/> interface to provide details about the pagination.
    /// </summary>
    /// <typeparam name="T">The type of the items in the paginated result set.</typeparam>
    public class PagedResult<T> : IPagedResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResult"/> class.
        /// This constructor is marked with the <see cref="JsonConstructorAttribute"/> 
        /// to enable deserialization from JSON. It is intended to be 
        [JsonConstructor]
        private PagedResult()
        {

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResult{T}"/> class.
        /// This constructor takes an enumerable of results, the current page number, 
        /// the size of each page, and the total row count.
        /// </summary>
        /// <param name="results">An <see cref="IEnumerable{T}"/> containing the results for the current page.</param>
        /// <param name="currentPage">The current page number (starting from 1).</param>
        /// <param name="pageSize">The number of items displayed per page.</param>
        /// <param name="rowCount">The total number of available items across all pages.</param>
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

        /// <summary>
        /// Invokes the ItemEvent for each item in the provided IReadOnlyList of items in parallel.
        /// </summary>
        /// <param name="itens">The list of items to process.</param>
        /// <remarks>
        /// This method is designed to allow concurrent processing of items from the list.
        /// It uses the Parallel.ForEach method to iterate over each item and invoke a delegate
        /// associated with the ItemEvent, passing in the current item and its position as an argument.
        /// </remarks>
        /// <typeparam name="T">The type of items in the IReadOnlyList.</typeparam>
        protected virtual void OnItem(IReadOnlyList<T> itens) => Parallel.ForEach(itens, 
                        (item, _, position) => ItemEvent?.Invoke(this, new ItemEventArgs<T>(item, System.Convert.ToInt32(position))));
        


        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the collection of results.
        /// This property is backed by a 
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

        /// <summary>
        /// Gets or sets the current page number in a paginated data set.
        /// </summary>
        /// <value>
        /// The current page number, represented as an integer.
        /// </value>
        /// <remarks>
        /// This property is annotated with <c>[JsonPropertyName("currentPage")]</c> to
        /// specify the name of the property when serialized to JSON.
        /// </remarks>
        [JsonPropertyName("currentPage")]
        public int CurrentPage
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the total number of pages based on the RowCount and PageSize.
        /// The property is computed on-demand and is cached for performance.
        /// </summary>
        /// <value>
        /// An integer representing the total number of pages. 
        /// The value is calculated by dividing the total number of rows (RowCount) by the number of rows per page (PageSize).
        /// If the PageCount has not been calculated yet, it will compute and cache the value.
        /// </value>
        /// <remarks>
        /// This property is marked with the [JsonIgnore] attribute, indicating that it should not be serialized 
        /// when converting the object to JSON format.
        /// </remarks>
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

        /// <summary>
        /// Represents the size of a page in a paginated data set.
        /// </summary>
        /// <remarks>
        /// This property is decorated with the <see cref="JsonPropertyName"/> attribute to specify
        /// the JSON property name when serialized or deserialized.
        /// </remarks>
        /// <example>
        /// An example usage of this property:
        /// <code>
        /// var pagination = new Pagination { PageSize = 20 };
        /// </code>
        /// In this example, the PageSize is set to 20.
        /// </example>
        /// <returns>
        /// The number of items per page indicated by this property.
        /// </returns>
        [JsonPropertyName("pageSize")]
        public int PageSize
        {
            get; set;
        }

        /// <summary>
        /// Represents the count of rows in a dataset or collection.
        /// </summary>
        /// <remarks>
        /// This property is serialized to JSON with the name "rowCount".
        /// </remarks>
        /// <returns>
        /// An integer value representing the number of rows.
        /// </returns>
        [JsonPropertyName("rowCount")]
        public int RowCount
        {
            get; set;
        }

        public T this[int index] => this._results[index];

        #endregion

        /// <summary>
        /// Returns an enumerator that iterates through the collection of results.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        /// <typeparam name="T">
        /// The type of elements in the collection.
        /// </typeparam>
        public IEnumerator<T> GetEnumerator() => this._results.GetEnumerator();


        /// <summary>
        /// Asynchronously converts the current object into a paged result of a different type.
        /// </summary>
        /// <typeparam name="TOut">The type of the output result after conversion.</typeparam>
        /// <param name="convertFunc">An optional function that defines how to convert from the current type to the output type.</param>
        /// <param name="parallel">A flag indicating whether the conversion should be performed in parallel.</param>
        /// <returns>A <see cref="ValueTask{PagedResult{TOut}}"/> representing the asynchronous operation, containing the converted paged result.</returns>
        public async ValueTask<PagedResult<TOut>> ConvertAsync<TOut>(Func<T, TOut>? convertFunc, bool parallel) =>
            await ValueTask.FromResult(this.Convert(convertFunc, parallel));

        /// <summary>
        /// Converts the current instance into a <see cref="PagedResult{TOut}"/>.
        /// This method is a generic implementation that allows conversion to a specified type.
        /// </summary>
        /// <typeparam name="TOut">The type to which the current instance should be converted.</typeparam>
        /// <returns>A <see cref="PagedResult{TOut}"/> representing the converted data.</returns>
        /// <remarks>
        /// This method 
        public PagedResult<TOut> Convert<TOut>() => Convert<TOut>(false);

        /// <summary>
        /// Converts the current object of type T to type TOut using various methods.
        /// The conversion can optionally be done in parallel.
        /// </summary>
        /// <param name="parallel">Indicates whether the conversion should be done in parallel.</param>
        /// <returns>
        /// A <see cref="PagedResult{TOut}"/> containing the converted objects of type TOut.
        /// </returns>
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

        /// <summary>
        /// Converts a collection of results from one type to another using a provided conversion function.
        /// The conversion can be performed in parallel based on the specified parameter.
        /// </summary>
        /// <typeparam name="TOut">The type to which each result will be converted.</typeparam>
        /// <param name="convertFunc">A function to convert each result from type T to type TOut. This can be null, in which case a default serialization/deserialization process will be used.</param>
        /// <param name="parallel">A boolean indicating whether the conversion should be performed in parallel.</param>
        /// <returns>
        /// A <see cref="PagedResult{TOut}"/> containing the converted results, the current page, page size, and total row count.
        /// If the conversion function is null, results are serialized and deserialized instead.
        /// </returns>
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
