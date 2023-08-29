// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureBlobStorage.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Groove.SP.Infrastructure.BlobStorage
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Azure;
    using global::Azure;
    using Groove.SP.Application.Exceptions;
    using Groove.SP.Application.Providers.Azure;
    using Groove.SP.Application.Providers.BlobStorage;
    using Groove.SP.Core.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;


    public class AzureBlobStorage : BaseBlobStorage, IBlobStorage
    {
        private readonly ILogger<AzureBlobStorage> _logger;
        private readonly IAzureBlobContext _blobContext;

        public AzureBlobStorage(
            IOptions<AppConfig> appConfig,
            IOptions<CSEDShippingDocumentCredential> csedShippingDocumentCredentialConfig,
            IAzureBlobContext blobContext, 
            ILogger<AzureBlobStorage> logger)
            : base(appConfig, csedShippingDocumentCredentialConfig)
        {
            _blobContext = blobContext;
            _logger = logger;
        }

        public async Task<Stream> GetBlobAsync(string key)
        {
            // If it is CSED shipping document, call CSED API
            if (IsKeyFromCSEDAPI(key))
            {
                var documentUrl = GetCSEDDocumentUrl(key);
                var response = await GetCSEDAPIResultAsync(documentUrl);
                var result = await response.Content.ReadAsStreamAsync();
                return result;
            }

            var blobAddress = KeyToAzureBlobAddress(key);
            return await DownloadBlobAsync(key, blobAddress);
        }

        public async Task<byte[]> GetBlobAsByteArrayAsync(string key)
        {
            var blobStream = (MemoryStream)await GetBlobAsync(key);
            var blobBytes = blobStream.ToArray();
            return blobBytes;
        }

        public async Task<string> PutBlobAsync(string category, string name, Stream contents)
        {
            CreateAzureBlobAddress(category, name, out string key, out AzureBlobAddress blobAddress);

            try
            {
                await _blobContext.PutBlobAsync(blobAddress.ContainerName, blobAddress.Path, contents);
            }
            catch (RequestFailedException ex)
            {
                // container not exist -> create and upload again
                if (HttpStatusCode.NotFound.Equals(ex.ErrorCode))
                {
                    _logger.LogInformation($"Creating new container '{category.ToLower()}'");
                    await _blobContext.CreateContainerAsync(blobAddress.ContainerName);
                    await _blobContext.PutBlobAsync(blobAddress.ContainerName, blobAddress.Path, contents);
                }
                else
                {
                    throw new AppException(ex.InnerException, ExceptionResources.AzureStorage_Error, ex.Message);
                }
            }

            return key;
        }

        private void CreateAzureBlobAddress(string category, string name, out string key, out AzureBlobAddress blobAddress)
        {
            // category -> container: name must be in lower case
            if(!string.IsNullOrWhiteSpace(category))
            {
                category = category.ToLowerInvariant();
            }
            var ts = DateTime.UtcNow;
            var id = Guid.NewGuid().ToString("N");

            /* Check input syntax: A container name must be a valid DNS name, conforming to the following naming rules:
             * Container names must start or end with a letter or number, and can contain only letters, numbers, and the dash (-) character.
             * Every dash (-) character must be immediately preceded and followed by a letter or number; consecutive dashes are not permitted in container names.
             * All letters in a container name must be lowercase.
             * Container names must be from 3 through 63 characters long.
             */
            if (!Regex.Match(category, "(?=^.{3,63}$)(?!.*--)[^-][a-z0-9-]*[^-]").Success)
            {
                throw new AppException(ExceptionResources.BlobStorage_InvalidCategory, category);
            }

            name = GetValidFileName(name);

            // Key
            key = $"{category}:{ts:yyyyMMddHH}:{id}:{name}";

            // Blob address
            var fileName = string.Format("{0}_{1}", id, name);
            var path = $@"{ts.Year}/{ts.Month:00}/{ts.Day:00}/{ts.Hour:00}/{fileName}";
            blobAddress = new AzureBlobAddress(category, path);
        }


        private AzureBlobAddress KeyToAzureBlobAddress(string key)
        {
            var partOfKey = PartOfKey.ToAzureBlobAddress(key);

            var path = $@"{partOfKey.Year}/{partOfKey.Month}/{partOfKey.Day}/{partOfKey.Hour}/{partOfKey.FileName}";

            return new AzureBlobAddress(partOfKey.Category, path);
        }

        private async Task<Stream> DownloadBlobAsync(string key, AzureBlobAddress blobAddress)
        {
            var blobStream = new MemoryStream();
            try
            {
                await _blobContext.GetBlobAsync(blobAddress.ContainerName, blobAddress.Path, blobStream);
                blobStream.Position = 0;
                return blobStream;
            }
            catch (RequestFailedException ex)
            {
                if (HttpStatusCode.NotFound.Equals(ex.ErrorCode))
                {
                    throw new AppException(ex.InnerException, ExceptionResources.BlobStorage_NotFound, key);
                }

                throw new AppException(ex.InnerException, ExceptionResources.AzureStorage_Error, ex.Message);
            }
        }
        public async Task DeleteBlobAsync(string blobId)
        {
            try
            {
                var blobAddress = KeyToAzureBlobAddress(blobId);
                await _blobContext.DeleteBlobAsync(blobAddress.ContainerName, blobAddress.Path);
            }
            catch (RequestFailedException ex)
            {
                throw new AppException(ex.InnerException, ExceptionResources.AzureStorage_Error, ex.Message);
            }
            catch (Exception ex)
            {
                throw new AppException(ex.InnerException, nameof(AzureBlobStorage.DeleteBlobAsync));
            }
        }

        public async Task<byte[]> GetBlobByRelativePathAsync(string blobRelativePath)
        {
            if (string.IsNullOrEmpty(blobRelativePath))
            {
                throw new AppException($"{nameof(blobRelativePath)} is null or empty.");
            }

            var blobStream = new MemoryStream();
            try
            {
                // split relative path by :
                var pathSegments = blobRelativePath.Split(':').ToList();

                // Azure storage container name is the first segment
                var containerName = blobRelativePath.Split(':')[0];
                pathSegments.RemoveAt(0);

                // get file content
                await _blobContext.GetBlobAsync(containerName, string.Join("/", pathSegments), blobStream);
                blobStream.Position = 0;
                return blobStream.ToArray();
            }
            catch (RequestFailedException ex)
            {
                if (HttpStatusCode.NotFound.Equals(ex.ErrorCode))
                {
                    throw new AppException(ex.InnerException, ExceptionResources.BlobStorage_NotFound, blobRelativePath);
                }

                throw new AppException(ex.InnerException, ExceptionResources.AzureStorage_Error, ex.Message);
            }
        }

        private class PartOfKey
        {
            public string Id { get; set; }

            /// <summary>
            /// It is container name on Azure storage, and has to be in lower case
            /// </summary>
            public string Category { get; private set; }

            public string Name { get; set; }

            public string FileName => string.Format("{0}_{1}", Id, Name);

            public string Year { get; private set; }

            public string Month { get; private set; }

            public string Day { get; private set; }

            public string Hour { get; private set; }

            public static PartOfKey ToAzureBlobAddress(string key)
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new AppException(ExceptionResources.BlobStorage_InvalidKey, key, "null or empty");
                }

                var parts = key.Split(':');
                if (parts.Length != 4)
                {
                    throw new AppException(ExceptionResources.BlobStorage_InvalidKey, key, "4 parts expected");
                }

                var category = parts[0];
                var ts = parts[1];
                if (ts.Length != 10)
                {
                    throw new AppException(ExceptionResources.BlobStorage_InvalidKey, key, "timestamp format not 'yyyyMMddHH'");
                }

                var year = ts.Substring(0, 4);
                var month = ts.Substring(4, 2);
                var day = ts.Substring(6, 2);
                var hour = ts.Substring(8, 2);
                var id = parts[2];
                var name = parts[3];

                return new PartOfKey
                {
                    Id = id,
                    Name = name,
                    // category = container name -> must be in lower case
                    Category = category.ToLowerInvariant(),
                    Year = year,
                    Month = month,
                    Day = day,
                    Hour = hour
                };
            }
        }

        private class AzureBlobAddress
        {
            public string ContainerName { get; }

            public string Path { get; }

            public AzureBlobAddress(string containerName, string path)
            {
                ContainerName = containerName?.ToLowerInvariant() ?? string.Empty;
                Path = path;
            }
        }
    }
}