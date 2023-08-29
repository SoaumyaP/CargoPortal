using Groove.SP.Application.Scheduling.Services;
using Groove.SP.Application.Scheduling.Services.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    public class WebhooksController : Controller
    {
        private readonly ISchedulingService _schedulingService;
        public WebhooksController(ISchedulingService schedulingService)
        {
            _schedulingService = schedulingService;
        }

        [Authorize(Policy = "WebhookSignatureVerification")]
        public async Task<ActionResult> Scheduled([FromBody] object data)
        {
            // Rewind, so the core is not lost when it looks the body for the request
            if (HttpContext.Request.Body.CanSeek)
            {
                HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            }
            //OR Request.Body.Position = 0;

            // Read request body to stream
            using (var reader = new StreamReader(Request.Body))
            {
                // Read request body to string
                var body = reader.ReadToEndAsync().Result;

                if (!string.IsNullOrWhiteSpace(body))
                {
                    dynamic bodyContent = Newtonsoft.Json.JsonConvert.DeserializeObject(body);
                    var notification = bodyContent["Notifications"][0];
                    string documentId = notification["DocumentId"];
                    string scheduleId = notification["Id"];

                    try
                    {
                        await _schedulingService.UploadFtpAsync(scheduleId, documentId);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            return Ok();
        }
    }
}
