// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureBlobContext.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Groove.SP.Infrastructure.Azure
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using global::Azure.Core;
    using global::Azure.Storage.Blobs;
    using Groove.SP.Application.Providers.Azure;
    using Groove.SP.Core.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class AzureBlobContext : IAzureBlobContext
    {
        private readonly ILogger<AzureBlobContext> _logger;
        private readonly string _connectionString;
        private readonly BlobClientOptions _blobClientOptions;

        public AzureBlobContext(IOptions<AppConfig> appConfig, ILogger<AzureBlobContext> logger) : this(appConfig.Value.BlobStorage.AzureStorageConnectionString, appConfig, logger)
        {
        }

        public AzureBlobContext(string connectionString, IOptions<AppConfig> appConfig, ILogger<AzureBlobContext> logger)
        {
            _logger = logger;
            _connectionString = connectionString;
            _blobClientOptions = new BlobClientOptions()
            {
                Retry = {
                    Delay = TimeSpan.FromSeconds(appConfig.Value.Azure.AzureBlobRetryBackoffTime),
                    MaxRetries = appConfig.Value.Azure.AzureBlobMaxRetryAttempts,
                    Mode = RetryMode.Exponential
                }
            };
        }

        public async Task GetBlobAsync(string containerName, string path, Stream outStream)
        {
            containerName = containerName.ToLowerInvariant();
            var blockBlob = new BlobClient(_connectionString, containerName, path, _blobClientOptions);
            await blockBlob.DownloadToAsync(outStream);
        }

        public async Task PutBlobAsync(string containerName, string path, Stream inStream)
        {
            containerName = containerName.ToLowerInvariant();
            var blockBlob = new BlobClient(_connectionString, containerName, path, _blobClientOptions);
            await blockBlob.UploadAsync(inStream);
        }

        public async Task CreateContainerAsync(string containerName)
        {
            containerName = containerName.ToLowerInvariant();
            var blobContainer = new BlobContainerClient(_connectionString, containerName, _blobClientOptions);
            await blobContainer.CreateIfNotExistsAsync();

        }

        public async Task DeleteBlobAsync(string containerName, string path)
        {
            containerName = containerName.ToLowerInvariant();
            var blockBlob = new BlobClient(_connectionString, containerName, path, _blobClientOptions);
            await blockBlob.DeleteIfExistsAsync();
        }
    }
}