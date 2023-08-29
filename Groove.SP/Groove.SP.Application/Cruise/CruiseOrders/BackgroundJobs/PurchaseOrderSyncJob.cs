using Groove.SP.Application.CruiseOrders.Services;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.Cruise.CruiseOrders.BackgroundJobs
{
    public class PurchaseOrderSyncJob
    {
        private readonly IRepository<CruiseOrderModel> _cruiseOrderRepository;
        private readonly IRepository<PurchaseOrderModel> _purchaseOrderRepository;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public PurchaseOrderSyncJob(IUnitOfWorkProvider unitOfWorkProvider, ICSFEApiClient csfeApiClient)
        {
            _csfeApiClient = csfeApiClient;
            _unitOfWorkProvider = unitOfWorkProvider;
            _unitOfWork = unitOfWorkProvider.CreateUnitOfWorkForBackgroundJob();
            _purchaseOrderRepository = _unitOfWork.GetRepository<PurchaseOrderModel>();
            _cruiseOrderRepository = _unitOfWork.GetRepository<CruiseOrderModel>();
        }

        [DisplayName("Sync Cruise Order #{0} to Purchare Order - Sync mode: {1}")]
        public async Task ExecuteAsync(long cruiseOrderId, POSyncMode syncMode)
        {
            if (syncMode == POSyncMode.Delete)
            {
                await DeletePurchaseOrderAsync(cruiseOrderId);
                return;
            }

            // ELse, Add new - Update

            var cruiseOrder = await _cruiseOrderRepository.GetAsNoTrackingAsync(x => x.Id == cruiseOrderId, includes: i => i.Include(x => x.Contacts).Include(x => x.Items));
            if (cruiseOrder == null)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }
            
            Groove.SP.Infrastructure.CSFE.Models.Organization principalOrg = null;

            var principalContact = cruiseOrder.Contacts.FirstOrDefault(contact => contact.OrganizationRole == OrganizationRole.Principal);
            if (principalContact != null)
            {
                principalOrg = await _csfeApiClient.GetOrganizationByIdAsync(principalContact.OrganizationId);
            }

            PurchaseOrderModel poModel = new();
            //existing po
            if (syncMode == POSyncMode.Update)
            {
                poModel = await _purchaseOrderRepository.GetAsync(x => x.CruiseOrderId == cruiseOrderId);

                if (poModel == null)
                {
                    throw new AppEntityNotFoundException($"Object not found!");
                }
            }
            //new po
            else if (syncMode == POSyncMode.Add)
            {
                poModel.CruiseOrderId = cruiseOrderId;

                //add new contacts
                if (cruiseOrder.Contacts.Any())
                {
                    poModel.Contacts = new List<PurchaseOrderContactModel>();

                    var contactOrgIds = cruiseOrder.Contacts.Select(c => c.OrganizationId).Where(id => id != 0).ToList();
                    var contactOrgs = await _csfeApiClient.GetOrganizationByIdsAsync(contactOrgIds);
                    foreach (var contact in cruiseOrder.Contacts)
                    {
                        PurchaseOrderContactModel poContactModel = new();

                        var contactOrg = contactOrgs.SingleOrDefault(x => x.Id == contact.OrganizationId);
                        poContactModel.SyncFromCruiseOrderContact(contact, contactOrg);

                        poModel.Contacts.Add(poContactModel);
                    }
                }
                
                //add new items
                if (cruiseOrder.Items.Any())
                {
                    poModel.LineItems = new List<POLineItemModel>();

                    foreach (var item in cruiseOrder.Items)
                    {
                        POLineItemModel poLineItemModel = new();

                        poLineItemModel.SyncFromCruiseOrderItem(item);
                        poLineItemModel.POLineKey = $"{principalOrg?.Code ??""}|{cruiseOrder.PONumber}|{poLineItemModel.LineOrder}|{poLineItemModel.ProductCode}";// CustomerCode|PONumber|Line|ProductCode

                        poModel.LineItems.Add(poLineItemModel);
                    }
                }
            }
            else
            {
                return;
            }

            poModel.SyncFromCruiseOrder(cruiseOrder);
            poModel.POKey = $"{principalOrg?.Code ?? ""}|{poModel.PONumber}"; // CustomerCode|PONumber

            if (poModel.Id == 0)
            {
                await _purchaseOrderRepository.AddAsync(poModel);
            }
            
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task DeletePurchaseOrderAsync(long cruiseOrderId)
        {
            var toDeletePO = await _purchaseOrderRepository.GetAsync(x => x.CruiseOrderId == cruiseOrderId);

            if (toDeletePO == null)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }

            _purchaseOrderRepository.Remove(toDeletePO);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
