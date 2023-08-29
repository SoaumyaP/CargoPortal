using Groove.SP.Application.ApplicationBackgroundJob.Services;
using Microsoft.AspNetCore.Mvc.Filters;

using System.Threading.Tasks;

namespace Groove.SP.API.Filters
{
    public class QueuedBackgroundJobsExecutionFilter : IAsyncResultFilter
    {
        private readonly IQueuedBackgroundJobs _queuedJobs;
        public QueuedBackgroundJobsExecutionFilter(IQueuedBackgroundJobs queuedJobs)
        {
            _queuedJobs = queuedJobs;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            await next();
            // Execute all jobs that enqueued in services, controllers' actions
            if (_queuedJobs != null)
            {
                _queuedJobs.FireAllJobs();
            }
        }
    }
}
