using System;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class AttachmentModel: Entity
    {
        public long Id { get; set; }

        public string FileName { get; set; }

        public string AttachmentType { get; set; }

        public string BlobId { get; set; }

        public string Description { get; set; }

        public string ReferenceNo { get; set; }

        public string UploadedBy { get; set; }

        public DateTime UploadedDate { get; set; }

        public bool IsDeleted { get; set; }

        public virtual ICollection<GlobalIdAttachmentModel> GlobalIdAttachments { get; set; }

    }
}
