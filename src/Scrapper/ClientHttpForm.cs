using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace UCode.Scrapper
{
    public class ClientHttpForm
    {
        public readonly string Action;

        public readonly string Id;


        internal readonly Dictionary<string, string> inputByName = new();
        public readonly string Method;
        public readonly string Name;

        public ClientHttpForm([NotNull] ClientHttpHandler clientHttpHandler, [NotNull] string id, [NotNull] string name,
            [NotNull] string method, [NotNull] string action)
        {
            this.ClientHttpHandler = clientHttpHandler;
            this.Id = id;
            this.Name = name;
            this.Method = method;
            this.Action = action;
        }

        public ClientHttpHandler ClientHttpHandler
        {
            get;
        }


        public int Count => this.inputByName.Count;

        public string this[string name] => this.inputByName[name];

        [return: NotNull]
        public async Task<IResultSnapshot> Submit()
        {
            var uri = new Uri(this.ClientHttpHandler.ResponseUri, this.Action);

            //Console.WriteLine($"Submit {uri}");

            var httpMethod = new HttpMethod(this.Method);

            var content = new FormUrlEncodedContent(this.inputByName);

            return await this.ClientHttpHandler.SendAsync(httpMethod, uri.ToString(), content);
        }

        [return: NotNull]
        public IEnumerable<string> Names()
        {
            foreach (var key in this.inputByName.Keys)
            {
                yield return key;
            }
        }

        public void SetValue([NotNull] Func<string, bool> predicade, [NotNull] string value)
        {
            var keys = this.inputByName.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];

                if (predicade(key))
                {
                    this.inputByName[key] = value;
                }
            }
        }

        public void SetValue([NotNull] string name, [NotNull] string value)
        {
            if (this.inputByName.ContainsKey(name))
            {
                this.inputByName[name] = value;
            }
            else
            {
                this.inputByName.Add(name, value);
            }
        }

        public bool Contains([NotNull] Func<string, bool> predicade)
        {
            var keys = this.inputByName.Keys.ToArray();

            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];

                if (predicade(key))
                {
                    return true;
                }
            }

            return false;
        }

        public void Remove([NotNull] string name) => this.inputByName.Remove(name);

        //public override string ToString()
        //{
        //    return string.Join('&', postContentArray.ToArray());
        //}
    }
}
