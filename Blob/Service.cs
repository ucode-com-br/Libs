using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace UCode.Blob
{
    public class Service
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly List<string> _listContainerNames = new();

        public Service([NotNull] string connectionstring)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::Ctor => BlobServiceClient");

            this._blobServiceClient = new BlobServiceClient(connectionstring);

            this.ListContainers();

            stopwatch.Stop();

            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::Ctor => Result ({stopwatch.ElapsedMilliseconds}ms)");
        }

        private void ListContainers()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            foreach (var blobContainerItem in this._blobServiceClient.GetBlobContainers())
            {
                this._listContainerNames.Add(blobContainerItem.Name);
            }

            stopwatch.Stop();

            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::ListContainers => Result ({stopwatch.ElapsedMilliseconds}ms)");
        }

        public int ContainerCount()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::ContainerCount => Count");

            var result = this._listContainerNames.Count;

            stopwatch.Stop();

            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::ContainerCount => Result ({stopwatch.ElapsedMilliseconds}ms)");

            return result;
        }

        [return: NotNull]
        public async Task<Container> Container([NotNull] string containerName)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::Container => GetBlobContainerClient");

            var blobContainerClient = this._blobServiceClient.GetBlobContainerClient(containerName);

            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::Container => CreateIfNotExistsAsync");

            _ = await blobContainerClient.CreateIfNotExistsAsync();

            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::Container => NewContainer");

            var result = new Container(this, blobContainerClient);

            stopwatch.Stop();

            DebugWrite($"{nameof(UCode)}.{nameof(Blob)}.{nameof(Service)}::Container => Result ({stopwatch.ElapsedMilliseconds}ms)");


            return result;
        }

        private static void DebugWrite(string output)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine(output);
            }
        }
    }
}
