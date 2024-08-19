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
        internal Container([NotNull] Service service, [NotNull] BlobContainerClient cloudBlobContainer)
        {
            this.Service = service;

            this.CloudBlobContainer = cloudBlobContainer;
        }

        public Service Service
        {
            get; set;
        }

        public string Name => this.CloudBlobContainer.Name;

        private BlobContainerClient CloudBlobContainer
        {
            get;
        }


        public async Task UploadAsync([NotNull] string path, Stream content,
            IDictionary<string, string> metadata = null,
            string contentType = null,
            bool overwrite = true)
        {
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            if (blob == null)
            {
                return;
            }

            if (content != null)
            {
                if (content.CanSeek)
                {
                    content.Seek(0, SeekOrigin.Begin);
                }

                _ = await blob.UploadAsync(content, overwrite);
            }

            if (await blob.ExistsAsync())
            {
                if (metadata != null)
                {
                    await blob.SetMetadataAsync(metadata);
                }

                if (contentType != null)
                {
                    BlobProperties properties = await blob.GetPropertiesAsync();

                    //using MD5 md5Hash = MD5.Create();

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

                    await blob.SetHttpHeadersAsync(headers);
                }
            }
        }

        [return: NotNull]
        public async Task<IDictionary<string, string>> GetMetadata([NotNull] string path)
        {
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            // Get the blob's properties and metadata.
            BlobProperties properties = await blob.GetPropertiesAsync();

            return properties.Metadata;
        }


        //public async Task<byte[]> DownloadAsync([NotNull] string path)
        [return: NotNull]
        public async Task<Stream> DownloadAsync([NotNull] string path)
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

                using (BlobDownloadInfo download = await blob.DownloadAsync())
                {
                    await download.Content.CopyToAsync(memoryStream);
                }

                return memoryStream;
            }

            throw new FileNotFoundException();
        }

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

                using (BlobDownloadInfo download = await blob.DownloadAsync())
                {
                    await download.Content.CopyToAsync(memoryStream);
                }

                try
                {
                    var properties = await blob.GetPropertiesAsync();

                    return (memoryStream, properties.Value.Metadata, properties.Value.ContentType);
                }
                catch (Exception)
                {
                    return (memoryStream, null, null);
                }
            }

            throw new FileNotFoundException();
        }

        [return: NotNull]
        public async Task<Uri> GenerateDownloadUrl([NotNull] string path, [NotNull] TimeSpan ttl)
        {
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            if (await blob.ExistsAsync())
            {
                if (blob.CanGenerateSasUri)
                {
                    // Create a SAS token that's valid for one hour.
                    var sasBuilder = new BlobSasBuilder
                    {
                        //BlobContainerName = CloudBlobContainer.Name,
                        BlobName = blob.Name,
                        ExpiresOn = DateTime.UtcNow.Add(ttl),
                        Resource = "b"
                    };

                    sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

                    return blob.GenerateSasUri(sasBuilder);
                }

                throw new Exception("Can`t generate sas uri.");
            }

            throw new FileNotFoundException();
        }

        public async Task<bool> DeleteIfExistsAsync([NotNull] string path)
        {
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            return (await blob.DeleteIfExistsAsync()).Value;
        }

        public async Task<bool> ExistAsync([NotNull] string path)
        {
            var blob = this.CloudBlobContainer.GetBlobClient(path);

            return (await blob.ExistsAsync()).Value;
        }

        public async Task MoveAsync([NotNull] string source, [NotNull] string destination, bool overwrite = false)
        {
            var sourceBlob = this.CloudBlobContainer.GetBlobClient(source);
            var destBlob = this.CloudBlobContainer.GetBlobClient(destination);

            await CopyAsync(sourceBlob, destBlob, overwrite);

            _ = sourceBlob.Delete();
        }

        public async Task CopyAsync([NotNull] string source, [NotNull] string destination, bool overwrite = false)
        {
            var sourceBlob = this.CloudBlobContainer.GetBlobClient(source);
            var destBlob = this.CloudBlobContainer.GetBlobClient(destination);

            await CopyAsync(sourceBlob, destBlob, overwrite);
        }

        private static async Task CopyAsync([NotNull] BlobClient sourceBlob, [NotNull] BlobClient destBlob, bool overwrite)
        {
            if (await sourceBlob.ExistsAsync())
            {
                var sourceBlobLease = sourceBlob.GetBlobLeaseClient();

                _ = await sourceBlobLease.AcquireAsync(TimeSpan.FromSeconds(-1));

                if (overwrite)
                {
                    _ = destBlob.DeleteIfExists();
                }

                _ = await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);

                // Busca propriedades
                BlobProperties sourceBlobProperties = await sourceBlob.GetPropertiesAsync();

                if (sourceBlobProperties.LeaseState == LeaseState.Leased)
                {
                    _ = await sourceBlobLease.BreakAsync();
                }
            }
            else
            {
                throw new FileNotFoundException();
            }
        }
    }
}
