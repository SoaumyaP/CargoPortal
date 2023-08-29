using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Consolidation.Services.Interfaces;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.FreightScheduler.Services.Interfaces;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Application.QuickTrack;
using Groove.SP.Application.ShipmentContact.Mappers;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Groove.SP.Application.Shipments.Services.Interfaces;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Groove.SP.Application.Shipments.Services
{
    public partial class ShipmentService : ServiceBase<ShipmentModel, ShipmentViewModel>, IShipmentService
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IActivityService _activityService;
        private readonly IActivityRepository _activityRepository;
        private readonly IShipmentContactRepository _shipmentContactRepository;
        private readonly IPOFulfillmentOrderRepository _poFulfillmentOrderRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IItineraryRepository _itineraryRepository;
        private readonly IConsignmentItineraryRepository _consignmentItineraryRepository;
        private readonly IPOFulfillmentRepository _poFulfillmentRepository;
        private readonly IPOFulfillmentBookingRequestRepository _poFulfillmentBookingRequestRepository;
        private readonly IContainerRepository _containerRepository;
        private readonly IRepository<ShipmentLoadModel> _shipmentLoadRepository;
        private readonly IConsolidationRepository _consolidationRepository;
        private readonly AppConfig _appConfig;
        private readonly IDataQuery _dataQuery;
        private readonly IEdiSonBookingService _ediSonBookingService;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IServiceProvider _services;
        private readonly IConsolidationService _consolidationService;
        private readonly IFreightSchedulerService _freightSchedulerService;
        private readonly IShipmentBillOfLadingRepository _shipmentBillOfLadingRepository;
        private readonly IBillOfLadingConsignmentRepository _billOfLadingConsignmentRepository;
        private readonly IRepository<MasterBillOfLadingModel> _masterBillOfLadingRepository;
        private readonly IBillOfLadingShipmentLoadRepository _billOfLadingShipmentLoadRepository;
        private readonly IBillOfLadingRepository _billOfLadingRepository;
        private readonly IConsignmentRepository _consignmentRepository;
        private readonly IContractMasterRepository _contractMasterRepository;
        private readonly TelemetryConfig _telemetryConfig;
        private readonly IRepository<IntegrationLogModel> _integrationLogRepository;
        private readonly IFreightSchedulerRepository _freightSchedulerRepository;
        private readonly IShipmentLoadDetailRepository _shipmentLoadDetailRepository;
        private readonly ICargoDetailRepository _cargoDetailRepository;
        private readonly IBillOfLadingItineraryRepository _billOfLadingItineraryRepository;
        private readonly IContainerItineraryRepository _containerItineraryRepository;
        private readonly IMasterBillOfLadingItineraryRepository _masterBillOfLadingItineraryRepository;
        private readonly IBillOfLadingContactRepository _billOfLadingContactRepository;
        private readonly IRepository<POFulfillmentContactModel> _poFulfillmentContactRepository;
        private readonly IGlobalIdActivityRepository _globalIdActivityRepository;
        private readonly IRepository<BuyerComplianceModel> _buyerComplianceRepository;

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
        /// Please do not use it directly, use POFulfillmentService instead.
        /// </summary>
        private IPOFulfillmentService _poFulfillmentServiceLazy;
        public IPOFulfillmentService POFulfillmentService
        {
            get
            {
                if (_poFulfillmentServiceLazy == null)
                {
                    _poFulfillmentServiceLazy = _services.GetRequiredService<IPOFulfillmentService>();
                }
                return _poFulfillmentServiceLazy;
            }
        }

        protected override Func<IQueryable<ShipmentModel>, IQueryable<ShipmentModel>> FullIncludeProperties
        {
            get
            {
                return x => x.Include(m => m.Contacts);
            }
        }

        public ShipmentService(IUnitOfWorkProvider unitOfWorkProvider,
                                IUserProfileService userProfileService,
                                IOptions<AppConfig> appConfig,
                                IActivityService activityService,
                                IConsolidationService consolidationService,
                                IDataQuery dataQuery,
                                IPOFulfillmentRepository poFulfillmentRepository,
                                IPOFulfillmentOrderRepository poFulfillmentOrderRepository,
                                IPurchaseOrderRepository purchaseOrderRepository,
                                IRepository<ShipmentLoadModel> shipmentLoadRepository,
                                IConsolidationRepository consolidationRepository,
                                IEdiSonBookingService ediSonBookingService,
                                IFreightSchedulerService freightSchedulerService,
                                ICSFEApiClient csfeApiClient,
                                IOptions<TelemetryConfig> telemetryConfig,
                                IShipmentBillOfLadingRepository shipmentBillOfLadingRepository,
                                IBillOfLadingConsignmentRepository billOfLadingConsignmentRepository,
                                IBillOfLadingShipmentLoadRepository billOfLadingShipmentLoadRepository,
                                IBillOfLadingRepository billOfLadingRepository,
                                IRepository<MasterBillOfLadingModel> masterBillOfLadingRepository,
                                IConsignmentRepository consignmentRepository,
                                IContractMasterRepository contractMasterRepository,
                                IRepository<IntegrationLogModel> integrationLogRepository,
                                IFreightSchedulerRepository freightSchedulerRepository,
                                IRepository<POFulfillmentContactModel> poFulfillmentContactRepository,
                                IServiceProvider services,
                                IRepository<BuyerComplianceModel> buyerComplianceRepository)
            : base(unitOfWorkProvider)
        {
            _telemetryConfig = telemetryConfig.Value;
            _userProfileService = userProfileService;
            _appConfig = appConfig.Value;
            _activityService = activityService;
            _consolidationService = consolidationService;
            _itineraryRepository = (IItineraryRepository)UnitOfWork.GetRepository<ItineraryModel>();
            _consignmentItineraryRepository = (IConsignmentItineraryRepository)UnitOfWork.GetRepository<ConsignmentItineraryModel>();
            _activityRepository = (IActivityRepository)UnitOfWork.GetRepository<ActivityModel>();
            _shipmentContactRepository = (IShipmentContactRepository)UnitOfWork.GetRepository<ShipmentContactModel>();
            _containerRepository = (IContainerRepository)UnitOfWork.GetRepository<ContainerModel>();
            _poFulfillmentBookingRequestRepository = (IPOFulfillmentBookingRequestRepository)UnitOfWork.GetRepository<POFulfillmentBookingRequestModel>();
            _dataQuery = dataQuery;
            _poFulfillmentRepository = poFulfillmentRepository;
            _poFulfillmentOrderRepository = poFulfillmentOrderRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _shipmentLoadRepository = shipmentLoadRepository;
            _consolidationRepository = consolidationRepository;
            _buyerComplianceRepository = buyerComplianceRepository;
            _ediSonBookingService = ediSonBookingService;
            _freightSchedulerService = freightSchedulerService;
            _csfeApiClient = csfeApiClient;
            _shipmentBillOfLadingRepository = shipmentBillOfLadingRepository;
            _billOfLadingConsignmentRepository = billOfLadingConsignmentRepository;
            _billOfLadingShipmentLoadRepository = billOfLadingShipmentLoadRepository;
            _billOfLadingRepository = billOfLadingRepository;
            _masterBillOfLadingRepository = masterBillOfLadingRepository;
            _consignmentRepository = consignmentRepository;
            _contractMasterRepository = contractMasterRepository;
            _integrationLogRepository = integrationLogRepository;
            _services = services;
            _freightSchedulerRepository = freightSchedulerRepository;
            _shipmentLoadDetailRepository = (IShipmentLoadDetailRepository)UnitOfWork.GetRepository<ShipmentLoadDetailModel>();
            _cargoDetailRepository = (ICargoDetailRepository)UnitOfWork.GetRepository<CargoDetailModel>();
            _billOfLadingItineraryRepository = (IBillOfLadingItineraryRepository)UnitOfWork.GetRepository<BillOfLadingItineraryModel>();
            _containerItineraryRepository = (IContainerItineraryRepository)UnitOfWork.GetRepository<ContainerItineraryModel>();
            _masterBillOfLadingItineraryRepository = (IMasterBillOfLadingItineraryRepository)UnitOfWork.GetRepository<MasterBillOfLadingItineraryModel>();
            _billOfLadingContactRepository = (IBillOfLadingContactRepository)UnitOfWork.GetRepository<BillOfLadingContactModel>();
            _poFulfillmentContactRepository = poFulfillmentContactRepository;
            _globalIdActivityRepository = (IGlobalIdActivityRepository)UnitOfWork.GetRepository<GlobalIdActivityModel>();
        }

        private async Task<IEnumerable<ShipmentViewModel>> GetShipmentViewModel(IEnumerable<ShipmentModel> shipments)
        {
            var result = Mapper.Map<IEnumerable<ShipmentViewModel>>(shipments);

            // Populate activities
            var shipmentIdList = shipments.Select(x => CommonHelper.GenerateGlobalId(x.Id, EntityType.Shipment));
            var shipmentActivities = await _activityRepository.Query(x => x.GlobalIdActivities.Any(y => shipmentIdList.Contains(y.GlobalId)),
                null,
                x => x.Include(a => a.GlobalIdActivities).ThenInclude(b => b.ReferenceEntity)).ToListAsync();

            foreach (var shipment in result)
            {
                var activities = shipmentActivities.Where(sa => sa.GlobalIdActivities.Any(ga => ga.ReferenceEntity.EntityId == shipment.Id));
                shipment.Activities = Mapper.Map<ICollection<ActivityViewModel>>(activities);
            }

            return result;
        }

        public async Task<IEnumerable<ShipmentViewModel>> GetShipmentsByBOLAsync(long billOfLadingId, bool isInternal, string affiliates)
        {
            var listOfAffiliates = new List<long>();
            IEnumerable<ShipmentModel> shipments = null;

            if (isInternal)
            {
                shipments = await Repository.Query(s => s.ShipmentBillOfLadings.Any(sb => sb.BillOfLadingId == billOfLadingId),
                                                               n => n.OrderBy(a => a.ShipFromETDDate),
                                                               m => m.Include(a => a.Contacts)).ToListAsync();
            }
            else
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }

                shipments = await Repository.Query(s => s.ShipmentBillOfLadings.Any(sb => sb.BillOfLadingId == billOfLadingId) &&
                                            s.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId)),
                                            n => n.OrderBy(a => a.ShipFromETDDate),
                                            m => m.Include(a => a.Contacts)).ToListAsync();
            }

            return await GetShipmentViewModel(shipments);
        }

        public async Task<IEnumerable<ShipmentViewModel>> GetShipmentsByMasterBOLAsync(long masterBOLId, bool isDirectMaster, bool isInternal, string affiliates)
        {
            var listOfAffiliates = new List<long>();
            IEnumerable<ShipmentModel> shipments = null;

            // Is internal user
            if (isInternal)
            {
                // Master BL links directly to Shipment
                if (isDirectMaster)
                {
                    shipments = await Repository.Query(s => s.Consignments.Any(c => c.MasterBillId == masterBOLId),
                                                               n => n.OrderBy(a => a.ShipFromETDDate),
                                                               m => m.Include(a => a.Contacts)).ToListAsync();
                }
                else
                {
                    // Master BL links to Shipment by House BL
                    shipments = await Repository.Query(s => s.ShipmentBillOfLadings.Any(sb => sb.BillOfLading.BillOfLadingShipmentLoads.Any(bsl => bsl.MasterBillOfLadingId == masterBOLId)),
                                                                 n => n.OrderBy(a => a.ShipFromETDDate),
                                                                 m => m.Include(a => a.Contacts)).ToListAsync();
                }

            }
            else
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }

                // Master BL links directly to Shipment
                if (isDirectMaster)
                {
                    // Master BL links to Shipment by House BL
                    shipments = await Repository.Query(s => s.Consignments.Any(c => c.MasterBillId == masterBOLId)
                                             && s.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId)),
                                             n => n.OrderBy(a => a.ShipFromETDDate),
                                             m => m.Include(a => a.Contacts)).ToListAsync();
                }
                else
                {
                    // Master BL links to Shipment by House BL
                    shipments = await Repository.Query(s => s.ShipmentBillOfLadings.Any(sb => sb.BillOfLading.BillOfLadingShipmentLoads.Any(bsl => bsl.MasterBillOfLadingId == masterBOLId))
                                           && s.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId)),
                                           n => n.OrderBy(a => a.ShipFromETDDate),
                                           m => m.Include(a => a.Contacts)).ToListAsync();

                }
            }

            return await GetShipmentViewModel(shipments);
        }

        private IQueryable<ReportingShipmentQueryModel> GetOceanTEUShipments(List<long> listOfAffiliates)
        {
            return this.GetTEUShipments(s => s.ModeOfTransport.Equals("sea"), listOfAffiliates);
        }

        private IQueryable<ReportingShipmentQueryModel> GetTEUShipments(Expression<Func<ReportingShipmentQueryModel, bool>> customFilters = null,
          List<long> listOfAffiliates = null)
        {
            var filterAffiliates = string.Empty;
            if (listOfAffiliates != null && listOfAffiliates.Any())
            {
                filterAffiliates = @"AND 
                    EXISTS
                    (
                        SELECT 1
                        FROM ShipmentContacts sc
                        WHERE s.Id = sc.ShipmentId AND sc.OrganizationId IN " + $"({string.Join(",", listOfAffiliates)}))";
            }

            var sql =
                @"SELECT s.Id, s.ShipFrom, s.ShipTo, s.Movement, s.ServiceType, s.ModeOfTransport,
	                CAST(CASE
	                WHEN c.ContainerType LIKE '20%'
	                THEN 1 ELSE CASE
		                WHEN c.ContainerType LIKE '40%'
		                THEN 2 ELSE CASE
			                WHEN c.ContainerType LIKE '45%'
			                THEN 2 ELSE 0
		                END
	                END
                END AS decimal(18,2)) AS OceanVolume, s.ShipFromETDDate
                FROM Shipments (NOLOCK) AS s
                JOIN ShipmentLoads (NOLOCK) AS sh ON s.Id = sh.ShipmentId
                JOIN Containers (NOLOCK) AS c ON sh.ContainerId = c.Id
                WHERE s.IsFCL = 1 AND s.Status ='" + StatusType.ACTIVE + @"'" + filterAffiliates + @"

                UNION ALL

                SELECT Id, ShipFrom, ShipTo, Movement, ServiceType, ModeOfTransport, 
                    TotalVolume / {0} AS OceanVolume, 
                    ShipFromETDDate
                FROM Shipments AS s (NOLOCK)
                WHERE IsFCL = 0 AND s.Status ='" + StatusType.ACTIVE + @"'" + filterAffiliates;



            var query = _dataQuery.GetQueryable<ReportingShipmentQueryModel>(sql,
                _appConfig.TEUCBMRatio).AsNoTracking();

            if (customFilters != null)
            {
                query = query.Where(customFilters);
            }
            return query;
        }

        public async Task<Top5OceanVolumeViewModel> GetTop5OceanVolumeAsync(bool isOrigin, bool isInternal, string affiliates, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);

            var listOfAffiliates = new List<long>();
            DateTime currentMonthStartDate = DateTime.Now.MonthStartDate();
            DateTime lastMonthStartDate = DateTime.UtcNow.LastMonthStartDate();
            var fromDate = DateTime.Parse(dates["FromDate"]);
            var toDate = DateTime.Parse(dates["ToDate"]);

            if (!string.IsNullOrEmpty(affiliates))
            {
                listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
            }

            var query = GetOceanTEUShipments(listOfAffiliates)
                .Where(s => s.ShipFromETDDate >= fromDate && s.ShipFromETDDate <= toDate)
                .GroupBy(s => isOrigin ? s.ShipFrom : s.ShipTo)
                .Select(s => new
                {
                    ShipLocation = s.Key,
                    TEU = Math.Round(s.Sum(t => t.OceanVolume), 1)
                })
                .OrderByDescending(s => s.TEU)
                .Take(5);

            var resultQuery = (await query.ToListAsync()).OrderBy(s => s.ShipLocation);
            var result = new Top5OceanVolumeViewModel()
            {
                ThisMonthStartDate = currentMonthStartDate.AddDays(-1),
                LastMonthStartDate = lastMonthStartDate,
                Top5 = resultQuery.Select(x => x.ShipLocation).ToList(),
                TEUs = resultQuery.Select(x => x.TEU).ToList(),
            };
            return result;
        }

        public async Task<ReportingMetricShipmentViewModel> GetReportingWeeklyShipments(bool isInternal, string affiliates, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var listOfAffiliates = new List<long>();

            if (!string.IsNullOrEmpty(affiliates))
            {
                listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
            }

            // Linq to Entity, string comparison with case insensitive
            var query = Repository.GetListQueryable().Where(s => s.Status == StatusType.ACTIVE);
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
            var thisWeekQuery = query.Where(s => s.ShipFromETDDate >= fromDate && s.ShipFromETDDate <= toDate);
            var lastWeekQuery = query.Where(s => s.ShipFromETDDate >= lastWeekStartDate && s.ShipFromETDDate < currentWeekStartDate);

            ReportingMetricShipmentViewModel result = new ReportingMetricShipmentViewModel
            {
                ThisWeekTotal = await thisWeekQuery.CountAsync(),
                LastWeekTotal = await lastWeekQuery.CountAsync(),
                ThisWeekStartDate = currentWeekStartDate,
                LastWeekStartDate = lastWeekStartDate,
                NextWeekStartDate = nextWeekStartDate.AddDays(-1)
            };

            return result;
        }

        public async Task<WeeklyReportingMetricOceanVolumeViewModel> GetReportingWeeklyOceanVolume(bool isInternal, string affiliates, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var listOfAffiliates = new List<long>();

            if (!string.IsNullOrEmpty(affiliates))
            {
                listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
            }

            var query = GetOceanTEUShipments(listOfAffiliates);

            var result = new WeeklyReportingMetricOceanVolumeViewModel();
            result.ReportingMetricOceanVolumeByMovement = new List<ReportingMetricOceanVolumeByMovementViewModel>();

            DateTime currentWeekStartDate = result.ThisWeekStartDate = DateTime.UtcNow.WeekStartDate();
            DateTime lastWeekStartDate = currentWeekStartDate.WeekStartDate();
            DateTime nextWeekStartDate = DateTime.UtcNow.Date;
            result.ThisWeekStartDate = currentWeekStartDate;
            result.NextWeekStartDate = nextWeekStartDate.AddDays(-1);

            var fromDate = DateTime.Parse(dates["FromDate"]);
            var toDate = DateTime.Parse(dates["ToDate"]);

            var thisWeekQuery = query.Where(s => s.ShipFromETDDate >= fromDate && s.ShipFromETDDate <= toDate);
            var lastWeekQuery = query.Where(s => s.ShipFromETDDate >= lastWeekStartDate && s.ShipFromETDDate < currentWeekStartDate);

            var thisWeekTotalVolumeByMovement = await thisWeekQuery.GroupBy(s => s.Movement)
                       .Select(g => new ReportingMetricOceanVolumeByMovementViewModel
                       {
                           Category = g.Key,
                           ThisWeekTotal = g.Sum(t => t.OceanVolume),
                       })
                       .ToListAsync();

            var lastWeekTotalVolumeByMovement = await lastWeekQuery.GroupBy(s => s.Movement)
                       .Select(g => new ReportingMetricOceanVolumeByMovementViewModel
                       {
                           Category = g.Key,
                           LastWeekTotal = g.Sum(t => t.OceanVolume),
                       })
                       .ToListAsync();

            var movementTypeList = new List<string>
            {
                "CY/CY",
                "CFS/CY",
                "CFS/CFS"
            };

            // Data synthesis for each movement type
            foreach (var movementType in movementTypeList)
            {
                var roundToDecimal = 1;
                var thisWeekTotal = thisWeekTotalVolumeByMovement.FirstOrDefault(t => t.Category == movementType)?.ThisWeekTotal ?? 0;
                var lastWeekTotal = lastWeekTotalVolumeByMovement.FirstOrDefault(t => t.Category == movementType)?.LastWeekTotal ?? 0;
                if (movementType == "CFS/CFS")
                {
                    thisWeekTotal *= _appConfig.TEUCBMRatio;
                    lastWeekTotal *= _appConfig.TEUCBMRatio;
                    roundToDecimal = 3;
                }
                result.ReportingMetricOceanVolumeByMovement.Add(new ReportingMetricOceanVolumeByMovementViewModel
                {
                    Category = movementType,
                    ThisWeekTotal = Math.Round(thisWeekTotal, roundToDecimal),
                    LastWeekTotal = Math.Round(lastWeekTotal, roundToDecimal)
                });
            }
            return result;
        }

        public async Task<MonthlyReportingOceanVolumeByMovementViewModel> GetReportingMonthlyOceanVolume(
            string groupBy,
            bool isInternal,
            string affiliates,
            string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var fromDate = DateTime.Parse(dates["FromDate"]);
            var toDate = DateTime.Parse(dates["ToDate"]);

            var listOfAffiliates = new List<long>();

            if (!isInternal && !string.IsNullOrEmpty(affiliates))
            {
                listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
            }

            var query = GetOceanTEUShipments(listOfAffiliates);

            DateTime currentMonthStartDate = DateTime.Now.MonthStartDate();
            DateTime lastMonthStartDate = DateTime.UtcNow.LastMonthStartDate();
            var thisPeriod = query.Where(s => s.ShipFromETDDate >= fromDate && s.ShipFromETDDate <= toDate);

            var result = new MonthlyReportingOceanVolumeByMovementViewModel();
            result.LastMonthStartDate = lastMonthStartDate;
            result.ThisMonthStartDate = currentMonthStartDate.AddDays(-1);
            result.ReportingPieCharts = new List<ReportingPieChartViewModel>();

            switch (groupBy)
            {
                case "movement":
                    result.ReportingPieCharts = await thisPeriod.GroupBy(s => s.Movement)
                         .Select(g => new ReportingPieChartViewModel
                         {
                             Category = g.Key,
                             Value = Math.Round(g.Sum(t => t.OceanVolume), 1)
                         })
                         .ToListAsync();
                    break;
                case "servicetype":
                    result.ReportingPieCharts = await thisPeriod.GroupBy(s => s.ServiceType)
                        .Select(g => new ReportingPieChartViewModel
                        {
                            Category = g.Key,
                            Value = Math.Round(g.Sum(t => t.OceanVolume), 1)
                        })
                        .ToListAsync();
                    break;
                default:
                    break;
            }

            result.ReportingPieCharts.ToList().ForEach(r => r.Category = string.IsNullOrWhiteSpace(r.Category) ? "N/A" : r.Category);

            return result;
        }

        public async Task<ShipmentViewModel> GetAsync(string shipmentNo, bool isInternal, string affiliates)
        {
            var listOfAffiliates = new List<long>();
            long.TryParse(shipmentNo, out var shipmentId);

            var query = Repository.GetListQueryable(i => i.Include(s => s.Contacts)
                    .Include(s => s.ShipmentBillOfLadings).ThenInclude(sb => sb.BillOfLading))
                    .Include(s => s.POFulfillment)
                    .Include(s => s.Consignments).ThenInclude(c => c.MasterBill)
                    .Include(s => s.ContractMaster)
                    .OrderByDescending(s => s.BookingDate)
                    .Where(s => s.Id == shipmentId || s.ShipmentNo == shipmentNo);
            if (!isInternal)
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }
                query = query.Where(s => s.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId)));
            }

            var model = await query.FirstOrDefaultAsync();

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the key {shipmentId} not found!");
            }

            var viewModel = Mapper.Map<ShipmentViewModel>(model);

            //get additional data
            var principalOrgId = model.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Principal)?.OrganizationId;
            if (principalOrgId.HasValue)
            {
                var compliance = await _buyerComplianceRepository.GetAsNoTrackingAsync(x => x.OrganizationId == principalOrgId);
                viewModel.EnforceCommercialInvoiceFormat = compliance?.EnforceCommercialInvoiceFormat;
            }

            //set value to null
            if (viewModel.EnforceCommercialInvoiceFormat == null ||
                viewModel.EnforceCommercialInvoiceFormat == true)
            {
                viewModel.CommercialInvoiceNo = null;
                viewModel.InvoiceDate = null;
            }
            

            return viewModel;
        }

        private async Task<IEnumerable<POLineItemArticleMasterViewModel>> GetInformationFromArticleMaster(long shipmentId, long customerOrgId, params string[] productCodes)
        {
            var customerOrg = await _csfeApiClient.GetOrganizationByIdAsync(customerOrgId);
            var productCodesString = string.Join(",", productCodes);

            var sql = @"SELECT item.Id, am.InnerQuantity, am.OuterQuantity
                        FROM ShipmentItems item JOIN ArticleMaster am WITH (NOLOCK) ON item.ProductCode = TRIM(am.ItemNo) 
                        WHERE item.ShipmentId = @shipmentId AND am.CompanyCode = @companyCode AND item.ProductCode IN (SELECT [VALUE] FROM [dbo].[fn_SplitStringToTable] (@productCodes, ','))";

            var filterParameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@shipmentId",
                        Value = shipmentId,
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

        public async Task<IEnumerable<ShipmentExceptionViewModel>> GetExceptionsAsync(string idList)
        {
            var idListNumber = new List<long>();
            if (!string.IsNullOrEmpty(idList))
            {
                idListNumber = JsonConvert.DeserializeObject<List<long>>(idList);
            }

            var result = new List<ShipmentExceptionViewModel>();
            if (idListNumber.Count > 0)
            {
                var shipmentIdList = idListNumber.Select(x => CommonHelper.GenerateGlobalId(x, EntityType.Shipment));
                var shipmentActivities = await _activityRepository.Query(x => x.GlobalIdActivities.Any(y => shipmentIdList.Contains(y.GlobalId)),
                    null,
                    x => x.Include(a => a.GlobalIdActivities).ThenInclude(b => b.ReferenceEntity)).ToListAsync();

                foreach (var id in idListNumber)
                {
                    var activities = shipmentActivities.Where(sa => sa.GlobalIdActivities.Any(ga => ga.ReferenceEntity.EntityId == id));
                    result.Add(new ShipmentExceptionViewModel()
                    {
                        Id = id,
                        IsException = activities.Any(x => x.Resolved != null && !x.Resolved.Value && !string.IsNullOrEmpty(x.ActivityType) && x.ActivityType[x.ActivityType.Length - 1] == 'E')
                    });
                }
            }
            return result;
        }

        public async Task<QuickTrackShipmentViewModel> GetQuickTrackAsync(string shipmentNo)
        {
            var model = await Repository.GetAsync(s => s.ShipmentNo == shipmentNo, null,
                                                    x => x.Include(m => m.ShipmentBillOfLadings)
                                                    .ThenInclude(sb => sb.BillOfLading));

            if (model == null)
                return null;

            var activities = await _activityService.GetActivities(EntityType.Shipment, model.Id);
            var shipmentActivities = Mapper.Map<List<QuickTrackActivityViewModel>>(activities);

            var result = Mapper.Map<QuickTrackShipmentViewModel>(model);
            result.Activities = shipmentActivities;
            result.Milestones = Milestone.ShipmentMileStones;

            foreach (var milestone in result.Milestones)
            {
                milestone.ActivityDate = result.Activities
                    .FirstOrDefault(a => a.ActivityCode == milestone.ActivityCode)?.ActivityDate;
            }

            return result;
        }

        /// <summary>
        /// To import shipment called via API from EdiSON
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public override async Task<ShipmentViewModel> CreateAsync(ShipmentViewModel viewModel)
        {
            var isDuplicatedNumber = await Repository.AnyAsync(x => x.ShipmentNo == viewModel.ShipmentNo);
            if (isDuplicatedNumber)
            {
                throw new ApplicationException($"Duplicated Shipment number {viewModel.ShipmentNo}.");
            }

            // Map booking to shipment
            if (!string.IsNullOrEmpty(viewModel.BookingReferenceNo))
            {
                var currentBooking = await _poFulfillmentBookingRequestRepository.GetAsync(x => x.BookingReferenceNumber == viewModel.BookingReferenceNo && x.Status == POFulfillmentBookingRequestStatus.Active,
                    includes: x => x.Include(y => y.POFulfillment).ThenInclude(y => y.Contacts));

                if (currentBooking != null)
                {
                    viewModel.FulfillmentId = currentBooking.POFulfillmentId;

                    // If bulk booking, copy destination agent contact
                    if (currentBooking.POFulfillment.FulfillmentType == FulfillmentType.Bulk)
                    {
                        var destinationAgent = viewModel.Contacts?.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.DestinationAgent, StringComparison.InvariantCultureIgnoreCase));

                        if (destinationAgent != null)
                        {
                            // Remove current destination agent before creating a new one

                            currentBooking.POFulfillment.Contacts = currentBooking.POFulfillment.Contacts.Where(c => !c.OrganizationRole.Equals(OrganizationRole.DestinationAgent)).ToList();
                            currentBooking.POFulfillment.Contacts.Add(new()
                            {
                                POFulfillmentId = currentBooking.POFulfillmentId,
                                OrganizationId = destinationAgent.OrganizationId,
                                OrganizationRole = destinationAgent.OrganizationRole,
                                CompanyName = destinationAgent.CompanyName,
                                ContactName = destinationAgent.ContactName,
                                ContactEmail = destinationAgent.ContactEmail,
                                ContactNumber = destinationAgent.ContactNumber,
                                Address = CompanyAddressLinesResolver.SplitCompanyAddressLines(destinationAgent.Address, 1),
                                AddressLine2 = CompanyAddressLinesResolver.SplitCompanyAddressLines(destinationAgent.Address, 2),
                                AddressLine3 = CompanyAddressLinesResolver.SplitCompanyAddressLines(destinationAgent.Address, 3),
                                AddressLine4 = CompanyAddressLinesResolver.SplitCompanyAddressLines(destinationAgent.Address, 4),
                                CreatedBy = viewModel.CreatedBy,
                                CreatedDate = DateTime.UtcNow,
                                UpdatedBy = viewModel.CreatedBy,
                                UpdatedDate = DateTime.UtcNow
                            });
                        }
                    }
                }
                else
                {
                    throw new ApplicationException($"Cannot find Booking Reference Number {viewModel.BookingReferenceNo} to create shipment.");
                }
            }

            // Check CarrierContractNo is existing on the system, else set to NULL
            if (!string.IsNullOrEmpty(viewModel.CarrierContractNo))
            {
                var isExisting = await _contractMasterRepository.AnyAsync(x => x.CarrierContractNo == viewModel.CarrierContractNo);
                if (!isExisting)
                {
                    viewModel.CarrierContractNo = null;
                }
            }

            return await base.CreateAsync(viewModel);
        }

        /// <summary>
        /// To update shipment called via API from EdiSON
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public override async Task<ShipmentViewModel> UpdateAsync(ShipmentViewModel viewModel, params object[] keys)
        {
            var shipmentId = (long?)keys.GetValue(0);

            if (shipmentId == null) throw new AppEntityNotFoundException($"Shipment Id must not be empty when Update");

            // Map booking to shipment
            if (!string.IsNullOrEmpty(viewModel.BookingReferenceNo))
            {
                var currentBooking = await _poFulfillmentBookingRequestRepository
                    .GetAsync(x => x.BookingReferenceNumber == viewModel.BookingReferenceNo && x.Status == POFulfillmentBookingRequestStatus.Active);

                if (currentBooking != null)
                {
                    viewModel.FulfillmentId = currentBooking.POFulfillmentId;
                }
                else
                {
                    throw new ApplicationException($"Cannot find Booking Reference Number {viewModel.BookingReferenceNo} to update shipment.");
                }
            }

            if (!string.IsNullOrWhiteSpace(viewModel.Status) && viewModel.Status.Equals(StatusType.INACTIVE, StringComparison.OrdinalIgnoreCase))
            {
                await CancelShipmentAsync(shipmentId.Value, viewModel.UpdatedBy);
            }

            viewModel.ValidateAndThrow(true);

            ShipmentModel model = await this.Repository.FindAsync(keys);

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Shipment Id is not existing in CSP when Update");
            }

            var itineraries = await _itineraryRepository.Query(s => s.ConsignmentItineraries.Any(x => x.ShipmentId == model.Id) && s.ModeOfTransport == ModeOfTransport.Sea && s.ScheduleId != null,
                                                    n => n.OrderBy(a => a.Sequence), x => x.Include(y => y.FreightScheduler)).ToListAsync();

            // The API should skip the updates of the dates if the linked Freight Scheduler already LOCKED
            if (itineraries.Count() > 0 && !itineraries.First().FreightScheduler.IsAllowExternalUpdate)
            {
                viewModel.ShipFromETDDate = model.ShipFromETDDate;
            }
            if (itineraries.Count() > 0 && !itineraries.Last().FreightScheduler.IsAllowExternalUpdate)
            {
                viewModel.ShipToETADate = model.ShipToETADate;
            }

            var previousCarrierContractNo = model.CarrierContractNo;

            Mapper.Map(viewModel, model);

            // Check CarrierContractNo is existing on the system, else set to NULL
            if (!string.IsNullOrEmpty(model.CarrierContractNo))
            {
                var isExisting = await _contractMasterRepository.AnyAsync(x => x.CarrierContractNo == model.CarrierContractNo);
                if (!isExisting)
                {
                    model.CarrierContractNo = previousCarrierContractNo;
                }
            }

            var error = await this.ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }

            this.Repository.Update(model);
            await this.UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<ShipmentViewModel>(model);
            return viewModel;
        }

        public override async Task<bool> DeleteByKeysAsync(params object[] keys)
        {
            var isDeleted = this.Repository.RemoveByKeys(keys);
            if (!isDeleted)
            {
                throw new AppEntityNotFoundException($"Shipment Id is not existing in CSP when delete");
            }

            await this.UnitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string shipmentNo)
        {
            var model = await Repository.GetAsync(s => s.ShipmentNo == shipmentNo);
            Repository.Remove(model);
            await this.UnitOfWork.SaveChangesAsync();

            OnEntityDeleted(model);
            return true;
        }

        public async Task<DashBoardSummaryGridViewModel<Top10ThisWeekViewModel>> GetTop10ThisWeekAsync(string organizationRole, bool isInternal, string affiliates, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var fromDate = DateTime.Parse(dates["FromDate"]);
            var toDate = DateTime.Parse(dates["ToDate"]);

            DateTime currentWeekStartDate = DateTime.UtcNow.WeekStartDate();
            DateTime nextWeekStartDate = DateTime.UtcNow.Date;

            string sql;
            List<SqlParameter> filterParameters;

            if (!isInternal)
            {
                var listOfAffiliates = "";
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = string.Join(",", JsonConvert.DeserializeObject<List<long>>(affiliates));
                }

                sql = @"SELECT TOP(10) SC.CompanyName AS [CompanyName],
	                            COUNT(1) AS [NoOfShipment],
	                            ROUND(SUM(S.TotalGrossWeight),2) AS [TotalGrossWeight],
	                            ROUND(SUM(S.TotalVolume),2) AS [TotalVolume]
                            FROM [ShipmentContacts] AS SC
                            INNER JOIN [Shipments] AS S ON S.Id = SC.ShipmentId
                            WHERE 
                                S.ShipFromETDDate >= @fromDate AND S.ShipFromETDDate <= @toDate
                                AND S.[Status] = 'Active'
                                AND SC.OrganizationRole = @organizationRole
                                AND EXISTS ( SELECT * FROM ShipmentContacts
		                                     WHERE ShipmentId = S.Id AND OrganizationId IN (SELECT [VALUE] FROM [dbo].[fn_SplitStringToTable] (@listOfAffiliates, ',')))
                            GROUP BY SC.CompanyName
                            ORDER BY [TotalVolume] DESC";

                // Add filter parameter
                filterParameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@organizationRole",
                        Value = organizationRole,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@fromDate",
                        Value = fromDate,
                        DbType = DbType.DateTime,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@toDate",
                        Value = toDate,
                        DbType = DbType.DateTime,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@listOfAffiliates",
                        Value = listOfAffiliates,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                };

            }
            else
            {
                sql = @"SELECT TOP(10) SC.CompanyName AS [CompanyName],
	                            COUNT(1) AS [NoOfShipment],
	                            ROUND(SUM(S.TotalGrossWeight),2) AS [TotalGrossWeight],
	                            ROUND(SUM(S.TotalVolume),2) AS [TotalVolume]
                            FROM [ShipmentContacts] AS SC
                            INNER JOIN [Shipments] AS S ON S.Id = SC.ShipmentId
                            WHERE
                                S.ShipFromETDDate >= @fromDate AND S.ShipFromETDDate <= @toDate
                                AND S.[Status] = 'Active'
                                AND SC.OrganizationRole = @organizationRole 
                            GROUP BY SC.CompanyName
                            ORDER BY [TotalVolume] DESC";

                // Add filter parameter
                filterParameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@organizationRole",
                        Value = organizationRole,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@fromDate",
                        Value = fromDate,
                        DbType = DbType.DateTime,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@toDate",
                        Value = toDate,
                        DbType = DbType.DateTime,
                        Direction = ParameterDirection.Input
                    }
                };

            }

            Func<DbDataReader, IEnumerable<Top10ThisWeekViewModel>> mappingCallback = (reader) =>
            {
                var mappedData = new List<Top10ThisWeekViewModel>();

                while (reader.Read())
                {
                    var newRow = new Top10ThisWeekViewModel
                    {
                        Name = reader[0]?.ToString() ?? "",
                        NoOfShipment = Convert.ToInt32(reader[1] ?? "0"),
                        TotalGrossWeight = Convert.ToDecimal(reader[2] ?? "0.00"),
                        TotalVolume = Convert.ToDecimal(reader[3] ?? "0.00")
                    };
                    mappedData.Add(newRow);
                }

                return mappedData;
            };

            var data = _dataQuery.GetDataBySql(sql, mappingCallback, filterParameters.ToArray());
            var result = new DashBoardSummaryGridViewModel<Top10ThisWeekViewModel>
            {
                QueryStartDate = currentWeekStartDate,
                QueryEndDate = nextWeekStartDate.AddDays(-1),
                Data = data
            };

            return result;
        }

        public async Task<DashBoardSummaryGridViewModel<Top10ThisWeekViewModel>> GetTop10CarrierThisWeekAsync(bool isInternal, string affiliates, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var fromDate = DateTime.Parse(dates["FromDate"]);
            var toDate = DateTime.Parse(dates["ToDate"]);

            DateTime currentWeekStartDate = DateTime.UtcNow.WeekStartDate();
            DateTime nextWeekStartDate = DateTime.UtcNow.Date;

            string sql;
            List<SqlParameter> filterParameters;

            if (!isInternal)
            {
                var listOfAffiliates = "";
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = string.Join(",", JsonConvert.DeserializeObject<List<long>>(affiliates));
                }
                sql = @"    SELECT TOP(10)
	                            T.CarrierName AS [Name],
                                COUNT(S.Id) AS [NoOfShipment],
                                SUM(S.TotalGrossWeight) AS [TotalGrossWeight],
	                            SUM(S.TotalVolume) AS [TotalVolume]
                            FROM [Shipments] S 
                                CROSS APPLY (
	                                SELECT DISTINCT I.CarrierName
	                                FROM [ConsignmentItineraries] CI INNER JOIN [Itineraries] I ON I.Id = CI.ItineraryId INNER JOIN [ShipmentContacts] AS SC ON S.Id = SC.ShipmentId
	                                WHERE S.Id = CI.ShipmentId
		                                AND SC.OrganizationId IN (SELECT [VALUE] FROM [dbo].[fn_SplitStringToTable] (@listOfAffiliates, ','))
	                            ) T
                            WHERE
                                S.ShipFromETDDate >= @fromDate AND S.ShipFromETDDate <= @toDate
                                AND S.[Status] = 'Active'
                            GROUP BY T.CarrierName
                            HAVING T.CarrierName IS NOT NULL AND T.CarrierName != ''
                            ORDER BY TotalVolume DESC
                      ";

                // Add filter parameter
                filterParameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@fromDate",
                        Value = fromDate,
                        DbType = DbType.DateTime,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@toDate",
                        Value = toDate,
                        DbType = DbType.DateTime,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@listOfAffiliates",
                        Value = listOfAffiliates,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };

            }
            else
            {
                sql = @"    SELECT TOP(10)
	                            T.CarrierName AS [Name],
                                COUNT(S.Id) AS [NoOfShipment],
                                SUM(S.TotalGrossWeight) AS [TotalGrossWeight],
	                            SUM(S.TotalVolume) AS [TotalVolume]
                            FROM [Shipments] S 
	                            CROSS APPLY (
                                    SELECT DISTINCT I.CarrierName
                                    FROM [ConsignmentItineraries] CI INNER JOIN [Itineraries] I ON I.Id = CI.ItineraryId
                                    WHERE S.Id = CI.ShipmentId ) T
                            WHERE
                                S.ShipFromETDDate >= @fromDate AND S.ShipFromETDDate <= @toDate
                                AND S.[Status] = 'Active'
                            GROUP BY T.CarrierName
                            HAVING T.CarrierName IS NOT NULL AND T.CarrierName != ''
                            ORDER BY TotalVolume DESC
                      ";

                // Add filter parameter
                filterParameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@fromDate",
                        Value = fromDate,
                        DbType = DbType.DateTime,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@toDate",
                        Value = toDate,
                        DbType = DbType.DateTime,
                        Direction = ParameterDirection.Input
                    }
                };

            }

            var data = _dataQuery.GetDataBySql(sql, _top10ThisWeekViewModelMapping, filterParameters.ToArray());
            var result = new DashBoardSummaryGridViewModel<Top10ThisWeekViewModel>
            {
                QueryStartDate = currentWeekStartDate,
                QueryEndDate = nextWeekStartDate.AddDays(-1),
                Data = data
            };
            return result;
        }

        private async Task<ShipmentViewModel> CancelAirShipmentAsync(long id, string userName)
        {
            var toDeleteShipment = await Repository.GetAsync(x => x.Id == id, includes: x
                => x.Include(s => s.Consignments)
                .Include(s => s.POFulfillment)
                .Include(s => s.ShipmentLoads).ThenInclude(sl => sl.ShipmentLoadDetails)
                .Include(s => s.ShipmentLoads).ThenInclude(sl => sl.Consolidation));

            if (toDeleteShipment == null)
            {
                throw new AppEntityNotFoundException($"Object with the number {string.Join(", ", id)} not found!");
            }

            if (toDeleteShipment.POFulfillmentId.HasValue)
            {
                var associatedBooking = await _poFulfillmentRepository.GetAsNoTrackingAsync(p => p.Id == toDeleteShipment.POFulfillmentId);

                if (associatedBooking.Stage == POFulfillmentStage.Closed)
                {
                    throw new AppValidationException("Cannot cancel Shipment! Associated Booking is closed.");
                }
            }

            UnitOfWork.BeginTransaction();

            toDeleteShipment.IsItineraryConfirmed = false;

            if (toDeleteShipment.Consignments != null && toDeleteShipment.Consignments.Any())
            {
                foreach (var consigment in toDeleteShipment.Consignments)
                {
                    consigment.Status = StatusType.INACTIVE;
                    consigment.Audit(userName);
                }
            }

            POFulfillmentBookingRequestModel bookingRequest = null;
            if (toDeleteShipment.POFulfillmentId.HasValue)
            {
                if (toDeleteShipment.POFulfillment.FulfillmentType == FulfillmentType.PO)
                {
                    POFulfillmentService.ReleaseQuantityOnPOLineItems(toDeleteShipment.POFulfillmentId.Value);
                }
                bookingRequest = await UpdateBookingToDraft(toDeleteShipment.POFulfillmentId.Value, userName);
            }

            #region Itineraries
            var toDeleteItineraries = new List<ItineraryModel>();
            var toUnlinkItineraries = new List<ItineraryModel>();

            var billOfLadingItineraries = new List<BillOfLadingItineraryModel>();

            // List of itineraries of deleting shipment.
            var itineraries = await _itineraryRepository.Query(i => i.ConsignmentItineraries.Any(ci => ci.ShipmentId == toDeleteShipment.Id), includes: x => x.Include(y => y.ConsignmentItineraries)
                .Include(y => y.ContainerItineraries)
                .Include(y => y.MasterBillOfLadingItineraries)
                .Include(y => y.BillOfLadingItineraries)).ToListAsync();

            if (itineraries != null && itineraries.Any())
            {
                var itineraryIds = itineraries.Select(x => x.Id).ToList();
                var inSharingItineraryIds = await _consignmentItineraryRepository.Query(ci => itineraryIds.Contains(ci.ItineraryId)).Select(x => new { x.ItineraryId, x.ShipmentId }).Distinct().ToListAsync();

                // Itineraries not sharing with other shipments
                toDeleteItineraries = itineraries.Where(x => inSharingItineraryIds.Count(y => y.ItineraryId == x.Id) == 1).ToList();
                // Itineraries sharing with other shipments
                toUnlinkItineraries = itineraries.Where(x => inSharingItineraryIds.Count(y => y.ItineraryId == x.Id) > 1).ToList();

                billOfLadingItineraries = itineraries.SelectMany(i => i.BillOfLadingItineraries).ToList();
            }

            foreach (var item in toDeleteItineraries)
            {
                // Remove appropriate linked tables
                item.ConsignmentItineraries.Clear();
                item.ConsignmentItineraries.Clear();
                item.ContainerItineraries.Clear();
                item.MasterBillOfLadingItineraries.Clear();
                item.BillOfLadingItineraries.Clear();
            }
            _itineraryRepository.RemoveRange(toDeleteItineraries.ToArray());

            foreach (var item in toUnlinkItineraries)
            {
                // Remove appropriate Consignment Itineraries
                item.ConsignmentItineraries = item.ConsignmentItineraries.Where(x => x.ShipmentId != toDeleteShipment.Id).ToList();
            }
            await UnitOfWork.SaveChangesAsync();
            #endregion Itineraries

            #region Containers
            var toDeleteContainers = new List<ContainerModel>();
            var toUnlinkContainers = new List<ContainerModel>();

            var toDeleteConsolidations = new List<ConsolidationModel>();

            // Get containers stored in database = CY + CFS
            // CY container will be removed
            var containers = await _containerRepository.Query(x => x.ShipmentLoads.Any(x => x.ShipmentId == toDeleteShipment.Id),
                                                                        includes: x => x.Include(y => y.ContainerItineraries).Include(y => y.Consolidation)
                                                                 ).ToListAsync();

            // Get CargoDetails stored in database
            var cargoDetails = await _cargoDetailRepository.Query(x => x.ShipmentId == toDeleteShipment.Id).ToListAsync();

            // Sum up ShipmentLoadDetails for further unlink calculation.
            var totalGrossWeight = cargoDetails?.Sum(x => x.GrossWeight) ?? 0;
            var totalNetWeight = cargoDetails?.Sum(x => x.NetWeight) ?? 0;
            var totalPackage = (int)(cargoDetails?.Sum(x => x.Package) ?? 0);
            var totalVolumn = cargoDetails?.Sum(x => x.Volume) ?? 0;

            if (containers != null && containers.Any())
            {
                // Need to check CFS containers in sharing
                var cfsContainers = containers.Where(x => !x.IsFCL).ToList();
                var cfsContainerIds = cfsContainers.Select(x => x.Id).ToList();

                var inSharingContainerIds = await _shipmentLoadRepository.Query(sl => sl.ContainerId != null && cfsContainerIds.Contains(sl.ContainerId.Value)).Select(x => new { x.ContainerId, x.ShipmentId }).Distinct().ToListAsync();

                // Remove CY containers
                toDeleteContainers = containers.Where(x => x.IsFCL).ToList();

                // Containers not sharing with other shipments
                toDeleteContainers.AddRange(cfsContainers.Where(x => inSharingContainerIds.Count(y => y.ContainerId == x.Id) == 1).ToList());
                // Delete Consolidations
                toDeleteConsolidations = toDeleteContainers.Where(x => x.Consolidation != null).Select(x => x.Consolidation).ToList();

                // Containers sharing with other shipments
                toUnlinkContainers = cfsContainers.Where(x => inSharingContainerIds.Count(y => y.ContainerId == x.Id) > 1).ToList();

                foreach (var item in toDeleteContainers)
                {
                    item.ContainerItineraries.Clear();
                }
                _containerRepository.RemoveRange(toDeleteContainers.ToArray());
                _consolidationRepository.RemoveRange(toDeleteConsolidations.ToArray());

                foreach (var item in toUnlinkContainers)
                {
                    item.TotalGrossWeight -= totalGrossWeight;
                    item.TotalNetWeight -= totalNetWeight;
                    item.TotalPackage -= totalPackage;
                    item.TotalVolume -= totalVolumn;

                    // Recalculate consolidation
                    item.Consolidation.TotalGrossWeight -= totalGrossWeight;
                    item.Consolidation.TotalNetWeight -= totalNetWeight;
                    item.Consolidation.TotalPackage -= totalPackage;
                    item.Consolidation.TotalVolume -= totalVolumn;
                }
            }
            #endregion Containers

            #region BillOfLadings
            var toDeleteBillOfLadings = new List<BillOfLadingModel>();
            var toUnlinkBillOfLadings = new List<BillOfLadingModel>();

            var billOfLadings = await _billOfLadingRepository.Query(bl => bl.ShipmentBillOfLadings.Any(sbl => sbl.ShipmentId == toDeleteShipment.Id),
                includes: x => x.Include(y => y.BillOfLadingConsignments).Include(y => y.BillOfLadingItineraries).Include(y => y.ShipmentBillOfLadings).Include(y => y.BillOfLadingShipmentLoads)).ToListAsync();

            if (billOfLadings != null && billOfLadings.Any())
            {
                var billOfLadingIds = billOfLadings.Select(bl => bl.Id).ToList();
                var inSharingBillOfLadingIds = await _shipmentBillOfLadingRepository.Query(sbl => billOfLadingIds.Contains(sbl.BillOfLadingId)).Select(x => new { x.BillOfLadingId, x.ShipmentId }).Distinct().ToListAsync();

                // Bill of ladings not sharing with other shipments
                toDeleteBillOfLadings = billOfLadings.Where(x => inSharingBillOfLadingIds.Count(y => y.BillOfLadingId == x.Id) == 1).ToList();

                // Bill of ladings sharing with other shipments
                toUnlinkBillOfLadings = billOfLadings.Where(x => inSharingBillOfLadingIds.Count(y => y.BillOfLadingId == x.Id) > 1).ToList();
            }

            foreach (var item in toDeleteBillOfLadings)
            {
                item.BillOfLadingConsignments.Clear();
                item.BillOfLadingItineraries.Clear();
                item.ShipmentBillOfLadings.Clear();
                item.BillOfLadingShipmentLoads.Clear();
            }
            _billOfLadingRepository.RemoveRange(toDeleteBillOfLadings.ToArray());

            foreach (var item in toUnlinkBillOfLadings)
            {
                item.BillOfLadingConsignments = item.BillOfLadingConsignments.Where(blc => blc.ShipmentId != toDeleteShipment.Id).ToList();
                item.ShipmentBillOfLadings = item.ShipmentBillOfLadings.Where(sbl => sbl.ShipmentId != toDeleteShipment.Id).ToList();
                // dbo.[BillOfLadingShipmentLoads] -> no need to unlink as it will all be deleted by shipment

                // Remove out of Consignment
                foreach (var consignment in toDeleteShipment.Consignments)
                {
                    if (consignment.HouseBillId.HasValue && consignment.HouseBillId.Value == item.Id)
                    {
                        consignment.HouseBill = null;
                    }
                }
                // Recalculate bill of lading total
                item.TotalGrossWeight -= totalGrossWeight;
                item.TotalNetWeight -= totalNetWeight;
                item.TotalPackage -= totalPackage;
                item.TotalVolume -= totalVolumn;
            }
            #endregion BillOfLadings

            #region MasterBills
            var toDeleteMasterBillOfLadings = new List<MasterBillOfLadingModel>();
            var toUnlinkMasterBillOfLadings = new List<MasterBillOfLadingModel>();

            var masterBillOfLadings = await _masterBillOfLadingRepository.Query(mb => mb.Consignments.Any(csm => csm.ShipmentId == toDeleteShipment.Id), includes: x => x.Include(y => y.MasterBillOfLadingItineraries)).ToListAsync();

            if (masterBillOfLadings != null && masterBillOfLadings.Any())
            {
                var masterBillOfLadingIds = masterBillOfLadings.Select(bl => bl.Id).ToList();
                var inSharingMasterBillOfLadingIds = await _consignmentRepository.Query(csm => csm.MasterBillId != null && masterBillOfLadingIds.Contains(csm.MasterBillId.Value)).Select(x => new { x.MasterBillId, x.ShipmentId }).Distinct().ToListAsync();

                // Master bills not sharing with other shipments
                toDeleteMasterBillOfLadings = masterBillOfLadings.Where(x => inSharingMasterBillOfLadingIds.Count(y => y.MasterBillId == x.Id) == 1).ToList();

                // Master bills sharing with other shipments
                toUnlinkMasterBillOfLadings = masterBillOfLadings.Where(x => inSharingMasterBillOfLadingIds.Count(y => y.MasterBillId == x.Id) > 1).ToList();
            }
            toDeleteMasterBillOfLadings.ForEach(mb => mb.MasterBillOfLadingItineraries.Clear());
            _masterBillOfLadingRepository.RemoveRange(toDeleteMasterBillOfLadings.ToArray());

            // Unlink Bill of lading out of the cancelling Shipment.
            foreach (var item in toUnlinkMasterBillOfLadings)
            {
                // Unlink out of Consignments
                foreach (var consignment in toDeleteShipment.Consignments)
                {
                    if (consignment.MasterBillId.HasValue && consignment.MasterBillId.Value == item.Id)
                    {
                        consignment.MasterBill = null;
                    }
                }

                // dbo.[ConsignmentItineraries] -> no need to unlink as it will all be deleted by unlinking/deleting itineraries
                // dbo.[BillOfLadingShipmentLoads] -> no need to unlink as it will all be deleted by shipment

                // Unlink out of BillOfLadingItineraries
                foreach (var billOfLadingItinerary in billOfLadingItineraries)
                {
                    if (billOfLadingItinerary.MasterBillOfLadingId.HasValue && billOfLadingItinerary.MasterBillOfLadingId.Value == item.Id)
                    {
                        billOfLadingItinerary.MasterBillOfLading = null;
                    }
                }
            }
            #endregion MasterBills

            #region ShipmentLoads
            var toDeleteShipmentLoads = new List<ShipmentLoadModel>();
            if (toDeleteShipment.ShipmentLoads != null && toDeleteShipment.ShipmentLoads.Any())
            {
                toDeleteShipmentLoads = toDeleteShipment.ShipmentLoads.ToList();
            }
            _shipmentLoadRepository.RemoveRange(toDeleteShipmentLoads.ToArray());
            #endregion ShipmentLoads

            toDeleteShipment.SetCancelledStatus();
            toDeleteShipment.Audit(userName);

            await UnitOfWork.SaveChangesAsync();

            await Trigger1070EventAsync(toDeleteShipment, userName);

            UnitOfWork.CommitTransaction();

            return Mapper.Map<ShipmentViewModel>(toDeleteShipment);
        }

        public async Task<ShipmentViewModel> CancelShipmentAsync(long id, string userName)
        {
            var shipment = await Repository.GetAsync(s => s.Id == id,
                null,
                i => i.Include(s => s.Consignments)
                .Include(s => s.POFulfillment).ThenInclude(x => x.ShortshipOrders)
                .Include(s => s.ShipmentLoads).ThenInclude(sl => sl.ShipmentLoadDetails)
                .Include(s => s.ShipmentLoads).ThenInclude(sl => sl.Consolidation)
                .Include(s => s.ShipmentLoads).ThenInclude(sl => sl.BillOfLadingShipmentLoads)
                .Include(s => s.ShipmentBillOfLadings));

            if (shipment == null) throw new AppEntityNotFoundException($"Shipment Id is not existing in CSP when Update");

            if (shipment.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.OrdinalIgnoreCase))
            {
                return await CancelAirShipmentAsync(id, userName);
            }

            var validationResult = await ValidateCancelingShipment(shipment);
            if (!validationResult)
            {
                return Mapper.Map<ShipmentViewModel>(shipment);
            }

            this.UnitOfWork.BeginTransaction();
            shipment.Audit(userName);

            foreach (var consigment in shipment.Consignments)
            {
                consigment.Status = StatusType.INACTIVE;
                consigment.Audit(userName);
            }

            var containerIdList = shipment.ShipmentLoads.Select(sl => sl.ContainerId);
            var containers = await _containerRepository.Query(c => containerIdList.Contains(c.Id),
                null,
                i => i.Include(c => c.ContainerItineraries)
                .Include(c => c.Consolidation)
                .Include(c => c.ShipmentLoads)
                .Include(c => c.ShipmentLoadDetails)
                .Include(c => c.BillOfLadingShipmentLoads)).ToListAsync();
            var shipmentLoads = shipment.ShipmentLoads;

            #region Data hard-delete on containers and shipment loads

            if ((containers != null && containers.Any()) || (shipmentLoads != null && shipmentLoads.Any()))
            {
                // Attentions: It is going to permanently remove data
                // Pls add business information logging

                var telemetry = new TraceTelemetry("Data hard-delete", SeverityLevel.Information);
                telemetry.Properties.Add("executed-on", _telemetryConfig.Source);
                telemetry.Properties.Add("executed-component", "ShipmentService");
                telemetry.Properties.Add("executed-method", "CancelShipmentAsync");
                telemetry.Properties.Add("params-id", id.ToString());
                telemetry.Properties.Add("params-userName", userName);

                // Remove related containers
                var relatedContainers = new List<ContainerModel>();
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
                    container.BillOfLadingShipmentLoads.Clear();
                    container.ShipmentLoads.Clear();
                    container.ShipmentLoadDetails.Clear();
                    container.Consolidation = null;
                    relatedContainers.Add(container);
                }
                _containerRepository.RemoveRange(relatedContainers.ToArray());
                await UnitOfWork.SaveChangesAsync();

                //Remove related shipment loads
                var relatedShipmentLoads = new List<ShipmentLoadModel>();
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
                    relatedShipmentLoads.Add(shipmentLoad);
                }
                _shipmentLoadRepository.RemoveRange(relatedShipmentLoads.ToArray());
                await UnitOfWork.SaveChangesAsync();

                // Need to update total amount for related consolidations
                foreach (var shipmentLoad in relatedShipmentLoads)
                {
                    if (shipmentLoad.ConsolidationId.HasValue)
                    {
                        await _consolidationService.UpdateConsolidationTotalAmountAsync(shipmentLoad.ConsolidationId.Value);
                    }
                }
                // Write the log 
                TelemetryClient.TrackTrace(telemetry);
            }

            #endregion

            POFulfillmentBookingRequestModel bookingRequest = null;
            if (shipment.POFulfillmentId.HasValue)
            {
                if (shipment.POFulfillment.FulfillmentType == FulfillmentType.PO)
                {
                    POFulfillmentService.ReleaseQuantityOnPOLineItems(shipment.POFulfillmentId.Value);
                }
                bookingRequest = await UpdateBookingToDraft(shipment.POFulfillmentId.Value, userName);

                //clear all shortship orders
                shipment.POFulfillment.ShortshipOrders.Clear();
            }

            shipment.SetCancelledStatus();
            await this.UnitOfWork.SaveChangesAsync();

            if (bookingRequest != null)
            {
                try
                {
                    await _ediSonBookingService.CancelBookingRequest(bookingRequest);
                }
                catch
                {
                    this.UnitOfWork.RollbackTransaction();
                    throw;
                }
            }

            await Trigger1070EventAsync(shipment, userName);

            this.UnitOfWork.CommitTransaction();

            var result = Mapper.Map<ShipmentViewModel>(shipment);
            return result;
        }

        public async Task Trigger1070EventAsync(ShipmentModel shipmentModel, string userName)
        {
            if (shipmentModel.POFulfillmentId.HasValue)
            {
                var event1070 = new ActivityViewModel()
                {
                    ActivityCode = Event.EVENT_1070,
                    POFulfillmentId = shipmentModel.POFulfillmentId,
                    ActivityDate = DateTime.UtcNow,
                    CreatedBy = userName,
                    Remark = shipmentModel.ShipmentNo
                };
                await _activityService.TriggerAnEvent(event1070);
            }
        }

        public async Task TrialValidateOnAssignHouseBLAsync(long id)
        {
            var shipment = await Repository.GetAsNoTrackingAsync(s => s.Id == id,
                null,
                i => i.Include(s => s.ShipmentLoadDetails)
                .Include(s => s.ShipmentLoads));

            if (shipment.IsFCL)
            {
                if (shipment.ShipmentLoadDetails == null || shipment.ShipmentLoadDetails.Count == 0)
                {
                    throw new AppValidationException("unconfirmedContainer#Please confirm container before assigning the House.");
                }
            }
            else
            {
                var linkedConsolidationIds = shipment.ShipmentLoads.Where(sl => sl.ConsolidationId.HasValue)
                    .Select(sl => sl.ConsolidationId)
                    .ToList();

                if (linkedConsolidationIds?.Count > 0)
                {
                    var hasUnconfirmedConsolidation = await _consolidationRepository.AnyAsync(
                        c => linkedConsolidationIds.Contains(c.Id) && c.Stage != ConsolidationStage.Confirmed);

                    if (hasUnconfirmedConsolidation)
                    {
                        throw new AppValidationException("unconfirmedConsolidation#Please confirm consolidation before assigning the House.");
                    }
                }
                else
                {
                    throw new AppValidationException("unconfirmedConsolidation#Please confirm consolidation before assigning the House.");
                }
            }
        }

        public async Task TrialValidationOnCancelShipmentAsync(long id)
        {
            var shipment = await Repository.GetAsync(s => s.Id == id,
                null,
                i => i.Include(s => s.Consignments)
                .Include(s => s.ShipmentLoads).ThenInclude(sl => sl.ShipmentLoadDetails)
                .Include(s => s.ShipmentLoads).ThenInclude(sl => sl.Consolidation)
                .Include(s => s.ShipmentBillOfLadings));

            var validationResult = await ValidateCancelingShipment(shipment);
        }

        private async Task<bool> ValidateCancelingShipment(ShipmentModel shipment)
        {
            if (shipment.Status.Equals(StatusType.INACTIVE, StringComparison.OrdinalIgnoreCase))
            {
                // we don't throw exceptions to avoid the retry behavior from ediSon
                // throw new AppValidationException("Cannot cancel Shipment! Shipment has been inactive.");
                return false;
            }

            if (shipment.POFulfillmentId.HasValue)
            {
                var associatedBooking = await _poFulfillmentRepository.GetAsNoTrackingAsync(p => p.Id == shipment.POFulfillmentId);

                if (associatedBooking.Stage == POFulfillmentStage.Closed)
                {
                    // we don't throw exceptions to avoid the retry behavior from ediSon
                    // throw new AppValidationException("Cannot cancel Shipment! Associated Booking is closed.");
                    return false;
                }
            }

            // Validate: Not allow to cancel shipment as container is already loaded.
            if (IsLoadedShipment(shipment))
            {
                throw new AppValidationException("shipmentAlreadyLoaded#Not allow to cancel the shipment because its container is already loaded.");
            }

            //Validate: Not allow to cancel if shipment has been linked to House BL/Master BL.

            if ((shipment.ShipmentBillOfLadings?.Count > 0 ||
            shipment.Consignments.Any(c => c.MasterBillId.HasValue)) && shipment.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.OrdinalIgnoreCase))
            {
                throw new AppValidationException("shipmentHasBLLinked#In order to cancel the shipment, you have to unlink the House / the Master.");
            }

            return true;
        }

        private async Task ValidateAndThrowCancelingShipmentAsync(ShipmentModel shipment)
        {
            if (shipment.POFulfillmentId.HasValue)
            {
                var associatedBooking = await _poFulfillmentRepository.GetAsNoTrackingAsync(p => p.Id == shipment.POFulfillmentId);

                if (associatedBooking.Stage == POFulfillmentStage.Closed)
                {
                    throw new AppValidationException("Cannot cancel Shipment! Associated Booking is closed.");
                }
            }

            // Validate: Not allow to cancel shipment as container is already loaded.
            if (IsLoadedShipment(shipment))
            {
                throw new AppValidationException("shipmentAlreadyLoaded#Not allow to cancel the shipment because its container is already loaded.");
            }
        }

        /// <summary>
        /// Check if the shipment cargos are not loaded into Container yet
        /// <br/> Notes: There is no data on ShipmentLoadDetails
        /// </summary>
        /// <param name="shipment"></param>
        /// <returns></returns>
        private bool IsLoadedShipment(ShipmentModel shipment)
        {
            bool isAlreadyLoaded = false;

            if (shipment.ShipmentLoads == null)
            {
                return isAlreadyLoaded;
            }

            if (!shipment.IsFCL)
            {
                isAlreadyLoaded = shipment.ShipmentLoads.Any(sl
                    => sl.Consolidation?.Stage == ConsolidationStage.Confirmed);
            }

            return isAlreadyLoaded;
        }

        private async Task<POFulfillmentBookingRequestModel> UpdateBookingToDraft(long poFulFillmentId, string userName)
        {
            var poff = await _poFulfillmentRepository.GetAsync(p => p.Id == poFulFillmentId,
                null,
                i => i.Include(p => p.Loads)
                .Include(p => p.Orders)
                .Include(p => p.Itineraries)
                .Include(p => p.BookingRequests)
                .Include(p => p.BuyerApprovals));

            poff.Stage = POFulfillmentStage.Draft;
            poff.BookingDate = null;
            poff.IsForwarderBookingItineraryReady = false;
            poff.IsGeneratePlanToShip = false;
            poff.Audit(userName);

            foreach (var itinerary in poff.Itineraries)
            {
                itinerary.Status = POFulfillmentItinerayStatus.Inactive;
                itinerary.Audit(userName);
            }

            if (poff.FulfillmentType == FulfillmentType.PO)
            {
                await UpdatePurchaseOrderStageByPOFFAsync(poff);
            }

            // Deactivate booking request
            var bookingRequest = poff.BookingRequests.SingleOrDefault(br => br.Status == POFulfillmentBookingRequestStatus.Active);
            if (bookingRequest != null)
            {
                bookingRequest.Status = POFulfillmentBookingRequestStatus.Inactive;
            }

            // Cancel pending approval
            if (poff.FulfillmentType == FulfillmentType.PO)
            {
                var pendingBuyerApproval = poff.BuyerApprovals.SingleOrDefault(x => x.Stage == BuyerApprovalStage.Pending);
                if (pendingBuyerApproval != null)
                {
                    pendingBuyerApproval.Stage = BuyerApprovalStage.Cancel;
                }
            }

            return bookingRequest;
        }

        private Func<DbDataReader, IEnumerable<Top10ThisWeekViewModel>> _top10ThisWeekViewModelMapping = (reader) =>
        {
            var mappedData = new List<Top10ThisWeekViewModel>();

            while (reader.Read())
            {
                var newRow = new Top10ThisWeekViewModel
                {
                    Name = reader[0]?.ToString() ?? "",
                    NoOfShipment = Convert.ToInt32(reader[1] ?? "0"),
                    TotalGrossWeight = Convert.ToDecimal(reader[2] ?? "0.00"),
                    TotalVolume = Convert.ToDecimal(reader[3] ?? "0.00")
                };
                mappedData.Add(newRow);
            }

            return mappedData;
        };

        public async Task<IEnumerable<DropDownListItem<long>>> SearchShipmentsByShipmentNumberAsync(string shipmentNumber, long cruiseOrderId)
        {
            var storedProcedureName = "spu_GetShipmentSelectionList_Cruise";
            List<SqlParameter> filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@shipmentNumber",
                        Value = shipmentNumber,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@cruiseOrderId",
                        Value = cruiseOrderId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, IEnumerable<DropDownListItem<long>>> mapping = (reader) =>
            {
                // Default value
                var mappedData = new List<DropDownListItem<long>>();
                while (reader.Read())
                {
                    var dataRow = new DropDownListItem<long>
                    {
                        Value = (long)reader[0],
                        Text = (string)reader[1]
                    };
                    mappedData.Add(dataRow);
                }
                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }

        public async Task<IEnumerable<DropDownListItem<long>>> SearchShipmentNumberByConsolidationAsync(long consolidationId, string shipmentNumber, bool isInternal, long organizationId)
        {
            var storedProcedureName = "spu_GetShipmentSelectionList_Consolidation";
            List<SqlParameter> filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@shipmentNumber",
                        Value = shipmentNumber,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@consolidationId",
                        Value = consolidationId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@isInternal",
                        Value = isInternal,
                        DbType = DbType.Boolean,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@organizationId",
                        Value = organizationId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                };

            Func<DbDataReader, IEnumerable<DropDownListItem<long>>> mapping = (reader) =>
            {
                // Default value
                var mappedData = new List<DropDownListItem<long>>();
                while (reader.Read())
                {
                    var dataRow = new DropDownListItem<long>
                    {
                        Value = (long)reader[0],
                        Text = (string)reader[1]
                    };
                    mappedData.Add(dataRow);
                }
                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }

        public async Task<bool> IsFirstChildShipmentDispatch(long shipmentId)
        {
            List<string> dispatchEventCode = new List<string>() { Event.EVENT_2029, Event.EVENT_2039, Event.EVENT_2054 };

            var poff = await _poFulfillmentRepository.GetAsNoTrackingAsync(x => x.Shipments.Any(s => s.Id == shipmentId),
                null,
                x => x.Include(i => i.Shipments));

            if (poff == null)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }

            var otherChildShipments = poff.Shipments.Where(s =>
                s.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase) &&
                s.Id != shipmentId);

            foreach (var otherChildShipment in otherChildShipments)
            {
                var globalActivityId = CommonHelper.GenerateGlobalId(otherChildShipment.Id, EntityType.Shipment);
                var activities = _activityRepository.QueryAsNoTracking(a => a.GlobalIdActivities.Any(g => g.GlobalId == globalActivityId) && dispatchEventCode.Contains(a.ActivityCode),
                    null,
                    i => i.Include(a => a.GlobalIdActivities));

                if (activities != null && activities.Any())
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> IsOtherChildShipmentsClosed(long poFulfillmentId, long shipmentId)
        {
            var closedShipmentEvents = new[] { Event.EVENT_2039, Event.EVENT_2054 };
            IEnumerable<ShipmentModel> otherChildShipments = await Repository.Query(s => s.POFulfillmentId == poFulfillmentId
                                && s.Id != shipmentId && s.Status == StatusType.ACTIVE).ToListAsync();

            foreach (var childShipment in otherChildShipments)
            {
                string globalId = CommonHelper.GenerateGlobalId(childShipment.Id, EntityType.Shipment);

                bool isShipmentClosed = await _activityRepository.AnyAsync(a => a.GlobalIdActivities.Any(g => g.GlobalId == globalId)
                    && closedShipmentEvents.Contains(a.ActivityCode));

                if (!isShipmentClosed)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task RevertAllocatedPurchaseOrderToFBConfirmedAsync(long shipmentId, string userName)
        {
            var shipment = await Repository.GetAsync(x => x.Id == shipmentId,
                null,
                i => i.Include(m => m.POFulfillmentAllocatedOrders));

            if (shipment == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", shipmentId)} not found!");
            }
            // Update associated POs
            var purchaseOrderIdList = shipment.POFulfillmentAllocatedOrders.Select(x => x.PurchaseOrderId).Distinct();
            var purchaseOrderList = await _purchaseOrderRepository.Query(po => purchaseOrderIdList.Any(poId => po.Id == poId)).ToListAsync();
            foreach (var po in purchaseOrderList)
            {
                if (po.Stage == POStageType.ShipmentDispatch)
                {
                    po.Stage = POStageType.ForwarderBookingConfirmed;
                }
            }

            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// To update stage of purchase order as moving backward stage of purchase order fulfillment
        /// </summary>
        /// <param name="poff"></param>
        /// <returns></returns>
        private async Task UpdatePurchaseOrderStageByPOFFAsync(POFulfillmentModel poff)
        {
            await POFulfillmentService.UpdatePurchaseOrderStageByPOFFAsync(poff);
        }

        /// <summary>
        /// To create linked to shipment and update some table
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <param name="billOfLadingId"></param>
        /// <param name="executionAgentId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task AssignHouseBLToShipmentAsync(long shipmentId, long billOfLadingId, long executionAgentId, string userName)
        {
            var query =
                         Repository.GetAsNoTrackingAsync(i => i.Id == shipmentId,
                         includes: x => x.Include(s => s.Contacts)
                        .Include(s => s.ShipmentLoads).ThenInclude(sm => sm.BillOfLadingShipmentLoads).ThenInclude(blsl => blsl.MasterBillOfLading)
                        .Include(s => s.Consignments)
                        .Include(s => s.ShipmentLoads)
                        .Include(s => s.ShipmentLoadDetails)
                        .Include(c => c.ConsignmentItineraries).ThenInclude(ci => ci.Itinerary).IgnoreQueryFilters());
            var shipmentModel = await query;
            var masterBL = await _consignmentRepository.QueryAsNoTracking(c => c.HouseBillId == billOfLadingId).FirstOrDefaultAsync();
            var masterBLId = masterBL?.MasterBillId;

            await CreateShipmentBillOfLadingAsync(shipmentId, billOfLadingId, userName);

            var consignmentId = shipmentModel.Consignments.FirstOrDefault(c => c.ExecutionAgentId == executionAgentId).Id;
            await CreateBillOfLadingConsignmentAsync(shipmentId, consignmentId, billOfLadingId, userName);

            await CreateBillOfLadingShipmentLoadAsync(shipmentModel.ShipmentLoads, billOfLadingId, masterBLId, userName);

            var billOfLadingModel =
                           await _billOfLadingRepository.GetListQueryable(c => c
                                .Include(s => s.BillOfLadingItineraries)
                                .Include(s => s.Contacts)
                            ).FirstOrDefaultAsync(c => c.Id == billOfLadingId);

            var consignmentItineraries = shipmentModel.ConsignmentItineraries.Where(x => x.Itinerary.ModeOfTransport.Equals(shipmentModel.ModeOfTransport, StringComparison.OrdinalIgnoreCase)).ToList();

            UpdateBillOfLadingItineraries(consignmentItineraries, masterBLId, billOfLadingModel, userName);
            UpdateBillOfLadingContacts(shipmentModel.Contacts, billOfLadingModel, userName);

            await UpdateConsignmentAsync(shipmentId, executionAgentId, billOfLadingId, masterBLId, userName);
            ReCalculateValueBillOfLading(billOfLadingModel, shipmentModel.ShipmentLoadDetails, userName);
            _billOfLadingRepository.Update(billOfLadingModel);

            await UnitOfWork.SaveChangesAsync();

            // Update related Shipment.CarrierContractNo

            // [dbo].[spu_UpdateShipmentCarrierContractNo]
            // @masterBOLId BIGINT = NULL,
            // @shipmentId BIGINT = NULL,
            // @updatedBy NVARCHAR(512)	

            var sql = @"spu_UpdateShipmentCarrierContractNo 
                        @p0,
	                    @p1,
	                   	@p2";
            var parameters = new object[]
            {
                null,
                shipmentId,
                userName
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());
        }

        private async Task CreateShipmentBillOfLadingAsync(long shipmentId, long billOfLadingId, string userName)
        {
            var shipmentBillOfLadingModel = new ShipmentBillOfLadingModel()
            {
                ShipmentId = shipmentId,
                BillOfLadingId = billOfLadingId
            };
            shipmentBillOfLadingModel.Audit(userName);
            await _shipmentBillOfLadingRepository.AddAsync(shipmentBillOfLadingModel);
        }

        private async Task CreateBillOfLadingConsignmentAsync(long shipmentId, long consignmentId, long billOfLadingId, string userName)
        {
            var billOfLadingConsignmentModel = new BillOfLadingConsignmentModel()
            {
                ShipmentId = shipmentId,
                ConsignmentId = consignmentId,
                BillOfLadingId = billOfLadingId
            };
            billOfLadingConsignmentModel.Audit(userName);
            await _billOfLadingConsignmentRepository.AddAsync(billOfLadingConsignmentModel);
        }

        private async Task CreateBillOfLadingShipmentLoadAsync(IEnumerable<ShipmentLoadModel> shipmentLoads, long billOfLadingId, long? masterBillId, string userName)
        {
            var billOfLadingShipmentLoads = new List<BillOfLadingShipmentLoadModel>();
            foreach (var shipmentLoad in shipmentLoads)
            {
                var billOfLadingShipmentLoad = new BillOfLadingShipmentLoadModel()
                {
                    BillOfLadingId = billOfLadingId,
                    ShipmentLoadId = shipmentLoad.Id,
                    ContainerId = shipmentLoad.ContainerId,
                    ConsolidationId = shipmentLoad.ConsolidationId,
                    MasterBillOfLadingId = masterBillId,
                    IsFCL = shipmentLoad.IsFCL
                };
                billOfLadingShipmentLoad.Audit(userName);
                billOfLadingShipmentLoads.Add(billOfLadingShipmentLoad);
            }
            await _billOfLadingShipmentLoadRepository.AddRangeAsync(billOfLadingShipmentLoads.ToArray());
        }

        private void UpdateBillOfLadingItineraries(IEnumerable<ConsignmentItineraryModel> consignmentItineraries, long? masterBLId, BillOfLadingModel billOfLadingModel, string userName)
        {
            if (consignmentItineraries != null)
            {
                var newConsignmentItineraries = consignmentItineraries.Where(c => !billOfLadingModel.BillOfLadingItineraries.Any(s => s.ItineraryId == c.ItineraryId)).ToList();

                foreach (var item in newConsignmentItineraries)
                {
                    var billOfLadingItinerary = new BillOfLadingItineraryModel()
                    {
                        ItineraryId = item.ItineraryId,
                        BillOfLadingId = billOfLadingModel.Id,
                        MasterBillOfLadingId = masterBLId
                    };
                    billOfLadingItinerary.Audit(userName);
                    billOfLadingModel.BillOfLadingItineraries.Add(billOfLadingItinerary);
                }
            }
        }

        private void UpdateBillOfLadingContacts(IEnumerable<ShipmentContactModel> shipmentContacts, BillOfLadingModel billOfLadingModel, string userName)
        {
            var newContacts = shipmentContacts.Where(c => !billOfLadingModel.Contacts.Any(a => a.OrganizationId == c.OrganizationId && a.OrganizationRole == c.OrganizationRole)).ToList();
            foreach (var item in newContacts)
            {
                var billOfLadingContactModel = new BillOfLadingContactModel()
                {
                    BillOfLadingId = billOfLadingModel.Id,
                    OrganizationId = item.OrganizationId,
                    OrganizationRole = item.OrganizationRole,
                    CompanyName = item.CompanyName,
                    Address = item.Address,
                    ContactName = item.ContactName,
                    ContactNumber = item.ContactNumber,
                    ContactEmail = item.ContactEmail
                };
                billOfLadingContactModel.Audit(userName);
                billOfLadingModel.Contacts.Add(billOfLadingContactModel);
            }
        }

        private async Task UpdateConsignmentAsync(long shipmentId, long executionAgentId, long? houseBLId, long? masterBLId, string userName)
        {
            var consignmentModel = await _consignmentRepository.GetAsync(c => c.ShipmentId == shipmentId && c.ExecutionAgentId == executionAgentId, null, includes: x => x.Include(y => y.ConsignmentItineraries));
            consignmentModel.HouseBillId = houseBLId;
            consignmentModel.MasterBillId = masterBLId;
            consignmentModel.Audit(userName);
            foreach (var item in consignmentModel.ConsignmentItineraries)
            {
                item.MasterBillId = masterBLId;
                item.Audit(userName);
            }
            _consignmentRepository.Update(consignmentModel);
        }

        private void ReCalculateValueBillOfLading(BillOfLadingModel billOfLadingModel, IEnumerable<ShipmentLoadDetailModel> shipmentLoadDetails, string userName)
        {
            var totalNetWeight = shipmentLoadDetails.Where(c => c.NetWeight != null).Sum(c => (decimal?)c.NetWeight) ?? 0;
            billOfLadingModel.TotalGrossWeight = billOfLadingModel.TotalGrossWeight + shipmentLoadDetails.Sum(c => c.GrossWeight);
            billOfLadingModel.TotalNetWeight = billOfLadingModel.TotalNetWeight + totalNetWeight;
            billOfLadingModel.TotalPackage = billOfLadingModel.TotalPackage + shipmentLoadDetails.Sum(c => c.Package);
            billOfLadingModel.TotalVolume = billOfLadingModel.TotalVolume + shipmentLoadDetails.Sum(c => c.Volume);
            billOfLadingModel.Audit(userName);
        }

        public async Task CreateAndAssignHouseBLAsync(long shipmentId, BillOfLadingViewModel billOfLadingViewModel, string userName)
        {
            try
            {
                UnitOfWork.BeginTransaction();
                var billOfLadingModel = Mapper.Map<BillOfLadingModel>(billOfLadingViewModel);
                billOfLadingModel.Audit(userName);
                await _billOfLadingRepository.AddAsync(billOfLadingModel);
                await UnitOfWork.SaveChangesAsync();
                await AssignHouseBLToShipmentAsync(shipmentId, billOfLadingModel.Id, billOfLadingViewModel.ExecutionAgentId ?? 0, userName);
                UnitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                UnitOfWork.RollbackTransaction();
                throw;
            }
        }

        public async Task<bool> IsFullLoadShipmentAsync(long shipmentId)
        {
            var model = await Repository.GetAsNoTrackingAsync(s => s.Id == shipmentId,
                null,
                s => s.Include(i => i.ShipmentLoadDetails).Include(i => i.CargoDetails));

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the key {shipmentId} not found!");
            }

            foreach (var item in model?.CargoDetails)
            {
                var totalPackage = model.ShipmentLoadDetails?.Where(sld => sld.CargoDetailId == item.Id)
                    .Sum(sld => sld.Package) ?? 0;

                if (totalPackage < item.Package)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> AssignMasterBLToShipmentAsync(long shipmentId, long masterBOLId, string userName)
        {
            //   [dbo].[spu_AssignMasterBOLToShipment]
            //   @shipmentId BIGINT,
            //   @masterBOLId BIGINT,
            //   @updatedBy NVARCHAR(512)

            var sql = @"spu_AssignMasterBOLToShipment 
                        @p0,
	                    @p1,
	                   	@p2";
            var parameters = new object[]
            {
                shipmentId,
                masterBOLId,
                userName
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

            // Update related Shipment.CarrierContractNo

            // [dbo].[spu_UpdateShipmentCarrierContractNo]
            // @masterBOLId BIGINT = NULL,
            // @shipmentId BIGINT = NULL,
            // @updatedBy NVARCHAR(512)	

            sql = @"spu_UpdateShipmentCarrierContractNo 
                        @p0,
	                    @p1,
	                   	@p2";
            parameters = new object[]
            {
                null,
                shipmentId,
                userName
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

            return true;
        }

        public async Task<bool> UnlinkMasterBillOfLadingAsync(long shipmentId, string userName)
        {
            //   [dbo].[spu_UnlinkMasterBOLFromShipment]
            //   @shipmentId BIGINT,
            //   @updatedBy NVARCHAR(512)

            var sql = @"spu_UnlinkMasterBOLFromShipment 
                        @p0,
	                    @p1";
            var parameters = new object[]
            {
                shipmentId,
                userName
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());
            return true;
        }

        public async Task<bool> EditShipmentAsync(long shipmentId, UpdateShipmentViewModel viewModel, string userName)
        {
            viewModel.ValidateAndThrow(true);

            var model = await Repository.GetAsync(x => x.Id == shipmentId, includes: x => x.Include(y => y.Consignments).Include(y => y.ShipmentLoads).Include(c => c.ShipmentBillOfLadings).ThenInclude(c => c.BillOfLading));

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {shipmentId} not found!");
            }

            var masterBillIds = model.Consignments.Where(x => x.MasterBillId != null).Select(x => x.MasterBillId);
            var containerIds = model.ShipmentLoads.Where(x => x.ContainerId != null).Select(x => x.ContainerId);


            if (viewModel.Movement != model.Movement && !model.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.OrdinalIgnoreCase))
            {
                // Update movement to related tables.

                foreach (var consignment in model.Consignments)
                {
                    consignment.Movement = viewModel.Movement;
                    consignment.Audit(userName);
                }

                if (containerIds.Any())
                {
                    var containers = await _containerRepository.Query(x => containerIds.Contains(x.Id)).ToListAsync();
                    foreach (var container in containers)
                    {
                        container.Movement = viewModel.Movement;
                        container.Audit(userName);
                    }
                }

                foreach (var shipmentBillOfLading in model.ShipmentBillOfLadings)
                {
                    shipmentBillOfLading.BillOfLading.Movement = viewModel.Movement;
                }
            }
            else
            {
                // Movement does not update

            }
            // If MasterBL assigned, not allow to change CarrierContractNo
            var previousCarrierContractNo = model.CarrierContractNo;
            Mapper.Map(viewModel, model);

            var isMasterBLAssigned = model.Consignments?.Any(x => x.MasterBillId != null) ?? false;
            if (isMasterBLAssigned)
            {
                model.CarrierContractNo = previousCarrierContractNo;
            }

            var error = await ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }
            model.Audit(userName);
            Repository.Update(model);
            await UnitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ShipmentLoadDetailViewModel>> GetShipmentLoadDetailsAsync(long shipmentId)
        {
            var storedData = await _shipmentLoadRepository.QueryAsNoTracking(x => x.ShipmentId == shipmentId, null, x => x.Include(y => y.ShipmentLoadDetails)).ToListAsync();
            var result = Mapper.Map<IEnumerable<ShipmentLoadDetailViewModel>>(storedData.SelectMany(x => x.ShipmentLoadDetails));
            return result;
        }

        public async Task<int> CountNumberOfEvent2054Async(long shipmentId)
        {
            var events2054 = await _globalIdActivityRepository.QueryAsNoTracking(c => c.GlobalId == $"{EntityType.Shipment}_{shipmentId}" && c.Activity.ActivityCode == Event.EVENT_2054, null, c => c.Include(c => c.Activity)).ToListAsync();
            return events2054.Count();
        }

        public async Task<DateTime?> GetDefaultCFSClosingDateAsync(long shipmentId)
        {
            // query all itineraries linked with shipment.
            var itineraryQuery = _itineraryRepository.QueryAsNoTracking(x => x.ConsignmentItineraries.Any(a => a.ShipmentId == shipmentId));

            // 1st itinerary
            var firstItinerary = await itineraryQuery.OrderBy(x => x.Sequence).FirstOrDefaultAsync();

            if (firstItinerary == null)
            {
                return null;
            }

            // query all other shipments linked with 1st schedule.
            var shipmentQuery = Repository.QueryAsNoTracking(x => x.ConsignmentItineraries.Any(a => a.Itinerary.ScheduleId == firstItinerary.ScheduleId)
                && x.Status == StatusType.ACTIVE
                && x.Id != shipmentId
                && x.POFulfillment.Stage == POFulfillmentStage.ForwarderBookingConfirmed
                && x.POFulfillment.CFSClosingDate != null,
                includes: i => i.Include(x => x.POFulfillment).Include(x => x.ConsignmentItineraries).ThenInclude(x => x.Itinerary));

            // to list ConsignmentItineraries
            var allConsignmentItineraries = await shipmentQuery.SelectMany(x => x.ConsignmentItineraries).ToListAsync();

            // to list first Itineraries by Shipment
            var groupedShipmentItineraries = allConsignmentItineraries.GroupBy(x => x.ShipmentId).Select(x => new
            {
                ShipmentId = x.Key,
                FirstItinerary = x.Select(x => x.Itinerary).OrderBy(x => x.Sequence).FirstOrDefault()
            });

            var matchedShipmentId = groupedShipmentItineraries.FirstOrDefault(x => x.FirstItinerary.ScheduleId == firstItinerary.ScheduleId)?.ShipmentId;

            if (matchedShipmentId != null)
            {
                var matchedShipment = await shipmentQuery.Where(x => x.Id == matchedShipmentId).SingleOrDefaultAsync();
                return matchedShipment?.POFulfillment.CFSClosingDate;
            }

            return null;
        }

        public async Task<ShipmentMilestoneViewModel> GetCurrentMilestoneAsync(long shipmentId)
        {
            var acts = await _activityService.GetActivityCrossModuleAsync(EntityType.Shipment, shipmentId);

            ShipmentMilestoneViewModel currentMilestone = null;

            Func<string, bool> hasEvent = (string code) => acts?.Any(x => x.ActivityCode == code) ?? false;

            if (hasEvent(Event.EVENT_2054))
            {
                currentMilestone = new ShipmentMilestoneViewModel { ActivityCode = Event.EVENT_2054, Title = "Handover to Consignee" };
            }
            else if (hasEvent(Event.EVENT_7002))
            {
                currentMilestone = new ShipmentMilestoneViewModel { ActivityCode = Event.EVENT_7002, Title = "Arrival at Port" };
            }
            else if (hasEvent(Event.EVENT_7001))
            {
                currentMilestone = new ShipmentMilestoneViewModel { ActivityCode = Event.EVENT_7001, Title = "In Transit" };
            }
            else if (hasEvent(Event.EVENT_2014))
            {
                currentMilestone = new ShipmentMilestoneViewModel { ActivityCode = Event.EVENT_2014, Title = "Handover from Shipper" };
            }
            else if (hasEvent(Event.EVENT_2005))
            {
                currentMilestone = new ShipmentMilestoneViewModel { ActivityCode = Event.EVENT_2005, Title = "Shipment Booked" };
            }

            return currentMilestone;
        }
    }
}