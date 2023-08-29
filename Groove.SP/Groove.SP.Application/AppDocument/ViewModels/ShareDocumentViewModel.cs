using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.AppDocument.ViewModels
{
    public class ShareDocumentViewModel : ViewModelBase<ShareDocumentModel>
    {
        public long Id { get; set; }

        public string FileName { get; set; }

        public string SharedBy { get; set; }

        public string BlobId { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
