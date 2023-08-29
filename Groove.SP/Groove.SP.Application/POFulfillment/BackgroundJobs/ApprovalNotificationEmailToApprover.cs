using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RazorLight;
using System;
using System.Threading.Tasks;

using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Provider.EmailSender;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using System.ComponentModel;
using Groove.SP.Application.ApplicationBackgroundJob;
using System.Linq;
using System.Collections.Generic;
using Groove.SP.Core.Entities;
using Groove.SP.Application.EmailSetting.Services;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.Notification.ViewModel;
using Groove.SP.Application.Notification.Interfaces;

namespace Groove.SP.Application.POFulfillment
{
    public class ApprovalNotificationEmailToApprover
    {
        private readonly AppConfig _appConfig;
        private readonly SendMailBackgroundJobs _sendMailBackgroundJobs;
        private readonly IBuyerApprovalRepository _buyerApprovalRepository;
        private readonly IRepository<BuyerComplianceModel> _buyerComplianceRepository;
        public readonly ICSFEApiClient _csfeApiClient;
        private readonly INotificationService _notificationService;

        public ApprovalNotificationEmailToApprover(IEmailSender emailSender,
            IRazorLightEngine razorLight,
            IOptions<AppConfig> appConfig,
            IBuyerApprovalRepository buyerApprovalRepository,
            ICSFEApiClient csfeApiClient,
            IEmailRecipientRepository emailRecipientRepository,
            IRepository<BuyerComplianceModel> buyerComplianceRepository,
            INotificationService notificationService)
        {
            _sendMailBackgroundJobs = new SendMailBackgroundJobs(appConfig, emailSender, razorLight, emailRecipientRepository);
            _appConfig = appConfig.Value;
            _buyerApprovalRepository = buyerApprovalRepository;
            _buyerComplianceRepository = buyerComplianceRepository;
            _csfeApiClient = csfeApiClient;
            _notificationService = notificationService;
        }

        [DisplayName("Send Mail: Booking is Pending for Approval #{0}")]
        public async Task ExecuteAsync(long pendingApprovalId, int delayedHours)
        {
            var pendingApproval = await _buyerApprovalRepository.GetAsync(b => b.Id == pendingApprovalId,
                null,
                x
                => x.Include(m => m.POFulfillment)
                .ThenInclude(m => m.Contacts));

            if (pendingApproval == null)
            {
                throw new AppEntityNotFoundException($"Pending Approval with the id {string.Join(", ", pendingApprovalId)} not found!");
            }

            if (pendingApproval.Stage != BuyerApprovalStage.Pending)
            {
                return;
            }

            var pricipalOrgId = pendingApproval.POFulfillment?.Contacts?.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Principal)?.OrganizationId;
            
            BuyerComplianceModel buyerCompliance = null;

            if (pricipalOrgId != null && pricipalOrgId > 0)
            {
                buyerCompliance = await _buyerComplianceRepository.GetAsNoTrackingAsync(x => x.OrganizationId == pricipalOrgId, includes: i => i.Include(x => x.EmailSettings));
            }
            string name = string.Empty;

            // default send to / cc
            List<string> mailToList = new();
            List<string> mailCCList = new();
            if (pendingApproval.ApproverSetting == ApproverSettingType.AnyoneInOrganization)
            {
                var approverOrg = await _csfeApiClient.GetOrganizationByIdAsync(pendingApproval.ApproverOrgId.Value);
                mailToList.Add(approverOrg.ContactEmail);
                name = approverOrg.ContactName;
            }
            else
            {
                var approverUsers = pendingApproval.ApproverUser.Split(Seperator.COMMA, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                mailToList.AddRange(approverUsers);
            }

            try
            {
                var supportEmail = _appConfig.SupportEmail;
                var templateEmailName = "ApprovalNotificationEmailToApprover";
                var emailSubject = $"Shipment Portal: Booking is Pending for Approval ({pendingApproval.POFulfillment.Number} - {pendingApproval.POFulfillment.ShipFromName})";
                var warehouseName = "";
                var warehouseAddress = "";
                var eta = "";
                var contactName = "";
                var contactNumber = "";
                var ContactEmail = "";
                var City = "";
                var Country = "";

                var isWarehouseBooking = pendingApproval.POFulfillment.FulfillmentType == FulfillmentType.Warehouse;

                if (isWarehouseBooking)
                {
                    templateEmailName = "ApprovalNotificationEmailToApproverWarehouse";
                    name = pendingApproval.POFulfillment?.Contacts?.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Principal)?.CompanyName;
                    var supplier = pendingApproval.POFulfillment.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Supplier)?.CompanyName ?? "";
                    emailSubject = $"{name} -  Delivery Approval";

                    if (pendingApproval.POFulfillment.ETADestination.HasValue)
                    {
                        eta = $" ETA {pendingApproval.POFulfillment.ETADestination.Value.ToString("MM")} {pendingApproval.POFulfillment.ETADestination.Value.ToString("dd")}";
                        emailSubject = emailSubject + eta;
                        eta = "," + eta;
                    }

                    emailSubject = emailSubject + $" - Supplier {supplier}";

                    // support email = default from system
                    if (pricipalOrgId != null && pricipalOrgId.Value > 0)
                    {
                        var warehouseAssignments = await _csfeApiClient.GetWarehouseAssignmentsByOrgIdAsync(pricipalOrgId.Value);
                        if (warehouseAssignments != null && warehouseAssignments.Any())
                        {
                            // Try to get from warehouse assignments if possible
                            supportEmail = warehouseAssignments.FirstOrDefault()?.ContactEmail;
                            warehouseName = warehouseAssignments.FirstOrDefault()?.WarehouseLocation?.Name;
                            contactName = warehouseAssignments.FirstOrDefault()?.ContactPerson;
                            contactNumber = warehouseAssignments.FirstOrDefault()?.ContactPhone;
                            ContactEmail = warehouseAssignments.FirstOrDefault()?.ContactEmail;

                            City = warehouseAssignments.FirstOrDefault()?.WarehouseLocation?.Location?.LocationDescription;
                            Country = warehouseAssignments.FirstOrDefault()?.WarehouseLocation?.Location?.Country?.Name;
                            warehouseAddress = warehouseAssignments.FirstOrDefault()?.WarehouseLocation?.CombineAddressLine();
                        }
                    }
                }

                #region Check Compliance Email Setting to define recipients.
                const char JOIN_EMAIL_BY = ';';
                if (buyerCompliance?.ServiceType == BuyerComplianceServiceType.WareHouse)
                {
                    var result = EmailSettingService.GetReceipientsFromBuyerCompliance(buyerCompliance.EmailSettings?.ToList(), EmailSettingType.BookingApproval, mailToList, mailCCList);
                    mailToList = result["sendTo"];
                    mailCCList = result["cc"];
                }
                #endregion
                var mailTo = string.Join(JOIN_EMAIL_BY, mailToList);
                await _sendMailBackgroundJobs.SendMailWithCCAsync($"Send Mail: Booking is Pending for Approval #{pendingApproval.POFulfillment.Id}", templateEmailName,
                       new PendingBookingEmailTemplateViewModel()
                       {
                           Name = name,
                           BookingRefNumber = pendingApproval.POFulfillment.Number,
                           DetailPage = $"{_appConfig.ClientUrl}/buyer-approvals/{pendingApproval.Id}",
                           SupportEmail = supportEmail,
                           Eta = eta,
                           WarehouseName = warehouseName,
                           WarehouseAddress = warehouseAddress,
                           City = City,
                           Country = Country,
                           ContactName = contactName,
                           ContactNumber = contactNumber,
                           ContactEmail = ContactEmail,
                       },
                      mailTo,
                      mailCCList,
                      emailSubject);

                // Send push notification
                if (pricipalOrgId != null && pricipalOrgId > 0)
                {
                    var readUrl = isWarehouseBooking ? $"/warehouse-bookings/view/{pendingApproval.POFulfillment.Id}"
                        : $"/po-fulfillments/view/{pendingApproval.POFulfillment.Id}";

                    await _notificationService.PushNotificationSilentAsync(pricipalOrgId.Value, new NotificationViewModel
                    {
                        MessageKey = $"~notification.msg.bookingNo~ <span class=\"k-link\">{pendingApproval.POFulfillment.Number}</span> ~notification.msg.needsYourApproval~.",
                        ReadUrl = readUrl
                    });
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            // Schedule the job next time
            BackgroundJob.Schedule<ApprovalNotificationEmailToApprover>(j => j.ExecuteAsync(pendingApproval.Id, delayedHours), TimeSpan.FromHours(delayedHours));
        }
    }
}