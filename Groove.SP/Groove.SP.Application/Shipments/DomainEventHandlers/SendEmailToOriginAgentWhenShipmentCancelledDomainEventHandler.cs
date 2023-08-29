using Groove.SP.Application.ApplicationBackgroundJob;
using Groove.SP.Application.ApplicationBackgroundJob.Services;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Notification.Interfaces;
using Groove.SP.Application.Notification.ViewModel;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Events;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Groove.SP.Application.Shipments.DomainEventHandlers
{
    public class SendEmailToOriginAgentWhenShipmentCancelledDomainEventHandler : INotificationHandler<ShipmentCancelledDomainEvent>
    {
        private readonly AppConfig _appConfig;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IRepository<POFulfillmentBookingRequestModel> _poFulfillmentBookingRequestRepository;
        private readonly INotificationService _notificationService;
        private readonly IQueuedBackgroundJobs _queuedBackgroundJobs;
        private readonly ICSFEApiClient _csfeApiClient;

        public SendEmailToOriginAgentWhenShipmentCancelledDomainEventHandler(IOptions<AppConfig> appConfig,
            IRepository<ShipmentModel> shipmentRepository,
            IQueuedBackgroundJobs queuedBackgroundJobs,
            ICSFEApiClient csfeApiClient,
            IRepository<POFulfillmentBookingRequestModel> poFulfillmentBookingRequestRepository,
            INotificationService notificationService)
        {
            _appConfig = appConfig.Value;
            _shipmentRepository = (IShipmentRepository)shipmentRepository;
            _queuedBackgroundJobs = queuedBackgroundJobs;
            _csfeApiClient = csfeApiClient;
            _poFulfillmentBookingRequestRepository = poFulfillmentBookingRequestRepository;
            _notificationService = notificationService;
        }

        public async Task Handle(ShipmentCancelledDomainEvent shipmentCancelledDomainEvent, CancellationToken cancellationToken)
        {
            var shipment = await _shipmentRepository.GetAsNoTrackingAsync(s => s.Id == shipmentCancelledDomainEvent.ShipmentId,
                null,
                i => i.Include(s => s.Contacts));

            var originAgent = shipment.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.OriginAgent, StringComparison.OrdinalIgnoreCase));
            var principal = shipment.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Principal, StringComparison.OrdinalIgnoreCase));
            
            // Send email to Origin Agent
            if (originAgent != null)
            {
                // Get customer org code
                string customerCode = string.Empty;
                if (principal is not null)
                {
                    var principalOrg = await _csfeApiClient.GetOrganizationByIdAsync(principal.OrganizationId);
                    if (principalOrg is not null)
                    {
                        customerCode = principalOrg.Code;
                    }
                }

                string mailTo = originAgent.ContactEmail;

                /**
                 * If shipment is Edison, system refer dbo.EmailNotifications of Agent Organization to determine the Notify Email and send Cancelling Shipment email to.
                 */

                var eShipment = await _poFulfillmentBookingRequestRepository.AnyAsync(x => x.POFulfillmentId == shipment.POFulfillmentId && x.Status == POFulfillmentBookingRequestStatus.Active);
                if (eShipment)
                {
                    var shipFromId = (await _csfeApiClient.GetLocationByDescriptionAsync(shipment.ShipFrom))?.Id ?? 0;
                    var customerId = principal?.OrganizationId ?? 0;
                    var emailNotification = await _csfeApiClient.GetEmailNotificationAsync(originAgent.OrganizationId, customerId, shipFromId);
                    if (emailNotification != null)
                    {
                        var splitEmails = emailNotification.Email.Split(Seperator.COMMA, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        if (splitEmails.Any())
                        {
                            mailTo = splitEmails[0];
                        }
                    }
                }
                var emailModel = new ShipmentCancelledEmailTemplateViewModel()
                {
                    Name = originAgent.ContactName,
                    ShipmentNo = shipment.ShipmentNo,
                    DetailPage = $"{_appConfig.ClientUrl}/shipments/{shipment.Id}",
                    SupportEmail = _appConfig.SupportEmail
                };
                _queuedBackgroundJobs.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"Shipment has been cancelled #{shipment.Id}", "Shipment_Cancelled",
                    emailModel, mailTo, $"Shipment Portal: [{customerCode}] Shipment has been cancelled ({shipment.ShipmentNo} - {shipment.ShipFrom})"));

                // Send push notification
                if (originAgent.OrganizationId != 0)
                {
                    await _notificationService.PushNotificationSilentAsync(originAgent.OrganizationId, new NotificationViewModel
                    {
                        MessageKey = $"~notification.msg.shipmentNo~ <span class=\"k-link\">{shipment.ShipmentNo}</span> ~notification.msg.hasBeenCancelled~.",
                        ReadUrl = $"/shipments/{shipment.Id}"
                    });
                }
            }
        }
    }
}