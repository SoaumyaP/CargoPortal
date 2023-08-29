// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseBlobStorage.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Groove.SP.Infrastructure.BlobStorage
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Groove.SP.Application.Exceptions;
    using Groove.SP.Core.Models;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;

    public abstract class BaseBlobStorage
    {
        private const string NO_NAME = "NoName";
        private const int MAX_FILE_NAME_LENGTH = 70;
        private const int MAX_FILE_EXTENSION_LENGTH = 10;
        protected const char PIPE = '|';
        protected const char COLON = ':';


        protected readonly AppConfig AppConfig;
        protected readonly CSEDShippingDocumentCredential _csedShippingDocumentCredentialConfig;


        protected BaseBlobStorage(
            IOptions<AppConfig> appConfig, 
            IOptions<CSEDShippingDocumentCredential> csedShippingDocumentCredentialConfig)
        {
            AppConfig = appConfig.Value;
            _csedShippingDocumentCredentialConfig = csedShippingDocumentCredentialConfig.Value;
        }

        /// <summary>
        /// Removes special characters.
        /// Truncate if file name is too long.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        protected string GetValidFileName(string fileName)
        {
            var name = fileName;
            if (string.IsNullOrWhiteSpace(name))
            {
                return NO_NAME;
            }

            name = Regex.Replace(name, "[^A-Za-z0-9\\s._-]+", string.Empty).Trim();
            if (name.Length > MAX_FILE_NAME_LENGTH)
            {
                var fileNameElements = name.Split('.');
                var fileExtension = fileNameElements.Last();
                var extensionHasFullStop = string.Format(".{0}", fileExtension);
                var nonFileExtension = fileNameElements.Length == 1 || (fileNameElements.Length > 1 && fileExtension.Length >= MAX_FILE_EXTENSION_LENGTH);

                // Cater for the case where there is no file extension passed in.
                if (nonFileExtension)
                {
                    name = string.Format(name.Substring(0, MAX_FILE_NAME_LENGTH));
                }
                else
                {
                    name = name.Substring(0, MAX_FILE_NAME_LENGTH - extensionHasFullStop.Length);
                    name = string.Format("{0}.{1}", name, fileExtension);
                }
            }

            return name;
        }

        #region CSED Shipping Document         

        /// <summary>
        /// To generate BlobId which integrate to CSED document API
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="documentPath"></param>
        /// <param name="documentName"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public string GenerateCSEDDocumentBlobId(Guid documentId, string documentPath, string documentName, string fileType)
        {
            var ts = DateTime.UtcNow;
            var result = $"{BlobCategories.CSEDSHIPPINGDOC}{PIPE}{documentId}{PIPE}{documentPath}{PIPE}{fileType}{PIPE}{ts:yyyyMMddHHmmss}";
            return result;
        }

        /// <summary>
        /// To check whether key (blobId) is from CSED
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected bool IsKeyFromCSEDAPI (string key)
        {
            return key.ToLowerInvariant().StartsWith(BlobCategories.CSEDSHIPPINGDOC.ToLowerInvariant());
        }

        /// <summary>
        /// To obtain document URL from blobId to get file content from CSED
        /// </summary>
        /// <param name="blobId"></param>
        /// <returns></returns>
        public string GetCSEDDocumentUrl(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new AppException(ExceptionResources.BlobStorage_InvalidKey, key, "null or empty");
            }

            var parts = key.Split(PIPE);
            if (parts.Length != 5)
            {
                throw new AppException(ExceptionResources.BlobStorage_InvalidKey, key, "5 parts expected");
            }

            var result = parts[2];
            return result;
        }

        /// <summary>
        /// To get a token prior to access CSED secured APIs
        /// </summary>
        /// <returns></returns>
        protected async Task<AuthenticationResult> GetCSEDAuthorizationResultAsync()
        {
            string clientId = _csedShippingDocumentCredentialConfig.ClientId;
            string appKey = _csedShippingDocumentCredentialConfig.ClientSecret;
            string authority = _csedShippingDocumentCredentialConfig.Authority;
            string[] scopes = _csedShippingDocumentCredentialConfig.Scopes;
            var applicationClient = ConfidentialClientApplicationBuilder.Create(clientId).WithClientSecret(appKey).WithAuthority(new Uri(authority)).Build();
            return await applicationClient.AcquireTokenForClient(scopes).ExecuteAsync();
        }

        /// <summary>
        /// To call CSED API to download file
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> GetCSEDAPIResultAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var authResult = await GetCSEDAuthorizationResultAsync();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", authResult.AccessToken);
                HttpResponseMessage response = await httpClient.GetAsync(url);
                return response;
            }
        }

        /// <summary>
        /// To get filtering text to check CSED document imported (by documentId)
        /// </summary>
        /// <param name="blobId"></param>
        /// <returns></returns>
        public string GetCSEDDocumentBlobFilter(string blobId)
        {
            var blobParts = blobId.Split(PIPE);
            var result = $"{blobParts[0]}|{blobParts[1]}%";
            return result;
        }        

        #endregion CSED Shipping Document
    }
}