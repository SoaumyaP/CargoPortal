using Groove.SP.Application.ApplicationBackgroundJob;
using Groove.SP.Application.BuyerComplianceService.Services.Interfaces;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Notification.Interfaces;
using Groove.SP.Application.Notification.ViewModel;
using Groove.SP.Application.Provider.EmailSender;
using Groove.SP.Application.WarehouseFulfillment.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using RazorLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.WarehouseFulfillment.BackgroundJobs
{
    public class WarehouseApprovalJob
    {
        private readonly IPOFulfillmentRepository _poFulfillmentRepository;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IRepository<BuyerComplianceModel> _buyerComplianceRepository;
        private readonly IEmailSender _emailSender;
        private readonly IRazorLightEngine _razorLight;
        private readonly INotificationService _notificationService;

        public WarehouseApprovalJob(
            IPOFulfillmentRepository poFulfillmentRepository,
            ICSFEApiClient csfeApiClient,
            IEmailSender emailSender,
            IRazorLightEngine razorLight,
            INotificationService notificationService,
            IRepository<BuyerComplianceModel> buyerComplianceRepository)
        {
            _poFulfillmentRepository = poFulfillmentRepository;
            _csfeApiClient = csfeApiClient;
            _buyerComplianceRepository = buyerComplianceRepository;
            _emailSender = emailSender;
            _razorLight = razorLight;
            _notificationService = notificationService;
        }

        [DisplayName("Proceed approved for Warehouse Booking Id = {0}")]
        public async Task ExecuteAsync(long bookingId)
        {
            var warehouseBooking = await _poFulfillmentRepository.QueryAsNoTracking(c => c.Id == bookingId, null, s => s.Include(a => a.Orders).Include(d => d.Contacts)).SingleOrDefaultAsync();
            var principalContact = warehouseBooking.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Principal);
            var supplierContact = warehouseBooking.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Supplier);
            var warehouseAssignments = await _csfeApiClient.GetWarehouseAssignmentsByOrgIdAsync(principalContact.OrganizationId);

            var eta = "";
            var supplierName = supplierContact?.CompanyName ?? "";
            var emailsTo = new List<string>();
            var emailsCC = new List<string>();

            var emailViewModel = new WarehouseApprovalFormViewModel()
            {
                CustomerName = principalContact?.CompanyName
            };

            if (warehouseBooking.ETADestination.HasValue)
            {
                eta = $" ETA {warehouseBooking.ETADestination.Value.ToString("MM")} {warehouseBooking.ETADestination.Value.ToString("dd")}";
            }

            var warehouseLocation = warehouseAssignments.FirstOrDefault()?.WarehouseLocation;

            emailViewModel.WarehouseName = warehouseLocation?.Name;

            var buyerCompliance = await _buyerComplianceRepository.GetAsNoTrackingAsync(c =>
                c.OrganizationId == principalContact.OrganizationId, null,
                c => c.Include(s => s.EmailSettings));

            var emailSetting = buyerCompliance.EmailSettings.FirstOrDefault(c => c.EmailType == EmailSettingType.BookingApproved);

            if (emailSetting.DefaultSendTo == true)
            {
                var warehouseEmails = new List<string>() { warehouseLocation?.ContactEmail ?? string.Empty };
                var settingEmails = emailSetting.SendTo?.Split(",").ToList();
                emailsTo = warehouseEmails?.Concat(settingEmails ?? new List<string>()).Select(c => c.Trim()).Distinct().ToList();
            }
            else
            {
                emailsTo = emailSetting.SendTo?.Split(",").Select(c => c.Trim()).Distinct().ToList();
            }

            if (!string.IsNullOrEmpty(emailSetting.CC))
            {
                emailsCC = emailSetting.CC.Split(",").Select(c => c.Trim()).Distinct().ToList();
            }

            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailWithCCAsync
            ($"Warehouse Booking has been approved #{bookingId}",
            "WarehouseBooking_Approved", emailViewModel, string.Join(";", emailsTo),
            emailsCC,
            $"{emailViewModel.CustomerName} - Delivery Approval{eta} - Supplier {supplierName}")
            );

            // Send push notification
            if (warehouseLocation?.OrganizationId != 0)
            {
                await _notificationService.PushNotificationSilentAsync(warehouseLocation.OrganizationId, new NotificationViewModel
                {
                    MessageKey = $"~notification.msg.bookingNo~ <span class=\"k-link\">{warehouseBooking.Number}</span> ~notification.msg.hasBeenApproved~.",
                    ReadUrl = $"/warehouse-bookings/view/{warehouseBooking.Id}"
                });
            }
        }
    }
}
