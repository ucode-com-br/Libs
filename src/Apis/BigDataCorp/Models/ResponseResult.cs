using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using UCode.Extensions;


namespace UCode.Apis.BigDataCorp.Models
{
    public class ResponseResult<T>
    {
        [JsonPropertyName("MatchKeys")]
        public string MatchKeys
        {
            get; set;
        }

        [JsonExtensionData()]
        [JsonInclude]
        private Dictionary<string, object?> _matchValues = new Dictionary<string, object?>();

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public ReadOnlyDictionary<string, object?> MatchValues
        {
            get => _matchValues.AsReadOnly(); set => _matchValues = value.ToDictionary();
        }

        protected virtual string? DatasetClassName
        {
            get;
            private set;
        }
        public T Value
        {
            get
            {

                var isDefaulClassName = false;

                if (DatasetClassName == null)
                {
                    isDefaulClassName = TryGetClassName<T>(out var datasetClassName);

                    if (isDefaulClassName)
                        this.DatasetClassName = datasetClassName;
                }
                else
                {
                    isDefaulClassName = true;
                }



                if (MatchValues.Count == 0)
                {
                    _matchValues.Add(DatasetClassName, Activator.CreateInstance<T>());
                }

                if (MatchValues.ContainsKey(DatasetClassName))
                {
                    var @value = MatchValues[DatasetClassName];

                    var result = default(T);

                    if (TryParse(ref @value, out result))
                    {
                        return result;
                    }
                    //else if (TryParse<JsonArray>(ref @value, out JsonArray jsonArray) && (result = jsonArray.Select(s=>s.Deserialize<T>())) != null)
                    //{
                    //    return result;
                    //}
                    else if (TryParse<JsonElement>(ref @value, out var jsonElement) && (result = jsonElement.Deserialize<T>()) != null)
                    {
                        return result;
                    }
                    else if (TryParse<string>(ref @value, out var str) && string.IsNullOrWhiteSpace(str) && (result = JsonSerializer.Deserialize<T>(str)) != null)
                    {
                        return result;
                    }
                    else
                    {
                        // Force
                        return (T?)@value;
                    }
                }
                else
                {
                    // Buscar na colecao implicitamente.
                    // - Se existir uma unica chave que seja possivel converter, que nao seja null, usar esta chave
                    // - Se existir mais de uma chave que seja possivel converter, que nao seja null, mesma acao de nao existir nenhuma chave
                    // - Se nao existir nenhuma chave criar uma

                    var founded = false;
                    string? selectedKey = null;
                    object? selectedValue = null;
                    foreach (var item in MatchValues)
                    {
                        var key = item.Key;
                        var value = item.Value;

                        // Verificar se é nulo ou conversivel
                        if (value != null && TryParse<T>(ref value, out var instance))
                        {
                            if (founded)
                            {
                                // apontar como nao encontrado porque existe itens duplicados
                                founded = false;

                                // Apontar nome padrao
                                selectedKey = DatasetClassName;

                                // Apontar valor padrao
                                selectedValue = Activator.CreateInstance<T>();

                                // Sair do loop
                                break;
                            }
                            else
                            {
                                // apontar como encontrado
                                founded = true;

                                // apontar a chave encontrada
                                selectedKey = key;

                                // apontar o valor encontrado
                                selectedValue = instance;
                            }
                        }
                    }

                    _matchValues.Add(selectedKey ?? DatasetClassName, selectedValue ?? Activator.CreateInstance<T>());

                    return (T)MatchValues[selectedKey ?? DatasetClassName]!;
                }
            }
            set
            {
                var isDefaulClassName = false;

                if (DatasetClassName == null)
                {
                    isDefaulClassName = TryGetClassName<T>(out var datasetClassName);

                    if (isDefaulClassName)
                        this.DatasetClassName = datasetClassName;
                }
                else
                {
                    isDefaulClassName = true;
                }

                _matchValues[DatasetClassName] = value;
            }
        }

        private bool TryGetClassName<T>(out string className)
        {
            className = typeof(T).Name;

            T defaultObject = Activator.CreateInstance<T>();

            if (TryParse<T, DatasetMap>(ref defaultObject, out var map) && map != null)
            {
                className = ((DatasetInfo)map).ClassName;

                return true;
            }
            else
            {
                var isDatasetMapAttribute = typeof(T).GetCustomAttribute<DatasetInfoAttribute>(true);

                if (isDatasetMapAttribute != null)
                {
                    className = isDatasetMapAttribute.ClassName;

                    return true;
                }
                else
                {
                    var isDisplayName = typeof(T).GetCustomAttribute<DisplayNameAttribute>(true);

                    if (isDisplayName != null)
                    {
                        className = isDisplayName.DisplayName;

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

        }

        private bool TryParse<TResult>(ref object? obj, out TResult? @value) => TryParse<object?, TResult>(ref obj, out @value);

        private bool TryParse<TObject, TResult>(ref TObject? obj, out TResult? @value)
        {
            try
            {
                if (obj == null)
                {
                    @value = (TResult?)(object?)obj;
                    return true;
                }
                else if (obj is TResult)
                {
                    @value = ((TResult)(object)obj)!;
                    return true;
                }
                else
                {
                    var ltype = obj.GetType();
                    var rtype = typeof(TResult);

                    if (ltype == rtype)
                    {
                        @value = ((TResult)(object)obj)!;
                        return true;
                    }

                    @value = default;
                    return false;
                }
            }
            catch
            {
                @value = default;
                return false;
            }
        }


        public static implicit operator T(ResponseResult<T> source) => source.Value;
    }

}
