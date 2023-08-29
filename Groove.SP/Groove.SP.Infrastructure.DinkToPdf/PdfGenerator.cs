using DinkToPdf;
using DinkToPdf.Contracts;
using System;

namespace Groove.SP.Infrastructure.DinkToPdf
{
    public class PdfGenerator : IPdfGenerator
    {
        private readonly IConverter _converter;
        public PdfGenerator(IConverter converter)
        {
            _converter = converter;
        }

        public byte[] GeneratePdf(PdfDetail pdfDetail)
        {
            if (pdfDetail == null)
            {
                throw new ArgumentNullException();
            }
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 5, Right = 5, Bottom = 5, Left = 5 },
                DocumentTitle = pdfDetail.DocumentTitle
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = pdfDetail.HtmlContent
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings },
            };

            return _converter.Convert(pdf);
        }
    }
}
