using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hangfire;
using Groove.SP.Application.ApplicationBackgroundJob.ViewModels;
using Groove.SP.Application.ApplicationBackgroundJob;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendMailsController : ControllerBase
    {
        public SendMailsController()
        {
        }

        [HttpPost]
        public async Task<IActionResult> SendEmailAsync(SendEmailViewModel viewModel)
        {
            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailWithBodyAsync(viewModel.JobDescription, viewModel.EmailBody, viewModel.MailTo, viewModel.MailSubject));
            return Ok();
        }
    }
}


