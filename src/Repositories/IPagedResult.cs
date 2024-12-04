using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace UCode.Repositories
{
    /// <summary>
    /// Represents a paged result of a collection of entities of type <typeparamref name="T"/>.
    /// This interface provides properties to access the current page, total items, 
    /// total pages, and the collection of items in the current page.
    /// </summary>
    /// <typeparam name="T">The type of items in the paged result.</typeparam>
    public interface IPagedResult<T>
    {
        /// <summary>
        /// Represents a method that will handle events related to items.
        /// </summary>
        /// <param name="sender">
        /// The source of the event, typically the object that raised the event.
        /// </param>
        /// <param name="args">
        /// An instance of <see cref="ItemEventArgs{T}"/> that contains the event data.
        /// </param>
        /// <typeparam name="T">
        /// The type of the item associated with the event.
        /// </typeparam>
        public delegate void ItemEventHandler(object sender, ItemEventArgs<T> args);

        public event ItemEventHandler ItemEvent;


        /// <summary>
        /// Asynchronously converts the current instance to a paged result of a specified type using the provided conversion function.
        /// </summary>
        /// <typeparam name="TOut">The type to which the current instance should be converted.</typeparam>
        /// <param name="convertFunc">A function that defines how to convert an instance of type T to type TOut. This parameter can be null.</param>
        /// <param name="parallel">Specifies whether the conversion should be performed in parallel.</param>
        /// <returns>A <see cref="ValueTask{IPagedResult{TOut}}"/> that represents the asynchronous operation. The value of the task contains the paged result of type TOut.</returns>
        public async ValueTask<IPagedResult<TOut>> ConvertAsync<TOut>(Func<T, TOut>? convertFunc, bool parallel) => await ValueTask.FromResult(this.Convert(convertFunc, parallel));

        /// <summary>
        /// Converts the current object to a different type specified by the generic type parameter.
        /// </summary>
        /// <typeparam name="TOut">The type to which the current object will be converted.</typeparam>
        /// <returns>An instance of <see cref="IPagedResult{TOut}"/> that represents the converted object.</returns>
        /// <remarks>
        /// This method provides a convenient way to convert the current instance into a paged result of a different type 
        /// without specifying pagination options. The default pagination setting is used.
        /// </remarks>
        public IPagedResult<TOut> Convert<TOut>() => Convert<TOut>(false);

        /// <summary>
        /// Converts an object of type T to an object of type TOut.
        /// This method supports parallel processing of conversion if specified.
        /// </summary>
        /// <param name="parallel">A boolean value indicating whether to perform the conversion in parallel.</param>
        /// <returns>An IPagedResult of type TOut containing the results of the conversion.</returns>
        public IPagedResult<TOut> Convert<TOut>(bool parallel)
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
                        result = (TOut?) System.Convert.ChangeType(obj, typeof(TOut));

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
        /// Converts the current results into a different type using the specified conversion function.
        /// </summary>
        /// <typeparam name="TOut">The type to convert the results to.</typeparam>
        /// <param name="convertFunc">A function used to convert each item in the results. Can be null.</param>
        /// <param name="parallel">A boolean indicating whether to execute the conversion in parallel.</param>
        /// <returns>
        /// Returns an <see cref="IPagedResult{TOut}"/> containing the converted results.
        /// If <paramref name="convertFunc"/> is null, the method will deserialize the current results assuming they can be serialized to JSON.
        /// </returns>
        public IPagedResult<TOut> Convert<TOut>(Func<T, TOut>? convertFunc, bool parallel)
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


        #region Public Properties

        /// <summary>
        /// Represents a read-only collection of results of type T.
        /// </summary>
        /// <remarks>
        /// This property provides access to a list of results without allowing modifications 
        /// to the collection itself. The collection can be accessed but not altered.
        /// </remarks>
        /// <typeparam name="T">The type of the elements in the results collection.</typeparam>
        /// <value>
        /// An IReadOnlyList<T> containing the results.
        /// </value>
        public IReadOnlyList<T> Results
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        /// <value>
        /// An integer representing the current page.
        /// </value>
        public int CurrentPage
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the number of pages.
        /// </summary>
        /// <value>
        /// An integer representing the total number of pages.
        /// </value>
        public int PageCount
        {
            get; set;
        }


        /// <summary>
        /// Gets or sets the number of items to display on a single page.
        /// This property is commonly used for pagination in data display scenarios.
        /// </summary>
        /// <value>
        /// An integer that represents the number of items per page. 
        /// The default value is typically set according to the application requirements.
        /// </value>
        public int PageSize
        {
            get; set;
        }


        /// <summary>
        /// Gets or sets the number of rows.
        /// This property represents the total count of rows available.
        /// </summary>
        /// <value>
        /// An integer representing the number of rows.
        /// </value>
        public int RowCount
        {
            get; set;
        }

        #endregion
    }
}
