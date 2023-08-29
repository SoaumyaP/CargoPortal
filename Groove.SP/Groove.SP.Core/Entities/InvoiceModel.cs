using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Core.Entities
{
    public class InvoiceModel : Entity
    {
        public long Id { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceType { get; set; }
        public DateTime? ETDDate { get; set; }
        public DateTime? ETADate { get; set; }
        public string BillOfLadingNo { get; set; }
        public string JobNo { get; set; }
        /// <summary>
        /// Fulfilled by <see cref="Groove.SP.Infrastructure.CSFE.Models.Organization.Name"/>
        /// </summary>
        public string BillTo { get; set; }
        public long? BillToOrganizationId { get; set; }
        /// <summary>
        /// Fulfilled by <see cref="Groove.SP.Infrastructure.CSFE.Models.Organization.Name"/>
        /// </summary>
        public string BillBy { get; set; }
        public long? BillByOrganizationId { get; set; }
        public string BlobId { get; set; }
        public string FileName { get; set; }
        public PaymentStatusType? PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }

        public DateTime? DateOfSubmissionToCruise { get; set; }
        public DateTime? PaymentDueDate { get; set; }
    }
}
