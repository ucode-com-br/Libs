using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UCode.Repositories.MongoDB
{
    /// <summary>
    /// Represents a paginated result set of a specified type.
    /// </summary>
    /// <typeparam name="T">The type of the items in the result set.</typeparam>
    /// <remarks>
    /// This class implements the <see cref="IPagedResult{T}"/> interface,
    /// providing properties and methods to facilitate pagination
    /// of collections in an application.
    /// </remarks>
    public class PagedResult<T> : IPagedResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResult"/> class.
        /// This constructor is 
        [JsonConstructor]
        private PagedResult()
        {

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResult{T}"/> class with the specified results,
        /// current page, page size, and total row count.
        /// </summary>
        /// <param name="results">An enumerable collection of results to be paged.</param>
        /// <param name="currentPage">The current page number.</param>
        /// <param name="pageSize">The number of items on each page.</param>
        /// <param name="rowCount">The total number of items across all pages.</param>
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

        public event IPagedResult<T>.ItemEventHandler ItemEvent;

        /// <summary>
        /// Invokes the ItemEvent for each item in the provided collection in parallel.
        /// </summary>
        /// <param name="itens">A read-only list of items of type T to be processed.</param>
        /// <remarks>
        /// This method uses <see cref="Parallel.ForEach"/> to iterate over the item list,
        /// invoking the <see cref="ItemEvent"/> for each item with its corresponding position 
        /// in the list, converted to an integer.
        /// This allows efficient parallel processing of items.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="itens"/> parameter is null.</exception>
        /// <example>
        /// <code>
        /// var itemList = new List<MyType> { item1, item2, item3 };
        /// OnItem(itemList.AsReadOnly());
        /// </code>
        /// </example>
        protected virtual void OnItem(IReadOnlyList<T> itens) => Parallel.ForEach(itens,
                        (item, _, position) => ItemEvent?.Invoke(this, new ItemEventArgs<T>(item, Convert.ToInt32(position))));
        
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the collection of results.
        /// </summary>
        /// <value>
        /// A read-only list of items of type <typeparamref name="T"/> 
        /// representing the results.
        /// </value>
        /// <remarks>
        /// The setter updates the 
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
        /// Represents the current page number in a paginated response.
        /// </summary>
        /// <remarks>
        /// This property is annotated with the <see cref="JsonPropertyName"/> attribute, 
        /// allowing for JSON serialization and deserialization with the specified name.
        /// </remarks>
        /// <returns>
        /// An integer value representing the current page number.
        /// </returns>
        [JsonPropertyName("currentPage")]
        public int CurrentPage
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the total number of pages for a collection based on the number of rows and page size.
        /// The value is computed lazily, meaning it is only calculated when needed.
        /// The computed page count is rounded up to the nearest whole number to ensure
        /// that any remaining rows fit within an additional page.
        /// </summary>
        /// <remarks>
        /// The property is marked with <c>[JsonIgnore]</c> attribute to prevent serialization during 
        /// the JSON serialization process, typically to reduce data transfer size when 
        /// the property value is not necessary.
        /// </remarks>
        /// <returns>
        /// An integer representing the total page count. The page count is calculated 
        /// as the ceiling of the division of <c>RowCount</c> by <c>PageSize</c>.
        /// </returns>
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
        /// Represents the size of the page for pagination purposes.
        /// </summary>
        /// <remarks>
        /// This property is serialized/deserialized from JSON using the name "pageSize".
        /// </remarks>
        /// <value>
        /// An integer value that indicates the number of items per page.
        /// </value>
        [JsonPropertyName("pageSize")]
        public int PageSize
        {
            get; set;
        }

        /// <summary>
        /// Represents the count of rows in a dataset.
        /// </summary>
        /// <remarks>
        /// This property is used to indicate the number of rows available, typically in contexts 
        /// related to databases or data tables. The value is serialized/deserialized with the 
        /// JSON property name "rowCount".
        /// </remarks>
        /// <value>
        /// An integer representing the total number of rows. The value can be set and retrieved.
        /// </value>
        [JsonPropertyName("rowCount")]
        public int RowCount
        {
            get; set;
        }


        public T this[int index] => this._results[index];

        #endregion
    }
}
