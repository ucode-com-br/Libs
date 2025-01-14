using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UCode.Apis.BigDataCorp.Models
{
    public class Payload<T>
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public readonly DatasetInfo DatasetInfo;

        public enum PayloadSort
        {
            Ascending, Descending
        }

        public class PayloadOrderBy : Dictionary<string, PayloadSort>
        {
            private PayloadDataset _dataset;
            internal PayloadOrderBy(PayloadDataset dataset)
            {
                _dataset = dataset;
            }

            public override string ToString()
            {
                var orderItens = new List<string>();
                foreach (var o in this)
                {
                    orderItens.Add($"{o.Key}={(o.Value == PayloadSort.Ascending ? "ascending" : "descending")}");
                }

                if (orderItens.Count > 0)
                {
                    return $"{_dataset.Name}.order({string.Join(",", orderItens)})";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public class PayloadFilter : Dictionary<string, IEnumerable<object>>
        {
            private PayloadDataset _dataset;
            internal PayloadFilter(PayloadDataset dataset)
            {
                _dataset = dataset;
            }

            public override string ToString()
            {
                var filterItens = new List<string>();
                foreach (var f in this)
                {
                    if (f.Value.Count() == 1)
                    {
                        filterItens.Add($"{f.Key}={f.Value.FirstOrDefault()}");
                    }
                    else if (f.Value.Count() > 1)
                    {
                        if (f.Value.Any(a => a.GetType() == typeof(int) ||
                            a.GetType() == typeof(long) ||
                            a.GetType() == typeof(decimal) ||
                            a.GetType() == typeof(float) ||
                            a.GetType() == typeof(short) ||
                            a.GetType() == typeof(double)))
                        {
                            filterItens.Add($"{f.Key}=[{string.Join(",", f.Value)}]");
                        }
                        else
                        {
                            filterItens.Add($"{f.Key}=[{string.Join(",", $"\"{f.Value}\"")}]");
                        }
                    }
                }
                if (filterItens.Count > 0)
                {
                    return $"{_dataset.Name}.filter({string.Join(",", filterItens)})";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public class PayloadQuery : Dictionary<string, string>
        {
            //"q": "name{Joao da Silva},birthdate{10/02/1995},dateformat{dd/MM/yyyy}",
            private void Set([NotNull] string key, string? value)
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    if (this.ContainsKey(key))
                    {
                        this[key] = value;
                    }
                    else
                    {
                        this.Add(key, value);
                    }
                }
            }

            private string? Get([NotNull] string key)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    return null;
                }

                if (this.ContainsKey(key))
                    return this[key];

                return null;
            }

            /// <summary>
            /// Número de registro em conselho de classe de pessoas
            /// </summary>
            public string? Classnumber
            {
                get => Get("classnumber");
                set => Set("classnumber", value);
            }

            /// <summary>
            /// NIT (Número de Inscrição do Trabalhador)
            /// </summary>
            public string? Nit
            {
                get => Get("nit");
                set => Set("nit", value);
            }

            /// <summary>
            /// Número de Registro na ANTT (pessoas e empresas)
            /// </summary>
            public string? Rntrc
            {
                get => Get("rntrc");
                set => Set("rntrc", value);
            }


            /// <summary>
            /// Nome da pessoa ou empresa
            /// </summary>
            public string? Name
            {
                get => Get("name");
                set => Set("name", value);
            }

            public DateOnly? PersonBirthdate
            {
                get
                {
                    var b = Get("birthdate");

                    if (b != null)
                        return DateOnly.Parse(b);

                    return null;
                }
                set
                {
                    Set("birthdate", value?.ToString("dd/MM/yyyy"));
                    Set("dateformat", "dd/MM/yyyy");
                }
            }

            public DateOnly? ReferenceDate
            {
                get
                {
                    var b = Get("referencedate");

                    if (b != null)
                        return DateOnly.Parse(b);

                    return null;
                }
                set
                {
                    Set("referencedate", value?.ToString("dd/MM/yyyy"));
                    Set("dateformat", "dd/MM/yyyy");
                }
            }

            /// <summary>
            /// CPF ou CNPJ para consultas de pessoas e empresas
            /// </summary>
            public string? Doc
            {
                get => Get("doc");
                set => Set("doc", value);
            }

            /// <summary>
            /// CEP
            /// </summary>
            public string? Zipcode
            {
                get => Get("zipcode");
                set => Set("zipcode", value);
            }

            /// <summary>
            /// Placa de veiculo
            /// </summary>
            public string? VeicleLicenseplate
            {
                get => Get("licenseplate");
                set => Set("licenseplate", value);
            }

            /// <summary>
            /// Número do Processo judicial
            /// </summary>
            public string? ProcessNumber
            {
                get => Get("processnumber");
                set => Set("processnumber", value);
            }

            /// <summary>
            /// Numero da nota fiscal eletronica
            /// </summary>
            public string? Receiptnumber
            {
                get => Get("receiptnumber");
                set => Set("receiptnumber", value);
            }


            /// <summary>
            /// Número do Código de Barras
            /// </summary>
            public string? ean
            {
                get => Get("ean");
                set => Set("ean", value);
            }



            ////"q": "doc{xxxxxxxxxxx},referencedate{2018-09-03},dateformat{yyyy-MM-dd}"
            public override string ToString()
            {
                var result = new List<string>();

                foreach (var item in this)
                {
                    result.Add($"{item.Key}{{{item.Value}}}");
                }

                if (result.Count > 0)
                    return string.Join(",", string.Join(",", result));
                else
                    return string.Empty;
            }
        }

        public class PayloadDataset
        {
            public PayloadDataset(string name = null)
            {
                Name = name;
                Order = new PayloadOrderBy(this);
                Filter = new PayloadFilter(this);
            }

            public string Name
            {
                get; set;
            }

            public PayloadOrderBy Order
            {
                get;
            }

            public PayloadFilter Filter
            {
                get;
            }

            public override string ToString()
            {
                var result = new List<string>();

                var order = Order.ToString();
                var filter = Filter.ToString();

                if (!string.IsNullOrWhiteSpace(order))
                {
                    result.Add(order);
                }

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    result.Add(filter);
                }

                if (result.Count == 0)
                {
                    result.Add(this.Name);
                }

                return string.Join(",", result);
            }
        }

        public Payload([NotNull] T instance) : this()
        {
            if (this.DatasetInfo != default)
            {
                if (instance == null)
                    throw new ArgumentNullException(nameof(instance));

                if (instance.TryConvert(out DatasetMap? map) && map != null)
                {
                    this.DatasetInfo = (DatasetInfo)map;
                }
                else
                {
                    throw new ArgumentException($"instance does not implement DatasetMap or have attributre DatasetInfoAttribute.");
                }
            }

            if (DatasetInfo != default)
            {
                Dataset ??= new PayloadDataset(DatasetInfo.Name);
                Query ??= new PayloadQuery();
            }
        }

        public Payload()
        {
            //T instance = Activator.CreateInstance<T>();


            //if(instance.TryDatasetInfo(out var datasetInfo) && datasetInfo != null)
            //{
            //    this.DatasetInfo = datasetInfo.Value;
            //}
            var getDatasetInfoAttribute = typeof(T).GetCustomAttribute<DatasetInfoAttribute>(true);

            if (getDatasetInfoAttribute != null)
            {
                this.DatasetInfo = (DatasetInfo)getDatasetInfoAttribute;
            }
            else
            {
                throw new ArgumentException($"Type does not have attribute DatasetInfoAttribute.");
            }

            if (DatasetInfo != default)
            {
                Dataset ??= new PayloadDataset(DatasetInfo.Name);
                Query ??= new PayloadQuery();
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public PayloadDataset Dataset
        {
            get;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public PayloadQuery Query
        {
            get;
        }



        [JsonPropertyName("Datasets")]
        [JsonInclude]
        private string _datasets
        {
            get => this.Dataset.ToString();
        }

        [JsonPropertyName("q")]
        [JsonInclude]
        private string _query
        {
            get => this.Query.ToString();
        }


        [JsonPropertyName("Limit")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Limit
        {
            get; set;
        }


    }

}
