using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.Cruise.CruiseOrders.BackgroundJobs
{
    public class POLineItemSyncJob
    {
        private readonly IRepository<CruiseOrderModel> _cruiseOrderRepository;
        private readonly IRepository<PurchaseOrderModel> _purchaseOrderRepository;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public POLineItemSyncJob(IUnitOfWorkProvider unitOfWorkProvider, ICSFEApiClient csfeApiClient)
        {
            _csfeApiClient = csfeApiClient;
            _unitOfWorkProvider = unitOfWorkProvider;
            _unitOfWork = unitOfWorkProvider.CreateUnitOfWorkForBackgroundJob();
            _purchaseOrderRepository = _unitOfWork.GetRepository<PurchaseOrderModel>();
            _cruiseOrderRepository = _unitOfWork.GetRepository<CruiseOrderModel>();
        }

        [DisplayName("Sync Cruise Order #{0} Items to Purchare Order")]
        public async Task ExecuteAsync(long cruiseOrderId)
        {
            var cruiseOrder = await _cruiseOrderRepository.GetAsNoTrackingAsync(x => x.Id == cruiseOrderId, includes: i => i.Include(x => x.Items).Include(x => x.Contacts));
            if (cruiseOrder == null)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }

            var poModel = await _purchaseOrderRepository.GetAsync(x => x.CruiseOrderId == cruiseOrderId, includes: i => i.Include(x => x.LineItems));
            if (poModel == null)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }

            Groove.SP.Infrastructure.CSFE.Models.Organization principalOrg = null;

            var principalContact = cruiseOrder.Contacts.FirstOrDefault(contact => contact.OrganizationRole == OrganizationRole.Principal);
            if (principalContact != null)
            {
                principalOrg = await _csfeApiClient.GetOrganizationByIdAsync(principalContact.OrganizationId);
            }

            if (cruiseOrder.Items.Any())
            {
                foreach (var item in cruiseOrder.Items) // add new/ update
                {
                    var poLineKey = $"{principalOrg?.Code ?? ""}|{cruiseOrder.PONumber}|{item.POLine}|{item.ItemId}";// CustomerCode|PONumber|Line|ProductCode

                    var poLineItem = poModel.LineItems.SingleOrDefault(x => x.POLineKey == poLineKey);

                    if (poLineItem == null) // new
                    {
                        poLineItem = new();
                        poLineItem.POLineKey = poLineKey;
                        poModel.LineItems.Add(poLineItem);
                    }

                    poLineItem.SyncFromCruiseOrderItem(item);
                }

                foreach (var item in poModel.LineItems) // delete
                {
                    var isDelete = !cruiseOrder.Items.Any(x => $"{principalOrg?.Code ?? ""}|{cruiseOrder.PONumber}|{x.POLine}|{x.ItemId}" == item.POLineKey);
                    if (isDelete)
                    {
                        poModel.LineItems.Remove(item);
                    }
                }
            }
            else
            {
                poModel.LineItems.Clear();
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}