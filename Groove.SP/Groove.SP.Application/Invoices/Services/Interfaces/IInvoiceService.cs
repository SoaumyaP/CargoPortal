using Groove.SP.Application.AppDocument.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Invoices.ViewModels;
using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.Invoices.Services.Interfaces
{
    public interface IInvoiceService : IServiceBase<InvoiceModel, InvoiceViewModel>
    {
        Task<InvoiceViewModel> GetAsync(string invoiceNo);
        Task<string> ImportInvoice(ImportInvoiceViewModel model, string blobId, string fileName);

        /// <summary>
        /// To import attachment for invoice from CSED (invoice)
        /// </summary>
        /// <param name="shippingDocument"></param>
        /// <param name="blobId"></param>
        /// <returns></returns>
        Task ImportCSEDSeaInvoiceAsync(CSEDShippingDocumentViewModel shippingDocument, string blobId);
        Task<InvoiceUpdatePaymentViewModel> UpdatePaymentAsync(InvoiceUpdatePaymentViewModel viewModel);
    }
}
