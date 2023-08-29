using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Events;
using Groove.SP.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Groove.SP.Application.Activity.DomainEventHandlers
{
    public class BookingActivityChangedDomainEventHandler : INotificationHandler<ActivityChangedDomainEvent>
    {
        private readonly IPOFulfillmentRepository _poFulfillmentRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BookingActivityChangedDomainEventHandler(IRepository<POFulfillmentModel> poFulfillmentRepository,
            IRepository<ActivityModel> activityRepository,
            IRepository<PurchaseOrderModel> purchaseOrderRepository,
            IUnitOfWorkProvider unitOfWorkProvider)
        {
            _poFulfillmentRepository = (IPOFulfillmentRepository)poFulfillmentRepository;
            _activityRepository = (IActivityRepository)activityRepository;
            _purchaseOrderRepository = (IPurchaseOrderRepository)purchaseOrderRepository;
            _unitOfWork = unitOfWorkProvider.CreateUnitOfWork();
        }

        public async Task Handle(ActivityChangedDomainEvent notification, CancellationToken cancellationToken)
        {
            if (!notification.CurrentBookingId.HasValue)
            {
                return;
            }

            var poFulfillment = await _poFulfillmentRepository.FindAsync(notification.CurrentBookingId.Value);

            if (poFulfillment == null)
            {
                return;
            }

            if (notification.CurrentActivityCode == Event.EVENT_1068)
            {
                await UpdateActivityOnPurchaseOrders(Event.EVENT_1009, notification.CurrentActivityDate, notification.CurrentLocation, notification.CurrentRemark, poFulfillment.Id);
                return;
            }

            if (notification.CurrentActivityCode == Event.EVENT_1071)
            {
                await UpdateActivityOnPurchaseOrders(Event.EVENT_1010, notification.CurrentActivityDate, notification.CurrentLocation, notification.CurrentRemark, poFulfillment.Id);
                return;
            }
        }

        private async Task UpdateActivityOnPurchaseOrders(string eventCode, DateTime activityDate, string location, string remark, long poffId)
        {
            var poff = await _poFulfillmentRepository.GetAsync(x => x.Id == poffId,
                null,
                i => i.Include(m => m.Orders));
            var purchaseOrderIdList = poff.Orders.Select(x => x.PurchaseOrderId).Distinct();
            var purchaseOrderList = _purchaseOrderRepository.Query(po => purchaseOrderIdList.Any(poId => po.Id == poId),
                null,
                i => i.Include(po => po.LineItems))
                .ToList();

            foreach (var po in purchaseOrderList)
            {
                var globalId = CommonHelper.GenerateGlobalId(po.Id, EntityType.CustomerPO);
                var activities = _activityRepository.Query(a => a.GlobalIdActivities.Any(g => g.GlobalId == globalId) && a.ActivityCode == eventCode,
                    null,
                    i => i.Include(a => a.GlobalIdActivities));

                foreach (var activity in activities)
                {
                    activity.ActivityDate = activityDate;
                    activity.Location = location;
                    activity.Remark = remark;
                    activity.Audit(AppConstant.SYSTEM_USERNAME);

                    foreach (var globalIdActivity in activity.GlobalIdActivities)
                    {
                        globalIdActivity.ActivityDate = activity.ActivityDate;
                        globalIdActivity.Location = activity.Location;
                        globalIdActivity.Remark = activity.Remark;
                        globalIdActivity.Audit(AppConstant.SYSTEM_USERNAME);
                    }
                }
            }   

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
