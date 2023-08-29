using Groove.SP.Core.Data;
using System.Threading.Tasks;

using Groove.SP.Application.FreightScheduler.Services.Interfaces;
using System.Linq;
using Groove.SP.Core.Entities;

using System;
using Groove.SP.Core.Models;
using Microsoft.Extensions.Options;
using Groove.SP.Application.Common;
using Groove.SP.Application.FreightScheduler.ViewModels;
using Groove.SP.Application.Exceptions;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using System.Linq.Expressions;
using Groove.SP.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Groove.SP.Application.Utilities;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Shipments.ViewModels;
using Newtonsoft.Json;
using Groove.SP.Application.Translations.Providers.Interfaces;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Groove.SP.Application.CruiseOrders.Services
{
    public class FreightSchedulerService : ServiceBase<FreightSchedulerModel, FreightSchedulerViewModel>, IFreightSchedulerService
    {
        private readonly IDataQuery _dataQuery;
        private readonly AppConfig _appConfig;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IActivityService _activityService;
        private readonly IRepository<ItineraryModel> _itineraryRepository;
        private readonly IRepository<ShipmentModel> _shipmentRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly ITranslationProvider _translation;

        public FreightSchedulerService(
            IUnitOfWorkProvider unitOfWorkProvider,
            IDataQuery dataQuery,
            ICSFEApiClient csfeApiClient,
            IRepository<ItineraryModel> itineraryRepository,
            IRepository<ShipmentModel> shipmentRepository,
            IActivityService activityService,
            IOptions<AppConfig> appConfig,
            ITranslationProvider translation) : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
            _appConfig = appConfig.Value;
            _csfeApiClient = csfeApiClient;
            _itineraryRepository = itineraryRepository;
            _shipmentRepository = shipmentRepository;
            _activityService = activityService;
            _activityRepository = (IActivityRepository)UnitOfWork.GetRepository<ActivityModel>();
            _translation = translation;
        }

        public async Task<FreightSchedulerViewModel> CreateAsync(FreightSchedulerViewModel viewModel, string userName)
        {
            viewModel.ValidateAndThrow();

            // Not allow to duplicate freight scheduler
            var isDuplicatedRecord = await IsDuplicatedFreightSchedulerAsync(viewModel);
            if (isDuplicatedRecord == true)
            {
                var message = await _translation.GetTranslationByKeyAsync("msg.scheduleDuplicated");
                throw new AppValidationException(message);
            }

            var freightSchedulerModel = Mapper.Map<FreightSchedulerViewModel, FreightSchedulerModel>(viewModel);
            freightSchedulerModel.Audit(userName);
            await Repository.AddAsync(freightSchedulerModel);
            await UnitOfWork.SaveChangesAsync();

            var result = Mapper.Map<FreightSchedulerViewModel>(freightSchedulerModel);
            return result;
        }

        public async Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, string affiliates = "", long? organizationId = 0)
        {
            IQueryable<FreightSchedulerQueryModel> query;
            string sql;

            sql =
                @"SELECT Id
	                    ,ModeOfTransport
                        ,CarrierName
                        ,CarrierCode
                        ,VesselName
                        ,Voyage
                        ,MAWB
                        ,FlightNumber
	                    ,LocationFromName
	                    ,LocationToName
	                    ,ETDDate
	                    ,ETADate
                        ,ATDDate
	                    ,ATADate
                        ,CYOpenDate
                        ,CYClosingDate
                        ,CASE WHEN IsAllowExternalUpdate = 0 THEN 'No' ELSE '' END AS AllowUpdateFromExternal
                        ,IsAllowExternalUpdate
                        ,t1.HasLinkedItineraries
	                    ,CASE ModeOfTransport WHEN 'Air' THEN STUFF(MAWB, 4, 0, '-')
						                      WHEN 'Sea' THEN IIF(VesselName IS NOT NULL AND VesselName != '', CONCAT(VesselName, '/',  Voyage), '')
	                    END AS VesselMAWB
                    FROM FreightSchedulers fs
                        OUTER APPLY (
		                        SELECT CAST (
						                        CASE
							                        WHEN EXISTS(SELECT * FROM Itineraries it WHERE it.ScheduleId = fs.Id) 
								                        THEN 1
								                        ELSE 0
						                        END AS BIT) AS HasLinkedItineraries
	                        ) AS t1
                    ";

            query = _dataQuery.GetQueryable<FreightSchedulerQueryModel>(sql);
            return await query.ToDataSourceResultAsync(request);
        }

        public async Task<FreightSchedulerViewModel> GetByIdAsync(long id)
        {
            var freightSchedulerModel = await Repository.GetAsNoTrackingAsync(fs => fs.Id == id);
            var result = freightSchedulerModel != null ? Mapper.Map<FreightSchedulerViewModel>(freightSchedulerModel) : null;
            if (result != null)
            {
                result.IsHasLinkedItineraries = await _itineraryRepository.AnyAsync(c => c.ScheduleId == freightSchedulerModel.Id);
            }
            return result;
        }

        public async Task<IEnumerable<FreightSchedulerListViewModel>> FilterAsync(string jsonFilter)
        {
            var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@jsonFilterSet",
                        Value = jsonFilter,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };

            List<FreightSchedulerListViewModel> mappingCallback(DbDataReader reader)
            {
                var freightSchedulerListReturned = new List<FreightSchedulerListViewModel>();
                while (reader.Read())
                {
                    var row = new FreightSchedulerListViewModel
                    {
                        Id = (long)reader[0],
                        VesselName = reader[1] as string,
                        Voyage = reader[2] as string,
                        FlightNumber = reader[3] as string,
                        VesselFlight = reader[4] as string,
                        MAWB = reader[5] as string,
                        CarrierName = reader[6] as string,
                        ETDDate = (DateTime)reader[7],
                        ETADate = (DateTime)reader[8]
                    };
                    freightSchedulerListReturned.Add(row);
                }
                return freightSchedulerListReturned;
            }

            var data = await _dataQuery.GetDataByStoredProcedureAsync("spu_GetScheduleListByFilterSet", mappingCallback, filterParameter.ToArray());

            return data;
        }

        public async Task<FreightSchedulerViewModel> UpdateAsync(long id, UpdateFreightSchedulerViewModel viewModel, string userName)
        {
            viewModel.ValidateAndThrow(true);

            var freightSchedulerModel = await Repository.FindAsync(id);

            // Store current data to write log later
            var jsonCurrentData = JsonConvert.SerializeObject(freightSchedulerModel, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });

            if (freightSchedulerModel == null)
            {
                var message = await _translation.GetTranslationByKeyAsync("msg.objectIdNotFound");
                throw new AppEntityNotFoundException(string.Format(message, id));
                //throw new AppEntityNotFoundException($"Object with the id {id} not found!");
            }

            // Check to allow update ATD
            var isSeaMode = freightSchedulerModel.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase);
            if (isSeaMode && viewModel.ATDDate.HasValue)
            {
                var canUpdateATD = (await IsReadyContainerManifestAsync(id)).Item1;
                if (!canUpdateATD)
                {
                    var message = await _translation.GetTranslationByKeyAsync("msg.scheduleContainerManifestNotReady");
                    throw new AppEntityNotFoundException(message);
                    //throw new ApplicationException($"Container manifest not ready for all shipments");
                }
            }

            var oldETDDate = freightSchedulerModel.ETDDate;
            var oldETADate = freightSchedulerModel.ETADate;

            Mapper.Map(viewModel, freightSchedulerModel);
            freightSchedulerModel.Audit(userName);

            Repository.Update(freightSchedulerModel);
            await UnitOfWork.SaveChangesAsync();

            // Store new data to write log later
            var jsonNewData = JsonConvert.SerializeObject(freightSchedulerModel, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });

            //  Call spu to write log
            //  [dbo].[spu_WriteLogFreightScheduler]
            //  @freightSchedulerId BIGINT,
            //  @jsonCurrentData NVARCHAR(MAX),
            //  @jsonNewData NVARCHAR(MAX),
            //  @updatedBy NVARCHAR(512)	
            var sql = @"[dbo].[spu_WriteLogFreightScheduler] @p0, @p1, @p2, @p3";
            var parameters = new object[]
            {
                viewModel.Id,
                $"[{jsonCurrentData}]",
                $"[{jsonNewData}]",
                userName
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

            await TriggerEvents(viewModel, freightSchedulerModel.ModeOfTransport, userName);

            if (oldETDDate != freightSchedulerModel.ETDDate || oldETADate != freightSchedulerModel.ETADate)
            {
                await BroadcastFreightScheduleUpdatesAsync(new List<long> { id }, string.Empty, true, userName);
            }

            await BroadcastCYClosingDateAsync(id);

            return Mapper.Map<FreightSchedulerViewModel>(freightSchedulerModel);
        }

        /// <summary>
        /// Based on updating ATD/ATA to trigger/update/delete events #7001/#7003, #7002/#7004:
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        private async Task TriggerEvents(UpdateFreightSchedulerViewModel viewModel, string modeOfTransport, string userName)
        {
            var globalId = CommonHelper.GenerateGlobalId(viewModel.Id, EntityType.FreightScheduler);
            var activities = await _activityRepository.Query(s => s.GlobalIdActivities.Any(g => g.GlobalId == globalId), null, c => c.Include(x => x.GlobalIdActivities)).ToListAsync();

            var isAirTransport = modeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase);
            var departureEventCode = isAirTransport ? Event.EVENT_7003 : Event.EVENT_7001;
            var arrivalEventCode = isAirTransport ? Event.EVENT_7004 : Event.EVENT_7002;

            var eventList = await _csfeApiClient.GetEventByCodesAsync(new List<string> {
                departureEventCode,
                arrivalEventCode
            });
            var departureEventModel = eventList.FirstOrDefault(e => e.ActivityCode == departureEventCode);
            var arrivalEventModel = eventList.FirstOrDefault(e => e.ActivityCode == arrivalEventCode);

            var departureActivity = new ActivityViewModel()
            {
                FreightSchedulerId = viewModel.Id,
                ActivityCode = departureEventCode,
                ActivityType = departureEventModel.ActivityType,
                ActivityDescription = departureEventModel.ActivityDescription,
                ActivityDate = viewModel.ATDDate ?? new DateTime(1, 1, 1),
                Location = viewModel.LocationFromName,
                CreatedBy = userName,
                CreatedDate = DateTime.UtcNow
            };
            var arrivalActivity = new ActivityViewModel()
            {
                FreightSchedulerId = viewModel.Id,
                ActivityCode = arrivalEventCode,
                ActivityType = arrivalEventModel.ActivityType,
                ActivityDescription = arrivalEventModel.ActivityDescription,
                ActivityDate = viewModel.ATADate ?? new DateTime(1, 1, 1),
                Location = viewModel.LocationToName,
                CreatedBy = userName,
                CreatedDate = DateTime.UtcNow
            };

            //// Add or update departure event (7001/7003)
            //// Remove arrival event (7002/7004)
            if (viewModel.ATDDate.HasValue && viewModel.ATADate == null)
            {
                var storedArrivalActivity = activities.SingleOrDefault(c => c.ActivityCode == arrivalEventCode);
                if (storedArrivalActivity != null)
                {
                    await _activityService.DeleteAsync(storedArrivalActivity.Id);
                }

                if (activities.Count == 0)
                {
                    await _activityService.CreateAsync(departureActivity);
                }
                else
                {
                    var storedDepartureActivity = activities.Find(c => c.ActivityCode == departureEventCode);
                    // If user change ATD on UI then update activity
                    if (storedDepartureActivity.ActivityDate != viewModel.ATDDate)
                    {
                        departureActivity.Id = storedDepartureActivity.Id;
                        departureActivity.Audit(userName);

                        // to work with Auto mapper
                        departureActivity.FieldStatus = new Dictionary<string, FieldDeserializationStatus>
                        {
                            {
                                nameof(ActivityViewModel.UpdatedBy), FieldDeserializationStatus.HasValue
                            },
                            {
                                nameof(ActivityViewModel.UpdatedDate), FieldDeserializationStatus.HasValue
                            },
                            {
                                nameof(ActivityViewModel.ActivityDate), FieldDeserializationStatus.HasValue
                            }
                        };
                        await _activityService.UpdateAsync(departureActivity, departureActivity.Id);
                    }
                }
            }

            // Add or update event
            if (viewModel.ATDDate.HasValue && viewModel.ATADate.HasValue)
            {
                if (activities.Count == 0)
                {
                    await _activityService.CreateAsync(departureActivity);
                    await _activityService.CreateAsync(arrivalActivity);
                }
                else
                {
                    var storedDepartureActivity = activities.SingleOrDefault(c => c.ActivityCode == departureEventCode);
                    if (storedDepartureActivity == null)
                    {
                        await _activityService.CreateAsync(departureActivity);
                    }
                    else
                    {
                        // If user change ATD on UI then update activity
                        if (storedDepartureActivity.ActivityDate != viewModel.ATDDate)
                        {
                            departureActivity.Id = storedDepartureActivity.Id;
                            departureActivity.Audit(userName);

                            // to work with Auto mapper
                            departureActivity.FieldStatus = new Dictionary<string, FieldDeserializationStatus>
                        {
                            {
                                nameof(ActivityViewModel.UpdatedBy), FieldDeserializationStatus.HasValue
                            },
                            {
                                nameof(ActivityViewModel.UpdatedDate), FieldDeserializationStatus.HasValue
                            },
                            {
                                nameof(ActivityViewModel.ActivityDate), FieldDeserializationStatus.HasValue
                            }
                        };
                            await _activityService.UpdateAsync(departureActivity, departureActivity.Id);
                        }
                    }

                    var storedArrivalActivity = activities.SingleOrDefault(c => c.ActivityCode == arrivalEventCode);
                    if (storedArrivalActivity == null)
                    {
                        await _activityService.CreateAsync(arrivalActivity);
                    }
                    else
                    {
                        // If user change ATA on UI then update activity
                        if (storedArrivalActivity.ActivityDate != viewModel.ATADate)
                        {
                            arrivalActivity.Id = storedArrivalActivity.Id;
                            arrivalActivity.Audit(userName);

                            // to work with Auto mapper
                            arrivalActivity.FieldStatus = new Dictionary<string, FieldDeserializationStatus>
                            {
                                {
                                    nameof(ActivityViewModel.UpdatedBy), FieldDeserializationStatus.HasValue
                                },
                                {
                                    nameof(ActivityViewModel.UpdatedDate), FieldDeserializationStatus.HasValue
                                },
                                {
                                    nameof(ActivityViewModel.ActivityDate), FieldDeserializationStatus.HasValue
                                }
                            };
                            await _activityService.UpdateAsync(arrivalActivity, arrivalActivity.Id);
                        }
                    }
                }
            }

            //// Remove events
            if (viewModel.ATDDate == null && viewModel.ATADate == null)
            {
                var storedDepartureActivity = activities.SingleOrDefault(c => c.ActivityCode == departureEventCode);
                var storedArrivalActivity = activities.SingleOrDefault(c => c.ActivityCode == arrivalEventCode);

                // WARNING: Please do not change the order of event deletion, it will affect the order when calling domain events !

                if (storedArrivalActivity != null)
                {
                    await _activityService.DeleteAsync(storedArrivalActivity.Id);

                }
                if (storedDepartureActivity != null)
                {
                    await _activityService.DeleteAsync(storedDepartureActivity.Id);

                }
            }
        }

        /// <summary>
        /// To update Freight scheduler via API
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<IEnumerable<FreightSchedulerViewModel>> UpdateAsync(UpdateFreightSchedulerApiViewModel viewModel, string userName)
        {
            var schedules = Repository.Query(s
                => s.ModeOfTransport == viewModel.ModeOfTransport && s.CarrierCode == viewModel.CarrierCode && s.LocationToCode == viewModel.LocationToCode);

            var schedulesToUpdate = new List<FreightSchedulerModel>();

            // Only support Sea
            if (viewModel.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase))
            {
                schedulesToUpdate = schedules.Where(s => s.VesselName == viewModel.VesselName && s.Voyage == viewModel.Voyage)?.ToList();
            }

            if (schedulesToUpdate?.Count() > 0)
            {
                var nonlockedSchedulesToUpdate = schedulesToUpdate.Where(s => s.IsAllowExternalUpdate).ToList();
                if (nonlockedSchedulesToUpdate?.Count() > 0)
                {
                    var jsonCurrentDataList = new List<Tuple<long, string>>();
                    foreach (var schedule in nonlockedSchedulesToUpdate)
                    {
                        // Store current data to write log later
                        var jsonCurrentData = JsonConvert.SerializeObject(schedule, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
                        jsonCurrentDataList.Add(Tuple.Create(schedule.Id, jsonCurrentData));

                        schedule.ETADate = viewModel.ETADate;
                        schedule.ATADate = viewModel.ATADate;

                        // Update audit info
                        if (viewModel.IsPropertyDirty(nameof(UpdateFreightSchedulerApiViewModel.CreatedBy)))
                        {
                            schedule.CreatedBy = viewModel.CreatedBy;
                        }
                        if (viewModel.IsPropertyDirty(nameof(UpdateFreightSchedulerApiViewModel.CreatedDate)))
                        {
                            schedule.CreatedDate = viewModel.CreatedDate;
                        }
                        if (viewModel.IsPropertyDirty(nameof(UpdateFreightSchedulerApiViewModel.UpdatedBy)))
                        {
                            schedule.UpdatedBy = viewModel.UpdatedBy;
                        }
                        if (viewModel.IsPropertyDirty(nameof(UpdateFreightSchedulerApiViewModel.UpdatedDate)))
                        {
                            schedule.UpdatedDate = viewModel.UpdatedDate;
                        }
                        else
                        {
                            schedule.UpdatedDate = DateTime.UtcNow;
                        }
                    }

                    Repository.UpdateRange(nonlockedSchedulesToUpdate.ToArray());
                    await UnitOfWork.SaveChangesAsync();

                    // Loop in updated items, then write logs
                    foreach (var item in jsonCurrentDataList)
                    {
                        var newData = nonlockedSchedulesToUpdate.FirstOrDefault(x => x.Id == item.Item1);
                        var jsonNewData = JsonConvert.SerializeObject(newData, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });

                        //  Call spu to write log
                        //  [dbo].[spu_WriteLogFreightScheduler]
                        //  @freightSchedulerId BIGINT,
                        //  @jsonCurrentData NVARCHAR(MAX),
                        //  @jsonNewData NVARCHAR(MAX),
                        //  @updatedBy NVARCHAR(512)    
                        var sql = @"[dbo].[spu_WriteLogFreightScheduler] @p0, @p1, @p2, @p3";
                        var parameters = new object[]
                        {
                            item.Item1,
                            $"[{item.Item2}]",
                            $"[{jsonNewData}]",
                            userName
                        };
                        _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());
                    }

                    await BroadcastFreightScheduleUpdatesAsync(nonlockedSchedulesToUpdate.Select(x => x.Id), Schedulers.UpdatedViaFreightSchedulerAPI, false, userName);
                }
            }
            else
            {
                throw new AppEntityNotFoundException($"No data found!");
            }

            /*** Add/Update EventDate of EventCode = 7002 */
            // Updating ATA for all schedule matched, not need to care state non-locked/locked
            if (viewModel.ATADate.HasValue)
            {
                if (schedulesToUpdate?.Count() > 0)
                {
                    var locationTo = await _csfeApiClient.GetLocationByCodeAsync(viewModel.LocationToCode);
                    var event7002 = await _csfeApiClient.GetEventByCodeAsync(Event.EVENT_7002);

                    // Add/ Update Event_7002 for the schedules
                    foreach (var scheduler in schedulesToUpdate)
                    {
                        var activities = await _activityService.GetActivities(EntityType.FreightScheduler, scheduler.Id);
                        if (activities.Any())
                        {
                            activities = activities.Where(x => x.ActivityCode == Event.EVENT_7002).ToList();
                        }

                        // If there is any 7002 activity
                        if (activities.Any())
                        {
                            // Don't need to update the [ActivityDate] if the [ATADate] the same with current [ActivityDate]
                            activities = activities.Where(x => x.Location == locationTo.LocationDescription && DateTime.Compare(x.ActivityDate, viewModel.ATADate.Value) != 0).ToList();

                            foreach (var updateActivity7002 in activities)
                            {
                                updateActivity7002.ActivityDate = viewModel.ATADate.Value;
                                updateActivity7002.FreightSchedulerId = scheduler.Id;
                                updateActivity7002.Audit(userName);

                                // to work with Auto mapper
                                updateActivity7002.FieldStatus = new Dictionary<string, FieldDeserializationStatus> {
                                    {
                                        nameof(ActivityViewModel.UpdatedBy), FieldDeserializationStatus.HasValue
                                    },
                                    {
                                        nameof(ActivityViewModel.ActivityDate), FieldDeserializationStatus.HasValue
                                    }
                                };
                                await _activityService.UpdateAsync(updateActivity7002, updateActivity7002.Id);
                            }
                        }
                        // Else, create new 7002 activity
                        else
                        {
                            var newActivity7002 = new ActivityViewModel
                            {
                                ActivityCode = Event.EVENT_7002,
                                ActivityType = event7002.ActivityType,
                                ActivityDescription = event7002.ActivityDescription,
                                Location = locationTo.LocationDescription,
                                ActivityDate = viewModel.ATADate.Value,
                                FreightSchedulerId = scheduler.Id,
                                CreatedBy = scheduler.CreatedBy
                            };
                            await _activityService.CreateAsync(newActivity7002);
                        }
                    }
                }
            }
            else
            {
                // Remove Event_7002
                if (viewModel.IsPropertyDirty(nameof(UpdateFreightSchedulerApiViewModel.ATADate)))
                {
                    foreach (var scheduler in schedulesToUpdate)
                    {
                        var activities = await _activityService.GetActivities(EntityType.FreightScheduler, scheduler.Id);
                        if (activities.Any())
                        {
                            activities = activities.Where(x => x.ActivityCode == Event.EVENT_7002).ToList();
                        }
                        foreach (var activity in activities)
                        {
                            await _activityService.DeleteAsync(activity.Id, true);
                        }
                    }
                }
            }

            return Mapper.Map<List<FreightSchedulerViewModel>>(schedulesToUpdate);
        }

        public async Task<FreightSchedulerViewModel> EditAsync(long id, FreightSchedulerViewModel viewModel, string userName)
        {
            viewModel.ValidateAndThrow(true);

            // Not allow to duplicate freight scheduler
            var isDuplicatedRecord = await IsDuplicatedFreightSchedulerAsync(viewModel);
            if (isDuplicatedRecord == true)
            {
                //throw new AppValidationException($"Schedule is duplicated.");
                var message = await _translation.GetTranslationByKeyAsync("msg.scheduleDuplicated");
                throw new AppValidationException(message);
            }

            var freightSchedulerModel = await Repository.FindAsync(id);

            if (freightSchedulerModel == null)
            {
                var message = await _translation.GetTranslationByKeyAsync("msg.objectIdNotFound");
                throw new AppEntityNotFoundException(string.Format(message, id));
                //throw new AppEntityNotFoundException($"Object with the id {id} not found!");

            }

            var hasLinkedItineraries = await _itineraryRepository.AnyAsync(i => i.ScheduleId == freightSchedulerModel.Id);
            if (hasLinkedItineraries)
            {
                var message = await _translation.GetTranslationByKeyAsync("msg.scheduleInUse");
                throw new AppValidationException($"ScheduleIsInUse#{message}");
                //throw new AppValidationException($"ScheduleIsInUse#Schedule is in use, not allow to edit.");
            }

            Mapper.Map(viewModel, freightSchedulerModel);

            freightSchedulerModel.Audit(userName);

            Repository.Update(freightSchedulerModel);

            await UnitOfWork.SaveChangesAsync();

            return Mapper.Map<FreightSchedulerViewModel>(freightSchedulerModel);
        }

        public override async Task<bool> DeleteByKeysAsync(params object[] keys)
        {
            var scheduleId = (long)keys.First();
            var hasLinkedItineraries = await _itineraryRepository.AnyAsync(i => i.ScheduleId == scheduleId);
            if (hasLinkedItineraries)
            {
                throw new AppValidationException($"The scheduler with id = {scheduleId} has been linked to Itineraries.");
            }
            return await base.DeleteByKeysAsync(keys);
        }

        public async Task<bool> IsDuplicatedFreightSchedulerAsync(FreightSchedulerViewModel viewModel)
        {
            Expression<Func<FreightSchedulerModel, bool>> duplicatedFilter = null;

            if (viewModel.ModeOfTransport == ModeOfTransportType.Sea.ToString())
            {
                duplicatedFilter = c =>
                    c.ModeOfTransport == viewModel.ModeOfTransport &&
                    c.CarrierCode == viewModel.CarrierCode &&
                    c.VesselName == viewModel.VesselName &&
                    c.Voyage == viewModel.Voyage &&
                    c.LocationFromName == viewModel.LocationFromName &&
                    c.LocationToName == viewModel.LocationToName &&
                    c.Id != viewModel.Id;
            }

            if (viewModel.ModeOfTransport == ModeOfTransportType.Air.ToString())
            {
                duplicatedFilter = c =>
                   c.ModeOfTransport == viewModel.ModeOfTransport &&
                   c.MAWB == viewModel.MAWB &&
                   c.Id != viewModel.Id;
            }
            if (duplicatedFilter != null)
            {
                return await Repository.AnyAsync(duplicatedFilter);
            }

            return false;
        }

        /// <summary>
        /// To check if all shipments of a Freight Scheduler have Container# linked.
        /// </summary>
        /// <param name="freightSchedulerId"></param>
        /// <returns></returns>
        public async Task<Tuple<bool, List<long>>> IsReadyContainerManifestAsync(long freightSchedulerId)
        {
            var shipments = await _shipmentRepository.QueryAsNoTracking(x => x.ConsignmentItineraries.Any(x => x.Itinerary.ScheduleId == freightSchedulerId) && x.Status == StatusType.ACTIVE,
                                                            includes: x => x.Include(y => y.ShipmentLoads)).ToListAsync();

            var notReadyShipmentIds = new List<long>();

            if (shipments == null || !shipments.Any())
            {
                return new(true, notReadyShipmentIds);
            }

            foreach (var item in shipments)
            {
                var hasContainer = item.ShipmentLoads.Any(x => x.ContainerId != null);

                if (!hasContainer)
                {
                    notReadyShipmentIds.Add(item.Id);
                }
            }
            return new(!notReadyShipmentIds.Any(), notReadyShipmentIds);
        }

        /// <summary>
        /// Call to broadcast Schedule updates value (ETA/ETD) to related tabels (dbo.Shipments/ dbo.Consignments/ dbo.Itineraries/ dbo.Containers/ dbo.BillOfLadings)
        /// </summary>
        /// <param name="freightSchedulerIds"></param>
        /// <param name="updatedFromKeyword"></param>
        /// <param name="updateViaUI"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<bool> BroadcastFreightScheduleUpdatesAsync(IEnumerable<long> freightSchedulerIds, string updatedFromKeyword, bool updateViaUI, string userName)
        {
            //  [dbo].[spu_BroadcastFreightScheduleUpdates]
            //  @freightSchedulerIds NVARCHAR(512),
            //  @updatedBy NVARCHAR(512),
            //  @updatedFromKeyword NVARCHAR(512),
            //  @updatedViaUI BIT

            var sql = @"spu_BroadcastFreightScheduleUpdates 
                        @p0,
	                    @p1,
	                   	@p2,
                        @p3";
            var parameters = new object[]
            {
                string.Join(",", freightSchedulerIds),
                userName,
                updatedFromKeyword,
                updateViaUI
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

            return true;
        }

        public async Task<int> CountVesselArrivalAsync(bool isInternal, string affiliates, long? delegatedOrgId, string customerRelationships, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var storedProcedureName = "";
            if (isInternal)
            {
                storedProcedureName = "spu_VesselArrival_Statistics_Internal";
                var sql = $@"
                        SET @result = 0;
                        EXEC @result = {storedProcedureName} @FromDate, @ToDate

                        ";

                var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@FromDate",
                        Value = dates["FromDate"],
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@ToDate",
                        Value = dates["ToDate"],
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };
                var sqlResult = _dataQuery.GetValueFromVariable(sql, filterParameter.ToArray());
                if (int.TryParse(sqlResult, out var result))
                {
                    return result;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                affiliates = affiliates.Replace("[", "").Replace("]", "");
                storedProcedureName = "spu_VesselArrival_Statistics_External";
                var sql = $@"
                        SET @result = 0;
                        EXEC @result = {storedProcedureName} @Affiliates, @DelegatedOrganizationId, @CustomerRelationships, @FromDate, @ToDate

                        ";
                var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@Affiliates",
                        Value = affiliates,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@DelegatedOrganizationId",
                        Value = delegatedOrgId ?? 0,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@CustomerRelationships",
                        Value = customerRelationships ?? "",
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@FromDate",
                        Value = dates["FromDate"],
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@ToDate",
                        Value = dates["ToDate"],
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };
                var sqlResult = _dataQuery.GetValueFromVariable(sql, filterParameter.ToArray());
                if (int.TryParse(sqlResult, out var result))
                {
                    return result;
                }
                else
                {
                    return 0;
                }
            }
        }

        public async Task<DataSourceResult> GetListVesselArrivalAsync(DataSourceRequest request, bool isInternal, string affiliates, long? delegatedOrgId, string customerRelationships, string statisticKey = "", string statisticFilter = "", bool isExport = false)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);

            IQueryable<VesselArrivalQueryModel> query;
            string sql = "";

            if (isInternal)
            {
                if (!string.IsNullOrEmpty(statisticKey))
                {
                    switch (statisticKey)
                    {
                        case "vesselArrival":
                            sql = $@"
                                     SELECT 
		                                PO.Id, 
		                                PO.PONumber, 
		                                FS.CarrierName,
		                                FS.VesselName + '/' + FS.Voyage AS VesselVoyage,
		                                FS.LocationFromName AS LoadingPort,
		                                FS.ETDDate,
		                                fs.LocationToName AS DischargePort,
		                                FS.ETADate
	                                FROM PurchaseOrders PO
	                                INNER JOIN POFulfillmentOrders POFO ON PO.Id = POFO.PurchaseOrderId
	                                INNER JOIN POFulfillments POF ON POF.Id = POFO.POFulfillmentId
	                                INNER JOIN Shipments S ON S.POFulfillmentId = POF.Id 
	                                INNER JOIN ConsignmentItineraries CI ON CI.ShipmentId = S.Id
	                                INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
	                                INNER JOIN FreightSchedulers FS ON I.ScheduleId = FS.Id AND FS.ModeOfTransport = 'Sea'
	                                AND  FS.ETADate >= '{dates["FromDate"]}' AND FS.ETADate <= '{dates["ToDate"]}'
	                                GROUP BY 
		                                PO.Id, 
		                                PO.PONumber, 
		                                FS.CarrierName,
		                                FS.VesselName ,
		                                FS.Voyage,
		                                FS.LocationFromName,
		                                FS.ETDDate,
		                                fs.LocationToName,
		                                FS.ETADate

	                                UNION ALL

	                                SELECT 
		                                PO.Id, 
		                                PO.PONumber, 
		                                FS.CarrierName,
		                                FS.VesselName + '/' + FS.Voyage AS VesselVoyage,
		                                FS.LocationFromName AS LoadingPort,
		                                FS.ETDDate,
		                                fs.LocationToName AS DischargePort,
		                                FS.ETADate
	                                FROM PurchaseOrders PO
	                                INNER JOIN CargoDetails C ON C.OrderId = PO.Id
	                                INNER JOIN Shipments S ON S.Id = C.ShipmentId AND S.POFulfillmentId IS NULL
	                                INNER JOIN ConsignmentItineraries CI ON CI.ShipmentId = S.Id
	                                INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
	                                INNER JOIN FreightSchedulers FS ON I.ScheduleId = FS.Id AND FS.ModeOfTransport = 'Sea'
		                                AND  FS.ETADate >= '{dates["FromDate"]}' AND FS.ETADate <= '{dates["ToDate"]}'
                                    GROUP BY 
		                                PO.Id, 
		                                PO.PONumber, 
		                                FS.CarrierName,
		                                FS.VesselName ,
		                                FS.Voyage,
		                                FS.LocationFromName,
		                                FS.ETDDate,
		                                fs.LocationToName,
		                                FS.ETADate
                              ";
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    affiliates = affiliates.Replace("[", string.Empty).Replace("]", string.Empty);
                }

                if (!string.IsNullOrEmpty(statisticKey))
                {
                    // user role = agent/ principal
                    if (string.IsNullOrEmpty(customerRelationships))
                    {
                        switch (statisticKey)
                        {
                            case "vesselArrival":
                                sql = $@"
                                     SELECT 
				                        PO.Id, 
				                        PO.PONumber, 
				                        FS.CarrierName,
				                        FS.VesselName + '/' + FS.Voyage AS VesselVoyage,
				                        FS.LocationFromName AS LoadingPort,
				                        FS.ETDDate,
				                        fs.LocationToName AS DischargePort,
				                        FS.ETADate
			                        FROM PurchaseOrders PO
			                        INNER JOIN POFulfillmentOrders POFO ON PO.Id = POFO.PurchaseOrderId
			                        INNER JOIN POFulfillments POF ON POF.Id = POFO.POFulfillmentId
			                        INNER JOIN Shipments S ON S.POFulfillmentId = POF.Id
			                        INNER JOIN ConsignmentItineraries CI ON CI.ShipmentId = S.Id
			                        INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
			                        INNER JOIN FreightSchedulers FS ON I.ScheduleId = FS.Id AND FS.ModeOfTransport = 'Sea'
			                        AND  FS.ETADate >= '{dates["FromDate"]}' AND FS.ETADate <= '{dates["ToDate"]}'
			                        WHERE EXISTS 
			                        (
				                        SELECT 1
				                        FROM PurchaseOrderContacts POC 
				                        WHERE POC.PurchaseOrderId = PO.Id AND POC.OrganizationId IN ({affiliates})
			                        )
			                        GROUP BY 
				                        PO.Id, 
				                        PO.PONumber, 
				                        FS.CarrierName,
				                        FS.VesselName ,
				                        FS.Voyage,
				                        FS.LocationFromName,
				                        FS.ETDDate,
				                        fs.LocationToName,
				                        FS.ETADate

			                        UNION ALL

			                        SELECT 
				                        PO.Id, 
				                        PO.PONumber, 
				                        FS.CarrierName,
				                        FS.VesselName + '/' + FS.Voyage AS VesselVoyage,
				                        FS.LocationFromName AS LoadingPort,
				                        FS.ETDDate,
				                        fs.LocationToName AS DischargePort,
				                        FS.ETADate
			                        FROM PurchaseOrders PO
			                        INNER JOIN CargoDetails C ON C.OrderId = PO.Id
			                        INNER JOIN Shipments S ON S.Id = C.ShipmentId AND S.POFulfillmentId IS NULL
			                        INNER JOIN ConsignmentItineraries CI ON CI.ShipmentId = S.Id
			                        INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
			                        INNER JOIN FreightSchedulers FS ON I.ScheduleId = FS.Id AND FS.ModeOfTransport = 'Sea'
			                        AND  FS.ETADate >= '{dates["FromDate"]}' AND FS.ETADate <= '{dates["ToDate"]}'
			                        WHERE EXISTS 
			                        (
				                        SELECT 1
				                        FROM PurchaseOrderContacts POC 
				                        WHERE POC.PurchaseOrderId = PO.Id AND POC.OrganizationId IN ({affiliates})
			                        )
			                        GROUP BY 
				                        PO.Id, 
				                        PO.PONumber, 
				                        FS.CarrierName,
				                        FS.VesselName ,
				                        FS.Voyage,
				                        FS.LocationFromName,
				                        FS.ETDDate,
				                        fs.LocationToName,
				                        FS.ETADate
                              ";
                                break;
                            default:
                                break;
                        }
                    }
                    // user role = shipper
                    else
                    {
                        switch (statisticKey)
                        {
                            case "vesselArrival":
                                sql = $@"
                                   SELECT
                                        PO.Id, 
				                        PO.PONumber, 
				                        FS.CarrierName,
				                        FS.VesselName + '/' + FS.Voyage AS VesselVoyage,
				                        FS.LocationFromName AS LoadingPort,
				                        FS.ETDDate,
				                        fs.LocationToName AS DischargePort,
				                        FS.ETADate
			                        FROM PurchaseOrders PO
			                        INNER JOIN POFulfillmentOrders POFO ON PO.Id = POFO.PurchaseOrderId
			                        INNER JOIN POFulfillments POF ON POF.Id = POFO.POFulfillmentId
			                        INNER JOIN Shipments S ON S.POFulfillmentId = POF.Id
			                        INNER JOIN ConsignmentItineraries CI  ON CI.ShipmentId = S.Id
			                        INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
			                        INNER JOIN FreightSchedulers FS ON I.ScheduleId = FS.Id AND FS.ModeOfTransport = 'Sea'
			                        AND  FS.ETADate >= '{dates["FromDate"]}' AND FS.ETADate <= '{dates["ToDate"]}'
			                        WHERE EXISTS (
				                        SELECT 1
				                        FROM PurchaseOrderContacts POC 
				                        WHERE POC.PurchaseOrderId = PO.Id 
                                        AND POC.OrganizationId = {delegatedOrgId} AND POC.OrganizationRole = 'Delegation'
				                        )

			                        GROUP BY 
				                        PO.Id, 
				                        PO.PONumber, 
				                        FS.CarrierName,
				                        FS.VesselName ,
				                        FS.Voyage,
				                        FS.LocationFromName,
				                        FS.ETDDate,
				                        fs.LocationToName,
				                        FS.ETADate
                                    
			                        UNION ALL

                                    SELECT
                                        PO.Id, 
				                        PO.PONumber, 
				                        FS.CarrierName,
				                        FS.VesselName + '/' + FS.Voyage AS VesselVoyage,
				                        FS.LocationFromName AS LoadingPort,
				                        FS.ETDDate,
				                        fs.LocationToName AS DischargePort,
				                        FS.ETADate
			                        FROM PurchaseOrders PO
			                        INNER JOIN POFulfillmentOrders POFO ON PO.Id = POFO.PurchaseOrderId
			                        INNER JOIN POFulfillments POF ON POF.Id = POFO.POFulfillmentId
			                        INNER JOIN Shipments S ON S.POFulfillmentId = POF.Id
			                        INNER JOIN ConsignmentItineraries CI  ON CI.ShipmentId = S.Id
			                        INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
			                        INNER JOIN FreightSchedulers FS ON I.ScheduleId = FS.Id AND FS.ModeOfTransport = 'Sea'
			                        AND  FS.ETADate >= '{dates["FromDate"]}' AND FS.ETADate <= '{dates["ToDate"]}'
			                        CROSS APPLY
			                        (
				                        SELECT sc.OrganizationId AS SupplierId
				                        FROM PurchaseOrderContacts sc
				                        WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
			                        ) POC
			                        CROSS APPLY
			                        (
				                         SELECT sc.OrganizationId AS CustomerId
				                         FROM PurchaseOrderContacts sc
				                         WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Principal'
			                        ) POC1
			                        WHERE
				                        CAST(POC.SupplierID AS NVARCHAR(20)) + ','+ CAST(POC1.CustomerId AS NVARCHAR(20)) IN (SELECT tmp.[Value] FROM dbo.fn_SplitStringToTable('{customerRelationships}', ';') tmp )
		
			                        GROUP BY 
				                        PO.Id, 
				                        PO.PONumber, 
				                        FS.CarrierName,
				                        FS.VesselName ,
				                        FS.Voyage,
				                        FS.LocationFromName,
				                        FS.ETDDate,
				                        fs.LocationToName,
				                        FS.ETADate

                                    UNION ALL 

			                        SELECT 
				                        PO.Id, 
				                        PO.PONumber, 
				                        FS.CarrierName,
				                        FS.VesselName + '/' + FS.Voyage AS VesselVoyage,
				                        FS.LocationFromName AS LoadingPort,
				                        FS.ETDDate,
				                        fs.LocationToName AS DischargePort,
				                        FS.ETADate
			                        FROM PurchaseOrders PO
			                        INNER JOIN CargoDetails C ON C.OrderId = PO.Id
			                        INNER JOIN Shipments S ON S.Id = C.ShipmentId AND S.POFulfillmentId IS NULL
			                        INNER JOIN ConsignmentItineraries CI ON CI.ShipmentId = S.Id
			                        INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
			                        INNER JOIN FreightSchedulers FS ON I.ScheduleId = FS.Id AND FS.ModeOfTransport = 'Sea'
			                        AND  FS.ETADate >= '{dates["FromDate"]}' AND FS.ETADate <= '{dates["ToDate"]}'
			                        WHERE EXISTS (
				                        SELECT 1
				                        FROM PurchaseOrderContacts POC 
				                        WHERE POC.PurchaseOrderId = PO.Id 
                                        AND POC.OrganizationId = {delegatedOrgId} AND POC.OrganizationRole = 'Delegation'
				                        )

			                        GROUP BY 
				                        PO.Id, 
				                        PO.PONumber, 
				                        FS.CarrierName,
				                        FS.VesselName ,
				                        FS.Voyage,
				                        FS.LocationFromName,
				                        FS.ETDDate,
				                        fs.LocationToName,
				                        FS.ETADate

                                    UNION ALL 

                                    SELECT 
				                        PO.Id, 
				                        PO.PONumber, 
				                        FS.CarrierName,
				                        FS.VesselName + '/' + FS.Voyage AS VesselVoyage,
				                        FS.LocationFromName AS LoadingPort,
				                        FS.ETDDate,
				                        fs.LocationToName AS DischargePort,
				                        FS.ETADate
			                        FROM PurchaseOrders PO
			                        INNER JOIN CargoDetails C ON C.OrderId = PO.Id
			                        INNER JOIN Shipments S ON S.Id = C.ShipmentId AND S.POFulfillmentId IS NULL
			                        INNER JOIN ConsignmentItineraries CI ON CI.ShipmentId = S.Id
			                        INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
			                        INNER JOIN FreightSchedulers FS ON I.ScheduleId = FS.Id AND FS.ModeOfTransport = 'Sea'
			                        AND  FS.ETADate >= '{dates["FromDate"]}' AND FS.ETADate <= '{dates["ToDate"]}'
			                        CROSS APPLY
			                        (
				                        SELECT sc.OrganizationId AS SupplierId
				                        FROM PurchaseOrderContacts sc
				                        WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
			                        ) POC
			                        CROSS APPLY
			                        (
				                         SELECT sc.OrganizationId AS CustomerId
				                         FROM PurchaseOrderContacts sc
				                         WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Principal'
			                        ) POC1
			                        WHERE 
				                        CAST(POC.SupplierID AS NVARCHAR(20)) + ','+ CAST(POC1.CustomerId AS NVARCHAR(20)) IN (SELECT tmp.[Value] FROM dbo.fn_SplitStringToTable('{customerRelationships}', ';') tmp )
			                        GROUP BY 
				                        PO.Id, 
				                        PO.PONumber, 
				                        FS.CarrierName,
				                        FS.VesselName ,
				                        FS.Voyage,
				                        FS.LocationFromName,
				                        FS.ETDDate,
				                        fs.LocationToName,
				                        FS.ETADate
                              ";
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            query = _dataQuery.GetQueryable<VesselArrivalQueryModel>(sql);
            return await query.ToDataSourceResultAsync(request);
        }

        public async Task<bool> BroadcastCYClosingDateAsync(long freightSchedulerId)
        {
            var sql = @"UPDATE Itineraries
                        SET CYClosingDate = T.CYClosingDate
                        FROM Itineraries ITI
                        CROSS APPLY (
	                        SELECT TOP(1) FS.CYClosingDate
	                        FROM FreightSchedulers FS
	                        WHERE FS.Id = @p0
	                        AND ITI.ScheduleId = FS.Id
                        ) T

                        UPDATE POFulfillments
                        SET CYClosingDate = T.CYClosingDate
                        FROM POFulfillments POFF
                        CROSS APPLY (
	                        SELECT TOP(1) FS.Id, FS.CYClosingDate
	                        FROM FreightSchedulers FS
	                        INNER JOIN Itineraries ITI ON FS.Id = ITI.ScheduleId
	                        INNER JOIN ConsignmentItineraries CI ON ITI.Id = CI.ItineraryId
	                        INNER JOIN Shipments SHI ON SHI.Id = CI.ShipmentId AND SHI.[Status] = 'Active'
	                        WHERE SHI.POFulfillmentId = POFF.Id
                            ORDER BY ITI.Sequence ASC

                        ) T
                        WHERE T.Id = @p0";

            var parameters = new object[]
            {
               freightSchedulerId
            };

            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

            return true;
        }
    }
}
