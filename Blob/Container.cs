using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace UCode.Blob
{
    public class Container
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="service">The service used to interact with containers.</param>
        /// <param name="cloudBlobContainer">The Azure Blob Container client.</param>
        internal Container([NotNull] Service service, [NotNull] BlobContainerClient cloudBlobContainer)
        {
            this.Service = service;

            this.CloudBlobContainer = cloudBlobContainer;
        }


        /// <summary>
        /// Gets or sets the service used to interact with containers.
        /// </summary>
        public Service Service
        {
            get; set;
        }

        /// <summary>
        /// Gets the name of the container.
        /// </summary>
        public string Name => this.CloudBlobContainer.Name;

        /// <summary>
        /// Gets the Azure Blob Container client.
        /// </summary>
        private BlobContainerClient CloudBlobContainer
        {
            get;
        }

        /// <summary>
        /// Uploads a file to the container.
        /// </summary>
        /// <param name="path">The path of the file in the container.</param>
        /// <param name="content">The content of the file to upload.</param>
        /// <param name="metadata">Optional metadata to associate with the file.</param>
        /// <param name="contentType">Optional content type of the file.</param>
        /// <param name="overwrite">Optional flag to determine if the file should be overwritten if it already exists.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UploadAsync([NotNull] string path, Stream content,
            IDictionary<string, string> metadata = null,
            string contentType = null,
            bool overwrite = true)
        {
            // Get the blob client for the specified path
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            // If the blob client is null, return early
            if (blob == null)
            {
                return;
            }

            // If content is not null, upload the file to the blob
            if (content != null)
            {
                if (content.CanSeek)
                {
                    content.Seek(0, SeekOrigin.Begin);
                }

                _ = await blob.UploadAsync(content, overwrite);
            }

            // If the blob exists, set its metadata and content type
            if (await blob.ExistsAsync())
            {
                // If metadata is not null, set the blob's metadata
                if (metadata != null)
                {
                    await blob.SetMetadataAsync(metadata);
                }

                // If contentType is not null, set the blob's content type
                if (contentType != null)
                {
                    // Get the blob's properties
                    BlobProperties properties = await blob.GetPropertiesAsync();

                    //using MD5 md5Hash = MD5.Create();

                    // Create a new BlobHttpHeaders object with the specified content type
                    var headers = new BlobHttpHeaders
                    {
                        // Set the MIME ContentType every time the properties 
                        // are updated or the field will be cleared
                        ContentType = contentType,

                        // Populate remaining headers with 
                        // the pre-existing properties
                        ContentLanguage = properties.ContentLanguage,
                        CacheControl = properties.CacheControl,
                        ContentDisposition = properties.ContentDisposition,
                        ContentEncoding = properties.ContentEncoding,
                        ContentHash = properties.ContentHash
                        //ContentHash = md5Hash.ComputeHash(content)
                    };

                    // Set the blob's HTTP headers
                    await blob.SetHttpHeadersAsync(headers);
                }
            }
        }

        /// <summary>
        /// Retrieves the metadata associated with the specified blob.
        /// </summary>
        /// <param name="path">The path of the blob.</param>
        /// <returns>A dictionary containing the blob's metadata.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the blob does not exist.</exception>
        [return: NotNull]
        public async Task<IDictionary<string, string>> GetMetadata([NotNull] string path)
        {
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            // Get the blob's properties and metadata.
            BlobProperties properties = await blob.GetPropertiesAsync();

            return properties.Metadata;
        }


        //public async Task<byte[]> DownloadAsync([NotNull] string path)

        /// <summary>
        /// Downloads the content of the specified blob as a stream.
        /// </summary>
        /// <param name="path">The path of the blob.</param>
        /// <returns>A stream containing the blob's content.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the blob does not exist.</exception>
        [return: NotNull]
        public async Task<Stream> DownloadAsync([NotNull] string path)
        {
            // Get the blob client for the specified path
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            // Check if the blob exists
            if (await blob.ExistsAsync())
            {
                // Create a new memory stream to store the blob's content
                MemoryStream memoryStream = new();

                //try
                //{
                //    using (BlobDownloadInfo download = await blob.DownloadAsync())
                //    {
                //        await download.Content.CopyToAsync(memoryStream);
                //    }
                //}
                //catch(Exception)
                //{
                //    memoryStream.Dispose();
                //    memoryStream = null;

                //    return null;
                //}

                // Download the blob's content and copy it to the memory stream.
                using (BlobDownloadInfo download = await blob.DownloadAsync())
                {
                    await download.Content.CopyToAsync(memoryStream);
                }

                // Return the memory stream containing the blob's content
                return memoryStream;
            }

            // Throw an exception if the blob does not exist
            throw new FileNotFoundException();
        }

        /// <summary>
        /// Downloads the content of the specified blob along with its metadata and content type.
        /// </summary>
        /// <param name="path">The path of the blob.</param>
        /// <returns>A tuple containing the blob's content as a stream, its metadata as a dictionary, and its content type as a string.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the blob does not exist.</exception>
        [return: NotNull]
        public async Task<(Stream Content, IDictionary<string, string> Metadata, string ContentType)> DownloadAllAsync([NotNull] string path)
        {
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            if (await blob.ExistsAsync())
            {
                MemoryStream memoryStream = new();

                //try
                //{
                //    using (BlobDownloadInfo download = await blob.DownloadAsync())
                //    {
                //        await download.Content.CopyToAsync(memoryStream);
                //    }
                //}
                //catch(Exception)
                //{
                //    memoryStream.Dispose();
                //    memoryStream = null;

                //    return null;
                //}

                // Download the blob's content and copy it to the memory stream.
                using (BlobDownloadInfo download = await blob.DownloadAsync())
                {
                    await download.Content.CopyToAsync(memoryStream);
                }

                try
                {
                    // Get the blob's properties and metadata.
                    var properties = await blob.GetPropertiesAsync();

                    // Return the blob's content, metadata, and content type.
                    return (memoryStream, properties.Value.Metadata, properties.Value.ContentType);
                }
                catch (Exception)
                {
                    // If there was an error getting the properties, return the content and null metadata and content type.
                    return (memoryStream, null, null);
                }
            }

            throw new FileNotFoundException();
        }

        /// <summary>
        /// Generates a download URL for a blob with a specified time-to-live (TTL).
        /// </summary>
        /// <param name="path">The path of the blob.</param>
        /// <param name="ttl">The time-to-live for the generated URL, in TimeSpan format.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains the generated download URL.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the blob does not exist.</exception>
        [return: NotNull]
        public async Task<Uri> GenerateDownloadUrl([NotNull] string path, [NotNull] TimeSpan ttl)
        {
            // Get the blob client for the specified path
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            // Check if the blob exists
            if (await blob.ExistsAsync())
            {
                // Check if the blob can generate a SAS URI
                if (blob.CanGenerateSasUri)
                {
                    // Create a SAS token that's valid for one hour.
                    var sasBuilder = new BlobSasBuilder
                    {
                        //BlobContainerName = CloudBlobContainer.Name,
                        // Set the blob name
                        BlobName = blob.Name,
                        // Set the expiration time to the current UTC time plus the specified TTL
                        ExpiresOn = DateTime.UtcNow.Add(ttl),
                        // Set the resource type to blob
                        Resource = "b"
                    };

                    // Set the permissions for the SAS token to allow read access
                    sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

                    // Generate the SAS URI for the blob using the specified permissions
                    return blob.GenerateSasUri(sasBuilder);
                }

                // Throw an exception if the blob cannot generate a SAS URI
                throw new Exception("Can`t generate sas uri.");
            }

            // Throw an exception if the blob does not exist
            throw new FileNotFoundException();
        }

        /// <summary>
        /// Deletes a blob from the container if it exists.
        /// </summary>
        /// <param name="path">The path of the blob to delete.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result is a boolean indicating whether the blob was successfully deleted.</returns>
        public async Task<bool> DeleteIfExistsAsync([NotNull] string path)
        {
            // Get the blob client for the specified path
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            // Delete the blob if it exists and return the result
            return (await blob.DeleteIfExistsAsync()).Value;
        }

        /// <summary>
        /// Checks if a blob exists in the container.
        /// </summary>
        /// <param name="path">The path of the blob to check.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result is a boolean indicating whether the blob exists.</returns>
        public async Task<bool> ExistAsync([NotNull] string path)
        {
            // Get the blob client for the specified path
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            // Check if the blob exists and return the result
            return (await blob.ExistsAsync()).Value;
        }

        /// <summary>
        /// Moves a blob from one path to another within the container.
        /// </summary>
        /// <param name="source">The path of the blob to move.</param>
        /// <param name="destination">The new path of the blob.</param>
        /// <param name="overwrite">Optional flag to determine if the destination blob should be overwritten if it already exists.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public async Task MoveAsync([NotNull] string source, [NotNull] string destination, bool overwrite = false)
        {
            // Get the blob client for the source and destination paths
            var sourceBlob = this.CloudBlobContainer.GetBlobClient(source);
            var destBlob = this.CloudBlobContainer.GetBlobClient(destination);

            // Copy the blob to the new destination
            await CopyAsync(sourceBlob, destBlob, overwrite);

            // Delete the source blob
            _ = sourceBlob.Delete();
        }

        /// <summary>
        /// Copies a blob from one path to another within the container.
        /// </summary>
        /// <param name="source">The path of the blob to copy.</param>
        /// <param name="destination">The new path of the blob.</param>
        /// <param name="overwrite">Optional flag to determine if the destination blob should be overwritten if it already exists.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public async Task CopyAsync([NotNull] string source, [NotNull] string destination, bool overwrite = false)
        {
            // Get the blob client for the source and destination paths
            var sourceBlob = this.CloudBlobContainer.GetBlobClient(source);
            var destBlob = this.CloudBlobContainer.GetBlobClient(destination);

            // Copy the blob to the new destination
            await CopyAsync(sourceBlob, destBlob, overwrite);
        }

        /// <summary>
        /// Copies a blob from one path to another within the container.
        /// </summary>
        /// <param name="sourceBlob">The blob client for the source blob.</param>
        /// <param name="destBlob">The blob client for the destination blob.</param>
        /// <param name="overwrite">Optional flag to determine if the destination blob should be overwritten if it already exists.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the source blob does not exist.</exception>
        private static async Task CopyAsync([NotNull] BlobClient sourceBlob, [NotNull] BlobClient destBlob, bool overwrite)
        {
            // Check if the source blob exists
            if (await sourceBlob.ExistsAsync())
            {
                // Acquire a lease on the source blob
                var sourceBlobLease = sourceBlob.GetBlobLeaseClient();
                _ = await sourceBlobLease.AcquireAsync(TimeSpan.FromSeconds(-1));

                // If overwrite is true, delete the destination blob if it exists
                if (overwrite)
                {
                    _ = destBlob.DeleteIfExists();
                }

                // Start copying the source blob to the destination blob
                _ = await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);

                // Get the properties of the source blob
                BlobProperties sourceBlobProperties = await sourceBlob.GetPropertiesAsync();

                // If the source blob is leased, break the lease
                if (sourceBlobProperties.LeaseState == LeaseState.Leased)
                {
                    _ = await sourceBlobLease.BreakAsync();
                }
            }
            else
            {
                // Throw an exception if the source blob does not exist
                throw new FileNotFoundException();
            }
        }
    }
}
