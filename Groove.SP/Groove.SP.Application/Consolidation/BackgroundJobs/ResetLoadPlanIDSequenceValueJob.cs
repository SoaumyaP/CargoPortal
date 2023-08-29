using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Consolidation
{
    public class ResetLoadPlanIDSequenceValueJob
    {
        private readonly ILogger<ResetLoadPlanIDSequenceValueJob> _logger;
        private readonly IConsolidationRepository _consolidationRepository;

        public ResetLoadPlanIDSequenceValueJob(ILogger<ResetLoadPlanIDSequenceValueJob> logger,
            IRepository<ConsolidationModel> consolidationRepository)
        {
            _logger = logger;
            _consolidationRepository = (IConsolidationRepository)consolidationRepository;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("ResetLoadPlanIDSequence Job Starting...");
            await _consolidationRepository.ResetLoadPlanIDSequenceValueAsync();
            _logger.LogInformation("ResetLoadPlanIDSequence Job Completed.");
        }
    }
}
