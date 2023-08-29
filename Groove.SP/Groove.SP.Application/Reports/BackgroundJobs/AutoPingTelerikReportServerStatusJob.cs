using Groove.SP.Application.Utilities;
using Groove.SP.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Groove.SP.Application.Reports.BackgroundJobs
{
    public class AutoPingTelerikReportServerStatusJob
    {
        private readonly ILogger<AutoPingTelerikReportServerStatusJob> _logger;
        private readonly AppConfig _appConfig;

        public AutoPingTelerikReportServerStatusJob(
             ILogger<AutoPingTelerikReportServerStatusJob> logger,
             IOptions<AppConfig> appConfig
        )
        {
            _logger = logger;
            _appConfig = appConfig.Value;
        }

        [ShortExpirationJob(20)]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("AutoPingTelerikReportServerStatus Job Starting...");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_appConfig.Report.ReportServerUrl);
                HttpResponseMessage response = await client.GetAsync("Account/Login");
                response.EnsureSuccessStatusCode();
            }
            _logger.LogInformation("AutoPingTelerikReportServerStatus Job Completed.");
        }


    }
}
