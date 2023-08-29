using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.Invoices.ViewModels
{
    [Keyless]
    public class InvoiceQueryModel
    {
        public string InvoiceType { set; get; }
        public string InvoiceTypeName { set; get; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? ETDDate { get; set; }
        public DateTime? ETADate { get; set; }
        public string BillOfLadingNo { get; set; }

        public ICollection<HouseBLInfo> BillOfLadingNos { get; set; }
        public string JobNo { get; set; }
        public string BillTo { get; set; }
        public string BillBy { get; set; }
        public string FileName { get; set; }
        public DateTime? DateOfSubmissionToCruise { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public PaymentStatusType? PaymentStatus { get; set; }
        public string PaymentStatusName { get; set; }
        public DateTime? PaymentDate { get; set; }

        public void SetBillOfLadingNos()
        {
            BillOfLadingNos = new List<HouseBLInfo>();
            var houseBLNo = BillOfLadingNo?.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            if (houseBLNo?.Count > 0)
            {
                foreach (var item in houseBLNo)
                {
                    BillOfLadingNos.Add(new HouseBLInfo()
                    {
                        BillOfLadingNo = item,
                    });
                }

            }
        }
    }

    public class HouseBLInfo
    {
        public string BillOfLadingNo { set; get; }
        public bool IsExistingInSystem { set; get; }
    }
}
