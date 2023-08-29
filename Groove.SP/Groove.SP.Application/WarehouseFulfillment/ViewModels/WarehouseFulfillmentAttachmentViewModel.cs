using Groove.SP.Application.Attachment.ViewModels;

namespace Groove.SP.Application.WarehouseFulfillment.ViewModels
{
    public class WarehouseFulfillmentAttachmentViewModel : AttachmentViewModel
    {
        public WarehouseFulfillmentAttachmentState? State { get; set; }
    }
    public enum WarehouseFulfillmentAttachmentState
    {
        Added = 1,
        Edited = 0,
        Deleted = -1
    }
}
