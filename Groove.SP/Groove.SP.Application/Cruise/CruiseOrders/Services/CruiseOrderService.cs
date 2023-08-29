using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Application.CruiseOrders.ViewModels;
using Groove.SP.Application.CruiseOrders.Services.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Groove.SP.Core.Models;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Cruise.CruiseOrders.BackgroundJobs;
using Groove.SP.Application.ApplicationBackgroundJob.Services;

namespace Groove.SP.Application.CruiseOrders.Services
{
    public class CruiseOrderService : ServiceBase<CruiseOrderModel, CreateCruiseOrderViewModel>, ICruiseOrderService
    {

        #region Fields or variables

        private readonly IQueuedBackgroundJobs _queuedBackgroundJobs;
        protected override Func<IQueryable<CruiseOrderModel>, IQueryable<CruiseOrderModel>> FullIncludeProperties => x => x.Include(m => m.Contacts).Include(m => m.Items);

        #endregion

        #region Constructor
        public CruiseOrderService(IUnitOfWorkProvider unitOfWorkProvider, IQueuedBackgroundJobs queuedBackgroundJobs
            ) : base(unitOfWorkProvider)
        {
            _queuedBackgroundJobs = queuedBackgroundJobs;
        }
        #endregion

        public async Task<CruiseOrderViewModel> CreateAsync(CreateCruiseOrderViewModel model, string userName)
        {
            var cruiseOrder = Mapper.Map<CruiseOrderModel>(model);
            cruiseOrder.POStatus = model.POStatus ?? CruiseOrderStatus.Active;

            // Ignore Sub1 and Sub2
            if (model.Items != null && model.Items.Any())
            {
                model.Items.Each(x => { x.Sub1 = null; x.Sub2 = null; });
            }

            await Repository.AddAsync(cruiseOrder);

            await this.UnitOfWork.SaveChangesAsync();

            // Sync to Purchase Order
            _queuedBackgroundJobs.Enqueue<PurchaseOrderSyncJob>(j => j.ExecuteAsync(cruiseOrder.Id, POSyncMode.Add));

            return Mapper.Map<CruiseOrderViewModel>(cruiseOrder);
        }

        public async Task<IEnumerable<CruiseOrderViewModel>> CreateBulkAsync(IEnumerable<CreateCruiseOrderViewModel> model, string userName)
        {
            var cruiseOrders = new List<CruiseOrderModel>();
            foreach (var item in model)
            {
                var cruiseOrder = Mapper.Map<CruiseOrderModel>(item);
                cruiseOrder.POStatus = item.POStatus ?? CruiseOrderStatus.Active;
                cruiseOrder.Audit(userName);

                cruiseOrders.Add(cruiseOrder);
            }
            if (cruiseOrders.Any())
            {
                // Ignore Sub1 and Sub2
                cruiseOrders.Where(x => x.Items != null && x.Items.Any()).SelectMany(x => x.Items).Each(x => { x.Sub1 = null; x.Sub2 = null; });
                await Repository.AddRangeAsync(cruiseOrders.ToArray());
                await UnitOfWork.SaveChangesAsync();

                // Sync to Purchase Order
                foreach (var adddedCruiseOrder in cruiseOrders)
                {
                    _queuedBackgroundJobs.Enqueue<PurchaseOrderSyncJob>(j => j.ExecuteAsync(adddedCruiseOrder.Id, POSyncMode.Add));
                }
            }
            return Mapper.Map<List<CruiseOrderViewModel>>(cruiseOrders);
        }

        public async Task DeleteAsync(long cruiseOrderId)
        {
            var cruiseOrder = await Repository.GetAsync(p => p.Id == cruiseOrderId, null, FullIncludeProperties);
            if (cruiseOrder == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {cruiseOrderId} not found!");
            }
            Repository.Remove(cruiseOrder);
            await UnitOfWork.SaveChangesAsync();

            // Sync to Purchase Order
            _queuedBackgroundJobs.Enqueue<PurchaseOrderSyncJob>(j => j.ExecuteAsync(cruiseOrder.Id, POSyncMode.Delete));
        }

        public async Task<CruiseOrderViewModel> UpdateAsync(long cruiseOrderId, UpdateCruiseOrderViewModel model, string userName)
        {
            var cruiseOrder = await Repository.GetAsync(p => p.Id == cruiseOrderId, null, FullIncludeProperties);

            if (cruiseOrder == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {cruiseOrderId} not found!");
            }

            if (model.IsPropertyDirty(nameof(UpdateCruiseOrderViewModel.Contacts)))
            {
                cruiseOrder.Contacts.Clear();
            }
            if (model.IsPropertyDirty(nameof(UpdateCruiseOrderViewModel.Items)))
            {
                cruiseOrder.Items.Clear();
                // Ignore Sub1 and Sub2
                if (model.Items != null && model.Items.Any())
                {
                    model.Items.Each(x => { x.Sub1 = null; x.Sub2 = null; });
                }
            }

            Mapper.Map(model, cruiseOrder);

            var isDuplicatedPONumber = await CheckDuplicatePONumberAsync(cruiseOrder);
            if (isDuplicatedPONumber)
            {
                throw new AppValidationException($"Duplicate '{nameof(UpdateCruiseOrderViewModel.PONumber)}'.");
            }

            Repository.Update(cruiseOrder);

            await UnitOfWork.SaveChangesAsync();

            #region Sync to Purchase Order
            _queuedBackgroundJobs.Enqueue<PurchaseOrderSyncJob>(j => j.ExecuteAsync(cruiseOrder.Id, POSyncMode.Update));

            if (model.IsPropertyDirty(nameof(UpdateCruiseOrderViewModel.Contacts)))
            {
                _queuedBackgroundJobs.Enqueue<PurchaseOrderContactSyncJob>(j => j.ExecuteAsync(cruiseOrder.Id));
            }

            if (model.IsPropertyDirty(nameof(UpdateCruiseOrderViewModel.Items)))
            {
                _queuedBackgroundJobs.Enqueue<POLineItemSyncJob>(j => j.ExecuteAsync(cruiseOrder.Id));
            }
            #endregion

            return Mapper.Map<CruiseOrderViewModel>(cruiseOrder);
        }

        public async Task<CruiseOrderViewModel> GetAsync(long cruiseOrderId)
        {
            Func<IQueryable<CruiseOrderModel>, IQueryable<CruiseOrderModel>> includeProperties = x
            => x.Include(m => m.Contacts)
                .Include(m => m.Items)
                .ThenInclude(m => m.Shipment);

            var cruiseOrder = await Repository.GetAsNoTrackingAsync(p => p.Id == cruiseOrderId, null, includeProperties);

            var result = Mapper.Map<CruiseOrderViewModel>(cruiseOrder);
            return result;
        }

        private async Task<bool> CheckDuplicatePONumberAsync(CruiseOrderModel model)
        {
            if (model.Contacts == null)
            {
                return false;
            }

            var cruiseOrders = await Repository.QueryAsNoTracking(c => c.PONumber == model.PONumber && c.Id != model.Id, includes: c => c.Include(a => a.Contacts)).ToListAsync();

            if (cruiseOrders?.Count() > 0)
            {
                var importPrincipalContacts = model.Contacts.Where(x => x.OrganizationRole.Equals(OrganizationRole.Principal, StringComparison.InvariantCultureIgnoreCase));
                var importConsigneeContacts = model.Contacts.Where(x => x.OrganizationRole.Equals(OrganizationRole.Consignee, StringComparison.InvariantCultureIgnoreCase));

                foreach (var cruiseOrder in cruiseOrders)
                {
                    var cruiseOrderContacts = cruiseOrder.Contacts;
                    if (cruiseOrderContacts?.Count() > 0)
                    {
                        var isDuplicatedByPrincipal = cruiseOrderContacts.Any(c =>
                            importPrincipalContacts.Any(x => x.OrganizationId == c.OrganizationId) &&
                            c.OrganizationRole.Equals(OrganizationRole.Principal, StringComparison.InvariantCultureIgnoreCase)
                            );


                        var isDuplicatedByConsignee = cruiseOrderContacts.Any(c =>
                            importConsigneeContacts.Any(x => x.CompanyName.Equals(c.CompanyName, StringComparison.InvariantCultureIgnoreCase)) &&
                            c.OrganizationRole.Equals(OrganizationRole.Consignee, StringComparison.InvariantCultureIgnoreCase)
                            );

                        if (isDuplicatedByPrincipal && isDuplicatedByConsignee)
                        {
                            return true;
                        }
                    }
                }

            }
            return false;
        }
    }
}