// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSystemBlobStorage.cs" company="Groove Technology">
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
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Groove.SP.Application.Exceptions;
    using Groove.SP.Application.Providers.BlobStorage;
    using Groove.SP.Core.Models;
    using Microsoft.Extensions.Options;

    public class FileSystemBlobStorage : BaseBlobStorage, IBlobStorage
    {
        private readonly string _rootPath;

        public FileSystemBlobStorage(
            IOptions<AppConfig> appConfig,
            IOptions<CSEDShippingDocumentCredential> csedShippingDocumentCredentialConfig)
        : base(appConfig, csedShippingDocumentCredentialConfig)
        {
            _rootPath = AppConfig.BlobStorage.FileSystemBlobStorageLocation;
            if (!_rootPath.EndsWith("\\"))
            {
                _rootPath = _rootPath + "\\";
            }
        }       

        public async Task<byte[]> GetBlobAsByteArrayAsync(string key)
        {
            // If it is CSED shipping document, call CSED API
            if (IsKeyFromCSEDAPI(key))
            {
                var documentUrl = GetCSEDDocumentUrl(key);
                var response = await GetCSEDAPIResultAsync(documentUrl);
                var result = await response.Content.ReadAsByteArrayAsync();
                return result;
            }

            string filePath = BlobKeyToPath(key, true, false);
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                byte[] buff = new byte[file.Length];
                await file.ReadAsync(buff, 0, (int)file.Length);
                return buff;
            }
        }

        public async Task<Stream> GetBlobAsync(string key)
        {
            // If it is CSED shipping document, call CSED secured API
            if (IsKeyFromCSEDAPI(key))
            {
                var documentUrl = GetCSEDDocumentUrl(key);
                var response = await GetCSEDAPIResultAsync(documentUrl);
                var result = await response.Content.ReadAsStreamAsync();
                return result;
            }

            return await Task.Run<Stream>(() =>
             {
                 var filePath = BlobKeyToPath(key, true, false);
                 return new FileStream(filePath, FileMode.Open, FileAccess.Read);
             });
        }

        public async Task<string> PutBlobAsync(string category, string name, Stream contents)
        {
            CreateFile(category, name, out string key, out FileInfo file);

            var buffer = new byte[4096];
            using (var fs = new FileStream(file.FullName, FileMode.Create, FileAccess.Write))
            {
                var count = contents.Read(buffer, 0, buffer.Length);
                while (count > 0)
                {
                    fs.Write(buffer, 0, count);
                    count = await contents.ReadAsync(buffer, 0, buffer.Length);
                }
            }

            return key;
        }

        public string BlobKeyToPath(string key, bool checkIfFileExists, bool createDirIfNotExists)
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
                throw new AppException(ExceptionResources.BlobStorage_InvalidKey, key,
                    "timestamp format not 'yyyyMMddHH'");
            }

            var year = ts.Substring(0, 4);
            var month = ts.Substring(4, 2);
            var day = ts.Substring(6, 2);
            var hour = ts.Substring(8, 2);
            var id = parts[2];
            var name = parts[3];

            var relativePath = $"{category}\\{year}\\{month}\\{day}\\{hour}";
            var dir = new DirectoryInfo(_rootPath + relativePath);
            if (!dir.Exists)
            {
                if (createDirIfNotExists)
                {
                    dir.Create();
                }
                else
                {
                    throw new AppException(ExceptionResources.BlobStorage_NotFound, key);
                }
            }

            var fileName = string.Format("{0}_{1}", id, name);
            var file = new FileInfo(dir.FullName + "\\" + fileName);
            if (checkIfFileExists)
            {
                if (!file.Exists)
                {
                    throw new AppException(ExceptionResources.BlobStorage_NotFound, key);
                }
            }

            return file.FullName;
        }

        private void CreateFile(string category, string name, out string key, out FileInfo file)
        {
            var ts = DateTime.UtcNow;
            var id = Guid.NewGuid().ToString("N");

            // Check input syntax
            if (!Regex.Match(category, "^[A-Za-z0-9]+$").Success)
            {
                throw new AppException(ExceptionResources.BlobStorage_InvalidCategory, category);
            }

            name = GetValidFileName(name);

            // Build blob key
            key = $"{category}:{ts:yyyyMMddHH}:{id}:{name}";

            // Make directory where we want to save the data
            var relativePath = $"{category}\\{ts.Year}\\{ts.Month:00}\\{ts.Day:00}\\{ts.Hour:00}";
            var dir = new DirectoryInfo(_rootPath + relativePath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            // Make the file name
            var fileName = string.Format("{0}_{1}", id, name);
            file = new FileInfo(dir.FullName + "\\" + fileName);
        }

        public async Task<byte[]> GetBlobByRelativePathAsync(string blobRelativePath)
        {
            if (string.IsNullOrEmpty(blobRelativePath))
            {
                throw new AppException($"{nameof(blobRelativePath)} is null or empty.");
            }

            // split relative path by :
            var pathSegments = blobRelativePath.Split(":");
            var fullPath = Path.Combine(_rootPath, String.Join("\\", pathSegments));

            // get file content
            using (var file = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                byte[] buff = new byte[file.Length];
                await file.ReadAsync(buff, 0, (int)file.Length);
                return buff;
            }

        }

        public async Task DeleteBlobAsync(string blobId)
        {
            await Task.Run(() => {
                string filePath = BlobKeyToPath(blobId, true, false);

                if (!string.IsNullOrEmpty(filePath))
                {
                    File.Delete(filePath);
                }
            });
        }
    }
}