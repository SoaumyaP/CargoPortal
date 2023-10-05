using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.Application.BuyerComplianceService.Services.Interfaces;
using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.Excel;
using Groove.SP.Infrastructure.CSFE;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OfficeOpenXml;
using Groove.SP.Application.PurchaseOrders.Validations;
using Groove.SP.Application.Utilities;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.Users.Services.Interfaces;
using Microsoft.Extensions.Options;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Core.Data;
using System.Data.SqlClient;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using Groove.SP.Application.PurchaseOrderContact.ViewModels;
using Groove.SP.Application.Shipments.ViewModels;

using Groove.SP.Application.ApplicationBackgroundJob;
using Groove.SP.Application.ApplicationBackgroundJob.Services;
using Groove.SP.Application.OrganizationPreference.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Activity.Services.Interfaces;
using OrganizationRoleConstant = Groove.SP.Core.Models.OrganizationRole;
using Groove.SP.Application.Notification.Interfaces;
using Groove.SP.Application.Notification.ViewModel;
using Groove.SP.Application.POFulfillment.Services;
using Groove.SP.Application.PurchaseOrders.BackgroundJobs;
using Hangfire;

namespace Groove.SP.Application.PurchaseOrders.Services
{
    public partial class PurchaseOrderService : ServiceBase<PurchaseOrderModel, PurchaseOrderViewModel>, IPurchaseOrderService
    {

        #region Fields
        private const string EXCEL_POHEADER_SHEET_NAME = "PO Header";
        private const string EXCEL_POPRODUCT_SHEET_NAME = "PO Product";
        private const string EXCEL_POCONTACT_SHEET_NAME = "PO Contact";

        private const string EXCEL_POHEADER_SHEET_NAME_TRANSLATE = "label.sheetName.poHeader";
        private const string EXCEL_POPRODUCT_SHEET_NAME_TRANSLATE = "label.sheetName.poProduct";
        private const string EXCEL_POCONTACT_SHEET_NAME_TRANSLATE = "label.sheetName.poContact";
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IBuyerComplianceService _buyerComplianceService;
        private readonly IPOFulfillmentService _poFulfillmentService;
        private readonly IUserProfileService _userProfileService;
        private readonly AppConfig _appConfig;
        private readonly CustomerOrgReference _customerOrgReference;
        private readonly IPOFulfillmentRepository _poFulfillmentRepository;
        private readonly IRepository<NoteModel> _noteRepository;
        private readonly IPOFulfillmentAllocatedOrderRepository _poFulfillmentAllocatedOrderRepository;
        private readonly IDataQuery _dataQuery;
        private readonly ILogger<PurchaseOrderService> _logger;
        private readonly IQueuedBackgroundJobs _queuedBackgroundJobs;
        private readonly IOrganizationPreferenceService _organizationPreferenceService;
        private readonly IActivityService _activityService;
        private readonly INotificationService _notificationService;
        private readonly IActivityRepository _activityRepository;
        private readonly IGlobalIdActivityRepository _globalIdActivityRepository;
        private readonly ICargoDetailRepository _cargoDetailRepository;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IRepository<ConsignmentItineraryModel> _consignmentItineraryRepository;
        private readonly IPOFulfillmentOrderRepository _poFulfillmentOrderRepository;

        #endregion

        #region ctors
        public PurchaseOrderService(IUnitOfWorkProvider unitOfWorkProvider,
            ICSFEApiClient csfeApiClient,
            IBuyerComplianceService buyerComplianceService,
            IPOFulfillmentService poFulfillmentService,
            IUserProfileService userProfileService,
            IOptions<AppConfig> appConfig,
            IOptions<CustomerOrgReference> customerOrgReference,
            IPOFulfillmentRepository poFulfillmentRepository,
            IRepository<NoteModel> noteRepository,
            IPOFulfillmentAllocatedOrderRepository poFulfillmentAllocatedOrderRepository,
            IDataQuery dataQuery,
            ILogger<PurchaseOrderService> logger,
            IQueuedBackgroundJobs queuedBackgroundJobs,
            IOrganizationPreferenceService organizationPreferenceService,
            IActivityService activityService,
            INotificationService notificationService,
            IRepository<ShipmentModel> shipmentRepository,
            IRepository<ConsignmentItineraryModel> consignmentItineraryRepository,
            IPOFulfillmentOrderRepository poFulfillmentOrderRepository
            ) : base(unitOfWorkProvider)
        {
            _csfeApiClient = csfeApiClient;
            _buyerComplianceService = buyerComplianceService;
            _poFulfillmentService = poFulfillmentService;
            _userProfileService = userProfileService;
            _appConfig = appConfig.Value;
            _customerOrgReference = customerOrgReference.Value;
            _poFulfillmentRepository = poFulfillmentRepository;
            _noteRepository = noteRepository;
            _poFulfillmentAllocatedOrderRepository = poFulfillmentAllocatedOrderRepository;
            _dataQuery = dataQuery;
            _logger = logger;
            _queuedBackgroundJobs = queuedBackgroundJobs;
            _organizationPreferenceService = organizationPreferenceService;
            _activityService = activityService;
            _notificationService = notificationService;
            _shipmentRepository = (IShipmentRepository)shipmentRepository;
            _activityRepository = (IActivityRepository)UnitOfWork.GetRepository<ActivityModel>();
            _globalIdActivityRepository = (IGlobalIdActivityRepository)UnitOfWork.GetRepository<GlobalIdActivityModel>();
            _cargoDetailRepository = (ICargoDetailRepository)UnitOfWork.GetRepository<CargoDetailModel>();
            _consignmentItineraryRepository = consignmentItineraryRepository;
            _poFulfillmentOrderRepository = poFulfillmentOrderRepository;
        }
        #endregion

        private async Task PopulatePOContacts(PurchaseOrderModel po)
        {
            var codes = po.Contacts.Select(c => c.OrganizationCode).Distinct();
            var ogranizations = await _csfeApiClient.GetOrganizationsByCodesAsync(codes);
            foreach (var contact in po.Contacts)
            {
                var organization = ogranizations.SingleOrDefault(o => o.Code.Trim().Equals(contact.OrganizationCode));

                if (organization != null)
                {
                    contact.OrganizationId = organization.Id;
                    contact.CompanyName = organization.Name;
                    contact.AddressLine1 = organization.Address;
                    contact.AddressLine2 = organization.AddressLine2;
                    contact.AddressLine3 = organization.AddressLine3;
                    contact.AddressLine4 = organization.AddressLine4;
                    contact.ContactName = organization.ContactName;
                    contact.ContactNumber = organization.ContactNumber;
                    contact.ContactEmail = organization.ContactEmail;
                    // This line code will make sure the Role is always UPPERCASE at first, unless it will cause some issues.
                    contact.OrganizationRole = StringHelper.FirstCharToUpperCase(contact.OrganizationRole);
                }
            }
        }

        private async Task<IEnumerable<PurchaseOrderModel>> PopulateWarehouseProviderContactAsync(List<PurchaseOrderModel> pos)
        {
            var principalOrgIds = pos.SelectMany(x => x.Contacts.Where(y => y.OrganizationRole == OrganizationRole.Principal && y.OrganizationId != 0).Select(y => y.OrganizationId))?.ToList();
            var compliances = (await _buyerComplianceService.GetListByOrgIdsAsync(principalOrgIds)).ToList();

            // List of PO whose customer is using warehouse services.
            var result = new List<PurchaseOrderModel>();

            foreach (var po in pos)
            {
                var principal = po.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Principal);
                if (principal != null)
                {
                    var compliance = compliances.FirstOrDefault(x => x.OrganizationId == principal.OrganizationId);
                    if (compliance?.ServiceType == BuyerComplianceServiceType.WareHouse)
                    {
                        var warehouseProvider = (await _csfeApiClient.GetWarehouseProviderByOrgIdAsync(compliance.OrganizationId)).FirstOrDefault();
                        if (warehouseProvider != null)
                        {
                            // if Warehouse Provider contact already exists => delete first then create new
                            po.Contacts = po.Contacts.Where(x => x.OrganizationRole != OrganizationRole.WarehouseProvider).ToList();

                            var newContact = new PurchaseOrderContactModel()
                            {
                                OrganizationRole = OrganizationRole.WarehouseProvider,
                                OrganizationId = warehouseProvider.Id,
                                OrganizationCode = warehouseProvider.Code,
                                CompanyName = warehouseProvider.Name,
                                AddressLine1 = warehouseProvider.Address,
                                AddressLine2 = warehouseProvider.AddressLine2,
                                AddressLine3 = warehouseProvider.AddressLine3,
                                AddressLine4 = warehouseProvider.AddressLine4,
                                ContactName = warehouseProvider.ContactName,
                                ContactEmail = warehouseProvider.ContactEmail,
                                ContactNumber = warehouseProvider.ContactNumber,
                                PurchaseOrderId = po.Id
                            };
                            newContact.Audit(po.UpdatedBy);
                            po.Contacts.Add(newContact);
                        }
                        result.Add(po);
                    }
                }
            }
            return result;
        }

        private async Task LinkPOsToWarehouseBookingSilentAsync(List<PurchaseOrderModel> pos)
        {
            try
            {
                var poNumbers = pos.Select(x => x.PONumber).ToList();
                var poffs = await _poFulfillmentRepository.Query(x => x.Orders.Any(y => poNumbers.Contains(y.CustomerPONumber) && (y.PurchaseOrderId == 0 || y.POLineItemId == 0)) && x.FulfillmentType == FulfillmentType.Warehouse,
                    null,
                    x => x.Include(y => y.Contacts)).ToListAsync();

                foreach (var poff in poffs)
                {
                    var poffPrincipalOrg = poff.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Principal);
                    if (poffPrincipalOrg != null)
                    {
                        foreach (var poffOrder in poff.Orders)
                        {
                            var po = pos.FirstOrDefault(x => x.PONumber == poffOrder.CustomerPONumber && x.Contacts.Any(c => c.OrganizationRole == OrganizationRole.Principal && c.OrganizationId == poffPrincipalOrg.OrganizationId));
                            if (po != null)
                            {
                                if (poffOrder.PurchaseOrderId == 0)
                                {
                                    poffOrder.PurchaseOrderId = po.Id;
                                    // Set PO Stage = booking stage
                                    switch (poff.Stage)
                                    {
                                        case POFulfillmentStage.Draft:
                                            po.Stage = POStageType.Released;
                                            break;
                                        case POFulfillmentStage.ForwarderBookingRequest:
                                            po.Stage = POStageType.ForwarderBookingRequest;
                                            break;
                                        case POFulfillmentStage.ForwarderBookingConfirmed:
                                            po.Stage = POStageType.ForwarderBookingConfirmed;
                                            break;
                                        case POFulfillmentStage.CargoReceived:
                                            po.Stage = POStageType.CargoReceived;
                                            break;
                                        case POFulfillmentStage.ShipmentDispatch:
                                            po.Stage = POStageType.ShipmentDispatch;
                                            break;
                                        case POFulfillmentStage.Closed:
                                            po.Stage = POStageType.Closed;
                                            break;
                                        default:
                                            break;
                                    }
                                }


                                var poLineItem = po.LineItems
                                    .Where(x =>
                                        // Ignore 3 keys = NULL
                                        (x.StyleNo ?? string.Empty) != string.Empty ||
                                        (x.ColourCode ?? string.Empty) != string.Empty ||
                                        (x.Size ?? string.Empty) != string.Empty
                                    )
                                    .FirstOrDefault(x =>
                                        // Matched with 3 keys, NULL = empty/blank string
                                        (x.StyleNo ?? string.Empty).Equals(poffOrder.StyleNo ?? string.Empty, StringComparison.InvariantCultureIgnoreCase) &&
                                        (x.ColourCode ?? string.Empty).Equals(poffOrder.ColourCode ?? string.Empty, StringComparison.InvariantCultureIgnoreCase) &&
                                        (x.Size ?? string.Empty).Equals(poffOrder.Size ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
                                    );

                                if (poLineItem != null)
                                {
                                    poffOrder.POLineItemId = poLineItem.Id;
                                    poffOrder.ProductCode = poLineItem.ProductCode;
                                    poffOrder.OrderedUnitQty = poLineItem.OrderedUnitQty;
                                    poffOrder.BalanceUnitQty = poLineItem.OrderedUnitQty - poffOrder.FulfillmentUnitQty;

                                    // Update balance qty for POLineItems.
                                    poLineItem.BookedUnitQty += poffOrder.FulfillmentUnitQty;
                                    poLineItem.BalanceUnitQty = poLineItem.OrderedUnitQty - poLineItem.BookedUnitQty;
                                }
                            }
                        }
                    }
                }
                await UnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
        }

        protected override Func<IQueryable<PurchaseOrderModel>, IQueryable<PurchaseOrderModel>> FullIncludeProperties => x => x.Include(m => m.Contacts).Include(m => m.LineItems).Include(m => m.BlanketPO);

        protected override IDictionary<string, string> SortMap => new Dictionary<string, string>() {
            { "statusName", "status" },
            { "stageName", "stage" }
        };

        public async Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, string affiliates)
        {
            var listOfAffiliates = new List<long>();

            if (request.Filters == null)
            {
                request.Filters = new FilterDescriptorCollection();
            }

            DataSourceResult result = null;

            if (isInternal)
            {
                result = await GetListAsync(request, FullIncludeProperties, null);
            }
            else
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }

                result = await GetListAsync(request, FullIncludeProperties, o => o.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId)));
            }

            return result;
        }

        public async Task<PurchaseOrderViewModel> GetAsync(long id, IdentityInfo currentUser, string affiliates, string supplierCustomerRelationships = "", long? delegatedOrganizationId = 0, bool? replacedByOrganizationReferences = false)
        {
            var isInternal = currentUser.IsInternal;
            var listOfAffiliates = new List<long>();
            Func<IQueryable<PurchaseOrderModel>, IQueryable<PurchaseOrderModel>> includeProperties = x
            => x.Include(m => m.Contacts)
                .Include(m => m.LineItems)
                .Include(m => m.AllocatedPOs)
                .ThenInclude(m => m.LineItems)
                .Include(m => m.BlanketPO);
            var query = Repository.GetListQueryable(includeProperties).Where(p => p.Id == id);
            var isAccesible = true;

            if (!isInternal)
            {
                listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);

                // user role = shipper
                if (!string.IsNullOrEmpty(supplierCustomerRelationships))
                {
                    var sql = @"
                        SET @result = 0;
                        SELECT @result = CASE WHEN t.Id > 0 THEN 1 END
                        FROM
                        (
	                        SELECT t1.*, t2.*, t3.*
	                        FROM
	                        (
		                        SELECT po.Id, po.PONumber, po.POIssueDate, po.Status, po.Stage, po.CargoReadyDate, po.CreatedDate, po.CreatedBy
		                        FROM PurchaseOrders po
                                WHERE po.Id = @poId
	                        ) t1
	                        OUTER APPLY
	                        (
				                        SELECT TOP(1) sc.OrganizationId AS SupplierId
				                        FROM PurchaseOrderContacts sc
				                        WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
	                        ) t2
	                        OUTER APPLY
	                        (
				                        SELECT TOP(1) sc.OrganizationId AS CustomerId
				                        FROM PurchaseOrderContacts sc
				                        WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Principal'
	                        ) t3

                        ) t
                        WHERE 
                            EXISTS (
                                    SELECT pc.PurchaseOrderId FROM PurchaseOrderContacts pc
                                    WHERE t.Id = pc.PurchaseOrderId AND pc.OrganizationRole = 'Delegation' AND pc.OrganizationId = @delegatedOrganizationId)
	                        OR
                            CAST(t.SupplierID AS NVARCHAR(20)) + ','+ CAST(t.CustomerId AS NVARCHAR(20)) IN (SELECT tmp.[Value] FROM dbo.fn_SplitStringToTable(@supplierCustomerRelationships, ';') tmp )
                    ";

                    // Add filter parameter
                    var filterParameters = new List<SqlParameter>
                    {
                        new SqlParameter
                        {
                            ParameterName = "@poId",
                            Value = id,
                            DbType = DbType.Int64,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@delegatedOrganizationId",
                            Value = delegatedOrganizationId,
                            DbType = DbType.Int64,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@supplierCustomerRelationships",
                            Value = supplierCustomerRelationships,
                            DbType = DbType.String,
                            Direction = ParameterDirection.Input
                        }
                    };

                    isAccesible = _dataQuery.GetValueFromVariable(sql, filterParameters.ToArray()).Equals("1");

                }
                // user role = agent/ principal
                else
                {
                    query = query.Where(s => s.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId)));
                }
            }

            var model = isAccesible ? await query.FirstOrDefaultAsync() : null;

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the key {string.Join(", ", id)} not found!");
            }

            model.AllocatedPOs = model.AllocatedPOs?.Where(a => a.Status == PurchaseOrderStatus.Active).ToList();

            var orgId = GetPrincipalOrgIdFromAPIModel(model.Contacts);
            var compliance = (await _buyerComplianceService.GetListByOrgIdsAsync(new List<long> { orgId })).FirstOrDefault();

            var result = Mapper.Map<PurchaseOrderViewModel>(model);

            //For case PO linked with shipment via CargoDetail
            var allBookings = new List<SummaryPOFulfillmentViewModel>();

            // Obtains more data from linked POFF, Shipment based on some business rules
            if (model.POType == POType.Bulk || compliance.AllowToBookIn == model.POType)
            {
                var fulfillments = await _poFulfillmentRepository.Query(f => f.Orders.Any(o => o.PurchaseOrderId == model.Id),
                null,
                i => i.Include(f => f.Orders).Include(f => f.Shipments)).ToListAsync();
                result.Fulfillments = Mapper.Map<ICollection<SummaryPOFulfillmentViewModel>>(fulfillments);
                // PO linked with shipment via actived Booking 
                result.Fulfillments = result.Fulfillments.Where(c => c.Status == POFulfillmentStatus.Active).ToList();

                //For case PO linked with shipment via CargoDetail
                allBookings =  Mapper.Map<List<SummaryPOFulfillmentViewModel>>(fulfillments); 

                foreach (var fulfillment in result.Fulfillments)
                {
                    var src = fulfillments.SingleOrDefault(f => f.Id == fulfillment.Id);
                    fulfillment.FulfillmentUnitQty = src.Orders.Where(f => f.PurchaseOrderId == model.Id).Sum(f => f.FulfillmentUnitQty);
                    fulfillment.ShipFromName = fulfillment.ShipFromName;
                    fulfillment.ShipToName = fulfillment.ShipToName;

                    fulfillment.Shipments = new List<ShipmentReferenceViewModel>();
                    var shipments = src.Shipments?.Where(s => s.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase));
                    foreach (var item in shipments)
                    {
                        var shipmentItem = new ShipmentReferenceViewModel()
                        {
                            ShipmentId = item.Id,
                            ShipmentNumber = item.ShipmentNo
                        };
                        fulfillment.Shipments.Add(shipmentItem);
                    }
                }
            }

            if (result != null)
            {
                result.LineItems = result.LineItems.OrderBy(x => x.LineOrder).ToList();
                // From compliance settings
                result.AllowToBookIn = compliance.AllowToBookIn;
                result.CustomerServiceType = compliance.ServiceType;
                result.IsAllowMissingPO = compliance.IsAllowMissingPO;

                // Replace HS code and Chinese description if external
                if (!currentUser.IsInternal && replacedByOrganizationReferences.Value)
                {
                    // Replace HS code and Chinese description
                    var userOrgId = currentUser.OrganizationId;
                    var storedOrgPreferences = await _organizationPreferenceService.GetAsNoTrackingAsync(userOrgId, result.LineItems.Select(x => x.ProductCode));
                    foreach (var item in result.LineItems)
                    {
                        var storedOrgPreference = storedOrgPreferences.FirstOrDefault(x => x.ProductCode == item.ProductCode);
                        item.HSCode = storedOrgPreference?.HSCode ?? item.HSCode;
                        item.ChineseDescription = storedOrgPreference?.ChineseDescription;
                    }
                }


            }

            #region Progress Check
            result.IsProgressCargoReadyDates = compliance.IsProgressCargoReadyDate;
            result.IsCompulsory = compliance.IsCompulsory;
            result.ProgressNotifyDay = compliance.ProgressNotifyDay;
            #endregion Progress Check

            #region Shipments
            // Get shipments without booking module
            if ((model.POType == POType.Bulk || compliance.AllowToBookIn == model.POType) && !allBookings.Any())
            {
                var shipments = new List<ShipmentViewModel>();
                var cargodetails = await _cargoDetailRepository.QueryAsNoTracking(c => c.OrderId == model.Id, null, c => c.Include(s => s.Shipment)).ToListAsync();
                //cargodetails = cargodetails.DistinctBy(c => c.ShipmentId).ToList();
                var groupedCargodetais = cargodetails.GroupBy(c => c.ShipmentId);
                foreach (var groupedCargodetai in groupedCargodetais)
                {
                    var shipment = Mapper.Map<ShipmentViewModel>(groupedCargodetai.FirstOrDefault().Shipment);
                    if ( shipment.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase))
                    {
                        shipment.BookedQuantity = groupedCargodetai.Sum(c => c.Unit);
                        shipments.Add(shipment);
                    }
                    
                }
                result.Shipments = Mapper.Map<ICollection<ShipmentViewModel>>(shipments);
            }
            #endregion Shipments

            if (result != null)
            {
                // Hide Unit Price and Currency from user of delegation org
                var delegationOrgIds = result?.Contacts?.Where(x => x.OrganizationRole.Equals(OrganizationRole.Delegation)).Select(x => x.OrganizationId);
                if (delegationOrgIds != null
                    && delegationOrgIds.Any()
                    && result.LineItems != null
                    && result.LineItems.Any()
                    && delegationOrgIds.Contains(currentUser.OrganizationId))
                {
                    result.LineItems.Each(x =>
                    {
                        x.UnitPrice = null;
                        x.CurrencyCode = null;
                    });
                }
                await UpdateContactsToLatestByOrgCodesAsync(result.Contacts);
            }

            return result;
        }

        /// <summary>
        /// Get customer PO list for selected purchase orders on booking page.
        /// As it fetches data from ArticleMaster table, customer information required.
        /// </summary>
        /// <param name="purchaseOrderIds">String of list of purchase orders' ids</param>
        /// <param name="customerOrgCode">Customer organization id</param>
        /// <param name="replacedByOrganizationReferences">Whether to replace HS code, Chinese description from Organization preferences</param>
        /// <param name="preferredOrganizationId">Id of preferred Organization</param>
        /// <returns></returns>
        public async Task<IEnumerable<BookingPOViewModel>> GetCustomerPOListByIds(string purchaseOrderIds, string customerOrgCode, bool replacedByOrganizationReferences, long preferredOrganizationId)
        {
            List<SqlParameter> filterParameter;
            var storedProcedureName = "spu_GetCustomerPurchaseOrderList_ByPOIds";
            filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@purchaseOrderIds",
                        Value = purchaseOrderIds,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@customerOrganizationCode",
                        Value = customerOrgCode,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@preferredOrganizationId",
                        // if no need to replace, set = -1
                        Value = replacedByOrganizationReferences ? preferredOrganizationId : -1,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, IEnumerable<BookingPOViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<BookingPOViewModel>();

                // Map data for purchase order information
                while (reader.Read())
                {
                    var newRow = new BookingPOViewModel
                    {
                        Contacts = new List<PurchaseOrderContactViewModel>(),
                        LineItems = new List<BookingPOLineItemViewModel>()
                    };
                    object tmpValue;

                    // Must be in order of data reader
                    newRow.Id = (long)reader[0];

                    tmpValue = reader[1];
                    newRow.CargoReadyDate = DBNull.Value == tmpValue ? null : (DateTime?)tmpValue;
                    tmpValue = reader[2];
                    newRow.CarrierCode = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[3];
                    newRow.CarrierName = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[4];
                    newRow.ContainerType = DBNull.Value == tmpValue ? (EquipmentType?)null : Enum.Parse<EquipmentType>(tmpValue.ToString());
                    tmpValue = reader[5];
                    newRow.ExpectedDeliveryDate = DBNull.Value == tmpValue ? null : (DateTime?)tmpValue;
                    tmpValue = reader[6];
                    newRow.ExpectedShipDate = DBNull.Value == tmpValue ? null : (DateTime?)tmpValue;
                    tmpValue = reader[7];
                    newRow.Incoterm = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[8];
                    newRow.ModeOfTransport = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[9];
                    newRow.PONumber = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[10];
                    newRow.POType = Enum.Parse<POType>(tmpValue.ToString());
                    tmpValue = reader[11];
                    newRow.ShipFrom = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[12];
                    newRow.ShipFromId = DBNull.Value == tmpValue ? null : (long?)tmpValue;
                    tmpValue = reader[13];
                    newRow.ShipTo = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[14];
                    newRow.ShipToId = DBNull.Value == tmpValue ? null : (long?)tmpValue;
                    tmpValue = reader[15];
                    newRow.Status = Enum.Parse<PurchaseOrderStatus>(tmpValue.ToString());
                    mappedData.Add(newRow);
                }

                reader.NextResult();
                // Map data for contacts
                while (reader.Read())
                {
                    var newRow = new PurchaseOrderContactViewModel();
                    object tmpValue;

                    // Must be in order of data reader
                    newRow.Id = (long)reader[0];

                    tmpValue = reader[1];
                    newRow.OrganizationId = (long)tmpValue;
                    tmpValue = reader[2];
                    newRow.OrganizationRole = tmpValue.ToString();
                    tmpValue = reader[3];
                    newRow.OrganizationCode = tmpValue.ToString();
                    tmpValue = reader[4];
                    newRow.CompanyName = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[5];
                    newRow.ContactName = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[6];
                    newRow.ContactNumber = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[7];
                    newRow.ContactEmail = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[8];
                    newRow.AddressLine1 = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[9];
                    newRow.AddressLine2 = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[10];
                    newRow.AddressLine3 = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[11];
                    newRow.AddressLine4 = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[12];
                    newRow.PurchaseOrderId = (long)tmpValue;

                    // Add record to contacts
                    mappedData.First(x => x.Id == newRow.PurchaseOrderId).Contacts.Add(newRow);
                }

                reader.NextResult();
                // Map data for line items
                while (reader.Read())
                {
                    var newRow = new BookingPOLineItemViewModel();
                    object tmpValue;

                    // Must be in order of data reader
                    newRow.Id = (long)reader[0];

                    tmpValue = reader[1];
                    newRow.BalanceUnitQty = DBNull.Value == tmpValue ? null : (int?)tmpValue;
                    tmpValue = reader[2];
                    newRow.BookedUnitQty = DBNull.Value == tmpValue ? null : (int?)tmpValue;
                    tmpValue = reader[3];
                    newRow.Commodity = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[4];
                    newRow.CountryCodeOfOrigin = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[5];
                    newRow.CurrencyCode = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[6];
                    newRow.DescriptionOfGoods = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[7];
                    newRow.HSCode = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[8];
                    newRow.ChineseDescription = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[9];
                    newRow.LineOrder = (int)tmpValue;
                    tmpValue = reader[10];
                    newRow.OrderedUnitQty = DBNull.Value == tmpValue ? null : (int?)tmpValue;
                    tmpValue = reader[11];
                    newRow.PackageUOM = DBNull.Value == tmpValue ? (PackageUOMType?)null : Enum.Parse<PackageUOMType>(tmpValue.ToString());
                    tmpValue = reader[12];
                    newRow.ProductCode = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[13];
                    newRow.ProductName = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[14];
                    newRow.PurchaseOrderId = (long)tmpValue;
                    tmpValue = reader[15];
                    newRow.ShippingMarks = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[16];
                    newRow.UnitPrice = DBNull.Value == tmpValue ? null : (decimal?)tmpValue;
                    tmpValue = reader[17];
                    newRow.UnitUOM = Enum.Parse<UnitUOMType>(tmpValue.ToString());
                    tmpValue = reader[18];
                    newRow.GridValue = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[19];
                    newRow.OuterDepth = DBNull.Value == tmpValue ? null : (decimal?)tmpValue;
                    tmpValue = reader[20];
                    newRow.OuterHeight = DBNull.Value == tmpValue ? null : (decimal?)tmpValue;
                    tmpValue = reader[21];
                    newRow.OuterWidth = DBNull.Value == tmpValue ? null : (decimal?)tmpValue;
                    tmpValue = reader[22];
                    newRow.OuterQuantity = DBNull.Value == tmpValue ? null : (int?)tmpValue;
                    tmpValue = reader[23];
                    newRow.InnerQuantity = DBNull.Value == tmpValue ? null : (int?)tmpValue;
                    tmpValue = reader[24];
                    newRow.OuterGrossWeight = DBNull.Value == tmpValue ? null : (decimal?)tmpValue;

                    // Add record to line items
                    mappedData.First(x => x.Id == newRow.PurchaseOrderId).LineItems.Add(newRow);
                }
                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }

        public async Task<IEnumerable<PrincipalDropdownListItemViewModel>> GetPrincipalDataSourceForMultiPOsSelectionAsync(bool isInternal, long roleId, long organizationId, string affiliates)
        {
            if (Enum.TryParse<Role>(roleId.ToString(), out var currentRole))
            {
                var sql = "";
                switch (currentRole)
                {
                    case Role.CSR:
                    case Role.SystemAdmin:
                        sql = @"
                            SELECT
                                O.Id AS [Value],
                                O.[Code] + ' - ' + O.[Name] AS [Text],
	                            CASE WHEN BC.IsAllowMissingPO IS NULL
                                     THEN '0'
                                     ELSE BC.IsAllowMissingPO
                                END AS [IsAllowMissingPO]
                            FROM Organizations O LEFT JOIN BuyerCompliances BC on O.Id = BC.OrganizationId
                            WHERE O.OrganizationType = 4 AND O.IsBuyer = 1 AND O.[status] = 1
                            ORDER BY [Text] ASC
                            ";
                        break;
                    case Role.Shipper:
                    case Role.Factory:
                        sql = @"
                            SELECT DISTINCT
                                Id AS [Value],
                                [Code] + ' - ' + [Name] AS [Text],
                                CASE WHEN bc.IsAllowMissingPO IS NULL
                                     THEN '0'
                                     ELSE bc.IsAllowMissingPO
                                END AS [IsAllowMissingPO]
                            FROM Organizations o
                            INNER JOIN CustomerRelationship cr on cr.CustomerId = o.Id
                            OUTER APPLY
                            (
	                            SELECT TOP(1) bc.IsAllowMissingPO
	                            FROM BuyerCompliances bc
	                            WHERE bc.OrganizationId = o.Id
                            ) bc
                            WHERE OrganizationType = 4 AND IsBuyer = 1 AND [status] = 1 AND cr.ConnectionType = 1 AND (cr.SupplierId = " + organizationId + @"
                            OR cr.SupplierId IN (
                               SELECT tmp.[Value] 
                               FROM dbo.fn_SplitStringToTable(CAST((
                                    SELECT ParentId
                                    FROM Organizations
                                    WHERE Id = " + organizationId + @") AS nvarchar(20)),'.') tmp))
                            ORDER BY [Text] ASC
                            ";
                        break;
                    case Role.Agent:
                        string organizationIds = string.Empty;
                        if (!string.IsNullOrEmpty(affiliates))
                        {
                            var listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                            organizationIds = string.Join(",", listOfAffiliates);
                        }
                        sql = @"
                            SELECT DISTINCT
                                O.Id AS [Value],
                                O.Code + ' - ' + O.Name AS [Text],
                                CASE WHEN B.IsAllowMissingPO IS NULL THEN '0'
                                     ELSE B.IsAllowMissingPO
                                END AS [IsAllowMissingPO]
							FROM AgentAssignments A
							INNER JOIN BuyerCompliances B ON A.BuyerComplianceId = B.Id AND B.Status = 1 AND B.Stage = 1
							INNER JOIN Organizations O ON B.OrganizationId = O.Id
							WHERE A.AgentOrganizationId IN (" + organizationIds + @")
							ORDER BY [Text] ASC
                            ";
                        break;
                    default:
                        return new List<PrincipalDropdownListItemViewModel>();
                }

                Func<DbDataReader, IEnumerable<PrincipalDropdownListItemViewModel>> mapping = (reader) =>
                {
                    var mappedData = new List<PrincipalDropdownListItemViewModel>();

                    while (reader.Read())
                    {
                        var newRow = new PrincipalDropdownListItemViewModel
                        {
                            Value = reader[0]?.ToString() ?? "",
                            Text = reader[1]?.ToString() ?? "",
                            IsAllowMissingPO = (bool)reader[2]
                        };
                        mappedData.Add(newRow);
                    }

                    return mappedData;
                };
                var result = _dataQuery.GetDataBySql(sql, mapping);
                return result;
            }
            return new List<PrincipalDropdownListItemViewModel>();
        }

        /// <summary>
        /// Fetch customer PO list as searching from Customer PO popup on booking page.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="searchType"></param>
        /// <param name="searchTerm"></param>
        /// <param name="selectedPOId"></param>
        /// <param name="selectedPOType"></param>
        /// <param name="affiliates"></param>
        /// <param name="customerOrgId"></param>
        /// <param name="customerOrgCode"></param>
        /// <param name="supplierOrgId"></param>
        /// <<param name="currentUser"></param>
        /// <param name="replacedByOrganizationReferences">Whether replace HS code and Chinese description for Organization Preferences</param>
        /// <returns></returns>
        public async Task<IEnumerable<BookingPOViewModel>> GetCustomerPOListBySearching(int skip, int take, string searchType, string searchTerm, long selectedPOId, POType selectedPOType, string affiliates, long customerOrgId, string customerOrgCode, long supplierOrgId, string supplierCompanyName, IdentityInfo currentUser, bool replacedByOrganizationReferences)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                var storedProcedureName = "";
                List<SqlParameter> filterParameter;

                filterParameter = new List<SqlParameter>
                {
                     new SqlParameter
                    {
                        ParameterName = "@CustomerOrganizationId",
                        Value = customerOrgId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@CustomerOrganizationCode",
                        Value = customerOrgCode,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@SearchType",
                        Value = searchType,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@SearchTerm",
                        Value = searchTerm,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@SupplierOrganizationId",
                        Value = supplierOrgId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                     new SqlParameter
                    {
                        ParameterName = "@SupplierCompanyName",
                        Value = supplierCompanyName,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@SelectedPOId",
                        Value = selectedPOId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@SelectedPOType",
                        Value = (int)selectedPOType,
                        DbType = DbType.Int32,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Skip",
                        Value = skip,
                        DbType = DbType.Int32,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Take",
                        Value = take,
                        DbType = DbType.Int32,
                        Direction = ParameterDirection.Input
                    }
                };

                if (currentUser.IsInternal)
                {
                    // It is internal user
                    // Not replace HS code, Chinese description
                    storedProcedureName = "[spu_GetCustomerPurchaseOrderList_InternalUsers]";


                }
                else
                {
                    affiliates = affiliates.Replace("[", "").Replace("]", "");
                    storedProcedureName = "[spu_GetCustomerPurchaseOrderList_ExternalUsers]";
                    filterParameter.AddRange(
                        new[] {
                        new SqlParameter
                        {
                            ParameterName = "@CurrentUserOrganizationId",
                            Value = currentUser.OrganizationId,
                            DbType = DbType.Int64,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@Affiliates",
                            Value = affiliates,
                            DbType = DbType.String,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@PreferredOrganizationId",
                            // if no need to replace, set = -1
                            Value = replacedByOrganizationReferences ? currentUser.OrganizationId : -1,
                            DbType = DbType.Int64,
                            Direction = ParameterDirection.Input
                        }
                        });
                }

                Func<DbDataReader, IEnumerable<BookingPOViewModel>> mapping = (reader) =>
                {
                    var mappedData = new List<BookingPOViewModel>();
                    var mappedContacts = new List<PurchaseOrderContactViewModel>();
                    var mappedLineItems = new List<BookingPOLineItemViewModel>();

                    while (reader.Read())
                    {
                        var cargoReadyDate = reader[0];
                        var carrierCode = reader[1];
                        var carrierName = reader[2];
                        var containerType = reader[3];
                        var expectedDeliveryDate = reader[4];
                        var expectedShipDate = reader[5];
                        var id = reader[6];
                        var incoterm = reader[7];
                        var modeOfTransport = reader[8];
                        var poNumber = reader[9];
                        var shipFrom = reader[10];
                        var shipFromId = reader[11];
                        var shipTo = reader[12];
                        var shipToId = reader[13];
                        var status = reader[14];
                        var poType = reader[15];
                        var rowCount = reader[16];

                        var newRow = new BookingPOViewModel
                        {
                            Id = (long)id,
                            PONumber = poNumber?.ToString(),
                            ModeOfTransport = modeOfTransport?.ToString(),
                            ShipFromId = (long?)(shipFromId != DBNull.Value ? shipFromId : null),
                            ShipFrom = shipFrom?.ToString(),
                            ShipToId = (long?)(shipToId != DBNull.Value ? shipToId : null),
                            ShipTo = shipTo?.ToString(),
                            Incoterm = incoterm?.ToString(),
                            CarrierCode = carrierCode?.ToString(),
                            CarrierName = carrierName?.ToString(),
                            CargoReadyDate = (DateTime?)(cargoReadyDate != DBNull.Value ? cargoReadyDate : null),
                            POType = Enum.Parse<POType>(poType.ToString()),
                            ExpectedShipDate = (DateTime?)(expectedShipDate != DBNull.Value ? expectedShipDate : null),
                            ExpectedDeliveryDate = (DateTime?)(expectedDeliveryDate != DBNull.Value ? expectedDeliveryDate : null),
                            Status = Enum.Parse<PurchaseOrderStatus>(status.ToString()),
                            ContainerType = (containerType != DBNull.Value ? Enum.Parse<EquipmentType>(containerType.ToString()) : (EquipmentType?)null),
                            RecordCount = (long)rowCount
                        };
                        mappedData.Add(newRow);
                    }
                    reader.NextResult();
                    while (reader.Read())
                    {
                        var id = reader[0];
                        var purchaseOrderId = reader[1];
                        var organizationId = reader[2];
                        var organizationCode = reader[3];
                        var organizationRole = reader[4];
                        var companyName = reader[5];
                        var addressLine1 = reader[6];
                        var addressLine2 = reader[7];
                        var addressLine3 = reader[8];
                        var addressLine4 = reader[9];
                        var department = reader[10];
                        var contactName = reader[11];
                        var name = reader[12];
                        var contactNumber = reader[13];
                        var contactEmail = reader[14];
                        var references = reader[15];

                        var newRow = new PurchaseOrderContactViewModel
                        {
                            Id = (long)id,
                            PurchaseOrderId = (long)purchaseOrderId,
                            OrganizationId = (long)organizationId,
                            OrganizationCode = organizationCode?.ToString(),
                            OrganizationRole = organizationRole?.ToString(),
                            CompanyName = companyName?.ToString(),
                            AddressLine1 = addressLine1?.ToString(),
                            AddressLine2 = addressLine2?.ToString(),
                            AddressLine3 = addressLine3?.ToString(),
                            AddressLine4 = addressLine4?.ToString(),
                            Department = department?.ToString(),
                            ContactName = contactName?.ToString(),
                            Name = name?.ToString(),
                            ContactNumber = contactNumber?.ToString(),
                            ContactEmail = contactEmail?.ToString(),
                            References = references?.ToString()
                        };
                        mappedContacts.Add(newRow);
                    }
                    reader.NextResult();
                    while (reader.Read())
                    {
                        var id = reader[0];
                        var purchaseOrderId = reader[1];
                        var balanceUnitQty = reader[2];
                        var bookedUnitQty = reader[3];
                        var commodity = reader[4];
                        var countryCodeOfOrigin = reader[5];
                        var currencyCode = reader[6];
                        var descriptionOfGoods = reader[7];
                        var hsCode = reader[8];
                        var chineseDescription = reader[9];
                        var lineOrder = reader[10];
                        var orderedUnitQty = reader[11];
                        var packageUOM = reader[12];
                        var productCode = reader[13];
                        var gridValue = reader[14];
                        var productName = reader[15];
                        var unitPrice = reader[16];
                        var unitUOM = reader[17];
                        var shippingMarks = reader[18];
                        var outerDepth = reader[19];
                        var outerHeight = reader[20];
                        var outerWidth = reader[21];
                        var outerQuantity = reader[22];
                        var innerQuantity = reader[23];
                        var outerGrossWeight = reader[24];


                        var newRow = new BookingPOLineItemViewModel
                        {
                            Id = (long)id,
                            PurchaseOrderId = (long)purchaseOrderId,
                            BalanceUnitQty = (int?)(balanceUnitQty != DBNull.Value ? balanceUnitQty : null),
                            BookedUnitQty = (int?)(bookedUnitQty != DBNull.Value ? bookedUnitQty : null),
                            Commodity = commodity?.ToString(),
                            CountryCodeOfOrigin = countryCodeOfOrigin?.ToString(),
                            CurrencyCode = currencyCode?.ToString(),
                            DescriptionOfGoods = descriptionOfGoods?.ToString(),
                            HSCode = hsCode?.ToString(),
                            ChineseDescription = chineseDescription?.ToString(),
                            LineOrder = (int)lineOrder,
                            OrderedUnitQty = (int?)(orderedUnitQty != DBNull.Value ? orderedUnitQty : null),
                            PackageUOM = (packageUOM != DBNull.Value ? Enum.Parse<PackageUOMType>(packageUOM.ToString()) : (PackageUOMType?)null),
                            ProductCode = productCode?.ToString(),
                            GridValue = gridValue?.ToString(),
                            ProductName = productName?.ToString(),
                            UnitPrice = (decimal?)(unitPrice != DBNull.Value ? unitPrice : null),
                            UnitUOM = Enum.Parse<UnitUOMType>(unitUOM.ToString()),
                            ShippingMarks = shippingMarks?.ToString(),
                            OuterDepth = (decimal?)(outerDepth != DBNull.Value ? outerDepth : null),
                            OuterHeight = (decimal?)(outerHeight != DBNull.Value ? outerHeight : null),
                            OuterWidth = (decimal?)(outerWidth != DBNull.Value ? outerWidth : null),
                            OuterQuantity = (int?)(outerQuantity != DBNull.Value ? outerQuantity : null),
                            InnerQuantity = (int?)(innerQuantity != DBNull.Value ? innerQuantity : null),
                            OuterGrossWeight = (decimal?)(outerGrossWeight != DBNull.Value ? outerGrossWeight : null)
                        };
                        mappedLineItems.Add(newRow);
                    }

                    foreach (var item in mappedData)
                    {
                        item.Contacts = mappedContacts.Where(x => x.PurchaseOrderId == item.Id).ToList();
                        item.LineItems = mappedLineItems.Where(x => x.PurchaseOrderId == item.Id).ToList();
                    }

                    return mappedData;
                };
                var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
                return result;
            }
            else
            {
                var storedProcedureName = "";
                List<SqlParameter> filterParameter;

                var searchTerms = searchTerm.Split(',');
                var resultData = new List<BookingPOViewModel>();


                foreach (var term in searchTerms)
                {
                    if (string.IsNullOrEmpty(term.Trim()))
                    {
                        continue;
                    }
                    filterParameter = new List<SqlParameter>
                {
                     new SqlParameter
                    {
                        ParameterName = "@CustomerOrganizationId",
                        Value = customerOrgId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@CustomerOrganizationCode",
                        Value = customerOrgCode,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@SearchType",
                        Value = searchType,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@SearchTerm",
                        Value = term.Trim(),
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@SupplierOrganizationId",
                        Value = supplierOrgId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                     new SqlParameter
                    {
                        ParameterName = "@SupplierCompanyName",
                        Value = supplierCompanyName,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@SelectedPOId",
                        Value = selectedPOId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@SelectedPOType",
                        Value = (int)selectedPOType,
                        DbType = DbType.Int32,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Skip",
                        Value = skip,
                        DbType = DbType.Int32,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Take",
                        Value = take,
                        DbType = DbType.Int32,
                        Direction = ParameterDirection.Input
                    }
                };

                    if (currentUser.IsInternal)
                    {
                        // It is internal user
                        // Not replace HS code, Chinese description
                        storedProcedureName = "[spu_GetCustomerPurchaseOrderList_InternalUsers]";


                    }
                    else
                    {
                        affiliates = affiliates.Replace("[", "").Replace("]", "");
                        storedProcedureName = "[spu_GetCustomerPurchaseOrderList_ExternalUsers]";
                        filterParameter.AddRange(
                            new[] {
                        new SqlParameter
                        {
                            ParameterName = "@CurrentUserOrganizationId",
                            Value = currentUser.OrganizationId,
                            DbType = DbType.Int64,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@Affiliates",
                            Value = affiliates,
                            DbType = DbType.String,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@PreferredOrganizationId",
                            // if no need to replace, set = -1
                            Value = replacedByOrganizationReferences ? currentUser.OrganizationId : -1,
                            DbType = DbType.Int64,
                            Direction = ParameterDirection.Input
                        }
                            });

                    }

                    Func<DbDataReader, IEnumerable<BookingPOViewModel>> mapping = (reader) =>
                    {
                        var mappedData = new List<BookingPOViewModel>();
                        var mappedContacts = new List<PurchaseOrderContactViewModel>();
                        var mappedLineItems = new List<BookingPOLineItemViewModel>();

                        while (reader.Read())
                        {
                            var cargoReadyDate = reader[0];
                            var carrierCode = reader[1];
                            var carrierName = reader[2];
                            var containerType = reader[3];
                            var expectedDeliveryDate = reader[4];
                            var expectedShipDate = reader[5];
                            var id = reader[6];
                            var incoterm = reader[7];
                            var modeOfTransport = reader[8];
                            var poNumber = reader[9];
                            var shipFrom = reader[10];
                            var shipFromId = reader[11];
                            var shipTo = reader[12];
                            var shipToId = reader[13];
                            var status = reader[14];
                            var poType = reader[15];
                            var rowCount = reader[16];

                            var newRow = new BookingPOViewModel
                            {
                                Id = (long)id,
                                PONumber = poNumber?.ToString(),
                                ModeOfTransport = modeOfTransport?.ToString(),
                                ShipFromId = (long?)(shipFromId != DBNull.Value ? shipFromId : null),
                                ShipFrom = shipFrom?.ToString(),
                                ShipToId = (long?)(shipToId != DBNull.Value ? shipToId : null),
                                ShipTo = shipTo?.ToString(),
                                Incoterm = incoterm?.ToString(),
                                CarrierCode = carrierCode?.ToString(),
                                CarrierName = carrierName?.ToString(),
                                CargoReadyDate = (DateTime?)(cargoReadyDate != DBNull.Value ? cargoReadyDate : null),
                                POType = Enum.Parse<POType>(poType.ToString()),
                                ExpectedShipDate = (DateTime?)(expectedShipDate != DBNull.Value ? expectedShipDate : null),
                                ExpectedDeliveryDate = (DateTime?)(expectedDeliveryDate != DBNull.Value ? expectedDeliveryDate : null),
                                Status = Enum.Parse<PurchaseOrderStatus>(status.ToString()),
                                ContainerType = (containerType != DBNull.Value ? Enum.Parse<EquipmentType>(containerType.ToString()) : (EquipmentType?)null),
                                RecordCount = (long)rowCount
                            };
                            mappedData.Add(newRow);
                        }
                        reader.NextResult();
                        while (reader.Read())
                        {
                            var id = reader[0];
                            var purchaseOrderId = reader[1];
                            var organizationId = reader[2];
                            var organizationCode = reader[3];
                            var organizationRole = reader[4];
                            var companyName = reader[5];
                            var addressLine1 = reader[6];
                            var addressLine2 = reader[7];
                            var addressLine3 = reader[8];
                            var addressLine4 = reader[9];
                            var department = reader[10];
                            var contactName = reader[11];
                            var name = reader[12];
                            var contactNumber = reader[13];
                            var contactEmail = reader[14];
                            var references = reader[15];

                            var newRow = new PurchaseOrderContactViewModel
                            {
                                Id = (long)id,
                                PurchaseOrderId = (long)purchaseOrderId,
                                OrganizationId = (long)organizationId,
                                OrganizationCode = organizationCode?.ToString(),
                                OrganizationRole = organizationRole?.ToString(),
                                CompanyName = companyName?.ToString(),
                                AddressLine1 = addressLine1?.ToString(),
                                AddressLine2 = addressLine2?.ToString(),
                                AddressLine3 = addressLine3?.ToString(),
                                AddressLine4 = addressLine4?.ToString(),
                                Department = department?.ToString(),
                                ContactName = contactName?.ToString(),
                                Name = name?.ToString(),
                                ContactNumber = contactNumber?.ToString(),
                                ContactEmail = contactEmail?.ToString(),
                                References = references?.ToString()
                            };
                            mappedContacts.Add(newRow);
                        }
                        reader.NextResult();
                        while (reader.Read())
                        {
                            var id = reader[0];
                            var purchaseOrderId = reader[1];
                            var balanceUnitQty = reader[2];
                            var bookedUnitQty = reader[3];
                            var commodity = reader[4];
                            var countryCodeOfOrigin = reader[5];
                            var currencyCode = reader[6];
                            var descriptionOfGoods = reader[7];
                            var hsCode = reader[8];
                            var chineseDescription = reader[9];
                            var lineOrder = reader[10];
                            var orderedUnitQty = reader[11];
                            var packageUOM = reader[12];
                            var productCode = reader[13];
                            var gridValue = reader[14];
                            var productName = reader[15];
                            var unitPrice = reader[16];
                            var unitUOM = reader[17];
                            var shippingMarks = reader[18];
                            var outerDepth = reader[19];
                            var outerHeight = reader[20];
                            var outerWidth = reader[21];
                            var outerQuantity = reader[22];
                            var innerQuantity = reader[23];
                            var outerGrossWeight = reader[24];


                            var newRow = new BookingPOLineItemViewModel
                            {
                                Id = (long)id,
                                PurchaseOrderId = (long)purchaseOrderId,
                                BalanceUnitQty = (int?)(balanceUnitQty != DBNull.Value ? balanceUnitQty : null),
                                BookedUnitQty = (int?)(bookedUnitQty != DBNull.Value ? bookedUnitQty : null),
                                Commodity = commodity?.ToString(),
                                CountryCodeOfOrigin = countryCodeOfOrigin?.ToString(),
                                CurrencyCode = currencyCode?.ToString(),
                                DescriptionOfGoods = descriptionOfGoods?.ToString(),
                                HSCode = hsCode?.ToString(),
                                ChineseDescription = chineseDescription?.ToString(),
                                LineOrder = (int)lineOrder,
                                OrderedUnitQty = (int?)(orderedUnitQty != DBNull.Value ? orderedUnitQty : null),
                                PackageUOM = (packageUOM != DBNull.Value ? Enum.Parse<PackageUOMType>(packageUOM.ToString()) : (PackageUOMType?)null),
                                ProductCode = productCode?.ToString(),
                                GridValue = gridValue?.ToString(),
                                ProductName = productName?.ToString(),
                                UnitPrice = (decimal?)(unitPrice != DBNull.Value ? unitPrice : null),
                                UnitUOM = Enum.Parse<UnitUOMType>(unitUOM.ToString()),
                                ShippingMarks = shippingMarks?.ToString(),
                                OuterDepth = (decimal?)(outerDepth != DBNull.Value ? outerDepth : null),
                                OuterHeight = (decimal?)(outerHeight != DBNull.Value ? outerHeight : null),
                                OuterWidth = (decimal?)(outerWidth != DBNull.Value ? outerWidth : null),
                                OuterQuantity = (int?)(outerQuantity != DBNull.Value ? outerQuantity : null),
                                InnerQuantity = (int?)(innerQuantity != DBNull.Value ? innerQuantity : null),
                                OuterGrossWeight = (decimal?)(outerGrossWeight != DBNull.Value ? outerGrossWeight : null)
                            };
                            mappedLineItems.Add(newRow);
                        }

                        foreach (var item in mappedData)
                        {
                            item.Contacts = mappedContacts.Where(x => x.PurchaseOrderId == item.Id).ToList();
                            item.LineItems = mappedLineItems.Where(x => x.PurchaseOrderId == item.Id).ToList();
                        }

                        return mappedData;
                    };
                    var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
                    foreach (var item in result)
                    {
                        // Check if the row's Id already exists in resultData
                        if (!resultData.Any(existingItem => existingItem.Id == item.Id))
                        {
                            resultData.Add(item);
                        }
                    }
                }
                return resultData;
            }

        }

        private async Task<IEnumerable<POLineItemArticleMasterViewModel>> GetPOLineItemArticleMasterList(long customerOrgId, List<PurchaseOrderModel> selectedPOList)
        {
            var customerOrg = await _csfeApiClient.GetOrganizationByIdAsync(customerOrgId);
            var productCodes = string.Join(",", selectedPOList.SelectMany(r => r.LineItems).Select(i => i.ProductCode).Distinct());

            var sql = @"SELECT item.Id, am.OuterDepth, am.OuterHeight, am.OuterWidth, am.OuterQuantity, am.InnerQuantity, am.OuterGrossWeight
                        FROM POLineItems item JOIN ArticleMaster am WITH (NOLOCK) ON item.ProductCode = TRIM(am.ItemNo) 
                        WHERE am.CompanyCode = @companyCode AND item.ProductCode IN (SELECT [VALUE] FROM [dbo].[fn_SplitStringToTable] (@productCodes, ','))";

            var filterParameters = new List<SqlParameter>
                {
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
                        Value = productCodes,
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
                        OuterDepth = reader["OuterDepth"] as decimal?,
                        OuterHeight = reader["OuterHeight"] as decimal?,
                        OuterWidth = reader["OuterWidth"] as decimal?,
                        OuterQuantity = reader["OuterQuantity"] as int?,
                        InnerQuantity = reader["InnerQuantity"] as int?,
                        OuterGrossWeight = reader["OuterGrossWeight"] as decimal?
                    };
                    mappedData.Add(newRow);
                }

                return mappedData;
            }

            return _dataQuery.GetDataBySql(sql, mappingCallback, filterParameters.ToArray());
        }

        public async Task<ReportingMetricPOViewModel> GetReportingPOs(bool isInternal, string affiliates, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var listOfAffiliates = new List<long>();

            if (!string.IsNullOrEmpty(affiliates))
            {
                listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
            }

            var query = Repository.GetListQueryable().Where(x => x.Status == PurchaseOrderStatus.Active);
            if (!isInternal)
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }
                query = query.Where(s => s.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId)));
            }

            DateTime currentWeekStartDate = DateTime.UtcNow.WeekStartDate();
            DateTime lastWeekStartDate = currentWeekStartDate.WeekStartDate();
            DateTime nextWeekStartDate = DateTime.UtcNow.Date;

            var fromDate = DateTime.Parse(dates["FromDate"]);
            var toDate = DateTime.Parse(dates["ToDate"]);

            var thisWeekQuery = query.Where(s => s.POIssueDate >= fromDate && s.POIssueDate <= toDate);
            var lastWeekQuery = query.Where(s => s.POIssueDate >= lastWeekStartDate && s.POIssueDate < currentWeekStartDate);

            ReportingMetricPOViewModel result = new ReportingMetricPOViewModel
            {
                ThisWeekTotalPOs = await thisWeekQuery.CountAsync(),
                LastWeekTotalPOs = await lastWeekQuery.CountAsync(),
                LastWeekStartDate = lastWeekStartDate,
                ThisWeekStartDate = currentWeekStartDate,
                NextWeekStartDate = nextWeekStartDate.AddDays(-1)
            };

            return result;
        }

        public async Task<IEnumerable<PurchaseOrderProgressCheckViewModel>> SearchPOsForProgressCheckAsync(string jsonFilter, IdentityInfo currentUser, string affiliates = "")
        {
            var storedProcedureName = "spu_GetPOList_ProgressCheck";
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
            Func<DbDataReader, IEnumerable<PurchaseOrderProgressCheckViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<PurchaseOrderProgressCheckViewModel>();

                // Map data for purchase order information
                while (reader.Read())
                {
                    var newRow = new PurchaseOrderProgressCheckViewModel();
                    object tmpValue;

                    // Must be in order of data reader
                    newRow.Id = (long)reader[0];
                    tmpValue = reader[1];
                    newRow.PONumber = tmpValue.ToString();
                    tmpValue = reader[2];
                    newRow.CargoReadyDate = DBNull.Value == tmpValue ? (DateTime?)null : (DateTime)tmpValue;
                    tmpValue = reader[3];
                    newRow.ProductionStarted = (bool)tmpValue;
                    tmpValue = reader[4];
                    newRow.ProposeDate = DBNull.Value == tmpValue ? (DateTime?)null : (DateTime)tmpValue;
                    tmpValue = reader[5];
                    newRow.QCRequired = (bool)tmpValue;
                    tmpValue = reader[6];
                    newRow.ShortShip = (bool)tmpValue;
                    tmpValue = reader[7];
                    newRow.SplitShipment = (bool)tmpValue;
                    tmpValue = reader[8];
                    newRow.Remark = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    mappedData.Add(newRow);
                }

                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, parameters.ToArray());
            return result;
        }

        public async Task<ShipmentPOLineItemViewModel> GetPOLineItem(long purchaseOrderId, long poLineItemId)
        {
            var purchaseOrder = await Repository.GetAsync(po => po.Id == purchaseOrderId, null, i => i.Include(x => x.LineItems));

            var lineItem = purchaseOrder.LineItems.FirstOrDefault(l => l.Id == poLineItemId);

            var result = Mapper.Map<ShipmentPOLineItemViewModel>(lineItem);

            return result;
        }

        public async Task ImportFromExcel(byte[] file, string userName, long importDataProgressId)
        {
            using (var bjUoW = UnitOfWorkProvider.CreateUnitOfWorkForBackgroundJob())
            {
                var importDataProgressRepository = (IImportDataProgressRepository)bjUoW.GetRepository<ImportDataProgressModel>();

                if (file == null)
                {
                    await importDataProgressRepository.UpdateStatusAsync(
                        importDataProgressId,
                        ImportDataProgressStatus.Failed,
                        "importResult.fileIsNull");

                    return;
                }

                try
                {
                    using (Stream fileStream = new MemoryStream(file))
                    {
                        using (var xlPackage = new ExcelPackage(fileStream))
                        {
                            var excelPOs = new List<ExcelPOViewModel>();
                            var excelPOContacts = new List<ExcelPOContactViewModel>();
                            var excelPOLineItems = new List<ExcelPOLineItemViewModel>();
                            var importErrorLogs = new List<ValidatorErrorInfo>();
                            var importedCreatedPOs = new List<PurchaseOrderModel>();
                            var importedUpdatedPOs = new List<PurchaseOrderModel>();

                            #region PO Contact sheet
                            var contactSheet = xlPackage.Workbook.Worksheets.SingleOrDefault(s => s.Name == EXCEL_POCONTACT_SHEET_NAME);
                            if (contactSheet == null)
                            {
                                await importDataProgressRepository.UpdateStatusAsync(
                                    importDataProgressId,
                                    ImportDataProgressStatus.Failed,
                                    "importResult.poContactSheetNotFound");

                                return;
                            }

                            var contactPropetyManager = new PropertyManager<ExcelPOContactViewModel>();
                            int contactRowCount = contactSheet.Dimension.Rows;

                            var excelPOContactsViewModel = new List<ExcelPOContactViewModel>();
                            var exContactPropetyManager = new PropertyManager<ExcelPOContactViewModel>();
                            for (var row = 2; row <= contactRowCount; row++)
                            {
                                if (contactSheet.IsRowEmpty(row)) continue;
                                var poContactViewModel = exContactPropetyManager.ReadExcelByRowWithoutValidation(contactSheet, row);
                                poContactViewModel.Row = row.ToString();
                                // This line code will make sure the Role is always UPPERCASE at first, unless it will cause some issues.
                                poContactViewModel.OrganizationRole = StringHelper.FirstCharToUpperCase(poContactViewModel.OrganizationRole);
                                excelPOContactsViewModel.Add(poContactViewModel);
                            }
                            var principalExcelContact = excelPOContactsViewModel.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Principal, StringComparison.OrdinalIgnoreCase));
                            dynamic principalOrganization = null;
                            if (principalExcelContact != null)
                            {
                                principalOrganization = await _csfeApiClient.GetOrganizationsByCodeAsync(principalExcelContact.OrganizationCode);
                            }

                            var excelPOContactViewModelValidator = new ExcelPOContactViewModelValidator(_csfeApiClient, _dataQuery, principalOrganization?.Id);

                            for (var row = 2; row <= contactRowCount; row++)
                            {
                                if (contactSheet.IsRowEmpty(row))
                                {
                                    continue;
                                }

                                var poContactViewModel = contactPropetyManager.ReadExcelByRow(contactSheet, row, "PONumber", EXCEL_POCONTACT_SHEET_NAME_TRANSLATE, excelPOContactViewModelValidator, out IList<ValidatorErrorInfo> errorInfoItems);
                                poContactViewModel.Row = row.ToString();
                                if (errorInfoItems.Any())
                                {
                                    importErrorLogs.AddRange(errorInfoItems);
                                }
                                else
                                {
                                    // This line code will make sure the Role is always UPPERCASE at first, unless it will cause some issues.
                                    poContactViewModel.OrganizationRole =
                                        StringHelper.FirstCharToUpperCase(poContactViewModel.OrganizationRole);
                                    excelPOContacts.Add(poContactViewModel);
                                }
                            }

                            excelPOContacts = excelPOContacts.OrderBy(p => p.PONumber).ToList();

                            var codes = excelPOContacts.Select(c => c.OrganizationCode).Distinct();
                            var organizations = (await _csfeApiClient.GetOrganizationsByCodesAsync(codes)).ToList();

                            var orgIds = organizations.Select(o => o.Id).ToList();
                            var compliances = (await _buyerComplianceService.GetListByOrgIdsAsync(orgIds)).ToList();

                            #endregion

                            #region PO Header sheet
                            var generalSheet = xlPackage.Workbook.Worksheets.SingleOrDefault(s => s.Name == EXCEL_POHEADER_SHEET_NAME);
                            if (generalSheet == null)
                            {
                                await importDataProgressRepository.UpdateStatusAsync(
                                    importDataProgressId,
                                    ImportDataProgressStatus.Failed,
                                    "importResult.poHeaderSheetNotFound");

                                return;
                            }

                            var generalPropetyManager = new PropertyManager<ExcelPOViewModel>();
                            int poRowCount = generalSheet.Dimension.Rows;

                            if (poRowCount - 1 > 500)
                            {
                                await importDataProgressRepository.UpdateStatusAsync(
                                    importDataProgressId,
                                    ImportDataProgressStatus.Failed,
                                    "importResult.limit500PO",
                                    "LIMIT_ITEM");

                                return;
                            }

                            var excelPOViewModelValidator = new ExcelPOViewModelValidator(_csfeApiClient);

                            await importDataProgressRepository.UpdateProgressAsync(importDataProgressId, 0, poRowCount - 1);

                            for (var row = 2; row <= poRowCount; row++)
                            {
                                if (generalSheet.IsRowEmpty(row))
                                {
                                    continue;
                                }

                                var poViewModel = generalPropetyManager.ReadExcelByRow(generalSheet, row, "PONumber", EXCEL_POHEADER_SHEET_NAME_TRANSLATE, excelPOViewModelValidator, out IList<ValidatorErrorInfo> errorInfoItems);
                                poViewModel.Row = row.ToString();

                                if (excelPOs.Any(po => po.PONumber == poViewModel.PONumber))
                                {
                                    errorInfoItems.Add(
                                       new ValidatorErrorInfo
                                       {
                                           SheetName = generalSheet.Name,
                                           ObjectName = "PONumber",
                                           Row = row.ToString(),
                                           Column = generalPropetyManager.GetProperty("PONumber").ColumnOrderPosition.ToString(),
                                           ErrorMsg = "importLog.duplicatePONumber"
                                       });
                                }

                                if (errorInfoItems.Any())
                                {
                                    importErrorLogs.AddRange(errorInfoItems);
                                }
                                else
                                {
                                    excelPOs.Add(poViewModel);
                                }

                                var orgCode = GetPrincipalOrgCodeByPONumberFromExcel(poViewModel.PONumber, excelPOContacts);
                                var compliance = GetActivatedComplianceByOrgCode(orgCode, organizations, compliances);
                                if (compliance?.PurchaseOrderVerificationSetting != null)
                                {
                                    var poVerification = compliance.PurchaseOrderVerificationSetting;

                                    if ((poVerification.ExpectedShipDateVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                        poVerification.ExpectedShipDateVerification == VerificationSettingType.AsPerPOAllowOverride ||
                                        poVerification.ExpectedShipDateVerification == VerificationSettingType.AsPerPODefault) &&
                                        poViewModel.ExpectedShipDate == null)
                                    {
                                        importErrorLogs.Add(
                                            new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POHEADER_SHEET_NAME_TRANSLATE,
                                                Column = "ExpectedShipDate",
                                                ErrorMsg = "importLog.missingMandatoryField",
                                                Row = row.ToString()
                                            });
                                    }

                                    if ((poVerification.ExpectedDeliveryDateVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                        poVerification.ExpectedDeliveryDateVerification == VerificationSettingType.AsPerPOAllowOverride ||
                                        poVerification.ExpectedDeliveryDateVerification == VerificationSettingType.AsPerPODefault) &&
                                        poViewModel.ExpectedDeliveryDate == null)
                                    {
                                        importErrorLogs.Add(
                                            new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POHEADER_SHEET_NAME_TRANSLATE,
                                                Column = "ExpectedShipDate",
                                                ErrorMsg = "importLog.missingMandatoryField",
                                                Row = row.ToString()
                                            });
                                    }

                                    if ((poVerification.ShipFromLocationVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                        poVerification.ShipFromLocationVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                                        string.IsNullOrWhiteSpace(poViewModel.ShipFrom))
                                    {
                                        importErrorLogs.Add(
                                            new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POHEADER_SHEET_NAME_TRANSLATE,
                                                Column = "ShipFrom",
                                                ErrorMsg = "importLog.missingMandatoryField",
                                                Row = row.ToString()
                                            });
                                    }

                                    if ((poVerification.ShipToLocationVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                        poVerification.ShipToLocationVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                                        string.IsNullOrWhiteSpace(poViewModel.ShipTo))
                                    {
                                        importErrorLogs.Add(
                                            new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POHEADER_SHEET_NAME_TRANSLATE,
                                                Column = "ShipTo",
                                                ErrorMsg = "importLog.missingMandatoryField",
                                                Row = row.ToString()
                                            });
                                    }

                                    if ((poVerification.PaymentTermsVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                        poVerification.PaymentTermsVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                                        string.IsNullOrWhiteSpace(poViewModel.PaymentTerms))
                                    {
                                        importErrorLogs.Add(
                                            new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POHEADER_SHEET_NAME_TRANSLATE,
                                                Column = "PaymentTerms",
                                                ErrorMsg = "importLog.missingMandatoryField",
                                                Row = row.ToString()
                                            });
                                    }

                                    if ((poVerification.PaymentCurrencyVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                        poVerification.PaymentCurrencyVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                                        string.IsNullOrWhiteSpace(poViewModel.PaymentCurrencyCode))
                                    {
                                        importErrorLogs.Add(
                                            new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POHEADER_SHEET_NAME_TRANSLATE,
                                                Column = "PaymentCurrencyCode",
                                                ErrorMsg = "importLog.missingMandatoryField",
                                                Row = row.ToString()
                                            });
                                    }

                                    if ((poVerification.ModeOfTransportVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                        poVerification.ModeOfTransportVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                                        !poViewModel.ModeOfTransport.HasValue)
                                    {
                                        importErrorLogs.Add(
                                            new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POHEADER_SHEET_NAME_TRANSLATE,
                                                Column = "ModeOfTransport",
                                                ErrorMsg = "importLog.missingMandatoryField",
                                                Row = row.ToString()
                                            });
                                    }

                                    if ((poVerification.IncotermVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                        poVerification.IncotermVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                                        !poViewModel.Incoterm.HasValue)
                                    {
                                        importErrorLogs.Add(
                                            new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POHEADER_SHEET_NAME_TRANSLATE,
                                                Column = "Incoterm",
                                                ErrorMsg = "importLog.missingMandatoryField",
                                                Row = row.ToString()
                                            });
                                    }

                                    if ((poVerification.PreferredCarrierVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                        poVerification.PreferredCarrierVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                                        string.IsNullOrWhiteSpace(poViewModel.CarrierCode))
                                    {
                                        importErrorLogs.Add(
                                            new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POHEADER_SHEET_NAME_TRANSLATE,
                                                Column = "CarrierCode",
                                                ErrorMsg = "importLog.missingMandatoryField",
                                                Row = row.ToString()
                                            });
                                    }
                                }

                                await importDataProgressRepository.UpdateProgressAsync(importDataProgressId, row - 1);
                            }

                            excelPOs = excelPOs.OrderBy(p => p.PONumber).ToList();

                            #endregion

                            #region PO Product sheet
                            var lineItemSheet = xlPackage.Workbook.Worksheets.SingleOrDefault(s => s.Name == EXCEL_POPRODUCT_SHEET_NAME);
                            if (lineItemSheet == null)
                            {
                                await importDataProgressRepository.UpdateStatusAsync(
                                    importDataProgressId,
                                    ImportDataProgressStatus.Failed,
                                    "importResult.poProductSheetNotFound");

                                return;
                            }

                            var lineItemPropetyManager = new PropertyManager<ExcelPOLineItemViewModel>();
                            int lineItemRowCount = lineItemSheet.Dimension.Rows;

                            for (var row = 2; row <= lineItemRowCount; row++)
                            {
                                if (lineItemSheet.IsRowEmpty(row))
                                {
                                    continue;
                                }

                                var poLineItemViewModel = lineItemPropetyManager.ReadExcelByRow(lineItemSheet, row, "PONumber", EXCEL_POPRODUCT_SHEET_NAME_TRANSLATE,
                                    new ExcelPOLineItemViewModelValidator(_csfeApiClient),
                                    out IList<ValidatorErrorInfo> errorInfoItems);
                                if (errorInfoItems.Any())
                                {
                                    importErrorLogs.AddRange(errorInfoItems);
                                }
                                else
                                {
                                    var existLineOrder = excelPOLineItems.Any(x =>
                                        x.LineOrder == poLineItemViewModel.LineOrder &&
                                        x.PONumber == poLineItemViewModel.PONumber);

                                    if (existLineOrder)
                                    {
                                        importErrorLogs.Add(
                                            new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POPRODUCT_SHEET_NAME_TRANSLATE,
                                                Column = "LineOrder",
                                                ErrorMsg = "importLog.duplicateLineOrder",
                                                Row = row.ToString()
                                            });

                                        excelPOLineItems.Add(poLineItemViewModel);
                                        // No need to check Compliance
                                        continue;
                                    }

                                    if (string.IsNullOrWhiteSpace(poLineItemViewModel.ProductCode))
                                    {
                                        poLineItemViewModel.ProductCode = poLineItemViewModel.LineOrder.ToString();
                                    }

                                    excelPOLineItems.Add(poLineItemViewModel);
                                }

                                var orgCode = GetPrincipalOrgCodeByPONumberFromExcel(poLineItemViewModel.PONumber, excelPOContacts);

                                // Trim product code for TUMI only
                                if (!string.IsNullOrWhiteSpace(orgCode) && orgCode.ToLower() == _customerOrgReference.TUMIOrgCode.ToLower())
                                {
                                    // Trim leading zeros
                                    poLineItemViewModel.ProductCode = poLineItemViewModel.ProductCode?.TrimStart('0');
                                }

                                var compliance = GetActivatedComplianceByOrgCode(orgCode, organizations, compliances);
                                if (compliance?.PurchaseOrderVerificationSetting != null)
                                {
                                    var productVerification = compliance.ProductVerificationSetting;

                                    if ((productVerification.ProductCodeVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                        productVerification.ProductCodeVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                                        string.IsNullOrWhiteSpace(poLineItemViewModel.ProductCode))
                                    {
                                        importErrorLogs.Add(
                                            new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POPRODUCT_SHEET_NAME_TRANSLATE,
                                                Column = "ProductCode",
                                                ErrorMsg = "importLog.missingMandatoryField",
                                                Row = row.ToString()
                                            });
                                    }

                                    if ((productVerification.HsCodeVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                        productVerification.HsCodeVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                                        string.IsNullOrWhiteSpace(poLineItemViewModel.HSCode))
                                    {
                                        importErrorLogs.Add(
                                            new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POPRODUCT_SHEET_NAME_TRANSLATE,
                                                Column = "HSCode",
                                                ErrorMsg = "importLog.missingMandatoryField",
                                                Row = row.ToString()
                                            });
                                    }

                                    if ((productVerification.CommodityVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                        productVerification.CommodityVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                                        string.IsNullOrWhiteSpace(poLineItemViewModel.Commodity))
                                    {
                                        importErrorLogs.Add(
                                            new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POPRODUCT_SHEET_NAME_TRANSLATE,
                                                Column = "Commodity",
                                                ErrorMsg = "importLog.missingMandatoryField",
                                                Row = row.ToString()
                                            });
                                    }

                                    if ((productVerification.CountryOfOriginVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                        productVerification.CountryOfOriginVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                                        string.IsNullOrWhiteSpace(poLineItemViewModel.CountryCodeOfOrigin))
                                    {
                                        importErrorLogs.Add(
                                            new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POPRODUCT_SHEET_NAME_TRANSLATE,
                                                Column = "CountryCodeOfOrigin",
                                                ErrorMsg = "importLog.missingMandatoryField",
                                                Row = row.ToString()
                                            });
                                    }
                                }
                            }

                            excelPOLineItems = excelPOLineItems.OrderBy(p => p.PONumber).ToList();

                            var groupedExcelPoLineItems = excelPOLineItems.GroupBy(
                                x => x.PONumber,
                                x => x.LineOrder,
                                (key, g) => new
                                {
                                    poNumber = key,
                                    lineOrderCount = g.Count()
                                });

                            foreach (var excelPoLineItem in groupedExcelPoLineItems)
                            {
                                var notEqualNumberOfLineItem = excelPOs.Any(x =>
                                    x.PONumber == excelPoLineItem.poNumber &&
                                    (x.NumberOfLineItems > excelPoLineItem.lineOrderCount || x.NumberOfLineItems < excelPoLineItem.lineOrderCount));

                                if (notEqualNumberOfLineItem)
                                {
                                    importErrorLogs.Add(
                                        new ValidatorErrorInfo
                                        {
                                            SheetName = EXCEL_POPRODUCT_SHEET_NAME_TRANSLATE,
                                            Column = "NumberOfLineItem",
                                            ErrorMsg = $"'{excelPoLineItem.poNumber}':;importLog.invalidNumberOfLineItem"
                                        });
                                }
                            }

                            #endregion

                            // Validate contacts/parties by Compliance
                            var contacts = excelPOContacts.GroupBy(p => p.PONumber, p => p.OrganizationRole,
                                (key, g) => new { PONumber = key, Roles = g.ToList() });

                            foreach (var contact in contacts)
                            {
                                var hasShipper = false;
                                var hasConsignee = false;
                                var hasOriginAgent = false;
                                var hasDestinationAgent = false;
                                var orgCode = GetPrincipalOrgCodeByPONumberFromExcel(contact.PONumber, excelPOContacts);
                                var compliance = GetActivatedComplianceByOrgCode(orgCode, organizations, compliances);
                                if (compliance?.PurchaseOrderVerificationSetting == null) continue;

                                var poVerification = compliance.PurchaseOrderVerificationSetting;
                                foreach (var role in contact.Roles)
                                {
                                    switch (role.ToLower())
                                    {
                                        case "shipper":
                                            hasShipper = true;
                                            break;
                                        case "consignee":
                                            hasConsignee = true;
                                            break;
                                        case "origin agent":
                                            hasOriginAgent = true;
                                            break;
                                        case "destination agent":
                                            hasDestinationAgent = true;
                                            break;
                                    }
                                }

                                if (compliance.IsAssignedAgent && !(hasOriginAgent && hasDestinationAgent))
                                {
                                    importErrorLogs.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = EXCEL_POCONTACT_SHEET_NAME_TRANSLATE,
                                        ObjectName = contact.PONumber,
                                        Column = "PONumber",
                                        ErrorMsg = $"importLog.missingAgent"
                                    });
                                }

                                if ((poVerification.ShipperVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                    poVerification.ShipperVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                                      !hasShipper)
                                {
                                    importErrorLogs.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = EXCEL_POCONTACT_SHEET_NAME_TRANSLATE,
                                        Column = "OrganizationRole",
                                        ErrorMsg = "importLog.missingMandatoryField"
                                    });
                                }

                                if ((poVerification.ConsigneeVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                                    poVerification.ConsigneeVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                                     !hasConsignee)
                                {
                                    importErrorLogs.Add(new ValidatorErrorInfo
                                    {
                                        SheetName = EXCEL_POCONTACT_SHEET_NAME_TRANSLATE,
                                        Column = "OrganizationRole",
                                        ErrorMsg = "importLog.missingMandatoryField"
                                    });
                                }
                            }

                            foreach (var excelPO in excelPOs)
                            {
                                var poExcelContactVMs = excelPOContacts.Where(c => c.PONumber == excelPO.PONumber);
                                var poExcelLineItemVMs = excelPOLineItems.Where(i => i.PONumber == excelPO.PONumber);

                                if (!ValidateExcelPO(excelPO, poExcelContactVMs, poExcelLineItemVMs, organizations, compliances, importErrorLogs))
                                {
                                    continue;
                                }

                                var principalOrganizationCode = poExcelContactVMs
                                    .FirstOrDefault(c =>
                                    c.OrganizationRole.Equals(OrganizationRole.Principal, StringComparison.OrdinalIgnoreCase))
                                    ?.OrganizationCode;

                                var poKey = (principalOrganizationCode + excelPO.PONumber).ToUpper();

                                var poModel = await Repository.GetAsync(p => p.POKey == poKey,
                                    null,
                                    x => x.Include(a => a.Contacts).Include(a => a.LineItems));


                                IEnumerable<PurchaseOrderContactModel> currentDelegation = null;
                                var isNew = poModel == null;
                                if (isNew)
                                {
                                    poModel = new PurchaseOrderModel
                                    {
                                        POKey = poKey,
                                        Status = excelPO.Status ?? PurchaseOrderStatus.Active,
                                        Stage = POStageType.Released
                                    };

                                    poModel.LineItems = Mapper.Map<IList<POLineItemModel>>(poExcelLineItemVMs);
                                }
                                else
                                {
                                    // Keep current delegation if any
                                    currentDelegation = poModel.Contacts.Where(x => x.OrganizationRole.Equals(OrganizationRole.Delegation)).ToList();

                                    var poLineItemViewModels = Mapper.Map<ICollection<POLineItemViewModel>>(poExcelLineItemVMs);
                                    UpdateLineItems(poLineItemViewModels, poModel.LineItems);

                                    // If status column in excel upload file is blank, do not update status of existing PO
                                    if (excelPO.Status.HasValue)
                                    {
                                        if (excelPO.Status == PurchaseOrderStatus.Cancel && poModel.Stage >= POStageType.ShipmentDispatch)
                                        {
                                            importErrorLogs.Add(new ValidatorErrorInfo
                                            {
                                                SheetName = EXCEL_POHEADER_SHEET_NAME_TRANSLATE,
                                                Row = excelPO.Row,
                                                ObjectName = excelPO.PONumber,
                                                Column = "Status",
                                                ErrorMsg = $"importLog.cannotCancelPODueToStage"
                                            });
                                        }
                                        else
                                        {
                                            poModel.Status = excelPO.Status.Value;
                                        }
                                    }
                                }

                                Mapper.Map(excelPO, poModel);
                                poModel.Audit(userName);

                                poModel.Contacts = Mapper.Map<IList<PurchaseOrderContactModel>>(poExcelContactVMs);

                                // Re-add current delegation if any
                                if (currentDelegation != null && currentDelegation.Any())
                                {
                                    poModel.Contacts.AddRange(currentDelegation);
                                }

                                foreach (var lineItem in poModel.LineItems)
                                {
                                    if (isNew)
                                    {
                                        lineItem.BookedUnitQty = 0;
                                    }
                                    lineItem.BalanceUnitQty = lineItem.OrderedUnitQty - lineItem.BookedUnitQty;
                                }

                                await PopulateLocationId(poModel, importErrorLogs);

                                foreach (var contact in poModel.Contacts)
                                {
                                    var organization = organizations.SingleOrDefault(o => o.Code.Trim() == contact.OrganizationCode.Trim());
                                    if (organization == null)
                                    {
                                        if (contact.OrganizationRole.Equals(OrganizationRole.Shipper, StringComparison.OrdinalIgnoreCase)
                                            || contact.OrganizationRole.Equals(OrganizationRole.Supplier, StringComparison.OrdinalIgnoreCase))
                                        {
                                            IQueryable<CustomerRelationshipQueryModel> query;
                                            string sql;
                                            sql = @"
                                                    SELECT SupplierId, CustomerId, CustomerRefId
                                                    FROM CustomerRelationship
                                                    WHERE CustomerRefId = {0}
                                                  ";

                                            query = _dataQuery.GetQueryable<CustomerRelationshipQueryModel>(sql, contact.OrganizationCode);
                                            var customerRef = await query.FirstOrDefaultAsync();
                                            organization = await _csfeApiClient.GetOrganizationByIdAsync(customerRef.SupplierId);
                                        }
                                    }

                                    contact.OrganizationId = organization.Id;
                                    contact.CompanyName = organization.Name;
                                    contact.AddressLine1 = organization.Address;
                                    contact.AddressLine2 = organization.AddressLine2;
                                    contact.AddressLine3 = organization.AddressLine3;
                                    contact.AddressLine4 = organization.AddressLine4;
                                    contact.ContactName = organization.ContactName;
                                    contact.ContactEmail = organization.ContactEmail;
                                    contact.ContactNumber = organization.ContactNumber;
                                }

                                if (isNew)
                                {
                                    importedCreatedPOs.Add(poModel);
                                }
                                else
                                {
                                    importedUpdatedPOs.Add(poModel);
                                }
                            }

                            int successfulCount = importedCreatedPOs.Count() + importedUpdatedPOs.Count();
                            string result = string.Format("{0};importResult.recordsImported", successfulCount);

                            if (importErrorLogs.Any())
                            {
                                string log = JsonConvert.SerializeObject(
                                   importErrorLogs,
                                   new JsonSerializerSettings
                                   {
                                       ContractResolver = new CamelCasePropertyNamesContractResolver()
                                   });

                                await importDataProgressRepository.UpdateStatusAsync(
                                   importDataProgressId,
                                   ImportDataProgressStatus.Failed,
                                   result,
                                   log);
                            }
                            else
                            {
                                await Repository.AddRangeAsync(importedCreatedPOs.ToArray());
                                if (importedUpdatedPOs != null && importedUpdatedPOs.Any())
                                {
                                    foreach (var item in importedUpdatedPOs)
                                    {
                                        var principalOrg = item.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Principal);
                                        if (principalOrg == null)
                                        {
                                            continue;
                                        }
                                        var compliance = GetActivatedComplianceByOrgCode(principalOrg?.OrganizationCode, organizations, compliances);
                                        ProceedPurchaseOrderAdhocChanges(item, compliance.Id, principalOrg.OrganizationId);
                                    }
                                }
                                Repository.UpdateRange(importedUpdatedPOs.ToArray());
                                await UnitOfWork.SaveChangesAsync();
                                await importDataProgressRepository.UpdateStatusAsync(
                                   importDataProgressId,
                                   ImportDataProgressStatus.Success,
                                   result);

                                if (importedCreatedPOs != null && importedCreatedPOs.Any())
                                {
                                    var viewModels = Mapper.Map<IEnumerable<POViewModel>>(excelPOs);
                                    await TriggerEvent1005Async(importedCreatedPOs, viewModels);
                                    BackgroundJob.Enqueue<MissingPONotificationJob>(c => c.ExecuteAsync(importedCreatedPOs.Select(c => c.Id).ToList()));
                                }

                                if (importedUpdatedPOs != null && importedUpdatedPOs.Any())
                                {
                                    var viewModels = Mapper.Map<IEnumerable<POViewModel>>(excelPOs);
                                    await TriggerEvent1005Async(importedUpdatedPOs, viewModels);
                                }
                            }
                        }
                    }
                }
                catch (InvalidDataException ex)
                {
                    await importDataProgressRepository.UpdateStatusAsync(
                        importDataProgressId,
                        ImportDataProgressStatus.Failed,
                        "importResult.invalidExcelFile",
                        ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot import PO from Excel", ex.Message);
                    await importDataProgressRepository.UpdateStatusAsync(
                        importDataProgressId,
                        ImportDataProgressStatus.Failed,
                        "importResult.cannotImport",
                         AppException.GetTrueExceptionMessage(ex));
                }
            }
        }

        private async Task TriggerEvent1005Async(IEnumerable<PurchaseOrderModel> pos, IEnumerable<POViewModel> viewModels, string userName = null)
        {
            try
            {
                foreach (var viewModel in viewModels)
                {
                    if (viewModel.Status == PurchaseOrderStatus.Cancel)
                    {
                        var po = pos.SingleOrDefault(c => c.PONumber.ToLower() == viewModel.PONumber.ToLower());
                        var event1005 = new ActivityViewModel()
                        {
                            ActivityCode = Event.EVENT_1005,
                            PurchaseOrderId = po.Id,
                            ActivityDate = po.UpdatedDate ?? DateTime.UtcNow,
                            CreatedBy = userName ?? AppConstant.SYSTEM_USERNAME
                        };

                        await _activityService.TriggerAnEvent(event1005);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// To manage purchase order adhoc changes
        /// </summary>
        /// <param name="purchaseOrder">A new value of PO, including contact and line items</param>
        /// <param name="buyerComplianceId">Buyer compliance id of PO. If zero, get from PO in SP</param>
        /// <param name="principalOrganizationId">Principal organization id for PO. If zero, get from PO in SP</param>
        private void ProceedPurchaseOrderAdhocChanges(PurchaseOrderModel purchaseOrder, long buyerComplianceId = 0, long principalOrganizationId = 0)
        {
            // It is calling a Stored Procedure to insert add-hoc changes into table PurchaseOrderAdhocChanges

            if (purchaseOrder == null || purchaseOrder.Id == 0)
            {
                return;
            }

            // It is new value of purchase order including contacts and line items.
            var jsonNewValue = JsonConvert.SerializeObject(purchaseOrder, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            // [dbo].[spu_ProceedPurchaseOrderAdhocChanges]
            // @PurchaseOrderId BIGINT,
            // @BuyerComplianceId BIGINT = 0,
            // @PrincipalOrganizationId BIGINT = 0,
            // @JsonNewValue NVARCHAR(MAX)
            var sql = @"spu_ProceedPurchaseOrderAdhocChanges 
                        @p0,
	                    @p1,
	                   	@p2,
                        @p3";
            var parameters = new object[]
            {
                purchaseOrder.Id,
                buyerComplianceId,
                principalOrganizationId,
                jsonNewValue
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());
        }

        private bool ValidateExcelPO(ExcelPOViewModel excelPO,
            IEnumerable<ExcelPOContactViewModel> poContactViewModels,
            IEnumerable<ExcelPOLineItemViewModel> poLineItemViewModels,
            IList<Infrastructure.CSFE.Models.Organization> organizations,
            IList<BuyerComplianceModel> compliances,
            IList<ValidatorErrorInfo> importErrorLogs
            )
        {
            bool valid = true;
            if (!poContactViewModels.Any())
            {
                importErrorLogs.Add(
                    new ValidatorErrorInfo
                    {
                        SheetName = EXCEL_POCONTACT_SHEET_NAME_TRANSLATE,
                        ObjectName = excelPO.PONumber,
                        ErrorMsg = "importLog.atLeastOnePartyAddress"
                    });
                valid = false;
            }
            else
            {
                var principal = poContactViewModels.FirstOrDefault(c => OrganizationRole.Principal.Equals(c.OrganizationRole, StringComparison.OrdinalIgnoreCase));
                if (principal == null)
                {
                    importErrorLogs.Add(
                        new ValidatorErrorInfo
                        {
                            SheetName = EXCEL_POHEADER_SHEET_NAME_TRANSLATE,
                            Row = excelPO.Row,
                            ObjectName = excelPO.PONumber,
                            ErrorMsg = "importLog.poContactMissingPrincipal"
                        });
                    valid = false;
                }
                else
                {
                    var compliance = GetActivatedComplianceByOrgCode(principal.OrganizationCode, organizations, compliances);
                    if (compliance == null)
                    {
                        importErrorLogs.Add(
                            new ValidatorErrorInfo
                            {
                                SheetName = EXCEL_POCONTACT_SHEET_NAME_TRANSLATE,
                                ObjectName = principal.PONumber,
                                Row = principal.Row,
                                ErrorMsg = "importLog.principalMissingCompliance"
                            });
                        valid = false;
                    }
                }

                if (!poContactViewModels.Any(c => OrganizationRole.Supplier.Equals(c.OrganizationRole, StringComparison.OrdinalIgnoreCase)))
                {
                    importErrorLogs.Add(
                        new ValidatorErrorInfo
                        {
                            SheetName = EXCEL_POHEADER_SHEET_NAME_TRANSLATE,
                            Row = excelPO.Row,
                            ObjectName = excelPO.PONumber,
                            ErrorMsg = "importLog.poContactMissingSupplier"
                        });
                    valid = false;
                }
            }


            if (!poLineItemViewModels.Any())
            {
                importErrorLogs.Add(
                    new ValidatorErrorInfo
                    {
                        SheetName = EXCEL_POPRODUCT_SHEET_NAME_TRANSLATE,
                        ObjectName = excelPO.PONumber,
                        ErrorMsg = "importLog.atLeastOneProduct"
                    });
                valid = false;
            }

            return valid;
        }

        private async Task PopulateLocationId(PurchaseOrderModel poModel, List<ValidatorErrorInfo> errorInfo = null)
        {
            var locations = await _csfeApiClient.GetAllLocationsAsync();
            var shipFrom = locations.SingleOrDefault(l => l.Name.Equals(poModel.ShipFrom, StringComparison.OrdinalIgnoreCase));
            var shipTo = locations.SingleOrDefault(l => l.Name.Equals(poModel.ShipTo, StringComparison.OrdinalIgnoreCase));

            if (shipFrom == null || shipTo == null)
            {
                var alternativeLocations = await _csfeApiClient.GetAllAlternativeLocationsAsync();
                if (shipFrom == null)
                {
                    var sf = alternativeLocations.SingleOrDefault(x =>
                        x.Name.Equals(poModel.ShipFrom, StringComparison.OrdinalIgnoreCase));

                    if (sf != null)
                    {
                        poModel.ShipFromId = sf.LocationId;

                        var loc = locations.SingleOrDefault(x => x.Id == sf.LocationId);
                        poModel.ShipFrom = loc?.LocationDescription;
                    }
                    else if (!string.IsNullOrWhiteSpace(poModel.ShipFrom))
                    {
                        if (errorInfo != null)
                        {
                            errorInfo.Add(
                                new ValidatorErrorInfo
                                {
                                    SheetName = EXCEL_POHEADER_SHEET_NAME_TRANSLATE,
                                    ObjectName = poModel.PONumber,
                                    ErrorMsg = "'ShipFrom':;importLog.inputtedDataNotExisting"
                                });
                        }
                        else
                        {

                            throw new Exception("'ShipFrom': Inputted data is not existing in system.");
                        }
                    }
                }
                else
                {
                    poModel.ShipFromId = shipFrom.Id;
                    poModel.ShipFrom = shipFrom.LocationDescription;
                }

                if (shipTo == null)
                {
                    var st = alternativeLocations.SingleOrDefault(x =>
                        x.Name.Equals(poModel.ShipTo, StringComparison.OrdinalIgnoreCase));

                    if (st != null)
                    {
                        poModel.ShipToId = st.LocationId;

                        var loc = locations.SingleOrDefault(x => x.Id == st.LocationId);
                        poModel.ShipTo = loc?.LocationDescription;
                    }
                    else if (!string.IsNullOrWhiteSpace(poModel.ShipTo))
                    {
                        if (errorInfo != null)
                        {
                            errorInfo.Add(
                                new ValidatorErrorInfo
                                {
                                    SheetName = EXCEL_POHEADER_SHEET_NAME_TRANSLATE,
                                    ObjectName = poModel.PONumber,
                                    ErrorMsg = "'ShipTo':;importLog.inputtedDataNotExisting"
                                });
                        }
                        else
                        {
                            throw new Exception("'ShipTo': Inputted data is not existing in system.");
                        }
                    }
                }
                else
                {
                    poModel.ShipToId = shipTo.Id;
                    poModel.ShipTo = shipTo.LocationDescription;
                }
            }
            else
            {
                poModel.ShipFromId = shipFrom.Id;
                poModel.ShipToId = shipTo.Id;
                poModel.ShipFrom = shipFrom.LocationDescription;
                poModel.ShipTo = shipTo.LocationDescription;
            }
        }

        public async Task<IEnumerable<PurchaseOrderViewModel>> CreateAsync(IEnumerable<CreatePOViewModel> model, string userName)
        {
            var pos = new List<PurchaseOrderModel>();
            if (model.Count() > 500)
            {
                throw new Exception("You can import 500 Purchase Orders once.");
            }

            if (model.GroupBy(x => x.POKey).Any(g => g.Count() > 1)) throw new Exception($"Duplicate '{nameof(CreatePOViewModel.POKey)}'.");

            foreach (var item in model)
            {
                var po = Mapper.Map<PurchaseOrderModel>(item);
                po.Status = PurchaseOrderStatus.Active;
                po.Stage = POStageType.Released;

                if (!string.IsNullOrWhiteSpace(item.BlanketPOKey))
                {
                    po.POType = POType.Allocated;
                    await UpdateBlanketPOAsync(po, item.BlanketPOKey);
                }
                await PopulateLocationId(po);
                await PopulatePOContacts(po);
                await PopulateLineItems(po);
                await ValidateByComplianceAsync(po);

                po.Audit(userName);
                pos.Add(po);
            }
            var warehousePOList = await PopulateWarehouseProviderContactAsync(pos);
            await Repository.AddRangeAsync(pos.ToArray());
            await this.UnitOfWork.SaveChangesAsync();

            if (warehousePOList != null)
            {
                await LinkPOsToWarehouseBookingSilentAsync(warehousePOList.ToList());
            }
            return Mapper.Map<IEnumerable<PurchaseOrderViewModel>>(pos);
        }

        public async Task<PurchaseOrderViewModel> CreateAsync(CreatePOViewModel model, string userName)
        {
            if (string.IsNullOrWhiteSpace(model.CreatedBy))
            {
                model.CreatedBy = "System";
            }

            var po = Mapper.Map<PurchaseOrderModel>(model);
            po.Status = model.Status ?? PurchaseOrderStatus.Active;
            po.Stage = POStageType.Released;

            if (!string.IsNullOrWhiteSpace(model.BlanketPOKey))
            {
                po.POType = POType.Allocated;
                await UpdateBlanketPOAsync(po, model.BlanketPOKey);
            }

            await PopulateLocationId(po);
            await PopulatePOContacts(po);
            await PopulateLineItems(po);
            await ValidateByComplianceAsync(po);

            var warehousePOList = await PopulateWarehouseProviderContactAsync(new List<PurchaseOrderModel> { po });

            // Not audit purchase order because audit information will get from API
            var createdDate = DateTime.UtcNow;
            var createdBy = AppConstant.SYSTEM_USERNAME;
            foreach (var item in po.Contacts)
            {
                item.CreatedBy = createdBy;
                item.CreatedDate = createdDate;
                item.UpdatedBy = createdBy;
                item.UpdatedDate = createdDate;
            }

            foreach (var item in po.LineItems)
            {
                item.CreatedBy = createdBy;
                item.CreatedDate = createdDate;
                item.UpdatedBy = createdBy;
                item.UpdatedDate = createdDate;
            }

            await Repository.AddAsync(po);
            await this.UnitOfWork.SaveChangesAsync();

            if (warehousePOList != null)
            {
                await LinkPOsToWarehouseBookingSilentAsync(warehousePOList.ToList());
            }

            await TriggerEvent1005Async(new List<PurchaseOrderModel>() { po }, Mapper.Map<IEnumerable<POViewModel>>(new List<CreatePOViewModel>() { model }), po.UpdatedBy);

            _queuedBackgroundJobs.Enqueue<MissingPONotificationJob>(c => c.ExecuteAsync(new List<long>() { po.Id }));
            return Mapper.Map<PurchaseOrderViewModel>(po);
        }

        public async Task<PurchaseOrderViewModel> UpdateAsync(string poKey, UpdatePOViewModel model)
        {
            PurchaseOrderModel po = await Repository.GetAsync(p => p.POKey == poKey, null, FullIncludeProperties);

            if (po == null)
            {
                throw new AppEntityNotFoundException($"Object with the key {string.Join(", ", poKey)} not found!");
            }

            if (!string.IsNullOrWhiteSpace(model.BlanketPOKey) && po.POType == POType.Blanket)
            {
                throw new AppValidationException("Blanket PO cannot be allocated to another Blanket PO");
            }

            if (string.IsNullOrWhiteSpace(model.CreatedBy))
            {
                model.CreatedBy = "System";
            }

            if (model.IsPropertyDirty(nameof(model.Status)))
            {
                if (model.Status == PurchaseOrderStatus.Cancel && po.Stage >= POStageType.ShipmentDispatch)
                {
                    throw new AppValidationException("NOT allow to cancel when PO stage from Shipment Dispatch.");
                }
            }

            // Keep current delegation if any
            var currentDelegation = po.Contacts.Where(x => x.OrganizationRole.Equals(OrganizationRole.Delegation)).ToList();

            Mapper.Map(model, po);
            await PopulateLocationId(po);
            await PopulatePOContacts(po);

            // Re-add current delegation if any
            if (currentDelegation != null && currentDelegation.Any())
            {
                po.Contacts.AddRange(currentDelegation);
            }


            #region Update line items
            if (model.IsPropertyDirty("LineItems"))
            {
                UpdateLineItems(model.LineItems, po.LineItems);
                await PopulateLineItems(po);
            }
            #endregion

            #region Update PO Relationships
            if (model.IsPropertyDirty("BlanketPOKey"))
            {
                po.POType = POType.Allocated;
                await UpdateBlanketPOAsync(po, model.BlanketPOKey);
            }
            #endregion

            var warehousePOList = await PopulateWarehouseProviderContactAsync(new List<PurchaseOrderModel> { po });

            await ValidateByComplianceAsync(po);

            // Proceed purchase order adhoc changes
            ProceedPurchaseOrderAdhocChanges(po);

            // Not audit purchase order because audit information will get from API
            var updatedDate = DateTime.UtcNow;
            var updatedBy = AppConstant.SYSTEM_USERNAME;
            foreach (var item in po.Contacts)
            {
                // Only fulfill CreatedBy/CreatedDate if adding new
                // Else, ignore, not touch -> may impact to existing data
                if (item.Id == 0)
                {
                    item.CreatedBy = updatedBy;
                    item.CreatedDate = updatedDate;
                }

                item.UpdatedBy = updatedBy;
                item.UpdatedDate = updatedDate;
            }

            foreach (var item in po.LineItems)
            {
                // Only fulfill CreatedBy/CreatedDate if adding new
                // Else, ignore, not touch -> may impact to existing data
                if (item.Id == 0)
                {
                    item.CreatedBy = updatedBy;
                    item.CreatedDate = updatedDate;
                }

                item.UpdatedBy = updatedBy;
                item.UpdatedDate = updatedDate;
            }

            Repository.Update(po);
            await this.UnitOfWork.SaveChangesAsync();

            if (warehousePOList != null)
            {
                await LinkPOsToWarehouseBookingSilentAsync(warehousePOList.ToList());
            }

            if (!model.IsPropertyDirty(nameof(model.PONumber)))
            {
                model.PONumber = poKey;
            }
            await TriggerEvent1005Async(new List<PurchaseOrderModel>() { po }, Mapper.Map<IEnumerable<POViewModel>>(new List<UpdatePOViewModel>() { model }), po.UpdatedBy);

            return Mapper.Map<PurchaseOrderViewModel>(po);
        }

        public async Task<IEnumerable<PurchaseOrderViewModel>> UpdatePOProgressCheckRangeAsync(IEnumerable<PurchaseOrderProgressCheckViewModel> viewModel, IdentityInfo currentUser)
        {
            var POListModel = await Repository.Query(x
                => viewModel.Select(y => y.Id).Contains(x.Id)).ToListAsync();

            UnitOfWork.BeginTransaction();

            var newNotes = new List<NoteModel>();
            foreach (var POModel in POListModel)
            {
                var POViewModel = viewModel.SingleOrDefault(x => x.Id == POModel.Id);
                Mapper.Map(POViewModel, POModel);
                POModel.Audit(currentUser.Username);

                // Create new Note
                NoteModel note = new NoteModel()
                {
                    GlobalObjectId = CommonHelper.GenerateGlobalId(POModel.Id, EntityType.CustomerPO),
                    NoteText = $"Production Started: {(POModel.ProductionStarted ? "Yes" : "No")}" +
                               $"{(POModel.ProposeDate.HasValue ? $"\nPropose CRD: {POModel.ProposeDate.Value.ToString("MM/dd/yyyy")}" : "")}" +
                               $"{(POModel.QCRequired ? $"\nQC Required: {(POModel.QCRequired ? "Yes" : "No")}" : "")}" +
                               $"{(POModel.ShortShip ? $"\nShort-ship: {(POModel.ShortShip ? "Yes" : "No")}" : "")}" +
                               $"{(POModel.SplitShipment ? $"\nSplit shipment: {(POModel.SplitShipment ? "Yes" : "No")}" : "")}" +
                               $"\n{POModel.Remark}",
                    Category = DialogCategory.General,
                    Owner = currentUser.Name
                };
                note.Audit(currentUser.Username);
                newNotes.Add(note);

                // Trigger event 1013 - PA - Progress Check 
                if (POModel.ProductionStarted)
                {
                    var event1013 = new ActivityViewModel()
                    {
                        ActivityCode = Event.EVENT_1013,
                        PurchaseOrderId = POModel.Id,
                        ActivityDate = DateTime.UtcNow,
                        CreatedBy = currentUser.Username
                    };
                    await _activityService.TriggerAnEvent(event1013);
                }
            }
            await _noteRepository.AddRangeAsync(newNotes.ToArray());
            await UnitOfWork.SaveChangesAsync();

            UnitOfWork.CommitTransaction();
            return Mapper.Map<IEnumerable<PurchaseOrderViewModel>>(POListModel);
        }

        private BuyerComplianceModel GetActivatedComplianceByOrgCode(
            string orgCode,
            IEnumerable<Infrastructure.CSFE.Models.Organization> organizations,
            IEnumerable<BuyerComplianceModel> compliances)
        {
            var orgId = organizations.Where(o => o.Code == orgCode).Select(o => o.Id).FirstOrDefault();
            var compliance = compliances.FirstOrDefault(c => c.OrganizationId == orgId && c.Stage == BuyerComplianceStage.Activated);

            return compliance;
        }

        private string GetPrincipalOrgCodeByPONumberFromExcel(string poNumber, IEnumerable<ExcelPOContactViewModel> contacts)
        {
            var orgCode = contacts.Where(c => c.PONumber == poNumber && c.OrganizationRole.ToLower() == OrganizationRole.Principal.ToLower())
                .Select(c => c.OrganizationCode)
                .FirstOrDefault();

            return orgCode;
        }

        private long GetPrincipalOrgIdFromAPIModel(IEnumerable<PurchaseOrderContactModel> contacts)
        {
            var orgId = contacts.Where(c => c.OrganizationRole.ToLower() == OrganizationRole.Principal.ToLower())
                .Select(c => c.OrganizationId)
                .FirstOrDefault();

            return orgId;
        }

        private async Task UpdateBlanketPOAsync(PurchaseOrderModel po, string blanketPOKey)
        {
            var blanketPO = await Repository.GetAsync(p => p.POKey == blanketPOKey);
            if (blanketPO != null)
            {
                blanketPO.POType = POType.Blanket;
                po.BlanketPOId = blanketPO.Id;
            }
        }

        private async Task ValidateByComplianceAsync(PurchaseOrderModel po)
        {
            var orgId = GetPrincipalOrgIdFromAPIModel(po.Contacts);
            var compliance =
                (await _buyerComplianceService.GetListByOrgIdsAsync(new List<long> { orgId })).FirstOrDefault();

            if (compliance?.Stage != BuyerComplianceStage.Activated)
            {
                throw new AppValidationException("Principal has no Compliance.");
            }

            if (compliance.PurchaseOrderVerificationSetting != null)
            {
                var poVerification = compliance.PurchaseOrderVerificationSetting;
                var hasShipper = false;
                var hasConsignee = false;
                var hasOriginAgent = false;
                var hasDestinationAgent = false;
                foreach (var contact in po.Contacts)
                {
                    switch (contact.OrganizationRole.ToLower())
                    {
                        case "shipper":
                            hasShipper = true;
                            break;
                        case "consignee":
                            hasConsignee = true;
                            break;
                        case "origin agent":
                            hasOriginAgent = true;
                            break;
                        case "destination agent":
                            hasDestinationAgent = true;
                            break;
                    }
                }

                if (compliance.IsAssignedAgent && !(hasOriginAgent && hasDestinationAgent))
                {
                    {
                        throw new Exception($"{po.PONumber}: Origin Agent, Destination Agent are required.");
                    }
                }

                if ((poVerification.ShipperVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                    poVerification.ShipperVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                      !hasShipper)
                {
                    throw new Exception("Shipper is missing.");
                }

                if ((poVerification.ConsigneeVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                    poVerification.ConsigneeVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                      !hasConsignee)
                {
                    throw new Exception("Consignee is missing.");
                }

                if ((poVerification.ExpectedShipDateVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                    poVerification.ExpectedShipDateVerification == VerificationSettingType.AsPerPOAllowOverride ||
                    poVerification.ExpectedShipDateVerification == VerificationSettingType.AsPerPODefault) &&
                    po.ExpectedShipDate == null)
                {
                    throw new Exception("ExpectedShipDate must not be empty.");
                }

                if ((poVerification.ExpectedDeliveryDateVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                    poVerification.ExpectedDeliveryDateVerification == VerificationSettingType.AsPerPOAllowOverride ||
                    poVerification.ExpectedDeliveryDateVerification == VerificationSettingType.AsPerPODefault) &&
                    po.ExpectedDeliveryDate == null)
                {
                    throw new Exception("ExpectedDeliveryDate must not be empty.");
                }

                if ((poVerification.ShipFromLocationVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                    poVerification.ShipFromLocationVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                    string.IsNullOrWhiteSpace(po.ShipFrom))
                {
                    throw new Exception("ShipFrom must not be empty.");
                }

                if ((poVerification.ShipToLocationVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                    poVerification.ShipToLocationVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                    string.IsNullOrWhiteSpace(po.ShipTo))
                {
                    throw new Exception("ShipTo must not be empty.");
                }

                if ((poVerification.PaymentTermsVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                    poVerification.PaymentTermsVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                    string.IsNullOrWhiteSpace(po.PaymentTerms))
                {
                    throw new Exception("PaymentTerms must not be empty.");
                }

                if ((poVerification.PaymentCurrencyVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                    poVerification.PaymentCurrencyVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                    string.IsNullOrWhiteSpace(po.PaymentCurrencyCode))
                {
                    throw new Exception("PaymentCurrencyCode must not be empty.");
                }

                if ((poVerification.ModeOfTransportVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                    poVerification.ModeOfTransportVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                    string.IsNullOrWhiteSpace(po.ModeOfTransport))
                {
                    throw new Exception("ModeOfTransport must not be empty.");
                }

                if (poVerification.IncotermVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                    poVerification.IncotermVerification == VerificationSettingType.AsPerPOAllowOverride)
                {
                    if (string.IsNullOrWhiteSpace(po.Incoterm))
                    {
                        throw new Exception("Incoterm must not be empty.");
                    }
                }

                if ((poVerification.PreferredCarrierVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                    poVerification.PreferredCarrierVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                    string.IsNullOrWhiteSpace(po.CarrierCode))
                {
                    throw new Exception("CarrierCode must not be empty.");
                }

                var productVerification = compliance.ProductVerificationSetting;
                foreach (var item in po.LineItems)
                {
                    if ((productVerification.ProductCodeVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                        productVerification.ProductCodeVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                        string.IsNullOrWhiteSpace(item.ProductCode))
                    {
                        throw new Exception("ProductCode must not be empty.");
                    }

                    if ((productVerification.CommodityVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                        productVerification.CommodityVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                        string.IsNullOrWhiteSpace(item.Commodity))
                    {
                        throw new Exception("Commodity must not be empty.");
                    }

                    if ((productVerification.HsCodeVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                        productVerification.HsCodeVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                        string.IsNullOrWhiteSpace(item.HSCode))
                    {
                        throw new Exception("HSCode must not be empty.");
                    }

                    if ((productVerification.CountryOfOriginVerification == VerificationSettingType.AsPerPONotAllowOverride ||
                        productVerification.CountryOfOriginVerification == VerificationSettingType.AsPerPOAllowOverride) &&
                       string.IsNullOrWhiteSpace(item.CountryCodeOfOrigin))
                    {
                        throw new Exception("CountryCodeOfOrigin must not be empty.");
                    }
                }
            }
        }

        public async Task DelegateAsync(long id, DelegationPOViewModel model)
        {
            Func<IQueryable<PurchaseOrderModel>, IQueryable<PurchaseOrderModel>> includeProperties = x
            => x.Include(m => m.Contacts)
                .Include(m => m.AllocatedPOs)
                .ThenInclude(m => m.Contacts);

            var po = await Repository.GetAsync(p => p.Id == id && p.POType != POType.Allocated, null, includeProperties);

            if (po == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", id)} not found!");
            }

            if (po.Stage != POStageType.Released)
            {
                throw new AppValidationException("Cannot delegate when PO Stage is not released.");
            }

            if (po.POType == POType.Allocated)
            {
                throw new AppValidationException("Cannot delegate when PO Type is allocated.");
            }

            var oldDelegationParty = po.Contacts.FirstOrDefault(x => x.OrganizationRole.Equals(OrganizationRole.Delegation, StringComparison.OrdinalIgnoreCase));
            var oldDelegationUserId = po.NotifyUserId;
            po.Audit(model.UpdatedBy);
            po.NotifyUserId = model.NotifyUserId;

            if (oldDelegationParty != null)
            {
                po.Contacts.Remove(oldDelegationParty);
            }

            var organization = await _csfeApiClient.GetOrganizationByIdAsync(model.OrganizationId);
            var delegationContact = new PurchaseOrderContactModel
            {
                OrganizationRole = OrganizationRole.Delegation,
                OrganizationId = model.OrganizationId,
                OrganizationCode = organization.Code,
                CompanyName = organization.Name,
                AddressLine1 = organization.Address,
                ContactName = organization.ContactName,
                ContactNumber = organization.ContactNumber,
                ContactEmail = organization.ContactEmail
            };

            delegationContact.Audit(model.UpdatedBy);
            po.Contacts.Add(delegationContact);

            if (model.NotifyUserId.HasValue)
            {
                await SendEmailToNotifyUserAsync(model.NotifyUserId.Value, po);
            }

            if (oldDelegationParty != null && oldDelegationUserId.HasValue)
            {
                var parentOrg = po.Contacts.FirstOrDefault(x => x.OrganizationRole.Equals(OrganizationRole.Supplier, StringComparison.OrdinalIgnoreCase));
                var notifyUser = await _userProfileService.GetAsync(oldDelegationUserId.Value);

                _queuedBackgroundJobs.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync(
                    $"Purchase Order #{po.Id} has been removed from Organization #{notifyUser.OrganizationId}",
                    "PODelegationAccessRemovedNotification", new PODelegationEmailTemplateViewModel()
                    {
                        Name = notifyUser.Name,
                        PONumber = po.PONumber,
                        CompanyName = parentOrg.CompanyName,
                        SupportEmail = _appConfig.SupportEmail
                    }, notifyUser.Email, $"Shipment Portal: Purchase Order {po.PONumber} has been removed from your access")
                );

                // Send push notification
                await _notificationService.PushNotificationSilentAsync(notifyUser.OrganizationId ?? 0, new NotificationViewModel
                {
                    MessageKey = $"~notification.msg.poNo~ <span class=\"k-link\">{po.PONumber}</span> ~notification.msg.hasBeenRemovedYourAccess~.",
                    ReadUrl = $"/purchase-orders/{po.Id}"
                });
            }

            // Set Delegation for Child PO (Allocated PO).

            if (po.POType == POType.Blanket)
            {
                foreach (var allocatedPO in po.AllocatedPOs)
                {
                    allocatedPO.NotifyUserId = po.NotifyUserId;
                    allocatedPO.Audit(model.UpdatedBy);

                    var allocatedPODelegation = allocatedPO.Contacts.FirstOrDefault(x => x.OrganizationRole.Equals(OrganizationRole.Delegation, StringComparison.OrdinalIgnoreCase));
                    if (allocatedPODelegation != null)
                    {
                        allocatedPO.Contacts.Remove(allocatedPODelegation);
                    }
                    var allocatedPODelegationContact = new PurchaseOrderContactModel
                    {
                        OrganizationRole = OrganizationRole.Delegation,
                        OrganizationId = model.OrganizationId,
                        OrganizationCode = organization.Code,
                        CompanyName = organization.Name,
                        AddressLine1 = organization.Address,
                        ContactName = organization.ContactName,
                        ContactNumber = organization.ContactNumber,
                        ContactEmail = organization.ContactEmail
                    };
                    allocatedPODelegationContact.Audit(model.UpdatedBy);
                    allocatedPO.Contacts.Add(allocatedPODelegationContact);
                }
            }

            await this.UnitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(string poKey)
        {
            PurchaseOrderModel po = await Repository.GetAsync(p => p.POKey == poKey);

            Repository.Remove(po);
            await this.UnitOfWork.SaveChangesAsync();
        }

        public async Task SendEmailToNotifyUserAsync(long notifyUserId, PurchaseOrderModel purchaseOrder)
        {
            var notifyUser = await _userProfileService.GetAsync(notifyUserId);
            var parentOrg = purchaseOrder.Contacts.FirstOrDefault(x => x.OrganizationRole.Equals(OrganizationRole.Supplier, StringComparison.OrdinalIgnoreCase));
            _queuedBackgroundJobs.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync(
                $"Purchase Order #{purchaseOrder.Id} has been delegated to Organization #{notifyUser.OrganizationId}",
                "PODelegationNotifyUser", new PODelegationEmailTemplateViewModel()
                {
                    Name = notifyUser.Name,
                    PONumber = purchaseOrder.PONumber,
                    CompanyName = parentOrg.CompanyName,
                    DetailPage = $"{_appConfig.ClientUrl}/purchase-orders/{purchaseOrder.Id}",
                    SupportEmail = _appConfig.SupportEmail
                }, notifyUser.Email, $"Shipment Portal: Purchase Order {purchaseOrder.PONumber} has been delegated to you")
            );

            // Send push notification
            await _notificationService.PushNotificationSilentAsync(notifyUser.OrganizationId ?? 0, new NotificationViewModel
            {
                MessageKey = $"~notification.msg.poNo~ <span class=\"k-link\">{purchaseOrder.PONumber}</span> ~notification.msg.hasBeenDelegatedToYou~.",
                ReadUrl = $"/purchase-orders/{purchaseOrder.Id}"
            });
        }

        private async Task PopulateLineItems(PurchaseOrderModel po)
        {
            var poFulfillments = await _poFulfillmentRepository
               .Query(f => f.Status == POFulfillmentStatus.Active &&
                   f.Orders.Any(o => o.PurchaseOrderId == po.Id),
                   null,
                   i => i.Include(f => f.Orders)).ToListAsync();

            var poFulfillmentOrders = poFulfillments
                .SelectMany(p => p.Orders)
                .Where(o => o.PurchaseOrderId == po.Id);

            var principalOrgCode = po.Contacts.Where(x => x.OrganizationRole.ToLower() == OrganizationRole.Principal.ToLower())
                                              .Select(x => x.OrganizationCode)
                                              .FirstOrDefault();

            foreach (var lineItem in po.LineItems)
            {
                if (string.IsNullOrEmpty(lineItem.ProductCode))
                {
                    lineItem.ProductCode = lineItem.LineOrder.ToString();
                }

                // Trim product code for TUMI only
                if (!string.IsNullOrWhiteSpace(principalOrgCode) && principalOrgCode.ToLower() == _customerOrgReference.TUMIOrgCode.ToLower())
                {
                    // Trim leading zeros
                    lineItem.ProductCode = lineItem.ProductCode?.TrimStart('0');
                }

                if (po.Id <= 0)
                {
                    lineItem.BookedUnitQty = 0;
                }
                else
                {
                    var poFulfillmentOrder = poFulfillmentOrders
                        .FirstOrDefault(o => o.ProductCode == lineItem.ProductCode);

                    if (poFulfillmentOrder == null)
                    {
                        lineItem.BookedUnitQty = 0;
                    }
                }

                lineItem.BalanceUnitQty = lineItem.OrderedUnitQty - lineItem.BookedUnitQty;
            }
        }

        private void UpdateLineItems(ICollection<POLineItemViewModel> poLineItemVMs, ICollection<POLineItemModel> poLineItems)
        {
            var deletedLineItems = new List<POLineItemModel>();

            foreach (var lineItem in poLineItems)
            {
                var lineItemVM = poLineItemVMs
                    .FirstOrDefault(x => x.POLineKey == lineItem.POLineKey);

                if (lineItemVM == null)
                {
                    deletedLineItems.Add(lineItem);
                }
                else
                {
                    lineItem.LineOrder = lineItemVM.LineOrder;
                    lineItem.OrderedUnitQty = lineItemVM.OrderedUnitQty.Value;
                    lineItem.ProductCode = lineItemVM.ProductCode;
                    lineItem.ProductName = lineItemVM.ProductName;
                    lineItem.UnitUOM = lineItemVM.UnitUOM;
                    lineItem.UnitPrice = lineItemVM.UnitPrice.Value;
                    lineItem.CurrencyCode = lineItemVM.CurrencyCode;
                    lineItem.ProductFamily = lineItemVM.ProductFamily;
                    lineItem.HSCode = lineItemVM.HSCode;
                    lineItem.SupplierProductCode = lineItemVM.SupplierProductCode;
                    lineItem.MinPackageQty = lineItemVM.MinPackageQty;
                    lineItem.MinOrderQty = lineItemVM.MinOrderQty;
                    lineItem.PackageUOM = lineItemVM.PackageUOM;
                    lineItem.CountryCodeOfOrigin = lineItemVM.CountryCodeOfOrigin;
                    lineItem.Commodity = lineItemVM.Commodity;
                    lineItem.ReferenceNumber1 = lineItemVM.ReferenceNumber1;
                    lineItem.ReferenceNumber2 = lineItemVM.ReferenceNumber2;
                    lineItem.ShippingMarks = lineItemVM.ShippingMarks;
                    lineItem.DescriptionOfGoods = lineItemVM.DescriptionOfGoods;
                    lineItem.PackagingInstruction = lineItemVM.PackagingInstruction;
                    lineItem.ProductRemark = lineItemVM.ProductRemark;
                    lineItem.StyleNo = lineItemVM.StyleNo;
                    lineItem.ColourCode = lineItemVM.ColourCode;
                    lineItem.Size = lineItemVM.Size;
                    lineItem.SeasonCode = lineItemVM.SeasonCode;
                    lineItem.Volume = lineItemVM.Volume;
                    lineItem.GrossWeight = lineItemVM.GrossWeight;

                    // For TUMI customer
                    lineItem.ScheduleLineNo = lineItemVM.ScheduleLineNo;
                    lineItem.InboundDelivery = lineItemVM.InboundDelivery;
                    lineItem.POItemReference = lineItemVM.POItemReference;
                    lineItem.ShipmentNo = lineItemVM.ShipmentNo;
                    lineItem.Plant = lineItemVM.Plant;
                    lineItem.StorageLocation = lineItemVM.StorageLocation;
                    lineItem.MatGrpDe = lineItemVM.MatGrpDe;
                    lineItem.MaterialType = lineItemVM.MaterialType;
                    lineItem.Sku = lineItemVM.Sku;
                    lineItem.GridValue = lineItemVM.GridValue;
                    lineItem.StockCategory = lineItemVM.StockCategory;
                    lineItem.HeaderText = lineItemVM.HeaderText;
                    lineItem.Length = lineItemVM.Length;
                    lineItem.Width = lineItemVM.Width;
                    lineItem.Height = lineItemVM.Height;
                    lineItem.NetWeight = lineItemVM.NetWeight;
                    lineItem.FactoryName = lineItemVM.FactoryName;

                    poLineItemVMs.Remove(lineItemVM);
                }
            }

            foreach (var deletedLineItem in deletedLineItems)
            {
                poLineItems.Remove(deletedLineItem);
            }

            IList<POLineItemModel> newLineItems = Mapper.Map<IList<POLineItemModel>>(poLineItemVMs);
            foreach (var newLineItem in newLineItems)
            {
                poLineItems.Add(newLineItem);
            }
        }

        public async Task<POLineItemArticleMasterViewModel> GetInformationFromArticleMaster(long purchaseOrderId, string productCode)
        {

            var po = await Repository.GetAsync(p => p.Id == purchaseOrderId, null, x => x.Include(i => i.Contacts));

            if (po == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", purchaseOrderId)} not found!");
            }

            var customerOrgId = (po.Contacts.FirstOrDefault(x => x.OrganizationRole.Equals(OrganizationRole.Principal))?.OrganizationId).Value;
            var customerOrgInfo = await _csfeApiClient.GetOrganizationByIdAsync(customerOrgId);

            var sql = @"SELECT am.InnerQuantity, am.OuterQuantity, am.StyleName, am.ColourName
                        FROM ArticleMaster am WITH (NOLOCK)
                        WHERE am.CompanyCode = @companyCode AND am.ItemNo = @productCode";

            var filterParameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@companyCode",
                        Value = customerOrgInfo.Code,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@productCode",
                        Value = productCode,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                };

            POLineItemArticleMasterViewModel mappingCallback(DbDataReader reader)
            {
                var mappedData = new POLineItemArticleMasterViewModel();
                while (reader.Read())
                {
                    mappedData = new POLineItemArticleMasterViewModel
                    {
                        InnerQuantity = reader["InnerQuantity"] as int?,
                        OuterQuantity = reader["OuterQuantity"] as int?,
                        StyleName = reader["StyleName"] as string,
                        ColourName = reader["ColourName"] as string,
                    };
                }
                return mappedData;
            }

            return _dataQuery.GetDataBySql(sql, mappingCallback, filterParameters.ToArray());
        }

        public async Task<IEnumerable<POLineItemArticleMasterViewModel>> GetInformationFromArticleMaster(string customerOrgCode)
        {
            var sql = @"SELECT 
                            am.ItemNo,
                            am.StyleNo,
                            am.ColourCode,
                            am.Size,
                            am.StyleName,
                            am.ColourName,
                            am.InnerQuantity,
                            am.OuterQuantity
                        FROM ArticleMaster am WITH (NOLOCK)
                        WHERE am.CompanyCode = @companyCode";

            var filterParameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@companyCode",
                        Value = customerOrgCode,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };

            IEnumerable<POLineItemArticleMasterViewModel> mappingCallback(DbDataReader reader)
            {
                var result = new List<POLineItemArticleMasterViewModel>();
                while (reader.Read())
                {
                    var mappedData = new POLineItemArticleMasterViewModel
                    {
                        ItemNo = reader["ItemNo"] as string,
                        StyleNo = reader["StyleNo"] as string,
                        ColourCode = reader["ColourCode"] as string,
                        Size = reader["Size"] as string,
                        StyleName = reader["StyleName"] as string,
                        ColourName = reader["ColourName"] as string,
                        InnerQuantity = reader["InnerQuantity"] as int?,
                        OuterQuantity = reader["OuterQuantity"] as int?,
                    };
                    result.Add(mappedData);
                }
                return result;
            }

            return _dataQuery.GetDataBySql(sql, mappingCallback, filterParameters.ToArray());
        }

        /// <summary>
        /// To update contacts (data from Organization table master data)
        /// </summary>
        /// <param name="contactsViewModel">Contacts viewModel</param>
        /// <returns></returns>
        public async Task UpdateContactsToLatestByOrgCodesAsync(IEnumerable<PurchaseOrderContactViewModel> contactsViewModel)
        {
            var newOrgs = await _csfeApiClient.GetOrganizationsByCodesAsync(contactsViewModel.Select(c => c.OrganizationCode));
            foreach (var contact in contactsViewModel)
            {
                var newContact = newOrgs.SingleOrDefault(c => c.Code == contact.OrganizationCode);
                if (newContact != null)
                {
                    contact.CompanyName = newContact.Name;
                    contact.AddressLine1 = $"{newContact.Address}\n{newContact.AddressLine2}\n{newContact.AddressLine3}\n{newContact.AddressLine4}";
                    contact.ContactName = newContact.ContactName;
                    contact.ContactNumber = newContact.ContactNumber;
                    contact.ContactEmail = newContact.ContactEmail;
                    contact.WeChatOrWhatsApp = newContact.WeChatOrWhatsApp;
                }
                else
                {
                    contact.AddressLine1 = $"{contact.AddressLine1 ?? string.Empty}\n{contact.AddressLine2 ?? string.Empty}\n{contact.AddressLine3 ?? string.Empty}\n{contact.AddressLine4 ?? string.Empty}";
                }
            }
        }

        public async Task CloseAsync(long id, UpdatePOViewModel viewModel, string userName)
        {
            var po = await Repository.GetAsync(c => c.Id == id);
            if (po == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", id)} not found!");
            }

            po.Stage = POStageType.Closed;
            Repository.Update(po);
            await UnitOfWork.SaveChangesAsync();

            var event1010 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1010,
                PurchaseOrderId = po.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = userName
            };

            await _activityService.TriggerAnEvent(event1010);
        }

        public async Task AssignPOsAsync(AssignPurchaseOrdersViewModel model, string userName)
        {
            var purchaseOrders = await Repository.Query(x => model.PurchaseOrderIds.Contains(x.Id), null, x => x.Include(y => y.Contacts)).ToListAsync();
            var organization = await _csfeApiClient.GetOrganizationByIdAsync(model.OrganizationId);
            if (organization == null)
            {
                throw new AppEntityNotFoundException($"Object with the key {string.Join(", ", model.OrganizationId)} not found!");
            }
            foreach (var purchaseOrder in purchaseOrders)
            {
                purchaseOrder.Contacts = purchaseOrder.Contacts.Where(x => x.OrganizationRole != model.OrganizationRole).ToList(); // delete existing contact whose organization role = new contact role
                var newContact = new PurchaseOrderContactModel
                {
                    OrganizationRole = model.OrganizationRole,
                    CompanyName = organization.Name,
                    AddressLine1 = organization.Address,
                    AddressLine2 = organization.AddressLine2,
                    AddressLine3 = organization.AddressLine3,
                    AddressLine4 = organization.AddressLine4,
                    ContactName = organization.ContactName,
                    ContactNumber = organization.ContactNumber,
                    ContactEmail = organization.ContactEmail,
                    OrganizationCode = organization.Code,
                    OrganizationId = organization.Id
                };
                newContact.Audit(userName);
                purchaseOrder.Contacts.Add(newContact);
            }
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// It is to concatenate address lines to address information with newline \n
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="addressLine2">Address line 2</param>
        /// <param name="addressLine3">Address line 3</param>
        /// <param name="addressLine4">Address line 4/param>
        /// <returns></returns>
        private static string ConcatenateCompanyAddressLines(string address, string addressLine2, string addressLine3, string addressLine4)
        {
            var result = address;

            if (!string.IsNullOrEmpty(addressLine2))
            {
                result = (!string.IsNullOrEmpty(result) ? (result + "\n") : "") + addressLine2;
            }
            if (!string.IsNullOrEmpty(addressLine3))
            {
                result += "\n" + addressLine3;
            }
            if (!string.IsNullOrEmpty(addressLine4))
            {
                result += "\n" + addressLine4;
            }
            return result;
        }

        public async Task<IEnumerable<POEmailNotificationQueryModel>> GetPOEmailNotificationAsync(long buyerComplianceId)
        {
            string sql = @"
                DECLARE @ProgressNotifyDay INT
                DECLARE @SystemDay DATE = CAST(GETDATE() AS Date) 
                DECLARE @BuyerCompliancesTbl TABLE (
	                OrganizationId BIGINT,
	                OrganizationName NVARCHAR(100),
	                ProgressNotifyDay INT
                )
                INSERT INTO @BuyerCompliancesTbl
                SELECT OrganizationId,OrganizationName, BC.ProgressNotifyDay
	                FROM BuyerCompliances BC
	                WHERE 
		                BC.Status = 1 
		                AND BC.IsEmailNotificationToSupplier = 1 
		                AND BC.EmailNotificationTime IS NOT NULL
		                AND BC.Id = {0}

                SELECT @ProgressNotifyDay = ProgressNotifyDay
                FROM @BuyerCompliancesTbl

                ;WITH 
                PurchaseOrderContactsCTE AS 
                (
	                SELECT POC.PurchaseOrderId
	                FROM PurchaseOrderContacts POC
	                INNER JOIN @BuyerCompliancesTbl BCT
		                ON BCT.OrganizationId = POC.OrganizationId
	                WHERE  POC.OrganizationRole = 'Principal'
                ),
                Result AS
                (
	                SELECT 
                        PO.Id,
						PO.PONumber,
						PO.CargoReadyDate,
						PO.ProposeDate,
						POC.OrganizationId,
						POC.OrganizationRole,
						POC.ContactEmail,
                        PO.ShipFromId
	                FROM PurchaseOrders PO
	                INNER JOIN PurchaseOrderContactsCTE POCTE ON PO.Id = POCTE.PurchaseOrderId
	                INNER JOIN PurchaseOrderContacts POC 
		                ON PO.Id = POC.PurchaseOrderId 
		                AND (POC.OrganizationRole = 'Principal' OR POC.OrganizationRole = 'Supplier' OR POC.OrganizationRole = 'Origin Agent')
	                WHERE
		                ProductionStarted = 0
		                AND PO.Stage < 30
		                AND (CargoReadyDate <= DATEADD(DAY, @ProgressNotifyDay, @SystemDay) OR ProposeDate <= DATEADD(DAY, @ProgressNotifyDay, @SystemDay))  
		                AND (@SystemDay <= CargoReadyDate OR @SystemDay <= ProposeDate)
                )
                SELECT *
                FROM Result
                        ";
            IQueryable<POEmailNotificationQueryModel> query = _dataQuery.GetQueryable<POEmailNotificationQueryModel>(sql, buyerComplianceId);
            var data = await query.ToListAsync();
            return data;
        }

        public IEnumerable<POEmailNotificationViewModel> CreatePOEmailNotificationAsync(long buyerComplianceId, IEnumerable<POEmailNotificationQueryModel> data)
        {
            var emails = new List<POEmailNotificationViewModel>();
            var customerId = data.FirstOrDefault(c => c.OrganizationRole == OrganizationRoleConstant.Principal)?.OrganizationId;
            var groupedSuppliers = data
                .Where(c => c.OrganizationRole == OrganizationRoleConstant.Supplier)
                .GroupBy(c => c.OrganizationId).ToList();

            foreach (var supplier in groupedSuppliers)
            {
                var suppilerPOs = supplier.ToList();
                var pos = data.Where(c =>
                    suppilerPOs.Any(s => s.PONumber == c.PONumber)
                    && c.OrganizationRole == OrganizationRoleConstant.OriginAgent
                    ).GroupBy(c => c.OrganizationId).ToList();

                foreach (var po in pos)
                {
                    var supplierId = suppilerPOs.FirstOrDefault()?.OrganizationId;
                    var originAgentId = po.FirstOrDefault()?.OrganizationId;

                    // Handle for case POs have the same ShipFrom,  PSP-3002
                    var poGroupedByShipFrom = po.GroupBy(c => c.shipFromId).ToList();
                    foreach (var groupedPO in poGroupedByShipFrom)
                    {
                        var email = new POEmailNotificationViewModel()
                        {
                            Here = $"{_appConfig.ClientUrl}/po-progress-check?navigateMode=email&buyerComplianceId={buyerComplianceId}&customerId={customerId}&supplierId={supplierId}&poIds={string.Join(",", groupedPO.Select(c => c.Id))}",
                            SupplierId = supplierId,
                            OriginAgentId = originAgentId,
                            shipFromId = groupedPO.FirstOrDefault()?.shipFromId ?? 0,
                            customerId = customerId ?? 0,
                            To = suppilerPOs.FirstOrDefault()?.ContactEmail,
                            CC = groupedPO.FirstOrDefault()?.ContactEmail,
                            POs = groupedPO.Select(c => new POEmailDetailViewModel() { Id = c.Id, PONumber = c.PONumber, Link = $"{_appConfig.ClientUrl}/purchase-orders/{c.Id}" }).ToList(),
                        };
                        emails.Add(email);
                    }
                }
            }
            return emails;
        }

        public async Task<IEnumerable<POProgressCheckQueryModel>> SearchPOsForProgressCheckFromEmailAsync(long buyerComplianceId, string poIds)
        {
            try
            {
                string sql = @"
                    DECLARE @ProgressNotifyDay INT
                    DECLARE @SystemDay DATE = CAST(GETDATE() AS Date) 
                    DECLARE @BuyerCompliancesTbl TABLE (
                        OrganizationId BIGINT,
                        OrganizationName NVARCHAR(100),
                        ProgressNotifyDay INT
                    )
                    INSERT INTO @BuyerCompliancesTbl
                    SELECT OrganizationId,OrganizationName, BC.ProgressNotifyDay
                        FROM BuyerCompliances BC
                        WHERE 
                            BC.Status = 1 
                            AND BC.IsEmailNotificationToSupplier = 1 
                            AND BC.EmailNotificationTime IS NOT NULL
                            AND BC.Id = {0}

                    SELECT @ProgressNotifyDay = ProgressNotifyDay
                    FROM @BuyerCompliancesTbl

                    SELECT
		                    PO.Id,
		                    PO.PONumber,
		                    PO.CargoReadyDate,
		                    PO.ProductionStarted,
		                    PO.ProposeDate,
		                    PO.QCRequired,
		                    PO.ShortShip,
		                    PO.SplitShipment,
		                    PO.Remark
	                    FROM PurchaseOrders PO
	                    WHERE 
		                    ProductionStarted = 0
		                    AND PO.Stage < 30
                            AND (CargoReadyDate <= DATEADD(DAY, @ProgressNotifyDay, @SystemDay) OR ProposeDate <= DATEADD(DAY, @ProgressNotifyDay, @SystemDay))  
                            AND PO.Id IN ((SELECT [VALUE] FROM [dbo].[fn_SplitStringToTable] ({1}, ','))) 
                        ";
                IQueryable<POProgressCheckQueryModel> query = _dataQuery.GetQueryable<POProgressCheckQueryModel>(sql, buyerComplianceId, poIds);
                var data = await query.ToListAsync();
                return data;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task ChangeStageToShipmentDispatchAsync(IEnumerable<long> poIds)
        {
            var pos = await Repository.Query(c => poIds.Contains(c.Id), null, c => c.Include(c => c.BlanketPO)).ToListAsync();

            foreach (var po in pos)
            {
                if (po.Stage < POStageType.ShipmentDispatch && po.POType != POType.Blanket)
                {
                    po.Stage = POStageType.ShipmentDispatch;
                    po.Audit(AppConstant.SYSTEM_USERNAME);
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

        public async Task ChangeStageToCloseAsync(IEnumerable<long> poIds, string userName, DateTime eventDate, string location = null, string remark = null)
        {
            var pos = await Repository.Query(c => poIds.Contains(c.Id), null, c => c.Include(c => c.BlanketPO).Include(c => c.LineItems)).ToListAsync();

            var event1010List = new List<ActivityViewModel>();
            var globalIds = new List<string>();

            foreach (var po in pos)
            {
                globalIds.Add(CommonHelper.GenerateGlobalId(po.Id, EntityType.CustomerPO));

                if (po.Stage == POStageType.Closed)
                {
                    continue;
                }

                if (po.LineItems.Any(item => item.BalanceUnitQty > 0))
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

            var globalIdActivities = await _globalIdActivityRepository.Query(
                c => globalIds.Contains(c.GlobalId) && c.Activity.ActivityCode == Event.EVENT_1010, null, c => c.Include(c => c.Activity)).ToListAsync();

            foreach (var globalIdActivity in globalIdActivities)
            {
                globalIdActivity.ActivityDate = eventDate;
                globalIdActivity.Location = location;
                globalIdActivity.Remark = remark;
                globalIdActivity.Audit(AppConstant.SYSTEM_USERNAME);

                globalIdActivity.Activity.ActivityDate = eventDate;
                globalIdActivity.Activity.Location = location;
                globalIdActivity.Activity.Remark = remark;
                globalIdActivity.Activity.Audit(AppConstant.SYSTEM_USERNAME);
            }

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task RevertStageToReleasedAsync(IEnumerable<long> poIds)
        {
            var pos = await Repository.Query(c => poIds.Contains(c.Id), null, c => c.Include(c => c.BlanketPO)).ToListAsync();

            foreach (var po in pos)
            {
                if (po.POType != POType.Blanket)
                {
                    po.Stage = POStageType.Released;
                    po.Audit(AppConstant.SYSTEM_USERNAME);
                    // If it's allocated purchase order, must update stage on related blanket PO
                    // If PO Blanket'stage greater than current stage then not update it
                    if (po.POType == POType.Allocated && po.BlanketPO != null && po.BlanketPO.Stage == POStageType.ShipmentDispatch)
                    {
                        po.BlanketPO.Stage = POStageType.Released;
                    }
                }
            }

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task RevertStageToShipmentDispatchAsync(IEnumerable<long> poIds)
        {
            var pos = await Repository.Query(c => poIds.Contains(c.Id), null, c => c.Include(c => c.BlanketPO)).ToListAsync();
            var purchaseOrderGlobalIdList = new List<string>();
            foreach (var po in pos)
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

            // Remove #1010
            var storedActivityList = await _activityRepository.Query(
                a => a.GlobalIdActivities.Any(g => purchaseOrderGlobalIdList.Contains(g.GlobalId)) && a.ActivityCode == Event.EVENT_1010,
                null,
                i => i.Include(a => a.GlobalIdActivities))
                .ToListAsync();

            _activityRepository.RemoveRange(storedActivityList.ToArray());
            await UnitOfWork.SaveChangesAsync();
        }

        public void AdjustQuantityOnPOLineItems(IEnumerable<long> shipmentIds, IEnumerable<long> poIds, AdjustBalanceOnPOLineItemsType type)
        {
            var adjustType = type.Equals(AdjustBalanceOnPOLineItemsType.Deduct) ? -1 : 1;

            // Adjust quantities for current purchase order line items
            var sql = @$"
                WITH CargoDetailsCTE AS
                (
	                SELECT ItemId, SUM(Unit) AS BookedQty
                    FROM CargoDetails CD
                    WHERE ShipmentId IN ({string.Join(",", shipmentIds)}) AND OrderId IN ({string.Join(",", poIds)})
                    GROUP BY ItemId
                )

                UPDATE POLineItems
                SET BalanceUnitQty = BalanceUnitQty + ({adjustType} * CTE.BookedQty), 
                    BookedUnitQty = BookedUnitQty - ({adjustType} * CTE.BookedQty)
                FROM POLineItems POL
                INNER JOIN CargoDetailsCTE CTE ON POL.Id = CTE.ItemId
               ;
            ";
            _dataQuery.ExecuteSqlCommand(sql);
        }

        public async Task<IEnumerable<CargoDetailModel>> GetCargoDetails(List<long> poIds)
        {
            var cargoDetails = await _cargoDetailRepository.QueryAsNoTracking(c => poIds.Contains(c.OrderId ?? 0) && c.Shipment.POFulfillmentId == null, null, c => c.Include(c => c.Shipment)).ToListAsync();
            return cargoDetails;
        }

        public async Task ChangeStagePOWithoutBookingAsync(long consignmentId)
        {
            var consignment = await _consignmentItineraryRepository.GetAsync(c => c.ConsignmentId == consignmentId);
            var shipmentId = consignment?.ShipmentId;
            if (shipmentId == null) return;

            var consignmentItineraries = await _consignmentItineraryRepository.QueryAsNoTracking(c =>
            c.ShipmentId == shipmentId
            && (c.Itinerary.FreightScheduler.ModeOfTransport == ModeOfTransport.Sea || c.Itinerary.FreightScheduler.ModeOfTransport == ModeOfTransport.Air)
            && (c.Itinerary.FreightScheduler.ATDDate != null || c.Itinerary.FreightScheduler.ATADate != null)
            && c.Shipment.CargoDetails.Any() && c.Shipment.POFulfillmentId == null && c.Shipment.OrderType == OrderType.Freight, null,
            c => c.Include(c => c.Shipment).Include(c => c.Itinerary).ThenInclude(c => c.FreightScheduler)
                .Include(c => c.Shipment).ThenInclude(c => c.CargoDetails)
            ).ToListAsync();

            var shipmentsLinkedPOWithoutBooking = consignmentItineraries.Select(x => x.Shipment).Distinct();

            if (shipmentsLinkedPOWithoutBooking == null || !shipmentsLinkedPOWithoutBooking.Any())
            {
                return;
            }

            var atdConsignmentItineraries = consignmentItineraries.Where(c => c.Itinerary.FreightScheduler.ATDDate != null).ToList();
            var ataConsignmentItineraries = consignmentItineraries
                .Where(c => c.Itinerary.FreightScheduler.ATADate != null)
                .Where(c =>
                      c.Shipment.ServiceType != null
                      && (c.Shipment.ServiceType.Contains("to-Port", StringComparison.OrdinalIgnoreCase) || c.Shipment.ServiceType.Contains("to-Airport", StringComparison.OrdinalIgnoreCase))
                      && c.Itinerary.FreightScheduler.LocationToName.Equals(c.Shipment.ShipTo ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                      && c.Shipment.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase))
                .ToList();

            //Handle for shipment is linked with PO by CargoDetails without booking module
            // 1. Shipment.PofulfillmentId = null
            // 2. CargoDetails.OrderId is not existing in POfulfillmentOrders

            var poIdsWithoutBooking = shipmentsLinkedPOWithoutBooking.SelectMany(c => c.CargoDetails).Where(x => x.OrderId != null).Select(c => c.OrderId.Value).Distinct().ToList();
            var poIdsBookedByBookings = await _poFulfillmentOrderRepository.QueryAsNoTracking(c => poIdsWithoutBooking.Contains(c.PurchaseOrderId)).ToListAsync();
            poIdsWithoutBooking = poIdsWithoutBooking.Where(c => !poIdsBookedByBookings.Any(s => s.PurchaseOrderId == c)).ToList();

            var cargoDetails = await GetCargoDetails(poIdsWithoutBooking);
            var shipmentsLinkedPO = cargoDetails.Select(c => c.Shipment).DistinctBy(c => c.Id).ToList();
            var consignmentItinerariesFSWithATD = await _consignmentItineraryRepository.QueryAsNoTracking(
                   c => shipmentsLinkedPO.Select(s => s.Id).ToList().Contains(c.ShipmentId ?? 0)
                       && c.Itinerary.FreightScheduler.ATDDate != null, null,
                   c => c.Include(c => c.Itinerary).ThenInclude(c => c.FreightScheduler)
                   ).ToListAsync();

            if (atdConsignmentItineraries.Count == 1)
            {
                var groupedByShipmentId = consignmentItinerariesFSWithATD.GroupBy(c => c.ShipmentId).ToList().Where(c => c.Count() > 1).ToList();
                var shipmentIdsNotUpdate = groupedByShipmentId.Select(c => c.Key).ToList();

                //PO#1 - Shipment#1
                //Shipment#1 has n FS
                shipmentsLinkedPOWithoutBooking = shipmentsLinkedPOWithoutBooking.Where(c => !shipmentIdsNotUpdate.Any(s => s == c.Id)).ToList();
                var ids = shipmentsLinkedPOWithoutBooking.Select(c => c.Id);
                if (ids.Any() && poIdsWithoutBooking.Any())
                {
                    AdjustQuantityOnPOLineItems(ids, poIdsWithoutBooking, AdjustBalanceOnPOLineItemsType.Deduct);
                }

                await ChangeStageToShipmentDispatchAsync(poIdsWithoutBooking);
            }

            if (ataConsignmentItineraries.Count == 1)
            {
                // Handle for shipment is linked with PO by CargoDetails without booking module
                // 1. Shipment.PofulfillmentId = null
                // 2. CargoDetails.OrderId is not existing in POfulfillmentOrders
                poIdsWithoutBooking = shipmentsLinkedPOWithoutBooking.SelectMany(c => c.CargoDetails).Where(x => x.OrderId != null).Select(c => c.OrderId.Value).Distinct().ToList();

                if (poIdsWithoutBooking.Any())
                {
                    var freightSchedulesHaveATA = await _consignmentItineraryRepository.QueryAsNoTracking(
                   c => shipmentsLinkedPO.Select(s => s.Id).ToList().Contains(c.ShipmentId ?? 0)
                       && c.Itinerary.FreightScheduler.ATADate.HasValue, null,
                   c => c.Include(c => c.Itinerary).ThenInclude(c => c.FreightScheduler)
                   ).ToListAsync();

                    var groupedFreightSchedulesHaveATA = freightSchedulesHaveATA.GroupBy(c => c.ShipmentId).ToList().Where(c => c.Count() > 1);
                    var shipmentIdsNotUpdate = groupedFreightSchedulesHaveATA.Select(c => c.Key).ToList();

                    //PO#1 - Shipment#1
                    //Shipment#1 has n FS
                    shipmentsLinkedPOWithoutBooking = shipmentsLinkedPOWithoutBooking.Where(c => !shipmentIdsNotUpdate.Any(s => s == c.Id)).ToList();

                    var ids = shipmentsLinkedPOWithoutBooking.Select(c => c.Id);
                    if (ids.Any() && poIdsWithoutBooking.Any() && atdConsignmentItineraries.Count == 0)
                    {
                        AdjustQuantityOnPOLineItems(ids, poIdsWithoutBooking, AdjustBalanceOnPOLineItemsType.Deduct);
                    }
                }

                await ChangeStageToCloseAsync(poIdsWithoutBooking, AppConstant.SYSTEM_USERNAME, ataConsignmentItineraries[0].Itinerary.FreightScheduler.ATADate ?? new DateTime(0001, 1, 1),
                    ataConsignmentItineraries[0].Itinerary.FreightScheduler.LocationToName);
            }
        }
    }
}