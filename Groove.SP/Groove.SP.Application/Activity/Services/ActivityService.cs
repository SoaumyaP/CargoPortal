using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Events;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Event = Groove.SP.Infrastructure.CSFE.Models.Event;

namespace Groove.SP.Application.Activity.Services
{
    public class ActivityService : ServiceBase<ActivityModel, ActivityViewModel>, IActivityService
    {
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IDataQuery _dataQuery;
        private readonly IRepository<ShipmentModel> _shipmentRepository;
        private readonly IRepository<PurchaseOrderModel> _purchaseOrderRepository;
        private readonly IRepository<ContainerModel> _containerModelRepository;
        public ActivityService(IUnitOfWorkProvider unitOfWorkProvider,
            ICSFEApiClient csfeApiClient,
            IDataQuery dataQuery,
            IRepository<ShipmentModel> shipmentRepository,
            IRepository<PurchaseOrderModel> purchaseOrderRepository,
            IRepository<ContainerModel> containerModelRepository)
            : base(unitOfWorkProvider)
        {
            _csfeApiClient = csfeApiClient;
            _dataQuery = dataQuery;
            _shipmentRepository = shipmentRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _containerModelRepository = containerModelRepository;
        }

        public async Task<IEnumerable<ActivityViewModel>> GetActivities(string entityType, long entityId)
        {
            // to get globalId
            var globalId = CommonHelper.GenerateGlobalId(entityId, entityType);

            // to get data: Activities are sorted by Activity Date, Activity Code DESC
            var result = await Repository.QueryAsNoTracking(s => s.GlobalIdActivities.Any(g => g.GlobalId == globalId),
                                                    s => s.OrderByDescending(c => c.ActivityDate).ThenByDescending(c => c.ActivityCode))
                                        .ToListAsync();
            // sorting by date (ignore time part) on memory
            result = result?.OrderByDescending(x => x.ActivityDate.Date).ThenByDescending(x => x.ActivityCode).ToList();

            return Mapper.Map<IEnumerable<ActivityViewModel>>(result);
        }

        public async Task<IEnumerable<ActivityViewModel>> GetActivityCrossModuleAsync(string entityType, long entityId)
        {
            var storedProcedureName = "spu_GetActivityCrossModule";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@entityType",
                        Value = entityType,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@entityId",
                        Value = entityId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, IEnumerable<ActivityViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<ActivityViewModel>();

                // Map data for activity information
                while (reader.Read())
                {
                    var newRow = new ActivityViewModel();
                    object tmpValue;

                    // Must be in order of data reader
                    newRow.Id = (long)reader[0];

                    tmpValue = reader[1];
                    newRow.ShipmentId = DBNull.Value == tmpValue ? (long?)null : (long)tmpValue;

                    tmpValue = reader[2];
                    newRow.ContainerId = DBNull.Value == tmpValue ? (long?)null : (long)tmpValue;

                    tmpValue = reader[3];
                    newRow.PurchaseOrderId = DBNull.Value == tmpValue ? (long?)null : (long)tmpValue;

                    tmpValue = reader[4];
                    newRow.POFulfillmentId = DBNull.Value == tmpValue ? (long?)null : (long)tmpValue;

                    tmpValue = reader[5];
                    newRow.FreightSchedulerId = DBNull.Value == tmpValue ? (long?)null : (long)tmpValue;

                    tmpValue = reader[6];
                    newRow.ActivityCode = tmpValue.ToString();

                    tmpValue = reader[7];
                    newRow.ActivityType = tmpValue.ToString();

                    tmpValue = reader[8];
                    newRow.ActivityLevel = tmpValue.ToString();

                    tmpValue = reader[9];
                    newRow.ActivityDate = (DateTime)tmpValue;

                    tmpValue = reader[10];
                    newRow.ActivityDescription = tmpValue.ToString();

                    tmpValue = reader[11];
                    newRow.Location = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[12];
                    newRow.Remark = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[13];
                    newRow.Resolved = DBNull.Value == tmpValue ? (bool?)null : (bool)tmpValue;

                    tmpValue = reader[14];
                    newRow.Resolution = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[15];
                    newRow.ResolutionDate = DBNull.Value == tmpValue ? (DateTime?)null : (DateTime)tmpValue;

                    tmpValue = reader[16];
                    newRow.CreatedBy = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[17];
                    newRow.CreatedDate = (DateTime)tmpValue;

                    tmpValue = reader[18];
                    newRow.SortSequence = (long)tmpValue;

                    mappedData.Add(newRow);
                }

                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, parameters.ToArray());
            result = result?.OrderByDescending(x => x.ActivityDate.Date).ThenByDescending(x => x.SortSequence);
            return result;
        }

        public async Task<ActivityViewModel> GetActivitiesByCodeAsync(string code)
        {
            var result = await Repository.GetAsync(s => s.ActivityCode == code);
            return Mapper.Map<ActivityViewModel>(result);
        }

        public override async Task<ActivityViewModel> CreateAsync(ActivityViewModel viewModel)
        {
            viewModel.ValidateAndThrow();

            var eventModel = await _csfeApiClient.GetEventByCodeAsync(viewModel.ActivityCode);

            bool isLocationRequired = eventModel?.LocationRequired ?? false;
            if (isLocationRequired && string.IsNullOrWhiteSpace(viewModel.Location))
            {
                throw new ApplicationException($"Location must not be empty for the event #{viewModel.ActivityCode}");
            }

            bool isRemarkRequired = eventModel?.RemarkRequired ?? false;
            if (isRemarkRequired && string.IsNullOrWhiteSpace(viewModel.Remark))
            {
                throw new ApplicationException($"Remark must not be empty for the event #{viewModel.ActivityCode}");
            }

            var activity = new ActivityModel(
                code: viewModel.ActivityCode,
                type: viewModel.ActivityType,
                description: viewModel.ActivityDescription,                
                location: viewModel.Location,
                activityDate: viewModel.ActivityDate,
                createdBy: viewModel.CreatedBy,
                remark: viewModel.Remark,
                resolution: viewModel.Resolution,
                resolutionDate: viewModel.ResolutionDate,
                resolved: viewModel.Resolved,
                shipmentId: viewModel.ShipmentId,
                containerId: viewModel.ContainerId,
                consignmentId: viewModel.ConsignmentId,
                purchaseOrderId: viewModel.PurchaseOrderId,
                poFulfillmentId: viewModel.POFulfillmentId,
                cruiseOrderId: viewModel.CruiseOrderId,
                freightSchedulerId: viewModel.FreightSchedulerId,
                metaData: viewModel.MetaData
                );

            await Repository.AddAsync(activity);
            await UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<ActivityViewModel>(activity);
            return viewModel;
        }

        public async Task<ActivityViewModel> TriggerAnEvent(ActivityViewModel viewModel)
        {
            var eventModel = await _csfeApiClient.GetEventByCodeAsync(viewModel.ActivityCode);
            if (eventModel == null)
            {
                throw new AppEntityNotFoundException($"Object with the code {string.Join(", ", viewModel.ActivityCode)} not found!");
            }
            var model = this.CombineMasterEvent(eventModel, viewModel);
            model.GlobalIdActivities.Add(GetGlobalIdActivities(model, viewModel).First());
            await this.Repository.AddAsync(model);
            await this.UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<ActivityViewModel>(model);
            return viewModel;
        }

        public async Task TriggerEventList(IList<ActivityViewModel> events)
        {
            if (events.Count > 0)
            {
                var eventModels = await _csfeApiClient.GetEventByCodesAsync(events.Select(x => x.ActivityCode));
                foreach (var item in events)
                {
                    var eventModel = eventModels.SingleOrDefault(x => x.ActivityCode == item.ActivityCode);
                    if (eventModel == null)
                    {
                        throw new AppEntityNotFoundException($"Object with the code {string.Join(", ", item.ActivityCode)} not found!");
                    }

                    var model = CombineMasterEvent(eventModel, item);
                    model.GlobalIdActivities.Add(GetGlobalIdActivities(model, item).First());
                    await Repository.AddAsync(model);
                }
                await UnitOfWork.SaveChangesAsync();
            }
        }

        protected override Func<IQueryable<ActivityModel>, IQueryable<ActivityModel>> FullIncludeProperties
        {
            get
            {
                return x => x.Include(m => m.GlobalIdActivities);
            }
        }

        /// <summary>
        /// Create activity from third party API (Agent)
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public async Task<ActivityViewModel> CreateAsync(AgentActivityCreateViewModel viewModel)
        {
            long? shipmentId = null;
            long? purchaseOrderId = null;
            long? containerId = null;
            if (!string.IsNullOrWhiteSpace(viewModel.ShipmentNo))
            {
                var customerOrg = await _csfeApiClient.GetOrganizationsByCodeAsync(viewModel.CustomerCode);
                var shipment = await _shipmentRepository.GetAsNoTrackingAsync(s 
                    => s.ShipmentNo == viewModel.ShipmentNo && s.Contacts.Any(c => c.OrganizationRole == OrganizationRole.Principal && c.OrganizationId == customerOrg.Id));
                if (shipment == null)
                {
                    throw new AppEntityNotFoundException($"Object with number #{viewModel.ShipmentNo} not found!");
                }
                shipmentId = shipment.Id;
            }
            else if (!string.IsNullOrWhiteSpace(viewModel.PurchaseOrderNo))
            {
                var customerOrg = await _csfeApiClient.GetOrganizationsByCodeAsync(viewModel.CustomerCode);

                var purchaseOrder = await _purchaseOrderRepository.GetAsNoTrackingAsync(po
                    => po.PONumber == viewModel.PurchaseOrderNo && po.Contacts.Any(c => c.OrganizationRole == OrganizationRole.Principal && c.OrganizationId == customerOrg.Id));
                if (purchaseOrder == null)
                {
                    throw new AppEntityNotFoundException($"Object with number #{viewModel.PurchaseOrderNo} not found!");
                }
                purchaseOrderId = purchaseOrder.Id;
            }
            else if (!string.IsNullOrWhiteSpace(viewModel.ContainerNo))
            {
                var container = await _containerModelRepository.QueryAsNoTracking(c => c.ContainerNo == viewModel.ContainerNo).OrderByDescending(c => c.Id).FirstOrDefaultAsync();
                if (container == null)
                {
                    throw new AppEntityNotFoundException($"Object with number #{viewModel.ContainerNo} not found!");
                }
                containerId = container.Id;
            }
            else
            {
                throw new ApplicationException("Either ShipmentNo, PurchaseOrderNo, or ContainerNo is required.");
            }

            var eventModel = await _csfeApiClient.GetEventByCodeAsync(viewModel.ActivityCode);
            if (!string.IsNullOrWhiteSpace(viewModel.Location))
            {
                var locationModel = await _csfeApiClient.GetLocationByCodeAsync(viewModel.Location);
                viewModel.Location = locationModel?.LocationDescription ?? viewModel.Location;
            }

            var activity = new ActivityModel(
                code: viewModel.ActivityCode,
                type: eventModel.ActivityType,
                description: eventModel.ActivityDescription,
                location: viewModel.Location,
                activityDate: viewModel.ActivityDate,
                createdBy: viewModel.CreatedBy,
                remark: viewModel.Remark,
                resolution: viewModel.Resolution,
                resolutionDate: viewModel.ResolutionDate,
                resolved: viewModel.Resolved,
                shipmentId: shipmentId,
                containerId: containerId,
                purchaseOrderId: purchaseOrderId);

            activity.CreatedDate = viewModel.CreatedDate;
            activity.UpdatedDate = viewModel.UpdatedDate;
            activity.UpdatedBy = viewModel.UpdatedBy;

            await Repository.AddAsync(activity);
            await UnitOfWork.SaveChangesAsync();

            var result = Mapper.Map<ActivityViewModel>(activity);
            return result;
        }

        /// <summary>
        /// Update activity from third party API (Agent)
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public async Task<ActivityViewModel> UpdateAsync(AgentActivityUpdateViewModel viewModel)
        {
            var model = await FirstActivityAsync(viewModel.CustomerCode, viewModel.ActivityCode, viewModel.PurchaseOrderNo, viewModel.ShipmentNo, viewModel.ContainerNo);

            var previousActivityCode = model.ActivityCode;

            if (!string.IsNullOrWhiteSpace(viewModel.Location))
            {
                var locationModel = await _csfeApiClient.GetLocationByCodeAsync(viewModel.Location);
                viewModel.Location = locationModel?.LocationDescription ?? viewModel.Location;
            }

            Mapper.Map(viewModel, model);

            var globalIdActivityModel = model.GlobalIdActivities.First();
            globalIdActivityModel.ActivityDate = model.ActivityDate;
            globalIdActivityModel.Location = model.Location;
            globalIdActivityModel.Remark = model.Remark;
            globalIdActivityModel.CreatedDate = model.CreatedDate;
            globalIdActivityModel.CreatedBy = model.CreatedBy;
            globalIdActivityModel.UpdatedDate = model.UpdatedDate;
            globalIdActivityModel.UpdatedBy = model.UpdatedBy;

            var globalIdParts = CommonHelper.GetGlobalIdParts(globalIdActivityModel.GlobalId);
            long? containerId = null;
            long? shipmentId = null;
            if (!string.IsNullOrEmpty(viewModel.ContainerNo))
            {
                containerId = globalIdParts.EntityId;
            }
            else if (globalIdParts.Type == EntityType.Shipment)
            {
                shipmentId = globalIdParts.EntityId;
            }
            model.AddDomainEvent(
               new ActivityChangedDomainEvent(
                   previousActivityCode,
                   currentActivityCode: model.ActivityCode,
                   currentActivityDate: model.ActivityDate,
                   currentContainerId: containerId,
                   currentShipmentId: shipmentId,
                   currentLocation: model.Location));

            Repository.Update(model);
            await UnitOfWork.SaveChangesAsync();

            var result = Mapper.Map<ActivityViewModel>(model);
            return result;
        }

        /// <summary>
        /// Delete activity from third party API (Agent)
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public async Task DeleteAsync(AgentActivityDeleteViewModel viewModel)
        {
            var model = await FirstActivityAsync(viewModel.CustomerCode, viewModel.ActivityCode, viewModel.PurchaseOrderNo, viewModel.ShipmentNo, viewModel.ContainerNo);
            model.AddDomainEvent(new ActivityDeletedDomainEvent(model));
            Repository.Remove(model);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Get first activity by CustomerCode (Organization code), ActivityCode and Entity number (purchaseOrderNo/ shipmentNo/ containerNo)
        /// <br>NOTES: Either ShipmentNo, PurchaseOrderNo, or ContainerNo is allowed.</br>
        /// </summary>
        /// <param name="customerCode"></param>
        /// <param name="activityCode"></param>
        /// <param name="purchaseOrderNo"></param>
        /// <param name="shipmentNo"></param>
        /// <param name="containerNo"></param>
        /// <returns></returns>
        private async Task<ActivityModel> FirstActivityAsync(string customerCode, string activityCode, string purchaseOrderNo, string shipmentNo, string containerNo)
        {
            string globalId = "";
            if (!string.IsNullOrEmpty(purchaseOrderNo))
            {
                var customerOrg = await _csfeApiClient.GetOrganizationsByCodeAsync(customerCode);
                var purchaseOrder = await _purchaseOrderRepository.GetAsNoTrackingAsync(po
                    => po.PONumber == purchaseOrderNo && po.Contacts.Any(c => c.OrganizationRole == OrganizationRole.Principal && c.OrganizationId == customerOrg.Id));

                if (purchaseOrder != null)
                {
                    globalId = EntityType.CustomerPO + "_" + purchaseOrder.Id;
                }
            }
            else if (!string.IsNullOrEmpty(shipmentNo))
            {
                var customerOrg = await _csfeApiClient.GetOrganizationsByCodeAsync(customerCode);
                var shipment = await _shipmentRepository.GetAsNoTrackingAsync(s
                    => s.ShipmentNo == shipmentNo && s.Contacts.Any(c => c.OrganizationRole == OrganizationRole.Principal && c.OrganizationId == customerOrg.Id));
                if (shipment != null)
                {
                    globalId = EntityType.Shipment + "_" + shipment.Id;
                }
            }
            else if (!string.IsNullOrEmpty(containerNo))
            {
                var container = await _containerModelRepository.QueryAsNoTracking(c => c.ContainerNo == containerNo).OrderByDescending(c => c.Id).FirstOrDefaultAsync();
                if (container != null)
                {
                    globalId = EntityType.Container + "_" + container.Id;
                }
            }

            if (string.IsNullOrEmpty(globalId))
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }
            var model = await Repository.Query(
                        activity => activity.ActivityCode == activityCode && activity.GlobalIdActivities.Any(globalIdActivity => globalIdActivity.GlobalId == globalId),
                        x => x.OrderByDescending(activity => activity.Id),
                        x => x.Include(activity => activity.GlobalIdActivities))
                        .FirstOrDefaultAsync();

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }

            model.GlobalIdActivities = model.GlobalIdActivities.Where(globalIdActivity => globalIdActivity.GlobalId == globalId).ToList();

            return model;
        }

        public async Task<ActivityViewModel> UpdateAsync(ActivityViewModel viewModel, long id, IdentityInfo currentUser = null)
        {
            viewModel.ValidateAndThrow(true);

            var model = await this.Repository.GetAsync(x => x.Id == id, null, FullIncludeProperties);

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", id)} not found!");
            }

            if (viewModel.PurchaseOrderId.HasValue)
            {
                if (model.CreatedBy == AppConstant.SYSTEM_USERNAME || model.CreatedBy == AppConstant.EDISON_USERNAME)
                {
                    throw new ApplicationException("Cannot update the Purchase Order Activity has been created by the system");
                } 

                if (currentUser == null)
                {
                    throw new ApplicationException("This activity cannot be updated");
                } 
                else
                {
                    if (!currentUser.IsInternal && model.CreatedBy != currentUser.Username && !currentUser.IsAgent) 
                    {
                        throw new ApplicationException("This activity cannot be updated");
                    }
                }
            }

            var previousActivityCode = model.ActivityCode;
          
            Mapper.Map(viewModel, model);

            var eventModel = await _csfeApiClient.GetEventByCodeAsync(model.ActivityCode);

            if (viewModel.IsPropertyDirty(nameof(viewModel.Location)))
            {
                bool isLocationRequired = eventModel?.LocationRequired ?? false;
                if (isLocationRequired && string.IsNullOrWhiteSpace(viewModel.Location))
                {
                    throw new ApplicationException($"Location must not be empty for the event #{model.ActivityCode}");
                }
            }
            if (viewModel.IsPropertyDirty(nameof(viewModel.Remark)))
            {
                bool isRemarkRequired = eventModel?.RemarkRequired ?? false;
                if (isRemarkRequired && string.IsNullOrWhiteSpace(viewModel.Remark))
                {
                    throw new ApplicationException($"Remark must not be empty for the event #{model.ActivityCode}");
                }
            }

            model.AddDomainEvent(
               new ActivityChangedDomainEvent(previousActivityCode,
               model.ActivityCode,
               model.ActivityDate,
               viewModel.ContainerId,
               viewModel.ShipmentId,
               viewModel.POFulfillmentId,
               viewModel.FreightSchedulerId,
               model.Location,
               model.Remark)
           );

            model.GlobalIdActivities = GetGlobalIdActivities(model, viewModel);

            Repository.Update(model);
            await UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<ActivityViewModel>(model);
            return viewModel;
        }

        public async Task DeleteAsync(long id)
        {
            var activity = await Repository.GetAsync(a => a.Id == id,
            null,
            i => i.Include(a => a.GlobalIdActivities));

            if (activity == null)
            {
                throw new AppEntityNotFoundException($"Object with the id #{id} not found!");
            }

            activity.AddDomainEvent(new ActivityDeletedDomainEvent(activity));
            Repository.Remove(activity);
            await this.UnitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id, bool IsDeleteATAViaFSApi)
        {
            var activity = await Repository.GetAsync(a => a.Id == id,
           null,
           i => i.Include(a => a.GlobalIdActivities));

            if (activity == null)
            {
                throw new AppEntityNotFoundException($"Object with the id #{id} not found!");
            }

            activity.AddDomainEvent(new ActivityDeletedDomainEvent(activity, IsDeleteATAViaFSApi));
            Repository.Remove(activity);
            await this.UnitOfWork.SaveChangesAsync();
        }

        private ActivityModel CombineMasterEvent(Event eventModel, ActivityViewModel viewModel)
        {
            var result = new ActivityModel()
            {
                ActivityCode = eventModel.ActivityCode,
                ActivityDescription = eventModel.ActivityDescription,
                ActivityType = eventModel.ActivityType,
                ActivityDate = viewModel.ActivityDate,
                CreatedBy = viewModel.CreatedBy,
                Remark = viewModel.Remark,
                Location = viewModel.Location,
                CreatedDate = viewModel.CreatedDate,
                Resolution = viewModel.Resolution,
                ResolutionDate = viewModel.ResolutionDate,
                Resolved = viewModel.Resolved,
                UpdatedBy = viewModel.UpdatedBy,
                UpdatedDate = viewModel.UpdatedDate,
                GlobalIdActivities = new List<GlobalIdActivityModel>()
            };
            return result;
        }

        private List<GlobalIdActivityModel> GetGlobalIdActivities(ActivityModel model, ActivityViewModel viewModel)
        {
            var newGlobalIdActivities = new List<GlobalIdActivityModel>();

            if (viewModel.ShipmentId.HasValue)
            {
                var globalId = CommonHelper.GenerateGlobalId(viewModel.ShipmentId.Value, EntityType.Shipment);
                newGlobalIdActivities.Add(GetNewGlobalActivity(globalId, model, viewModel));
            }
            if (viewModel.ContainerId.HasValue)
            {
                var globalId = CommonHelper.GenerateGlobalId(viewModel.ContainerId.Value, EntityType.Container);
                newGlobalIdActivities.Add(GetNewGlobalActivity(globalId, model, viewModel));
            }
            if (viewModel.ConsignmentId.HasValue)
            {
                var globalId = CommonHelper.GenerateGlobalId(viewModel.ConsignmentId.Value, EntityType.Consignment);
                newGlobalIdActivities.Add(GetNewGlobalActivity(globalId, model, viewModel));
            }
            if (viewModel.PurchaseOrderId.HasValue)
            {
                var globalId = CommonHelper.GenerateGlobalId(viewModel.PurchaseOrderId.Value, EntityType.CustomerPO);
                newGlobalIdActivities.Add(GetNewGlobalActivity(globalId, model, viewModel));
            }
            if (viewModel.POFulfillmentId.HasValue)
            {
                var globalId = CommonHelper.GenerateGlobalId(viewModel.POFulfillmentId.Value, EntityType.POFullfillment);
                newGlobalIdActivities.Add(GetNewGlobalActivity(globalId, model, viewModel));
            }
            if (viewModel.CruiseOrderId.HasValue)
            {
                var globalId = CommonHelper.GenerateGlobalId(viewModel.CruiseOrderId.Value, EntityType.CruiseOrder);
                newGlobalIdActivities.Add(GetNewGlobalActivity(globalId, model, viewModel));
            }

            if (viewModel.FreightSchedulerId.HasValue)
            {
                var globalId = CommonHelper.GenerateGlobalId(viewModel.FreightSchedulerId.Value, EntityType.FreightScheduler);
                newGlobalIdActivities.Add(GetNewGlobalActivity(globalId, model, viewModel));
            }

            return newGlobalIdActivities;
        }

        private GlobalIdActivityModel GetNewGlobalActivity(string globalId, ActivityModel model, ActivityViewModel viewModel)
        {
            return new GlobalIdActivityModel
            {
                GlobalId = globalId,
                ActivityId = model.Id,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                Remark = viewModel.Remark,
                Location = viewModel.Location,
                ActivityDate = viewModel.ActivityDate,
                CreatedBy = viewModel.CreatedBy,
                RowVersion = model.GlobalIdActivities?.OrderByDescending(x => x.CreatedDate).FirstOrDefault(ga => ga.GlobalId == globalId)?.RowVersion
            };
        }
    }
}
