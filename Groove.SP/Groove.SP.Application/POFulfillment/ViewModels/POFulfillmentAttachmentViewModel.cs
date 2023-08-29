using Groove.SP.Application.Attachment.ViewModels;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class POFulfillmentAttachmentViewModel: AttachmentViewModel
    {
        public POFulfillmentAttachmentState? State { get; set; }
    }

    public enum POFulfillmentAttachmentState
    {
        Added = 1,
        Edited = 0,
        Deleted = -1
    }
}
