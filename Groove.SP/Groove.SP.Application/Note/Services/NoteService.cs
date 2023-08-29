using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Note.Services.Interfaces;
using Groove.SP.Application.Note.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.Note.Services
{
    public class NoteService : ServiceBase<NoteModel, NoteViewModel>, INoteService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IRepository<CruiseOrderModel> _cruiseOrderRepository;
        private readonly IRepository<CruiseOrderItemModel> _cruiseOrderItemRepository;

        public NoteService(IUnitOfWorkProvider unitOfWorkProvider,
            IRepository<CruiseOrderModel> cruiseOrderRepository,
            IRepository<CruiseOrderItemModel> cruiseOrderItemRepository,
            IUserProfileRepository userProfileRepository) 
            : base(unitOfWorkProvider)
        {
            _userProfileRepository = userProfileRepository;
            _cruiseOrderRepository = cruiseOrderRepository;
            _cruiseOrderItemRepository = cruiseOrderItemRepository;
        }

        public async Task<IEnumerable<NoteViewModel>> GetNotesByIdAsync(long objectId, string objectType)
        {
            var globalId = CommonHelper.GenerateGlobalId(objectId, objectType);
            var noteList = await Repository.Query(n => n.GlobalObjectId == globalId, o => o.OrderByDescending(n => n.CreatedDate)).ToListAsync();

            var result = Mapper.Map<IEnumerable<NoteViewModel>>(noteList);
            return result;
        }

        public async Task<IEnumerable<NoteViewModel>> GetPurchaseOrderNotesByIdAsync(long purchaseOrderId)
        {
            return await GetNotesByIdAsync(purchaseOrderId, EntityType.CustomerPO);
        }

        public async Task<IEnumerable<NoteViewModel>> GetCruiseOrderItemNotesByIdAsync(long cruiseOrderItemId)
        {
            return await GetNotesByIdAsync(cruiseOrderItemId, EntityType.CruiseOrderItem);
        }

        public async Task<IEnumerable<NoteViewModel>> GetPOFulfillmentNotesByIdAsync(long poFulfillmentId)
        {
            return await GetNotesByIdAsync(poFulfillmentId, EntityType.POFullfillment);
        }

        public async Task<IEnumerable<NoteViewModel>> GetShipmentNotesByIdAsync(long shipmentId)
        {
            return await GetNotesByIdAsync(shipmentId, EntityType.Shipment);
        }

        public async Task<IEnumerable<NoteViewModel>> GetRoutingOrderNotesAsync(long routingOrderId)
        {
            return await GetNotesByIdAsync(routingOrderId, EntityType.RoutingOrder);
        }

        public async Task<NoteViewModel> CreateAsync(CreateAndUpdateNoteViewModel viewModel)
        {
            viewModel.ValidateAndThrow();

            var model = Mapper.Map<NoteModel>(viewModel);
            model.Audit(viewModel.CreatedBy);

            if (viewModel.PurchaseOrderId.HasValue)
            {
                var globalId = CommonHelper.GenerateGlobalId(viewModel.PurchaseOrderId.Value, EntityType.CustomerPO);
                model.GlobalObjectId = globalId;
            }

            if (viewModel.RoutingOrderId.HasValue)
            {
                var globalId = CommonHelper.GenerateGlobalId(viewModel.RoutingOrderId.Value, EntityType.RoutingOrder);
                model.GlobalObjectId = globalId;
            }

            if (viewModel.POFulfillmentId.HasValue)
            {
                var globalId = CommonHelper.GenerateGlobalId(viewModel.POFulfillmentId.Value, EntityType.POFullfillment);
                model.GlobalObjectId = globalId;
            }

            if (viewModel.ShipmentId.HasValue)
            {
                var globalId = CommonHelper.GenerateGlobalId(viewModel.ShipmentId.Value, EntityType.Shipment);
                model.GlobalObjectId = globalId;
            }

            if (viewModel.CruiseOrderId.HasValue)
            {
                List<NoteModel> noteModels = new List<NoteModel>();
                List<CruiseOrderItemModel> updatedCruiseOrderItems = new List<CruiseOrderItemModel>();
                var extendedData = JsonConvert.DeserializeObject<List<string>>(viewModel.ExtendedData);
                var cruiseOrder = await _cruiseOrderRepository.GetAsNoTrackingAsync(x => x.Id == viewModel.CruiseOrderId, 
                    null,
                    x => x.Include(i => i.Items));

                foreach (var item in extendedData)
                {
                    CreateAndUpdateNoteViewModel createViewModel = viewModel;
                    createViewModel.ExtendedData = JsonConvert.SerializeObject(new List<string>() { item });

                    var lineNumber = int.Parse(item.Split('-')[0]);
                    var lineItem = cruiseOrder.Items.SingleOrDefault(x => x.POLine == lineNumber);
                    lineItem.LatestDialog = createViewModel.Category;
                    updatedCruiseOrderItems.Add(lineItem);
                    var globalId = CommonHelper.GenerateGlobalId(lineItem.Id, EntityType.CruiseOrderItem);

                    var noteModel = Mapper.Map<NoteModel>(createViewModel);
                    noteModel.Audit(viewModel.CreatedBy);
                    noteModel.GlobalObjectId = globalId;
                    noteModels.Add(noteModel);
                }

                await Repository.AddRangeAsync(noteModels.ToArray());
                _cruiseOrderItemRepository.UpdateRange(updatedCruiseOrderItems.ToArray());
                await UnitOfWork.SaveChangesAsync();
                return Mapper.Map<NoteViewModel>(model);
            }

            await Repository.AddAsync(model);
            await UnitOfWork.SaveChangesAsync();
            return Mapper.Map<NoteViewModel>(model);
        }

        public async Task<NoteViewModel> UpdateAsync(long id, CreateAndUpdateNoteViewModel viewModel)
        {
            viewModel.ValidateAndThrow(true);

            var model = await this.Repository.GetAsync(x => x.Id == id);
            model.Audit(viewModel.UpdatedBy);

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id #{id} not found!");
            } 

            var globalIdPartsViewModel = CommonHelper.GetGlobalIdParts(model.GlobalObjectId);
            if (globalIdPartsViewModel.Type == EntityType.CruiseOrderItem)
            {
                // Update latest dialog for the item
                var cruiseOrderItemModel = await _cruiseOrderItemRepository.GetAsync(x => x.Id == globalIdPartsViewModel.EntityId);
                if (cruiseOrderItemModel != null)
                {
                    var cruiseOrderItemNotesViewModel = await this.GetNotesByIdAsync(globalIdPartsViewModel.EntityId, globalIdPartsViewModel.Type);
                    if (cruiseOrderItemNotesViewModel != null && cruiseOrderItemNotesViewModel.Count() > 0)
                    {
                        var latestNote = cruiseOrderItemNotesViewModel.OrderByDescending(x => x.CreatedDate).First();
                        if (latestNote.Id == model.Id)
                        {
                            cruiseOrderItemModel.LatestDialog = viewModel.Category;
                        }
                    }
                    else
                    {
                        cruiseOrderItemModel.LatestDialog = string.Empty;
                    }
                    _cruiseOrderItemRepository.Update(cruiseOrderItemModel);
                }
            }

            Mapper.Map(viewModel, model);

            Repository.Update(model);
            await UnitOfWork.SaveChangesAsync();
            return Mapper.Map<NoteViewModel>(model);
        }

        public async Task DeleteAsync(long id)
        {
            var note = await Repository.GetAsync(a => a.Id == id);

            if (note == null)
            {
                throw new AppEntityNotFoundException($"Object with the id #{id} not found!");
            }

            var globalIdPartsViewModel = CommonHelper.GetGlobalIdParts(note.GlobalObjectId);
            if (globalIdPartsViewModel.Type == EntityType.CruiseOrderItem)
            {
                // Update latest dialog for the item
                var cruiseOrderItemModel = await _cruiseOrderItemRepository.GetAsync(x => x.Id == globalIdPartsViewModel.EntityId);
                if (cruiseOrderItemModel != null)
                {
                    var cruiseOrderItemNotesViewModel = await this.GetNotesByIdAsync(globalIdPartsViewModel.EntityId, globalIdPartsViewModel.Type);
                    if (cruiseOrderItemNotesViewModel != null && cruiseOrderItemNotesViewModel.Count() > 1)
                    {
                        var latestNote = cruiseOrderItemNotesViewModel.OrderByDescending(x => x.CreatedDate).First(x => x.Id != id);
                        cruiseOrderItemModel.LatestDialog = latestNote.Category;
                    }
                    else
                    {
                        cruiseOrderItemModel.LatestDialog = string.Empty;
                    }
                    _cruiseOrderItemRepository.Update(cruiseOrderItemModel);
                }
            }

            Repository.Remove(note);
            await this.UnitOfWork.SaveChangesAsync();
        }
    }
}
