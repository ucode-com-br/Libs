using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace UCode.Extensions
{

    /// <summary>
    ///     Converte e verifica o sincronismo de implementacão de propriedades de classes.
    /// </summary>
    /// <typeparam name="TBase">Classe que esta herdando esta classe abstrata</typeparam>
    /// <typeparam name="TParent">Classe derivada ou classe pai, contendo todos os atributos</typeparam>
    [Obsolete("Usar Automapper", true)]
    public abstract class SyncClasses<TBase, TParent> where TBase : SyncClasses<TBase, TParent>
    {
        private static Type? _tBaseType;

        private static Type? _tParentType;

        private static string? _assembliesFullName;


        //private static Dictionary<string, string[]> checkImplementedParentPropertiesCache = new Dictionary<string, string[]>();
        private static readonly ConcurrentDictionary<string, string[]> checkImplementedParentPropertiesCache = new();

        public static Type? TBaseType
        {
            get
            {
                if (_tBaseType == null)
                {
                    _tBaseType = typeof(TBase);
                }

                return _tBaseType;
            }
        }

        public static Type? TParentType
        {
            get
            {
                if (_tParentType == null)
                {
                    _tParentType = typeof(TParent);
                }

                return _tParentType;
            }
        }

        public static string? AssembliesFullName
        {
            get
            {
                _assembliesFullName ??= $"{TBaseType.FullName}\n{TParentType.FullName}";

                return _assembliesFullName;
            }
        }

        public static TBase Convert(TParent source)
        {
            var x = typeof(TParent);
            var y = typeof(TBase);

            var result = default(TBase);

            if (source == null)
            {
                return result.Convert(ref source, ref result);
            }

            CheckImplementedParentProperties();

            var json = JsonSerializer.Serialize(source, JsonExtensions.GlobalSerializerOptions);

            result = JsonSerializer.Deserialize<TBase>(json, JsonExtensions.GlobalSerializerOptions);

            return result.Convert(ref source, ref result);
        }

        public static TParent Convert(TBase source)
        {
            var x = typeof(TParent);
            var y = typeof(TBase);


            var result = default(TParent);

            if (source == null)
            {
                return source.Convert(ref source, ref result);
            }

            CheckImplementedParentProperties();

            var json = JsonSerializer.Serialize(source, JsonExtensions.GlobalSerializerOptions);

            result = JsonSerializer.Deserialize<TParent>(json, JsonExtensions.GlobalSerializerOptions);

            return source.Convert(ref source, ref result);
        }

        //private static object checkImplementedParentPropertiesCacheLock = new object();
        private static void CheckImplementedParentProperties()
        {
            //lock (checkImplementedParentPropertiesCacheLock)
            //{
            if (AssembliesFullName == null)
            {
                return;
            }

            if (!checkImplementedParentPropertiesCache.ContainsKey(AssembliesFullName))
            {
                var tbaseProperties = TBaseType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var tparentProperties = TParentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                var inexistentItem = new List<string>();

                foreach (var tparentPropertiesItem in tparentProperties)
                {
                    if (tbaseProperties.All(a => a.Name != tparentPropertiesItem.Name))
                    {
                        inexistentItem.Add(tparentPropertiesItem.Name);
                    }
                }

                //checkImplementedParentPropertiesCache.Add(AssembliesFullName, inexistentItem.ToArray());
                checkImplementedParentPropertiesCache.TryAdd(AssembliesFullName, inexistentItem.ToArray());
            }

            if (checkImplementedParentPropertiesCache[AssembliesFullName].Length > 0)
            {
                throw new AggregateException(checkImplementedParentPropertiesCache[AssembliesFullName].Select(s =>
                    new Exception($"Property \"{s}\" in \"{TBaseType.FullName}\" does not exist.")));
            }
            //}
        }


        protected virtual TParent Convert(ref TBase from, ref TParent to) => to;

        protected virtual TBase Convert(ref TParent from, ref TBase to) => to;
    }
}
