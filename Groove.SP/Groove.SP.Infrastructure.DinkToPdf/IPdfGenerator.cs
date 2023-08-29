using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Infrastructure.DinkToPdf
{
    public interface IPdfGenerator
    {
        byte[] GeneratePdf(PdfDetail pdfDetail);
    }
}
