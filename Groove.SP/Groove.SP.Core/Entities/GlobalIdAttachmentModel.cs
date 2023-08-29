namespace Groove.SP.Core.Entities
{
    public class GlobalIdAttachmentModel: Entity
    {
        public string GlobalId { get; set; }

        public long AttachemntId { get; set; }

        public GlobalIdModel ReferenceEntity { get; set; }

        public AttachmentModel Attachment { get; set; }
    }
}
