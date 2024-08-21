using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace UCode.Blob
{
    public class Containers
    {
        /// <summary>
        /// A dictionary to store containers by name.
        /// </summary>
        private readonly Dictionary<string, Container> _containers = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Containers"/> class.
        /// </summary>
        /// <param name="service">The service used to interact with containers.</param>
        public Containers([NotNull] Service service) => this.Service = service;

        /// <summary>
        /// Gets or sets the service used to interact with containers.
        /// </summary>
        public Service Service
        {
            get; set;
        }

        /// <summary>
        /// Gets the container with the specified name.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>The container with the specified name, or null if the name is empty or null.</returns>
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
                    // If the container is not in the dictionary, create it and add it to the dictionary.
                    var containerTask = this.Service.Container(containerName);

                    containerTask.Wait();

                    this._containers.Add(containerName, containerTask.Result);
                }

                return this._containers[containerName];
            }
        }

        /// <summary>
        /// Converts the containers to an enumerable.
        /// </summary>
        /// <returns>An enumerable of containers.</returns>
        [return: NotNull]
        public IEnumerable<Container> ToIEnumerable()
        {
            foreach (var item in this._containers)
            {
                yield return item.Value;
            }
        }

        /// <summary>
        /// Converts the containers to a list.
        /// </summary>
        /// <returns>A list of containers.</returns>
        [return: NotNull]
        public IList<Container> ToList() => this.ToIEnumerable().ToList();

        /// <summary>
        /// Converts the containers to an array.
        /// </summary>
        /// <returns>An array of containers.</returns>
        [return: NotNull]
        public Container[] ToArray() => this.ToIEnumerable().ToArray();

        /// <summary>
        /// Gets the number of containers in the service.
        /// </summary>
        /// <returns>The number of containers in the service.</returns>
        public int ContainerCount() => this.Service.ContainerCount();
    }
}
