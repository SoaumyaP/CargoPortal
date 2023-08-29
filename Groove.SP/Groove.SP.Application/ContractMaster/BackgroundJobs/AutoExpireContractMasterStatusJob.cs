using Groove.SP.Application.MasterBillOfLading.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.ContractMaster.BackgroundJobs
{
    public class AutoExpireContractMasterStatusJob
    {
        private readonly ILogger<AutoExpireContractMasterStatusJob> _logger;
        private readonly IContractMasterService _contractMasterService;

        public AutoExpireContractMasterStatusJob(
            ILogger<AutoExpireContractMasterStatusJob> logger,
            IContractMasterService contractMasterService)
        {
            _logger = logger;
            _contractMasterService = contractMasterService;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("AutoExpireContractMasterStatus Job Starting...");
            await _contractMasterService.AutoExpireStatusAsync();
            _logger.LogInformation("AutoExpireContractMasterStatus Job Completed.");
        }
    }
}
