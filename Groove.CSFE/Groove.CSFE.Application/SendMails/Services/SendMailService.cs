using Groove.CSFE.Core;
using Microsoft.Extensions.Options;
using Groove.CSFE.Application.SendMails.Services.Interfaces;
using System.Threading.Tasks;
using RazorLight;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Groove.CSFE.Application.SendEmail.ViewModels;

namespace Groove.CSFE.Application.SendMails.Services
{
    public class SendMailService: ISendMailService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppConfig _appConfig;
        private readonly IRazorLightEngine _razorLight;


        public SendMailService(IOptions<AppConfig> appConfig, IRazorLightEngine razorLight, IHttpClientFactory httpClientFactory)
        {
            _appConfig = appConfig.Value;
            _razorLight = razorLight;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendMailAsync<T>(string jobDescription, string templateName, T model, string mailTo, string mailSubject)
        {
            string emailBody = await _razorLight.CompileRenderAsync(templateName, model);

            var httpClient = _httpClientFactory.CreateClient();

            var viewModel = new SendEmailViewModel()
            {
                MailTo = mailTo,
                EmailBody = emailBody,
                MailSubject = mailSubject,
                JobDescription = jobDescription
            };
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _appConfig.SendMailEndpoint)
            {
                Content = new StringContent(JsonConvert.SerializeObject(viewModel), Encoding.UTF8, "application/json"),
                
            };

            await httpClient.SendAsync(httpRequestMessage);
        }
    }
}
