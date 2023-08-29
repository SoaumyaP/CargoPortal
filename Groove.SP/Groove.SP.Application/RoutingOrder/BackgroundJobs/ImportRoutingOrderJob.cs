using Groove.SP.Application.Provider.Sftp;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE.Configs;
using Hangfire;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Groove.SP.Application.RoutingOrder.BackgroundJobs
{
    /// <summary>
    /// Importing routing order job by scanning the XML files on SFTP server.<br></br>
    /// Note that before importing, the job will move all processing files to another folder to avoid duplicated import on another Job.
    /// </summary>
    public class ImportRoutingOrderJob
    {
        #region Properties
        private readonly ILogger<ImportRoutingOrderJob> _logger;
        private readonly ISftpProvider _sftpProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppConfig _appConfig;
        private readonly CSFEApiSettings _csfeApiSettings;
        private readonly TokenResponse _tokenResponse;
        private readonly SFTPRoutingOrderServerProfile _sftpRoutingOrderServerProfile;
        #endregion

        public ImportRoutingOrderJob(ILogger<ImportRoutingOrderJob> logger,
            ISftpProvider sftpProvider,
            IOptions<AppConfig> appConfig,
            IOptions<CSFEApiSettings> csfeApiSettings,
            IOptions<SFTPRoutingOrderServerProfile> sftpRoutingOrderServerProfile,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _sftpProvider = sftpProvider;
            _appConfig = appConfig.Value;
            _csfeApiSettings = csfeApiSettings.Value;
            _httpClientFactory = httpClientFactory;
            _sftpRoutingOrderServerProfile = sftpRoutingOrderServerProfile.Value;

            // request token
            var _tokenRequest = new TokenRequest
            {
                Address = _csfeApiSettings.TokenEndpoint,
                ClientId = _csfeApiSettings.ClientId,
                ClientSecret = _csfeApiSettings.ClientSecret,
                GrantType = "client_credentials"
            };
            _tokenResponse = httpClientFactory.CreateClient().RequestTokenAsync(_tokenRequest).Result;
        }

        [JobDisplayName("Import Routing Orders by scanning XML files on the SFTP server")]
        [ShortExpirationJob(30)]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task ExecuteAsync()
        {
            try
            {
                SftpProfile sftpProfile = new()
                {
                    HostName = _sftpRoutingOrderServerProfile.Host,
                    Port = _sftpRoutingOrderServerProfile.Port,
                    Username = _sftpRoutingOrderServerProfile.Username,
                    BlobKeyId = _sftpRoutingOrderServerProfile.BlobKeyId
                };

                // In case missing the configuration info
                if (string.IsNullOrWhiteSpace(sftpProfile.HostName) ||
                    string.IsNullOrWhiteSpace(sftpProfile.Username) ||
                    string.IsNullOrWhiteSpace(sftpProfile.BlobKeyId)) return;
                

                // Get and move files which need to be import...
                var fileNames = await _sftpProvider.GetAndMoveFilesAsync(sftpProfile, _sftpRoutingOrderServerProfile.ImportDirectory, _sftpRoutingOrderServerProfile.ArchiveDirectory);

                foreach (var fn in fileNames)
                {
                    // Get file content 
                    byte[] file = await _sftpProvider.GetFileAsync(sftpProfile, $"{_sftpRoutingOrderServerProfile.ArchiveDirectory}/{fn}");

                    // Perform import

                    var importUrl = _appConfig.ApiUrl + "/routingorders/import";

                    using (Stream fs = new MemoryStream(file))
                    {
                        HttpRequestMessage request = new(HttpMethod.Post, importUrl);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenResponse.AccessToken);

                        var client = _httpClientFactory.CreateClient();

                        // Add the file
                        using (var multipartFormContent = new MultipartFormDataContent())
                        {
                            var fsContent = new StreamContent(fs);

                            fsContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
                            multipartFormContent.Add(fsContent, name: "routingOrderForm", fn);

                            request.Content = multipartFormContent;

                            try
                            {
                                HttpResponseMessage response = await client.SendAsync(request);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "An error occurred sending the request.", ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred importing routing order.", ex.Message);

                throw;
            }
        }
    }
}
