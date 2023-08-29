using Groove.SP.Application.Notification.Interfaces;
using Groove.SP.Application.Notification.ViewModel;
using Groove.SP.Application.Provider.EmailSender;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Hangfire;
using Microsoft.Extensions.Options;
using RazorLight;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.PurchaseOrders.BackgroundJobs
{
    public class EmailToSupplierJob
    {
        private readonly AppConfig _appConfig;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;
        private readonly IRazorLightEngine _razorLight;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IUserProfileService _userProfileService;

        public EmailToSupplierJob(
            IPurchaseOrderService purchaseOrderService,
            INotificationService notificationService,
            IOptions<AppConfig> appConfig,
            IRazorLightEngine razorLight,
            ICSFEApiClient csfeApiClient,
            IEmailSender emailSender,
            IUserProfileService userProfileService)
        {
            _purchaseOrderService = purchaseOrderService;
            _notificationService = notificationService;
            _emailSender = emailSender;
            _appConfig = appConfig.Value;
            _razorLight = razorLight;
            _csfeApiClient = csfeApiClient;
            _userProfileService = userProfileService;
        }

        [JobDisplayName("Progress Check - Email notification to Supplier")]
        public async Task ExecuteAsync(long buyercomplianceId, AgentAssignmentMethodType agentAssignmentMethod, string principalCompanyName)
        {
            var pos = await _purchaseOrderService.GetPOEmailNotificationAsync(buyercomplianceId);
            var emails = _purchaseOrderService.CreatePOEmailNotificationAsync(buyercomplianceId, pos);

            if (emails.Count() > 0)
            {
                foreach (var email in emails)
                {
                    if (agentAssignmentMethod == AgentAssignmentMethodType.BySystem)
                    {
                        var emailNotification = await _csfeApiClient.GetEmailNotificationAsync(email.OriginAgentId ?? 0, email.customerId, email.shipFromId);
                        if (emailNotification != null)
                        {
                            var splitEmails = emailNotification.Email.Split(Seperator.COMMA, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            if (splitEmails.Any())
                            {
                                email.CC = splitEmails[0];
                            }
                        }
                    }

                    string emailSubject = $"Shipment Portal: {principalCompanyName} - Progress Check";
                    string emailBody = await _razorLight.CompileRenderAsync("ProgressCheck", email);
                    _emailSender.SendMail(email.To, emailSubject, emailBody, email.CC);


                    // Send push notification

                    // Each PO# will be displayed as one message because the list of PO# need progress check may be long
                    if (email.SupplierId != null)
                    {
                        foreach (var po in email.POs)
                        {
                            await _notificationService.PushNotificationSilentAsync(email.SupplierId.Value, new NotificationViewModel
                            {
                                MessageKey = $"~notification.msg.poNo~ <span class=\"k-link\">{po.PONumber}</span> ~notification.msg.needYourUpdateProgress~.",
                                ReadUrl = $"/purchase-orders/{po.Id}"
                            });
                        }
                    }
                }
            }
        }
    }
}
