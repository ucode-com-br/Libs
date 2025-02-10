using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace UCode.Blob
{


    /// <summary>
    /// Represents a collection of Containers.
    /// </summary>
    public class Containers
    {
        protected readonly Dictionary<string, Container> _containers = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Containers"/> class
        /// with a specified <see cref="Service"/> object.
        /// </summary>
        /// <param name="service">
        /// The <see cref="Service"/> instance that is used to initialize the <see cref="Containers"/> class.
        /// This parameter must not be null.
        /// </param>
        public Containers([NotNull] Service service) => this.Service = service;

        /// <summary>
        /// Gets or sets the <see cref="Service"/> associated with this instance.
        /// </summary>
        /// <value>
        /// The <see cref="Service"/> object representing the service.
        /// </value>
        public Service Service
        {
            get; set;
        }

        public Container? this[string containerName]
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
        /// Converts the 
        [return: NotNull]
        public IEnumerable<Container> ToIEnumerable()
        {
            foreach (var item in this._containers)
            {
                yield return item.Value;
            }
        }

        /// <summary>
        /// Converts the current instance to a list of containers.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Container"/> objects representing the items in the current instance.
        /// This method returns a non-null list, regardless of the input state.
        /// </returns>
        /// <remarks>
        /// This method utilizes the ToIEnumerable() method to get an enumerable collection,
        /// which is then converted to a list. It ensures that the returned IList is not null,
        /// adhering to the NotNull attribute.
        /// </remarks>
        [return: NotNull]
        public IList<Container> ToList() => this.ToIEnumerable().ToList();

        /// <summary>
        /// Converts the current collection to an array of <see cref="Container"/> objects.
        /// </summary>
        /// <returns>
        /// An array containing the elements of the collection.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the enumeration is invalid due to changes made to the collection.
        /// </exception>
        /// <remarks>
        /// This method calls the <see cref="ToIEnumerable"/> method to get an <see cref="IEnumerable{Container}"/>
        /// representation of the collection before converting it to an array.
        /// </remarks>
        [return: NotNull]
        public Container[] ToArray() => this.ToIEnumerable().ToArray();

        /// <summary>
        /// Gets the count of containers from the service.
        /// </summary>
        /// <returns>
        /// An integer representing the number of containers.
        /// </returns>
        public int ContainerCount() => this.Service.ContainerCount();
    }
}
