using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace UCode.Blob
{
    /// <summary>
    /// Represents a service that provides functionality or operations.
    /// </summary>
    /// <remarks>
    /// This class may define methods and properties specific to the service 
    /// it represents, and will be the foundation for additional service 
    /// implementations or extensions.
    /// </remarks>
    public class Service
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly List<string> _listContainerNames = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// This constructor accepts a connection string to initialize the BlobServiceClient.
        /// </summary>
        /// <param name="connectionstring">The connection string for the Blob Storage account.</param>
        public Service([NotNull] string connectionstring)
        {
            // Start a stopwatch to measure the time it takes to initialize the service
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Write a debug message indicating that the BlobServiceClient is being created
            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::Ctor => BlobServiceClient");

            // Create a new instance of the BlobServiceClient using the provided connection string
            this._blobServiceClient = new BlobServiceClient(connectionstring);

            // List all the containers in the Blob Storage account
            this.ListContainers();

            // Stop the stopwatch and write a debug message indicating the initialization is complete
            stopwatch.Stop();

            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::Ctor => Result ({stopwatch.ElapsedMilliseconds}ms)");
        }

        /// <summary>
        /// Lists all blob containers in the associated Blob Storage account and measures the time taken for the operation.
        /// </summary>
        /// <remarks>
        /// This method uses a stopwatch to track the duration of the listing operation and logs the total time taken once the operation is complete.
        /// </remarks>
        /// <returns>
        /// This method does not return a value.
        /// </returns>
        /// <example>
        /// <code>
        /// listContainersInstance.ListContainers();
        /// </code>
        /// This example shows how to call the ListContainers method.
        /// </example>
        private void ListContainers()
        {
            // Start a stopwatch to measure the time it takes to list the containers
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Iterate over each container in the Blob Storage account and add its name to the list
            foreach (var blobContainerItem in this._blobServiceClient.GetBlobContainers())
            {
                this._listContainerNames.Add(blobContainerItem.Name);
            }

            // Stop the stopwatch and write a debug message indicating the list operation is complete
            stopwatch.Stop();

            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::ListContainers => Result ({stopwatch.ElapsedMilliseconds}ms)");
        }

        /// <summary>
        /// Gets the count of containers in the Blob Storage account.
        /// </summary>
        /// <returns>
        /// The total number of containers present in the Blob Storage account.
        /// </returns>
        public int ContainerCount()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::ContainerCount => Count");

            // Get the count of containers in the Blob Storage account
            var result = this._listContainerNames.Count;

            stopwatch.Stop();

            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::ContainerCount => Result ({stopwatch.ElapsedMilliseconds}ms)");

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves a blob container by its name, creating it if it does not exist.
        /// </summary>
        /// <param name="containerName">The name of the blob container to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation, with a result of type <see cref="Container"/>.
        /// If the container did not previously exist, it will be created.</returns>
        [return: NotNull]
        public async Task<Container> Container([NotNull] string containerName)
        {
            // Start a stopwatch to measure the time it takes to get the container
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Write a debug message indicating that the BlobContainerClient is being retrieved
            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::Container => GetBlobContainerClient");

            // Get the BlobContainerClient for the specified container name
            var blobContainerClient = this._blobServiceClient.GetBlobContainerClient(containerName);

            // Write a debug message indicating that the container is being created
            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::Container => CreateIfNotExistsAsync");

            // Create the container if it does not exist
            _ = await blobContainerClient.CreateIfNotExistsAsync();

            // Write a debug message indicating that a new container is being created
            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::Container => NewContainer");

            // Create a new Container object using the specified container name and BlobContainerClient
            var result = new Container(this, blobContainerClient);

            // Stop the stopwatch and write a debug message indicating the container retrieval is complete
            stopwatch.Stop();

            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::Container => Result ({stopwatch.ElapsedMilliseconds}ms)");


            return result;
        }

        /// <summary>
        /// Writes output to the debug console if a debugger is attached.
        /// </summary>
        /// <param name="output">
        /// The output string to be written to the debug console.
        /// </param>
        private static void DebugWrite(string output)
        {
            // Check if a debugger is attached
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Write the output to the debug console
                System.Diagnostics.Debug.WriteLine(output);
            }
        }
    }
}
