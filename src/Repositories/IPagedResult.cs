using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UCode.Repositories
{
    public interface IPagedResult<T>
    {
        public delegate void ItemEventHandler(object sender, ItemEventArgs<T> args);

        /// <summary>
        /// Evento dos itens adicionados (parallel async)
        /// </summary>
        public event ItemEventHandler ItemEvent;


        public async ValueTask<IPagedResult<TOut>> ConvertAsync<TOut>(Func<T, TOut>? convertFunc = null, bool parallel = true) => await ValueTask.FromResult(this.Convert(convertFunc, parallel));

        public IPagedResult<TOut> Convert<TOut>(Func<T, TOut>? convertFunc = null, bool parallel = true)
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
        /// Resultados
        /// </summary>
        public IReadOnlyList<T> Results
        {
            get; set;
        }

        /// <summary>
        /// Página atual
        /// </summary>
        public int CurrentPage
        {
            get; set;
        }

        /// <summary>
        /// Total de páginas
        /// </summary>
        public int PageCount
        {
            get; set;
        }


        /// <summary>
        /// Tamanho da paginação
        /// </summary>
        public int PageSize
        {
            get; set;
        }


        /// <summary>
        /// Numero total de itens encontrados
        /// </summary>
        public int RowCount
        {
            get; set;
        }

        #endregion
    }
}
