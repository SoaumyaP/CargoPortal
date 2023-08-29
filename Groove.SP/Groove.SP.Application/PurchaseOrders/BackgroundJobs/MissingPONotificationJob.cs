using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Notification.Interfaces;
using Groove.SP.Application.Notification.ViewModel;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.PurchaseOrders.BackgroundJobs
{
    /// <summary>
    /// To notify Supplier that the missing PO(s) of their booking has been imported into the system.
    /// </summary>
    public class MissingPONotificationJob
    {
        private readonly IPOFulfillmentRepository _poFulfillmentRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly INotificationService _notificationService;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly ICSFEApiClient _csfeApiClient;

        public MissingPONotificationJob(
           IPOFulfillmentRepository poFulfillmentRepository,
           IUserProfileRepository userProfileRepository,
           IPurchaseOrderRepository purchaseOrderRepository,
           INotificationService notificationService
            )
        {
            _poFulfillmentRepository = poFulfillmentRepository;
            _userProfileRepository = userProfileRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _notificationService = notificationService;
        }

        [JobDisplayName("Notify Supplier that the missing PO(s) has been imported")]
        public async Task ExecuteAsync(List<long> poIds)
        {
            var pos = await _purchaseOrderRepository.QueryAsNoTracking(c => poIds.Contains(c.Id), includes: c => c.Include(c => c.Contacts)).ToListAsync();

            var poffQuery = _poFulfillmentRepository.QueryAsNoTracking(c => c.FulfillmentType == FulfillmentType.PO && c.Stage == POFulfillmentStage.Draft,
                                                                       includes: c => c.Include(c => c.Contacts).Include(c => c.Orders));

            // List of POFulfillments which contain missing PO.
            var toNotifyPOFFs = new List<POFulfillmentModel>();

            foreach (var po in pos)
            {
                var principalContact = po.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Principal);
                if (principalContact == null)
                {
                    continue;
                }

                var poffs = await poffQuery.Where(
                    x => x.Contacts.Any(c => c.OrganizationRole == OrganizationRole.Principal && c.OrganizationId == principalContact.OrganizationId)
                    && x.Orders.Any(o => o.CustomerPONumber == po.PONumber && o.PurchaseOrderId == 0)
                    ).ToListAsync();

                toNotifyPOFFs.AddRange(poffs);
            }

            if (toNotifyPOFFs.Any())
            {
                toNotifyPOFFs = toNotifyPOFFs.DistinctBy(x => x.Id).ToList();
            }

            foreach (var poff in toNotifyPOFFs)
            {
                if (string.IsNullOrWhiteSpace(poff.CreatedBy))
                {
                    continue;
                }

                var userProfile = await _userProfileRepository.GetAsNoTrackingAsync(x => x.Username == poff.CreatedBy);
                if(userProfile is null)
                {
                    continue;
                }

                if (!userProfile.IsInternal && userProfile.OrganizationId is not null) // External, send to all users under organization
                {
                    await _notificationService.PushNotificationSilentAsync(userProfile.OrganizationId.Value, new NotificationViewModel
                    {
                        MessageKey = $"~notification.msg.missingPOOfBookingNo~ <span class=\"k-link\">{poff.Number}</span> ~notification.msg.hasBeenUploaded~.",
                        ReadUrl = $"/missing-po-fulfillments/view/{poff.Id}"
                    });
                }
                else
                {
                    await _notificationService.PushNotificationSilentAsync(new List<string> { userProfile.Username }, new NotificationViewModel
                    {
                        MessageKey = $"~notification.msg.missingPOOfBookingNo~ <span class=\"k-link\">{poff.Number}</span> ~notification.msg.hasBeenUploaded~.",
                        ReadUrl = $"/missing-po-fulfillments/view/{poff.Id}"
                    });
                }
            }
        }
    }
}
