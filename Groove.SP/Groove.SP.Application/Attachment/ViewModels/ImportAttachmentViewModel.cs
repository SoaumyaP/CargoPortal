using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Groove.SP.Application.Attachment.ViewModels
{
    public class ImportAttachmentViewModel
    {
        [Required]
        public long Id { get; set; }

        public long? ShipmentId { get; set; }

        public long? ContainerId { get; set; }

        public long? BillOfLadingId { get; set; }

        public long? MasterBillId { get; set; }

        public long? POFulfillmentId { get; set; }

        public string AttachmentType { get; set; }

        public string ReferenceNo { get; set; }

        public string Description { get; set; }

        [Required]
        public string UploadedBy { get; set; }

        [Required]
        public DateTime UploadedDate { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }
}
