using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Infrastructure.DinkToPdf;
using Groove.SP.Infrastructure.RazorLight;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillment.Services
{
    public class SOFormPdfGenerator : ISOFormGeneratorStrategy
    {
        private readonly IHtmlStringBuilder _htmlStringBuilder;
        private readonly IPdfGenerator _pdfGenerator;

        public SOFormPdfGenerator(IHtmlStringBuilder htmlStringBuilder, IPdfGenerator pdfGenerator)
        {
            _htmlStringBuilder = htmlStringBuilder;
            _pdfGenerator = pdfGenerator;
        }

        public async Task<byte[]> GenerateAsync(ShippingOrderFormViewModel shippingOrderForm)
        {
            // Read template by RazorLight
            var htmlString = await _htmlStringBuilder.CreateHtmlStringAsync("Pdf/ShippingOrderForm.cshtml", shippingOrderForm);
            var pdfDetail = new PdfDetail
            {
                DocumentTitle = "Shipping Order Form",
                HtmlContent = htmlString
            };

            // Then generate .pdf content
            var fileBytes = _pdfGenerator.GeneratePdf(pdfDetail);

            return fileBytes;
        }
    }
}
