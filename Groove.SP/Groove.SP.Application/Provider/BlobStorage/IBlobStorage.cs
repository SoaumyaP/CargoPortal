// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBlobStorage.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Groove.SP.Application.Providers.BlobStorage
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface IBlobStorage
    {
        /// <summary>
        /// To download file content as byte array
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<byte[]> GetBlobAsByteArrayAsync(string key);

        /// <summary>
        /// To download file content as stream
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<Stream> GetBlobAsync(string key);

        /// <summary>
        /// To store file: 1. File local system (development mode) 2. Azure blob storage
        /// </summary>
        /// <param name="category"></param>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        Task<string> PutBlobAsync(string category, string name, Stream contents);

        /// <summary>
        /// To generate BlobId which integrate to CSED document API
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="documentPath"></param>
        /// <param name="documentName"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        string GenerateCSEDDocumentBlobId(Guid documentId, string documentPath, string documentName, string fileType);

        /// <summary>
        /// To obtain document URL from blobId to get file content from CSED
        /// </summary>
        /// <param name="blobId"></param>
        /// <returns></returns>
        string GetCSEDDocumentUrl(string blobId);

        /// <summary>
        /// To get filtering text to check CSED document imported (by documentId)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetCSEDDocumentBlobFilter(string key);

        /// <summary>
        /// To get file by relative path
        /// </summary>
        /// <param name="blobRelativePath">e.g: template:warehousebooking:ORG123:Standard Booking Form.xlsx</param>
        /// <returns></returns>
        Task<byte[]> GetBlobByRelativePathAsync(string blobRelativePath);

        /// <summary>
        /// To delete a file
        /// </summary>
        /// <param name="blobId"></param>
        Task DeleteBlobAsync(string blobId);
    }
}