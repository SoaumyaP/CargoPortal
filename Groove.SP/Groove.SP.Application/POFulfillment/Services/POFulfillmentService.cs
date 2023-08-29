using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.BuyerComplianceService.Services.Interfaces;
using Groove.SP.Application.BuyerApproval.Services.Interfaces;
using Microsoft.Extensions.Options;
using Groove.SP.Application.BookingValidationLog.Services.Interfaces;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Application.Utilities;
using Groove.SP.Application.Attachment.Services.Interfaces;
using Groove.SP.Application.Attachment.ViewModels;
using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Event = Groove.SP.Core.Models.Event;
using Groove.SP.Application.Itinerary.Services.Interfaces;
using OrganizationRole = Groove.SP.Core.Models.OrganizationRole;
using Groove.SP.Application.Translations.Providers.Interfaces;
using Groove.SP.Application.Translations.Helpers;
using Groove.SP.Application.Providers.BlobStorage;
using Groove.SP.Infrastructure.DinkToPdf;
using Groove.SP.Infrastructure.RazorLight;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data;
using Groove.SP.Core.Data;
using Groove.SP.Application.POFulfillmentContact.Mappers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Groove.SP.Application.ApplicationBackgroundJob;
using Groove.SP.Application.ApplicationBackgroundJob.Services;
using Groove.SP.Application.OrganizationPreference.Services.Interfaces;
using Groove.SP.Application.OrganizationPreference.ViewModels;
using System.Runtime.Serialization;
using Groove.SP.Core.Events;
using Groove.SP.Application.Notification.Interfaces;
using Groove.SP.Application.Notification.ViewModel;
using Groove.SP.Application.Shipments.ViewModels;

namespace Groove.SP.Application.POFulfillment.Services
{
    public partial class POFulfillmentService : ServiceBase<POFulfillmentModel, POFulfillmentViewModel>, IPOFulfillmentService
    {
        private readonly IBookingValidationLogService _bookingValidationLogService;
        private readonly IUserProfileService _userProfileService;
        private readonly IBuyerComplianceService _buyerComplianceService;
        private readonly IAttachmentService _attachmentService;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IActivityService _activityService;
        private readonly INotificationService _notificationService;
        private readonly IGlobalIdActivityRepository _globalIdActivityRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly ICSFEApiClient _csfeApiClient;

        private readonly IRepository<POLineItemModel> _poLineItemRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly AppConfig _appConfig;
        private readonly IBuyerApprovalRepository _buyerApprovalRepository;
        private readonly IPOFulfillmentOrderRepository _poFulfillmentOrderRepository;
        private readonly IRepository<ShipmentModel> _shipmentRepository;
        private readonly IRepository<ConsignmentModel> _consignmentRepository;
        private readonly IContainerRepository _containerRepository;
        private readonly IRepository<ShipmentLoadModel> _shipmentLoadRepository;
        private readonly IItineraryService _itineraryService;
        private readonly ITranslationProvider _translation;
        private readonly IRepository<POFulfillmentContactModel> _poFulfillmentContactRepository;
        private readonly IRepository<POFulfillmentBookingRequestModel> _poFulfillmentBookingRequestRepository;
        private readonly IEdiSonBookingService _ediSonBookingService;
        private readonly IPdfGenerator _pdfGenerator;
        private readonly IBlobStorage _blobStorage;
        private readonly IHtmlStringBuilder _htmlStringBuilder;
        private readonly IPOFulfillmentRepository _poFulfillmentRepository;
        private readonly IDataQuery _dataQuery;
        private readonly IServiceProvider _services;
        private readonly TelemetryConfig _telemetryConfig;
        private readonly IPOFulfillmentAllocatedOrderRepository _poFulfillmentAllocatedOrderRepository;
        private readonly IQueuedBackgroundJobs _queuedBackgroundJobs;
        private readonly IOrganizationPreferenceService _organizationPreferenceService;
        private readonly IRepository<CargoDetailModel> _cargoDetailRepository;
        private readonly IRepository<IntegrationLogModel> _integrationLogRepository;

        /// <summary>
        /// Please do not use it directly. Use TelemetryClient instead.
        /// </summary>
        private TelemetryClient _telemetryClient;
        public TelemetryClient TelemetryClient
        {
            get
            {
                if (_telemetryClient == null)
                {
                    var key = _telemetryConfig.Key;
                    var telConfig = new TelemetryConfiguration(key);
                    _telemetryClient = new TelemetryClient(telConfig);
                }
                return _telemetryClient;
            }
        }

        /// <summary>
        /// It is lazy service injection.
        /// Please do not use it directly, use BuyerApprovalService instead.
        /// </summary>
        private IBuyerApprovalService _buyerApprovalServiceLazy;
        public IBuyerApprovalService BuyerApprovalService
        {
            get
            {
                if (_buyerApprovalServiceLazy == null)
                {
                    _buyerApprovalServiceLazy = _services.GetRequiredService<IBuyerApprovalService>();
                }
                return _buyerApprovalServiceLazy;
            }
        }

        public POFulfillmentService(IUnitOfWorkProvider unitOfWorkProvider,
            IRepository<POFulfillmentModel> poFulfillmentRepository,
            IBuyerComplianceService buyerComplianceService,
            IRepository<POLineItemModel> poLineItemRepository,
            IBookingValidationLogService bookingValidationLogService,
            IUserProfileService userProfileService,
            ICSFEApiClient csfeApiClient,
            IAttachmentService attachmentService,
            IActivityService activityService,
            INotificationService notificationService,
            IOptions<AppConfig> appConfig,
            IBuyerApprovalRepository buyerApprovalRepository,
            IPOFulfillmentOrderRepository poFulfillmentOrderRepository,
            IRepository<ShipmentModel> shipmentRepository,
            IRepository<ConsignmentModel> consignmentRepository,
            IContainerRepository containerRepository,
            IRepository<ShipmentLoadModel> shipmentLoadRepository,
            IItineraryService itineraryService,
            ITranslationProvider translationProvider,
            IEdiSonBookingService ediSonBookingService,
            IPdfGenerator pdfGenerator,
            IBlobStorage blobStorage,
            IHtmlStringBuilder htmlStringBuilder,
            IDataQuery dataQuery,
            IServiceProvider services,
            IOptions<TelemetryConfig> telemetryConfig,
            IPOFulfillmentAllocatedOrderRepository poFulfillmentAllocatedOrderRepository,
            IQueuedBackgroundJobs appBackgroundJob,
            IOrganizationPreferenceService organizationPreferenceService,
            IRepository<CargoDetailModel> cargoDetailRepository,
            IRepository<IntegrationLogModel> integrationLogRepository
            ) : base(unitOfWorkProvider)
        {
            _telemetryConfig = telemetryConfig.Value;
            _services = services;
            _buyerComplianceService = buyerComplianceService;
            _poLineItemRepository = poLineItemRepository;
            _appConfig = appConfig.Value;
            _bookingValidationLogService = bookingValidationLogService;
            _userProfileService = userProfileService;
            _csfeApiClient = csfeApiClient;
            _attachmentService = attachmentService;
            _notificationService = notificationService;
            _attachmentRepository = (IAttachmentRepository)UnitOfWork.GetRepository<AttachmentModel>();
            _activityService = activityService;
            _purchaseOrderRepository = (IPurchaseOrderRepository)UnitOfWork.GetRepository<PurchaseOrderModel>();
            _buyerApprovalRepository = buyerApprovalRepository;
            _poFulfillmentOrderRepository = poFulfillmentOrderRepository;
            _globalIdActivityRepository = (IGlobalIdActivityRepository)UnitOfWork.GetRepository<GlobalIdActivityModel>();
            _shipmentRepository = UnitOfWork.GetRepository<ShipmentModel>();
            _consignmentRepository = consignmentRepository;
            _containerRepository = containerRepository;
            _shipmentLoadRepository = shipmentLoadRepository;
            _itineraryService = itineraryService;
            _translation = translationProvider;
            _poFulfillmentContactRepository = UnitOfWork.GetRepository<POFulfillmentContactModel>();
            _poFulfillmentBookingRequestRepository = UnitOfWork.GetRepository<POFulfillmentBookingRequestModel>();
            _ediSonBookingService = ediSonBookingService;
            _pdfGenerator = pdfGenerator;
            _blobStorage = blobStorage;
            _htmlStringBuilder = htmlStringBuilder;
            _poFulfillmentRepository = (IPOFulfillmentRepository)Repository;
            _dataQuery = dataQuery;
            _poFulfillmentAllocatedOrderRepository = poFulfillmentAllocatedOrderRepository;
            _queuedBackgroundJobs = appBackgroundJob;
            _organizationPreferenceService = organizationPreferenceService;
            _cargoDetailRepository = cargoDetailRepository;
            _activityRepository = (IActivityRepository)UnitOfWork.GetRepository<ActivityModel>();
            _integrationLogRepository = integrationLogRepository;
        }
        protected override Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> FullIncludeProperties => x
            => x.Include(m => m.Contacts)
                .Include(m => m.Loads)
                .ThenInclude(i => i.Details)
                .Include(m => m.Orders)
                .Include(m => m.CargoDetails)
                .Include(m => m.BookingRequests)
                .Include(m => m.BuyerApprovals)
                .Include(m => m.Itineraries);

        protected override IDictionary<string, string> SortMap => new Dictionary<string, string>() {
            { "statusName", "status" },
            { "stageName", "stage" }
        };

        #region private methods
        private async Task<string> GenerateBookingNumber(string customerPrefix, long customerOrgId, DateTime createdDate)
        {
            if (string.IsNullOrEmpty(customerPrefix))
            {
                throw new ApplicationException("Customer prefix is not valid to create booking!");
            }
            var nextSequenceValue = await _poFulfillmentRepository.GetNextPOFFSequenceValueAsync(customerOrgId);
            return $"{customerPrefix}{createdDate.ToString("yyMM")}{nextSequenceValue}";
        }

        private async Task<string> GenerateBookingLoadNumber(DateTime createdDate)
        {
            var nextSequenceValue = await _poFulfillmentRepository.GetNextPOFFLoadSequenceValueAsync();
            return $"LD{createdDate.ToString("yyMM")}{nextSequenceValue}";
        }

        /// <summary>
        /// If POFF has a active shipment, Synchronize Container info to Shipment Container
        /// </summary>
        /// <param name="poff"></param>
        private static void SyncContainerInfoFromPOFFLoad(POFulfillmentModel poff)
        {
            var activeShipment = poff.Shipments.FirstOrDefault(x => x.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase));
            if (activeShipment != null)
            {
                foreach (var load in activeShipment.ShipmentLoads)
                {
                    var poffLoad = poff.Loads.FirstOrDefault(x => x.LoadReferenceNumber == load.Container.LoadPlanRefNo);
                    if (poffLoad != null)
                    {
                        if (!String.IsNullOrEmpty(poffLoad.ContainerNumber))
                        {
                            load.Container.ContainerNo = poffLoad.ContainerNumber;
                            load.Container.SealNo = poffLoad.SealNumber;
                            load.Container.TotalGrossWeight = poffLoad.TotalGrossWeight != null ? poffLoad.TotalGrossWeight.Value : load.Container.TotalGrossWeight;
                            load.Container.TotalGrossWeightUOM = "KGS";
                            load.Container.TotalNetWeight = poffLoad.TotalNetWeight != null ? poffLoad.TotalNetWeight.Value : load.Container.TotalNetWeight;
                            load.Container.TotalNetWeightUOM = "KGS";
                        }
                    }
                }
            }
        }

        private bool IsHasChangedCargoDetailsOrLoadDetails(InputPOFulfillmentViewModel viewModel, POFulfillmentModel model)
        {
            foreach (var item in viewModel.CargoDetails)
            {
                var cgModel = model.CargoDetails.FirstOrDefault(x => x.Id == item.Id);

                if (cgModel != null)
                {
                    var cgViewModel = Mapper.Map<POFulfillmentLoadDetailViewModel>(cgModel);
                    if (cgViewModel.Id == 0)
                    {
                        return true;
                    }

                    var properties = item.GetType().GetProperties();

                    foreach (var propInfo in properties)
                    {
                        var cgViewModelProp = propInfo.GetValue(item) == null ? string.Empty : propInfo.GetValue(item);
                        var cgModelProp = propInfo.GetValue(cgViewModel) == null ? string.Empty : propInfo.GetValue(cgViewModel);
                        if (!cgViewModelProp.Equals(cgModelProp))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }

            foreach (var item in viewModel.Loads.SelectMany(l => l.Details))
            {
                var ldModel = model.Loads.SelectMany(l => l.Details).FirstOrDefault(x => x.Id == item.Id);

                if (ldModel != null)
                {
                    var ldViewModel = Mapper.Map<POFulfillmentLoadDetailViewModel>(ldModel);
                    if (ldViewModel.Id == 0)
                    {
                        return true;
                    }

                    var properties = item.GetType().GetProperties();

                    foreach (var propInfo in properties)
                    {
                        var ldViewModelProp = propInfo.GetValue(item) == null ? string.Empty : propInfo.GetValue(item);
                        var ldModelProp = propInfo.GetValue(ldViewModel) == null ? string.Empty : propInfo.GetValue(ldViewModel);
                        if (!ldViewModelProp.Equals(ldModelProp))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }

            return !(viewModel.Loads.SelectMany(l => l.Details).Count() >= model.Loads.SelectMany(l => l.Details).Count()
                && viewModel.CargoDetails.Count() >= model.CargoDetails.Count());
        }

        #endregion

        public async Task<POFulfillmentViewModel> GetAsync(long id, bool isInternal, string affiliates)
        {
            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
            => x.Include(m => m.Contacts)
                .Include(m => m.Loads)
                .ThenInclude(i => i.Details)
                .Include(m => m.Orders)
                .Include(m => m.CargoDetails)
                .Include(m => m.BookingRequests)
                .Include(m => m.BuyerApprovals)
                .Include(m => m.Itineraries)
                .Include(m => m.Shipments)
                .Include(m => m.PurchaseOrderAdhocChanges);

            var listOfAffiliates = new List<long>();
            POFulfillmentModel model = null;

            if (isInternal)
            {
                model = await Repository.GetAsync(p => p.Id == id,
                    null,
                    includeProperties);
            }
            else
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }
                model = await Repository.GetAsync(p => p.Id == id && p.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId)),
                    null,
                    includeProperties);
            }

            var viewModel = Mapper.Map<POFulfillmentViewModel>(model);
            if (model != null)
            {
                viewModel.Shipments = viewModel.Shipments?.Where(s => s.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase)).ToList();

                // Populate <article master> data for each order
                var productCodes = viewModel.Orders.Select(x => x.ProductCode).Distinct().ToArray();
                var customerOrgId = viewModel.Contacts.FirstOrDefault(x => x.OrganizationRole.Equals(OrganizationRole.Principal))?.OrganizationId;

                if (productCodes != null && productCodes.Any() && customerOrgId.HasValue)
                {
                    var articleMasterInformations = await GetInformationFromArticleMaster(viewModel.Id, customerOrgId.Value, productCodes);

                    foreach (var item in articleMasterInformations)
                    {
                        var order = viewModel.Orders.FirstOrDefault(i => i.Id == item.Id);
                        if (order != null)
                        {
                            order.InnerQuantity = item.InnerQuantity;
                            order.OuterQuantity = item.OuterQuantity;
                        }
                    }
                }

                // Populate DescriptionOfGoods from PO Line Item
                var poLineItemIds = viewModel.Orders.Select(x => x.POLineItemId).Distinct();
                if (poLineItemIds != null && poLineItemIds.Any())
                {
                    var poLineItems = await _poLineItemRepository.Query(x => poLineItemIds.Contains(x.Id)).ToListAsync();
                    if (poLineItems != null && poLineItems.Any())
                    {
                        foreach (var item in poLineItems)
                        {
                            var poOrders = viewModel.Orders.Where(x => x.POLineItemId.Equals(item.Id)).ToList();
                            if (poOrders != null && poOrders.Any())
                            {
                                poOrders.ForEach(x => x.DescriptionOfGoods = item.DescriptionOfGoods);
                            }
                        }
                    }
                }

                if (viewModel.Contacts != null)
                {
                    foreach (var contact in viewModel.Contacts)
                    {
                        switch (contact.OrganizationRole)
                        {
                            case OrganizationRole.Principal:
                                contact.ContactSequence = RoleSequence.Principal;
                                break;
                            case OrganizationRole.Shipper:
                                contact.ContactSequence = RoleSequence.Shipper;
                                break;
                            case OrganizationRole.Consignee:
                                contact.ContactSequence = RoleSequence.Consignee;
                                break;
                            case OrganizationRole.NotifyParty:
                                contact.ContactSequence = RoleSequence.NotifyParty;
                                break;
                            case OrganizationRole.AlsoNotify:
                                contact.ContactSequence = RoleSequence.AlsoNotifyParty;
                                break;
                            case OrganizationRole.Supplier:
                                contact.ContactSequence = RoleSequence.Supplier;
                                break;
                            case OrganizationRole.Delegation:
                                contact.ContactSequence = RoleSequence.Delegation;
                                break;
                            case OrganizationRole.Pickup:
                                contact.ContactSequence = RoleSequence.PickupAddress;
                                break;
                            case OrganizationRole.BillingParty:
                                contact.ContactSequence = RoleSequence.BillingAddress;
                                break;
                            case OrganizationRole.OriginAgent:
                                contact.ContactSequence = RoleSequence.OriginAgent;
                                break;
                            case OrganizationRole.DestinationAgent:
                                contact.ContactSequence = RoleSequence.DestinationAgent;
                                break;
                            default:
                                break;
                        }
                    }
                    viewModel.Contacts = viewModel.Contacts.OrderBy(c => c.ContactSequence).ToList();
                }
            }
            return viewModel;
        }

        public async Task<POFulfillmentViewModel> CreateAsync(InputPOFulfillmentViewModel model, IdentityInfo currentUser)
        {
            var userName = currentUser.Username;
            var poFulfillment = Mapper.Map<POFulfillmentModel>(model);
            var customerOrgId = model.Contacts.Single(x => x.OrganizationRole == OrganizationRole.Principal).OrganizationId;

            if (!ValidatePOTypes(model.Orders.Select(x => x.PurchaseOrderId), model.FulfilledFromPOType, customerOrgId))
            {
                throw new ApplicationException("All selected Customer PO have to be same PO type and follow buyer compliance settings!");
            }

            var principalContact = model.Contacts.First(x => x.OrganizationRole.Equals(OrganizationRole.Principal));
            var customerOrg = await _csfeApiClient.GetOrganizationByIdAsync(principalContact.OrganizationId);

            // Customer prefix must be available, try to fire request to CSFE Master data API if not
            if (string.IsNullOrEmpty(model.CustomerPrefix))
            {
                if (customerOrg == null || string.IsNullOrEmpty(customerOrg.CustomerPrefix))
                {
                    throw new ApplicationException("Customer prefix is not valid to create booking!");
                }
                model.CustomerPrefix = customerOrg.CustomerPrefix;
            }

            poFulfillment.Number = await GenerateBookingNumber(model.CustomerPrefix, customerOrg.Id, model.CreatedDate);
            poFulfillment.Status = POFulfillmentStatus.Active;
            poFulfillment.Stage = POFulfillmentStage.Draft;
            poFulfillment.BookingDate = null;

            var groupUpdatedLineItems = poFulfillment.Orders
                .GroupBy(x => x.POLineItemId)
                .Select(i => new
                {
                    LineItemId = i.Key,
                    Total = i.Sum(s => s.FulfillmentUnitQty)
                });

            foreach (var order in poFulfillment.Orders)
            {
                order.OpenQty = order.BookedPackage ?? 0;
                order.LoadedQty = 0;
            }

            foreach (var load in poFulfillment.Loads)
            {
                load.LoadReferenceNumber = await GenerateBookingLoadNumber(DateTime.UtcNow);
            }

            await Repository.AddAsync(poFulfillment);

            await this.UnitOfWork.SaveChangesAsync();

            await ProceedAttachmentsAsync(model.Attachments?.ToList(), poFulfillment.Id, currentUser);

            #region Store HsCode / ChineseDescription
            // Only store organization preference if called by application GUI

            if (model.UpdateOrganizationPreferences)
            {
                await StoreOrganizationPreferenceSilentAsync(model.Orders, currentUser);
            }

            #endregion Store HsCode / ChineseDescription

            var result = Mapper.Map<POFulfillmentViewModel>(poFulfillment);
            return result;
        }

        public async Task<POFulfillmentViewModel> UpdateAsync(InputPOFulfillmentViewModel model, IdentityInfo currentUser)
        {
            var userName = currentUser.Username;
            var poFulfillment = await Repository.GetAsync(x => x.Id == model.Id, null, FullIncludeProperties);

            if (poFulfillment == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", model.Id)} not found!");
            }

            if (!ValidatePOTypes(model.Orders.Select(x => x.PurchaseOrderId), poFulfillment.FulfilledFromPOType))
            {
                throw new ApplicationException("All selected Customer POs have to be same PO type!");
            }

            var poAdhocChanges = await GetPurchaseOrderAdhocChangesAsync(poFulfillment.Id);

            if (poAdhocChanges != null && poAdhocChanges.Count() > 0)
            {
                var purchaseOrderAdhocChangesTopPriority = poAdhocChanges.OrderBy(x => x.Priority).First();

                if (purchaseOrderAdhocChangesTopPriority.Priority == PurchaseOrderAdhocChangePriority.Level1)
                {
                    throw new AppValidationException($"#POAdhocChanged#{purchaseOrderAdhocChangesTopPriority.Priority}#{purchaseOrderAdhocChangesTopPriority.Message}#{string.Join(",", poAdhocChanges.Where(x => x.Priority == purchaseOrderAdhocChangesTopPriority.Priority).Select(x => x.PurchaseOrderId))}");
                }
                else if (purchaseOrderAdhocChangesTopPriority.Priority == PurchaseOrderAdhocChangePriority.Level3
                    || (purchaseOrderAdhocChangesTopPriority.Priority == PurchaseOrderAdhocChangePriority.Level2 && model.IsPurchaseOrderRefreshed))
                {
                    DeletePurchaseOrderAdhocChanges(0, poFulfillment.Id, 0);
                }
            }

            // Confirm from front-end by adding Itinerary
            if (model.IsForwarderBookingItineraryReady && !poFulfillment.IsForwarderBookingItineraryReady && poFulfillment.Stage == POFulfillmentStage.ForwarderBookingRequest)
            {
                model.Stage = POFulfillmentStage.ForwarderBookingConfirmed;
                var bookingRequest = poFulfillment.BookingRequests.SingleOrDefault(x => x.Status == POFulfillmentBookingRequestStatus.Active);
                if (bookingRequest == null)
                {
                    throw new NullReferenceException("Cannot find booking request");
                }
                bookingRequest.RequestContent = JsonConvert.SerializeObject(Mapper.Map<BookingRequestContentViewModel>(poFulfillment));
                var event1052 = new ActivityViewModel()
                {
                    ActivityCode = Event.EVENT_1052,
                    POFulfillmentId = poFulfillment.Id,
                    ActivityDate = DateTime.UtcNow,
                    CreatedBy = userName
                };
                await _activityService.TriggerAnEvent(event1052);
                await ConfirmPurchaseOrderFulfillmentAsync(poFulfillment.Id, bookingRequest.SONumber, userName);
            }

            var isNeedPlanToShipAgain = false;
            if (poFulfillment.Stage == POFulfillmentStage.ForwarderBookingConfirmed && this.IsHasChangedCargoDetailsOrLoadDetails(model, poFulfillment))
            {
                var existingAttachments = await _attachmentService.GetAttachmentsAsync(EntityType.POFullfillment, poFulfillment.Id);
                if (existingAttachments.Any(a => a.AttachmentType == AttachmentType.PACKING_LIST ||
                                                 a.AttachmentType == AttachmentType.COMMERCIAL_INVOICE))
                {
                    poFulfillment.IsGeneratePlanToShip = false;
                    isNeedPlanToShipAgain = true;
                }
            }

            #region POFulfilment General
            // Mapping POFulfillment
            poFulfillment.Number = model.Number;
            poFulfillment.Owner = model.Owner;
            poFulfillment.Status = model.Status;
            poFulfillment.IsRejected = false;
            poFulfillment.Stage = model.Stage;
            poFulfillment.CargoReadyDate = model.CargoReadyDate;
            poFulfillment.Incoterm = model.Incoterm;
            poFulfillment.IsPartialShipment = model.IsPartialShipment;
            poFulfillment.IsContainDangerousGoods = model.IsContainDangerousGoods;
            poFulfillment.ModeOfTransport = model.ModeOfTransport;
            poFulfillment.PreferredCarrier = model.PreferredCarrier;
            poFulfillment.LogisticsService = model.LogisticsService;
            poFulfillment.MovementType = model.MovementType;
            poFulfillment.ShipFrom = model.ShipFrom;
            poFulfillment.ShipTo = model.ShipTo;
            poFulfillment.ShipFromName = model.ShipFromName;
            poFulfillment.ShipToName = model.ShipToName;
            poFulfillment.ExpectedShipDate = model.ExpectedShipDate;
            poFulfillment.ExpectedDeliveryDate = model.ExpectedDeliveryDate;
            poFulfillment.Remarks = model.Remarks;
            poFulfillment.IsForwarderBookingItineraryReady = model.IsForwarderBookingItineraryReady;
            poFulfillment.IsShipperPickup = model.IsShipperPickup;
            poFulfillment.IsNotifyPartyAsConsignee = model.IsNotifyPartyAsConsignee;
            poFulfillment.DeliveryPortId = model.DeliveryPortId;
            poFulfillment.ReceiptPortId = model.ReceiptPortId;
            poFulfillment.DeliveryPort = model.DeliveryPort;
            poFulfillment.ReceiptPort = model.ReceiptPort;
            poFulfillment.IsBatteryOrChemical = model.IsBatteryOrChemical;
            poFulfillment.IsCIQOrFumigation = model.IsCIQOrFumigation;
            poFulfillment.IsExportLicence = model.IsExportLicence;
            #endregion

            #region POFulfillment Contacts
            // Mapping POFulfillment Contacts
            var deletedContacts = new ArrayList();
            foreach (var contact in poFulfillment.Contacts)
            {
                var c = model.Contacts.FirstOrDefault(x => x.Id == contact.Id);
                if (c != null)
                {
                    contact.OrganizationId = c.OrganizationId;
                    contact.OrganizationRole = c.OrganizationRole;
                    contact.CompanyName = c.CompanyName;
                    contact.ContactName = c.ContactName;
                    contact.ContactNumber = c.ContactNumber;
                    contact.ContactEmail = c.ContactEmail;
                    contact.WeChatOrWhatsApp = c.WeChatOrWhatsApp;
                    contact.Address = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(c.Address, 1);
                    contact.AddressLine2 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(c.Address, 2);
                    contact.AddressLine3 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(c.Address, 3);
                    contact.AddressLine4 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(c.Address, 4);
                }
                else
                {
                    deletedContacts.Add(contact);
                }
            }

            foreach (POFulfillmentContactModel deletedContact in deletedContacts)
            {
                poFulfillment.Contacts.Remove(deletedContact);
            }

            var newContacts = model.Contacts.Where(x => x.Id <= 0);
            foreach (var newContact in newContacts)
            {
                var con = new POFulfillmentContactModel
                {
                    OrganizationId = newContact.OrganizationId,
                    OrganizationRole = newContact.OrganizationRole,
                    CompanyName = newContact.CompanyName,
                    ContactName = newContact.ContactName,
                    ContactNumber = newContact.ContactNumber,
                    WeChatOrWhatsApp = newContact.WeChatOrWhatsApp,
                    ContactEmail = newContact.ContactEmail,
                    CreatedDate = newContact.CreatedDate,
                    CreatedBy = newContact.CreatedBy,
                    UpdatedDate = newContact.UpdatedDate,
                    UpdatedBy = newContact.UpdatedBy,
                    Address = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(newContact.Address, 1),
                    AddressLine2 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(newContact.Address, 2),
                    AddressLine3 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(newContact.Address, 3),
                    AddressLine4 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(newContact.Address, 4)
                };
                poFulfillment.Contacts.Add(con);
            }

            // Changes AgentAssignmen in contact tab
            poFulfillment.AgentAssignmentMode = model.AgentAssignmentMode;
            #endregion

            #region POFulfillment Customer PO
            // Mapping POFulfillmentOrder
            var deletedOrders = new ArrayList();

            foreach (var updatedOrder in poFulfillment.Orders)
            {
                var updatedViewmodelOrder = model.Orders.FirstOrDefault(x => x.Id == updatedOrder.Id);
                if (updatedViewmodelOrder != null)
                {
                    updatedOrder.PurchaseOrderId = updatedViewmodelOrder.PurchaseOrderId;
                    updatedOrder.POLineItemId = updatedViewmodelOrder.POLineItemId;
                    updatedOrder.CustomerPONumber = updatedViewmodelOrder.CustomerPONumber;
                    updatedOrder.ProductCode = updatedViewmodelOrder.ProductCode;
                    updatedOrder.ProductName = updatedViewmodelOrder.ProductName;
                    updatedOrder.OrderedUnitQty = updatedViewmodelOrder.OrderedUnitQty;
                    updatedOrder.FulfillmentUnitQty = updatedViewmodelOrder.FulfillmentUnitQty;
                    updatedOrder.BalanceUnitQty = updatedViewmodelOrder.BalanceUnitQty;
                    updatedOrder.UnitUOM = updatedViewmodelOrder.UnitUOM;
                    updatedOrder.PackageUOM = updatedViewmodelOrder.PackageUOM;
                    updatedOrder.Commodity = updatedViewmodelOrder.Commodity;
                    updatedOrder.Status = updatedViewmodelOrder.Status;
                    updatedOrder.HsCode = updatedViewmodelOrder.HsCode;
                    updatedOrder.CountryCodeOfOrigin = updatedViewmodelOrder.CountryCodeOfOrigin;
                    updatedOrder.LoadedQty = updatedViewmodelOrder.LoadedQty;
                    updatedOrder.OpenQty = updatedViewmodelOrder.OpenQty;
                    updatedOrder.BookedPackage = updatedViewmodelOrder.BookedPackage;
                    updatedOrder.Volume = updatedViewmodelOrder.Volume;
                    updatedOrder.GrossWeight = updatedViewmodelOrder.GrossWeight;
                    updatedOrder.NetWeight = updatedViewmodelOrder.NetWeight;
                    updatedOrder.ChineseDescription = updatedViewmodelOrder.ChineseDescription;
                    updatedOrder.ShippingMarks = updatedViewmodelOrder.ShippingMarks;
                }
                else
                {
                    deletedOrders.Add(updatedOrder);
                }
            }

            var newViewModelOrders = model.Orders.Where(x => x.Id <= 0);
            foreach (var newViewModelOrder in newViewModelOrders)
            {
                var newOrder = new POFulfillmentOrderModel
                {
                    PurchaseOrderId = newViewModelOrder.PurchaseOrderId,
                    POLineItemId = newViewModelOrder.POLineItemId,
                    CustomerPONumber = newViewModelOrder.CustomerPONumber,
                    ProductCode = newViewModelOrder.ProductCode,
                    ProductName = newViewModelOrder.ProductName,
                    OrderedUnitQty = newViewModelOrder.OrderedUnitQty,
                    FulfillmentUnitQty = newViewModelOrder.FulfillmentUnitQty,
                    BalanceUnitQty = newViewModelOrder.BalanceUnitQty,
                    UnitUOM = newViewModelOrder.UnitUOM,
                    PackageUOM = newViewModelOrder.PackageUOM,
                    Commodity = newViewModelOrder.Commodity,
                    Status = newViewModelOrder.Status,
                    HsCode = newViewModelOrder.HsCode,
                    CountryCodeOfOrigin = newViewModelOrder.CountryCodeOfOrigin,
                    LoadedQty = 0, // For the new Order, LoadedQty should be equal to 0
                    OpenQty = newViewModelOrder.BookedPackage ?? 0, // For the new Order, OpenQty should be equal to Booked Package
                    BookedPackage = newViewModelOrder.BookedPackage,
                    Volume = newViewModelOrder.Volume,
                    GrossWeight = newViewModelOrder.GrossWeight,
                    NetWeight = newViewModelOrder.NetWeight,
                    ChineseDescription = newViewModelOrder.ChineseDescription,
                    ShippingMarks = newViewModelOrder.ShippingMarks
                };
                poFulfillment.Orders.Add(newOrder);
            }

            foreach (POFulfillmentOrderModel deletedOrder in deletedOrders)
            {
                poFulfillment.Orders.Remove(deletedOrder);
            }
            #endregion

            #region POFulfillment Loads and LoadDetails
            // Mapping POFulfillment Loads
            var deletedLoads = new List<POFulfillmentLoadModel>();
            var deletedLoadDetails = new ArrayList();
            foreach (var load in poFulfillment.Loads)
            {
                var viewModelLoad = model.Loads.FirstOrDefault(x => x.Id == load.Id);
                if (viewModelLoad != null)
                {
                    load.LoadReferenceNumber = viewModelLoad.LoadReferenceNumber;
                    load.EquipmentType = viewModelLoad.EquipmentType;
                    load.PlannedVolume = viewModelLoad.PlannedVolume;
                    load.PlannedNetWeight = viewModelLoad.PlannedNetWeight;
                    load.PlannedGrossWeight = viewModelLoad.PlannedGrossWeight;
                    load.PlannedPackageQuantity = viewModelLoad.PlannedPackageQuantity;
                    load.PackageUOM = viewModelLoad.PackageUOM;
                    load.Status = viewModelLoad.Status;

                    load.ContainerNumber = viewModelLoad.ContainerNumber;
                    load.SealNumber = viewModelLoad.SealNumber;
                    load.SealNumber2 = viewModelLoad.SealNumber2;
                    load.LoadingDate = viewModelLoad.LoadingDate;

                    // Assure that sequence will start from 1 by +1
                    if (load.Details != null && load.Details.Any())
                    {
                        foreach (var details in load.Details)
                        {
                            details.Sequence = load.Details.IndexOf(details) + 1;
                        }
                    }

                    // Update detail
                    foreach (var detail in load.Details)
                    {
                        var updatedLoadDetail = viewModelLoad.Details.FirstOrDefault(x => x.Id == detail.Id);

                        // also check if pofulfillment order removed
                        if (updatedLoadDetail != null && poFulfillment.Orders.Any(x => x.Id == updatedLoadDetail.POFulfillmentOrderId))
                        {
                            detail.POFulfillmentLoadId = updatedLoadDetail.POFulfillmentLoadId;
                            detail.CustomerPONumber = updatedLoadDetail.CustomerPONumber;
                            detail.ProductCode = updatedLoadDetail.ProductCode;
                            detail.PackageQuantity = updatedLoadDetail.PackageQuantity;
                            detail.PackageUOM = updatedLoadDetail.PackageUOM;
                            detail.Height = updatedLoadDetail.Height;
                            detail.Width = updatedLoadDetail.Width;
                            detail.Length = updatedLoadDetail.Length;
                            detail.DimensionUnit = updatedLoadDetail.DimensionUnit;
                            detail.UnitQuantity = updatedLoadDetail.UnitQuantity;
                            detail.Volume = updatedLoadDetail.Volume;
                            detail.GrossWeight = updatedLoadDetail.GrossWeight;
                            detail.NetWeight = updatedLoadDetail.NetWeight;
                            detail.POFulfillmentOrderId = updatedLoadDetail.POFulfillmentOrderId;
                            detail.PackageDescription = updatedLoadDetail.PackageDescription;
                            detail.ShippingMarks = updatedLoadDetail.ShippingMarks;
                            detail.Sequence = updatedLoadDetail.Sequence;
                        }
                        else
                        {
                            deletedLoadDetails.Add(detail);
                        }
                    }

                    foreach (POFulfillmentLoadDetailModel deletedLoadDetail in deletedLoadDetails)
                    {
                        load.Details.Remove(deletedLoadDetail);
                    }

                    var viewModelLoadDetails = viewModelLoad.Details.Where(x => x.Id <= 0);
                    foreach (var newViewModelLoadDetail in viewModelLoadDetails)
                    {
                        var newLoadDetail = new POFulfillmentLoadDetailModel
                        {
                            POFulfillmentLoadId = newViewModelLoadDetail.POFulfillmentLoadId,
                            CustomerPONumber = newViewModelLoadDetail.CustomerPONumber,
                            ProductCode = newViewModelLoadDetail.ProductCode,
                            PackageQuantity = newViewModelLoadDetail.PackageQuantity,
                            PackageUOM = newViewModelLoadDetail.PackageUOM,
                            Height = newViewModelLoadDetail.Height,
                            Width = newViewModelLoadDetail.Width,
                            Length = newViewModelLoadDetail.Length,
                            DimensionUnit = newViewModelLoadDetail.DimensionUnit,
                            UnitQuantity = newViewModelLoadDetail.UnitQuantity,
                            Volume = newViewModelLoadDetail.Volume,
                            GrossWeight = newViewModelLoadDetail.GrossWeight,
                            NetWeight = newViewModelLoadDetail.NetWeight,
                            POFulfillmentOrderId = newViewModelLoadDetail.POFulfillmentOrderId,
                            PackageDescription = newViewModelLoadDetail.PackageDescription,
                            ShippingMarks = newViewModelLoadDetail.ShippingMarks,
                            Sequence = newViewModelLoadDetail.Sequence
                        };
                        load.Details.Add(newLoadDetail);
                    }

                    // Calculate Subtotal and Total for load
                    load.SubtotalNetWeight = load.Details.Sum(x => x.NetWeight ?? 0);
                    load.SubtotalGrossWeight = load.Details.Sum(x => x.GrossWeight);
                    load.SubtotalVolume = load.Details.Sum(x => x.Volume);
                    load.SubtotalPackageQuantity = load.Details.Sum(x => x.PackageQuantity);
                    load.SubtotalUnitQuantity = load.Details.Sum(x => x.UnitQuantity);
                    load.TotalGrossWeight = load.SubtotalGrossWeight;
                    load.TotalNetWeight = load.SubtotalNetWeight;

                }
                else
                {
                    // The item is already deleted
                    deletedLoads.Add(load);
                }

                // Assure that sequence will start from 1 by +1 incase some load details removed
                if (load.Details != null && load.Details.Any())
                {
                    var orderedLoadDetails = load.Details.OrderBy(x => x.Sequence);
                    foreach (var details in orderedLoadDetails)
                    {
                        details.Sequence = orderedLoadDetails.IndexOf(details) + 1;
                    }
                }

            }

            foreach (POFulfillmentLoadModel deletedLoad in deletedLoads)
            {
                poFulfillment.Loads.Remove(deletedLoad);
            }

            var newLoads = model.Loads.Where(x => x.Id <= 0);
            foreach (var newLoad in newLoads)
            {
                var load = new POFulfillmentLoadModel
                {
                    LoadReferenceNumber = await GenerateBookingLoadNumber(DateTime.UtcNow),
                    EquipmentType = newLoad.EquipmentType,
                    PlannedVolume = newLoad.PlannedVolume,
                    PlannedNetWeight = newLoad.PlannedNetWeight,
                    PlannedGrossWeight = newLoad.PlannedGrossWeight,
                    PlannedPackageQuantity = newLoad.PlannedPackageQuantity,
                    PackageUOM = newLoad.PackageUOM,
                    Status = newLoad.Status,
                    SubtotalNetWeight = newLoad.SubtotalNetWeight,
                    SubtotalGrossWeight = newLoad.SubtotalGrossWeight,
                    SubtotalVolume = newLoad.SubtotalVolume,
                    SubtotalUnitQuantity = newLoad.SubtotalUnitQuantity,
                    SubtotalPackageQuantity = newLoad.SubtotalPackageQuantity,

                    ContainerNumber = newLoad.ContainerNumber,
                    SealNumber = newLoad.SealNumber,
                    TotalGrossWeight = newLoad.TotalGrossWeight,
                    TotalNetWeight = newLoad.TotalNetWeight
                };
                poFulfillment.Loads.Add(load);
            }

            #endregion

            #region Calculate LoadedQty and OpenQty

            foreach (var updatedOrder in poFulfillment.Orders)
            {
                var loadedQty = poFulfillment.Loads.Where(x => x.Details != null && x.Details.Any()).SelectMany(x => x.Details).Where(x => x.POFulfillmentOrderId == updatedOrder.Id).Sum(x => x.PackageQuantity);
                updatedOrder.LoadedQty = loadedQty;
                updatedOrder.OpenQty = (updatedOrder.BookedPackage ?? 0) - loadedQty;
            }

            #endregion

            #region POFulfillment Cargo Details
            // Mapping POFulfillment Cargo Details
            var deletedCargoDetails = new ArrayList();
            foreach (var cargoDetail in poFulfillment.CargoDetails)
            {
                var cd = model.CargoDetails.FirstOrDefault(x => x.Id == cargoDetail.Id);
                if (cd != null)
                {
                    cargoDetail.LineOrder = cd.LineOrder;
                    cargoDetail.Height = cd.Height;
                    cargoDetail.Width = cd.Width;
                    cargoDetail.Length = cd.Length;
                    cargoDetail.DimensionUnit = cd.DimensionUnit;
                    cargoDetail.UnitUOM = cd.UnitUOM;
                    cargoDetail.PackageQuantity = cd.PackageQuantity;
                    cargoDetail.PackageUOM = cd.PackageUOM;
                    cargoDetail.Volume = cd.Volume;
                    cargoDetail.GrossWeight = cd.GrossWeight;
                    cargoDetail.NetWeight = cd.NetWeight;
                    cargoDetail.CountryCodeOfOrigin = cd.CountryCodeOfOrigin;
                    cargoDetail.HsCode = cd.HsCode;
                    cargoDetail.ShippingMarks = cd.ShippingMarks;
                    cargoDetail.PackageDescription = cd.PackageDescription;
                    cargoDetail.UnitQuantity = cd.UnitQuantity;
                    cargoDetail.Commodity = cd.Commodity;
                    cargoDetail.LoadReferenceNumber = cd.LoadReferenceNumber;
                }
                else
                {
                    // The item is already deleted
                    deletedCargoDetails.Add(cargoDetail);
                }

                if (deletedLoads.Any(l => l.LoadReferenceNumber == cargoDetail.LoadReferenceNumber))
                {
                    deletedCargoDetails.Add(cargoDetail);
                }
            }

            foreach (POFulfillmentCargoDetailModel cargoDetail in deletedCargoDetails)
            {
                poFulfillment.CargoDetails.Remove(cargoDetail);
            }

            var newCargoDetails = model.CargoDetails.Where(x => x.Id <= 0);
            foreach (var newCargoDetail in newCargoDetails)
            {
                var newCd = new POFulfillmentCargoDetailModel
                {
                    LineOrder = newCargoDetail.LineOrder,
                    Height = newCargoDetail.Height,
                    Width = newCargoDetail.Width,
                    Length = newCargoDetail.Length,
                    DimensionUnit = newCargoDetail.DimensionUnit,
                    UnitUOM = newCargoDetail.UnitUOM,
                    PackageQuantity = newCargoDetail.PackageQuantity,
                    PackageUOM = newCargoDetail.PackageUOM,
                    Volume = newCargoDetail.Volume,
                    GrossWeight = newCargoDetail.GrossWeight,
                    NetWeight = newCargoDetail.NetWeight,
                    CountryCodeOfOrigin = newCargoDetail.CountryCodeOfOrigin,
                    HsCode = newCargoDetail.HsCode,
                    ShippingMarks = newCargoDetail.ShippingMarks,
                    PackageDescription = newCargoDetail.PackageDescription,
                    UnitQuantity = newCargoDetail.UnitQuantity,
                    Commodity = newCargoDetail.Commodity,
                    LoadReferenceNumber = newCargoDetail.LoadReferenceNumber
                };

                poFulfillment.CargoDetails.Add(newCd);
            }
            #endregion

            #region POFulfillment Itineraries
            // Mapping POFulfillment Itineraries
            var deletedItineraries = new ArrayList();
            foreach (var existingItinerary in poFulfillment.Itineraries)
            {
                var itinerary = model.Itineraries.FirstOrDefault(x => x.Id == existingItinerary.Id);
                if (itinerary != null)
                {
                    existingItinerary.CarrierId = itinerary.CarrierId;
                    existingItinerary.CarrierName = itinerary.CarrierName;
                    existingItinerary.LoadingPortId = itinerary.LoadingPortId;
                    existingItinerary.LoadingPort = itinerary.LoadingPort;
                    existingItinerary.DischargePortId = itinerary.DischargePortId;
                    existingItinerary.DischargePort = itinerary.DischargePort;
                    existingItinerary.ETADate = itinerary.ETADate;
                    existingItinerary.ETDDate = itinerary.ETDDate;
                    existingItinerary.ModeOfTransport = itinerary.ModeOfTransport;
                    existingItinerary.Status = itinerary.Status;
                    existingItinerary.VesselFlight = itinerary.VesselFlight;
                    existingItinerary.CreatedBy = itinerary.CreatedBy;
                    existingItinerary.CreatedDate = itinerary.CreatedDate;
                }
                else
                {
                    // The item is already deleted
                    deletedItineraries.Add(existingItinerary);
                }
            }

            foreach (POFulfillmentItineraryModel deletedItinerary in deletedItineraries)
            {
                poFulfillment.Itineraries.Remove(deletedItinerary);
            }

            var newItineraries = model.Itineraries.Where(x => x.Id <= 0);
            foreach (var newItinerary in newItineraries)
            {
                var itinerary = new POFulfillmentItineraryModel
                {
                    CarrierId = newItinerary.CarrierId,
                    CarrierName = newItinerary.CarrierName,
                    LoadingPortId = newItinerary.LoadingPortId,
                    LoadingPort = newItinerary.LoadingPort,
                    DischargePortId = newItinerary.DischargePortId,
                    DischargePort = newItinerary.DischargePort,
                    ETADate = newItinerary.ETADate,
                    ETDDate = newItinerary.ETDDate,
                    ModeOfTransport = newItinerary.ModeOfTransport,
                    VesselFlight = newItinerary.VesselFlight,
                    Status = POFulfillmentItinerayStatus.Active,
                    CreatedBy = newItinerary.CreatedBy,
                    CreatedDate = newItinerary.CreatedDate
                };
                poFulfillment.Itineraries.Add(itinerary);
            }

            #endregion

            #region POFulfillment Attachments
            // attachments

            // Need to send email to Origin Agent as document is either created or updated
            var isNewShippingDocuments = await ProceedAttachmentsAsync(model.Attachments?.ToList(), poFulfillment.Id, currentUser);
            #endregion           

            Repository.Update(poFulfillment);
            await this.UnitOfWork.SaveChangesAsync();

            // Stage >= ForwarderBookingRequest and booking already submitted
            var isBookingSubmitted = poFulfillment.Stage >= POFulfillmentStage.ForwarderBookingRequest
                                    && (poFulfillment.BuyerApprovals == null || !poFulfillment.BuyerApprovals.Any(x => x.Stage == BuyerApprovalStage.Pending));

            // Need to send mail to Origin Agent on new shipping documents
            if (isBookingSubmitted)
            {
                await SendNewShippingDocumentsNotificationEmailToOriginAgent(poFulfillment);
            }

            var result = Mapper.Map<POFulfillmentViewModel>(poFulfillment);
            result.IsNeedToPlanToShipAgain = isNeedPlanToShipAgain;

            #region Store HsCode / ChineseDescription
            // Only store organization preference if called by application GUI

            if (model.UpdateOrganizationPreferences)
            {
                await StoreOrganizationPreferenceSilentAsync(model.Orders, currentUser);
            }

            #endregion Store HsCode / ChineseDescription

            return result;
        }

        public async Task<POFulfillmentViewModel> UpdateAsync(EdiSonUpdateConfirmPOFFViewModel model, IdentityInfo currentUser)
        {
            var poFulfillment = await Repository.GetAsync(x => x.BookingRequests.Any(br => br.BookingReferenceNumber == model.BookingReferenceNo && br.Status == POFulfillmentBookingRequestStatus.Active),
                includes: i => i.Include(m => m.BookingRequests));

            if (poFulfillment == null)
            {
                throw new AppEntityNotFoundException($"POFF with the BookingReferenceNo {model.BookingReferenceNo} not found!");
            }

            if (poFulfillment.Stage != POFulfillmentStage.ForwarderBookingConfirmed)
            {
                throw new AppValidationException($"Cannot update PO Fulfillment because of stage!");
            }

            var bookingRequest = poFulfillment.BookingRequests.SingleOrDefault(x => x.Status == POFulfillmentBookingRequestStatus.Active);
            if (bookingRequest == null)
            {
                throw new AppValidationException($"Booking with the BookingReferenceNo {model.BookingReferenceNo} is inactive!");
            }

            Mapper.Map(model, poFulfillment);
            poFulfillment.Audit(currentUser.Username);

            // update POFulfillmentRequests 
            Mapper.Map(model, bookingRequest);
            bookingRequest.Audit(currentUser.Username);

            await UnitOfWork.SaveChangesAsync();

            return Mapper.Map<POFulfillmentViewModel>(poFulfillment);
        }

        /// <summary>
        /// To apply logic on re-uploading attachment, it will call methods on AttachmentService:
        /// <list type="bullet">
        /// <item><see cref="SP.Application.Attachment.Services.AttachmentService.ImportAttachmentAsync(AttachmentViewModel, bool?, long?)"/></item>
        /// <item><see cref="SP.Application.Attachment.Services.AttachmentService.UpdateAttachmentAsync(AttachmentViewModel, long?)"/></item>
        /// <item><see cref="SP.Application.Attachment.Services.AttachmentService.DeleteAttachmentAsync(string, long, long?)"/></item>
        /// </list>
        /// </summary>
        /// <param name="storingAttachments">Attachments which sent from client: edited, added</param>
        /// <param name="pofulfillmentId">Id of booking</param>
        /// <param name="currentUser">Current user identity information</param>
        /// <returns>true if there is any new document, otherwise false</returns>
        private async Task<bool> ProceedAttachmentsAsync(List<POFulfillmentAttachmentViewModel> storingAttachments, long pofulfillmentId, IdentityInfo currentUser)
        {
            if (storingAttachments == null || !storingAttachments.Any())
            {
                var isExistingAttachments = await _attachmentRepository.AnyAsync(s => s.GlobalIdAttachments.Any(g => g.GlobalId == CommonHelper.GenerateGlobalId(pofulfillmentId, EntityType.POFullfillment)));
                // Delete the last attachment case
                if (!isExistingAttachments)
                {
                    return false;
                }
            }

            // Handle edited cases
            var isNewShippingDocuments = false;
            var globalId = CommonHelper.GenerateGlobalId(pofulfillmentId, EntityType.POFullfillment);
            var storedAttachments = await _attachmentRepository.QueryAsNoTracking(s => s.GlobalIdAttachments.Any(g => g.GlobalId == globalId), includes: x => x.Include(y => y.GlobalIdAttachments)).ToListAsync();
            var accessibleDocumentTypes = await _attachmentService.GetAccessibleDocumentTypesAsync(currentUser.UserRoleId, EntityType.POFullfillment);

            var deletingAttachments = new List<AttachmentModel>();

            for (var i = 0; i < storedAttachments.Count; i++)
            {
                var existingAttachment = storedAttachments[i];


                var existingAttachmentViewModel = storingAttachments.FirstOrDefault(x => x.Id == existingAttachment.Id);

                // Existing on list which sent from client
                if (existingAttachmentViewModel != null)
                {
                    if (currentUser.UserRoleId == (int)Role.Factory && existingAttachmentViewModel.AttachmentType == AttachmentType.COMMERCIAL_INVOICE)
                    {
                        existingAttachmentViewModel.AttachmentType = AttachmentType.FACTORY_COMMERCIAL_INVOICE;
                    }

                    // No change then continue
                    if (existingAttachmentViewModel.State != POFulfillmentAttachmentState.Edited)
                    {
                        continue;
                    }

                    // Document type of stored attachment is not accessible, ignore
                    var isAccessibleDocumentType = accessibleDocumentTypes.Any(x => x.Equals(existingAttachment.AttachmentType, StringComparison.OrdinalIgnoreCase));
                    if (!isAccessibleDocumentType)
                    {
                        continue;
                    }

                    // New value of document type is not accessible, ignore
                    isAccessibleDocumentType = accessibleDocumentTypes.Any(x => x.Equals(existingAttachmentViewModel.AttachmentType, StringComparison.OrdinalIgnoreCase));
                    if (!isAccessibleDocumentType)
                    {
                        continue;
                    }

                    // Attachment is being updated/edited
                    isNewShippingDocuments = true;

                    existingAttachmentViewModel.Audit(currentUser.Username);


                    await _attachmentService.UpdateAttachmentAsync(existingAttachmentViewModel);
                }
                // user is accessible to current document type -> deleted
                else if (accessibleDocumentTypes.Any(x => x.Equals(existingAttachment.AttachmentType, StringComparison.OrdinalIgnoreCase)))
                {
                    // The item is deleted
                    deletingAttachments.Add(existingAttachment);
                }
            }

            // Handle deleted cases
            foreach (var deletingAttachment in deletingAttachments)
            {
                try
                {
                    await _attachmentService.DeleteAttachmentAsync(CommonHelper.GenerateGlobalId(pofulfillmentId, EntityType.POFullfillment), deletingAttachment.Id);
                }
                catch (Exception ex) when (ex.GetType().Name == typeof(AppEntityNotFoundException).Name)
                {
                    continue;
                }
            }

            // Handle added cases
            var newAttachmentViewModels = storingAttachments.Where(x => x.State == POFulfillmentAttachmentState.Added).ToList();
            isNewShippingDocuments = isNewShippingDocuments || (newAttachmentViewModels != null && newAttachmentViewModels.Any());
            foreach (var viewModel in newAttachmentViewModels)
            {
                if (currentUser.UserRoleId == (int)Role.Factory && viewModel.AttachmentType == AttachmentType.COMMERCIAL_INVOICE)
                {
                    viewModel.AttachmentType = AttachmentType.FACTORY_COMMERCIAL_INVOICE;
                }

                // filter on accessible document types
                if (accessibleDocumentTypes.Contains(viewModel.AttachmentType, StringComparer.OrdinalIgnoreCase))
                {
                    // assign id of booking
                    viewModel.POFulfillmentId = pofulfillmentId;
                    await _attachmentService.ImportAttachmentAsync(viewModel);
                }
            }
            return isNewShippingDocuments;
        }

        public async Task<POFulfillmentViewModel> CancelPOFulfillmentAsync(long id, string userName, CancelPOFulfillmentViewModel cancelModel)
        {
            UnitOfWork.BeginTransaction();
            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
             => x.Include(m => m.Contacts)
                 .Include(m => m.Loads)
                 .Include(m => m.Orders)
                 .Include(m => m.ShortshipOrders)
                 .Include(m => m.BookingRequests)
                 .Include(m => m.BuyerApprovals)
                 .Include(m => m.Shipments)
                 .ThenInclude(i => i.Consignments);

            var poff = await Repository.GetAsync(x => x.Id == id, null, includeProperties);

            if (poff != null && poff.Stage <= POFulfillmentStage.ForwarderBookingRequest)
            {
                poff.Status = POFulfillmentStatus.Inactive;
                poff.IsRejected = false;
                poff.BookingDate = null;

                // Release quantity on PO here, must before deactivating purchase order fulfillment loads
                if (poff.Stage == POFulfillmentStage.ForwarderBookingRequest)
                {
                    // When Booking stage = FB Request, release balance quantity from PO line items
                    ReleaseQuantityOnPOLineItems(id);
                }


                var event1057 = new ActivityViewModel()
                {
                    ActivityCode = Event.EVENT_1057,
                    POFulfillmentId = poff.Id,
                    ActivityDate = DateTime.UtcNow,
                    CreatedBy = userName,
                    Remark = cancelModel.Reason
                };
                await _activityService.TriggerAnEvent(event1057);

                foreach (var load in poff.Loads)
                {
                    load.Status = POFulfillmentLoadStatus.Inactive;
                }

                foreach (var order in poff.Orders)
                {
                    order.Status = POFulfillmentOrderStatus.Inactive;
                }

                // Cancel current Booking Request
                var bookingRequest = poff.BookingRequests.SingleOrDefault(br => br.Status == POFulfillmentBookingRequestStatus.Active);
                if (bookingRequest != null)
                {
                    bookingRequest.Status = POFulfillmentBookingRequestStatus.Inactive;
                    await _ediSonBookingService.CancelBookingRequest(bookingRequest);
                }

                Repository.Update(poff);

                // Update PO stage
                await UpdatePurchaseOrderStageByPOFFAsync(poff);

                // When user cancel a FB Request fulfillment which has a pending approval associated,
                // update stage of buyer approval to Cancel as well.
                var buyerApproval = await _buyerApprovalRepository.GetAsync(x => x.POFulfillmentId == poff.Id && x.Stage == BuyerApprovalStage.Pending);
                if (buyerApproval != null)
                {
                    buyerApproval.Stage = BuyerApprovalStage.Cancel;
                    _buyerApprovalRepository.Update(buyerApproval);
                }

                #region Deactivate shipments
                // Set Shipments' status to Inactive
                // If there is shipment associated to PO Fulfillment, system should update Shipment status = Inactive.
                var shipments = poff.Shipments?.Where(x => x.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase));
                var shipment = shipments?.FirstOrDefault();

                if (shipments != null && shipments.Any())
                {
                    foreach (var item in shipments)
                    {
                        foreach (var consigment in item.Consignments)
                        {
                            consigment.Status = StatusType.INACTIVE;
                        }

                        item.SetCancelledStatus();
                    }
                }
                #endregion

                // Remove all POAdhocChanges for the Booking
                DeletePurchaseOrderAdhocChanges(0, id, 0);


                // Get reference number for the booking
                var referenceNumber = shipment != null ? shipment.ShipmentNo
                    : (bookingRequest != null ? bookingRequest.BookingReferenceNumber : string.Empty);

                var originAgent = poff.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.OriginAgent, StringComparison.OrdinalIgnoreCase));
                var shipper = poff.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Shipper, StringComparison.OrdinalIgnoreCase));
                var consignee = poff.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Consignee, StringComparison.OrdinalIgnoreCase));

                //collect all equipment types of the booking
                var equipmentTypes = poff.Loads?.Select(x => EnumHelper<EquipmentType>.GetDisplayDescription(x.EquipmentType)).ToList();

                // TODO: Will change the logic on PSP-1981
                // Send email to Origin Agent about cancellation
                // If stage = FWBRequest and referenceNumber is available (after sent to ediSon or created outport shipment)
                if (poff.Stage == POFulfillmentStage.ForwarderBookingRequest && !string.IsNullOrEmpty(referenceNumber))
                {
                    if (originAgent != null)
                    {
                        var emailModel = new POFulfillmentEmailViewModel()
                        {
                            Name = originAgent.ContactName,
                            BookingRefNumber = referenceNumber,
                            Shipper = shipper?.CompanyName,
                            Consignee = consignee?.CompanyName,
                            ShipFrom = poff.ShipFromName,
                            ShipTo = poff.ShipToName,
                            CargoReadyDate = poff.CargoReadyDate,
                            EquipmentTypes = equipmentTypes,
                            DetailPage = $"{_appConfig.ClientUrl}/po-fulfillments/view/{poff.Id}",
                            SupportEmail = _appConfig.SupportEmail
                        };

                        _queuedBackgroundJobs.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"Booking has been cancelled #{poff.Id}", "Booking_Cancelled",
                            emailModel, originAgent.ContactEmail, $"Shipment Portal: Booking is cancelled ({referenceNumber} - {poff.ShipFromName})"));

                        // Send push notification
                        if (originAgent.OrganizationId != 0)
                        {
                            await _notificationService.PushNotificationSilentAsync(originAgent.OrganizationId, new NotificationViewModel
                            {
                                MessageKey = $"~notification.msg.bookingNo~ <span class=\"k-link\">{poff.Number}</span> ~notification.msg.hasBeenCancelled~.",
                                ReadUrl = $"/po-fulfillments/view/{poff.Id}"
                            });
                        }
                    }
                }

                //clear all shortship orders
                poff.ShortshipOrders.Clear();

                await UnitOfWork.SaveChangesAsync();
                UnitOfWork.CommitTransaction();

                return Mapper.Map<POFulfillmentViewModel>(poff);
            }

            return null;
        }

        public void SendBookedNotificationToOriginAgentEmail(long poffId, string contactName, string contactEmail, string shipmentNumber, string shipFromName)
        {
            _queuedBackgroundJobs.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"Booking has been submitted (Origin Agent) #{poffId}",
                "Booking_BookedNotificationToOriginAgent", new POFulfillmentEmailViewModel
                {
                    Name = contactName,
                    BookingRefNumber = shipmentNumber,
                    DetailPage = $"{_appConfig.ClientUrl}/po-fulfillments/view/{poffId}",
                    SupportEmail = _appConfig.SupportEmail
                },
                contactEmail, $"Shipment Portal: Booking has been submitted ({shipmentNumber} - {shipFromName})"));
        }

        /// <summary>
        /// Create consignment linked to shipment for outport booking
        /// </summary>
        /// <param name="shipments"></param>
        /// <returns></returns>
        private async Task<IEnumerable<ConsignmentModel>> CreateConsignmentsAsync(IEnumerable<ShipmentModel> shipments)
        {
            if (shipments == null && !shipments.Any())
            {
                return null;
            }
            var consignments = new List<ConsignmentModel>();

            foreach (var shipment in shipments)
            {
                var originAgent = shipment.Contacts.Where(c => c.OrganizationRole == Core.Models.OrganizationRole.OriginAgent).FirstOrDefault();
                ConsignmentModel consignment = new ConsignmentModel
                {
                    ConsignmentType = Core.Models.OrganizationRole.OriginAgent,
                    ShipmentId = shipment.Id,
                    ExecutionAgentId = originAgent.OrganizationId,
                    ExecutionAgentName = originAgent.CompanyName,
                    ShipFrom = shipment.ShipFrom,
                    ShipFromETDDate = shipment.ShipFromETDDate,
                    ShipTo = shipment.ShipTo,
                    // Outport shipment, it always has value
                    ShipToETADate = shipment.ShipToETADate.Value,
                    Status = "Active",
                    ModeOfTransport = shipment.ModeOfTransport,
                    Movement = shipment.Movement,
                    Unit = shipment.TotalUnit,
                    UnitUOM = shipment.TotalUnitUOM,
                    Package = shipment.TotalPackage,
                    PackageUOM = shipment.TotalPackageUOM,
                    Volume = shipment.TotalVolume,
                    VolumeUOM = shipment.TotalVolumeUOM,
                    GrossWeight = shipment.TotalGrossWeight,
                    GrossWeightUOM = shipment.TotalGrossWeightUOM,
                    NetWeight = shipment.TotalNetWeight,
                    NetWeightUOM = shipment.TotalNetWeightUOM,
                    TriangleTradeFlag = false,
                    MemoBOLFlag = false,
                    Sequence = 1,
                    ServiceType = shipment.ServiceType,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                consignments.Add(consignment);
            }
            await _consignmentRepository.AddRangeAsync(consignments.ToArray());
            await UnitOfWork.SaveChangesAsync();

            return consignments;
        }

        private async Task CreateContainerAsync(ShipmentModel shipment, POFulfillmentModel poFulfillment, long consignmentId)
        {
            List<ContainerModel> containers = new List<ContainerModel>();
            foreach (var poffLoad in poFulfillment.Loads)
            {
                ContainerModel container = new ContainerModel
                {
                    ContainerNo = poffLoad.LoadReferenceNumber,
                    LoadPlanRefNo = poffLoad.LoadReferenceNumber,
                    ContainerType = poffLoad.EquipmentType.GetAttributeValue<EnumMemberAttribute, string>(x => x.Value),
                    ShipFrom = shipment.ShipFrom,
                    ShipFromETDDate = shipment.ShipFromETDDate,
                    ShipTo = shipment.ShipTo,
                    // Outport flow, it always has value
                    ShipToETADate = shipment.ShipToETADate.Value,
                    SealNo = "",
                    Movement = shipment.Movement,
                    TotalGrossWeight = poffLoad.PlannedGrossWeight,
                    TotalGrossWeightUOM = AppConstant.KILOGGRAMS,
                    TotalNetWeight = poffLoad.PlannedNetWeight != null ? poffLoad.PlannedNetWeight.Value : 0,
                    TotalNetWeightUOM = AppConstant.KILOGGRAMS,
                    TotalPackage = poffLoad.PlannedPackageQuantity,
                    TotalPackageUOM = poffLoad.PackageUOM.ToString(),
                    TotalVolume = poffLoad.PlannedVolume,
                    TotalVolumeUOM = AppConstant.CUBIC_METER,
                    IsFCL = shipment.IsFCL,
                    CreatedBy = shipment.CreatedBy,
                    CreatedDate = shipment.CreatedDate
                };

                await _containerRepository.AddAsync(container);
                containers.Add(container);
            }
            await this.UnitOfWork.SaveChangesAsync();

            foreach (var container in containers)
            {
                ShipmentLoadModel shipmentLoad = new ShipmentLoadModel
                {
                    ContainerId = container.Id,
                    ShipmentId = shipment.Id,
                    ConsignmentId = consignmentId,
                    ModeOfTransport = shipment.ModeOfTransport,
                    IsFCL = true,
                    EquipmentType = container.ContainerType
                };

                await _shipmentLoadRepository.AddAsync(shipmentLoad);
            }
            await this.UnitOfWork.SaveChangesAsync();
        }

        public async Task CheckPOFFDataBeforeCreatingBookingAsync(POFulfillmentModel poff, IdentityInfo currentUser)
        {

            if (poff == null)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }

            if (poff.Stage >= POFulfillmentStage.ForwarderBookingRequest)
            {
                throw new AppValidationException($"Cannot book PO Fulfillment because of stage!");
            }

            var poffId = poff.Id;

            // Check if Delegation shipper is still authorized
            if (currentUser.UserRoleId == (long)Role.Shipper)
            {
                // check on contact from purchase orders
                var purchaseOrderIds = poff.Orders.Select(x => x.PurchaseOrderId).Distinct().ToList();
                var purchaseOrders = await _purchaseOrderRepository.Query(x => purchaseOrderIds.Any(y => y == x.Id), includes: x => x.Include(y => y.Contacts)).ToListAsync();

                foreach (var purchaseOrder in purchaseOrders)
                {
                    var purchaseOrderContacts = purchaseOrder.Contacts;
                    var isAuthorized = purchaseOrderContacts.Any(x => (x.OrganizationRole == OrganizationRole.Supplier || x.OrganizationRole == OrganizationRole.Delegation) && x.OrganizationId == currentUser.OrganizationId);
                    if (!isAuthorized)
                    {
                        throw new AppValidationException($"Unauthorized#You are not authorized to book the PO.");
                    }
                }
            }

            // Check for any purchase order adhoc changes
            var poAdhocChanges = await GetPurchaseOrderAdhocChangesAsync(poffId);
            if (poAdhocChanges != null && poAdhocChanges.Count() > 0)
            {
                var purchaseOrderAdhocChangesTopPriority = poAdhocChanges.OrderBy(x => x.Priority).First();

                if (purchaseOrderAdhocChangesTopPriority.Priority == PurchaseOrderAdhocChangePriority.Level1
                                            || purchaseOrderAdhocChangesTopPriority.Priority == PurchaseOrderAdhocChangePriority.Level2)
                {
                    throw new AppValidationException($"#POAdhocChanged#{purchaseOrderAdhocChangesTopPriority.Priority}#{purchaseOrderAdhocChangesTopPriority.Message}#{string.Join(",", poAdhocChanges.Where(x => x.Priority == purchaseOrderAdhocChangesTopPriority.Priority).Select(x => x.PurchaseOrderId))}");
                }
            }

            // Check for any purchase order line item with balance qty <= 0
            var selectedPOLineItemIds = poff.Orders.Select(x => x.POLineItemId).ToList();
            var isFullyBooked = _poLineItemRepository.GetListQueryable().Any(x => selectedPOLineItemIds.Contains(x.Id) && x.BalanceUnitQty <= 0);
            if (isFullyBooked)
            {
                throw new AppValidationException($"POBookedFully#PO has been fully booked.");
            }

            // Check ordered quantity of all children are equal to mother's
            if (poff.FulfilledFromPOType.Equals(POType.Blanket))
            {
                var validationResult = await ValidateBlanketPurchaseOrderFulfillment(poff.Id);
                if (!string.IsNullOrEmpty(validationResult))
                {
                    throw new AppValidationException($"BlanketBooking#Quantity not matched.");
                }
            }
            else if (poff.FulfilledFromPOType.Equals(POType.Allocated))
            {
                var validationResult = await ValidateAllocatedPurchaseOrderFulfillment(poff.Id);
                if (!string.IsNullOrEmpty(validationResult))
                {
                    throw new AppValidationException($"AllocatedBooking#Quantity not matched.");
                }
            }

        }

        public async Task<POFulfillmentViewModel> CreateBookingAsync(long id, IdentityInfo currentUser)
        {
            var poff = await Repository.GetAsync(x => x.Id == id,
                null,
                x
                => x.Include(m => m.Contacts)
                    .Include(m => m.Loads)
                    .ThenInclude(i => i.Details)
                    .Include(m => m.Orders)
                    .Include(m => m.CargoDetails)
                    .Include(m => m.BookingRequests)
                    .Include(m => m.BuyerApprovals)
                    .Include(m => m.Itineraries)
                    .Include(m => m.Shipments));

            if (poff == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {id} not found!");
            }

            // Check purchase order fulfillment data first, before creating booking
            await CheckPOFFDataBeforeCreatingBookingAsync(poff, currentUser);

            var userName = currentUser.Username;
            var companyName = currentUser.CompanyName;

            UnitOfWork.BeginTransaction();

            // Check for any purchase order adhoc changes
            var poAdhocChanges = await GetPurchaseOrderAdhocChangesAsync(id);
            if (poAdhocChanges != null && poAdhocChanges.Count() > 0)
            {
                var purchaseOrderAdhocChangesTopPriority = poAdhocChanges.OrderBy(x => x.Priority).First();

                if (purchaseOrderAdhocChangesTopPriority.Priority == PurchaseOrderAdhocChangePriority.Level1 ||
                    purchaseOrderAdhocChangesTopPriority.Priority == PurchaseOrderAdhocChangePriority.Level2)
                {
                    throw new AppValidationException($"#POAdhocChanged#{purchaseOrderAdhocChangesTopPriority.Priority}#{purchaseOrderAdhocChangesTopPriority.Message}#{string.Join(",", poAdhocChanges.Where(x => x.Priority == purchaseOrderAdhocChangesTopPriority.Priority).Select(x => x.PurchaseOrderId))}");
                }
                else
                {
                    DeletePurchaseOrderAdhocChanges(0, poff.Id, 0);
                }
            }

            poff.BookingDate = DateTime.UtcNow;

            // Generate Consignee/ Notify Party
            var isNotifyPartyAsConsignee = poff.IsNotifyPartyAsConsignee;
            await GenerateConsigneeNotifyPartyAsync(userName, isNotifyPartyAsConsignee, poff.Contacts);

            await UnitOfWork.SaveChangesAsync();

            // Run booking validation then also execute further process
            await ValidateBookingAsync(poff, userName, companyName);

            UnitOfWork.CommitTransaction();

            return Mapper.Map<POFulfillmentViewModel>(poff);
        }

        public async Task ProceedBookingForPurchaseOrderFulfillment(long poffId, string userName, ActionCalledFrom calledFrom)
        {
            var poff = await Repository.GetAsync(x => x.Id == poffId,
               null,
               x
               => x.Include(m => m.Contacts)
                   .Include(m => m.Loads)
                   .ThenInclude(i => i.Details)
                   .Include(m => m.Orders)
                   .Include(m => m.CargoDetails)
                   .Include(m => m.BookingRequests)
                   .Include(m => m.BuyerApprovals)
                   .Include(m => m.Itineraries)
                   .Include(m => m.Shipments));

            if (poff == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", poffId)} not found!");
            }

            if (poff.Stage != POFulfillmentStage.ForwarderBookingRequest)
            {
                throw new AppValidationException($"Cannot send booking request or create outport shipment because of stage!");
            }

            // If called from approval by users, no need to check Purchase order ad-hoc changes
            if (calledFrom != ActionCalledFrom.ApprovedByUser)
            {
                var poAdhocChange = await GetPurchaseOrderAdhocChangesTopPriorityAsync(poff.Id);

                if (poAdhocChange != null && poAdhocChange.Priority > PurchaseOrderAdhocChangePriority.NotChanged)
                {
                    if (poAdhocChange.Priority == PurchaseOrderAdhocChangePriority.Level1 ||
                        poAdhocChange.Priority == PurchaseOrderAdhocChangePriority.Level2)
                    {
                        throw new AppValidationException($"{poAdhocChange.Priority}#{poAdhocChange.Message}");
                    }
                    else
                    {
                        DeletePurchaseOrderAdhocChanges(0, poff.Id, 0);
                    }
                }
            }

            poff.Audit(userName);

            if (await IsCreateShipmentFromPOFF(poff))
            {
                var shipments = await CreateShipmentsAsync(userName, poff);
                var consignments = await CreateConsignmentsAsync(shipments);

                // Creating container with Single booking only
                if (poff.FulfilledFromPOType.Equals(POType.Bulk))
                {
                    // There is only one shipment and consignment for Single booking
                    var shipment = shipments.First();
                    var consignment = consignments.First();
                    if (shipment.IsFCL)
                    {
                        await CreateContainerAsync(shipment, poff, consignment.Id);
                    }
                    else
                    {
                        ShipmentLoadModel shipmentLoad = new()
                        {
                            ShipmentId = shipment.Id,
                            ConsignmentId = consignment.Id,
                            ModeOfTransport = shipment.ModeOfTransport,
                            IsFCL = false
                        };
                        shipmentLoad.Audit(userName);
                        await _shipmentLoadRepository.AddAsync(shipmentLoad);
                        await this.UnitOfWork.SaveChangesAsync();
                    }
                }
            }
            else
            {
                var existingActiveBookingRequest = poff.BookingRequests.SingleOrDefault(br => br.Status == POFulfillmentBookingRequestStatus.Active);

                if (existingActiveBookingRequest != null)
                {
                    throw new AppValidationException($"Booking already exists for the PO fulfillment.");
                }

                List<POFulfillmentAllocatedOrderViewModel> poffAllocatedOrders = null;
                if (poff.FulfilledFromPOType.Equals(POType.Blanket))
                {
                    // Generate data into table POfulfillmentAllocatedOrders
                    poffAllocatedOrders = await GeneratePOFFAllocatedOrders(poff.Id, userName);

                    if (poffAllocatedOrders == null || !poffAllocatedOrders.Any())
                    {
                        throw new AppException("POFF is blanket but no allocated POs!");
                    }
                }
                var bookingRequest = await _ediSonBookingService.CreateBookingRequestAsync(userName, poff);
                poff.BookingRequests.Add(bookingRequest);

                await UnitOfWork.SaveChangesAsync();

                if (poff.FulfilledFromPOType.Equals(POType.Blanket) && poffAllocatedOrders != null && poffAllocatedOrders.Any())
                {
                    // Append POFulfillmentBookingRequestId for each row on allocated order
                    foreach (var allocatedOrder in poffAllocatedOrders)
                    {
                        allocatedOrder.POFulfillmentBookingRequestId = bookingRequest.Id;
                    }
                    var pofulfillmentAllocatedOrders = Mapper.Map<List<POFulfillmentAllocatedOrderModel>>(poffAllocatedOrders);
                    await _poFulfillmentAllocatedOrderRepository.AddRangeAsync(pofulfillmentAllocatedOrders.ToArray());
                }
            }

            // Update stage on related single / blanket POs
            var poList = await _purchaseOrderRepository.Query(x => poff.Orders.Select(y => y.PurchaseOrderId).Contains(x.Id)).ToListAsync();
            foreach (var po in poList)
            {
                if (po.Stage == POStageType.Released)
                {
                    po.Stage = POStageType.ForwarderBookingRequest;
                }
            }

            #region Update stage for related allocated/blanket POs
            var purchaseOrderListId = new List<long>();

            switch (poff.FulfilledFromPOType)
            {
                case POType.Bulk:
                    break;
                case POType.Blanket:
                    // If it is blanket
                    // Update stage on related allocated POs

                    purchaseOrderListId = poff.Orders.Select(x => x.PurchaseOrderId).Distinct().ToList();
                    var allocatedPOs = await _purchaseOrderRepository.Query(x =>
                                            x.POType == POType.Allocated
                                            && x.BlanketPOId != null
                                            && x.Stage == POStageType.Released
                                            && purchaseOrderListId.Any(y => y == x.BlanketPOId.Value)).ToListAsync();
                    if (allocatedPOs != null && allocatedPOs.Any())
                    {
                        foreach (var allocatedPO in allocatedPOs)
                        {
                            allocatedPO.Stage = POStageType.ForwarderBookingRequest;
                        }
                    }

                    break;
                case POType.Allocated:
                    // If it is allocated purchase order fulfillment
                    // Update stage on related blanket POs
                    purchaseOrderListId = poff.Orders.Select(x => x.PurchaseOrderId).Distinct().ToList();
                    var blanketPOIds = _purchaseOrderRepository.QueryAsNoTracking(x =>
                                           x.POType == POType.Allocated
                                           && x.BlanketPOId != null
                                           && purchaseOrderListId.Any(y => y == x.Id)).Select(x => x.BlanketPOId).Distinct().ToList();

                    var blanketPOs = await _purchaseOrderRepository.Query(x =>
                                            x.POType == POType.Blanket
                                            && x.BlanketPOId == null
                                            && x.Stage == POStageType.Released
                                            && blanketPOIds.Any(y => y == x.Id)).ToListAsync();
                    if (blanketPOs != null && blanketPOs.Any())
                    {
                        foreach (var blanketPO in blanketPOs)
                        {
                            blanketPO.Stage = POStageType.ForwarderBookingRequest;
                        }
                    }
                    break;
                default:
                    break;
            }

            #endregion

            await UnitOfWork.SaveChangesAsync();

            // To generate Shipping Order form then send email
            ProceedShippingOrderFormBackground(ShippingFormType.Booking, poff.Id, userName, poff.FulfillmentType);
        }

        public async Task ConfirmAllocatedPurchaseOrderShipmentAsync(long shipmentId, string userName)
        {
            var shipment = await _shipmentRepository.GetAsync(x => x.Id == shipmentId, null, x => x.Include(m => m.POFulfillmentAllocatedOrders));
            if (shipment == null)
            {
                throw new AppEntityNotFoundException($"Object Shipment not found!");
            }

            shipment.IsItineraryConfirmed = true;
            var purchaseOrderListId = shipment.POFulfillmentAllocatedOrders.Select(x => x.PurchaseOrderId).Distinct().ToList();
            var purchaseOrderList = await _purchaseOrderRepository.Query(x => purchaseOrderListId.Any(y => x.Id == y)).ToListAsync();

            foreach (var item in purchaseOrderList)
            {
                if (item.Stage == POStageType.ForwarderBookingRequest)
                {
                    item.Stage = POStageType.ForwarderBookingConfirmed;
                }
            }

            _purchaseOrderRepository.UpdateRange(purchaseOrderList.ToArray());

            _shipmentRepository.Update(shipment);
        }

        public async Task ConfirmPurchaseOrderFulfillmentAsync(long poffId, string shipmentNumber, string userName)
        {
            var poFulfillment = await Repository.GetAsync(x => x.Id == poffId, null, x => x.Include(m => m.Orders));
            if (poFulfillment == null)
            {
                throw new AppEntityNotFoundException($"Object POFulfillment not found!");
            }

            if (poFulfillment.Stage != POFulfillmentStage.ForwarderBookingRequest)
            {
                throw new AppValidationException($"Cannot confirm PO Fulfillment because of stage!");
            }

            poFulfillment.Stage = POFulfillmentStage.ForwarderBookingConfirmed;
            var purchaseOrderListId = poFulfillment.Orders.Select(x => x.PurchaseOrderId).ToList();
            var purchaseOrderList = await _purchaseOrderRepository.Query(x => purchaseOrderListId.Any(y => x.Id == y),
                null, x => x.Include(y => y.AllocatedPOs)).ToListAsync();

            foreach (var item in purchaseOrderList)
            {
                if (item.Stage == POStageType.ForwarderBookingRequest)
                {
                    item.Stage = POStageType.ForwarderBookingConfirmed;

                    switch (poFulfillment.FulfilledFromPOType)
                    {
                        case POType.Blanket:
                            // Also confirm the child (allocated) POs of this mother (blanket) PO
                            foreach (var allocatedPO in item.AllocatedPOs)
                            {
                                if (allocatedPO.Stage == POStageType.ForwarderBookingRequest)
                                {
                                    allocatedPO.Stage = POStageType.ForwarderBookingConfirmed;
                                }
                            }
                            break;

                        case POType.Allocated:
                            var blanketPOIds = _purchaseOrderRepository.QueryAsNoTracking(x =>
                                              x.POType == POType.Allocated
                                              && x.BlanketPOId != null
                                              && purchaseOrderListId.Any(y => y == x.Id)).Select(x => x.BlanketPOId).Distinct().ToList();

                            var blanketPOs = await _purchaseOrderRepository.Query(x =>
                                                    x.POType == POType.Blanket
                                                    && x.BlanketPOId == null
                                                    && x.Stage == POStageType.ForwarderBookingRequest
                                                    && blanketPOIds.Any(y => y == x.Id)).ToListAsync();

                            if (blanketPOs != null && blanketPOs.Any())
                            {
                                foreach (var blanketPO in blanketPOs)
                                {
                                    if (blanketPO.Stage == POStageType.ForwarderBookingRequest)
                                    {
                                        blanketPO.Stage = POStageType.ForwarderBookingConfirmed;
                                    }
                                }
                                _purchaseOrderRepository.UpdateRange(blanketPOs.ToArray());
                            }
                            break;

                        case POType.Bulk:
                            break;
                        default:
                            break;
                    }
                }
            }

            // Do not mess up with list of events for PO
            var event1061 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1061,
                POFulfillmentId = poFulfillment.Id,
                ActivityDate = DateTime.UtcNow,
                Remark = shipmentNumber,
                CreatedBy = userName
            };
            await _activityService.TriggerAnEvent(event1061);

            _purchaseOrderRepository.UpdateRange(purchaseOrderList.ToArray());
            Repository.Update(poFulfillment);

            ProceedShippingOrderFormBackground(ShippingFormType.ShippingOrder, poFulfillment.Id, userName, poFulfillment.FulfillmentType);
        }

        public async Task DispatchAllocatedPurchaseOrderShipmentAsync(long shipmentId, string userName, DateTime eventDate, string location = null)
        {
            var shipment = await _shipmentRepository.GetAsync(x => x.Id == shipmentId, null, x => x.Include(m => m.POFulfillmentAllocatedOrders));
            if (shipment == null)
            {
                throw new AppEntityNotFoundException($"Object Shipment not found!");
            }

            var purchaseOrderListId = shipment.POFulfillmentAllocatedOrders.Select(x => x.PurchaseOrderId).Distinct().ToList();
            var purchaseOrderList = await _purchaseOrderRepository.Query(x => purchaseOrderListId.Any(y => x.Id == y)).ToListAsync();

            var poEventList = new List<ActivityViewModel>();
            foreach (var item in purchaseOrderList)
            {
                if (item.Stage == POStageType.ForwarderBookingConfirmed)
                {
                    item.Stage = POStageType.ShipmentDispatch;

                    // Trigger 1009-PM-Shipment Dispatch event into all associated POs
                    var event1009 = new ActivityViewModel()
                    {
                        ActivityCode = Event.EVENT_1009,
                        PurchaseOrderId = item.Id,
                        ActivityDate = eventDate,
                        Location = location,
                        CreatedBy = userName
                    };
                    poEventList.Add(event1009);
                }
            }
            await _activityService.TriggerEventList(poEventList);
            _purchaseOrderRepository.UpdateRange(purchaseOrderList.ToArray());
        }

        /// <summary>
        /// To create shipment(s) for outport booking. It works for both single/blanket/allocated booking.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="poff"></param>
        /// <returns></returns>
        private async Task<IEnumerable<ShipmentModel>> CreateShipmentsAsync(string userName, POFulfillmentModel poff)
        {
            if (poff == null)
            {
                throw new AppEntityNotFoundException(nameof(poff));
            }

            var result = new List<ShipmentModel>();
            #region Single booking
            if (poff.FulfilledFromPOType.Equals(POType.Bulk))
            {
                var shipment = await CreateShipmentForSingleOrAllocatedBookingAsync(userName, poff);
                result.Add(shipment);

            }
            #endregion

            #region Blanket booking
            else if (poff.FulfilledFromPOType.Equals(POType.Blanket))
            {
                var shipments = await CreateShipmentsForBlanketBookingAsync(userName, poff);
                result.AddRange(shipments);
            }
            #endregion

            #region Allocated booking
            if (poff.FulfilledFromPOType.Equals(POType.Allocated))
            {
                var shipment = await CreateShipmentForSingleOrAllocatedBookingAsync(userName, poff);
                result.Add(shipment);

            }
            #endregion
            return result;
        }

        /// <summary>
        /// To create shipment(s) for outport booking. It works for booking fulfilled from Bulk or Allocated Purchase Order(s)
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="poff"></param>
        /// <returns></returns>
        private async Task<ShipmentModel> CreateShipmentForSingleOrAllocatedBookingAsync(string userName, POFulfillmentModel poff)
        {
            if (poff == null)
            {
                throw new AppEntityNotFoundException(nameof(poff));
            }

            // FulfilledFromPOType must be null, single or allocated
            if (!poff.FulfilledFromPOType.Equals(POType.Bulk)
                && !poff.FulfilledFromPOType.Equals(POType.Allocated))
            {
                throw new AppException("POFF must be fulfilled from bulk or allocated purchase order(s)!");
            }

            // For fix duplicate PO Number in CustomerReferenceNo in the created shipment
            var customerPONumbers = poff.Orders.Select(x => x.CustomerPONumber).Distinct();

            var shipment = new ShipmentModel
            {
                BookingNo = poff.Number + $"{poff.Shipments.Count + 1:00}",
                POFulfillment = poff,
                ShipFrom = (await _csfeApiClient.GetLocationByIdAsync(poff.ShipFrom)).LocationDescription,
                ShipTo = (await _csfeApiClient.GetLocationByIdAsync(poff.ShipTo)).LocationDescription,
                Incoterm = Enum.GetName(typeof(IncotermType), poff.Incoterm),
                ModeOfTransport = await _translation.GetTranslationByKeyAsync(EnumHelper<ModeOfTransportType>.GetDisplayName(poff.ModeOfTransport),
                              TranslationUtility.ENGLISH_KEY),
                Movement = EnumHelper<MovementType>.GetDisplayName(poff.MovementType),
                ServiceType = await _translation.GetTranslationByKeyAsync(EnumHelper<LogisticsServiceType>.GetDisplayName(poff.LogisticsService),
                              TranslationUtility.ENGLISH_KEY),
                TotalNetWeight = poff.Loads.Where(l => l.PlannedNetWeight != null).Sum(l => l.PlannedNetWeight.Value),
                TotalNetWeightUOM = AppConstant.KILOGGRAMS,
                TotalGrossWeight = poff.Loads.Sum(l => l.PlannedGrossWeight),
                TotalGrossWeightUOM = AppConstant.KILOGGRAMS,
                TotalPackage = poff.Loads.Sum(l => l.PlannedPackageQuantity),
                TotalPackageUOM = Enum.GetName(typeof(PackageUOMType), poff.Loads.FirstOrDefault()?.PackageUOM),
                TotalVolume = poff.Loads.Sum(l => l.PlannedVolume),
                TotalVolumeUOM = AppConstant.CUBIC_METER,
                CustomerReferenceNo = string.Join(",", customerPONumbers),
                CargoReadyDate = poff.CargoReadyDate ?? DateTime.MinValue,
                BookingDate = poff.BookingDate.Value,
                ShipFromETDDate = poff.ExpectedShipDate ?? DateTime.MinValue,
                ShipToETADate = poff.ExpectedDeliveryDate ?? DateTime.MinValue,
                TotalUnit = poff.Orders.Sum(o => o.FulfillmentUnitQty),
                TotalUnitUOM = AppConstant.PIECES,
                Status = StatusType.ACTIVE,
                CreatedBy = userName,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = userName,
                UpdatedDate = DateTime.UtcNow
            };

            shipment.ShipmentNo = shipment.BookingNo;

            if (poff.MovementType == MovementType.CY_CY ||
                poff.MovementType == MovementType.CY_CFS)
            {
                shipment.IsFCL = true;
            }
            else
            {
                shipment.IsFCL = false;
            }

            #region Create ShipmentContacts
            shipment.Contacts = new List<ShipmentContactModel>();

            foreach (var poffContact in poff.Contacts)
            {
                var shipmentContact = new ShipmentContactModel
                {
                    OrganizationId = poffContact.OrganizationId,
                    CompanyName = poffContact.CompanyName,
                    OrganizationRole = poffContact.OrganizationRole,
                    ContactName = poffContact.ContactName,
                    ContactNumber = poffContact.ContactNumber,
                    ContactEmail = poffContact.ContactEmail,
                    Address = ConcatenateCompanyAddressLinesResolver.ConcatenateCompanyAddressLines(poffContact.Address, poffContact.AddressLine2, poffContact.AddressLine3, poffContact.AddressLine4),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userName
                };

                shipment.Contacts.Add(shipmentContact);
            }
            #endregion

            /**Note: Because of the business rule of the trg_CargoDetails trigger,
             * we need to save the CargoDetails after we have created the ShipmentContacts */

            await _shipmentRepository.AddAsync(shipment);
            await UnitOfWork.SaveChangesAsync();

            #region Create CargoDetails from CustomerPO of booking

            var newCargoDetails = new List<CargoDetailModel>();

            var poLineItems = await _poLineItemRepository.QueryAsNoTracking(x
                => poff.Orders.Select(o => o.POLineItemId).Any(id => id == x.Id), null, y => y.Include(i => i.PurchaseOrder))
                .ToListAsync();

            var poffOrders = poff.Orders.OrderBy(x => x.Id);
            var cargoDetailSequence = 1;
            foreach (var poffOrder in poffOrders)
            {
                var poLineItem = poLineItems.SingleOrDefault(x => x.Id == poffOrder.POLineItemId);
                var cargoDetail = new CargoDetailModel
                {
                    ShipmentId = shipment.Id,
                    Sequence = cargoDetailSequence,
                    ShippingMarks = poffOrder.ShippingMarks,
                    Description = poLineItem.DescriptionOfGoods,
                    Unit = poffOrder.FulfillmentUnitQty,
                    UnitUOM = Enum.GetName(typeof(UnitUOMType), poffOrder.UnitUOM),
                    GrossWeight = !(poffOrder.GrossWeight.HasValue) ? 0 : poffOrder.GrossWeight.Value,
                    GrossWeightUOM = AppConstant.KILOGGRAMS,
                    NetWeight = poffOrder.NetWeight,
                    NetWeightUOM = AppConstant.KILOGGRAMS,
                    Package = !(poffOrder.BookedPackage.HasValue) ? 0 : poffOrder.BookedPackage.Value,
                    PackageUOM = Enum.GetName(typeof(PackageUOMType), poffOrder.PackageUOM),
                    Volume = !(poffOrder.Volume.HasValue) ? 0 : poffOrder.Volume.Value,
                    VolumeUOM = AppConstant.CUBIC_METER,
                    Commodity = poffOrder.Commodity,
                    HSCode = poffOrder.HsCode,
                    ProductNumber = $"{poLineItem.PurchaseOrder.PONumber}~{poLineItem.ProductCode}~{poLineItem.LineOrder}",
                    ItemId = poffOrder.POLineItemId,
                    OrderId = poffOrder.PurchaseOrderId,
                    OrderType = OrderType.Freight,
                    CountryOfOrigin = poffOrder.CountryCodeOfOrigin
                };
                if (poLineItem.ScheduleLineNo.HasValue)
                {
                    cargoDetail.ProductNumber += $"~{poLineItem.ScheduleLineNo.Value}";
                }
                cargoDetail.Audit(userName);
                newCargoDetails.Add(cargoDetail);
                cargoDetailSequence += 1;
            }
            await _cargoDetailRepository.AddRangeAsync(newCargoDetails.ToArray());
            await UnitOfWork.SaveChangesAsync();
            #endregion

            var event2005 = await _csfeApiClient.GetEventByCodeAsync(Event.EVENT_2005);
            var activity2005 = new ActivityViewModel
            {
                ActivityCode = Event.EVENT_2005,
                ActivityDescription = event2005.ActivityDescription,
                ActivityType = event2005.ActivityType,
                ShipmentId = shipment.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = userName,
                CreatedDate = DateTime.UtcNow
            };

            activity2005.FieldStatus = new Dictionary<string, FieldDeserializationStatus>
                {
                    { "ActivityCode", FieldDeserializationStatus.HasValue },
                    { "ActivityDescription", FieldDeserializationStatus.HasValue },
                    { "ActivityType", FieldDeserializationStatus.HasValue },
                    { "ShipmentId", FieldDeserializationStatus.HasValue },
                    { "CreatedBy", FieldDeserializationStatus.HasValue },
                    { "CreatedDate", FieldDeserializationStatus.HasValue },
                    { "ActivityDate", FieldDeserializationStatus.HasValue }
                };

            await _activityService.CreateAsync(activity2005);
            return shipment;
        }

        /// <summary>
        /// To create shipment(s) for outport booking. It works for only booking fulfilled from Blanket Purchase Order(s)
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="poff"></param>
        /// <returns></returns>
        private async Task<IEnumerable<ShipmentModel>> CreateShipmentsForBlanketBookingAsync(string userName, POFulfillmentModel poff)
        {
            if (poff == null)
            {
                throw new AppEntityNotFoundException(nameof(poff));
            }

            if (!poff.FulfilledFromPOType.Equals(POType.Blanket))
            {
                throw new AppException("POFF is not blanket!");
            }

            var poffAllocatedOrders = await GeneratePOFFAllocatedOrders(poff.Id, userName);

            if (poffAllocatedOrders == null || !poffAllocatedOrders.Any())
            {
                throw new AppException("POFF is blanket but no allocated POs!");
            }

            // Group allocated POs by ShipTo, then create shipment for each one.
            var groupByShipTo = poffAllocatedOrders.GroupBy(x => x.ShipToId);

            var currentShipmentSequence = poff.Shipments == null || !poff.Shipments.Any()
                    ? 0
                    : poff.Shipments.Where(x => !string.IsNullOrEmpty(x.ShipmentNo))
                       .Select(x => x.ShipmentNo)
                       .Max(x => int.Parse(x.Substring(x.Length - 2)));

            var shipments = new List<ShipmentModel>();
            var newCargoDetails = new List<CargoDetailModel>();
            foreach (var item in groupByShipTo)
            {
                var shipToId = item.Key;
                var shipToLocation = await _csfeApiClient.GetLocationByIdAsync(shipToId);
                var allocatedOrders = item.ToList();

                // For fix duplicate PO Number in CustomerReferenceNo in the created shipment
                var customerPONumbers = allocatedOrders.Select(x => x.CustomerPONumber).Distinct();

                var shipment = new ShipmentModel
                {
                    BookingNo = poff.Number + $"{shipToLocation.Name.Substring(shipToLocation.Name.Length - 3)}" + $"{currentShipmentSequence + 1:00}",
                    POFulfillment = poff,
                    ShipFrom = (await _csfeApiClient.GetLocationByIdAsync(poff.ShipFrom)).LocationDescription,
                    ShipTo = shipToLocation.LocationDescription,
                    Incoterm = Enum.GetName(typeof(IncotermType), poff.Incoterm),
                    ModeOfTransport = await _translation.GetTranslationByKeyAsync(EnumHelper<ModeOfTransportType>.GetDisplayName(poff.ModeOfTransport),
                                  TranslationUtility.ENGLISH_KEY),
                    Movement = EnumHelper<MovementType>.GetDisplayName(poff.MovementType),
                    ServiceType = await _translation.GetTranslationByKeyAsync(EnumHelper<LogisticsServiceType>.GetDisplayName(poff.LogisticsService),
                                  TranslationUtility.ENGLISH_KEY),
                    TotalNetWeight = allocatedOrders.Where(l => l.NetWeight != null).Sum(l => l.NetWeight.Value),
                    TotalNetWeightUOM = AppConstant.KILOGGRAMS,
                    TotalGrossWeight = allocatedOrders.Where(l => l.GrossWeight != null).Sum(l => l.GrossWeight.Value),
                    TotalGrossWeightUOM = AppConstant.KILOGGRAMS,
                    TotalPackage = allocatedOrders.Where(l => l.BookedPackage != null).Sum(l => l.BookedPackage.Value),
                    TotalPackageUOM = Enum.GetName(typeof(PackageUOMType), poff.Loads.FirstOrDefault()?.PackageUOM),
                    TotalVolume = allocatedOrders.Where(l => l.Volume != null).Sum(l => l.Volume.Value),
                    TotalVolumeUOM = AppConstant.CUBIC_METER,
                    CustomerReferenceNo = string.Join(",", customerPONumbers),
                    CargoReadyDate = poff.CargoReadyDate ?? DateTime.MinValue,
                    BookingDate = poff.BookingDate.Value,
                    ShipFromETDDate = allocatedOrders.Where(x => x.CargoReadyDate != null)?.Min(l => l.CargoReadyDate) ?? DateTime.MinValue,
                    ShipToETADate = allocatedOrders.Where(x => x.ExpectedDeliveryDate != null)?.Min(l => l.ExpectedDeliveryDate) ?? DateTime.MinValue,
                    TotalUnit = allocatedOrders.Sum(l => l.BookedQty),
                    TotalUnitUOM = AppConstant.PIECES,
                    Status = StatusType.ACTIVE,
                    CreatedBy = userName,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = userName,
                    UpdatedDate = DateTime.UtcNow
                };

                shipment.ShipmentNo = shipment.BookingNo;

                if (poff.MovementType == MovementType.CY_CY ||
                    poff.MovementType == MovementType.CY_CFS)
                {
                    shipment.IsFCL = true;
                }
                else
                {
                    shipment.IsFCL = false;
                }

                shipment.Contacts = new List<ShipmentContactModel>();

                foreach (var poffContact in poff.Contacts)
                {
                    var shipmentContact = new ShipmentContactModel
                    {
                        OrganizationId = poffContact.OrganizationId,
                        CompanyName = poffContact.CompanyName,
                        OrganizationRole = poffContact.OrganizationRole,
                        ContactName = poffContact.ContactName,
                        ContactNumber = poffContact.ContactNumber,
                        ContactEmail = poffContact.ContactEmail,
                        Address = ConcatenateCompanyAddressLinesResolver.ConcatenateCompanyAddressLines(poffContact.Address, poffContact.AddressLine2, poffContact.AddressLine3, poffContact.AddressLine4),
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = userName
                    };

                    shipment.Contacts.Add(shipmentContact);
                }

                #region Create Cargo Details

                var lineItems = await _poLineItemRepository.QueryAsNoTracking(x
                    => allocatedOrders.Select(o => o.POLineItemId).Any(id => id == x.Id), null, y => y.Include(i => i.PurchaseOrder))
                    .ToListAsync();

                // Order by Id to generate sequence number for the Cargo Detail
                allocatedOrders = allocatedOrders.OrderBy(x => x.Id).ToList();
                var cargoDetailSequence = 1;

                foreach (var allocatedOrder in allocatedOrders)
                {
                    var lineItem = lineItems.SingleOrDefault(x => x.Id == allocatedOrder.POLineItemId);
                    var poffOrder = poff.Orders.First(x => x.ProductCode.Equals(allocatedOrder.ProductCode, StringComparison.InvariantCultureIgnoreCase));
                    var cargoDetail = new CargoDetailModel()
                    {
                        Sequence = cargoDetailSequence,
                        ShippingMarks = poffOrder.ShippingMarks,
                        Description = lineItem.DescriptionOfGoods,
                        Unit = allocatedOrder.BookedQty,
                        UnitUOM = Enum.GetName(typeof(UnitUOMType), poffOrder.UnitUOM),
                        GrossWeight = !(allocatedOrder.GrossWeight.HasValue) ? 0 : allocatedOrder.GrossWeight.Value,
                        GrossWeightUOM = AppConstant.KILOGGRAMS,
                        NetWeight = allocatedOrder.NetWeight,
                        NetWeightUOM = AppConstant.KILOGGRAMS,
                        Package = !(allocatedOrder.BookedPackage.HasValue) ? 0 : allocatedOrder.BookedPackage.Value,
                        PackageUOM = Enum.GetName(typeof(PackageUOMType), poffOrder.PackageUOM),
                        Volume = !(allocatedOrder.Volume.HasValue) ? 0 : allocatedOrder.Volume.Value,
                        VolumeUOM = AppConstant.CUBIC_METER,
                        Commodity = poffOrder.Commodity,
                        HSCode = poffOrder.HsCode,
                        ProductNumber = $"{lineItem.PurchaseOrder.PONumber}~{lineItem.ProductCode}~{lineItem.LineOrder}",
                        ItemId = allocatedOrder.POLineItemId,
                        OrderId = allocatedOrder.PurchaseOrderId,
                        OrderType = OrderType.Freight,
                        CountryOfOrigin = poffOrder.CountryCodeOfOrigin,
                        Shipment = shipment
                    };
                    if (lineItem.ScheduleLineNo.HasValue)
                    {
                        cargoDetail.ProductNumber += $"~{lineItem.ScheduleLineNo.Value}";
                    }
                    cargoDetail.Audit(userName);
                    newCargoDetails.Add(cargoDetail);
                    cargoDetailSequence += 1;

                    // Use this property to link shipmentid to pofulfillmentallocatedorder
                    allocatedOrder.ShipToLocationDescription = shipment.ShipTo;
                }
                #endregion

                shipments.Add(shipment);

            }

            await _shipmentRepository.AddRangeAsync(shipments.ToArray());
            await UnitOfWork.SaveChangesAsync();

            /**Note: Because of the business-rule of the trg_CargoDetails,
             * we need to save the CargoDetails after we have created the ShipmentContacts */

            foreach (var newCargoDetail in newCargoDetails)
            {
                newCargoDetail.ShipmentId = newCargoDetail.Shipment.Id;
            }
            await _cargoDetailRepository.AddRangeAsync(newCargoDetails.ToArray());
            await UnitOfWork.SaveChangesAsync();

            foreach (var shipment in shipments)
            {
                var event2005 = await _csfeApiClient.GetEventByCodeAsync(Event.EVENT_2005);
                var activity2005 = new ActivityViewModel
                {
                    ActivityCode = Event.EVENT_2005,
                    ActivityDescription = event2005.ActivityDescription,
                    ActivityType = event2005.ActivityType,
                    ShipmentId = shipment.Id,
                    ActivityDate = DateTime.UtcNow,
                    CreatedBy = userName,
                    CreatedDate = DateTime.UtcNow
                };

                activity2005.FieldStatus = new Dictionary<string, FieldDeserializationStatus>
                {
                    { "ActivityCode", FieldDeserializationStatus.HasValue },
                    { "ActivityDescription", FieldDeserializationStatus.HasValue },
                    { "ActivityType", FieldDeserializationStatus.HasValue },
                    { "ShipmentId", FieldDeserializationStatus.HasValue },
                    { "CreatedBy", FieldDeserializationStatus.HasValue },
                    { "CreatedDate", FieldDeserializationStatus.HasValue },
                    { "ActivityDate", FieldDeserializationStatus.HasValue }
                };
                await _activityService.CreateAsync(activity2005);
            }

            // Store data into table POfulfillmentAllocatedOrders
            // Append ShipmentId for each row on allocated order
            foreach (var allocatedOrder in poffAllocatedOrders)
            {
                allocatedOrder.ShipmentId = shipments.Where(x => x.ShipTo.Equals(allocatedOrder.ShipToLocationDescription)).First().Id;
            }

            var pofulfillmentAllocatedOrders = Mapper.Map<List<POFulfillmentAllocatedOrderModel>>(poffAllocatedOrders);

            await _poFulfillmentAllocatedOrderRepository.AddRangeAsync(pofulfillmentAllocatedOrders.ToArray());
            await UnitOfWork.SaveChangesAsync();

            return shipments;
        }

        public async Task AmendPOFulfillmentAsync(long poffId, string userName)
        {
            var poAdhocChanges = await GetPurchaseOrderAdhocChangesAsync(poffId);

            UnitOfWork.BeginTransaction();

            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
            => x.Include(m => m.Contacts)
                .Include(m => m.Loads)
                .Include(m => m.Orders)
                .Include(m => m.ShortshipOrders)
                .Include(m => m.BookingRequests)
                .Include(m => m.BuyerApprovals)
                .Include(m => m.Itineraries)
                .Include(m => m.Shipments)
                .ThenInclude(i => i.Consignments)
                .Include(m => m.Shipments)
                .ThenInclude(s => s.ShipmentLoads).ThenInclude(sl => sl.ShipmentLoadDetails);

            var poff = await Repository.GetAsync(x => x.Id == poffId, null, includeProperties);

            if (poff == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {poffId} not found!");
            }

            if (poff.Stage != POFulfillmentStage.ForwarderBookingRequest)
            {
                throw new AppValidationException("Cannot amend PO Fulfillment. Stage must be FB Request!");
            }

            if (poAdhocChanges != null && poAdhocChanges.Count() > 0)
            {
                var purchaseOrderAdhocChangesTopPriority = poAdhocChanges.OrderBy(x => x.Priority).First();

                if (purchaseOrderAdhocChangesTopPriority.Priority == PurchaseOrderAdhocChangePriority.Level1)
                {
                    throw new AppValidationException($"#POAdhocChanged#{purchaseOrderAdhocChangesTopPriority.Priority}#{purchaseOrderAdhocChangesTopPriority.Message}#{string.Join(",", poAdhocChanges.Where(x => x.Priority == purchaseOrderAdhocChangesTopPriority.Priority).Select(x => x.PurchaseOrderId))}");
                }
            }

            poff.Audit(userName);
            poff.IsRejected = false;
            poff.IsGeneratePlanToShip = false;
            poff.Stage = POFulfillmentStage.Draft;
            poff.BookingDate = null;

            // Update quantity for related single POs
            ReleaseQuantityOnPOLineItems(poffId);

            // Cancel current Booking Request
            var bookingRequest = poff.BookingRequests.SingleOrDefault(br => br.Status == POFulfillmentBookingRequestStatus.Active);
            if (bookingRequest != null)
            {
                bookingRequest.Status = POFulfillmentBookingRequestStatus.Inactive;
                await _ediSonBookingService.CancelBookingRequest(bookingRequest);
            }

            poff.IsForwarderBookingItineraryReady = false;
            foreach (var itinerary in poff.Itineraries)
            {
                itinerary.Status = POFulfillmentItinerayStatus.Inactive;
            }

            foreach (var load in poff.Loads)
            {
                load.ContainerNumber = null;
                load.SealNumber = null;
                load.TotalGrossWeight = null;
                load.TotalNetWeight = null;
            }

            await UpdatePurchaseOrderStageByPOFFAsync(poff);


            // Cancel pending approval
            var pendingBuyerApproval = poff.BuyerApprovals.SingleOrDefault(x => x.Stage == BuyerApprovalStage.Pending);
            if (pendingBuyerApproval != null)
            {
                pendingBuyerApproval.Stage = BuyerApprovalStage.Cancel;
                pendingBuyerApproval.ResponseOn = DateTime.UtcNow;
            }

            var event1056 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1056,
                POFulfillmentId = poff.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = userName
            };
            event1056.Audit(userName);
            await _activityService.TriggerAnEvent(event1056);

            #region Deactivate shipments
            // Set Shipments' status to Inactive
            var shipments = poff.Shipments?.Where(x => x.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase));
            var shipment = shipments?.FirstOrDefault();

            if (shipments != null)
            {
                foreach (var item in shipments)
                {
                    var consignmentNumbers = string.Empty;

                    foreach (var consigment in item.Consignments)
                    {
                        consigment.Status = StatusType.INACTIVE;
                        consignmentNumbers = consignmentNumbers + consigment.Id + ", ";
                    }

                    var containerIdList = item.ShipmentLoads.Select(sl => sl.ContainerId);
                    var containers = await _containerRepository.Query(c => containerIdList.Contains(c.Id),
                        null,
                        i => i.Include(c => c.ContainerItineraries)).ToListAsync();

                    var shipmentLoads = item.ShipmentLoads;

                    #region Data hard-delete on containers and shipment loads

                    if ((containers != null && containers.Any()) || (shipmentLoads != null && shipmentLoads.Any()))
                    {
                        // Attentions: It is going to permanently remove data
                        // Pls add business information logging                

                        var telemetry = new TraceTelemetry("Data hard-delete", SeverityLevel.Information);
                        telemetry.Properties.Add("executed-on", _telemetryConfig.Source);
                        telemetry.Properties.Add("executed-component", "POFulfillmentService");
                        telemetry.Properties.Add("executed-method", "AmendPOFulfillmentAsync");
                        telemetry.Properties.Add("params-poffId", poffId.ToString());
                        telemetry.Properties.Add("params-userName", userName);


                        // Remove related containers
                        foreach (var container in containers)
                        {
                            // create object to log
                            dynamic containerLog = new
                            {
                                container.Id,
                                container.ContainerNo,
                                container.LoadPlanRefNo,
                                container.ContainerType,
                                container.ShipFrom,
                                container.ShipFromETDDate,
                                container.ShipTo,
                                container.ShipToETADate,
                                container.SealNo,
                                container.Movement,
                                container.TotalGrossWeight,
                                container.TotalGrossWeightUOM,
                                container.TotalNetWeight,
                                container.TotalNetWeightUOM,
                                container.TotalPackage,
                                container.TotalPackageUOM,
                                container.TotalVolume,
                                container.TotalVolumeUOM,
                                container.IsFCL,
                                ContainerItineraries = container.ContainerItineraries.Select(x => new { x.ItineraryId })
                            };
                            telemetry.Properties.Add($"data-container-{container.Id}", JsonConvert.SerializeObject(containerLog, settings: new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));

                            container.ContainerItineraries.Clear();
                            _containerRepository.Remove(container);
                        }

                        // Remove related shipment loads
                        foreach (var shipmentLoad in shipmentLoads)
                        {
                            // create object to log
                            dynamic shipmentLoadLog = new
                            {
                                shipmentLoad.Id,
                                shipmentLoad.ShipmentId,
                                shipmentLoad.ConsignmentId,
                                shipmentLoad.ConsolidationId,
                                shipmentLoad.ContainerId,
                                shipmentLoad.ModeOfTransport,
                                shipmentLoad.CarrierBookingNo,
                                shipmentLoad.IsFCL,
                                shipmentLoad.LoadingPartyId,
                                shipmentLoad.EquipmentType,
                                ShipmentLoadDetails = shipmentLoad.ShipmentLoadDetails.Select(x =>
                                new
                                {
                                    x.Id,
                                    x.ShipmentId,
                                    x.ConsignmentId,
                                    x.CargoDetailId,
                                    x.ShipmentLoadId,
                                    x.ContainerId,
                                    x.ConsolidationId,
                                    x.Package,
                                    x.PackageUOM,
                                    x.Unit,
                                    x.UnitUOM,
                                    x.Volume,
                                    x.VolumeUOM,
                                    x.GrossWeight,
                                    x.GrossWeightUOM,
                                    x.NetWeight,
                                    x.NetWeightUOM,
                                    x.Sequence
                                })
                            };
                            telemetry.Properties.Add($"data-shipmentLoad-{shipmentLoad.Id}", JsonConvert.SerializeObject(shipmentLoadLog, settings: new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));

                            shipmentLoad.ContainerId = null;
                            shipmentLoad.ShipmentLoadDetails.Clear();
                        }

                        // Write the log to Telemetry
                        TelemetryClient.TrackTrace(telemetry);
                    }

                    #endregion

                    item.SetCancelledStatus();
                }

            }
            #endregion Deactivate shipments

            // Send email to Origin Agent about Amendment
            // If referenceNumber is available (after sent to ediSon or created outport shipment)

            // TODO: Will change the logic on PSP-1981
            var referenceNumber = shipment != null ? shipment.ShipmentNo
               : (bookingRequest != null ? bookingRequest.BookingReferenceNumber : string.Empty);

            //collect all equipment types of the booking
            var equipmentTypes = poff.Loads?.Select(x => EnumHelper<EquipmentType>.GetDisplayDescription(x.EquipmentType)).ToList();

            var originAgent = poff.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.OriginAgent, StringComparison.OrdinalIgnoreCase));
            var shipper = poff.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Shipper, StringComparison.OrdinalIgnoreCase));
            var consignee = poff.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Consignee, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(referenceNumber))
            {
                var bookingEmailModel = new POFulfillmentEmailViewModel()
                {
                    Name = originAgent.ContactName,
                    BookingRefNumber = referenceNumber,
                    Shipper = shipper?.CompanyName,
                    Consignee = consignee?.CompanyName,
                    ShipFrom = poff.ShipFromName,
                    ShipTo = poff.ShipToName,
                    CargoReadyDate = poff.CargoReadyDate,
                    EquipmentTypes = equipmentTypes,
                    DetailPage = $"{_appConfig.ClientUrl}/po-fulfillments/view/{poff.Id}",
                    SupportEmail = _appConfig.SupportEmail
                };
                _queuedBackgroundJobs.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"Booking has been amended #{poff.Id}", "Booking_Amended", bookingEmailModel,
                    originAgent.ContactEmail, $"Shipment Portal: Booking is amended ({referenceNumber} - {poff.ShipFromName})"));

                // Send push notification
                await _notificationService.PushNotificationSilentAsync(originAgent.OrganizationId, new NotificationViewModel
                {
                    MessageKey = $"~notification.msg.bookingNo~ <span class=\"k-link\">{poff.Number}</span> ~notification.msg.hasBeenAmended~.",
                    ReadUrl = $"/po-fulfillments/view/{poff.Id}"
                });

            }

            //clear all shortship orders
            poff.ShortshipOrders.Clear();

            await UnitOfWork.SaveChangesAsync();
            UnitOfWork.CommitTransaction();
        }

        public void ProceedShippingOrderFormBackground(ShippingFormType formType, long poFulfillmentId, string userName, FulfillmentType fulfillmentType)
        {
            string jobDescription = formType.Equals(ShippingFormType.Booking) ? "Booking form" : "Shipping Order form";
            // It will be executed in back-ground
            _queuedBackgroundJobs.Enqueue<ProceedShippingOrderForm>(j => j.ExecuteAsync(jobDescription, poFulfillmentId, userName, formType, fulfillmentType));
        }

        private async Task GeneratePackingListAttachment(POFulfillmentModel poFulfillment, string userName)
        {
            var attachment = new AttachmentViewModel()
            {
                AttachmentType = AttachmentType.PACKING_LIST,
                POFulfillmentId = poFulfillment.Id,
                Description = userName,
                UploadedBy = AppConstant.SYSTEM_USERNAME,
                UploadedDateTime = DateTime.UtcNow,
                BlobId = _appConfig.BlobStorage.POFulfillment.PackingListTemplate,
                FileName = "Packing List.pdf"
            };
            await _attachmentService.ImportAttachmentAsync(attachment);
        }

        private async Task GenerateShippingInvoiceAttachment(POFulfillmentModel poFulfillment, string userName)
        {
            var attachment = new AttachmentViewModel()
            {
                AttachmentType = AttachmentType.COMMERCIAL_INVOICE,
                POFulfillmentId = poFulfillment.Id,
                Description = userName,
                UploadedBy = AppConstant.SYSTEM_USERNAME,
                UploadedDateTime = DateTime.UtcNow,
                BlobId = _appConfig.BlobStorage.POFulfillment.ShippingInvoiceTemplate,
                FileName = "Commercial Invoice.pdf"
            };
            await _attachmentService.ImportAttachmentAsync(attachment);
        }

        public async Task ReloadAsync(long id, InputPOFulfillmentViewModel model, IdentityInfo currentUser)
        {
            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
            => x.Include(m => m.Contacts)
                .Include(m => m.Loads)
                .ThenInclude(i => i.Details)
                .Include(m => m.Orders)
                .Include(m => m.CargoDetails)
                .Include(m => m.BookingRequests)
                .Include(m => m.BuyerApprovals)
                .Include(m => m.Itineraries)
                .Include(m => m.Shipments)
                .ThenInclude(s => s.CargoDetails);

            var poFulfillment = await Repository.GetAsync(x => x.Id == id, null, includeProperties);

            var allowedStages = new POFulfillmentStage[] { POFulfillmentStage.ForwarderBookingConfirmed, POFulfillmentStage.ShipmentDispatch };

            if (poFulfillment == null || !allowedStages.Contains(poFulfillment.Stage))
            {
                throw new AppEntityNotFoundException($"Object with the id {id} not found!");
            }

            if (poFulfillment.MovementType == MovementType.CFS_CY)
            {
                throw new AppValidationException($"Cannot plan to ship Booking. MovementType must be CY!");
            }

            UnitOfWork.BeginTransaction();

            #region Update POFulfillment Loads and LoadDetails
            var deletedLoadDetails = new ArrayList();
            foreach (var load in poFulfillment.Loads)
            {
                var viewModelLoad = model.Loads.FirstOrDefault(x => x.Id == load.Id);
                if (viewModelLoad != null)
                {
                    load.LoadReferenceNumber = viewModelLoad.LoadReferenceNumber;
                    load.EquipmentType = viewModelLoad.EquipmentType;
                    load.PlannedVolume = viewModelLoad.PlannedVolume;
                    load.PlannedNetWeight = viewModelLoad.PlannedNetWeight;
                    load.PlannedGrossWeight = viewModelLoad.PlannedGrossWeight;
                    load.PlannedPackageQuantity = viewModelLoad.PlannedPackageQuantity;
                    load.PackageUOM = viewModelLoad.PackageUOM;
                    load.Status = viewModelLoad.Status;

                    load.ContainerNumber = viewModelLoad.ContainerNumber;
                    load.SealNumber = viewModelLoad.SealNumber;
                    load.SealNumber2 = viewModelLoad.SealNumber2;
                    load.LoadingDate = viewModelLoad.LoadingDate;

                    // Assure that sequence will start from 1 by +1
                    if (load.Details != null && load.Details.Any())
                    {
                        foreach (var details in load.Details)
                        {
                            details.Sequence = load.Details.IndexOf(details) + 1;
                        }
                    }

                    // Update detail
                    foreach (var detail in load.Details)
                    {
                        var updatedLoadDetail = viewModelLoad.Details.FirstOrDefault(x => x.Id == detail.Id);

                        // also check if pofulfillment order removed
                        if (updatedLoadDetail != null && poFulfillment.Orders.Any(x => x.Id == updatedLoadDetail.POFulfillmentOrderId))
                        {
                            detail.POFulfillmentLoadId = updatedLoadDetail.POFulfillmentLoadId;
                            detail.CustomerPONumber = updatedLoadDetail.CustomerPONumber;
                            detail.ProductCode = updatedLoadDetail.ProductCode;
                            detail.PackageQuantity = updatedLoadDetail.PackageQuantity;
                            detail.PackageUOM = updatedLoadDetail.PackageUOM;
                            detail.Height = updatedLoadDetail.Height;
                            detail.Width = updatedLoadDetail.Width;
                            detail.Length = updatedLoadDetail.Length;
                            detail.DimensionUnit = updatedLoadDetail.DimensionUnit;
                            detail.UnitQuantity = updatedLoadDetail.UnitQuantity;
                            detail.Volume = updatedLoadDetail.Volume;
                            detail.GrossWeight = updatedLoadDetail.GrossWeight;
                            detail.NetWeight = updatedLoadDetail.NetWeight;
                            detail.POFulfillmentOrderId = updatedLoadDetail.POFulfillmentOrderId;
                            detail.PackageDescription = updatedLoadDetail.PackageDescription;
                            detail.ShippingMarks = updatedLoadDetail.ShippingMarks;
                            detail.Sequence = updatedLoadDetail.Sequence;
                        }
                        else
                        {
                            deletedLoadDetails.Add(detail);
                        }
                    }

                    foreach (POFulfillmentLoadDetailModel deletedLoadDetail in deletedLoadDetails)
                    {
                        load.Details.Remove(deletedLoadDetail);
                    }

                    var viewModelLoadDetails = viewModelLoad.Details.Where(x => x.Id <= 0);
                    foreach (var newViewModelLoadDetail in viewModelLoadDetails)
                    {
                        var newLoadDetail = new POFulfillmentLoadDetailModel
                        {
                            POFulfillmentLoadId = newViewModelLoadDetail.POFulfillmentLoadId,
                            CustomerPONumber = newViewModelLoadDetail.CustomerPONumber,
                            ProductCode = newViewModelLoadDetail.ProductCode,
                            PackageQuantity = newViewModelLoadDetail.PackageQuantity,
                            PackageUOM = newViewModelLoadDetail.PackageUOM,
                            Height = newViewModelLoadDetail.Height,
                            Width = newViewModelLoadDetail.Width,
                            Length = newViewModelLoadDetail.Length,
                            DimensionUnit = newViewModelLoadDetail.DimensionUnit,
                            UnitQuantity = newViewModelLoadDetail.UnitQuantity,
                            Volume = newViewModelLoadDetail.Volume,
                            GrossWeight = newViewModelLoadDetail.GrossWeight,
                            NetWeight = newViewModelLoadDetail.NetWeight,
                            POFulfillmentOrderId = newViewModelLoadDetail.POFulfillmentOrderId,
                            PackageDescription = newViewModelLoadDetail.PackageDescription,
                            ShippingMarks = newViewModelLoadDetail.ShippingMarks,
                            Sequence = newViewModelLoadDetail.Sequence
                        };
                        load.Details.Add(newLoadDetail);
                    }

                    // Calculate Subtotal and Total for load
                    load.SubtotalNetWeight = load.Details.Sum(x => x.NetWeight ?? 0);
                    load.SubtotalGrossWeight = load.Details.Sum(x => x.GrossWeight);
                    load.SubtotalVolume = load.Details.Sum(x => x.Volume);
                    load.SubtotalPackageQuantity = load.Details.Sum(x => x.PackageQuantity);
                    load.SubtotalUnitQuantity = load.Details.Sum(x => x.UnitQuantity);
                    load.TotalGrossWeight = load.SubtotalGrossWeight;
                    load.TotalNetWeight = load.SubtotalNetWeight;

                }

                // Assure that sequence will start from 1 by +1 incase some load details removed
                if (load.Details != null && load.Details.Any())
                {
                    var orderedLoadDetails = load.Details.OrderBy(x => x.Sequence);
                    foreach (var details in orderedLoadDetails)
                    {
                        details.Sequence = orderedLoadDetails.IndexOf(details) + 1;
                    }
                }
            }
            #endregion

            #region Calculate LoadedQty and OpenQty
            foreach (var updatedOrder in poFulfillment.Orders)
            {
                var loadedQty = poFulfillment.Loads.Where(x => x.Details != null && x.Details.Any()).SelectMany(x => x.Details).Where(x => x.POFulfillmentOrderId == updatedOrder.Id).Sum(x => x.PackageQuantity);
                updatedOrder.LoadedQty = loadedQty;
                updatedOrder.OpenQty = (updatedOrder.BookedPackage ?? 0) - loadedQty;
            }
            #endregion

            await UnitOfWork.SaveChangesAsync();

            #region Plan to ship

            // Trigger an event: 1062-FA-Plan to ship into Booking Activity.
            var event1062 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1062,
                POFulfillmentId = poFulfillment.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = currentUser.Username
            };
            await _activityService.TriggerAnEvent(event1062);

            var isEdisonShipment = poFulfillment.BookingRequests.Any(x => x.Status.Equals(POFulfillmentBookingRequestStatus.Active));

            // Shipment is edison
            if (isEdisonShipment)
            {
                //TODO: implement soon as business not ready yet.
            }

            // Shipment is outport
            else
            {
                await ExecuteOutportPlanToShipAsync(poFulfillment);
                await UnitOfWork.SaveChangesAsync();
            }

            #endregion

            UnitOfWork.CommitTransaction();
        }

        private async Task ExecuteOutportPlanToShipAsync(POFulfillmentModel model)
        {
            // Sync load details into shipment
            var shipment = model.Shipments.FirstOrDefault(x => x.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase));
            if (shipment != null)
            {
                var isExistingCargoDetails = shipment.CargoDetails.Any();

                foreach (var poffLoad in model.Loads)
                {
                    var shipmentLoad = await _shipmentLoadRepository
                        .GetAsync(sl => sl.ShipmentId == shipment.Id &&
                        sl.Container.LoadPlanRefNo == poffLoad.LoadReferenceNumber,
                        null,
                        x => x.Include(sl => sl.Container).Include(sl => sl.ShipmentLoadDetails).ThenInclude(sld => sld.CargoDetail));

                    if (isExistingCargoDetails)
                    {
                        shipmentLoad.ShipmentLoadDetails.Clear();
                    }

                    // Synchronize Container (if there is data inputted)
                    if (!string.IsNullOrEmpty(poffLoad.ContainerNumber))
                    {
                        shipmentLoad.Container.ContainerNo = poffLoad.ContainerNumber;
                    }
                    if (!string.IsNullOrEmpty(poffLoad.SealNumber))
                    {
                        shipmentLoad.Container.SealNo = poffLoad.SealNumber;
                    }
                    if (!string.IsNullOrEmpty(poffLoad.SealNumber2))
                    {
                        shipmentLoad.Container.SealNo2 = poffLoad.SealNumber2;
                    }
                    if (poffLoad.LoadingDate.HasValue)
                    {
                        shipmentLoad.Container.LoadingDate = poffLoad.LoadingDate;
                    }

                    // Re-sum up total of Net Weight, Gross Weight, Package Qty, Volume into container.
                    shipmentLoad.Container.TotalGrossWeight = poffLoad.SubtotalGrossWeight;
                    shipmentLoad.Container.TotalNetWeight = poffLoad.SubtotalNetWeight;
                    shipmentLoad.Container.TotalPackage = poffLoad.SubtotalPackageQuantity;
                    shipmentLoad.Container.TotalVolume = poffLoad.SubtotalVolume;

                    // CargoDetails and ShipmentLoadDetails should be same order as sequence of POFulfillmentLoadDetails
                    var loadDetails = poffLoad.Details.OrderBy(x => x.Sequence);

                    foreach (var loadDetail in loadDetails)
                    {
                        var order = model.Orders.First(x => x.Id == loadDetail.POFulfillmentOrderId);

                        ShipmentLoadDetailModel newShipmentLoadDetail = new ShipmentLoadDetailModel
                        {

                            Sequence = loadDetail.Sequence,
                            ShipmentId = shipmentLoad.ShipmentId,
                            ContainerId = shipmentLoad.ContainerId,
                            GrossWeight = loadDetail.GrossWeight,
                            GrossWeightUOM = AppConstant.KILOGGRAMS,
                            NetWeight = loadDetail.NetWeight,
                            NetWeightUOM = AppConstant.KILOGGRAMS,
                            Unit = loadDetail.UnitQuantity,
                            UnitUOM = order.UnitUOM.ToString(),
                            Package = loadDetail.PackageQuantity,
                            PackageUOM = loadDetail.PackageUOM.ToString(),
                            Volume = loadDetail.Volume,
                            VolumeUOM = AppConstant.CUBIC_METER,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = AppConstant.SYSTEM_USERNAME
                        };

                        var cargoDetail = shipment.CargoDetails?.FirstOrDefault(cd => cd.OrderId == order.PurchaseOrderId && cd.ItemId == order.POLineItemId);

                        if (cargoDetail != null)
                        {
                            newShipmentLoadDetail.CargoDetail = cargoDetail;
                        }
                        // Synchronize into Container Cargo Details
                        shipmentLoad.ShipmentLoadDetails.Add(newShipmentLoadDetail);
                    }
                }
            }
        }

        public async Task PlanToShipAsync(long id, string userName)
        {
            UnitOfWork.BeginTransaction();

            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
            => x.Include(m => m.Contacts)
                .Include(m => m.Loads)
                .ThenInclude(i => i.Details)
                .Include(m => m.Orders)
                .Include(m => m.CargoDetails)
                .Include(m => m.BookingRequests)
                .Include(m => m.BuyerApprovals)
                .Include(m => m.Itineraries)
                .Include(m => m.Shipments)
                .ThenInclude(s => s.CargoDetails);

            var poFulfillment = await Repository.GetAsync(x => x.Id == id, null, includeProperties);

            if (poFulfillment == null || poFulfillment.Stage != POFulfillmentStage.ForwarderBookingConfirmed)
            {
                throw new AppEntityNotFoundException($"Object with the id {id} not found!");
            }

            if (poFulfillment.MovementType == MovementType.CFS_CY)
            {
                throw new AppValidationException($"Cannot plan to ship Booking. MovementType must be CY!");
            }

            var customer = poFulfillment.Contacts.Where(x => x.OrganizationRole == OrganizationRole.Principal).First();
            var buyerCompliance = await _buyerComplianceService.GetByOrgIdAsync(customer.OrganizationId);

            // Add event and generate documents
            var event1062 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1062,
                POFulfillmentId = poFulfillment.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = userName
            };

            await _activityService.TriggerAnEvent(event1062);

            if (buyerCompliance.EnforcePackingListFormat)
            {
                await GeneratePackingListAttachment(poFulfillment, userName);
            }

            if (buyerCompliance.EnforceCommercialInvoiceFormat)
            {
                await GenerateShippingInvoiceAttachment(poFulfillment, userName);
            }

            var isEdisonShipment = poFulfillment.BookingRequests.Any(x => x.Status.Equals(POFulfillmentBookingRequestStatus.Active));

            #region Shipment is edison
            if (isEdisonShipment)
            {
                //TODO: implement soon as business not ready yet.
            }
            #endregion

            #region Shipment is outport
            else
            {
                await ExecuteOutportPlanToShipAsync(poFulfillment);
            }
            #endregion

            poFulfillment.IsGeneratePlanToShip = true;
            await UnitOfWork.SaveChangesAsync();
            UnitOfWork.CommitTransaction();
        }

        public async Task DispatchAsync(long id, string userName)
        {
            var poAdhocChanges = await GetPurchaseOrderAdhocChangesAsync(id);

            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
            => x.Include(m => m.Contacts)
            .Include(m => m.Loads)
            .Include(m => m.Orders)
            .Include(m => m.BookingRequests)
            .Include(m => m.Shipments)
            .ThenInclude(s => s.ShipmentLoads)
            .ThenInclude(s => s.Container);

            UnitOfWork.BeginTransaction();
            var poff = await Repository.GetAsync(x => x.Id == id, null, includeProperties);

            if (poff == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", id)} not found!");
            }

            if (poAdhocChanges != null && poAdhocChanges.Count() > 0)
            {
                var purchaseOrderAdhocChangesTopPriority = poAdhocChanges.OrderBy(x => x.Priority).First();

                if (purchaseOrderAdhocChangesTopPriority.Priority == PurchaseOrderAdhocChangePriority.Level1)
                {
                    throw new AppValidationException($"#POAdhocChanged#{purchaseOrderAdhocChangesTopPriority.Priority}#{purchaseOrderAdhocChangesTopPriority.Message}#{string.Join(",", poAdhocChanges.Where(x => x.Priority == purchaseOrderAdhocChangesTopPriority.Priority).Select(x => x.PurchaseOrderId))}");
                }
                else if (purchaseOrderAdhocChangesTopPriority.Priority == PurchaseOrderAdhocChangePriority.Level3)
                {
                    DeletePurchaseOrderAdhocChanges(0, poff.Id, 0);
                }
            }

            poff.Audit(userName);

            await ChangeStageToClosedAsync(new List<long> { id }, userName, DateTime.UtcNow);
            await UnitOfWork.SaveChangesAsync();
            UnitOfWork.CommitTransaction();
        }

        public async Task ChangeStageToShipmentDispatchAsync(List<long> poffIds, string userName)
        {
            var poffs = await _poFulfillmentRepository.Query(x => poffIds.Contains(x.Id),
                null,
                i => i.Include(m => m.Orders)).ToListAsync();

            foreach (var poff in poffs)
            {
                if (poff.Stage == POFulfillmentStage.ForwarderBookingConfirmed)
                {
                    poff.Stage = POFulfillmentStage.ShipmentDispatch;
                    poff.Audit(userName);
                }
            }
            // Update associated POs
            var purchaseOrderIdList = poffs.SelectMany(poff => poff.Orders.Select(x => x.PurchaseOrderId)).Distinct();
            var purchaseOrderList = await _purchaseOrderRepository.Query(po => purchaseOrderIdList.Any(poId => po.Id == poId), null, po => po.Include(x => x.BlanketPO)).ToListAsync();
            foreach (var po in purchaseOrderList)
            {
                if (po.Stage == POStageType.ForwarderBookingConfirmed)
                {
                    po.Stage = POStageType.ShipmentDispatch;

                    // If it's allocated purchase order, must update stage on related blanket PO
                    // If PO Blanket'stage greater than current stage then not update it
                    if (po.POType == POType.Allocated && po.BlanketPO != null && po.BlanketPO.Stage < POStageType.ShipmentDispatch)
                    {
                        po.BlanketPO.Stage = POStageType.ShipmentDispatch;
                    }
                }
            }
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task ChangeStageToClosedAsync(List<long> poffIds, string userName, DateTime eventDate, string location = null, string remark = null)
        {
            var poffs = await _poFulfillmentRepository.Query(x => poffIds.Contains(x.Id),
                null,
                i => i.Include(m => m.Orders)).ToListAsync();

            var closedPOFFIds = poffs.Where(poff => poff.Stage == POFulfillmentStage.Closed).Select(poff => poff.Id).ToList();

            var event1071List = new List<ActivityViewModel>();

            foreach (var poff in poffs)
            {
                if (poff.Stage != POFulfillmentStage.Closed)
                {
                    poff.Audit(userName);
                    poff.Stage = POFulfillmentStage.Closed;

                    // Trigger an event: 1071-FM-Booking Closed
                    var event1071 = new ActivityViewModel()
                    {
                        ActivityCode = Event.EVENT_1071,
                        POFulfillmentId = poff.Id,
                        ActivityDate = eventDate,
                        Location = location,
                        Remark = remark,
                        CreatedBy = userName
                    };
                    event1071List.Add(event1071);
                }
            }

            await _activityService.TriggerEventList(event1071List);

            // Update associated POs
            var purchaseOrderIdList = poffs.SelectMany(poff => poff.Orders.Select(x => x.PurchaseOrderId)).Distinct();
            var purchaseOrderList = await _purchaseOrderRepository.Query(po => purchaseOrderIdList.Any(poId => po.Id == poId),
                null,
                i => i.Include(po => po.LineItems).Include(po => po.BlanketPO))
                .ToListAsync();

            var event1010List = new List<ActivityViewModel>();

            foreach (var po in purchaseOrderList)
            {
                if (po.Stage == POStageType.Closed)
                {
                    continue;
                }

                if (po.LineItems.Any(item => item.BalanceUnitQty > 0))
                {
                    continue;
                }

                var hasOtherPOFFs = await Repository
                    .AnyAsync(f => !poffs.Select(poff => poff.Id).Contains(f.Id)
                        && f.Status == POFulfillmentStatus.Active
                        && f.Stage != POFulfillmentStage.Closed
                        && f.Stage != POFulfillmentStage.Draft
                        && f.Orders.Any(o => o.PurchaseOrderId == po.Id));

                if (hasOtherPOFFs)
                {
                    continue;
                }

                po.Stage = POStageType.Closed;

                // Trigger 1010-PM-PO Closed event into all associated POs
                var event1010 = new ActivityViewModel()
                {
                    ActivityCode = Event.EVENT_1010,
                    PurchaseOrderId = po.Id,
                    ActivityDate = eventDate,
                    Location = location,
                    Remark = remark,
                    CreatedBy = userName
                };

                event1010List.Add(event1010);

                // If it's allocated purchase order, must update stage on related blanket PO
                if (po.POType == POType.Allocated && po.BlanketPO != null)
                {
                    po.BlanketPO.Stage = POStageType.Closed;
                    // Trigger 1010-PM-PO Closed event into all related blanket POs
                    event1010List.Add(new ActivityViewModel()
                    {
                        ActivityCode = Event.EVENT_1010,
                        PurchaseOrderId = po.BlanketPO.Id,
                        ActivityDate = eventDate,
                        Location = location,
                        Remark = remark,
                        CreatedBy = userName
                    });
                }
            }

            await _activityService.TriggerEventList(event1010List);

            if (closedPOFFIds?.Count() > 0)
            {
                // Update stored activities (#1071-FM Booking Closed).
                await UpdateActivityAsync(closedPOFFIds, Event.EVENT_1071, eventDate, location, remark);
            }

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task RevertStageToFBConfirmedAsync(List<long> poffIds, string userName)
        {
            var poffs = await _poFulfillmentRepository.Query(x => poffIds.Contains(x.Id),
                null,
                i => i.Include(m => m.Orders))
                .ToListAsync();

            foreach (var poff in poffs)
            {
                poff.Audit(userName);

                if (poff.Stage == POFulfillmentStage.ShipmentDispatch)
                {
                    poff.Stage = POFulfillmentStage.ForwarderBookingConfirmed;
                }
            }
            // Update associated POs
            var purchaseOrderIdList = poffs.SelectMany(poff => poff.Orders.Select(x => x.PurchaseOrderId)).Distinct();
            var purchaseOrderList = await _purchaseOrderRepository.Query(po => purchaseOrderIdList.Any(poId => po.Id == poId), null, po => po.Include(x => x.BlanketPO)).ToListAsync();
            foreach (var po in purchaseOrderList)
            {
                if (po.Stage == POStageType.ShipmentDispatch)
                {
                    po.Stage = POStageType.ForwarderBookingConfirmed;

                    // If it's allocated purchase order, must update stage on related blanket PO
                    if (po.POType == POType.Allocated && po.BlanketPO != null)
                    {
                        po.BlanketPO.Stage = POStageType.ForwarderBookingConfirmed;
                    }
                }
            }

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task RevertStageToShipmentDispatchAsync(List<long> poffIds, string userName)
        {
            var poffs = await _poFulfillmentRepository.Query(x => poffIds.Contains(x.Id),
                null,
                i => i.Include(m => m.Orders))
                .ToListAsync();

            var poffGlobalIdList = new List<string>();

            foreach (var poff in poffs)
            {
                poff.Audit(userName);

                if (poff.Stage == POFulfillmentStage.Closed)
                {
                    poff.Stage = POFulfillmentStage.ShipmentDispatch;

                    var globalId = CommonHelper.GenerateGlobalId(poff.Id, EntityType.POFullfillment);
                    poffGlobalIdList.Add(globalId);
                }
            }

            // Update associated POs
            var purchaseOrderIdList = poffs.SelectMany(poff => poff.Orders.Select(x => x.PurchaseOrderId)).Distinct();
            var purchaseOrderList = await _purchaseOrderRepository.Query(po => purchaseOrderIdList.Any(poId => po.Id == poId), null, po => po.Include(x => x.BlanketPO)).ToListAsync();
            var purchaseOrderGlobalIdList = new List<string>();
            foreach (var po in purchaseOrderList)
            {
                if (po.Stage == POStageType.Closed)
                {
                    po.Stage = POStageType.ShipmentDispatch;

                    var globalId = CommonHelper.GenerateGlobalId(po.Id, EntityType.CustomerPO);
                    purchaseOrderGlobalIdList.Add(globalId);

                    // If it's allocated purchase order, must update stage on related blanket PO
                    if (po.POType == POType.Allocated && po.BlanketPO != null)
                    {
                        po.BlanketPO.Stage = POStageType.ShipmentDispatch;

                        globalId = CommonHelper.GenerateGlobalId(po.BlanketPO.Id, EntityType.CustomerPO);
                        purchaseOrderGlobalIdList.Add(globalId);
                    }
                }
            }

            // Remove event #1071; #1010
            var storedActivityList = await _activityRepository.Query(
                a => (a.GlobalIdActivities.Any(g => poffGlobalIdList.Contains(g.GlobalId)) && a.ActivityCode == Event.EVENT_1071) ||
                (a.GlobalIdActivities.Any(g => purchaseOrderGlobalIdList.Contains(g.GlobalId)) && a.ActivityCode == Event.EVENT_1010),
                null,
                i => i.Include(a => a.GlobalIdActivities))
                .ToListAsync();


            _activityRepository.RemoveRange(storedActivityList.ToArray());
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task UpdateActivityAsync(List<long> poffIds, string eventCode, DateTime activityDate, string location, string remark = null)
        {
            foreach (var poffId in poffIds.Distinct())
            {
                var globalId = CommonHelper.GenerateGlobalId(poffId, EntityType.POFullfillment);
                var activities = await _activityRepository.Query(a => a.GlobalIdActivities.Any(g => g.GlobalId == globalId) && a.ActivityCode == eventCode,
                    null,
                    i => i.Include(a => a.GlobalIdActivities)).ToListAsync();

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

                    activity.AddDomainEvent(new ActivityChangedDomainEvent(activity.ActivityCode,
                        activity.ActivityCode,
                        activity.ActivityDate,
                        currentBookingId: poffId,
                        currentLocation: activity.Location,
                        currentRemark: activity.Remark));
                }
            }

            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Detect if it is ediSON booking (to create booking request) or outport booking
        /// </summary>
        /// <param name="poff"></param>
        /// <returns>true: outport booking / false: ediSON booking</returns>
        public async Task<bool> IsCreateShipmentFromPOFF(POFulfillmentModel poff)
        {
            if (poff == null)
                return false;

            var buyerCompliance = await _buyerComplianceService.GetByPOFFAsync(poff);
            var originAgentId = poff.Contacts
                .SingleOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.OriginAgent, StringComparison.OrdinalIgnoreCase))
                ?.OrganizationId;
            var shipFromId = poff.ShipFrom;

            string toCompareModeOfTransport = string.Empty;
            switch (poff.ModeOfTransport)
            {
                case ModeOfTransportType.Sea:
                    toCompareModeOfTransport = ModeOfTransport.Sea;
                    break;
                case ModeOfTransportType.Air:
                    toCompareModeOfTransport = ModeOfTransport.Air;
                    break;
                default:
                    break;
            }

            var originAssignmentSettings = buyerCompliance.AgentAssignments
                .Where(a => a.AgentType == AgentType.Origin && a.ModeOfTransport == toCompareModeOfTransport)
                .OrderBy(a => a.Order);

            var customOriginAssignmentSettings = originAssignmentSettings.Skip(1);

            foreach (var setting in customOriginAssignmentSettings)
            {
                if (setting.AgentOrganizationId == originAgentId)
                {
                    if (!string.IsNullOrEmpty(setting.PortSelectionIds))
                    {
                        var splittedStr1 = setting.PortSelectionIds.Split(',');
                        var settingLocationIds = new List<long>();
                        foreach (var item in splittedStr1)
                        {
                            var splittedStr2 = item.Split('-');
                            if (splittedStr2.Count() == 2)
                            {
                                settingLocationIds.Add(long.Parse(splittedStr2[1]));
                            }
                        }

                        if (settingLocationIds.Any(x => x == shipFromId))
                        {
                            return setting.AutoCreateShipment == 1;
                        }
                    }
                    else
                    {
                        var shipFrom = await _csfeApiClient.GetLocationByIdAsync(shipFromId);
                        if (setting.CountryId == shipFrom?.CountryId)
                        {
                            return setting.AutoCreateShipment == 1;
                        }
                    }
                }
            }

            var defaultOriginAssignmentSetting = originAssignmentSettings.First();
            return defaultOriginAssignmentSetting?.AutoCreateShipment == 1;
        }

        public async Task ConfirmItinerariesFromShipmentAsync(long shipmentId, ShipmentConfirmItineraryViewModel model, string userName, string companyName)
        {
            UnitOfWork.BeginTransaction();

            var shipmentItineraries = await _itineraryService.GetItinerariesByShipmentAsync(shipmentId);
            var poff = await Repository.GetAsync(p => p.Shipments.Any(s => s.Id == shipmentId),
                null,
                x => x.Include(m => m.Contacts)
                   .Include(m => m.Loads)
                   .Include(m => m.Orders)
                   .Include(m => m.Itineraries)
                   .Include(m => m.BookingRequests)
                   .Include(m => m.Shipments));

            if (poff == null)
            {
                throw new AppEntityNotFoundException(nameof(poff));
            }

            var shipment = poff.Shipments.SingleOrDefault(s => s.Id == shipmentId);

            if (shipment.IsItineraryConfirmed)
            {
                throw new AppValidationException("Cannot confirm Itinerary.");
            }

            if (poff.FulfilledFromPOType == POType.Bulk)
            {
                foreach (var newItinerary in shipmentItineraries)
                {
                    var itinerary = new POFulfillmentItineraryModel
                    {
                        CarrierName = newItinerary.CarrierName,
                        LoadingPort = newItinerary.LoadingPort,
                        DischargePort = newItinerary.DischargePort,
                        ETADate = newItinerary.ETADate,
                        ETDDate = newItinerary.ETDDate,
                        ModeOfTransport = (ModeOfTransportType)Enum.Parse(typeof(ModeOfTransportType), newItinerary.ModeOfTransport, true),
                        VesselFlight = newItinerary.VesselFlight,
                        Status = POFulfillmentItinerayStatus.Active,
                        CreatedBy = newItinerary.CreatedBy,
                        CreatedDate = newItinerary.CreatedDate
                    };
                    poff.Itineraries.Add(itinerary);
                }

                poff.IsForwarderBookingItineraryReady = true;
                shipment.IsItineraryConfirmed = true;
                await UnitOfWork.SaveChangesAsync();

                var event1052 = new ActivityViewModel()
                {
                    ActivityCode = Event.EVENT_1052,
                    POFulfillmentId = poff.Id,
                    ActivityDate = DateTime.UtcNow,
                    CreatedBy = userName
                };
                await _activityService.TriggerAnEvent(event1052);
                await ConfirmPurchaseOrderFulfillmentAsync(poff.Id, shipment.ShipmentNo, userName);
            }
            else
            {
                var isAllShipmentsConfirmed = poff.Shipments.Any(x => !x.IsItineraryConfirmed && x.Id != shipmentId && x.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase));
                if (!isAllShipmentsConfirmed)
                {
                    var shipments = poff.Shipments.Where(s => s.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase));
                    foreach (var item in shipments)
                    {
                        var itineraries = await _itineraryService.GetItinerariesByShipmentAsync(item.Id);
                        foreach (var newItinerary in itineraries)
                        {
                            var itinerary = new POFulfillmentItineraryModel
                            {
                                CarrierName = newItinerary.CarrierName,
                                LoadingPort = newItinerary.LoadingPort,
                                DischargePort = newItinerary.DischargePort,
                                ETADate = newItinerary.ETADate,
                                ETDDate = newItinerary.ETDDate,
                                ModeOfTransport = (ModeOfTransportType)Enum.Parse(typeof(ModeOfTransportType), newItinerary.ModeOfTransport, true),
                                VesselFlight = newItinerary.VesselFlight,
                                Status = POFulfillmentItinerayStatus.Active,
                                CreatedBy = newItinerary.CreatedBy,
                                CreatedDate = newItinerary.CreatedDate
                            };
                            poff.Itineraries.Add(itinerary);
                        }
                    }

                    poff.IsForwarderBookingItineraryReady = true;
                    await UnitOfWork.SaveChangesAsync();

                    var event1052 = new ActivityViewModel()
                    {
                        ActivityCode = Event.EVENT_1052,
                        POFulfillmentId = poff.Id,
                        ActivityDate = DateTime.UtcNow,
                        CreatedBy = userName
                    };
                    await _activityService.TriggerAnEvent(event1052);
                    await ConfirmAllocatedPurchaseOrderShipmentAsync(shipmentId, userName);
                    await ConfirmPurchaseOrderFulfillmentAsync(poff.Id, shipment.ShipmentNo, userName);
                }
                else
                {
                    await ConfirmAllocatedPurchaseOrderShipmentAsync(shipmentId, userName);
                }
            }

            if (!model.SkipUpdates)
            {
                if (!shipment.IsFCL)
                {
                    poff.CFSClosingDate = model.CFSClosingDate;
                    poff.CFSWarehouseCode = model.CFSWarehouseCode;
                    poff.CFSWarehouseDescription = model.CFSWarehouseDescription;

                    var firstItinerary = shipmentItineraries.OrderBy(x => x.Sequence).FirstOrDefault();

                    if (firstItinerary.ScheduleId != null)
                    {
                        await UpdatePOFFCFSClosingDateAsync(model.CFSClosingDate, firstItinerary.ScheduleId.Value, userName, new List<long> { shipment.Id });
                    }
                }
                else
                {
                    poff.CYClosingDate = model.CYClosingDate;
                    poff.CYEmptyPickupTerminalCode = model.CYEmptyPickupTerminalCode;
                    poff.CYEmptyPickupTerminalDescription = model.CYEmptyPickupTerminalDescription;
                }
            }

            await UnitOfWork.SaveChangesAsync();
            await _itineraryService.ChangeStageOfBookingAndPOAsync(shipmentId);
            UnitOfWork.CommitTransaction();
        }

        public async Task UpdateConfirmItineraryFromShipmentAsync(long shipmentId, ShipmentConfirmItineraryViewModel model, string userName)
        {
            var shipment = await _shipmentRepository.GetAsync(x => x.Id == shipmentId, 
                includes: i => i.Include(x => x.POFulfillment).Include(x => x.ConsignmentItineraries).ThenInclude(x => x.Itinerary));

            if (shipment.POFulfillment == null)
            {
                throw new AppEntityNotFoundException(nameof(shipment.POFulfillment));
            }

            if (shipment.IsFCL)
            {
                shipment.POFulfillment.CYEmptyPickupTerminalCode = model.CYEmptyPickupTerminalCode;
                shipment.POFulfillment.CYEmptyPickupTerminalDescription = model.CYEmptyPickupTerminalDescription;
            }
            else
            {
                shipment.POFulfillment.CFSClosingDate = model.CFSClosingDate;
                shipment.POFulfillment.CFSWarehouseCode = model.CFSWarehouseCode;
                shipment.POFulfillment.CFSWarehouseDescription = model.CFSWarehouseDescription;

                var firstItinerary = shipment.ConsignmentItineraries.Select(x => x.Itinerary).OrderBy(x => x.Sequence).FirstOrDefault();
                if (firstItinerary != null && firstItinerary.ScheduleId != null)
                {
                    await UpdatePOFFCFSClosingDateAsync(model.CFSClosingDate, firstItinerary.ScheduleId.Value, userName, new List<long> { shipment.Id });
                }
            }

            shipment.POFulfillment.Audit(userName);

            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// To update CFSClosingDate of all POFulfillments whose 1st itinerary is related to the schedule.
        /// </summary>
        /// <param name="cfsClosingDate"></param>
        /// <param name="scheduleId"></param>
        /// <param name="userName"></param>
        /// <param name="excludeShipmentIds"></param>
        /// <returns></returns>
        private async Task UpdatePOFFCFSClosingDateAsync(DateTime? cfsClosingDate, long scheduleId, string userName, List<long> excludeShipmentIds = null)
        {
            if (cfsClosingDate == null)
            {
                return;
            }

            // query all shipments linked with itinerary.
            var shipmentQuery = _shipmentRepository.Query(s => s.ConsignmentItineraries.Any(a => a.Itinerary.ScheduleId == scheduleId) &&
                s.Status == StatusType.ACTIVE &&
                s.POFulfillmentId != null,
                includes: i => i.Include(x => x.POFulfillment).Include(x => x.ConsignmentItineraries).ThenInclude(x => x.Itinerary));

            if (excludeShipmentIds != null)
            {
                shipmentQuery = shipmentQuery.Where(s => !excludeShipmentIds.Contains(s.Id));
            }

            // to list ConsignmentItineraries
            var allConsignmentItineraries = await shipmentQuery.SelectMany(x => x.ConsignmentItineraries).ToListAsync();

            // to list first Itineraries by Shipment
            var groupedShipmentItineraries = allConsignmentItineraries.GroupBy(x => x.ShipmentId).Select(x => new
            {
                ShipmentId = x.Key,
                FirstItinerary = x.Select(x => x.Itinerary).OrderBy(x => x.Sequence).FirstOrDefault()
            });

            var shipmentIds = groupedShipmentItineraries.Where(x => x.FirstItinerary?.ScheduleId == scheduleId).Select(x => x.ShipmentId).ToList();

            var poffs = await shipmentQuery.Where(x => shipmentIds.Contains(x.Id)).Select(s => s.POFulfillment).ToListAsync();

            foreach (var poff in poffs)
            {
                poff.CFSClosingDate = cfsClosingDate;
                poff.Audit(userName);
            }
        }

        private async Task GenerateConsigneeNotifyPartyAsync(string username, bool isNotifyPartyAsConsignee, ICollection<POFulfillmentContactModel> poffContacts)
        {
            if (isNotifyPartyAsConsignee)
            {
                var newContacts = new List<POFulfillmentContactModel>();
                var consignee = poffContacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Consignee);
                if (consignee == null)
                {
                    var principal = poffContacts.First(x => x.OrganizationRole == OrganizationRole.Principal);
                    consignee = new POFulfillmentContactModel
                    {
                        POFulfillmentId = principal.POFulfillmentId,
                        OrganizationRole = OrganizationRole.Consignee,
                        OrganizationId = principal.OrganizationId,
                        CompanyName = principal.CompanyName,
                        Address = principal.Address,
                        ContactName = principal.ContactName,
                        ContactNumber = principal.ContactNumber,
                        ContactEmail = principal.ContactEmail,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = username
                    };
                    newContacts.Add(consignee);
                }
                var notifyParty = poffContacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.NotifyParty);
                if (notifyParty == null)
                {
                    notifyParty = new POFulfillmentContactModel
                    {
                        POFulfillmentId = consignee.POFulfillmentId,
                        OrganizationRole = OrganizationRole.NotifyParty,
                        OrganizationId = consignee.OrganizationId,
                        CompanyName = consignee.CompanyName,
                        Address = consignee.Address,
                        ContactName = consignee.ContactName,
                        ContactNumber = consignee.ContactNumber,
                        ContactEmail = consignee.ContactEmail,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = username
                    };
                    newContacts.Add(notifyParty);
                }
                if (newContacts.Any())
                {
                    await _poFulfillmentContactRepository.AddRangeAsync(newContacts.ToArray());
                    await UnitOfWork.SaveChangesAsync();
                }
            }
        }

        private async Task<IEnumerable<POLineItemArticleMasterViewModel>> GetInformationFromArticleMaster(long poffId, long customerOrgId, params string[] productCodes)
        {
            var customerOrg = await _csfeApiClient.GetOrganizationByIdAsync(customerOrgId);
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
                        Value = customerOrg.Code,
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

        public async Task<PurchaseOrderAdhocChangeViewModel> GetPurchaseOrderAdhocChangesTopPriorityAsync(long poffId)
        {
            var storedProcedureName = "";
            List<SqlParameter> filterParameter;
            storedProcedureName = "spu_GetPurchaseOrderAdhocChanges";
            filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@POFFId",
                        Value = poffId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@GetTopPriority",
                        Value = true,
                        DbType = DbType.Boolean,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, PurchaseOrderAdhocChangeViewModel> mapping = (reader) =>
            {
                // Default value
                var mappedData = PurchaseOrderAdhocChangeViewModel.NotChangeResult();
                while (reader.Read())
                {
                    // Must be in order of data reader
                    mappedData.Id = (long)reader[0];
                    mappedData.PurchaseOrderId = (long)reader[1];
                    mappedData.POFulfillmentId = (long)reader[2];
                    var priority = (int)reader[3];
                    mappedData.Message = (string)reader[4];

                    if (Enum.TryParse<PurchaseOrderAdhocChangePriority>(priority.ToString(), out var ePriority))
                    {
                        mappedData.Priority = ePriority;
                    }
                }
                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }

        public async Task<IEnumerable<PurchaseOrderAdhocChangeViewModel>> GetPurchaseOrderAdhocChangesAsync(long poffId)
        {
            var storedProcedureName = "";
            List<SqlParameter> filterParameter;
            storedProcedureName = "spu_GetPurchaseOrderAdhocChanges";
            filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@POFFId",
                        Value = poffId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@GetTopPriority",
                        Value = false,
                        DbType = DbType.Boolean,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, IEnumerable<PurchaseOrderAdhocChangeViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<PurchaseOrderAdhocChangeViewModel>();

                while (reader.Read())
                {
                    var newRow = new PurchaseOrderAdhocChangeViewModel();

                    // Must be in order of data reader
                    newRow.Id = (long)reader[0];
                    newRow.PurchaseOrderId = (long)reader[1];
                    newRow.POFulfillmentId = (long)reader[2];
                    var priority = (int)reader[3];
                    newRow.Message = (string)reader[4];

                    if (Enum.TryParse<PurchaseOrderAdhocChangePriority>(priority.ToString(), out var ePriority))
                    {
                        newRow.Priority = ePriority;
                    }

                    mappedData.Add(newRow);
                }
                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }

        #region Utilities for Single Booking/POFF
        public void DeletePurchaseOrderAdhocChanges(long id, long poffId, long purchaseOrderId)
        {

            // [dbo].[spu_DeletePurchaseOrderAdhocChanges]
            // @Id BIGINT = 0,
            // @POFFId BIGINT = 0,
            // @PurchaseOrderId BIGINT = 0
            var sql = @"spu_DeletePurchaseOrderAdhocChanges 
                        @p0,
	                    @p1,
	                   	@p2";
            var parameters = new object[]
            {
                id,
                poffId,
                purchaseOrderId
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());
        }

        public void AdjustQuantityOnPOLineItems(long poffId, AdjustBalanceOnPOLineItemsType type)
        {
            if (poffId == 0)
            {
                return;
            }
            var adjustType = type.Equals(AdjustBalanceOnPOLineItemsType.Deduct) ? -1 : 1;

            // Adjust quantities for current purchase order line items
            var sql = @"
                WITH POFulfillmentOrdersCTE AS
                (
	                SELECT POLineItemId, 
		                SUM(FulfillmentUnitQty) AS [BookedQty] 
	                FROM POFulfillmentOrders 
	                WHERE POFulfillmentId = @p0 AND [Status] = 1
                    GROUP BY POLineItemId
                )

                UPDATE POLineItems
                SET BalanceUnitQty = BalanceUnitQty + (@p1 * CTE.BookedQty), BookedUnitQty = BookedUnitQty - (@p1 * CTE.BookedQty)
                FROM POLineItems POL
                INNER JOIN POFulfillmentOrdersCTE CTE ON POL.Id = CTE.POLineItemId;
            ";
            var parameters = new object[]
            {
                poffId,
                adjustType
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters);

            // Then call adjust quantity of allocated purchase orders if any
            AdjustQuantityOnAllocatedPOLineItems(poffId, type);

            // Then call adjust quantity of blanket purchase orders if any
            AdjustQuantityOnBlanketPOLineItems(poffId, type);

        }

        public void DeductQuantityOnPOLineItems(long poffId)
        {
            if (poffId == 0)
            {
                return;
            }
            var adjustType = AdjustBalanceOnPOLineItemsType.Deduct;
            AdjustQuantityOnPOLineItems(poffId, adjustType);
        }

        public void ReleaseQuantityOnPOLineItems(long poffId)
        {
            if (poffId == 0)
            {
                return;
            }
            var adjustType = AdjustBalanceOnPOLineItemsType.Return;
            AdjustQuantityOnPOLineItems(poffId, adjustType);
        }

        public async Task UpdatePurchaseOrderStageByPOFFAsync(POFulfillmentModel poff)
        {
            var groupedPoOrders = poff.Orders.GroupBy(x => x.PurchaseOrderId).ToList();
            if (groupedPoOrders.Count > 0)
            {
                foreach (var groupedPoOrder in groupedPoOrders)
                {
                    var poId = groupedPoOrder.Key;
                    var orders = _poFulfillmentOrderRepository
                        .GetListQueryable()
                        .Where(o => o.PurchaseOrderId == poId && o.POFulfillmentId != poff.Id)
                        .Include(i => i.POFulfillment)
                        .ToList();

                    // Orders is empty that means the PO is not linked to any POFF, update Stage of PO to Released.
                    POStageType shouldUpdateTo = POStageType.Released;

                    if (orders.Count > 0)
                    {
                        var groupOrdersByPo = orders.GroupBy(
                            x => x.PurchaseOrderId,
                            x => x.POFulfillment,
                            (key, g) => new { PurchaseOrderId = key, POFulfillments = g.ToList() });

                        foreach (var groupOrder in groupOrdersByPo)
                        {
                            if (groupOrder.POFulfillments != null && groupOrder.POFulfillments.Any())
                            {
                                var stages = groupOrder.POFulfillments.Where(
                                    poFulfillment => poFulfillment.Id != poff.Id &&
                                    poFulfillment.Status == POFulfillmentStatus.Active
                                    ).Select(x => x.Stage);

                                // In case PO group without POFF, move PO to Released.
                                if (stages != null && stages.Any())
                                {
                                    var maxStage = stages.Max();
                                    switch (maxStage)
                                    {
                                        case POFulfillmentStage.Draft:
                                            shouldUpdateTo = POStageType.Released;
                                            break;
                                        case POFulfillmentStage.ForwarderBookingRequest:
                                            shouldUpdateTo = POStageType.ForwarderBookingRequest;
                                            break;
                                        case POFulfillmentStage.ForwarderBookingConfirmed:
                                            shouldUpdateTo = POStageType.ForwarderBookingConfirmed;
                                            break;
                                        case POFulfillmentStage.ShipmentDispatch:
                                            shouldUpdateTo = POStageType.ShipmentDispatch;
                                            break;
                                        case POFulfillmentStage.Closed:
                                            shouldUpdateTo = POStageType.Closed;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    var po = await _purchaseOrderRepository.GetAsync(x => x.Id == groupedPoOrder.Key, null, x => x.Include(i => i.LineItems));

                    // It is only changed to 'Closed' if: All product's balance qty = 0
                    if (shouldUpdateTo == POStageType.Closed)
                    {
                        if (po != null)
                        {
                            foreach (var item in po?.LineItems)
                            {
                                if (item.BalanceUnitQty > 0)
                                {
                                    // Keep current po stage
                                    shouldUpdateTo = po.Stage;
                                    break;
                                }
                            }
                        }
                    }
                    var isStageChanged = (po?.Stage ?? shouldUpdateTo) != shouldUpdateTo;
                    // update stage for blanked/allocated POs only stage of current PO changed
                    if (po != null && isStageChanged)
                    {
                        po.Stage = shouldUpdateTo;
                        _purchaseOrderRepository.Update(po);

                        switch (po.POType)
                        {
                            case POType.Bulk:
                                break;
                            case POType.Blanket:
                                // If it's blanket purchase order, must update stage for all allocated POs
                                var allocatedPOs = await _purchaseOrderRepository.Query(x => x.POType == POType.Allocated
                                                    && x.BlanketPOId != null
                                                    && x.BlanketPOId.Value == po.Id).ToListAsync();
                                if (allocatedPOs != null && allocatedPOs.Any())
                                {
                                    foreach (var allocatedPO in allocatedPOs)
                                    {
                                        allocatedPO.Stage = shouldUpdateTo;
                                        _purchaseOrderRepository.Update(allocatedPO);
                                    }
                                }
                                break;
                            case POType.Allocated:
                                // If it's allocated purchase order, must update stage on related blanket PO
                                var blanketPO = await _purchaseOrderRepository.GetAsync(x => x.POType == POType.Blanket
                                                    && x.BlanketPOId == null
                                                    && x.Id == po.BlanketPOId.Value);
                                if (blanketPO != null)
                                {
                                    blanketPO.Stage = shouldUpdateTo;
                                    _purchaseOrderRepository.Update(blanketPO);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        public async Task UpdatePurchaseOrderStageByPOFFAsync(long poffId)
        {
            var poff = await _poFulfillmentRepository.GetAsync(x => x.Id == poffId,
                null, i => i.Include(x => x.Orders));

            if (poff == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {poffId} not found!");
            }

            await UpdatePurchaseOrderStageByPOFFAsync(poff);
        }

        public async Task<string> ValidateBlanketPurchaseOrderFulfillment(long blanketPOFFId)
        {
            var storedProcedureName = "";
            List<SqlParameter> filterParameter;
            storedProcedureName = "spu_ValidateBlanketPurchaseOrderFulfillment";
            filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@blanketPOFFId",
                        Value = blanketPOFFId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, string> mapping = (reader) =>
            {
                // Default value
                var mappedData = "";
                while (reader.Read())
                {
                    var tmp = reader[0];
                    mappedData = tmp != DBNull.Value ? tmp.ToString() : string.Empty;
                }
                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }

        public async Task<string> ValidateAllocatedPurchaseOrderFulfillment(long allocatedPOFFId)
        {
            var storedProcedureName = "";
            List<SqlParameter> filterParameter;
            storedProcedureName = "spu_ValidateAllocatedPurchaseOrderFulfillment";
            filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@allocatedPOFFId",
                        Value = allocatedPOFFId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, string> mapping = (reader) =>
            {
                // Default value
                var mappedData = "";
                while (reader.Read())
                {
                    var tmp = reader[0];
                    mappedData = tmp != DBNull.Value ? tmp.ToString() : string.Empty;
                }
                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }

        #endregion

        #region Utilities for Blanket Booking/POFF
        /// <summary>
        /// Generate list of POFF allocated orders into memory
        /// </summary>
        /// <param name="blanketPOFFId">Id of blanket purchase order fulfillment</param>
        /// <param name="userName">Current user name</param>
        /// <returns></returns>
        private async Task<List<POFulfillmentAllocatedOrderViewModel>> GeneratePOFFAllocatedOrders(long blanketPOFFId, string userName)
        {
            // There are some calculations then execution on database gains advantages of performance
            var storedProcedureName = "";
            List<SqlParameter> filterParameter;
            storedProcedureName = "spu_GeneratePurchaseOrderFulfillmentAllocatedOrders";
            filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@blanketPOFFId",
                        Value = blanketPOFFId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, List<POFulfillmentAllocatedOrderViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<POFulfillmentAllocatedOrderViewModel>();
                while (reader.Read())
                {
                    var purchaseOrderId = reader[0];
                    var poLineItemId = reader[1];
                    var poFulfillmentId = reader[2];
                    var orderedQty = reader[3];
                    var bookedQty = reader[4];
                    var balanceQty = reader[5];
                    var loadedQty = reader[6];
                    var openQty = reader[7];
                    var bookedPackage = reader[8];
                    var volume = reader[9];
                    var grossWeight = reader[10];
                    var netWeight = reader[11];
                    var shipTo = reader[12];
                    var shipToId = reader[13];
                    var expectedShipDate = reader[14];
                    var expectedDeliveryDate = reader[15];
                    var cargoReadyDate = reader[16];
                    var customerPONumber = reader[17];
                    var productCode = reader[18];
                    var productName = reader[19];
                    var containerType = reader[20];
                    var productNumber = reader[21];


                    var newRow = new POFulfillmentAllocatedOrderViewModel
                    {
                        PurchaseOrderId = (long)purchaseOrderId,
                        POLineItemId = (long)poLineItemId,
                        POFulfillmentId = (long)poFulfillmentId,
                        OrderedQty = (int)orderedQty,
                        BookedQty = (int)bookedQty,
                        BalanceQty = (int)balanceQty,
                        LoadedQty = (int)loadedQty,
                        OpenQty = (int)openQty,
                        BookedPackage = bookedPackage != DBNull.Value ? (int)bookedPackage : (int?)null,
                        Volume = volume != DBNull.Value ? (decimal)volume : (decimal?)null,
                        GrossWeight = grossWeight != DBNull.Value ? (decimal)grossWeight : (decimal?)null,
                        NetWeight = netWeight != DBNull.Value ? (decimal)netWeight : (decimal?)null,
                        ShipTo = (string)shipTo,
                        ShipToId = (long)shipToId,
                        ExpectedShipDate = (DateTime)expectedShipDate,
                        ExpectedDeliveryDate = (DateTime)expectedDeliveryDate,
                        CargoReadyDate = (DateTime)cargoReadyDate,
                        CustomerPONumber = (string)customerPONumber,
                        ProductCode = (string)productCode,
                        ProductName = productName != DBNull.Value ? (string)productName : null,
                        ContainerType = (int)containerType,
                        ProductNumber = (string)productNumber,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = userName
                    };
                    mappedData.Add(newRow);

                }
                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }

        /// <summary>
        /// Validate to assure that all selected POs must be correct POType
        /// If selectedCustomerOrgId provided, check also if matched with current buyer compliance
        /// </summary>
        /// <param name="purchaseOrderIds"></param>
        /// <param name="mustBePOType"></param>
        /// <param name="selectedCustomerOrgId">Customer organization id</param>
        /// <returns></returns>
        private bool ValidatePOTypes(IEnumerable<long> purchaseOrderIds, POType mustBePOType, long? selectedCustomerOrgId = null)
        {
            if (!purchaseOrderIds.Any())
            {
                return true;
            }
            // Check if all selected POs have same PO type
            var sql = $@"
                        -- Default is valid
                        SET @result = 1;
                        
		                SELECT @result = 0
		                FROM PurchaseOrders PO WITH(NOLOCK)
		                WHERE PO.Id IN ({string.Join(',', purchaseOrderIds.Distinct())}) AND PO.POType != {(int)mustBePOType}
                    ";
            if (selectedCustomerOrgId.HasValue)
            {
                // Check on PO type allowed from buyer compliance
                sql += $@"
                        IF (@result = 1)
                        BEGIN
                            SELECT @result = 0
		                    FROM BuyerCompliances BC
		                    WHERE BC.OrganizationId = {selectedCustomerOrgId.Value}
                                AND BC.AllowToBookIn != {(int)mustBePOType} AND {(int)mustBePOType} != 10
                        END
                    ";
            }

            var isValid = _dataQuery.GetValueFromVariable(sql, null).Equals("1");
            return isValid;
        }

        /// <summary>
        /// Update quantities of all allocated purchase orders if any
        /// NOTES: It is safe to call if POFF is not blanket
        /// </summary>
        /// <param name="blanketPOFFId"></param>
        /// <param name="type"></param>
        private void AdjustQuantityOnAllocatedPOLineItems(long blanketPOFFId, AdjustBalanceOnPOLineItemsType type)
        {
            if (blanketPOFFId == 0)
            {
                return;
            }
            var adjustType = type.Equals(AdjustBalanceOnPOLineItemsType.Deduct) ? -1 : 1;

            // Fully adjusted by OrderedUnitQty
            var sql = $@"
                UPDATE POLineItems
                SET BalanceUnitQty = BalanceUnitQty + ({adjustType} * OrderedUnitQty), BookedUnitQty = BookedUnitQty - ({adjustType} * OrderedUnitQty)
                FROM POLineItems POL
                WHERE POL.PurchaseOrderId IN (
	                SELECT PO.Id 
	                FROM PurchaseOrders PO                    
	                INNER JOIN POFulfillmentOrders POFFO ON PO.BlanketPOId = POFFO.PurchaseOrderId
                    INNER JOIN POFulfillments POFF ON POFF.Id = POFFO.POFulfillmentId
	                WHERE POFF.Id = {blanketPOFFId} AND PO.[Status] = 1 AND POFFO.[Status] = 1 AND PO.POType = 30 AND POFF.FulfilledFromPOType = 20
                );
            ";
            var parameters = new object[]
            {
                blanketPOFFId,
                adjustType
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters);

        }

        /// <summary>
        /// Update quantities of all blanket purchase order if any
        /// NOTES: It is safe to call if POFF is not allocated
        /// </summary>
        /// <param name="allocatedPOFFId"></param>
        /// <param name="type"></param>
        private void AdjustQuantityOnBlanketPOLineItems(long allocatedPOFFId, AdjustBalanceOnPOLineItemsType type)
        {
            if (allocatedPOFFId == 0)
            {
                return;
            }
            var adjustType = type.Equals(AdjustBalanceOnPOLineItemsType.Deduct) ? -1 : 1;

            // Adjusted by BookedQuantity by allocated
            var sql = $@"
                    UPDATE POLineItems
                    SET BalanceUnitQty = BPOL.BalanceUnitQty + ({adjustType} * APOFFO.FulfillmentUnitQty),
	                    BookedUnitQty = BPOL.BookedUnitQty - ({adjustType} * APOFFO.FulfillmentUnitQty)
                    FROM POLineItems BPOL
                     -- Link by Product Code to allocated poff order
                    INNER JOIN 
					(
						SELECT APOFFO.ProductCode, SUM(APOFFO.FulfillmentUnitQty) AS [FulfillmentUnitQty]
						FROM POFulfillmentOrders APOFFO
						WHERE APOFFO.POFulfillmentId = {allocatedPOFFId}
						GROUP BY ProductCode
					) APOFFO ON APOFFO.ProductCode = BPOL.ProductCode
                    WHERE
                        -- Link to blanket po id
	                    BPOL.PurchaseOrderId IN 
	                    (
		                    SELECT BPO.Id
		                    FROM PurchaseOrders BPO
		                    INNER JOIN PurchaseOrders APO on BPO.Id = APO.BlanketPOId 
		                    INNER JOIN POFulfillmentOrders APOFFO on APOFFO.PurchaseOrderId = APO.Id
		                    WHERE BPO.[Status] = 1 AND BPO.POType = 20 
			                    AND APO.[Status] = 1 AND APO.POType = 30
			                    AND APOFFO.POFulfillmentId = {allocatedPOFFId}
                        );
            ";
            var parameters = new object[]
            {
                allocatedPOFFId,
                adjustType
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters);

        }

        #endregion

        #region Sending emails

        /// <summary>
        /// It will send email to inform OriginAgent, attaching all current POFF attachments (Not send Shipping Order Form)
        /// </summary>
        /// <param name="poff"></param>
        /// <returns></returns>
        private async Task SendNewShippingDocumentsNotificationEmailToOriginAgent(POFulfillmentModel poff)
        {
            if (poff == null)
            {
                throw new AppEntityNotFoundException($"Object is null or not found!");
            }

            var poffId = poff.Id;
            var originAgent = poff.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.OriginAgent);

            // Because of implicit load, if current model doesn't not contain Shipments, then try to get from database
            var shipment = poff.Shipments?.FirstOrDefault(x => x.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase));
            shipment = shipment ?? _shipmentRepository.QueryAsNoTracking(x => x.POFulfillmentId == poff.Id && x.Status.Equals(StatusType.ACTIVE)).FirstOrDefault();

            // Because of implicit load, if current model doesn't not contain Booking requests, then try to get from database
            var bookingRequest = poff.BookingRequests?.FirstOrDefault(x => x.Status == POFulfillmentBookingRequestStatus.Active);
            bookingRequest = bookingRequest ?? _poFulfillmentBookingRequestRepository.QueryAsNoTracking(x => x.POFulfillmentId == poff.Id && x.Status.Equals(POFulfillmentBookingRequestStatus.Active)).FirstOrDefault();
            var referenceNumber = bookingRequest?.BookingReferenceNumber ?? shipment?.ShipmentNo ?? string.Empty;

            // Get attachments for the email
            var attachments = await _attachmentService.GetNewShippingEmailAttachmentsAsync(poff.Id);

            _queuedBackgroundJobs.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync(
                $"Booking has been submitted with new shipping documents (Origin Agent) #{poffId}",
                "Booking_NewShippingDocumentsNotificationToOriginAgent",
                new POFulfillmentEmailViewModel
                {
                    Name = originAgent.ContactName,
                    BookingRefNumber = referenceNumber,
                    DetailPage = $"{_appConfig.ClientUrl}/po-fulfillments/view/{poffId}",
                    SupportEmail = _appConfig.SupportEmail
                },
                originAgent.ContactEmail,
                $"Shipment Portal: Booking has been submitted with new shipping documents ({referenceNumber} - {poff.ShipFromName})",
                attachments.ToArray()
                )
            );

            // Send push notification
            if (originAgent.OrganizationId != 0)
            {
                await _notificationService.PushNotificationSilentAsync(originAgent.OrganizationId, new NotificationViewModel
                {
                    MessageKey = $"~notification.msg.bookingNo~ <span class=\"k-link\">{poff.Number}</span> ~notification.msg.hasBeenSubmittedWithNewShippingDocument~.",
                    ReadUrl = $"/po-fulfillments/view/{poff.Id}"
                });
            }
        }

        #endregion

        #region Organization Preference

        /// <summary>
        /// To store preference to organization which current login belongs to.
        /// <br/> It works in silent mode that no exception/error thrown if failed.
        /// </summary>
        /// <param name="poffOrders"></param>
        /// <param name="userIdentityInfo"></param>
        /// <returns></returns>
        private async Task StoreOrganizationPreferenceSilentAsync(IEnumerable<POFulfillmentOrderViewModel> poffOrders, IdentityInfo userIdentityInfo)
        {
            try
            {
                var currentUserOrgId = userIdentityInfo.OrganizationId;
                // store only if external users (organization id > 0 valid)
                if (!userIdentityInfo.IsInternal && currentUserOrgId > 0 && poffOrders?.Count() > 0)
                {
                    var currentUserName = userIdentityInfo.Username;
                    var storingOrgPreference = Mapper.Map<IEnumerable<OrganizationPreferenceViewModel>>(poffOrders);

                    await _organizationPreferenceService.InsertOrUpdateRangeAsync(storingOrgPreference, currentUserOrgId, currentUserName);
                }
            }
            catch (Exception ex)
            {
            }

        }
        #endregion Organization Preference

        public async Task AskMissingPOAsync(long bookingId, InputPOFulfillmentViewModel viewModel)
        {
            if (viewModel != null)
            {
                var principalContact = viewModel.Contacts.SingleOrDefault(c => c.OrganizationRole == OrganizationRole.Principal);
                var userProfile = await _userProfileService.GetByUsernameAsync(viewModel.CreatedBy);
                var emailTemlateModel = new AskMissingPOEmailViewModel()
                {
                    Name = principalContact.ContactName,
                    BookingNumber = viewModel.Number,
                    BookingDetailPage = $"{_appConfig.ClientUrl}/missing-po-fulfillments/view/{viewModel.Id}",
                    ListOfPOPage = $"{_appConfig.ClientUrl}/purchase-orders",
                    EmailBookingOwner = viewModel.CreatedBy,
                    CompanyName = userProfile?.OrganizationName
                };

                foreach (var order in viewModel.Orders.Where(c => c.PurchaseOrderId == 0 && c.POLineItemId == 0))
                {
                    emailTemlateModel.POs.Add(new POEmailDetailViewModel
                    {
                        PONumber = order.CustomerPONumber
                    });
                }

                _queuedBackgroundJobs.Enqueue<SendMailBackgroundJobs>(c => c.SendMailAsync($"Booking#{viewModel.Number} is missing Purchase Orders",
                    "Booking_AskForMissingPO",
                    emailTemlateModel,
                    principalContact.ContactEmail,
                    $"Shipment Portal: Booking is missing your Purchase Orders ({viewModel.Number})")
                );
            }
        }
    }

    public enum AdjustBalanceOnPOLineItemsType
    {
        Deduct = -1,
        Return = 1
    }
}
