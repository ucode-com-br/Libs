using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UCode.Extensions
{

    public static class CloneExtensions
    {
        // Lista de tipos simples que não precisam de clonagem profunda
        private static readonly Type[] SimpleTypes =
        {
            typeof(string),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid)
        };

        /// <summary>
        /// Método de extensão para clonagem profunda (deep clone).
        /// 1. Tenta clonagem por reflexão.
        /// 2. Se falhar, cai no fallback de clonagem via JSON.
        /// </summary>
        public static T? DeepClone<T>(this T source)
        {
            if (source is null)
                return default;

            try
            {
                // Tenta clonagem reflexiva (caso haja tipos complexos,
                // mas com construtores padrão, campos/propriedades setáveis, etc.)
                return (T)CloneObject(source, new Dictionary<object, object>(ReferenceEqualityComparer.Instance));
            }
            catch
            {
                // Fallback: clonagem via System.Text.Json
                return source.CloneUsingJson();
            }
        }

        /// <summary>
        /// Clonagem "pura" por reflexão.
        /// Se algo der errado (ex.: tipo sem construtor padrão), lança exceção.
        /// </summary>
        private static object CloneObject(object original, IDictionary<object, object> visited)
        {
            var type = original.GetType();

            // Se for tipo simples ou enum, devolve ele mesmo.
            if (IsSimpleType(type) || type.IsEnum)
                return original;

            // Se já visitado antes, evita referência cíclica
            if (visited.ContainsKey(original))
                return visited[original];

            // Se for array
            if (original is Array array)
            {
                var elementType = type.GetElementType()!;
                var clonedArray = Array.CreateInstance(elementType, array.Length);
                visited[original] = clonedArray;

                for (int i = 0; i < array.Length; i++)
                {
                    var value = array.GetValue(i);
                    clonedArray.SetValue(value != null ? CloneObject(value, visited) : null, i);
                }

                return clonedArray;
            }

            // Se for coleção do tipo IList (ex.: List<T>, ArrayList, etc.)
            if (original is IList list)
            {
                var clonedList = (IList)Activator.CreateInstance(type)!;
                visited[original] = clonedList;
                foreach (var item in list)
                {
                    clonedList.Add(item != null ? CloneObject(item, visited) : null);
                }
                return clonedList;
            }

            // Podemos checar se é IDictionary (ex.: Dictionary<K,V>)
            if (original is IDictionary dict)
            {
                var clonedDict = (IDictionary)Activator.CreateInstance(type)!;
                visited[original] = clonedDict;
                foreach (var key in dict.Keys)
                {
                    var dictValue = dict[key];
                    clonedDict[key] = dictValue != null ? CloneObject(dictValue, visited) : null;
                }
                return clonedDict;
            }

            // Tenta criar instância (necessita construtor sem parâmetro)
            var clone = Activator.CreateInstance(type);
            if (clone is null)
            {
                // Se não conseguiu criar instância, forçamos falha
                // para cair no fallback (JSON)
                throw new InvalidOperationException(
                    $"Não foi possível criar instância do tipo {type.FullName}."
                );
            }

            visited[original] = clone;

            // Copiamos todos os campos e propriedades
            var members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var member in members)
            {
                switch (member)
                {
                    case FieldInfo fieldInfo:
                    {
                        // Se campo é "readonly", pode falhar => disparará exception
                        var fieldValue = fieldInfo.GetValue(original);
                        fieldInfo.SetValue(clone, fieldValue != null
                            ? CloneObject(fieldValue, visited)
                            : null);
                        break;
                    }
                    case PropertyInfo propInfo:
                    {
                        // Somente se a propriedade tem setter
                        if (propInfo.CanWrite && propInfo.GetIndexParameters().Length == 0)
                        {
                            var propValue = propInfo.GetValue(original);
                            propInfo.SetValue(clone, propValue != null
                                ? CloneObject(propValue, visited)
                                : null);
                        }
                        break;
                    }
                }
            }

            return clone;
        }

        /// <summary>
        /// Clonagem via serialização JSON com System.Text.Json.
        /// </summary>
        public static T? CloneUsingJson<T>(this T source)
        {
            if (source is null)
                return default;

            var options = new JsonSerializerOptions
            {
                // Se quiser evitar referência cíclica
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                // Ajuste outras opções conforme necessidade
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(source, options);
            return JsonSerializer.Deserialize<T>(json, options);
        }

        /// <summary>
        /// Verifica se o tipo em questão é um tipo "simples" (primitivo, string, date, etc.).
        /// </summary>
        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || SimpleTypes.Contains(type);
        }

        /// <summary>
        /// Comparador de referência para uso no dicionário "visited",
        /// garantindo que a chave seja o mesmo objeto, não apenas
        /// equivalência de Equals().
        /// </summary>
        private class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static ReferenceEqualityComparer Instance { get; } = new ReferenceEqualityComparer();

            private ReferenceEqualityComparer()
            {
            }

            public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }

}
