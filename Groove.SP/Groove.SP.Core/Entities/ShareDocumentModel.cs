namespace Groove.SP.Core.Entities
{
    public class ShareDocumentModel : Entity
    {
        public long Id { get; set; }

        public string FileName { get; set; }

        public string SharedBy { get; set; }

        public string BlobId { get; set; }
    }
}
