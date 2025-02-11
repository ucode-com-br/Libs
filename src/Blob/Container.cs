using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.VisualBasic;

namespace UCode.Blob
{
    public class Container<T>: Container
    {
        public Container([NotNull] Service service): base(service, (service._blobServiceClient).GetBlobContainerClient(typeof(T).Name.ToLower()))
        {
        }

        internal Container([NotNull] Service service, [NotNull] BlobContainerClient cloudBlobContainer): base(service, cloudBlobContainer)
        {
        }
    }


    /// <summary>
    /// Represents a generic container that can hold items.
    /// This class can be used to store and manipulate a collection of objects of a specified type.
    /// </summary>
    public class Container
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="service">The <see cref="Service"/> instance that will be associated with this container.</param>
        /// <param name="cloudBlobContainer">The <see cref="BlobContainerClient"/> instance used for interacting with Azure Blob Storage.</param>
        /// <returns>
        /// A new instance of the <see cref="Container"/> class.
        /// </returns>
        internal Container([NotNull] Service service, [NotNull] BlobContainerClient cloudBlobContainer)
        {
            this.Service = service;

            this.CloudBlobContainer = cloudBlobContainer;
        }


        /// <summary>
        /// Gets or sets the <see cref="Service"/> associated with this instance.
        /// </summary>
        /// <value>
        /// The <see cref="Service"/> object that represents the service.
        /// </value>
        public Service Service
        {
            get; set;
        }

        /// <summary>
        /// Gets the name of the Cloud Blob Container associated with this instance.
        /// </summary>
        /// <value>
        /// A string representing the name of the Cloud Blob Container.
        /// </value>
        public string Name => this.CloudBlobContainer.Name;

        /// <summary>
        /// Gets the BlobContainerClient instance used to interact with the Azure Blob Storage container.
        /// </summary>
        /// <value>
        /// A <see cref="BlobContainerClient"/> that represents a client to the Azure Blob Storage container.
        /// </value>
        protected BlobContainerClient CloudBlobContainer
        {
            get;
            set;
        }

        /// <summary>
        /// Asynchronously uploads a file to an Azure Blob Storage container.
        /// </summary>
        /// <param name="path">The path within the blob container where the file will be uploaded.</param>
        /// <param name="content">The stream containing the content to upload.</param>
        /// <param name="metadata">Optional dictionary of metadata to associate with the blob.</param>
        /// <param name="contentType">Optional MIME type to set for the blob.</param>
        /// <param name="overwrite">Indicates whether to overwrite the blob if it already exists. Default is true.</param>
        /// <returns>A task that represents the asynchronous upload operation.</returns>
        public async Task UploadAsync([NotNull] string path, Stream content,
            IDictionary<string, string> metadata = null,
            string contentType = null,
            bool overwrite = true, CancellationToken cancellationToken = default)
        {
            // Get the blob client for the specified path
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            // If the blob client is null, return early
            if (blob == null)
            {
                return;
            }

            // If content is not null, upload the file to the blob
            if (content != null && content.CanRead)
            {
                if (content.CanSeek)
                {
                    content.Seek(0, SeekOrigin.Begin);
                }

                BlobUploadOptions blobUploadOptions = new BlobUploadOptions()
                {
                    Metadata = metadata ?? new Dictionary<string, string>(),
                    HttpHeaders = new BlobHttpHeaders
                    {
                        // Set the MIME ContentType every time the properties 
                        // are updated or the field will be cleared
                        ContentType = contentType ?? "application/octet-stream"
                    },
                    Conditions = overwrite ? null : new BlobRequestConditions { IfNoneMatch = new ETag("*") }
                };

                _ = await blob.UploadAsync(content, blobUploadOptions, cancellationToken: cancellationToken);

            }
            else
            {
                throw new ArgumentNullException(nameof(content));
            }
        }

        /// <summary>
        /// Asynchronously retrieves the metadata of a blob from the specified path in the cloud blob container.
        /// </summary>
        /// <param name="path">The path to the blob for which the metadata is to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary of the blob's metadata, 
        /// where the key is the metadata name and the value is the metadata value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> is null.</exception>
        /// <exception cref="StorageException">Thrown when there is an error while accessing the blob storage.</exception>
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
        /// Downloads the content of a blob from the specified path and returns it as a stream.
        /// </summary>
        /// <param name="path">The path to the blob in the cloud storage.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a stream 
        /// that holds the content of the blob if it exists; otherwise, an exception is thrown.
        /// </returns>
        /// <exception cref="FileNotFoundException">
        /// Thrown when the blob with the specified path does not exist.
        /// </exception>
        [return: NotNull]
        public async Task<Stream> DownloadAsync([NotNull] string path, CancellationToken cancellationToken = default)
        {
            // Get the blob client for the specified path
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            // Check if the blob exists
            if (await blob.ExistsAsync(cancellationToken))
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
                using (BlobDownloadInfo download = await blob.DownloadAsync(cancellationToken))
                {
                    await download.Content.CopyToAsync(memoryStream, cancellationToken);
                }

                // Return the memory stream containing the blob's content
                return memoryStream;
            }

            // Throw an exception if the blob does not exist
            throw new FileNotFoundException();
        }

        /// <summary>
        /// Asynchronously downloads the content of a blob from the specified path.
        /// </summary>
        /// <param name="path">The path of the blob to download.</param>
        /// <returns>
        /// A tuple containing the following:
        /// <list type="bullet">
        /// <item>
        /// <description>A <see cref="Stream"/> representing the content of the downloaded blob.</description>
        /// </item>
        /// <item>
        /// <description>An <see cref="IDictionary{TKey,TValue}"/> representing the metadata associated with the blob.</description>
        /// </item>
        /// <item>
        /// <description>A <see cref="string"/> representing the content type of the blob.</description>
        /// </item>
        /// </list>
        /// If the blob does not exist, a <see cref="FileNotFoundException"/> is thrown.
        /// </returns>
        /// <exception cref="FileNotFoundException">Thrown when the specified blob does not exist.</exception>
        [return: NotNull]
        public async Task<(Stream Content, IDictionary<string, string> Metadata, string ContentType)> DownloadAllAsync([NotNull] string path, CancellationToken cancellationToken = default)
        {
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            if (await blob.ExistsAsync(cancellationToken))
            {
                MemoryStream memoryStream = new();


                // Download the blob's content and copy it to the memory stream.
                using (BlobDownloadInfo download = await blob.DownloadAsync(cancellationToken))
                {
                    await download.Content.CopyToAsync(memoryStream);
                }

                try
                {
                    // Get the blob's properties and metadata.
                    var properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken);

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
        /// Generates a download URL for a blob in the cloud storage that is valid for a specified time-to-live (TTL).
        /// </summary>
        /// <param name="path">The path of the blob for which the download URL is to be generated.</param>
        /// <param name="ttl">The time span that indicates how long the generated URL will be valid.</param>
        /// <returns>
        /// A <see cref="Uri"/> that represents the generated SAS (Shared Access Signature) URI for the specified blob.
        /// </returns>
        /// <exception cref="FileNotFoundException">
        /// Thrown when the specified blob does not exist in the cloud storage.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when the blob cannot generate a SAS URI.
        /// </exception>
        [return: NotNull]
        public async Task<Uri> GenerateDownloadUrl([NotNull] string path, [NotNull] TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            // Get the blob client for the specified path
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            // Check if the blob exists
            if (await blob.ExistsAsync(cancellationToken: cancellationToken))
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
        /// Asynchronously deletes a blob at the specified path if it exists.
        /// </summary>
        /// <param name="path">The path of the blob to delete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result 
        /// contains a boolean indicating whether the blob was deleted (`true`) 
        /// or if it did not exist (`false`).
        /// </returns>
        public async Task<bool> DeleteIfExistsAsync([NotNull] string path, CancellationToken cancellationToken = default)
        {
            // Get the blob client for the specified path
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            // Delete the blob if it exists and return the result
            return (await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken)).Value;
        }

        /// <summary>
        /// Asynchronously checks if a blob exists at the specified path in the cloud blob container.
        /// </summary>
        /// <param name="path">The string path of the blob whose existence is to be checked. This parameter cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the blob exists.</returns>
        public async Task<bool> ExistAsync([NotNull] string path, CancellationToken cancellationToken = default)
        {
            // Get the blob client for the specified path
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            // Check if the blob exists and return the result
            return (await blob.ExistsAsync(cancellationToken)).Value;
        }

        /// <summary>
        /// Asynchronously moves a blob from the specified source path to the specified destination path,
        /// with an option to overwrite the destination blob if it exists.
        /// </summary>
        /// <param name="source">The source path of the blob to move.</param>
        /// <param name="destination">The destination path where the blob should be moved.</param>
        /// <param name="overwrite">A boolean value indicating whether to overwrite the destination blob if it already exists. Default is false.</param>
        /// <returns>A task that represents the asynchronous move operation.</returns>
        public async Task MoveAsync([NotNull] string source, [NotNull] string destination, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            // Get the blob client for the source and destination paths
            var sourceBlob = this.CloudBlobContainer.GetBlobClient(source);
            var destBlob = this.CloudBlobContainer.GetBlobClient(destination);

            // Copy the blob to the new destination
            await CopyAsync(sourceBlob, destBlob, overwrite, cancellationToken);

            // Delete the source blob
            _ = await sourceBlob.DeleteAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously copies a blob from the specified source path to the specified destination path.
        /// </summary>
        /// <param name="source">The path of the source blob to be copied.</param>
        /// <param name="destination">The path where the blob should be copied to.</param>
        /// <param name="overwrite">A boolean value indicating whether to overwrite the destination blob if it already exists. The default is false.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CopyAsync([NotNull] string source, [NotNull] string destination, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            // Get the blob client for the source and destination paths
            var sourceBlob = this.CloudBlobContainer.GetBlobClient(source);
            var destBlob = this.CloudBlobContainer.GetBlobClient(destination);

            // Copy the blob to the new destination
            await CopyAsync(sourceBlob, destBlob, overwrite, cancellationToken);
        }

        /// <summary>
        /// Asynchronously copies a blob from a source BlobClient to a destination BlobClient.
        /// </summary>
        /// <param name="sourceBlob">The BlobClient representing the source blob to copy.</param>
        /// <param name="destBlob">The BlobClient representing the destination where the blob will be copied.</param>
        /// <param name="overwrite">A boolean indicating whether to overwrite the destination blob if it exists.</param>
        /// <returns>A Task representing the asynchronous operation of copying the blob.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the source blob does not exist.</exception>
        private static async Task CopyAsync([NotNull] BlobClient sourceBlob, [NotNull] BlobClient destBlob, bool overwrite, CancellationToken cancellationToken = default)
        {
            // Check if the source blob exists
            if (await sourceBlob.ExistsAsync(cancellationToken))
            {
                // Acquire a lease on the source blob
                var sourceBlobLease = sourceBlob.GetBlobLeaseClient();
                _ = await sourceBlobLease.AcquireAsync(TimeSpan.FromSeconds(-1), cancellationToken: cancellationToken);

                // If overwrite is true, delete the destination blob if it exists
                if (overwrite)
                {
                    _ = destBlob.DeleteIfExists(cancellationToken: cancellationToken);
                }

                // Start copying the source blob to the destination blob
                _ = await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);

                // Get the properties of the source blob
                BlobProperties sourceBlobProperties = await sourceBlob.GetPropertiesAsync(cancellationToken: cancellationToken);

                // If the source blob is leased, break the lease
                if (sourceBlobProperties.LeaseState == LeaseState.Leased)
                {
                    _ = await sourceBlobLease.BreakAsync(cancellationToken: cancellationToken);
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
