using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.Invoices.ViewModels
{
    public class InvoiceViewModel : ViewModelBase<InvoiceModel>
    {
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string Vessel { get; set; }
        public DateTime ETDDate { get; set; }
        public DateTime ETADate { get; set; }
        public string BillOfLadingNo { get; set; }
        public ICollection<string> BillOfLadingNos
        {
            get
            {
                return BillOfLadingNo?.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            }
        }
        public string JobNo { get; set; }
        public string BillTo { get; set; }
        public string BillBy { get; set; }
        public string BlobId { get; set; }
        public string FileName { get; set; }

        public DateTime? DateOfSubmissionToCruise { get; set; }
        public DateTime? PaymentDueDate { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new NotImplementedException();
        }
    }
}
