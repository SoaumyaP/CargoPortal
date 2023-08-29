using Azure;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Provider.Sftp;
using Groove.SP.Application.Providers.Azure;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Models;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Groove.SP.Infrastructure.Sftp
{
    public class SftpProvider : ISftpProvider
    {
        private readonly IAzureBlobContext _azureBlobContext;
        public SftpProvider(IAzureBlobContext azureBlobContext)
        {
            _azureBlobContext = azureBlobContext;
        }
        
        public async Task UploadFileAsync(byte[] file, string fileName, string folder, SftpProfile profile)
        {
            if (file is null || file.Length == 0)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(profile.BlobKeyId))
            {
                // get openssh private key file

                byte[] keyFile = await GetKeyAsync(profile.BlobKeyId);

                // login to sftp server

                ConnectionInfo conn = GetConnection(profile.HostName, profile.Username, profile.Port, keyFile);

                SftpClient client = new(conn);

                client.Connect();

                if (!string.IsNullOrEmpty(folder))
                {
                    client.ChangeDirectory(folder + @"/");
                }

                var stream = new MemoryStream(file);
                client.BufferSize = 4 * 1024;
                client.UploadFile(stream, fileName, null);

                client.Disconnect();
                client.Dispose();
            }
        }

        public async Task<List<string>> GetAndMoveFilesAsync(SftpProfile profile, string fromDirectory, string toDirectory)
        {
            var fileNames = new List<string>();

            byte[] file = await GetKeyAsync(profile.BlobKeyId);
            var conn = GetConnection(profile.HostName, profile.Username, profile.Port, file);

            using (SftpClient client = new(conn))
            {
                client.Connect();

                var zFiles = await GetFilesAsync(client, client.WorkingDirectory + fromDirectory, new List<SftpFile>());
                foreach (var zFile in zFiles)
                {
                    zFile.MoveTo(client.WorkingDirectory + $"{toDirectory}/{zFile.Name.AppendTimeStamp()}");
                    fileNames.Add(zFile.Name);
                }

                client.Dispose();
            }

            return fileNames;
        }

        public async Task<byte[]> GetFileAsync(SftpProfile profile, string fileName)
        {
            byte[] key = await GetKeyAsync(profile.BlobKeyId);
            var conn = GetConnection(profile.HostName, profile.Username, profile.Port, key);

            using (SftpClient client = new(conn))
            {
                client.Connect();

                Stream ms = new MemoryStream();

                var startDowload = client.BeginDownloadFile(client.WorkingDirectory + fileName, ms);
                client.EndDownloadFile(startDowload);

                ms.Seek(0, SeekOrigin.Begin);

                client.Dispose();

                return ms.GetAllBytes();
            }
        }

        private async Task<List<SftpFile>> GetFilesAsync(SftpClient sftpClient, string directory, List<SftpFile> files)
        {
            foreach (SftpFile sftpFile in sftpClient.ListDirectory(directory))
            {
                if (sftpFile.Name.StartsWith('.')) { continue; }

                if (sftpFile.IsDirectory)
                {
                    await GetFilesAsync(sftpClient, sftpFile.FullName, files);
                }
                else
                {
                    files.Add(sftpFile);
                }
            }
            return files;
        }

        /// <summary>
        /// Get connection info by private key file.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="username"></param>
        /// <param name="port"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private ConnectionInfo GetConnection(string host, string username, int port, byte[] key)
        {
            Stream stream = new MemoryStream(key);

            var keyFile = new PrivateKeyFile(stream, string.Empty);
            var authMethod = new PrivateKeyAuthenticationMethod(username, keyFile);

            ConnectionInfo connection = new(host, port, username, authMethod);

            return connection;
        }

        /// <summary>
        /// To get the openshsh private key file on Azure blob storage.
        /// </summary>
        /// <param name="blobId"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        private async Task<byte[]> GetKeyAsync(string blobId)
        {
            // try to get openssh private key file
            var ms = new MemoryStream();
            try
            {
                var blobUrlParts = blobId.Split(':');
                var category = blobUrlParts[0];
                var path = string.Join('/', blobUrlParts.Skip(1));

                await _azureBlobContext.GetBlobAsync(category, path, ms);
                ms.Position = 0;

                return ms.ToArray();
            }
            catch (RequestFailedException ex)
            {
                if (HttpStatusCode.NotFound.Equals(ex.ErrorCode))
                {
                    throw new AppException(ex.InnerException, ExceptionResources.BlobStorage_NotFound, blobId);
                }

                throw new AppException(ex.InnerException, ExceptionResources.AzureStorage_Error, ex.Message);
            }
        }
    }
}