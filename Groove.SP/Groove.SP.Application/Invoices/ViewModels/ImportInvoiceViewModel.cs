using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Groove.SP.Application.Invoices.ViewModels
{
    public class ImportInvoiceViewModel
    {
        [Required]
        public string InvoiceNo { get; set; }

        [Required]
        public DateTime? InvoiceDate { get; set; }

        [Required]
        [MaxLength(1)]
        public string InvoiceType { get; set; }

        public DateTime? ETDDate { get; set; }

        public DateTime? ETADate { get; set; }

        public string BillOfLadingNo { get; set; }

        public string JobNo { get; set; }

        public long? BillToOrganizationId { get; set; }

        public long? BillByOrganizationId { get; set; }

        public string BillTo { get; set; }

        public string BillBy { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }
}