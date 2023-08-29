using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.POFulfillment
{
    public class ResetPOFFLoadSequenceValueJob
    {
        private readonly ILogger<ResetPOFFSequenceValueJob> _logger;
        private readonly IPOFulfillmentRepository _poFulfillmentRepository;

        public ResetPOFFLoadSequenceValueJob(ILogger<ResetPOFFSequenceValueJob> logger,
            IRepository<POFulfillmentModel> poFulfillmentRepository)
        {
            _logger = logger;
            _poFulfillmentRepository = (IPOFulfillmentRepository)poFulfillmentRepository;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("ResetPOFFLoadSequenceNumber Job Starting...");
            await _poFulfillmentRepository.ResetPOFFLoadSequenceValueAsync();
            _logger.LogInformation("ResetPOFFLoadSequenceNumber Job Completed.");
        }
    }
}
