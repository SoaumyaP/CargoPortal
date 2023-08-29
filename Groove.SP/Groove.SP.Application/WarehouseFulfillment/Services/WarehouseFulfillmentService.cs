using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.ApplicationBackgroundJob.Services;
using Groove.SP.Application.Attachment.Services.Interfaces;
using Groove.SP.Application.Attachment.ViewModels;
using Groove.SP.Application.BuyerApproval.Services.Interfaces;
using Groove.SP.Application.BuyerApproval.ViewModels;
using Groove.SP.Application.BuyerComplianceService.Services.Interfaces;
using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.POFulfillment;
using Groove.SP.Application.POFulfillmentContact.Mappers;
using Groove.SP.Application.Providers.BlobStorage;
using Groove.SP.Application.Utilities;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.WarehouseFulfillment.Services.Interfaces;
using Groove.SP.Application.WarehouseFulfillment.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Infrastructure.CSFE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OrganizationRole = Groove.SP.Core.Models.OrganizationRole;
using Event = Groove.SP.Core.Models.Event;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Application.ApplicationBackgroundJob;
using Hangfire;
using static Groove.SP.Application.ApplicationBackgroundJob.SendMailBackgroundJobs;
using Groove.SP.Infrastructure.RazorLight;
using Groove.SP.Infrastructure.DinkToPdf;
using Groove.SP.Application.WarehouseFulfillment.BackgroundJobs;
using Groove.SP.Application.POFulfillmentCargoReceive.ViewModels;
using Groove.SP.Application.IntegrationLog.ViewModels;

namespace Groove.SP.Application.WarehouseFulfillment.Services
{
    public class WarehouseFulfillmentService : ServiceBase<POFulfillmentModel, WarehouseFulfillmentViewModel>, IWarehouseFulfillmentService
    {
        private readonly IPOFulfillmentRepository _poFulfillmentRepository;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IAttachmentService _attachmentService;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IServiceProvider _services;
        private readonly IActivityService _activityService;
        private readonly IQueuedBackgroundJobs _queuedBackgroundJobs;
        private readonly IBuyerComplianceService _buyerComplianceService;
        private readonly IBlobStorage _blobStorage;
        private readonly ILogger<WarehouseFulfillmentService> _logger;
        private readonly IUserProfileService _userProfileService;
        private readonly IHtmlStringBuilder _htmlStringBuilder;
        private readonly IPdfGenerator _pdfGenerator;
        private readonly IPOFulfillmentCargoReceiveRepository _poFulfillmentCargoReceiveRepository;

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

        private readonly IBuyerApprovalRepository _buyerApprovalRepository;
        private readonly IPOFulfillmentOrderRepository _poFulfillmentOrderRepository;
        private readonly IDataQuery _dataQuery;
        private readonly AppConfig _appConfig;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IPOLineItemRepository _purchaseOrderLineItemRepository;
        private readonly IRepository<IntegrationLogModel> _integrationLogRepository;

        /// <summary>
        /// It is lazy service injection.
        /// Please do not use it directly, use POFulfillmentService instead.
        /// </summary>
        private IPOFulfillmentService _poFulfillmentServiceLazy;

        public WarehouseFulfillmentService(
            IPOFulfillmentRepository poFulfillmentRepository,
            IPOFulfillmentCargoReceiveRepository poFulfillmentCargoReceiveRepository,
            IAttachmentService attachmentService,
            IUnitOfWorkProvider unitOfWorkProvider,
            ICSFEApiClient csfeApiClient,
            IServiceProvider services,
            IActivityService activityService,
            IQueuedBackgroundJobs appBackgroundJob,
            IBuyerComplianceService buyerComplianceService,
            IBlobStorage blobStorage,
            IEdiSonBookingService ediSonBookingService,
            IUserProfileService userProfileService,
            IPOFulfillmentOrderRepository poFulfillmentOrderRepository,
            IDataQuery dataQuery,
            IBuyerApprovalRepository buyerApprovalRepository,
            ILogger<WarehouseFulfillmentService> logger,
            IOptions<AppConfig> appConfig,
            IHtmlStringBuilder htmlStringBuilder,
            IPdfGenerator pdfGenerator,
            IPurchaseOrderService purchaseOrderService,
            IRepository<IntegrationLogModel> integrationLogRepository
            ) : base(unitOfWorkProvider)
        {
            _poFulfillmentRepository = poFulfillmentRepository;
            _poFulfillmentCargoReceiveRepository = poFulfillmentCargoReceiveRepository;
            _attachmentRepository = (IAttachmentRepository)UnitOfWork.GetRepository<AttachmentModel>();
            _attachmentService = attachmentService;
            _csfeApiClient = csfeApiClient;
            _userProfileService = userProfileService;
            _purchaseOrderRepository = (IPurchaseOrderRepository)UnitOfWork.GetRepository<PurchaseOrderModel>();
            _services = services;
            _activityService = activityService;
            _queuedBackgroundJobs = appBackgroundJob;
            _buyerComplianceService = buyerComplianceService;
            _blobStorage = blobStorage;
            _activityService = activityService;
            _buyerApprovalRepository = buyerApprovalRepository;
            _poFulfillmentOrderRepository = poFulfillmentOrderRepository;
            _purchaseOrderRepository = (IPurchaseOrderRepository)UnitOfWork.GetRepository<PurchaseOrderModel>();
            _purchaseOrderLineItemRepository = (IPOLineItemRepository)UnitOfWork.GetRepository<POLineItemModel>();
            _dataQuery = dataQuery;
            _services = services;
            _logger = logger;
            _appConfig = appConfig.Value;
            _purchaseOrderService = purchaseOrderService;
            _htmlStringBuilder = htmlStringBuilder;
            _pdfGenerator = pdfGenerator;
            _integrationLogRepository = integrationLogRepository;
        }

        public async Task ConfirmWarehouseBookingsAsync(IdentityInfo currentUser, IEnumerable<InputConfirmWarehouseFulfillmentViewModel> viewModels)
        {
            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
           => x.Include(m => m.Orders).Include(c => c.Contacts);

            var pofIds = viewModels.Select(c => c.Id).ToList();
            var warehouseBookings = await _poFulfillmentRepository.Query(c => pofIds.Contains(c.Id) && c.Status == POFulfillmentStatus.Active && c.Stage == POFulfillmentStage.ForwarderBookingRequest, null, includeProperties).ToListAsync();
            try
            {
                UnitOfWork.BeginTransaction();
                foreach (var item in warehouseBookings)
                {
                    var viewModel = viewModels.SingleOrDefault(c => c.Id == item.Id);
                    item.Stage = POFulfillmentStage.ForwarderBookingConfirmed;
                    item.ConfirmBy = currentUser.Email;
                    item.ConfirmedAt = DateTime.UtcNow;
                    item.ConfirmedHubArrivalDate = viewModel.ConfirmedHubArrivalDate;
                    item.Time = viewModel.Time;
                    item.LoadingBay = viewModel.LoadingBay;
                    item.SONo = $"{item.Number}01";

                    var event1061 = new ActivityViewModel()
                    {
                        ActivityCode = Event.EVENT_1061,
                        POFulfillmentId = item.Id,
                        ActivityDate = DateTime.UtcNow,
                        Remark = item.SONo,
                        CreatedBy = currentUser.Username
                    };
                    await _activityService.TriggerAnEvent(event1061);
                }

                var order = warehouseBookings.SelectMany(s => s.Orders).ToList();
                var poIds = order.Select(c => c.PurchaseOrderId).ToList();
                var purchaseOrders = await _purchaseOrderRepository.Query(c => poIds.Contains(c.Id)).ToListAsync();
                foreach (var item in purchaseOrders)
                {
                    if (item.Stage == POStageType.ForwarderBookingRequest)
                    {
                        item.Stage = POStageType.ForwarderBookingConfirmed;
                    }
                }

                await UnitOfWork.SaveChangesAsync();
                UnitOfWork.CommitTransaction();
            }

            catch (Exception ex)
            {
                UnitOfWork.RollbackTransaction();
                throw ex;
            }

            foreach (var pofId in pofIds)
            {
                _queuedBackgroundJobs.Enqueue<ProceedWarehouseShippingOrderForm>(c => c.ExecuteAsync(pofId, currentUser.Username));
            }
        }

        public async Task<IEnumerable<ConfirmWarehouseFulfillmentViewModel>> SearchWarehouseBookingToConfirmAsync(string jsonFilter, IdentityInfo currentUser, string affiliates = "")
        {
            var storedProcedureName = "spu_GetWarehouseBooking_Confirm";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@JsonFilterSet",
                        Value = jsonFilter,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Affiliates",
                        Value = affiliates?.Replace("[", "")?.Replace("]", ""),
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@IsInternal",
                        Value = currentUser.IsInternal,
                        DbType = DbType.Boolean,
                        Direction = ParameterDirection.Input
                    }
                };
            Func<DbDataReader, IEnumerable<ConfirmWarehouseFulfillmentViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<ConfirmWarehouseFulfillmentViewModel>();

                while (reader.Read())
                {
                    var newRow = new ConfirmWarehouseFulfillmentViewModel();
                    object tmpValue;

                    // Must be in order of data reader
                    tmpValue = reader[0];
                    newRow.Id = (long)tmpValue;

                    tmpValue = reader[1];
                    newRow.BookingNumber = tmpValue.ToString();

                    tmpValue = reader[2];
                    newRow.ShipmentNumber = tmpValue.ToString();

                    tmpValue = reader[3];
                    newRow.ExpectedDeliveryDate = DBNull.Value == tmpValue ? null : (DateTime?)tmpValue;

                    tmpValue = reader[4];
                    newRow.CustomerOrgId = (long)tmpValue;

                    tmpValue = reader[5];
                    newRow.SupplierOrgId = (long)tmpValue;

                    tmpValue = reader[6];
                    newRow.WarehouseLocation = tmpValue.ToString();

                    mappedData.Add(newRow);
                }

                return mappedData;
            };

            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, parameters.ToArray());
            return result;
        }

        public async Task<BuyerApprovalViewModel> ApproveAsync(long id, BuyerApprovalViewModel viewModel, string userName)
        {
            UnitOfWork.BeginTransaction();

            var penddingApprovalModel = await _buyerApprovalRepository.GetAsync(x => x.Id == id, null, x => x.Include(y => y.POFulfillment).ThenInclude(c => c.Orders));

            if (penddingApprovalModel == null && penddingApprovalModel.Stage != BuyerApprovalStage.Pending && penddingApprovalModel.POFulfillment.Stage != POFulfillmentStage.Draft)
            {
                throw new AppEntityNotFoundException($"Object with the id {id} not found!");
            }

            penddingApprovalModel.Stage = BuyerApprovalStage.Approved;
            penddingApprovalModel.Status = BuyerApprovalStatus.Active;
            penddingApprovalModel.Owner = userName;
            penddingApprovalModel.ExceptionDetail = viewModel.ExceptionDetail;
            penddingApprovalModel.ResponseOn = DateTime.UtcNow;

            _buyerApprovalRepository.Update(penddingApprovalModel);

            var event1058 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1058,
                POFulfillmentId = penddingApprovalModel.POFulfillment.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = userName,
                Remark = penddingApprovalModel.ExceptionDetail
            };
            await _activityService.TriggerAnEvent(event1058);

            #region Update stage for related single/allocated/blanket POs
            var purchaseOrderListId = new List<long>();
            var poList = await _purchaseOrderRepository.Query(x => penddingApprovalModel.POFulfillment.Orders.Select(x => x.PurchaseOrderId).Contains(x.Id)).ToListAsync();
            foreach (var po in poList)
            {
                if (po.Stage == POStageType.Released)
                {
                    po.Stage = POStageType.ForwarderBookingRequest;
                }
            }
            #endregion

            await UnitOfWork.SaveChangesAsync();
            UnitOfWork.CommitTransaction();

            viewModel = Mapper.Map<BuyerApprovalViewModel>(penddingApprovalModel);
            BackgroundJob.Enqueue<WarehouseApprovalJob>(c => c.ExecuteAsync(penddingApprovalModel.POFulfillment.Id));
            return viewModel;
        }

        public async Task<BuyerApprovalViewModel> RejectAsync(long id, BuyerApprovalViewModel viewModel, string userName)
        {
            Func<IQueryable<BuyerApprovalModel>, IQueryable<BuyerApprovalModel>> includeProperties = x => x.Include(m => m.POFulfillment);
            var model = await _buyerApprovalRepository.GetAsync(x => x.Id == id, null, includeProperties);
            if (model == null && model.Stage != BuyerApprovalStage.Pending)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", id)} not found!");
            }

            model.Stage = BuyerApprovalStage.Rejected;
            model.Status = BuyerApprovalStatus.Active;
            model.Owner = userName;
            model.ExceptionDetail = viewModel.ExceptionDetail;
            model.POFulfillment.Stage = POFulfillmentStage.Draft;
            model.POFulfillment.IsRejected = true;
            model.POFulfillment.BookingDate = null;
            model.ResponseOn = DateTime.UtcNow;

            var event1059 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1059,
                POFulfillmentId = model.POFulfillment.Id,
                Remark = model.ExceptionDetail,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = userName
            };
            await _activityService.TriggerAnEvent(event1059);

            _buyerApprovalRepository.Update(model);

            if (viewModel.POFulfillmentId.HasValue)
            {
                _poFulfillmentServiceLazy = _services.GetRequiredService<IPOFulfillmentService>();
                // Update stage of related purchase orders
                await _poFulfillmentServiceLazy.UpdatePurchaseOrderStageByPOFFAsync(viewModel.POFulfillmentId.Value);
                _poFulfillmentServiceLazy.ReleaseQuantityOnPOLineItems(viewModel.POFulfillmentId ?? 0);
            }

            await this.UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<BuyerApprovalViewModel>(model);
            return viewModel;
        }

        public async Task<WarehouseFulfillmentViewModel> CancelAsync(long id, string userName, CancelWarehouseFulfillmentViewModel cancelViewModel)
        {
            UnitOfWork.BeginTransaction();
            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
             => x.Include(m => m.Contacts)
                 .Include(m => m.Loads)
                 .Include(m => m.Orders)
                 .Include(m => m.BuyerApprovals);

            var warehouseBooking = await Repository.GetAsync(x => x.Id == id, null, includeProperties);

            if (warehouseBooking != null && warehouseBooking.Stage <= POFulfillmentStage.ForwarderBookingConfirmed)
            {
                warehouseBooking.Status = POFulfillmentStatus.Inactive;
                warehouseBooking.IsRejected = false;
                warehouseBooking.BookingDate = null;

                var event1057 = new ActivityViewModel()
                {
                    ActivityCode = Event.EVENT_1057,
                    POFulfillmentId = warehouseBooking.Id,
                    ActivityDate = DateTime.UtcNow,
                    CreatedBy = userName,
                    Remark = cancelViewModel.Reason
                };
                await _activityService.TriggerAnEvent(event1057);

                foreach (var load in warehouseBooking.Loads)
                {
                    load.Status = POFulfillmentLoadStatus.Inactive;
                }

                foreach (var order in warehouseBooking.Orders)
                {
                    order.Status = POFulfillmentOrderStatus.Inactive;
                }

                Repository.Update(warehouseBooking);

                // When user cancel a FB Request fulfillment which has a pending approval associated,
                // update stage of buyer approval to Cancel as well.
                var buyerApproval = await _buyerApprovalRepository.GetAsync(x => x.POFulfillmentId == warehouseBooking.Id && x.Stage == BuyerApprovalStage.Pending);
                if (buyerApproval != null)
                {
                    buyerApproval.Stage = BuyerApprovalStage.Cancel;
                    _buyerApprovalRepository.Update(buyerApproval);
                }

                // Cancel a Draft booking, we should NOT release PO stage / quantity anymore because it’s done before when user reject the booking.
                if (warehouseBooking.Stage > POFulfillmentStage.Draft)
                {
                    // Update PO stage
                    await UpdatePurchaseOrderStageByPOFFAsync(warehouseBooking);

                    _poFulfillmentServiceLazy = _services.GetRequiredService<IPOFulfillmentService>();
                    _poFulfillmentServiceLazy.ReleaseQuantityOnPOLineItems(warehouseBooking.Id);
                }

                // Remove all POAdhocChanges for the Booking
                DeletePurchaseOrderAdhocChanges(0, id, 0);

                await UnitOfWork.SaveChangesAsync();
                UnitOfWork.CommitTransaction();

                return Mapper.Map<WarehouseFulfillmentViewModel>(warehouseBooking);
            }

            return null;
        }

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
                    }
                }
            }
        }

        public async Task<WarehouseFulfillmentViewModel> GetAsync(long id, bool isInternal, string affiliates)
        {
            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
            => x.Include(m => m.Contacts)
                .Include(m => m.Orders).ThenInclude(m => m.POFulfillmentCargoReceiveItem)
                .Include(m => m.BuyerApprovals);

            var listOfAffiliates = new List<long>();
            POFulfillmentModel model = null;

            if (isInternal)
            {
                model = await Repository.GetAsync(p => p.Id == id && p.FulfillmentType == FulfillmentType.Warehouse,
                    null,
                    includeProperties);
            }
            else
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }
                model = await Repository.GetAsync(p => p.Id == id && p.FulfillmentType == FulfillmentType.Warehouse && p.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId)),
                    null,
                    includeProperties);
            }

            var viewModel = Mapper.Map<WarehouseFulfillmentViewModel>(model);
            if (model != null)
            {
                if (viewModel.Contacts != null)
                {
                    foreach (var contact in viewModel.Contacts)
                    {
                        switch (contact.OrganizationRole)
                        {
                            case OrganizationRole.Principal:
                                contact.ContactSequence = RoleSequence.Principal;
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
                            case OrganizationRole.WarehouseProvider:
                                contact.ContactSequence = RoleSequence.WarehouseProvider;
                                break;
                            default:
                                break;
                        }
                    }
                    viewModel.Contacts = viewModel.Contacts.OrderBy(c => c.ContactSequence).ToList();
                }

                #region populate data for POFulfillmentOrders from article master
                var articalMasterInfo = new List<POLineItemArticleMasterViewModel>();
                var principalContact = viewModel.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Principal);
                if (principalContact != null)
                {
                    var principalOrg = await _csfeApiClient.GetOrganizationByIdAsync(principalContact.OrganizationId);
                    if (principalOrg != null)
                    {
                        articalMasterInfo = (await _purchaseOrderService.GetInformationFromArticleMaster(principalOrg.Code))?.ToList();
                    }
                }

                foreach (var order in viewModel.Orders)
                {
                    var articalMaster = articalMasterInfo?.FirstOrDefault(x => x.ItemNo == order.ProductCode && order.POLineItemId > 0);

                    if (articalMaster == null) // If null -> try to get by [StyleNo, ColourCode, Size]
                    {
                        articalMaster = articalMasterInfo?.FirstOrDefault(x
                                => String.Equals(x.StyleNo?.Trim(), order.StyleNo?.Trim(), StringComparison.OrdinalIgnoreCase)
                                && String.Equals(x.ColourCode?.Trim(), order.ColourCode?.Trim(), StringComparison.OrdinalIgnoreCase)
                                && String.Equals(x.Size?.Trim(), order.Size?.Trim(), StringComparison.OrdinalIgnoreCase));
                    }

                    if (articalMaster != null)
                    {
                        order.OuterQuantity = articalMaster.OuterQuantity;
                        order.InnerQuantity = articalMaster.InnerQuantity;
                        order.StyleName = articalMaster.StyleName;
                        order.ColourName = articalMaster.ColourName;
                    }
                }
                #endregion populate data for POFulfillmentOrders from article master

                if (model.Stage >= POFulfillmentStage.ForwarderBookingConfirmed)
                {                   

                    var pricipalOrgId = viewModel?.Contacts?.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Principal)?.OrganizationId;
                    if (pricipalOrgId != null && pricipalOrgId.Value > 0)
                    {
                        var warehouseAssignments = await _csfeApiClient.GetWarehouseAssignmentsByOrgIdAsync(pricipalOrgId.Value);
                        if (warehouseAssignments != null && warehouseAssignments.Any())
                        {
                            var warehouseAssignment = warehouseAssignments.FirstOrDefault();
                            // Fulfill Warehouse location information
                            var warehouseLocation = warehouseAssignment?.WarehouseLocation;
                            viewModel.LocationName = warehouseLocation?.Name;
                            viewModel.LocationCode = warehouseLocation?.Code;
                            viewModel.AddressLine1 = warehouseLocation?.AddressLine1;
                            viewModel.AddressLine2 = warehouseLocation?.AddressLine2;
                            viewModel.AddressLine3 = warehouseLocation?.AddressLine3;
                            viewModel.AddressLine4 = warehouseLocation?.AddressLine4;
                            viewModel.City = warehouseLocation?.Location.LocationDescription;
                            viewModel.Country = warehouseLocation?.Location?.Country.Name;

                            // Try to get from warehouse assignments if possible
                            viewModel.ContactPhone = warehouseAssignment?.ContactPhone;
                            viewModel.ContactEmail = warehouseAssignment?.ContactEmail;
                            viewModel.ContactPerson = warehouseAssignment?.ContactPerson;
                        }
                    }
                }
            }
            return viewModel;
        }



        public async Task<WarehouseFulfillmentViewModel> UpdateAsync(InputWarehouseFulfillmentViewModel model, IdentityInfo currentUser)
        {
            var userName = currentUser.Username;

            Func<IQueryable<POFulfillmentModel>, IQueryable<POFulfillmentModel>> includeProperties = x
            => x.Include(m => m.Contacts)
                .Include(m => m.Orders)
                .Include(m => m.BookingRequests)
                .Include(m => m.BuyerApprovals);
            var poFulfillment = await Repository.GetAsync(x => x.Id == model.Id && x.FulfillmentType == FulfillmentType.Warehouse, null, includeProperties);

            if (poFulfillment == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", model.Id)} not found!");
            }

            #region General
            // Mapping POFulfillment
            poFulfillment.Number = model.Number;
            poFulfillment.Owner = model.Owner;
            poFulfillment.Status = model.Status;
            poFulfillment.IsRejected = false;
            poFulfillment.Stage = model.Stage;
            poFulfillment.PlantNo = model.PlantNo;
            poFulfillment.CargoReadyDate = model.CargoReadyDate;
            poFulfillment.Remarks = model.Remarks;
            #endregion General

            #region Contacts
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
            #endregion Contacts

            #region Attachments
            await ProceedAttachmentsAsync(model.Attachments?.ToList(), poFulfillment.Id, currentUser);
            #endregion Attachments

            Repository.Update(poFulfillment);
            await UnitOfWork.SaveChangesAsync();

            var result = Mapper.Map<WarehouseFulfillmentViewModel>(poFulfillment);

            return result;
        }

        /// <summary>
        /// To handle cargo received from GUI
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viewModels"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        /// <exception cref="AppEntityNotFoundException"></exception>
        public async Task CargoReceiveAsync(long id, List<WarehouseFulfillmentOrderViewModel> viewModels, IdentityInfo currentUser)
        {
            var poFulfillment = await _poFulfillmentRepository.GetAsync(x => x.Id == id, null, x => x.Include(y => y.Orders));

            if (poFulfillment == null)
            {
                throw new AppEntityNotFoundException($"Object with id = {id} not found!");
            }

            var newPOFFCargoReceive = new POFulfillmentCargoReceiveModel
            {
                POFulfillmentId = poFulfillment.Id,
                PlantNo = poFulfillment.PlantNo ?? string.Empty,
                CargoReceiveItems = new List<POFulfillmentCargoReceiveItemModel>()
            };

            foreach (var order in viewModels)
            {
                if (order.Received)
                {
                    var poffOrderModel = poFulfillment.Orders.SingleOrDefault(x => x.Id == order.Id);
                    if (poffOrderModel != null)
                    {
                        poffOrderModel.Status = POFulfillmentOrderStatus.Received;
                    }
                    var newCargoReceiveItem = new POFulfillmentCargoReceiveItemModel
                    {
                        POFulfillmentOrderId = order.Id,
                        Quantity = order.ReceivedQty.Value,
                        InDate = order.ReceivedDate.Value
                    };
                    newCargoReceiveItem.Audit(currentUser.Username);
                    newPOFFCargoReceive.CargoReceiveItems.Add(newCargoReceiveItem);
                }
            }

            newPOFFCargoReceive.Audit(currentUser.Username);
            await _poFulfillmentCargoReceiveRepository.AddAsync(newPOFFCargoReceive);

            poFulfillment.Stage = POFulfillmentStage.CargoReceived;
            poFulfillment.Audit(currentUser.Username);

            // Trigger event #1063-Cargo Received
            var event1063 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1063,
                POFulfillmentId = poFulfillment.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = currentUser.Username
            };
            await _activityService.TriggerAnEvent(event1063);

            // Update related POs
            await ChangePOStageToCargoReceiveAsync(poFulfillment.Orders, currentUser.Username);

            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Fully cargo recived from mobile app scanning
        /// </summary>
        /// <param name="bookingNumber"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<WarehouseCargoReceiveDateMobileModel> FullCargoReceiveAsync(string bookingNumber, string userName)
        {
            var warehouseBooking = await _poFulfillmentRepository.GetAsync(x => x.Number == bookingNumber && x.FulfillmentType == FulfillmentType.Warehouse,
                                                                                includes: i => i.Include(y => y.Orders)
                                                                                                .ThenInclude(y => y.POFulfillmentCargoReceiveItem));
            if (warehouseBooking == null)
            {
                throw new AppEntityNotFoundException($"Object with number {bookingNumber} not found!");
            }

            if (warehouseBooking.Stage < POFulfillmentStage.ForwarderBookingConfirmed)
            {
                throw new AppValidationException("Booking stage is not confirmed yet.");
            }

            /*
             * Check and return if booking has been cargo received.
             */
            var firstReceivedOrder = warehouseBooking.Orders?.FirstOrDefault(x => x.POFulfillmentCargoReceiveItem != null)?.POFulfillmentCargoReceiveItem;
            if (firstReceivedOrder != null)
            {
                return new WarehouseCargoReceiveDateMobileModel
                {
                    CargoReceivedDate = firstReceivedOrder.InDate
                };
            }

            DateTime inDate = DateTime.UtcNow;

            var newPOFFCargoReceive = new POFulfillmentCargoReceiveModel
            {
                POFulfillmentId = warehouseBooking.Id,
                PlantNo = warehouseBooking.PlantNo ?? string.Empty,
                CargoReceiveItems = new List<POFulfillmentCargoReceiveItemModel>()
            };

            foreach (var order in warehouseBooking.Orders)
            {
                order.Status = POFulfillmentOrderStatus.Received;
                var newCargoReceiveItem = new POFulfillmentCargoReceiveItemModel
                {
                    POFulfillmentOrderId = order.Id,
                    Quantity = order.BookedPackage.Value,
                    InDate = inDate
                };
                newCargoReceiveItem.Audit(userName);
                newPOFFCargoReceive.CargoReceiveItems.Add(newCargoReceiveItem);
            }

            newPOFFCargoReceive.Audit(userName);
            await _poFulfillmentCargoReceiveRepository.AddAsync(newPOFFCargoReceive);

            warehouseBooking.Stage = POFulfillmentStage.CargoReceived;
            warehouseBooking.Audit(userName);

            // Trigger event #1063-Cargo Received
            var event1063 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1063,
                POFulfillmentId = warehouseBooking.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = userName
            };
            await _activityService.TriggerAnEvent(event1063);

            // Update related POs
            await ChangePOStageToCargoReceiveAsync(warehouseBooking.Orders, userName);

            await UnitOfWork.SaveChangesAsync();

            return new WarehouseCargoReceiveDateMobileModel
            {
                CargoReceivedDate = inDate
            };
        }

        private async Task ChangePOStageToCargoReceiveAsync(IEnumerable<POFulfillmentOrderModel> poffOrders, string currentUser)
        {
            var poIds = poffOrders.Where(x => x.PurchaseOrderId > 0 && x.Status == POFulfillmentOrderStatus.Received).Select(x => x.PurchaseOrderId);
            var pos = await _purchaseOrderRepository.Query(x => poIds.Contains(x.Id)).ToListAsync();
            foreach (var po in pos)
            {
                if (po.Stage == POStageType.ForwarderBookingConfirmed)
                {
                    po.Stage = POStageType.CargoReceived;
                    po.Audit(currentUser);
                }
            }
        }

        public async Task<ImportingWarehouseBookingResultViewModel> ImportWarehouseBookingSilentAsync(ImportWarehouseBookingViewModel importData, string profile)
        {

            var importingResult = ValidateWarehouseBookingImportContent(importData, out var inputBookingModel);
            var warehouseBookingViewModel = new WarehouseFulfillmentViewModel();

            if (importingResult.Success)
            {
                try
                {
                    var bookingOwner = importData.Owner;
                    var emailSubject = importData.EmailSubject;
                    warehouseBookingViewModel = await CreateWarehouseBookingAsync(inputBookingModel, bookingOwner, emailSubject);
                    await AttachWarehouseBookingFormAsync(warehouseBookingViewModel, bookingOwner, importData.Files);
                    await ProceedWarehouseBookingAsync(warehouseBookingViewModel.Id, bookingOwner);
                    importingResult.LogBookingSuccess("Id", $"{warehouseBookingViewModel.Id}");
                    importingResult.LogBookingSuccess("Number", $"{warehouseBookingViewModel.Number}");
                    importingResult.LogBookingSuccess("Url", $"{_appConfig.ClientUrl}/warehouse-bookings/view/{warehouseBookingViewModel.Id}");
                }
                catch (Exception ex)
                {
                    importingResult.LogErrors("Message", $"{ex.Message}");
                    importingResult.Exception = ex;
                    _logger.LogError(ex, "Import warehouse booking failed");
                }
            }

            // Get pricipal organization and buyer compliance from inputBookingModel
            IEnumerable<WarehouseAssignment> warehouseAssignments = null;
            BuyerComplianceModel buyerCompliance = null;
            Organization customerOrganization;

            buyerCompliance = inputBookingModel?.BuyerCompliance;
            customerOrganization = inputBookingModel?.PrincipalOrganization;

            var customerOrganizationId = customerOrganization?.Id;
            if (customerOrganizationId != null && customerOrganizationId.Value > 0)
            {
                warehouseAssignments = await _csfeApiClient.GetWarehouseAssignmentsByOrgIdAsync(customerOrganizationId.Value);
            }

            // Import warehouse booking successfully
            // Send mail
            if (importingResult.Success)
            {
                var emailSettings = GetReceipientsFromBuyerCompliance(buyerCompliance.EmailSettings.ToList(), EmailSettingType.BookingImportedSuccessfully, importData.Owner);

                var emailModel = new WarehouseBookingEmailViewModel
                {
                    CustomerName = warehouseBookingViewModel.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Principal)?.CompanyName,
                    WarehouseAssignment = warehouseAssignments.FirstOrDefault()
                };

                var attachments = new List<SPEmailAttachment>()
                {
                    new SPEmailAttachment
                    {
                        AttachmentContent = importData.BookingForm.GetAllBytes(),
                        AttachmentName = importData.BookingForm.FileName
                    }
                };

                BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailWithCCAsync(
                   $"Warehouse Booking is placed successfully #{warehouseBookingViewModel.Id}",
                   "WarehouseBooking_Import_Success",
                   emailModel,
                   emailSettings["sendTo"],
                   emailSettings["cc"],
                   $"Re: {importData.EmailSubject}",
                   attachments.ToArray())
                );
            }
            else
            {
                // Write integration log if error
                await WriteIntegrationLogAsync(importingResult, importData, profile);

                // Only send mail back to vendor if error is not from API param or internal server error
                if (!string.IsNullOrEmpty(importData.Owner) && importingResult.ResultType != ImportingWarehouseBookingResult.ErrorDuringImport)
                {
                    // Send mail
                    var emailModel = new WarehouseBookingEmailViewModel
                    {
                        CustomerName = customerOrganization?.Name,
                        WarehouseAssignment = warehouseAssignments?.FirstOrDefault(),
                        FailureReasons = importingResult.ExportLogDetails()
                    };

                    var templateName = "WarehouseBooking_Import_IncorrectFileNaming";
                    switch (importingResult.ResultType)
                    {
                        case ImportingWarehouseBookingResult.IncorrectFileNaming:
                            templateName = "WarehouseBooking_Import_IncorrectFileNaming";
                            break;
                        case ImportingWarehouseBookingResult.DuplicatedFile:
                            templateName = "WarehouseBooking_Import_DuplicatedFileNaming";
                            break;
                        case ImportingWarehouseBookingResult.InvalidBookingForm:
                            templateName = "WarehouseBooking_Import_Failed";
                            break;
                    }

                    // attach all uploaded files into email
                    var attachments = new List<SPEmailAttachment>();
                    if (importData.Files != null && importData.Files.Any())
                    {
                        foreach (var file in importData.Files)
                        {
                            var attachment = new SPEmailAttachment
                            {
                                AttachmentContent = file.GetAllBytes(),
                                AttachmentName = file.FileName
                            };
                            attachments.Add(attachment);
                        }
                    }

                    var emailSettings = GetReceipientsFromBuyerCompliance(buyerCompliance.EmailSettings.ToList(), EmailSettingType.BookingImportedFailure, importData.Owner);

                    switch (importingResult.ResultType)
                    {
                        case ImportingWarehouseBookingResult.IncorrectFileNaming:
                            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailWithCCAsync(
                                  $"Warehouse Booking failed: Incorrect file naming",
                                  templateName,
                                  emailModel,
                                  emailSettings["sendTo"],
                                  emailSettings["cc"],
                                  $"Re: {importData.EmailSubject}",
                                  attachments.ToArray()
                                  ));
                            break;

                        case ImportingWarehouseBookingResult.DuplicatedFile:
                            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailWithCCAsync(
                                  $"Warehouse Booking failed: Duplicated documents",
                                  templateName,
                                  emailModel,
                                  emailSettings["sendTo"],
                                  emailSettings["cc"],
                                  $"Re: {importData.EmailSubject}",
                                  attachments.ToArray()
                                  ));
                            break;

                        case ImportingWarehouseBookingResult.InvalidBookingForm:
                            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailWithCCAsync(
                                  $"Warehouse Booking failed: Invalid booking form",
                                  templateName,
                                  emailModel,
                                  emailSettings["sendTo"],
                                  emailSettings["cc"],
                                  $"Re: {importData.EmailSubject}",
                                  attachments.ToArray()
                                  ));
                            break;
                    };

                }
            }
            return importingResult;

        }

        /// <summary>
        /// To handle import cargo receive item from API
        /// </summary>
        /// <param name="importData"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// <exception cref="AppEntityNotFoundException"></exception>
        /// <exception cref="AppValidationException"></exception>
        public async Task ImportCargoReceiveAsync(ImportPOFulfillmentCargoReceiveViewModel importData, string userName)
        {
            var poffModel = await _poFulfillmentRepository.GetAsync(x => x.Number == importData.BookingNo && x.FulfillmentType == FulfillmentType.Warehouse,
                null,
                x => x.Include(y => y.Orders));

            if (poffModel == null)
            {
                throw new AppEntityNotFoundException($"Booking #{importData.BookingNo} not found!");
            }

            if (poffModel.Stage < POFulfillmentStage.ForwarderBookingConfirmed)
            {
                throw new AppValidationException("Booking stage is not confirmed yet.");
            }

            var poffCargoReceiveModel = await _poFulfillmentCargoReceiveRepository.GetAsync(x => x.POFulfillmentId == poffModel.Id,
                null,
                x => x.Include(y => y.CargoReceiveItems));
            if (poffCargoReceiveModel == null)
            {
                poffCargoReceiveModel = new POFulfillmentCargoReceiveModel
                {
                    POFulfillmentId = poffModel.Id,
                    CRNo = importData.CRNo,
                    PlantNo = importData.PlantNo,
                    HouseNo = importData.HouseNo,
                    CargoReceiveItems = new List<POFulfillmentCargoReceiveItemModel>()
                };
                poffCargoReceiveModel.Audit(userName);
            }
            else
            {
                poffCargoReceiveModel.CRNo = importData.CRNo;
                poffCargoReceiveModel.PlantNo = importData.PlantNo;
                poffCargoReceiveModel.HouseNo = importData.HouseNo;
            }

            foreach (var customerPO in importData.CustomerPO)
            {
                var poffOrderModel = poffModel.Orders.FirstOrDefault(x => x.CustomerPONumber == customerPO.PONo.Trim() && x.ProductCode == customerPO.ProductCode.Trim());
                if (poffOrderModel != null)
                {
                    var isImported = poffCargoReceiveModel.CargoReceiveItems.FirstOrDefault(x => x.POFulfillmentOrderId == poffOrderModel.Id) != null;
                    if (!isImported)
                    {
                        var newCargoReceiveItemModel = Mapper.Map<POFulfillmentCargoReceiveItemModel>(customerPO);
                        newCargoReceiveItemModel.POFulfillmentOrderId = poffOrderModel.Id;

                        newCargoReceiveItemModel.Audit(userName);
                        poffCargoReceiveModel.CargoReceiveItems.Add(newCargoReceiveItemModel);

                        poffOrderModel.Status = POFulfillmentOrderStatus.Received;
                    }
                }
            }

            if (poffCargoReceiveModel.Id == 0)
            {
                await _poFulfillmentCargoReceiveRepository.AddAsync(poffCargoReceiveModel);
            }

            if (poffModel.Stage == POFulfillmentStage.ForwarderBookingConfirmed)
            {
                poffModel.Stage = POFulfillmentStage.CargoReceived;

                // Trigger event #1063-Cargo Received
                var event1063 = new ActivityViewModel()
                {
                    ActivityCode = Event.EVENT_1063,
                    POFulfillmentId = poffModel.Id,
                    ActivityDate = DateTime.UtcNow,
                    CreatedBy = userName
                };
                await _activityService.TriggerAnEvent(event1063);
            }

            // Update related POs
            await ChangePOStageToCargoReceiveAsync(poffModel.Orders, userName);

            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// To validate import data to import warehouse booking
        /// </summary>
        /// <param name="importData">Import data to validate</param>
        /// <param name="outputData">If validation passes, it will contain value</param>
        /// <returns>Validation result</returns>
        private ImportingWarehouseBookingResultViewModel ValidateWarehouseBookingImportContent(ImportWarehouseBookingViewModel importData, out InputWarehouseFulfillmentViewModel outputData)
        {
            outputData = null;

            var importingResult = new ImportingWarehouseBookingResultViewModel();

            var activeOrganizations = _csfeApiClient.GetActiveOrganizationsAsync().Result;

            #region Check request params

            var customerCode = importData.Customer;
            Organization principalOrg = null;

            if (string.IsNullOrEmpty(importData.Owner))
            {
                importingResult.LogErrors("Booking", "Owner is missing");
            }

            if (!importData.CreatedDate.HasValue)
            {
                importingResult.LogErrors("Booking", "CreatedDate is missing");
            }

            if (string.IsNullOrEmpty(importData.EmailSubject))
            {
                importingResult.LogErrors("Booking", "EmailSubject is missing");
            }

            if (string.IsNullOrEmpty(customerCode))
            {
                importingResult.LogErrors("Booking", "Customer is missing");
            }
            else
            {
                principalOrg = activeOrganizations.FirstOrDefault(x => x.Code.Equals(customerCode, StringComparison.InvariantCultureIgnoreCase));
                if (principalOrg == null)
                {
                    importingResult.LogErrors($"Booking", $"Customer code '${customerCode}' doesn't exist in the system.");
                }
            }


            BuyerComplianceModel buyerCompliance = null;
            // Check Buyer compliance
            if (principalOrg != null)
            {
                buyerCompliance = _buyerComplianceService.GetByOrgIdAsync(principalOrg.Id).Result;

                // buyer compliance must be in activated and warehouse type
                if (buyerCompliance == null || buyerCompliance.Stage != BuyerComplianceStage.Activated || buyerCompliance.Status == BuyerComplianceStatus.Inactive)
                {
                    importingResult.LogErrors("Booking", $"Buyer compliance of Customer code {importData.Customer} is unavailable or inactive.");
                }
            }
           
            if (!importingResult.Success)
            {
                return importingResult;
            }

            #endregion Check request params


            // Try to get principal organization contact and buyer compliance to send email
            if (outputData == null)
            {
                outputData = new InputWarehouseFulfillmentViewModel();
            }
            outputData.PrincipalOrganization = principalOrg;
            outputData.BuyerCompliance = buyerCompliance;


            #region To check names of uploaded files

            var uploadedFiles = importData.Files;
            if (uploadedFiles != null && uploadedFiles.Any())
            {
                var transformedFileNames = uploadedFiles.Select(x => x.FileName.ToLowerInvariant().Replace(" ", string.Empty)).ToList() ?? new List<string>();

                // Check correct naming
                var fileKeys = new[]
                {
                    "bookingform", "packinglist", "commercialinvoice", "coo"
                };
                var isCorrect = transformedFileNames.Any(x => x.Contains("bookingform"))
                                && transformedFileNames.Any(x => x.Contains("packinglist"))
                                && transformedFileNames.Any(x => x.Contains("commercialinvoice"))
                                && !transformedFileNames.Any(x => !x.Contains("bookingform") && !x.Contains("packinglist") && !x.Contains("commercialinvoice") && !x.Contains("coo"));
                if (!isCorrect)
                {

                    if (importData.BookingForm == null)
                    {
                        // quick return that there is no valid booking form
                        importingResult.LogBookingValidationFailed("Booking", "BookingForm is missing", ImportingWarehouseBookingResult.IncorrectFileNaming);
                        return importingResult;
                    }
                    else
                    {
                        importingResult.LogBookingValidationFailed("Booking", "Incorrect file naming", ImportingWarehouseBookingResult.IncorrectFileNaming);

                    }
                }
                else
                {
                    isCorrect = transformedFileNames.Count(x => x.Contains("bookingform")) == 1
                               && transformedFileNames.Count(x => x.Contains("packinglist")) == 1
                               && transformedFileNames.Count(x => x.Contains("commercialinvoice")) == 1
                               && transformedFileNames.Count(x => x.Contains("coo")) <= 1;
                    if (!isCorrect)
                    {
                        importingResult.LogBookingValidationFailed("Booking", "Duplicated documents", ImportingWarehouseBookingResult.DuplicatedFile);
                        
                        // quick return that there is no valid booking form
                        if (transformedFileNames.Count(x => x.Contains("bookingform")) > 1)
                        {
                            return importingResult;
                        }
                    }
                }

            }
            else
            {
                importingResult.LogBookingValidationFailed("Booking", "Upload file is missing", ImportingWarehouseBookingResult.IncorrectFileNaming);
                // quick return that there is no valid booking form
                return importingResult;
            }

            #endregion To check names of uploaded files

            var file = importData.BookingForm.GetAllBytes();           
            var locations = _csfeApiClient.GetAllLocationsAsync().Result;

            using (Stream fileStream = new MemoryStream(file))
            {
                using (var xlPackage = new ExcelPackage(fileStream))
                {
                    var bookingFormSheet = xlPackage.Workbook.Worksheets["BOOKING FORM"];
                    if (bookingFormSheet == null)
                    {
                        importingResult.LogBookingValidationFailed("Booking", "Sheet BOOKING FORM is missing");
                        return importingResult;
                    }

                    #region Validate Booking Information
                    ExcelRange cell;
                    string cellName = "";
                    int lastRowProduct = 29;


                    // Check Vendor name
                    cellName = "C17";
                    cell = bookingFormSheet.Cells[cellName];
                    if (cell.Value == null)
                    {
                        importingResult.LogBookingValidationFailed($"Cell #{cellName}", "Vendor name is missing.");
                    }

                    // Check Vendor address
                    cellName = "C18";
                    cell = bookingFormSheet.Cells[cellName];
                    if (cell.Value != null)
                    {
                        var lines = cell.Value.ToString().Split("\n");
                        if (lines.Any(line => line.Length > 50))
                        {
                            importingResult.LogBookingValidationFailed($"Cell #{cellName}", "Vendor Address must be less than 50 characters for each line.");
                        }
                    }

                    // Check Expected Hub Arrival Date
                    cellName = "I18";
                    cell = bookingFormSheet.Cells[cellName];
                    if (cell.Value == null)
                    {
                        importingResult.LogBookingValidationFailed($"Cell #{cellName}", "Expected Hub Arrival Date is missing");
                    }
                    else
                    {
                        try
                        {
                            DateTime.FromOADate(long.Parse(cell.Value.ToString()));
                        }
                        catch
                        {
                            importingResult.LogBookingValidationFailed($"Cell #{cellName}", "Incorrect data format. Only date is allowed.");
                        }
                    }

                    // Check Actual Time Arrival (At Terminal)
                    cellName = "I19";
                    cell = bookingFormSheet.Cells[cellName];
                    if (cell.Value != null)
                    {
                        try
                        {
                            DateTime.FromOADate(long.Parse(cell.Value.ToString()));
                        }
                        catch
                        {
                            importingResult.LogBookingValidationFailed($"Cell #{cellName}", "Incorrect data format. Only date is allowed.");
                        }
                    }

                    // Check Container#
                    cellName = "I20";
                    cell = bookingFormSheet.Cells[cellName];
                    if (cell.Value == null)
                    {
                        importingResult.LogBookingValidationFailed($"Cell #{cellName}", "Container# is missing");
                    }

                    // Check ETD Origin
                    cellName = "C27";
                    cell = bookingFormSheet.Cells[cellName];
                    if (cell.Value != null)
                    {
                        try
                        {
                            DateTime.FromOADate(long.Parse(cell.Value.ToString()));
                        }
                        catch
                        {
                            importingResult.LogBookingValidationFailed($"Cell #{cellName}", "Incorrect data format. Only date is allowed.");
                        }
                    }

                    // Check ETA Destination
                    cellName = "I27";
                    cell = bookingFormSheet.Cells[cellName];
                    if (cell.Value != null)
                    {
                        try
                        {
                            DateTime.FromOADate(long.Parse(cell.Value.ToString()));
                        }
                        catch
                        {
                            importingResult.LogBookingValidationFailed($"Cell #{cellName}", "Incorrect data format. Only date is allowed.");
                        }
                    }

                    #endregion

                    #region Validate PO/Product level

                    // Find cell "Total" cell

                    string cellRange = "E29:E1000";

                    var totalCells = from findCell in bookingFormSheet.Cells[cellRange]
                                     where findCell.Value != null && findCell.Value.ToString().Equals("Total", StringComparison.InvariantCultureIgnoreCase)
                                     select findCell.Start.Row;
                    lastRowProduct = totalCells.First() - 1;
                    var listOfCustomerPONumber = new List<string>();

                    // Add calculation to count how many rows will be imported
                    cellRange = $"S29:S{lastRowProduct}";
                    bookingFormSheet.Cells[cellRange].FormulaR1C1 = "IF(OR(R[0]C[-17]<>\"\", R[0]C[-9]<>\"\",R[0]C[-8]<>\"\",R[0]C[-7]<>\"\"),1,0)";
                    bookingFormSheet.Workbook.Calculate();
                    var productCount = (from findCell in bookingFormSheet.Cells[cellRange]
                                        where findCell.GetValue<int>() == 1
                                        select findCell.FullAddress).Count();
                    // If there is no row, write failed log
                    if (productCount == 0)
                    {
                        importingResult.LogProductValidationFailed($"Products", "There is no product to import.");
                    }

                    for (int rowNumber = 29; rowNumber <= lastRowProduct; rowNumber++)
                    {
                        // Missing all 4 fields (PO#, No. of Piece, Pcs/Pair, No. of Ctn), skip the record 
                        if (bookingFormSheet.Cells[$"B{rowNumber}"].Value == null
                            && bookingFormSheet.Cells[$"J{rowNumber}"].Value == null
                            && bookingFormSheet.Cells[$"K{rowNumber}"].Value == null
                            && bookingFormSheet.Cells[$"L{rowNumber}"].Value == null)
                        {
                            continue;
                        }

                        // Required values
                        if (bookingFormSheet.Cells[$"B{rowNumber}"].Value == null)
                        {
                            importingResult.LogProductValidationFailed($"Cell #B{rowNumber}", "PO# is missing.");
                        }
                        else
                        {
                            listOfCustomerPONumber.Add(bookingFormSheet.Cells[$"B{rowNumber}"].Value.ToString());
                        }

                        if (bookingFormSheet.Cells[$"J{rowNumber}"].Value == null)
                        {
                            importingResult.LogProductValidationFailed($"Cell #J{rowNumber}", "No. of Piece is missing.");
                        }
                        if (bookingFormSheet.Cells[$"K{rowNumber}"].Value == null)
                        {
                            importingResult.LogProductValidationFailed($"Cell #K{rowNumber}", "(Pcs / Pair) is missing.");
                        }
                        else
                        {
                            var acceptedValues = new[] { "pcs", "pair" };
                            if (!acceptedValues.Contains(bookingFormSheet.Cells[$"K{rowNumber}"].Value.ToString().ToLowerInvariant()))
                            {
                                importingResult.LogProductValidationFailed($"Cell #K{rowNumber}", "Incorrect data format. Only accept Pcs / Pair.");
                            }
                        }
                        if (bookingFormSheet.Cells[$"L{rowNumber}"].Value == null)
                        {
                            importingResult.LogProductValidationFailed($"Cell #L{rowNumber}", "No. of Ctn is missing.");
                        }

                        int temp = 0;
                        if (bookingFormSheet.Cells[$"J{rowNumber}"].Value != null && !int.TryParse(bookingFormSheet.Cells[$"J{rowNumber}"].Value.ToString(), out temp))
                        {
                            importingResult.LogProductValidationFailed($"Cell #J{rowNumber}", "Incorrect data format. Only integer is allowed.");
                        }
                        if (bookingFormSheet.Cells[$"L{rowNumber}"].Value != null && !int.TryParse(bookingFormSheet.Cells[$"L{rowNumber}"].Value.ToString(), out temp))
                        {
                            importingResult.LogProductValidationFailed($"Cell #L{rowNumber}", "Incorrect data format. Only integer is allowed.");
                        }

                        // Optional values
                        decimal temp2 = 0;
                        var oCellValue = bookingFormSheet.Cells[$"O{rowNumber}"].Value;
                        var pCellValue = bookingFormSheet.Cells[$"P{rowNumber}"].Value;
                        var qCellValue = bookingFormSheet.Cells[$"Q{rowNumber}"].Value;

                        if (oCellValue != null 
                            &&  (!decimal.TryParse(oCellValue.ToString(), out temp2) 
                                || (oCellValue.ToString().IndexOf(".") >= 0 && oCellValue.ToString().Substring(oCellValue.ToString().IndexOf(".") + 1).Length > 2))
                        )
                        {
                            importingResult.LogProductValidationFailed($"Cell #O{rowNumber}", "Incorrect data format. Only accept 2 decimal places.");
                        }

                        if (pCellValue != null
                            && (!decimal.TryParse(pCellValue.ToString(), out temp2)
                                || (pCellValue.ToString().IndexOf(".") >= 0 && pCellValue.ToString().Substring(pCellValue.ToString().IndexOf(".") + 1).Length > 2))
                        )
                        {
                            importingResult.LogProductValidationFailed($"Cell #P{rowNumber}", "Incorrect data format. Only accept 2 decimal places.");
                        }

                        if (qCellValue != null
                            && (!decimal.TryParse(qCellValue.ToString(), out temp2)
                                || (qCellValue.ToString().IndexOf(".") >= 0 && qCellValue.ToString().Substring(qCellValue.ToString().IndexOf(".") + 1).Length > 2))
                        )
                        {
                            importingResult.LogProductValidationFailed($"Cell #Q{rowNumber}", "Incorrect data format. Only accept 2 decimal places.");
                        }
                    }

                    #endregion

                    if (!importingResult.Success)
                    {
                        return importingResult;
                    }

                    #region Mapping data to model

                    var model = new InputWarehouseFulfillmentViewModel();

                    try
                    {

                        model.CustomerPrefix = principalOrg.CustomerPrefix;
                        model.ExpectedDeliveryDate = DateTime.FromOADate(long.Parse(bookingFormSheet.Cells["I18"].Value.ToString()));
                        model.ActualTimeArrival = bookingFormSheet.Cells["I19"].Value != null ? DateTime.FromOADate(long.Parse(bookingFormSheet.Cells["I19"].Value.ToString())) : (DateTime?)null;
                        model.ContainerNo = bookingFormSheet.Cells["I20"].Value?.ToString();
                        model.HAWBNo = bookingFormSheet.Cells["I21"].Value?.ToString();
                        model.Incoterm = bookingFormSheet.Cells["I22"].Value != null ? Enum.Parse<IncotermType>(bookingFormSheet.Cells["I22"].Value.ToString(), true) : 0;
                        model.NameofInternationalAccount = bookingFormSheet.Cells["I23"].Value?.ToString();
                        model.Forwarder = bookingFormSheet.Cells["C24"].Value?.ToString();
                        model.CompanyNo = bookingFormSheet.Cells["C25"].Value?.ToString();
                        model.PlantNo = bookingFormSheet.Cells["C26"].Value?.ToString();
                        model.DeliveryMode = bookingFormSheet.Cells["I25"].Value?.ToString();
                        model.ShipFromName = bookingFormSheet.Cells["I26"].Value?.ToString();
                        model.ShipFrom = locations.FirstOrDefault(x => x.LocationDescription.Equals(model.ShipFromName, StringComparison.InvariantCultureIgnoreCase))?.Id ?? 0;
                        model.ETDOrigin = bookingFormSheet.Cells["C27"].Value != null ? DateTime.FromOADate(long.Parse(bookingFormSheet.Cells["C27"].Value.ToString())) : (DateTime?)null;
                        model.ETADestination = bookingFormSheet.Cells["I27"].Value != null ? DateTime.FromOADate(long.Parse(bookingFormSheet.Cells["I27"].Value.ToString())) : (DateTime?)null;
                        model.CreatedBy = importData.Owner;
                        model.CreatedDate = importData.CreatedDate.Value;
                    }
                    catch (Exception ex)
                    {
                        importingResult.LogBookingValidationFailed($"Booking", ex.Message);
                    }

                    // Add contacts
                    model.Contacts = new List<WarehouseFulfillmentContactViewModel>();

                    model.Contacts.Add(new WarehouseFulfillmentContactViewModel
                    {
                        OrganizationRole = OrganizationRole.Principal,
                        OrganizationId = principalOrg.Id,
                        CompanyName = principalOrg.Name,
                        Address = string.Join("\n", principalOrg.Address, principalOrg.AddressLine2, principalOrg.AddressLine3, principalOrg.AddressLine4),
                        ContactName = principalOrg.ContactName,
                        ContactEmail = principalOrg.ContactEmail,
                        ContactNumber = principalOrg.ContactNumber,
                        CreatedBy = importData.Owner,
                        CreatedDate = importData.CreatedDate.Value
                    });

                    var supplierName = bookingFormSheet.Cells["C17"].Value.ToString();
                    var supplierAddress = bookingFormSheet.Cells["C18"].Value?.ToString() ?? string.Empty;

                    model.Contacts.Add(new WarehouseFulfillmentContactViewModel
                    {
                        OrganizationRole = OrganizationRole.Supplier,
                        OrganizationId = 0,
                        CompanyName = supplierName,
                        Address = supplierAddress,
                        CreatedBy = importData.Owner,
                        CreatedDate = importData.CreatedDate.Value
                    });

                    var warehouseLocationProviders = _csfeApiClient.GetWarehouseProviderByOrgIdAsync(principalOrg.Id).Result;
                    if (warehouseLocationProviders != null && warehouseLocationProviders.Any())
                    {
                        var warehouseLocationProvider = warehouseLocationProviders.First();
                        model.Contacts.Add(new WarehouseFulfillmentContactViewModel
                        {
                            OrganizationRole = OrganizationRole.WarehouseProvider,
                            OrganizationId = warehouseLocationProvider.Id,
                            CompanyName = warehouseLocationProvider.Name,
                            Address = string.Join("\n", warehouseLocationProvider.Address, warehouseLocationProvider.AddressLine2, warehouseLocationProvider.AddressLine3, warehouseLocationProvider.AddressLine4),
                            ContactName = warehouseLocationProvider.ContactName,
                            ContactEmail = warehouseLocationProvider.ContactEmail,
                            ContactNumber = warehouseLocationProvider.ContactNumber,
                            CreatedBy = importData.Owner,
                            CreatedDate = importData.CreatedDate.Value
                        });
                    }

                    // Add orders
                    model.Orders = new List<WarehouseFulfillmentOrderViewModel>();
                    for (int rowNumber = 29; rowNumber <= lastRowProduct; rowNumber++)
                    {
                        if (bookingFormSheet.Cells[$"B{rowNumber}"].Value == null)
                        {
                            continue;
                        }

                        try
                        {

                            var newOrder = new WarehouseFulfillmentOrderViewModel
                            {
                                ShippingMarks = bookingFormSheet.Cells[$"A{rowNumber}"].Value?.ToString(),
                                CustomerPONumber = bookingFormSheet.Cells[$"B{rowNumber}"].Value.ToString(),
                                SeasonCode = bookingFormSheet.Cells[$"C{rowNumber}"].Value?.ToString(),
                                StyleNo = bookingFormSheet.Cells[$"D{rowNumber}"].Value?.ToString(),
                                ProductName = bookingFormSheet.Cells[$"F{rowNumber}"].Value?.ToString(),
                                ColourCode = bookingFormSheet.Cells[$"G{rowNumber}"].Value?.ToString(),
                                Size = bookingFormSheet.Cells[$"I{rowNumber}"].Value?.ToString(),
                                FulfillmentUnitQty = int.Parse(bookingFormSheet.Cells[$"J{rowNumber}"].Value.ToString()),
                                UnitUOM = bookingFormSheet.Cells[$"K{rowNumber}"].Value.ToString().Equals("Pcs", StringComparison.OrdinalIgnoreCase)
                                                                                    ? UnitUOMType.Piece
                                                                                    : (bookingFormSheet.Cells[$"K{rowNumber}"].Value.ToString().Equals("Pair", StringComparison.OrdinalIgnoreCase) ? UnitUOMType.Pair : 0),
                                BookedPackage = int.Parse(bookingFormSheet.Cells[$"L{rowNumber}"].Value.ToString()),
                                NetWeight = bookingFormSheet.Cells[$"M{rowNumber}"].Value != null ? decimal.Parse(bookingFormSheet.Cells[$"M{rowNumber}"].Value.ToString()) : (decimal?)null,
                                GrossWeight = bookingFormSheet.Cells[$"N{rowNumber}"].Value != null ? decimal.Parse(bookingFormSheet.Cells[$"N{rowNumber}"].Value.ToString()) : (decimal?)null,
                                Length = bookingFormSheet.Cells[$"O{rowNumber}"].Value != null ? decimal.Parse(bookingFormSheet.Cells[$"O{rowNumber}"].Value.ToString()) : (decimal?)null,
                                Width = bookingFormSheet.Cells[$"P{rowNumber}"].Value != null ? decimal.Parse(bookingFormSheet.Cells[$"P{rowNumber}"].Value.ToString()) : (decimal?)null,
                                Height = bookingFormSheet.Cells[$"Q{rowNumber}"].Value != null ? decimal.Parse(bookingFormSheet.Cells[$"Q{rowNumber}"].Value.ToString()) : (decimal?)null,
                                Volume = bookingFormSheet.Cells[$"R{rowNumber}"].Value != null ? decimal.Parse(bookingFormSheet.Cells[$"R{rowNumber}"].Value.ToString()) : (decimal?)null,
                                Status = POFulfillmentOrderStatus.Active
                            };
                            model.Orders.Add(newOrder);
                        }
                        catch (Exception ex)
                        {
                            importingResult.LogProductValidationFailed($"Product #{rowNumber}", ex.Message);
                        }
                    }

                    if (!model.Orders.Any())
                    {
                        importingResult.LogProductValidationFailed($"Products", "There is no product to import.");
                    }


                    #endregion

                    if (importingResult.Success)
                    {
                        model.BuyerCompliance = outputData.BuyerCompliance;
                        model.PrincipalOrganization = outputData.PrincipalOrganization;
                        outputData = model;

                    }
                }
            }
            return importingResult;
        }

        /// <summary>
        /// To store warehouse booking
        /// </summary>
        /// <param name="model"></param>
        /// <param name="bookingOwner"></param>
        /// <param name="emailSubject"></param>
        /// <returns></returns>
        private async Task<WarehouseFulfillmentViewModel> CreateWarehouseBookingAsync(InputWarehouseFulfillmentViewModel model, string bookingOwner, string emailSubject)
        {
            var poFulfillment = Mapper.Map<POFulfillmentModel>(model);
            poFulfillment.FulfillmentType = FulfillmentType.Warehouse;
            poFulfillment.EmailSubject = emailSubject;

            var customerOrgId = model.Contacts.Single(x => x.OrganizationRole == OrganizationRole.Principal).OrganizationId;
            var buyerCompliance = await _buyerComplianceService.GetByOrgIdAsync(customerOrgId);
            // Customer prefix must be available, try to fire request to CSFE Master data API if not
            if (string.IsNullOrEmpty(model.CustomerPrefix))
            {
                var principalContact = model.Contacts.First(x => x.OrganizationRole.Equals(OrganizationRole.Principal));
                var customerOrg = await _csfeApiClient.GetOrganizationByIdAsync(principalContact.OrganizationId);
                if (customerOrg == null || string.IsNullOrEmpty(customerOrg.CustomerPrefix))
                {
                    throw new ApplicationException("Customer prefix is not valid to create booking.");
                }
                model.CustomerPrefix = customerOrg.CustomerPrefix;
            }

            if (buyerCompliance != null)
            {
                if (buyerCompliance.ShippingCompliance?.AllowMultiplePOPerFulfillment == false)
                {
                    var isHasMultiplePO = model.Orders.Select(x => x.CustomerPONumber).Distinct().Count() > 1;

                    if (isHasMultiplePO)
                    {
                        throw new ApplicationException("Not allow multiple PO per Booking.");
                    }
                }
            }

            poFulfillment.Number = await GenerateWarehouseBookingNumber(model.CustomerPrefix, customerOrgId, DateTime.UtcNow);
            poFulfillment.Status = POFulfillmentStatus.Active;
            poFulfillment.Stage = POFulfillmentStage.Draft;
            poFulfillment.BookingDate = null;
            poFulfillment.ExpectedShipDate = null;
            poFulfillment.BookingDate = model.CreatedDate;
            poFulfillment.CargoReadyDate = model.ExpectedDeliveryDate;

            // Manually add audit data
            poFulfillment.UpdatedDate = poFulfillment.CreatedDate;
            poFulfillment.UpdatedBy = poFulfillment.CreatedBy;

            foreach (var item in poFulfillment.Contacts)
            {
                item.CreatedDate = poFulfillment.CreatedDate;
                item.CreatedBy = poFulfillment.CreatedBy;
                item.UpdatedDate = poFulfillment.CreatedDate;
                item.UpdatedBy = poFulfillment.CreatedBy;
            }

            foreach (var item in poFulfillment.Orders)
            {
                item.CreatedDate = poFulfillment.CreatedDate;
                item.CreatedBy = poFulfillment.CreatedBy;
                item.UpdatedDate = poFulfillment.CreatedDate;
                item.UpdatedBy = poFulfillment.CreatedBy;
            }

            await Repository.AddAsync(poFulfillment);

            await UnitOfWork.SaveChangesAsync();

            await LinkWarehouseBookingToPurchaseOrderSilentAsync(poFulfillment);

            var result = Mapper.Map<WarehouseFulfillmentViewModel>(poFulfillment);
            return result;
        }

        /// <summary>
        /// To attach uploaded files to warehouse booking
        /// </summary>
        /// <param name="warehouseFulfillment"></param>
        /// <param name="bookingOwner"></param>
        /// <param name="uploadFiles"></param>
        /// <returns></returns>
        private async Task AttachWarehouseBookingFormAsync(WarehouseFulfillmentViewModel warehouseFulfillment, string bookingOwner, List<IFormFile> uploadFiles)
        {
            foreach (var file in uploadFiles)
            {
                var path = "";
                var fileName = file.FileName;
                var fileBytes = file.GetAllBytes();

                // Upload file into storage
                using (var stream = new MemoryStream(fileBytes))
                {
                    path = await _blobStorage.PutBlobAsync("attachment", warehouseFulfillment.Number, stream);
                }

                var attachmentFileName = fileName;

                var transformedFileName = attachmentFileName.ToLowerInvariant().Replace(" ", string.Empty);

                var fileKeys = new[]
                {
                    "bookingform", "packinglist", "commercialinvoice", "coo"
                };

                var attachmentType = AttachmentType.OTHERS;
                foreach (var key in fileKeys)
                {
                    if (transformedFileName.Contains(key))
                    {
                        switch (key)
                        {
                            case "bookingform":
                                attachmentType = AttachmentType.BOOKING_FORM;
                                break;
                            case "packinglist":
                                attachmentType = AttachmentType.PACKING_LIST;
                                break;
                            case "commercialinvoice":
                                attachmentType = AttachmentType.COMMERCIAL_INVOICE;
                                break;
                            case "coo":
                                attachmentType = AttachmentType.CERTIFICATE_OF_ORIGIN;
                                break;

                        }
                    }
                }

                var attachment = new AttachmentViewModel()
                {
                    AttachmentType = attachmentType,
                    ReferenceNo = warehouseFulfillment.Number,
                    POFulfillmentId = warehouseFulfillment.Id,
                    Description = bookingOwner,
                    UploadedBy = AppConstant.SYSTEM_USERNAME,
                    UploadedDateTime = DateTime.UtcNow,
                    BlobId = path,
                    FileName = attachmentFileName
                };

                // call one by one as there is business logic as importing attachment
                await _attachmentService.ImportAttachmentAsync(attachment);
            }
        }

        /// <summary>
        /// To proceed further actions to warehouse booking with Buyer Compliance settings
        /// </summary>
        /// <param name="warehouseBookingId"></param>
        /// <param name="bookingOwner"></param>
        /// <returns></returns>
        private async Task ProceedWarehouseBookingAsync(long warehouseBookingId, string bookingOwner)
        {
            var poFulfillment = await Repository.GetAsync(x => x.Id == warehouseBookingId,
                null,
                x
                => x.Include(m => m.Contacts)
                    .Include(m => m.Orders));

            if (poFulfillment == null || poFulfillment.Stage != POFulfillmentStage.Draft)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }
            var customer = poFulfillment.Contacts.Where(x => x.OrganizationRole == OrganizationRole.Principal).First();
            var supplier = poFulfillment.Contacts.Where(x => x.OrganizationRole == OrganizationRole.Supplier).First();
            var buyerCompliance = await _buyerComplianceService.GetByOrgIdAsync(customer.OrganizationId);

            var matchedServiceTypes = new BuyerComplianceServiceType[]
            {
                BuyerComplianceServiceType.WareHouse,
                BuyerComplianceServiceType.WareHouseFreight,
                BuyerComplianceServiceType.FreightWareHouse
            };

            if (buyerCompliance == null || !matchedServiceTypes.Contains(buyerCompliance.ServiceType))
            {
                return;
            }

            var policyAction = ValidationResultType.BookingAccepted;
            switch (buyerCompliance.BookingPolicyAction)
            {
                case ValidationResultType.PendingForApproval:
                    policyAction = ValidationResultType.PendingForApproval;
                    break;
                case ValidationResultType.BookingAccepted:
                    policyAction = ValidationResultType.BookingAccepted;
                    break;
                case ValidationResultType.WarehouseApproval:

                    var bookingOwnerDomain = bookingOwner.Split('@')[1];
                    List<string> bypassEmailDomains = new();
                    if (!string.IsNullOrWhiteSpace(buyerCompliance.BypassEmailDomain))
                    {
                        bypassEmailDomains = buyerCompliance.BypassEmailDomain.Split(",").Select(x => x.Trim())
                            .Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    }

                    var isMatched = bypassEmailDomains.Contains(bookingOwnerDomain, StringComparer.OrdinalIgnoreCase);
                    if (isMatched)
                    {
                        policyAction = ValidationResultType.BookingAccepted;
                    }
                    else
                    {
                        policyAction = ValidationResultType.PendingForApproval;
                    }
                    break;
            }

            switch (policyAction)
            {
                case ValidationResultType.PendingForApproval:
                    poFulfillment.IsRejected = false;
                    poFulfillment.Stage = POFulfillmentStage.ForwarderBookingRequest;
                    var buyerApprovalViewModel = new BuyerApprovalViewModel()
                    {
                        POFulfillmentId = poFulfillment.Id,
                        Reference = "",
                        Owner = "",
                        CreatedDate = DateTime.UtcNow,
                        ExceptionType = ExceptionType.POFulfillmentException,
                        Customer = customer.CompanyName,
                        ApproverSetting = buyerCompliance.BookingApproverSetting,
                        RequestByOrganization = "",
                        Requestor = poFulfillment.CreatedBy,
                        ExceptionActivity = "Booking Approval Request",
                        ActivityDate = DateTime.UtcNow,
                        AlertNotificationFrequencyType = buyerCompliance.ApprovalAlertFrequency,
                        Severity = "",
                        ExceptionDetail = "",
                        Stage = BuyerApprovalStage.Pending,
                        Status = BuyerApprovalStatus.Active,
                        Transaction = CommonHelper.GenerateGlobalId(poFulfillment.Id, EntityType.POFullfillment),
                        Reason = ""
                    };

                    var dueOnDate = DateTime.UtcNow.AddHours((int)buyerCompliance.ApprovalDuration);

                    if (DateTime.Compare(dueOnDate, DateTime.UtcNow) > 0)
                    {
                        buyerApprovalViewModel.DueOnDate = dueOnDate;
                    }

                    if (buyerApprovalViewModel.ApproverSetting == ApproverSettingType.AnyoneInOrganization)
                    {
                        buyerApprovalViewModel.ApproverOrgId = buyerCompliance.OrganizationId;
                    }
                    else
                    {
                        buyerApprovalViewModel.ApproverUser = buyerCompliance.BookingApproverUser;
                    }

                    var pendingApproval = await BuyerApprovalService.CreateAsync(buyerApprovalViewModel);

                    var event1051 = new ActivityViewModel()
                    {
                        ActivityCode = Core.Models.Event.EVENT_1051,
                        POFulfillmentId = poFulfillment.Id,
                        ActivityDate = DateTime.UtcNow,
                        CreatedBy = bookingOwner
                    };
                    await _activityService.TriggerAnEvent(event1051);

                    var event1055PFA = new ActivityViewModel()
                    {
                        ActivityCode = Core.Models.Event.EVENT_1055,
                        POFulfillmentId = poFulfillment.Id,
                        ActivityDate = DateTime.UtcNow,
                        CreatedBy = AppConstant.SYSTEM_USERNAME
                    };
                    await _activityService.TriggerAnEvent(event1055PFA);

                    var purchaseOrderListId = poFulfillment.Orders.Select(x => x.PurchaseOrderId).ToList();
                    var purchaseOrderList = await _purchaseOrderRepository.Query(x => purchaseOrderListId.Any(y => x.Id == y)).ToListAsync();
                    foreach (var singlePO in purchaseOrderList)
                    {
                        if (singlePO.Stage == POStageType.Released)
                        {
                            singlePO.Stage = POStageType.ForwarderBookingRequest;
                        }

                    }
                    _purchaseOrderRepository.UpdateRange(purchaseOrderList.ToArray());

                    _queuedBackgroundJobs.Enqueue<ApprovalNotificationEmailToApprover>(j => j.ExecuteAsync(pendingApproval.Id, (int)buyerCompliance.ApprovalAlertFrequency));
                    if (buyerCompliance.ApprovalDuration != ApprovalDurationType.NoExpiration)
                    {
                        _queuedBackgroundJobs.Schedule<TriggerRejectedOnOverduePendingApproval>(j => j.ExecuteAsync(pendingApproval.POFulfillmentId.Value, pendingApproval.Id), TimeSpan.FromHours((int)buyerCompliance.ApprovalDuration));
                    }
                    break;

                case ValidationResultType.BookingAccepted:

                    poFulfillment.IsRejected = false;
                    poFulfillment.Stage = POFulfillmentStage.ForwarderBookingRequest;

                    var event1051BA = new ActivityViewModel()
                    {
                        ActivityCode = Core.Models.Event.EVENT_1051,
                        POFulfillmentId = poFulfillment.Id,
                        ActivityDate = DateTime.UtcNow,
                        CreatedBy = bookingOwner
                    };
                    await _activityService.TriggerAnEvent(event1051BA);

                    var event1053 = new ActivityViewModel()
                    {
                        ActivityCode = Core.Models.Event.EVENT_1053,
                        POFulfillmentId = poFulfillment.Id,
                        ActivityDate = DateTime.UtcNow,
                        CreatedBy = AppConstant.SYSTEM_USERNAME
                    };
                    await _activityService.TriggerAnEvent(event1053);
                    break;
            }

            Repository.Update(poFulfillment);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// To generate number for warehouse booking
        /// </summary>
        /// <param name="customerPrefix"></param>
        /// <param name="createdDate"></param>
        /// <returns></returns>
        private async Task<string> GenerateWarehouseBookingNumber(string customerPrefix, long customerOrgId, DateTime createdDate)
        {
            if (string.IsNullOrEmpty(customerPrefix))
            {
                throw new ApplicationException("Customer prefix is not valid to create booking!");
            }
            var nextSequenceValue = await _poFulfillmentRepository.GetNextPOFFSequenceValueAsync(customerOrgId);
            return $"{customerPrefix}{createdDate.ToString("yyMM")}{nextSequenceValue}";
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
        private async Task<bool> ProceedAttachmentsAsync(List<WarehouseFulfillmentAttachmentViewModel> storingAttachments, long pofulfillmentId, IdentityInfo currentUser)
        {
            if (storingAttachments == null || !storingAttachments.Any())
            {
                return false;
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
                    // No change then continue
                    if (existingAttachmentViewModel.State != WarehouseFulfillmentAttachmentState.Edited)
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
            var newAttachmentViewModels = storingAttachments.Where(x => x.State == WarehouseFulfillmentAttachmentState.Added).ToList();
            isNewShippingDocuments = isNewShippingDocuments || (newAttachmentViewModels != null && newAttachmentViewModels.Any());
            foreach (var viewModel in newAttachmentViewModels)
            {
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

        /// <summary>
        /// Get cargo receive information for warehouse booking
        /// </summary>
        /// <param name="warehouseBookingNumber"></param>
        /// <returns></returns>
        public async Task<WarehouseCargoReceiveMobileModel> GetWarehouseCargoReceiveAsync(string warehouseBookingNumber)
        {
            var sql = @"SELECT WH.Stage, WHL.LocationName, CR.InDate
                        FROM POFulfillments WH

                        CROSS APPLY
                        (
	                        SELECT TOP(1) WHC.OrganizationId
	                        FROM POFulfillmentContacts WHC
	                        WHERE WHC.POFulfillmentId = WH.Id AND WHC.OrganizationRole = 'Principal'
                        ) PRIN

                        OUTER APPLY
                        (
	                        SELECT TOP(1) 
		                        WL.Name AS LocationName
                            FROM WarehouseAssignments WA 
	                        INNER JOIN WarehouseLocations WL ON WA.WarehouseLocationId = WL.Id
	                        WHERE WA.OrganizationId = PRIN.OrganizationId

                        ) WHL

                        OUTER APPLY
                        (
	                        SELECT TOP(1) 
		                        POFFCRI.InDate
                            FROM POFulfillmentOrders POFFO
	                        INNER JOIN POFulfillmentCargoReceiveItems POFFCRI ON POFFO.Id = POFFCRI.POFulfillmentOrderId
	                        WHERE POFFO.POFulfillmentId = WH.Id

                        ) CR

                        WHERE WH.FulfillmentType = 3 AND WH.Number = @bookingNumber";
            var filterParameters = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@bookingNumber",
                    Value = warehouseBookingNumber,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                }
            };

            WarehouseCargoReceiveMobileModel mappingCallback(DbDataReader reader)
            { 
                while (reader.Read())
                {
                    var stage = reader[0];
                    var location = reader[1];
                    var cargoReceivedDate = reader[2];
                    var data = new WarehouseCargoReceiveMobileModel
                    {
                        Stage = (int)stage,
                        Location = location != DBNull.Value ? (string)location : null,
                        CargoReceivedDate = cargoReceivedDate != DBNull.Value ? DateTime.Parse(cargoReceivedDate.ToString()) : null
                    };
                    return data;
                }

                return null;
            }
            var result = _dataQuery.GetDataBySql(sql, mappingCallback, filterParameters.ToArray());
            return result;

        }

        /// <summary>
        /// To link Warehouse Booking to Purchase Orders
        /// </summary>
        /// <remarks>It is silent method, try-catch inside</remarks>
        /// <param name="pos"></param>
        /// <returns></returns>
        private async Task LinkWarehouseBookingToPurchaseOrderSilentAsync(POFulfillmentModel warehouseBooking)
        {
            try
            {
                var customerPONumbers = warehouseBooking.Orders.Select(x => x.CustomerPONumber);
                var principalOrganizationId = warehouseBooking.Contacts.First(x => x.OrganizationRole == OrganizationRole.Principal).OrganizationId;
                var matchedPurchaseOrders = await _purchaseOrderRepository.Query(x => customerPONumbers.Contains(x.PONumber) && x.Contacts.Any(y => y.OrganizationId == principalOrganizationId && y.OrganizationRole == OrganizationRole.Principal)).ToListAsync();
                var lineItems = await _purchaseOrderLineItemRepository.Query(x => matchedPurchaseOrders.Select(y => y.Id).Contains(x.PurchaseOrderId)).ToListAsync();
                foreach (var purchaseOrder in matchedPurchaseOrders)
                {
                    if (purchaseOrder.Stage == POStageType.Released)
                    {
                        purchaseOrder.Stage = POStageType.ForwarderBookingRequest;
                    }
                }

                foreach (var pofulfillmentOrder in warehouseBooking.Orders)
                {
                    // Matched with PO
                    var matchedPurchaseOrder = matchedPurchaseOrders.FirstOrDefault(y => y.PONumber == pofulfillmentOrder.CustomerPONumber);

                    pofulfillmentOrder.PurchaseOrderId = matchedPurchaseOrder?.Id ?? 0;

                    var matchedPOlineItem = lineItems

                                            .Where(x => !(
                                                    // Ignore 3 keys = NULL
                                                    (x.StyleNo ?? string.Empty) == string.Empty
                                                    && (x.ColourCode ?? string.Empty) == string.Empty
                                                    && (x.Size ?? string.Empty) == string.Empty
                                                )
                                            )

                                            .FirstOrDefault(x =>
                                                // Matched with 3 keys, NULL = empty/blank string
                                                (x.StyleNo ?? string.Empty).Equals(pofulfillmentOrder.StyleNo ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
                                                && (x.ColourCode ?? string.Empty).Equals(pofulfillmentOrder.ColourCode ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
                                                && (x.Size ?? string.Empty).Equals(pofulfillmentOrder.Size ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)

                                                // Matched with PO
                                                && x.PurchaseOrderId == matchedPurchaseOrder?.Id
                                           );
                    if (matchedPOlineItem != null)
                    {
                        pofulfillmentOrder.POLineItemId = matchedPOlineItem.Id;
                        pofulfillmentOrder.ProductCode = matchedPOlineItem.ProductCode;
                        pofulfillmentOrder.OrderedUnitQty = matchedPOlineItem.OrderedUnitQty;
                        pofulfillmentOrder.BalanceUnitQty = pofulfillmentOrder.OrderedUnitQty - pofulfillmentOrder.FulfillmentUnitQty;

                        matchedPOlineItem.BookedUnitQty += pofulfillmentOrder.FulfillmentUnitQty;
                        matchedPOlineItem.BalanceUnitQty -= pofulfillmentOrder.FulfillmentUnitQty;
                    }
                }
                await UnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
        }

        private async Task WriteIntegrationLogAsync(ImportingWarehouseBookingResultViewModel importingResult, ImportWarehouseBookingViewModel importData, string profile)
        {
            // Must create new instance of DB context to save integration log isolatedly
            // to prevent issue on other entities
            using (var uow = UnitOfWorkProvider.CreateUnitOfWorkForBackgroundJob())
            {
                var logModel = new IntegrationLogModel
                {
                    APIName = "Import Warehouse Booking",
                    APIMessage = $"Owner: {importData.Owner} " +
                        $"\nCreatedDate: {importData.CreatedDate} " +
                        $"\nCustomer: {importData.Customer}" +
                        $"\nEmailSubject: {importData.EmailSubject}" +
                        $"\nBookingForm: {importData.BookingForm?.FileName}",
                    EDIMessageRef = string.Empty,
                    EDIMessageType = string.Empty,
                    PostingDate = DateTime.UtcNow,
                    Profile = profile,
                    Status = importingResult.Success ? IntegrationStatus.Succeed : IntegrationStatus.Failed,
                    Remark = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} GMT",
                    Response = JsonConvert.SerializeObject(importingResult, Formatting.Indented)
                };
                logModel.Audit();
                var integrationLogRepository = uow.GetRepository<IntegrationLogModel>();
                await integrationLogRepository.AddAsync(logModel);
                await uow.SaveChangesAsync();
            }
        }

        /// <summary>
        /// To standardize/distinct list of recipient emails
        /// </summary>
        /// <param name="emailSettings"></param>
        /// <param name="type"></param>
        /// <param name="defaultEmailSendTo"></param>
        /// <returns> Dictionary with keys "sendTo" and "cc"
        /// <code>
        /// Dictionary {
        ///     "sendTo": "email1.sample@domain1.com, email2.sample@domain2.com, email3.sample@domain3.com",
        ///     "cc": "email4.sample@domain4.com"
        /// }
        /// </code>
        /// </returns>
        private Dictionary<string, string> GetReceipientsFromBuyerCompliance(List<EmailSettingModel> emailSettings, EmailSettingType type, string defaultEmailSendTo)
        {
            // get recipients as booking import failed
            var recipients = emailSettings.FirstOrDefault(x => x.EmailType == type);

            // if recipients = null or send to default = true, send email booking owner
            var mailTo = (recipients == null || recipients?.DefaultSendTo == true) ? defaultEmailSendTo : "";
            if (!string.IsNullOrEmpty(recipients?.SendTo))
            {
                mailTo += (!string.IsNullOrEmpty(mailTo) ? ", " : "") + recipients?.SendTo;
            }
            var mailCC = recipients?.CC ?? "";

            return new Dictionary<string, string>
            {
                {"sendTo", string.Join(", ", mailTo.Replace(" ","").Replace(",", ";").Split(";", StringSplitOptions.RemoveEmptyEntries).Distinct()) },
                {"cc", string.Join(", ", mailCC.Replace(" ","").Replace(",", ";").Split(";", StringSplitOptions.RemoveEmptyEntries).Distinct())}
            };
        }
    }


}