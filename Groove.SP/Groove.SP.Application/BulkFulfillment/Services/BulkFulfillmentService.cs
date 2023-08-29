using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.ApplicationBackgroundJob;
using Groove.SP.Application.ApplicationBackgroundJob.Services;
using Groove.SP.Application.Attachment.Services.Interfaces;
using Groove.SP.Application.BulkFulfillment.BackgroundJobs;
using Groove.SP.Application.BulkFulfillment.Services.Interfaces;
using Groove.SP.Application.BulkFulfillment.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Notification.Interfaces;
using Groove.SP.Application.Notification.ViewModel;
using Groove.SP.Application.OrganizationPreference.Services.Interfaces;
using Groove.SP.Application.OrganizationPreference.ViewModels;
using Groove.SP.Application.OrgContactPreference.Services.Interfaces;
using Groove.SP.Application.OrgContactPreference.ViewModels;
using Groove.SP.Application.POFulfillment;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.POFulfillmentContact.Mappers;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.BulkFulfillment.Services
{
    /// <summary>
    /// To handle NVO/Bulk Booking
    /// </summary>
    public class BulkFulfillmentService : ServiceBase<POFulfillmentModel, BulkFulfillmentViewModel>, IBulkFulfillmentService
    {
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IRepository<ShipmentModel> _shipmentRepository;
        private readonly IRepository<POFulfillmentModel> _poffRepository;
        private readonly IQueuedBackgroundJobs _queuedBackgroundJobs;
        private readonly IPOFulfillmentRepository _poFulfillmentRepository;
        private readonly IPOFulfillmentOrderRepository _poffOrderRepository;
        private readonly IPOFulfillmentLoadRepository _poffLoadRepository;
        private readonly IAttachmentService _attachmentService;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IContainerRepository _containerRepository;
        private readonly IActivityService _activityService;
        private readonly AppConfig _appConfig;
        private readonly IEdiSonBookingService _ediSonBookingService;
        private readonly IEdisonBulkFulfillmentService _edisonBulkFulfillmentService;
        private readonly IOrganizationPreferenceService _organizationPreferenceService;
        private readonly IOrgContactPreferenceService _orgContactPreferenceService;
        private readonly INotificationService _notificationService;
        private readonly IDataQuery _dataQuery;

        protected override Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> FullIncludeProperties => x
           => x.Include(m => m.Contacts)
               .Include(m => m.Loads)
               .ThenInclude(i => i.Details)
               .Include(m => m.Orders)
               .Include(m => m.CargoDetails)
               .Include(m => m.BookingRequests)
               .Include(m => m.BuyerApprovals)
               .Include(m => m.Itineraries);

        public BulkFulfillmentService(
            ICSFEApiClient csfeApiClient,
            IQueuedBackgroundJobs appBackgroundJob,
            IEdisonBulkFulfillmentService edisonBulkFulfillmentService,
            IOrganizationPreferenceService organizationPreferenceService,
            IOrgContactPreferenceService orgContactPreferenceService,
            IUnitOfWorkProvider unitOfWorkProvider,
            IRepository<POFulfillmentModel> poffRepository,
            IPOFulfillmentOrderRepository poffOrderRepository,
            IPOFulfillmentLoadRepository poffLoadRepository,
            IRepository<ShipmentModel> shipmentRepository,
            IAttachmentService attachmentService,
            IActivityService activityService,
            IOptions<AppConfig> appConfig,
            IEdiSonBookingService ediSonBookingService,
            INotificationService notificationService,
            IContainerRepository containerRepository,
            IDataQuery dataQuery
            ) : base(unitOfWorkProvider)
        {
            _edisonBulkFulfillmentService = edisonBulkFulfillmentService;
            _poFulfillmentRepository = (IPOFulfillmentRepository)Repository;
            _attachmentService = attachmentService;
            _organizationPreferenceService = organizationPreferenceService;
            _orgContactPreferenceService = orgContactPreferenceService;
            _attachmentRepository = (IAttachmentRepository)UnitOfWork.GetRepository<AttachmentModel>();
            _activityService = activityService;
            _poffRepository = poffRepository;
            _poffOrderRepository = poffOrderRepository;
            _poffLoadRepository = poffLoadRepository;
            _csfeApiClient = csfeApiClient;
            _shipmentRepository = shipmentRepository;
            _appConfig = appConfig.Value;
            _queuedBackgroundJobs = appBackgroundJob;
            _ediSonBookingService = ediSonBookingService;
            _containerRepository = containerRepository;
            _notificationService = notificationService;
            _dataQuery = dataQuery;
        }

        public async Task<DataSourceResult> SearchAsync(DataSourceRequest request, bool isInternal, string affiliates, long? organizationId = 0)
        {
            IQueryable<BulkFulfillmentQueryModel> query;
            string sql;
            if (isInternal)
            {
                sql = @"SELECT  POFF.[Id]
                                ,POFF.[Number]
                                ,CAST(POFF.[BookingDate] AS DATE) AS BookingDate
                                ,POFF.[CargoReadyDate]
                                ,POFF.[ShipFromName]
                                ,POFF.[ShipToName]

                                FROM [POFulfillments] POFF
                                WHERE POFF.Status = {2} AND POFF.FulfillmentType = {1}";
            }
            else
            {
                sql = @"SELECT  POFF.[Id]
                                ,POFF.[Number]
                                ,CAST(POFF.[BookingDate] AS DATE) AS BookingDate
                                ,POFF.[CargoReadyDate]
                                ,POFF.[ShipFromName]
                                ,POFF.[ShipToName]

                                FROM [POFulfillments] POFF
								WHERE POFF.Status = {2} AND POFF.FulfillmentType = {1} AND EXISTS (
										  SELECT 1 
										  FROM POFulfillmentContacts POFC
										  WHERE POFF.Id = POFC.POFulfillmentId AND POFC.OrganizationId = {0}
									  )";
            }

            query = _dataQuery.GetQueryable<BulkFulfillmentQueryModel>(sql, organizationId, FulfillmentType.Bulk, POFulfillmentStatus.Active);

            return await query.ToDataSourceResultAsync(request);
        }
        public async Task<BulkFulfillmentViewModel> SubmitBookingAsync(long id, IdentityInfo currentUser)
        {
            var bulkBooking = await Repository.GetAsync(x => x.Id == id,
                null, x =>
                      x.Include(m => m.Contacts)
                     .Include(m => m.Loads)
                     .Include(m => m.Orders)
                     .Include(m => m.BookingRequests));

            UnitOfWork.BeginTransaction();

            var bookingRequest = await _edisonBulkFulfillmentService.CreateBookingRequest(currentUser.Username, bulkBooking);

            // Edison doesn't support API for Air booking as the current.
            if (bulkBooking.ModeOfTransport != ModeOfTransportType.Air)
            {
                await _edisonBulkFulfillmentService.LoginToEdisonAsync();
                await _edisonBulkFulfillmentService.SendEBookingToEdisonAsync();
            }

            UpdateBulkBooking(bulkBooking, bookingRequest);
            await UnitOfWork.SaveChangesAsync();

            await TriggerEventWhenSubmitBooking(id, currentUser.Username);

            UnitOfWork.CommitTransaction();

            var result = Mapper.Map<BulkFulfillmentViewModel>(bulkBooking);
            ProceedShippingOrderFormBackground(ShippingFormType.Booking, bulkBooking.Id, currentUser.Username);
            return result;
        }

        public async Task ConfirmBookingAsync(EdiSonConfirmPOFFViewModel importVM)
        {
            var bulkBooking = await _poffRepository.GetAsync(
                x => x.BookingRequests.Any(br => br.BookingReferenceNumber == importVM.BookingReferenceNo
                && br.Status == POFulfillmentBookingRequestStatus.Active),
                null,
                i => i.Include(m => m.BookingRequests).Include(m => m.Itineraries));

            if (bulkBooking == null)
            {
                throw new AppEntityNotFoundException($"Booking with the BookingReferenceNo {importVM.BookingReferenceNo} not found!");
            }

            var bookingRequest = bulkBooking.BookingRequests.SingleOrDefault(x => x.Status == POFulfillmentBookingRequestStatus.Active);
            if (bookingRequest == null)
            {
                throw new AppValidationException($"Booking with the BookingReferenceNo {importVM.BookingReferenceNo} is inactive!");
            }

            if (bulkBooking.IsForwarderBookingItineraryReady || bulkBooking.Stage != POFulfillmentStage.ForwarderBookingRequest)
            {
                throw new AppValidationException("Booking is already confirmed!");
            }

            bulkBooking.IsForwarderBookingItineraryReady = true;
            bulkBooking.CYEmptyPickupTerminalCode = importVM.CYEmptyPickupTerminalCode;
            bulkBooking.CYEmptyPickupTerminalDescription = importVM.CYEmptyPickupTerminalDescription;
            bulkBooking.CFSWarehouseCode = importVM.CFSWarehouseCode;
            bulkBooking.CFSWarehouseDescription = importVM.CFSWarehouseDescription;
            bulkBooking.CYClosingDate = importVM.CYClosingDate;
            bulkBooking.CFSClosingDate = importVM.CFSClosingDate;

            bookingRequest.SONumber = importVM.SONumber;
            bookingRequest.BillOfLadingHeader = importVM.BillOfLadingHeader;
            bookingRequest.CYEmptyPickupTerminalCode = importVM.CYEmptyPickupTerminalCode;
            bookingRequest.CYEmptyPickupTerminalDescription = importVM.CYEmptyPickupTerminalDescription;
            bookingRequest.CFSWarehouseCode = importVM.CFSWarehouseCode;
            bookingRequest.CFSWarehouseDescription = importVM.CFSWarehouseDescription;
            bookingRequest.CYClosingDate = importVM.CYClosingDate;
            bookingRequest.CFSClosingDate = importVM.CFSClosingDate;

            var shipment = await _shipmentRepository.GetAsync(x => x.ShipmentNo == importVM.SONumber && x.Status == StatusType.ACTIVE);

            if (shipment != null)
            {
                shipment.POFulfillmentId = bulkBooking.Id;
            }

            foreach (var leg in importVM.Legs)
            {
                var loadingPort = await _csfeApiClient.GetLocationByCodeAsync(leg.LoadingPortCode);
                var dischargePort = await _csfeApiClient.GetLocationByCodeAsync(leg.DischargePortCode);
                var carrier = string.IsNullOrWhiteSpace(leg.CarrierCode) ? null :
                    await _csfeApiClient.GetCarrierByCodeAsync(leg.CarrierCode);

                var itinerary = new POFulfillmentItineraryModel
                {
                    Sequence = importVM.Legs.IndexOf(leg) + 1,
                    CreatedDate = DateTime.UtcNow,
                    ETADate = leg.ETA,
                    ETDDate = leg.ETD,
                    ModeOfTransport = leg.ModeOfTransport,
                    CarrierId = carrier?.Id,
                    CarrierName = carrier?.Name,
                    LoadingPortId = loadingPort?.Id ?? 0,
                    LoadingPort = loadingPort?.LocationDescription ?? leg.LoadingPortCode,
                    DischargePortId = dischargePort?.Id ?? 0,
                    DischargePort = dischargePort?.LocationDescription ?? leg.DischargePortCode,
                    VesselFlight = leg.VesselFlight,
                    Status = POFulfillmentItinerayStatus.Active
                };

                bulkBooking.Itineraries.Add(itinerary);
            }

            var event1052 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1052,
                POFulfillmentId = bulkBooking.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = AppConstant.SYSTEM_USERNAME
            };
            await _activityService.TriggerAnEvent(event1052);

            bulkBooking.Stage = POFulfillmentStage.ForwarderBookingConfirmed;

            var event1061 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1061,
                POFulfillmentId = bulkBooking.Id,
                ActivityDate = DateTime.UtcNow,
                Remark = bookingRequest.SONumber,
                CreatedBy = AppConstant.EDISON_USERNAME
            };
            await _activityService.TriggerAnEvent(event1061);

            ProceedShippingOrderFormBackground(ShippingFormType.ShippingOrder, bulkBooking.Id, AppConstant.EDISON_USERNAME);
            await this.UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// To amend bulk booking.
        /// </summary>
        /// <param name="poffId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task AmendAsync(long poffId, string userName)
        {

            UnitOfWork.BeginTransaction();

            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
            => x.Include(m => m.Contacts)
                .Include(m => m.Loads)
                .Include(m => m.BookingRequests)
                .Include(m => m.Itineraries)
                .Include(m => m.Shipments)
                .ThenInclude(s => s.ShipmentLoads).ThenInclude(sl => sl.ShipmentLoadDetails);

            var bookingModel = await Repository.GetAsync(x => x.Id == poffId, null, includeProperties);

            if (bookingModel == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {poffId} not found!");
            }

            if (bookingModel.Stage != POFulfillmentStage.ForwarderBookingRequest)
            {
                throw new AppValidationException("Cannot amend PO Fulfillment. Stage must be FB Request!");
            }

            bookingModel.Audit(userName);
            bookingModel.IsRejected = false;
            bookingModel.IsGeneratePlanToShip = false;
            bookingModel.Stage = POFulfillmentStage.Draft;
            bookingModel.BookingDate = null;

            // Cancel current Booking Request
            var bookingRequest = bookingModel.BookingRequests.SingleOrDefault(br => br.Status == POFulfillmentBookingRequestStatus.Active);
            if (bookingRequest != null)
            {
                bookingRequest.Status = POFulfillmentBookingRequestStatus.Inactive;
                await _ediSonBookingService.CancelBookingRequest(bookingRequest);
            }

            bookingModel.IsForwarderBookingItineraryReady = false;
            foreach (var itinerary in bookingModel.Itineraries)
            {
                itinerary.Status = POFulfillmentItinerayStatus.Inactive;
            }

            foreach (var load in bookingModel.Loads)
            {
                load.ContainerNumber = null;
                load.SealNumber = null;
                load.TotalGrossWeight = null;
                load.TotalNetWeight = null;
            }

            var event1056 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1056,
                POFulfillmentId = bookingModel.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = userName
            };
            event1056.Audit(userName);
            await _activityService.TriggerAnEvent(event1056);

            #region Deactivate shipments
            // Set Shipments' status to Inactive
            var shipments = bookingModel.Shipments?.Where(x => x.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase));
            var shipment = shipments?.FirstOrDefault();

            if (shipments != null)
            {
                foreach (var item in shipments)
                {
                    var consignmentNumbers = string.Empty;

                    if (item.Consignments != null && item.Consignments.Any())
                    {
                        foreach (var consigment in item.Consignments)
                        {
                            consigment.Status = StatusType.INACTIVE;
                            consignmentNumbers = consignmentNumbers + consigment.Id + ", ";
                        }
                    }

                    var containerIdList = item.ShipmentLoads.Select(sl => sl.ContainerId);
                    var containers = await _containerRepository.Query(c => containerIdList.Contains(c.Id),
                        null,
                        i => i.Include(c => c.ContainerItineraries)).ToListAsync();

                    var shipmentLoads = item.ShipmentLoads;

                    #region Data hard-delete on containers and shipment loads

                    if ((containers != null && containers.Any()) || (shipmentLoads != null && shipmentLoads.Any()))
                    {
                        // Remove related containers
                        foreach (var container in containers)
                        {
                            container.ContainerItineraries.Clear();
                            _containerRepository.Remove(container);
                        }

                        // Remove related shipment loads
                        foreach (var shipmentLoad in shipmentLoads)
                        {
                            shipmentLoad.ContainerId = null;
                            shipmentLoad.ShipmentLoadDetails.Clear();
                        }
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
            var equipmentTypes = bookingModel.Loads?.Select(x => EnumHelper<EquipmentType>.GetDisplayDescription(x.EquipmentType)).ToList();

            var originAgent = bookingModel.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.OriginAgent, StringComparison.OrdinalIgnoreCase));
            var shipper = bookingModel.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Shipper, StringComparison.OrdinalIgnoreCase));
            var consignee = bookingModel.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Consignee, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(referenceNumber))
            {
                var bookingEmailModel = new POFulfillmentEmailViewModel()
                {
                    Name = originAgent.ContactName,
                    BookingRefNumber = referenceNumber,
                    Shipper = shipper?.CompanyName,
                    Consignee = consignee?.CompanyName,
                    ShipFrom = bookingModel.ShipFromName,
                    ShipTo = bookingModel.ShipToName,
                    CargoReadyDate = bookingModel.CargoReadyDate,
                    EquipmentTypes = equipmentTypes,
                    DetailPage = $"{_appConfig.ClientUrl}/bulk-fulfillments/view/{bookingModel.Id}",
                    SupportEmail = _appConfig.SupportEmail
                };
                _queuedBackgroundJobs.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync($"Booking has been amended #{bookingModel.Id}", "Booking_Amended", bookingEmailModel,
                    originAgent.ContactEmail, $"Shipment Portal: Booking is amended ({referenceNumber} - {bookingModel.ShipFromName})"));

                // Send push notification
                await _notificationService.PushNotificationSilentAsync(originAgent.OrganizationId, new NotificationViewModel
                {
                    MessageKey = $"~notification.msg.bookingNo~ <span class=\"k-link\">{bookingModel.Number}</span> ~notification.msg.hasBeenAmended~.",
                    ReadUrl = $"/bulk-fulfillments/view/{bookingModel.Id}"
                });
            }
            await UnitOfWork.SaveChangesAsync();
            UnitOfWork.CommitTransaction();
        }

        private void UpdateBulkBooking(POFulfillmentModel bulkBooking, POFulfillmentBookingRequestModel bookingRequest)
        {
            bulkBooking.IsRejected = false;
            bulkBooking.Stage = POFulfillmentStage.ForwarderBookingRequest;
            bulkBooking.BookingDate = DateTime.UtcNow;
            bulkBooking.BookingRequests.Add(bookingRequest);
        }

        private async Task TriggerEventWhenSubmitBooking(long bulkBookingId, string userName)
        {
            var event1051 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1051,
                POFulfillmentId = bulkBookingId,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = userName
            };
            await _activityService.TriggerAnEvent(event1051);
        }

        public async Task<BulkFulfillmentViewModel> GetAsync(long id, bool isInternal, string affiliates)
        {
            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
            => x.Include(m => m.Contacts)
                .Include(m => m.Loads)
                .ThenInclude(i => i.Details)
                .Include(m => m.Orders)
                .Include(m => m.Shipments);

            var listOfAffiliates = new List<long>();
            POFulfillmentModel model = null;

            if (isInternal)
            {
                model = await Repository.GetAsync(p => p.Id == id && p.FulfillmentType == FulfillmentType.Bulk,
                    null,
                    includeProperties);
            }
            else
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }
                model = await Repository.GetAsync(p => p.Id == id && p.FulfillmentType == FulfillmentType.Bulk && p.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId)),
                    null,
                    includeProperties);
            }

            var viewModel = Mapper.Map<BulkFulfillmentViewModel>(model);
            if (model != null)
            {
                viewModel.Shipments = viewModel.Shipments?.Where(s => s.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase)).ToList();

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

        public async Task<BulkFulfillmentViewModel> GetPlannedScheduleAsync(long bulkBookingId)
        {
            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
           => x.Include(m => m.Shipments)
               .Include(m => m.Itineraries);

            var model = await Repository.GetAsNoTrackingAsync(c => c.Id == bulkBookingId, null, includeProperties);
            var result = Mapper.Map<BulkFulfillmentViewModel>(model);
            return result;
        }

        public async Task<BulkFulfillmentViewModel> CreateAsync(InputBulkFulfillmentViewModel viewModel, IdentityInfo currentUser)
        {
            UnitOfWork.BeginTransaction();

            var model = Mapper.Map<POFulfillmentModel>(viewModel);
            var isExistingConsigneeOrg = false;
            foreach (var contact in model.Contacts)
            {
                if (string.IsNullOrWhiteSpace(contact.CompanyName))
                {
                    contact.CompanyName = string.Empty;
                }
                if (!string.IsNullOrWhiteSpace(contact.Address))
                {
                    var concatenatedAddress = contact.Address;
                    contact.Address = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(concatenatedAddress, 1);
                    contact.AddressLine2 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(concatenatedAddress, 2);
                    contact.AddressLine3 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(concatenatedAddress, 3);
                    contact.AddressLine4 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(concatenatedAddress, 4);
                }

                if (contact.OrganizationRole == OrganizationRole.Consignee && !string.IsNullOrWhiteSpace(contact.CompanyName))
                {
                    var organization = await _csfeApiClient.GetOrganizationByNameAsync(contact.CompanyName);
                    if (organization != null)
                    {
                        contact.OrganizationId = organization.Id;
                        isExistingConsigneeOrg = true;
                    }
                }
            }
            model.Number = await GenerateBookingNumber();
            model.Orders = new List<POFulfillmentOrderModel>();
            model.Loads = new List<POFulfillmentLoadModel>();

            await Repository.AddAsync(model);
            await UnitOfWork.SaveChangesAsync();

            // Save relevant data
            const int ITEMS_PER_PATCH = 8;
            #region POFulfillmentLoads
            int patchIndex = 0;
            int patchTotal = (int)Math.Ceiling((double)(viewModel.Loads?.Count() / (double)ITEMS_PER_PATCH));
            while (patchIndex < patchTotal)
            {
                var loads = viewModel.Loads.Skip(patchIndex * ITEMS_PER_PATCH).Take(ITEMS_PER_PATCH);
                foreach (var load in loads)
                {
                    var newLoad = Mapper.Map<POFulfillmentLoadModel>(load);
                    newLoad.POFulfillmentId = model.Id;
                    newLoad.LoadReferenceNumber = await GenerateBookingLoadNumber(model.CreatedDate);
                    newLoad.Audit(currentUser.Name);
                    await _poffLoadRepository.AddAsync(newLoad);
                }
                await UnitOfWork.SaveChangesAsync();
                patchIndex++;
            }
            #endregion

            #region POFulfillmentOrders
            patchIndex = 0;
            patchTotal = (int)Math.Ceiling((double)(viewModel.Orders.Count() / (double)ITEMS_PER_PATCH));
            while (patchIndex < patchTotal)
            {
                var orders = viewModel.Orders.Skip(patchIndex * ITEMS_PER_PATCH).Take(ITEMS_PER_PATCH);
                foreach (var order in orders)
                {
                    var newOrder = Mapper.Map<POFulfillmentOrderModel>(order);
                    newOrder.POFulfillmentId = model.Id;
                    await _poffOrderRepository.AddAsync(newOrder);
                }
                await UnitOfWork.SaveChangesAsync();
                patchIndex++;
            }
            #endregion

            await ProceedAttachmentsAsync(viewModel.Attachments?.ToList(), model.Id, currentUser);

            UnitOfWork.CommitTransaction();

            //Notify to support team that booking has a new consignee organization
            if (isExistingConsigneeOrg == false)
            {
                _queuedBackgroundJobs.Enqueue<ConsigneeOrganizationJob>(j => j.ExecuteAsync(model.Id, model.Number));
            }

            #region Store HsCode / ChineseDescription
            // Only store organization preference if called by application GUI

            if (viewModel.UpdateOrganizationPreferences)
            {
                await StoreOrganizationPreferenceSilentAsync(viewModel.Orders, currentUser);
            }

            #endregion Store HsCode / ChineseDescription

            #region Store contacts
            // Only store organization contact preference if called by application GUI

            if (viewModel.UpdateOrgContactPreferences)
            {
                await StoreOrgContactPreferenceSilentAsync(viewModel.Contacts, currentUser);
            }

            #endregion

            var result = Mapper.Map<BulkFulfillmentViewModel>(model);
            return result;
        }

        public async Task<BulkFulfillmentViewModel> UpdateAsync(InputBulkFulfillmentViewModel model, IdentityInfo currentUser)
        {
            var userName = currentUser.Username;
            var bulkFulfillment = await Repository.GetAsync(x => x.Id == model.Id, null, FullIncludeProperties);

            if (bulkFulfillment == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", model.Id)} not found!");
            }

            UnitOfWork.BeginTransaction();

            #region POFulfilment General
            // Mapping POFulfillment
            bulkFulfillment.Owner = model.Owner;
            bulkFulfillment.Status = model.Status;
            bulkFulfillment.IsRejected = false;
            bulkFulfillment.Stage = model.Stage;
            bulkFulfillment.CargoReadyDate = model.CargoReadyDate;
            bulkFulfillment.Incoterm = model.Incoterm;
            bulkFulfillment.IsContainDangerousGoods = model.IsContainDangerousGoods;
            bulkFulfillment.ModeOfTransport = model.ModeOfTransport;
            bulkFulfillment.PreferredCarrier = model.PreferredCarrier;
            bulkFulfillment.LogisticsService = model.LogisticsService;
            bulkFulfillment.MovementType = model.MovementType;
            bulkFulfillment.ShipFrom = model.ShipFrom;
            bulkFulfillment.ShipTo = model.ShipTo;
            bulkFulfillment.ShipFromName = model.ShipFromName;
            bulkFulfillment.ShipToName = model.ShipToName;
            bulkFulfillment.ExpectedShipDate = model.ExpectedShipDate;
            bulkFulfillment.ExpectedDeliveryDate = model.ExpectedDeliveryDate;
            bulkFulfillment.Remarks = model.Remarks;
            bulkFulfillment.IsShipperPickup = model.IsShipperPickup;
            bulkFulfillment.IsNotifyPartyAsConsignee = model.IsNotifyPartyAsConsignee;
            bulkFulfillment.DeliveryPortId = model.DeliveryPortId;
            bulkFulfillment.ReceiptPortId = model.ReceiptPortId;
            bulkFulfillment.DeliveryPort = model.DeliveryPort;
            bulkFulfillment.ReceiptPort = model.ReceiptPort;
            bulkFulfillment.IsBatteryOrChemical = model.IsBatteryOrChemical;
            bulkFulfillment.IsCIQOrFumigation = model.IsCIQOrFumigation;
            bulkFulfillment.IsExportLicence = model.IsExportLicence;
            bulkFulfillment.VesselName = model.VesselName;
            bulkFulfillment.VoyageNo = model.VoyageNo;
            bulkFulfillment.IsAllowMixedCarton = model.IsAllowMixedCarton;
            //Added Ayush
            bulkFulfillment.PoRemark = model.PoRemark;
            #endregion

            #region POFulfillment Contacts
            // Mapping POFulfillment Contacts
            var deletedContacts = new ArrayList();
            foreach (var contact in bulkFulfillment.Contacts)
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
                    contact.Address = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(c.Address, 1);
                    contact.AddressLine2 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(c.Address, 2);
                    contact.AddressLine3 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(c.Address, 3);
                    contact.AddressLine4 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(c.Address, 4);
                    contact.WeChatOrWhatsApp = c.WeChatOrWhatsApp;
                }
                else
                {
                    deletedContacts.Add(contact);
                }
            }

            foreach (POFulfillmentContactModel deletedContact in deletedContacts)
            {
                bulkFulfillment.Contacts.Remove(deletedContact);
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
                    ContactEmail = newContact.ContactEmail,
                    CreatedDate = newContact.CreatedDate,
                    CreatedBy = newContact.CreatedBy,
                    UpdatedDate = newContact.UpdatedDate,
                    UpdatedBy = newContact.UpdatedBy,
                    Address = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(newContact.Address, 1),
                    AddressLine2 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(newContact.Address, 2),
                    AddressLine3 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(newContact.Address, 3),
                    AddressLine4 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(newContact.Address, 4),
                    WeChatOrWhatsApp = newContact.WeChatOrWhatsApp
            };
                bulkFulfillment.Contacts.Add(con);
            }

            var consignee = bulkFulfillment.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Consignee);
            if (consignee != null && !string.IsNullOrWhiteSpace(consignee.CompanyName))
            {
                var organization = await _csfeApiClient.GetOrganizationByNameAsync(consignee.CompanyName);
                if (organization != null)
                {
                    consignee.OrganizationId = organization.Id;
                }
                else
                {
                    consignee.OrganizationId = 0;
                    _queuedBackgroundJobs.Enqueue<ConsigneeOrganizationJob>(j => j.ExecuteAsync(bulkFulfillment.Id, bulkFulfillment.Number));
                }
            }
            #endregion

            #region POFulfillment Customer PO
            // Mapping POFulfillmentOrder
            var deletedOrders = new ArrayList();

            foreach (var updatedOrder in bulkFulfillment.Orders)
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
            const int ITEMS_PER_PATCH = 8;
            int patchTotal = 0;
            int patchIndex = 0;
            patchTotal = (int)Math.Ceiling((double)(newViewModelOrders?.Count() / (double)ITEMS_PER_PATCH));
            while (patchIndex < patchTotal)
            {
                var newOrders = newViewModelOrders.Skip(patchIndex * ITEMS_PER_PATCH).Take(ITEMS_PER_PATCH);
                foreach (var newOrder in newOrders)
                {
                    var newOrderModel = new POFulfillmentOrderModel
                    {
                        PurchaseOrderId = newOrder.PurchaseOrderId,
                        POLineItemId = newOrder.POLineItemId,
                        POFulfillmentId = bulkFulfillment.Id,
                        CustomerPONumber = newOrder.CustomerPONumber,
                        ProductCode = newOrder.ProductCode,
                        ProductName = newOrder.ProductName,
                        OrderedUnitQty = newOrder.OrderedUnitQty,
                        FulfillmentUnitQty = newOrder.FulfillmentUnitQty,
                        BalanceUnitQty = newOrder.BalanceUnitQty,
                        UnitUOM = newOrder.UnitUOM,
                        PackageUOM = newOrder.PackageUOM,
                        Commodity = newOrder.Commodity,
                        Status = newOrder.Status,
                        HsCode = newOrder.HsCode,
                        CountryCodeOfOrigin = newOrder.CountryCodeOfOrigin,
                        LoadedQty = 0, // For the new Order, LoadedQty should be equal to 0
                        OpenQty = newOrder.BookedPackage ?? 0, // For the new Order, OpenQty should be equal to Booked Package
                        BookedPackage = newOrder.BookedPackage,
                        Volume = newOrder.Volume,
                        GrossWeight = newOrder.GrossWeight,
                        NetWeight = newOrder.NetWeight,
                        ChineseDescription = newOrder.ChineseDescription,
                        ShippingMarks = newOrder.ShippingMarks
                    };
                    await _poffOrderRepository.AddAsync(newOrderModel);
                }
                await UnitOfWork.SaveChangesAsync();
                patchIndex++;
            }

            foreach (POFulfillmentOrderModel deletedOrder in deletedOrders)
            {
                bulkFulfillment.Orders.Remove(deletedOrder);
            }

            #endregion

            #region POFulfillment Loads and LoadDetails
            if (bulkFulfillment.Stage <= POFulfillmentStage.Draft)
            {
                var deletedLoads = new List<POFulfillmentLoadModel>();
                var newLoads = new List<POFulfillmentLoadModel>();
                var viewModelLoads = model.Loads.GroupBy(load => load.EquipmentType).Select(s => new { s.Key, Count = s.Count() });
                foreach (var viewModelLoad in viewModelLoads)
                {
                    var storedLoads = bulkFulfillment.Loads.Where(x => x.EquipmentType == viewModelLoad.Key).OrderByDescending(x => x.Id);
                    if (storedLoads.Count() < viewModelLoad.Count)
                    {
                        var n = viewModelLoad.Count - storedLoads.Count();
                        for (int i = 0; i < n; i++)
                        {
                            newLoads.Add(new POFulfillmentLoadModel
                            {
                                EquipmentType = viewModelLoad.Key,
                                LoadReferenceNumber = await GenerateBookingLoadNumber(model.CreatedDate),
                                POFulfillmentId = bulkFulfillment.Id,
                                Status = POFulfillmentLoadStatus.Active
                            });
                        }
                    }
                    else if (storedLoads.Count() > viewModelLoad.Count)
                    {
                        var n = storedLoads.Count() - viewModelLoad.Count;
                        var i = 0;
                        while (n > i)
                        {
                            deletedLoads.Add(storedLoads.ElementAt(i));
                            i++;
                        }
                    }
                }

                patchIndex = 0;
                patchTotal = (int)Math.Ceiling((double)(newLoads?.Count() / (double)ITEMS_PER_PATCH));
                while (patchIndex < patchTotal)
                {
                    var loads = newLoads.Skip(patchIndex * ITEMS_PER_PATCH).Take(ITEMS_PER_PATCH);
                    foreach (var load in loads)
                    {
                        await _poffLoadRepository.AddAsync(load);
                    }
                    await UnitOfWork.SaveChangesAsync();
                    patchIndex++;
                }

                var ortherLoads = bulkFulfillment.Loads.Where(load => !viewModelLoads.Select(x => x.Key).Contains(load.EquipmentType));
                deletedLoads.AddRange(ortherLoads.ToArray());
                foreach (POFulfillmentLoadModel deletedLoad in deletedLoads)
                {
                    bulkFulfillment.Loads.Remove(deletedLoad);
                }
            }
            else
            {
                UpdateLoadDetail(model.Loads, bulkFulfillment);
            }

            #endregion

            #region Calculate LoadedQty and OpenQty

            foreach (var updatedOrder in bulkFulfillment.Orders)
            {
                var loadedQty = bulkFulfillment.Loads.Where(x => x.Details != null && x.Details.Any()).SelectMany(x => x.Details).Where(x => x.POFulfillmentOrderId == updatedOrder.Id).Sum(x => x.PackageQuantity);
                updatedOrder.LoadedQty = loadedQty;
                updatedOrder.OpenQty = (updatedOrder.BookedPackage ?? 0) - loadedQty;
            }

            #endregion

            #region POFulfillment Attachments
            // attachments

            // Need to send email to Origin Agent as document is either created or updated
            var isNewShippingDocuments = await ProceedAttachmentsAsync(model.Attachments?.ToList(), bulkFulfillment.Id, currentUser);
            #endregion           

            Repository.Update(bulkFulfillment);
            await this.UnitOfWork.SaveChangesAsync();

            UnitOfWork.CommitTransaction();

            #region Store HsCode / ChineseDescription
            // Only store organization preference if called by application GUI

            if (model.UpdateOrganizationPreferences)
            {
                await StoreOrganizationPreferenceSilentAsync(model.Orders, currentUser);
            }

            #endregion Store HsCode / ChineseDescription

            #region Store contacts
            // Only store organization contact preference if called by application GUI

            if (model.UpdateOrgContactPreferences)
            {
                await StoreOrgContactPreferenceSilentAsync(model.Contacts, currentUser);
            }

            #endregion

            var result = Mapper.Map<BulkFulfillmentViewModel>(bulkFulfillment);

            return result;
        }

        public async Task<BulkFulfillmentViewModel> CancelAsync(long id, string userName, CancelBulkFulfillmentViewModel cancelModel)
        {
            UnitOfWork.BeginTransaction();
            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
             => x.Include(m => m.Contacts)
                 .Include(m => m.Loads)
                 .Include(m => m.Orders)
                 .Include(m => m.BookingRequests)
                 .Include(m => m.Shipments)
                 .ThenInclude(i => i.Consignments);

            var poff = await Repository.GetAsync(x => x.Id == id, null, includeProperties);

            if (poff != null && poff.Stage <= POFulfillmentStage.ForwarderBookingRequest)
            {
                poff.Status = POFulfillmentStatus.Inactive;
                poff.IsRejected = false;
                poff.BookingDate = null;


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

                // Get reference number for the booking
                var referenceNumber = shipment != null ? shipment.ShipmentNo
                    : (bookingRequest != null ? bookingRequest.BookingReferenceNumber : string.Empty);

                var originAgent = poff.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.OriginAgent, StringComparison.OrdinalIgnoreCase));
                var shipper = poff.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Shipper, StringComparison.OrdinalIgnoreCase));
                var consignee = poff.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Consignee, StringComparison.OrdinalIgnoreCase));

                //collect all equipment types of the booking
                var equipmentTypes = poff.Loads?.Select(x => EnumHelper<EquipmentType>.GetDisplayDescription(x.EquipmentType)).ToList();

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
                            DetailPage = $"{_appConfig.ClientUrl}/bulk-fulfillments/view/{poff.Id}",
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
                                ReadUrl = $"/bulk-fulfillments/view/{poff.Id}"
                            });
                        }
                    }
                }

                await UnitOfWork.SaveChangesAsync();
                UnitOfWork.CommitTransaction();

                return Mapper.Map<BulkFulfillmentViewModel>(poff);
            }

            return null;
        }

        public async Task PlanToShipAsync(long id, string userName)
        {
            UnitOfWork.BeginTransaction();

            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
            => x.Include(m => m.Contacts)
                .Include(m => m.Loads)
                .ThenInclude(i => i.Details)
                .Include(m => m.Orders)
                .Include(m => m.BookingRequests)
                .Include(m => m.CargoDetails)
                .Include(m => m.Itineraries);

            var bulkFulfillment = await Repository.GetAsync(x => x.Id == id, null, includeProperties);

            if (bulkFulfillment == null || bulkFulfillment.Stage != POFulfillmentStage.ForwarderBookingConfirmed)
            {
                throw new AppEntityNotFoundException($"Object with the id {id} not found!");
            }

            if (bulkFulfillment.MovementType == MovementType.CFS_CY)
            {
                throw new AppValidationException($"Cannot plan to ship Booking. MovementType must be CY!");
            }

            // Add event
            var event1062 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1062,
                POFulfillmentId = bulkFulfillment.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = userName
            };

            await _activityService.TriggerAnEvent(event1062);

            // Call eSI API to send Load Details to ediSON -> waiting for eSI API launching
            //var bookingRequest = bulkFulfillment.BookingRequests.SingleOrDefault(x => x.Status.Equals(POFulfillmentBookingRequestStatus.Active));
            //if (bookingRequest != null)
            //{
            //    await _edisonBulkFulfillmentService.ProcesseSIAsync(bulkFulfillment, bookingRequest);
            //}
            //else
            //{
            //    throw new AppEntityNotFoundException($"Booking request not found!");
            //}

            bulkFulfillment.IsGeneratePlanToShip = true;
            await UnitOfWork.SaveChangesAsync();
            UnitOfWork.CommitTransaction();
        }

        public async Task ReloadAsync(long id, InputBulkFulfillmentViewModel model, IdentityInfo currentUser)
        {
            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
            => x.Include(m => m.Loads)
                .ThenInclude(i => i.Details)
                .Include(m => m.Orders);

            var bulkFulfillment = await Repository.GetAsync(x => x.Id == id, null, includeProperties);

            var allowedStages = new POFulfillmentStage[] { POFulfillmentStage.ForwarderBookingConfirmed, POFulfillmentStage.ShipmentDispatch };

            if (bulkFulfillment == null || !allowedStages.Contains(bulkFulfillment.Stage))
            {
                throw new AppEntityNotFoundException($"Object with the id {id} not found!");
            }

            if (bulkFulfillment.MovementType == MovementType.CFS_CY)
            {
                throw new AppValidationException($"Cannot plan to ship Booking. MovementType must be CY!");
            }

            UnitOfWork.BeginTransaction();

            UpdateLoadDetail(model.Loads, bulkFulfillment);

            #region Calculate LoadedQty and OpenQty
            foreach (var updatedOrder in bulkFulfillment.Orders)
            {
                var loadedQty = bulkFulfillment.Loads.Where(x => x.Details != null && x.Details.Any()).SelectMany(x => x.Details).Where(x => x.POFulfillmentOrderId == updatedOrder.Id).Sum(x => x.PackageQuantity);
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
                POFulfillmentId = bulkFulfillment.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = currentUser.Username
            };
            await _activityService.TriggerAnEvent(event1062);

            // TODO: Call eSI API to send Load Details to ediSON -> waiting for eSI API launching
            //var bookingRequest = bulkFulfillment.BookingRequests.SingleOrDefault(x => x.Status.Equals(POFulfillmentBookingRequestStatus.Active));
            //if (bookingRequest != null)
            //{
            //    await _edisonBulkFulfillmentService.ProcesseSIAsync(bulkFulfillment, bookingRequest);
            //}
            //else
            //{
            //    throw new AppEntityNotFoundException($"Booking request not found!");
            //}

            #endregion

            UnitOfWork.CommitTransaction();
        }

        private void UpdateLoadDetail(ICollection<BulkFulfillmentLoadViewModel> loadViewModels, POFulfillmentModel bulkFulfillmentModel)
        {
            var deletedLoadDetails = new ArrayList();
            foreach (var load in bulkFulfillmentModel.Loads)
            {
                var viewModelLoad = loadViewModels.FirstOrDefault(x => x.Id == load.Id);
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
                    load.GateInDate = viewModelLoad.GateInDate;

                    // Update detail
                    foreach (var detail in load.Details)
                    {
                        var updatedLoadDetail = viewModelLoad.Details.FirstOrDefault(x => x.Id == detail.Id);

                        // also check if pofulfillment order removed
                        if (updatedLoadDetail != null && bulkFulfillmentModel.Orders.Any(x => x.Id == updatedLoadDetail.POFulfillmentOrderId))
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
            }
        }

        private async Task<string> GenerateBookingLoadNumber(DateTime createdDate)
        {
            var nextSequenceValue = await _poFulfillmentRepository.GetNextPOFFLoadSequenceValueAsync();
            return $"LD{createdDate.ToString("yyMM")}{nextSequenceValue}";
        }

        private async Task<string> GenerateBookingNumber()
        {
            var currentNumber = $"{AppConstant.PrefixNVOBooking}{DateTime.UtcNow.ToString("yy")}{DateTime.UtcNow.ToString("MM")}{DateTime.UtcNow.ToString("dd")}";
            var latestBooking =
                    await Repository
                        .QueryAsNoTracking(c => c.Number.StartsWith(currentNumber) && c.FulfillmentType == FulfillmentType.Bulk, a => a.OrderByDescending(s => s.Number))
                        .FirstOrDefaultAsync();
            var lastestNumber = Convert.ToInt64(latestBooking?.Number.Split(currentNumber)[1]);
            if (lastestNumber == 0)
            {
                return $"{currentNumber}{"0000"}{1}";
            }
            else
            {
                var value = "";
                switch ($"{lastestNumber + 1}".Length)
                {
                    case 1:
                        value = $"{currentNumber}{"0000"}{lastestNumber + 1}";
                        break;
                    case 2:
                        value = $"{currentNumber}{"000"}{lastestNumber + 1}";
                        break;
                    case 3:
                        value = $"{currentNumber}{"00"}{lastestNumber + 1}";
                        break;
                    case 4:
                        value = $"{currentNumber}{"0"}{lastestNumber + 1}";
                        break;
                    default:
                        value = $"{currentNumber}{lastestNumber + 1}";
                        break;
                }
                return value;
            }
        }

        /// <summary>
        /// To store preference to organization which current login belongs to.
        /// <br/> It works in silent mode that no exception/error thrown if failed.
        /// </summary>
        /// <param name="bulkFulfillmentOrders"></param>
        /// <param name="userIdentityInfo"></param>
        /// <returns></returns>
        private async Task StoreOrganizationPreferenceSilentAsync(IEnumerable<BulkFulfillmentOrderViewModel> bulkFulfillmentOrders, IdentityInfo userIdentityInfo)
        {
            try
            {
                var currentUserOrgId = userIdentityInfo.OrganizationId;
                // store only if external users (organization id > 0 valid)
                if (!userIdentityInfo.IsInternal && currentUserOrgId > 0 && bulkFulfillmentOrders?.Count() > 0)
                {
                    var currentUserName = userIdentityInfo.Username;
                    var storingOrgPreference = Mapper.Map<IEnumerable<OrganizationPreferenceViewModel>>(bulkFulfillmentOrders);

                    await _organizationPreferenceService.InsertOrUpdateRangeAsync(storingOrgPreference, currentUserOrgId, currentUserName);
                }
            }
            catch (Exception ex)
            {
            }

        }

        /// <summary>
        /// To store contact preference to organization which current login belongs to.
        /// <br/> It works in silent mode that no exception/error thrown if failed.
        /// </summary>
        /// <param name="bulkFulfillmentContacts"></param>
        /// <param name="userIdentityInfo"></param>
        /// <returns></returns>
        private async Task StoreOrgContactPreferenceSilentAsync(IEnumerable<BulkFulfillmentContactViewModel> bulkFulfillmentContacts, IdentityInfo userIdentityInfo)
        {
            try
            {
                var currentUserOrgId = userIdentityInfo.OrganizationId;
                // store only if external users (organization id > 0 valid)
                if (!userIdentityInfo.IsInternal && currentUserOrgId > 0 && bulkFulfillmentContacts?.Count() > 0)
                {
                    var currentUserName = userIdentityInfo.Username;
                    var storingOrgContactPreference = Mapper.Map<IEnumerable<OrgContactPreferenceViewModel>>(bulkFulfillmentContacts);

                    await _orgContactPreferenceService.InsertOrUpdateRangeAsync(storingOrgContactPreference, currentUserOrgId, currentUserName);
                }
            }
            catch (Exception ex)
            {
            }
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

        public void ProceedShippingOrderFormBackground(ShippingFormType formType, long bulkFulfillmentId, string userName)
        {
            string jobDescription = formType.Equals(ShippingFormType.Booking) ? "Booking form" : "Shipping Order form";
            // It will be executed in back-ground
            _queuedBackgroundJobs.Enqueue<ProceedShippingOrderForm>(j => j.ExecuteAsync(jobDescription, bulkFulfillmentId, userName, formType, FulfillmentType.Bulk));
        }
    }
}
