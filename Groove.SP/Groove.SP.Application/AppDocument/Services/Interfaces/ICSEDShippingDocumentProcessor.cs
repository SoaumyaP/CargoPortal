using System.Threading.Tasks;

namespace Groove.SP.Application.AppDocument.Services.Interfaces
{
    public interface ICSEDShippingDocumentProcessor
    {
        /// <summary>
        /// To start to subscribe new message on Azure Service Bus as the system starts
        /// </summary>
        /// <returns></returns>
        Task StartProccessorAsync();
    }
}
