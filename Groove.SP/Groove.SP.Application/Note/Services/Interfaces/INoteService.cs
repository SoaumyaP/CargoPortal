using Groove.SP.Application.Common;
using Groove.SP.Application.Note.ViewModels;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.Note.Services.Interfaces
{
    public interface INoteService : IServiceBase<NoteModel, NoteViewModel>
    {
        Task<IEnumerable<NoteViewModel>> GetPurchaseOrderNotesByIdAsync(long purchaseOrderId);
        Task<IEnumerable<NoteViewModel>> GetCruiseOrderItemNotesByIdAsync(long cruiseOrderItemId);
        Task<IEnumerable<NoteViewModel>> GetPOFulfillmentNotesByIdAsync(long poFulfillmentId);
        Task<IEnumerable<NoteViewModel>> GetShipmentNotesByIdAsync(long shipmentId);
        Task<IEnumerable<NoteViewModel>> GetRoutingOrderNotesAsync(long routingOrderId);
        Task<NoteViewModel> CreateAsync(CreateAndUpdateNoteViewModel viewModel);
        Task<NoteViewModel> UpdateAsync(long id, CreateAndUpdateNoteViewModel viewModel);
        Task DeleteAsync(long id);
    }
}
