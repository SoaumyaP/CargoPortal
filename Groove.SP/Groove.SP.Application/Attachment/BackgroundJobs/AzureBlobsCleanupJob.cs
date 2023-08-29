using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.Attachment.BackgroundJobs
{
    public class AzureBlobsCleanupJob
    {
        private readonly AppConfig _appConfig;
        private readonly IRepository<AttachmentModel> _attachmentRepository;
        private readonly IRepository<ShareDocumentModel> _shareDocumentRepository;

        private const string _attachmentContainerName = "attachment";
        private const string _sharedDocumentsContainerName = "share";

        private BlobContainerClient AttachmentContainerClient { set; get; }
        private BlobContainerClient ShareDocumentContainerClient { set; get; }

        private int BatchSize
        {
            get
            {
                return 200;
            }

        }

        public AzureBlobsCleanupJob(
            IOptions<AppConfig> appConfig,
            IRepository<ShareDocumentModel> shareDocumentRepository,
            IRepository<AttachmentModel> attachmentRepository)
        {
            _appConfig = appConfig.Value;
            _attachmentRepository = attachmentRepository;
            _shareDocumentRepository = shareDocumentRepository;
            AttachmentContainerClient = new BlobContainerClient(_appConfig.BlobStorage.AzureStorageConnectionString, _attachmentContainerName);
            ShareDocumentContainerClient = new BlobContainerClient(_appConfig.BlobStorage.AzureStorageConnectionString, _sharedDocumentsContainerName);

        }

        [JobDisplayName("Azure Blob clean-up: Attachments")]
        public void AzureAttachmentCleanupJob()
        {
            var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var fromDate = currentMonth.AddMonths(-1);
            var toDate = currentMonth.AddDays(-1);

            // If it is current month, ignore -> wait next month

            var currentDate = DateTime.Now;
            if (fromDate.Month == currentDate.Month && fromDate.Year == currentDate.Year)
            {
                return;
            }

            // Fire another back-ground job to keep track
            BackgroundJob.Enqueue(() => AzureAttachmentCleanupExecutionAsync(fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd")));

        }

        [JobDisplayName("Azure Blob clean-up: Attachments from {0} to {1}")]
        public async Task AzureAttachmentCleanupExecutionAsync(string fromDateString, string toDateString)
        {
            try
            {
                var fromDate = DateTime.Parse(fromDateString);

                var prefix = $"{fromDate.Year:0000}/{fromDate.Month:00}";

                // get blobs by prefix 2020/01, 2020/02, 20202/03
                var blobsOnAzure = await GetBlobsOnAzureAsync(AttachmentContainerClient, prefix);

                // split to batch of 200
                var chunkBlobs = blobsOnAzure.Chunk(BatchSize).ToList();

                foreach (var chunkBlob in chunkBlobs)
                {
                    var attachments = await _attachmentRepository.QueryAsNoTracking(a => chunkBlob.Select(b => GenerateBolbId(b, _attachmentContainerName)).Contains(a.BlobId))
                        .Select(c => c.BlobId)
                        .ToListAsync();

                    if (attachments.Count != chunkBlob.Length)
                    {
                        var toDeleteBlobs = chunkBlob.Select(c => GenerateBolbId(c, _attachmentContainerName)).Where(c => !attachments.Contains(c, StringComparer.InvariantCultureIgnoreCase)).ToList();

                        foreach (var item in toDeleteBlobs)
                        {
                            try
                            {
                                var blobAddress = KeyToAzureBlobAddress(item);
                                var blob = AttachmentContainerClient.GetBlobClient(blobAddress.Path);
                                await blob.DeleteIfExistsAsync();
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        [JobDisplayName("Azure Blob clean-up: Shared Documents")]
        public void AzureSharedDocumentsCleanupJob()
        {
            var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var fromDate = currentMonth.AddMonths(-1);
            var toDate = currentMonth.AddDays(-1);

            // If it is current month, ignore -> wait next month
            var currentDate = DateTime.Now;
            if (fromDate.Month == currentDate.Month && fromDate.Year == currentDate.Year)
            {
                return;
            }

            BackgroundJob.Enqueue(() => AzureSharedDocumentsCleanupExecutionAsync(fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd")));

        }

        [JobDisplayName("Azure Blob clean-up: Shared Documents from {0} to {1}")]
        public async Task AzureSharedDocumentsCleanupExecutionAsync(string fromDateString, string toDateString)
        {
            try
            {
                var fromDate = DateTime.Parse(fromDateString);

                int.TryParse(_appConfig.Email.AttachmentExpiredTime.ToString(), out var expiredTime);

                var prefix = $"{fromDate.Year:0000}/{fromDate.Month:00}";

                // get blobs by prefix 2020/01, 2020/02, 20202/03
                var blobsOnAzure = await GetBlobsOnAzureAsync(ShareDocumentContainerClient, prefix);

                // split to batch of 200
                var chunkBlobs = blobsOnAzure.Chunk(BatchSize).ToList();

                foreach (var chunkBlob in chunkBlobs)
                {
                    // get share document record stored in database
                    var sharedDocuments = await _shareDocumentRepository.QueryAsNoTracking(a =>
                        a.UpdatedDate.HasValue && chunkBlob.Select(b => GenerateBolbId(b, _sharedDocumentsContainerName)).Contains(a.BlobId))
                        .Select(c => new { UpdatedDate = c.UpdatedDate.Value, c.BlobId })
                        .ToListAsync();


                    // filter for records that expried
                    var toDeleteSharedDocuments = sharedDocuments.Where(c => DateTime.UtcNow.Subtract(c.UpdatedDate).TotalHours > expiredTime).ToList();
                    var toDeleteBlobAddresses = new List<AzureBlobAddress>();
                    foreach (var item in toDeleteSharedDocuments)
                    {
                        try
                        {
                            var blobAddress = KeyToAzureBlobAddress(item.BlobId);
                            toDeleteBlobAddresses.Add(blobAddress);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }

                    var toDeleteBlobs = blobsOnAzure
                        .Where(c => toDeleteBlobAddresses.Select(c => c.Path).Contains(c.Name, StringComparer.InvariantCultureIgnoreCase))
                        .ToList();

                    if (toDeleteBlobs?.Count > 0)
                    {
                        foreach (var item in toDeleteBlobs)
                        {
                            var blob = ShareDocumentContainerClient.GetBlobClient(item.Name);
                            await blob.DeleteIfExistsAsync();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// To get blobs by specific prefix
        /// </summary>
        /// <param name="blobContainerClient"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private async Task<List<BlobItem>> GetBlobsOnAzureAsync(BlobContainerClient blobContainerClient, string prefix)
        {
            try
            {
                var blobItems = new List<BlobItem>();

                // Call the listing operation and return pages of the specified size.
                var resultSegment = blobContainerClient.GetBlobsAsync(prefix: prefix).AsPages();

                // Enumerate the blobs returned for each page.
                await foreach (Page<BlobItem> blobPage in resultSegment)
                {
                    foreach (BlobItem blobItem in blobPage.Values)
                    {
                        blobItems.Add(blobItem);
                    }
                }

                return blobItems;
            }
            catch (RequestFailedException e)
            {
                throw e;
            }
        }

        private string GenerateBolbId(BlobItem blobItem, string containerName)
        {
            var lastIndex = blobItem.Name.IndexOf("_");
            var firstIndex = blobItem.Name.LastIndexOf("/", lastIndex);
            var guid = blobItem.Name.Substring(firstIndex + 1, lastIndex - firstIndex - 1);
            var yearMonthDayHour = blobItem.Name.Split("/")[0] + blobItem.Name.Split("/")[1] + blobItem.Name.Split("/")[2] + blobItem.Name.Split("/")[3];
            var name = blobItem.Name.Substring(lastIndex + 1, blobItem.Name.Length - lastIndex - 1);
            var blobId = $"{containerName}:{yearMonthDayHour}:{guid}:{name}";
            return blobId;
        }

        private AzureBlobAddress KeyToAzureBlobAddress(string key)
        {
            var partOfKey = PartOfKey.ToAzureBlobAddress(key);

            var path = $@"{partOfKey.Year}/{partOfKey.Month}/{partOfKey.Day}/{partOfKey.Hour}/{partOfKey.FileName}";

            return new AzureBlobAddress(partOfKey.Category, path);
        }

        private class AzureBlobAddress
        {
            public string ContainerName { get; }

            public string Path { get; }

            public AzureBlobAddress(string containerName, string path)
            {
                ContainerName = containerName;
                Path = path;
            }
        }
        private class PartOfKey
        {
            public string Id { get; set; }

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
                    Category = category,
                    Year = year,
                    Month = month,
                    Day = day,
                    Hour = hour
                };
            }
        }

    }
}
