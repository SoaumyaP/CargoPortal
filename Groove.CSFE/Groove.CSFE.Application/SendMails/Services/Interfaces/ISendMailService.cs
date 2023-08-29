using System.Threading.Tasks;

namespace Groove.CSFE.Application.SendMails.Services.Interfaces
{
    public interface ISendMailService
    {
        Task SendMailAsync<T>(string jobDescription, string templateName, T model, string mailTo, string mailSubject);
    }
}
