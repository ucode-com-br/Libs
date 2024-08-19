using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace UCode.Blob
{
    public class Containers
    {
        private readonly Dictionary<string, Container> _containers = new();

        public Containers([NotNull] Service service) => this.Service = service;

        public Service Service
        {
            get; set;
        }

        public Container this[string containerName]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(containerName))
                {
                    return null;
                }

                if (!this._containers.ContainsKey(containerName))
                {
                    var containerTask = this.Service.Container(containerName);

                    containerTask.Wait();

                    this._containers.Add(containerName, containerTask.Result);
                }

                return this._containers[containerName];
            }
        }

        [return: NotNull]
        public IEnumerable<Container> ToIEnumerable()
        {
            foreach (var item in this._containers)
            {
                yield return item.Value;
            }
        }

        [return: NotNull]
        public IList<Container> ToList() => this.ToIEnumerable().ToList();

        [return: NotNull]
        public Container[] ToArray() => this.ToIEnumerable().ToArray();

        public int ContainerCount() => this.Service.ContainerCount();
    }
}
