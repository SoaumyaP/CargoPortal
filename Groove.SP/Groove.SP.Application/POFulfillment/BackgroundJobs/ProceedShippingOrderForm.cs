using Groove.SP.Application.Interfaces.Repositories;
using System;
using System.Threading.Tasks;
using Groove.SP.Core.Models;
using System.Linq;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Application.Provider.EmailSender;
using RazorLight;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Groove.SP.Infrastructure.RazorLight;
using Groove.SP.Infrastructure.DinkToPdf;
using Groove.SP.Application.Utilities;
using System.IO;
using Groove.SP.Application.Providers.BlobStorage;
using Groove.SP.Application.Attachment.ViewModels;
using Groove.SP.Application.Attachment.Services.Interfaces;
using static Groove.SP.Application.ApplicationBackgroundJob.SendMailBackgroundJobs;
using Groove.SP.Core.Entities;
using Groove.SP.Application.POFulfillment.ViewModels;
using System.ComponentModel.DataAnnotations;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Application.Users.ViewModels;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Groove.SP.Core.Data;
using System.ComponentModel;
using Hangfire;
using Groove.SP.Application.ApplicationBackgroundJob;
using AttachmentType = Groove.SP.Core.Models.AttachmentType;
using System.Text.RegularExpressions;
using Groove.SP.Application.Notification.ViewModel;
using Groove.SP.Application.Notification.Interfaces;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.POFulfillment.Services;

namespace Groove.SP.Application.POFulfillment
{
    public class ProceedShippingOrderForm 
    {
        private readonly AppConfig _appConfig;
        private readonly SendMailBackgroundJobs _sendMailBackgroundJobs;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly INotificationService _notificationService;
        private readonly IAttachmentService _attachmentService;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IPOFulfillmentRepository _poFulfillmentRepository;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IBlobStorage _blobStorage;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHtmlStringBuilder _htmlStringBuilder;
        private readonly IPdfGenerator _pdfGenerator;
        private readonly IUserProfileService _userProfileService;
        private readonly ISOFormGeneratorContext _soFormGeneratorContext;
        private readonly IDataQuery _dataQuery;


        public ProceedShippingOrderForm(
            IEmailSender emailSender,
            IRazorLightEngine razorLight,
            IOptions<AppConfig> appConfig,
            IAttachmentService attachmentService,
            ICSFEApiClient csfeApiClient,
            IUserProfileRepository userProfileRepository,
            IPOFulfillmentRepository poFulfillmentRepository,
            IPurchaseOrderRepository poRepository,
            IUserProfileService userProfileService,
            IBlobStorage blobStorage,
            IPdfGenerator pdfGenerator,
            IHtmlStringBuilder htmlStringBuilder,
            ISOFormGeneratorContext soFormGeneratorContext,
            IUnitOfWorkProvider unitOfWorkProvider,
            IDataQuery dataQuery,
            IEmailRecipientRepository emailRecipientRepository,
            INotificationService notificationService
            )
        {
            _appConfig = appConfig.Value;
            _attachmentService = attachmentService;
            _userProfileRepository = userProfileRepository;
            _poFulfillmentRepository = poFulfillmentRepository;
            _purchaseOrderRepository = poRepository;
            _notificationService = notificationService;
            _csfeApiClient = csfeApiClient;
            _blobStorage = blobStorage;
            _htmlStringBuilder = htmlStringBuilder;
            _pdfGenerator = pdfGenerator;
            _soFormGeneratorContext = soFormGeneratorContext;
            _unitOfWork = unitOfWorkProvider.CreateUnitOfWork();
            _userProfileService = userProfileService;
            _dataQuery = dataQuery;
            _sendMailBackgroundJobs = new SendMailBackgroundJobs(appConfig, emailSender, razorLight, emailRecipientRepository);
        }


        /// <summary>
        /// It is to generate SO form content then attach with current booking attachments to email.
        /// Please use BackgroudJob on Hangfire for background job inside as current method is running at background already.
        /// </summary>
        /// <param name="jobDescription">Job description for Hangfire</param>
        /// <param name="poFulfillmentId">Purchase Order Fulfillment Id</param>
        /// <param name="userName">Current user who takes an action</param>
        /// <returns></returns>

        [DisplayName("Proceed {0} for POFulfillment Id = {1}")]
        public async Task ExecuteAsync(string jobDescription, long poFulfillmentId, string userName, ShippingFormType formType, FulfillmentType? bookingType = FulfillmentType.PO)
        {
            var poFulfillment = await _poFulfillmentRepository.GetAsync(x => x.Id == poFulfillmentId, 
                null,
                x => x.Include(m => m.Contacts)
                .Include(m => m.Loads)
                .Include(m => m.Orders)
                .Include(m => m.Itineraries)
                .Include(m => m.BookingRequests)
                .Include(m => m.Shipments)
                );

            if (poFulfillment == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", poFulfillmentId)} not found!");
            }

            // Get organization details for contacts
            var getOrganizationTasks = new List<Task<Infrastructure.CSFE.Models.Organization>>();
            foreach (var contact in poFulfillment.Contacts)
            {
                // In case role = Pickup/Billing Party -> OrganizationId = 0, then no need to get Organization inform
                if (contact.OrganizationId <= 0)
                {
                    continue;
                }
                getOrganizationTasks.Add(_csfeApiClient.GetOrganizationByIdAsync(contact.OrganizationId));
            }
            var contactOrganizations = await Task.WhenAll(getOrganizationTasks).ConfigureAwait(false);
            // Remove organization not existing in Master data
            contactOrganizations = contactOrganizations.Where(x => x != null).ToArray();
            var purchaseOrderIds = poFulfillment.Orders.Select(x => x.PurchaseOrderId);
            var purchaseOrders = await _purchaseOrderRepository.Query(x => purchaseOrderIds.Contains(x.Id), null, x => x.Include(m => m.LineItems)).ToListAsync();

            // Get location details
            var getLocationTasks = new List<Task<Infrastructure.CSFE.Models.Location>>(); ;

            var locationIds = new List<long>
            {
                poFulfillment.ShipFrom,
                poFulfillment.ShipTo,
                poFulfillment.ReceiptPortId.Value,
                poFulfillment.DeliveryPortId.Value
            }.Distinct();

            foreach (var locationId in locationIds)
            {
                getLocationTasks.Add(_csfeApiClient.GetLocationByIdAsync(locationId));
            }
            var locations = await Task.WhenAll(getLocationTasks).ConfigureAwait(false);

            var carrierInfo = new Infrastructure.CSFE.Models.Carrier();
            if (poFulfillment.FulfillmentType == FulfillmentType.Bulk
                && poFulfillment.PreferredCarrier != default(int))
            {
                carrierInfo = await _csfeApiClient.GetCarrierByIdAsync(poFulfillment.PreferredCarrier);
            }

            //POFF Owner contact
            var owner = await _userProfileService.GetAsync(poFulfillment.CreatedBy);

            // Populate <article master> data for each order
            var productCodes = poFulfillment.Orders.Select(x => x.ProductCode).Distinct().ToArray();
            var customerOrgId = poFulfillment.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Principal, StringComparison.OrdinalIgnoreCase))?.OrganizationId;
            var customerOrgCode = contactOrganizations.FirstOrDefault(x => x.Id.Equals(customerOrgId))?.Code;

            var articleMasterInformations = new List<POLineItemArticleMasterViewModel>();

            if (productCodes != null && productCodes.Any() && customerOrgCode.HasValue())
            {
                articleMasterInformations = GetInformationFromArticleMaster(poFulfillment.Id, customerOrgCode, productCodes).ToList();
            }

            Infrastructure.CSFE.Models.Organization SupplierOrg = null;
            var supplierOrgId = (poFulfillment.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Supplier))?.OrganizationId;
            if (supplierOrgId != null)
            {
                SupplierOrg = contactOrganizations.FirstOrDefault(x => x.Id == supplierOrgId);
            }

            var generateFileType = SOFormGenerationFileType.Pdf; // Default is pdf
            if (SupplierOrg != null)
            {
                generateFileType = (SOFormGenerationFileType)SupplierOrg.SOFormGenerationFileType;
            }

            var fileExtension = "";
            switch (generateFileType)
            {
                case SOFormGenerationFileType.Excel:
                    fileExtension = ".xlsx";
                    break;
                case SOFormGenerationFileType.Pdf:
                    fileExtension = ".pdf";
                    break;
                default:
                    break;
            }

            // Generate file content
            var fileBytes = await GenerateShippingOrderFormContentAsync(formType, poFulfillment, owner, contactOrganizations.ToList(), locations.ToList(), carrierInfo, purchaseOrders.SelectMany(x => x.LineItems).ToList(), articleMasterInformations, generateFileType, _htmlStringBuilder, _pdfGenerator, _soFormGeneratorContext);
            var path = string.Empty;

            // Upload file into storage
            using (var stream = new MemoryStream(fileBytes))
            {
                path = await _blobStorage.PutBlobAsync("attachment", poFulfillment.Number, stream);
            }

            // Import file into the system (database)
            string attachmentReferenceNo = poFulfillment.Shipments.FirstOrDefault(x => x.Status.ToLower().Equals(StatusType.ACTIVE.ToLower()))?.ShipmentNo ?? "";
            if (string.IsNullOrEmpty(attachmentReferenceNo))
            {
                var theLastBookingRequest = poFulfillment.BookingRequests.FirstOrDefault(x => x.Status.Equals(POFulfillmentBookingRequestStatus.Active));
                attachmentReferenceNo = theLastBookingRequest?.BookingReferenceNumber ?? "";
            }
            var attachmentFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{(formType.Equals(ShippingFormType.Booking) ? "Booking" : "Shipping Order")} Form{fileExtension}";
            var attachment = new AttachmentViewModel()
            {
                AttachmentType = formType.Equals(ShippingFormType.Booking) ? AttachmentType.BOOKING_FORM : AttachmentType.SHIPPING_ORDER_FORM,
                ReferenceNo = attachmentReferenceNo,
                POFulfillmentId = poFulfillment.Id,
                Description = userName,
                UploadedBy = AppConstant.SYSTEM_USERNAME,
                UploadedDateTime = DateTime.UtcNow,
                BlobId = path,
                FileName = attachmentFileName
            };
            await _attachmentService.ImportAttachmentAsync(attachment);

            // Compose pdf file then attach into POFF
            var emailAttachment = new SPEmailAttachment
            {
                AttachmentContent = fileBytes,
                AttachmentName = attachmentFileName
            };

            // Send email to notify supplier/origin agent, only apply to 2 stages

            switch (formType)
            {
                case ShippingFormType.Booking:
                    await SendPOFulfillmentBookedEmailToSupplier(poFulfillment, emailAttachment, bookingType);
                    await SendBookedNotificationEmailToOriginAgent(poFulfillment, bookingType);
                    break;
                case ShippingFormType.ShippingOrder:
                    await SendBookingConfirmedNotificationToSuplier(poFulfillment, emailAttachment, bookingType);
                    break;
                default:
                    break;
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task SendPOFulfillmentBookedEmailToSupplier(POFulfillmentModel poff, SPEmailAttachment emailAttachment, FulfillmentType? bookingType)
        {
            if (poff == null)
            {
                throw new AppEntityNotFoundException($"Object is null or not found!");
            }
            var poffId = poff.Id;

            //collect all equipment types of the booking
            var equipmentTypes = poff.Loads?.Select(x => EnumHelper<EquipmentType>.GetDisplayDescription(x.EquipmentType)).ToList();

            var supplier = poff.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Supplier);
            var shipper = poff.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Shipper);
            var consignee = poff.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Consignee);

            var ownerInfo = await _userProfileService.GetAsync(poff.CreatedBy);
            var mailCCList = new List<string> { supplier.ContactEmail };
            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailWithCCAsync(
                 $"Booking has been submitted (Supplier) #{poffId}",
                "POFulfillment_Booked",
                new POFulfillmentEmailViewModel
                {
                    Name = ownerInfo.Name,
                    BookingRefNumber = poff.Number,
                    Shipper = shipper != null ? shipper.CompanyName : null,
                    Consignee = consignee != null ? consignee.CompanyName : null,
                    ShipFrom = poff.ShipFromName,
                    ShipTo = poff.ShipToName,
                    EquipmentTypes = equipmentTypes,
                    CargoReadyDate = poff.CargoReadyDate,
                    DetailPage = bookingType == FulfillmentType.Bulk ? $"{_appConfig.ClientUrl}/bulk-fulfillments/view/{poffId}" : $"{_appConfig.ClientUrl}/po-fulfillments/view/{poffId}",
                    SupportEmail = _appConfig.SupportEmail
                },
                ownerInfo.Email,
                mailCCList,
                $"Shipment Portal: Booking has been submitted ({poff.Number} - {poff.ShipFromName})",
                emailAttachment
                )
            );

            // Send push notification
            if (ownerInfo.OrganizationId != null && ownerInfo.OrganizationId != 0)
            {
                await _notificationService.PushNotificationSilentAsync(ownerInfo.OrganizationId.Value, new NotificationViewModel
                {
                    MessageKey = $"~notification.msg.bookingNo~ <span class=\"k-link\">{poff.Number}</span> ~notification.msg.hasBeenSubmitted~.",
                    ReadUrl = bookingType == FulfillmentType.Bulk ? $"/bulk-fulfillments/view/{poff.Id}" : $"/po-fulfillments/view/{poff.Id}"
                });
            }
        }

        /// <summary>
        /// It will send email to inform OrinalAgent, attaching all current POFF attachments (included Booking Form)
        /// </summary>
        /// <param name="poff"></param>
        /// <returns></returns>
        private async Task SendBookedNotificationEmailToOriginAgent(POFulfillmentModel poff, FulfillmentType? bookingType)
        {
            if (poff == null)
            {
                throw new AppEntityNotFoundException($"Object is null or not found!");
            }

            var poffId = poff.Id;

            //collect all equipment types of the booking
            var equipmentTypes = poff.Loads?.Select(x => EnumHelper<EquipmentType>.GetDisplayDescription(x.EquipmentType)).ToList();

            var originAgent = poff.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.OriginAgent);
            var shipper = poff.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Shipper);
            var consignee = poff.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Consignee);
            var customer = poff.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Principal);

            string mailTo = originAgent.ContactEmail;

            //to get recipient emails from dbo.EmailNotifications setup if any
            if (customer != null)
            {
                var emailNotifications = await _csfeApiClient.GetEmailNotificationsAsync(originAgent.OrganizationId, customer.OrganizationId, poff.ShipFrom);
                if (emailNotifications != null &&
                    emailNotifications.Any())
                {
                    //each email string can contain multiple email addresses which have been separated by comma.
                    var emailStrings = emailNotifications.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email);

                    //list of email addresses
                    List<string> emails = new();

                    foreach (var emailString in emailStrings)
                    {
                        emails.AddRange(emailString.Split(Seperator.COMMA, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
                    }
                    emails = emails.Distinct().ToList(); //avoid duplicate emails
                    mailTo = string.Join(Seperator.SEMICOLON, emails);
                }
            }

            var shipment = poff.Shipments.FirstOrDefault(x => x.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase));
            var bookingRequest = poff.BookingRequests.FirstOrDefault(x => x.Status == POFulfillmentBookingRequestStatus.Active);
            var referenceNumber = shipment != null ? shipment.ShipmentNo
                : (bookingRequest != null ? bookingRequest.BookingReferenceNumber : string.Empty);

            // Get attachments for the email
            var attachments = await _attachmentService.GetNewShippingEmailAttachmentsAsync(poff.Id);

            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync(
                $"Booking has been submitted (Origin Agent) #{poffId}",
                "Booking_BookedNotificationToOriginAgent",
                new POFulfillmentEmailViewModel
                {
                    Name = originAgent.ContactName,
                    BookingRefNumber = referenceNumber,
                    Shipper = shipper != null ? shipper.CompanyName : null,
                    Consignee = consignee != null ? consignee.CompanyName : null,
                    ShipFrom = poff.ShipFromName,
                    ShipTo = poff.ShipToName,
                    CargoReadyDate = poff.CargoReadyDate,
                    EquipmentTypes = equipmentTypes,
                    DetailPage = bookingType == FulfillmentType.Bulk ? $"{_appConfig.ClientUrl}/bulk-fulfillments/view/{poffId}" : $"{_appConfig.ClientUrl}/po-fulfillments/view/{poffId}",
                    SupportEmail = _appConfig.SupportEmail
                },
                mailTo,
                $"Shipment Portal: Booking has been submitted ({referenceNumber} - {poff.ShipFromName})",
                attachments.ToArray()
                )
            );
            
            // Send push notification
            if (originAgent.OrganizationId != 0)
            {
                await _notificationService.PushNotificationSilentAsync(originAgent.OrganizationId, new NotificationViewModel
                {
                    MessageKey = $"~notification.msg.bookingNo~ <span class=\"k-link\">{poff.Number}</span> ~notification.msg.hasBeenSubmitted~.",
                    ReadUrl = bookingType == FulfillmentType.Bulk ? $"/bulk-fulfillments/view/{poff.Id}" : $"/po-fulfillments/view/{poff.Id}"
                });
            }
        }

        private async Task SendBookingConfirmedNotificationToSuplier(POFulfillmentModel poFulfillment, SPEmailAttachment emailAttachment, FulfillmentType? bookingType)
        {
            if (poFulfillment == null)
            {
                throw new AppEntityNotFoundException($"Object is null or not found!");
            }

            //collect all equipment types of the booking
            var equipmentTypes = poFulfillment.Loads?.Select(x => EnumHelper<EquipmentType>.GetDisplayDescription(x.EquipmentType)).ToList();

            var supplier = poFulfillment.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Supplier, StringComparison.OrdinalIgnoreCase));
            var shipper = poFulfillment.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Shipper, StringComparison.OrdinalIgnoreCase));
            var consignee = poFulfillment.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Consignee, StringComparison.OrdinalIgnoreCase));

            long pushNotificationToOrgId = 0;
            string sendToEmail = poFulfillment.CreatedBy;
            string sendToName = "";
            List<string> mailCCList = new();

            if (!string.IsNullOrWhiteSpace(sendToEmail) && sendToEmail.IsValidEmailAddress())
            {
                var ownerInfo = await _userProfileService.GetAsync(sendToEmail);
                if (ownerInfo != null)
                {
                    sendToName = ownerInfo.Name;
                    pushNotificationToOrgId = ownerInfo.OrganizationId ?? 0;
                }

                mailCCList.Add(supplier.ContactEmail);
            }
            else
            {
                sendToEmail = supplier.ContactEmail;
                sendToName = supplier.ContactName;
                pushNotificationToOrgId = supplier.OrganizationId;
            }

            var emailModel = new POFulfillmentEmailViewModel
            {
                Name = sendToName,
                BookingRefNumber = poFulfillment.Number,
                Shipper = shipper?.CompanyName,
                Consignee = consignee?.CompanyName,
                ShipFrom = poFulfillment.ShipFromName,
                ShipTo = poFulfillment.ShipToName,
                CargoReadyDate = poFulfillment.CargoReadyDate,
                EquipmentTypes = equipmentTypes,
                DetailPage = bookingType == FulfillmentType.Bulk ? $"{_appConfig.ClientUrl}/bulk-fulfillments/view/{poFulfillment.Id}" : $"{_appConfig.ClientUrl}/po-fulfillments/view/{poFulfillment.Id}",
                SupportEmail = _appConfig.SupportEmail
            };

            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailWithCCAsync(
                $"Booking has been confirmed #{poFulfillment.Id}",
                "BookingConfirmedNotificationToSuplier",
                emailModel,
                sendToEmail,
                mailCCList,
                $"Shipment Portal: Booking has been confirmed ({poFulfillment.Number} - {poFulfillment.ShipFromName})",
                emailAttachment)
            );

            var notif = new NotificationViewModel
            {
                MessageKey = $"~notification.msg.bookingNo~ <span class=\"k-link\">{poFulfillment.Number}</span> ~notification.msg.hasBeenConfirmed~.",
                ReadUrl = bookingType == FulfillmentType.Bulk ? $"/bulk-fulfillments/view/{poFulfillment.Id}" : $"/po-fulfillments/view/{poFulfillment.Id}"
            };
            await _notificationService.PushNotificationSilentAsync(pushNotificationToOrgId, notif);
        }

        public static async Task<byte[]> GenerateShippingOrderFormContentAsync(ShippingFormType formType, POFulfillmentModel poFulfillment, UserProfileViewModel poffOwner, List<Infrastructure.CSFE.Models.Organization> contactOrganizations, List<Infrastructure.CSFE.Models.Location> locations, Infrastructure.CSFE.Models.Carrier carrierInfo, List<POLineItemModel> poLineItems, List<POLineItemArticleMasterViewModel> articleMasterInformations, SOFormGenerationFileType fileType, IHtmlStringBuilder htmlStringBuilder, IPdfGenerator pdfGenerator, ISOFormGeneratorContext soFormGeneratorContext)
        {
            // Convert POfilfillment to Shipping Order Form
            string soNumber = poFulfillment.Shipments.FirstOrDefault(x => x.Status.ToLower().Equals(StatusType.ACTIVE.ToLower()))?.ShipmentNo ?? "";
            var theLastBookingRequest = poFulfillment.BookingRequests.FirstOrDefault(x => x.Status.Equals(POFulfillmentBookingRequestStatus.Active));
            var bookingRequestReferenceNumber = theLastBookingRequest?.BookingReferenceNumber;

            if (string.IsNullOrEmpty(soNumber))
            {                
                soNumber = theLastBookingRequest?.SONumber ?? "";
            }

            var shippingOrderForm = new ShippingOrderFormViewModel()
            {
                FormType = formType,
                IsDangerousGoods = poFulfillment.IsContainDangerousGoods,
                SOFormStage = poFulfillment.Stage,
                PoffNumber = poFulfillment.Number,
                BookingRequestReferenceNumber = bookingRequestReferenceNumber,
                SoNumber = soNumber,
                ConfirmDate = DateTime.UtcNow,
                ExpectedShipDate = poFulfillment.ExpectedShipDate,
                PoffShipmentDate = poFulfillment.CargoReadyDate,
                PortOfLoading = locations.FirstOrDefault(x => x.Id == poFulfillment.ShipFrom)?.LocationDescription ?? "",
                PortOfDischarge = locations.FirstOrDefault(x => x.Id == poFulfillment.ShipTo)?.LocationDescription ?? "",
                PlaceOfReceipt = locations.FirstOrDefault(x => x.Id == poFulfillment.ReceiptPortId)?.LocationDescription ?? "",
                PlaceOfDelivery = locations.FirstOrDefault(x => x.Id == poFulfillment.DeliveryPortId)?.LocationDescription ?? "",
                Remarks = poFulfillment.Remarks,
                IsBatteryOrChemical = poFulfillment.IsBatteryOrChemical,
                IsCIQOrFumigation = poFulfillment.IsCIQOrFumigation,
                IsExportLicence = poFulfillment.IsExportLicence,
                IsNotifyPartyAsConsignee = poFulfillment.IsNotifyPartyAsConsignee,
                Incoterm = Enum.GetName(typeof(IncotermType), poFulfillment.Incoterm),
                ModeOfTransport = Enum.GetName(typeof(ModeOfTransportType), poFulfillment.ModeOfTransport),
                FulfillmentType = poFulfillment.FulfillmentType,
                VesselName = poFulfillment.VesselName,
                VoyageNo = poFulfillment.VoyageNo,
                CargoReadyDate = poFulfillment.CargoReadyDate,
                CarrierName = carrierInfo?.Name ?? string.Empty
            };

            var soOrders = (poFulfillment.Orders != null && poFulfillment.Orders.Any())
                            ? poFulfillment.Orders.Select(x =>
                                    new SOFormOrderModel
                                    {
                                        // Id is also used to obtain data from Master Data
                                        Id = x.Id,
                                        CustomerPONumber = x.CustomerPONumber,
                                        NoOfPieces = x.FulfillmentUnitQty,
                                        POFulfillmentId = x.POFulfillmentId,
                                        POLineItemId = x.POLineItemId,
                                        ProductCode = x.ProductCode,
                                        PurchaseOrderId = x.PurchaseOrderId,
                                        HsCode = x.HsCode,
                                        NoOfCartons = x.BookedPackage,
                                        Volume = x.Volume,
                                        Weight = x.GrossWeight,
                                        ChineseDescription = x.ChineseDescription,
                                        ShippingMarks = !string.IsNullOrEmpty(x.ShippingMarks) ? Regex.Replace(x.ShippingMarks, @"\u00A0", " ") : x.ShippingMarks,
                                        BookedPackage = x.BookedPackage,
                                        ProductName = x.ProductName
                                    }).ToList()
                            : null;
            var soProducts = (poLineItems != null && poLineItems.Any())
                            ? poLineItems.Select(x =>
                                    new SOFormProductModel
                                    {
                                        Id = x.Id,
                                        PurchaseOrderId = x.PurchaseOrderId,
                                        DescriptionOfGoods = !string.IsNullOrEmpty(x.DescriptionOfGoods) ? Regex.Replace(x.DescriptionOfGoods, @"\u00A0", " ") : x.DescriptionOfGoods,
                                    }).ToList()
                            : null;

            // Order list by CustomerPO number then Product code
            shippingOrderForm.Orders = (soOrders != null && soOrders.Any()) ? soOrders.OrderBy(x => x.CustomerPONumber).ThenBy(x => x.ProductCode).ToList() : null;
            shippingOrderForm.Products = soProducts;

            var soFirstItinerrary = poFulfillment.Itineraries.FirstOrDefault();
            var soOriginAgent = poFulfillment.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.OriginAgent, StringComparison.OrdinalIgnoreCase));
            var soShipper = poFulfillment.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Shipper, StringComparison.OrdinalIgnoreCase));
            var soSupplier = poFulfillment.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Supplier, StringComparison.OrdinalIgnoreCase));
            var soConsignee = poFulfillment.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Consignee, StringComparison.OrdinalIgnoreCase));
            var soNotify = poFulfillment.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.NotifyParty, StringComparison.OrdinalIgnoreCase));

            shippingOrderForm.FirstItinerrary = soFirstItinerrary != null ? new SOFormItineraryModel { CarrierName = soFirstItinerrary.CarrierName, VesselFlight = soFirstItinerrary.VesselFlight } : null;
            shippingOrderForm.Shipper = soShipper != null ? new SOFormContactModel { CompanyName = soOriginAgent.CompanyName } : null;
            shippingOrderForm.Supplier = soSupplier != null ? new SOFormContactModel { CompanyName = soOriginAgent.CompanyName } : null;
            shippingOrderForm.Consignee = soConsignee != null ? new SOFormContactModel { CompanyName = soOriginAgent.CompanyName } : null;
            shippingOrderForm.Notify = soNotify != null ? new SOFormContactModel { CompanyName = soOriginAgent.CompanyName } : null;
            shippingOrderForm.OriginAgent = soOriginAgent != null ? new SOFormContactModel { CompanyName = soOriginAgent.CompanyName } : null;


            foreach (var contact in poFulfillment.Contacts)
            {
                var organization = contactOrganizations?.FirstOrDefault(x => x.Id.Equals(contact.OrganizationId));

                var soContact = new SOFormContactModel { CompanyName = contact.CompanyName };
                var soOrganization = new SOFormOrganizationModel
                {
                    Address = contact.Address,
                    AddressLine2 = contact.AddressLine2,
                    AddressLine3 = contact.AddressLine3,
                    AddressLine4 = contact.AddressLine4,
                    Location = organization?.Location
                };

                switch (contact.OrganizationRole)
                {
                    case OrganizationRole.Shipper:
                        shippingOrderForm.Shipper = soContact;
                        shippingOrderForm.ShipperOrganization = soOrganization;
                        break;
                    case OrganizationRole.Supplier:
                        shippingOrderForm.Supplier = soContact;
                        shippingOrderForm.SupplierOrganization = soOrganization;
                        break;
                    case OrganizationRole.OriginAgent:
                        shippingOrderForm.OriginAgent = soContact;
                        shippingOrderForm.OriginAgentOrganization = soOrganization;
                        break;
                    case OrganizationRole.Consignee:
                        shippingOrderForm.Consignee = soContact;
                        shippingOrderForm.ConsigneeOrganization = soOrganization;
                        break;
                    case OrganizationRole.NotifyParty:
                        shippingOrderForm.Notify = soContact;
                        shippingOrderForm.NotifyOrganization = soOrganization;
                        break;
                    default:
                        break;
                }
            }

            // Assign value for Pickup Address            
            var pickupContact = poFulfillment.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Pickup);
            if (pickupContact != null)
            {
                var soPickup = shippingOrderForm.PickupAddress = new SOFormContactModel();
                soPickup.CompanyName = pickupContact.CompanyName;
                soPickup.ContactName = pickupContact.ContactName;
                soPickup.ContactNumber = pickupContact.ContactNumber;
                soPickup.EmailAddress = pickupContact.ContactEmail;
                shippingOrderForm.PickupOrganization = new SOFormOrganizationModel
                {
                    Address = pickupContact.Address,
                    AddressLine2 = pickupContact.AddressLine2,
                    AddressLine3 = pickupContact.AddressLine3,
                    AddressLine4 = pickupContact.AddressLine4
                };
            }

            // Assign value for Billing Party            
            var billingContact = poFulfillment.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.BillingParty);
            if (billingContact != null)
            {
                var soBilling = shippingOrderForm.BillingAddress = new SOFormContactModel();
                soBilling.CompanyName = billingContact.CompanyName;
                soBilling.ContactName = billingContact.ContactName;
                soBilling.ContactNumber = billingContact.ContactNumber;
                soBilling.EmailAddress = billingContact.ContactEmail;
                shippingOrderForm.BillingPartyOrganization = new SOFormOrganizationModel
                {
                    Address = billingContact.Address,
                    AddressLine2 = billingContact.AddressLine2,
                    AddressLine3 = billingContact.AddressLine3,
                    AddressLine4 = billingContact.AddressLine4
                };
            }

            // Assign value for Booking Owner
            shippingOrderForm.Owner = new SOFormContactModel
            {
                ContactNumber = poffOwner?.Phone,
                EmailAddress = poffOwner?.Email
            };

            if (poFulfillment.MovementType == MovementType.CY_CY || poFulfillment.MovementType == MovementType.CY_CFS)
            {
                var equipmentTypes = poFulfillment.Loads.Select(x => x.EquipmentType).Distinct().ToList(); 
                if(equipmentTypes != null && equipmentTypes.Any())
                {
                    var soBookingContainers = "";
                    // format = 2 x 20GP, 1,000 x 40RF
                    var tmp = equipmentTypes.Select(x => $"{poFulfillment.Loads.Count(y => y.EquipmentType == x):#,##0} x {x.GetAttributeValue<DisplayAttribute, string>( y => y.ShortName)}" );
                    soBookingContainers = string.Join(", ", tmp);
                    shippingOrderForm.BookingContainers = soBookingContainers;
                }
                
            }
            else
            {
                shippingOrderForm.BookingContainers = "";
            }

            // Append values from Article Master to Order

            foreach (var item in articleMasterInformations)
            {
                var order = shippingOrderForm.Orders.FirstOrDefault(x => x.Id == item.Id);
                if(order != null)
                {
                    order.InnerQuantity = item.InnerQuantity;
                    order.OuterQuantity = item.OuterQuantity;
                }

            }

            switch (fileType)
            {
                case SOFormGenerationFileType.Pdf:
                    soFormGeneratorContext.SetStrategy(new SOFormPdfGenerator(htmlStringBuilder, pdfGenerator));
                    break;
                case SOFormGenerationFileType.Excel:
                    soFormGeneratorContext.SetStrategy(new SOFormExcelGenerator());
                    break;
                default:
                    break;
            }

            var fileBytes = await soFormGeneratorContext.ExecuteStrategyAsync(shippingOrderForm);

            return fileBytes;
        }

        private IEnumerable<POLineItemArticleMasterViewModel> GetInformationFromArticleMaster(long poffId, string customerOrgCode, string[] productCodes)
        {
            var productCodesString = string.Join(",", productCodes);

            var sql = @"SELECT item.Id, am.InnerQuantity, am.OuterQuantity
                        FROM POFulfillmentOrders item JOIN ArticleMaster am WITH (NOLOCK) ON item.ProductCode = TRIM(am.ItemNo) 
                        WHERE item.POFulfillmentId = @poffId AND am.CompanyCode = @companyCode AND item.ProductCode IN (SELECT [VALUE] FROM [dbo].[fn_SplitStringToTable] (@productCodes, ','))";

            var filterParameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@poffId",
                        Value = poffId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@companyCode",
                        Value = customerOrgCode,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@productCodes",
                        Value = productCodesString,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                };

            IEnumerable<POLineItemArticleMasterViewModel> mappingCallback(DbDataReader reader)
            {
                var mappedData = new List<POLineItemArticleMasterViewModel>();

                while (reader.Read())
                {
                    var newRow = new POLineItemArticleMasterViewModel
                    {
                        Id = (long)reader["Id"],
                        InnerQuantity = reader["InnerQuantity"] as int?,
                        OuterQuantity = reader["OuterQuantity"] as int?,
                    };
                    mappedData.Add(newRow);
                }

                return mappedData;
            }

            return _dataQuery.GetDataBySql(sql, mappingCallback, filterParameters.ToArray());
        }
    }
    public enum ShippingFormType
    {
        Booking = 1,
        ShippingOrder
    }
}
