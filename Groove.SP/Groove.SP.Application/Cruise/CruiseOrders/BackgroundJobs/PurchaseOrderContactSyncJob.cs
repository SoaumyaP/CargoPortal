using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Infrastructure.CSFE;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.Cruise.CruiseOrders.BackgroundJobs
{
    public class PurchaseOrderContactSyncJob
    {
        private readonly IRepository<CruiseOrderModel> _cruiseOrderRepository;
        private readonly IRepository<PurchaseOrderModel> _purchaseOrderRepository;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public PurchaseOrderContactSyncJob(IUnitOfWorkProvider unitOfWorkProvider, ICSFEApiClient csfeApiClient)
        {
            _csfeApiClient = csfeApiClient;
            _unitOfWorkProvider = unitOfWorkProvider;
            _unitOfWork = unitOfWorkProvider.CreateUnitOfWorkForBackgroundJob();
            _purchaseOrderRepository = _unitOfWork.GetRepository<PurchaseOrderModel>();
            _cruiseOrderRepository = _unitOfWork.GetRepository<CruiseOrderModel>();
        }

        [DisplayName("Sync Cruise Order #{0} Contacts to Purchare Order")]
        public async Task ExecuteAsync(long cruiseOrderId)
        {
            var cruiseOrder = await _cruiseOrderRepository.GetAsNoTrackingAsync(x => x.Id == cruiseOrderId, includes: i => i.Include(x => x.Contacts));
            if (cruiseOrder == null)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }

            var poModel = await _purchaseOrderRepository.GetAsync(x => x.CruiseOrderId == cruiseOrderId, includes: i => i.Include(x => x.Contacts));
            if (poModel == null)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }

            poModel.Contacts.Clear();

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

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
