using System.Threading.Tasks;
using Groove.SP.API.Filters;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Invoices.Services.Interfaces;
using Groove.SP.Application.Invoices.ViewModels;
using Groove.SP.Application.Providers.BlobStorage;
using Groove.SP.Core.Data;
using Groove.SP.Infrastructure.BlobStorage;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceListService _invoiceListService;
        private readonly IBlobStorage _blobStorage;

        public InvoicesController(IInvoiceService invoiceService, IBlobStorage blobStorage, IInvoiceListService invoiceListService)
        {
            _invoiceService = invoiceService;
            _blobStorage = blobStorage;
            _invoiceListService = invoiceListService;
        }

        [HttpGet]
        [Route("Search")]
        [AppAuthorize(AppPermissions.Invoice_List)]
        public async Task<IActionResult> Search([DataSourceRequest] DataSourceRequest request)
        {
            var viewModels = await _invoiceListService.GetListInvoiceAsync(request, CurrentUser);
            return new JsonResult(viewModels);
        }

        private async Task<FileContentResult> GetDownloadFile(string fileName, string blobPointer)
        {
            byte[] content = await _blobStorage.GetBlobAsByteArrayAsync(blobPointer);
            return File(content, "application/octet-stream", fileName);
        }

        [HttpGet]
        [AppAuthorize(AppPermissions.Invoice_List)]
        [Route("{id}/download/{fileName}")]
        public async Task<IActionResult> DownloadInvoice(string id, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var document = await _invoiceService.GetAsync(id);
                if (document != null && fileName.Equals(document.FileName))
                {
                    return await GetDownloadFile(document.FileName, document.BlobId);
                }
            }
            return null;
        }

        [HttpPost]
        [Route("import")]
        [Consumes("application/json", "application/json-patch+json", "multipart/form-data", "application/octet-stream")]
        public async Task<IActionResult> ImportInvoice([FromForm] ImportInvoiceViewModel model)
        {
            var stream = model.File.OpenReadStream();
            var fileName = model.File.FileName;
            var blobId = await _blobStorage.PutBlobAsync(BlobCategories.INVOICE, fileName, stream);
            var invoiceNo = await _invoiceService.ImportInvoice(model, blobId, fileName);

            var invoice = new {id = invoiceNo, fileName = fileName};
            var downloadUrl = Url.Action("DownloadInvoice", invoice);

            return Created(downloadUrl, invoice);
        }

        [HttpPut]
        [Route("updatepayment")]
        public async Task<IActionResult> UpdatePaymentAsync(InvoiceUpdatePaymentViewModel viewModel)
        {
            var updatedInvoice = await _invoiceService.UpdatePaymentAsync(viewModel);
            return Ok(updatedInvoice);
        }
    }
}