using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.Application.GlobalIdActivity.RequestModels;
using Groove.SP.Application.GlobalIdActivity.Services.Interfaces;
using Groove.SP.Application.GlobalIdActivity.ViewModels;
using Groove.SP.Application.Interfaces.Repositories;

using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Core.Data;

namespace Groove.SP.Application.GlobalIdActivity.Services
{
    public class GlobalIdActivityService : ServiceBase<GlobalIdActivityModel, GlobalIdActivityViewModel>, IGlobalIdActivityService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IDataQuery _dataQuery;
        public GlobalIdActivityService(
            IUnitOfWorkProvider unitOfWorkProvider,
            IUserProfileRepository userProfileRepository,
            IDataQuery dataQuery)
            : base(unitOfWorkProvider)
        {
            _userProfileRepository = userProfileRepository;
            _dataQuery = dataQuery;
        }

        protected override Func<IQueryable<GlobalIdActivityModel>, IQueryable<GlobalIdActivityModel>>
            FullIncludeProperties => x
            => x.Include(i => i.Activity);

        public async Task<IEnumerable<GlobalIdActivityViewModel>> GetByPOFF(long poffId, GlobalIdActivityRequestModel requestModel)
        {
            var globalId = CommonHelper.GenerateGlobalId(poffId, EntityType.POFullfillment);
            IEnumerable<GlobalIdActivityViewModel> viewModel;

            if (requestModel.Ascending)
            {
                var rs = Repository.GetListQueryable(FullIncludeProperties)
                    .Where(a => a.GlobalId == globalId &&
                                (requestModel.FilterEventDate == default(DateTime) ||
                                 a.ActivityDate.Date == requestModel.FilterEventDate.Date))
                    .OrderBy(o => o.ActivityDate)
                    .Skip(requestModel.PageIndex * requestModel.PageSize).Take(requestModel.PageSize);

                viewModel = Mapper.Map<IEnumerable<GlobalIdActivityViewModel>>(rs);
            }
            else
            {
                var rs = Repository.GetListQueryable(FullIncludeProperties)
                    .Where(a => a.GlobalId == globalId &&
                                (requestModel.FilterEventDate == default(DateTime) ||
                                 a.ActivityDate.Date == requestModel.FilterEventDate.Date))
                    .OrderByDescending(o => o.ActivityDate)
                    .Skip(requestModel.PageIndex * requestModel.PageSize).Take(requestModel.PageSize);

                viewModel = Mapper.Map<IEnumerable<GlobalIdActivityViewModel>>(rs);
            }

            await BindingActor(viewModel);
            return viewModel;
        }

        private async Task BindingActor(IEnumerable<GlobalIdActivityViewModel> viewModel)
        {
            var createdByList = viewModel.GroupBy(x => x.CreatedBy).Select(x => x.First()).ToList().Select(x => x.CreatedBy);
            var userList = await _userProfileRepository.Query(x => createdByList.Contains(x.Username)).ToListAsync();

            foreach (var item in viewModel)
            {
                if (item.CreatedBy == AppConstant.SYSTEM_USERNAME || item.CreatedBy == AppConstant.EDISON_USERNAME)
                {
                    item.Actor = AppConstant.SYSTEM_USERNAME;
                }
                else
                {
                    item.Actor = userList.Find(x => x.Username == item.CreatedBy)?.Name ?? item.CreatedBy;
                }
            }
        }

        public async Task<IEnumerable<GlobalIdActivityViewModel>> GetByPO(long poId, GlobalIdActivityRequestModel requestModel)
        {
            var globalId = CommonHelper.GenerateGlobalId(poId, EntityType.CustomerPO);
            IEnumerable<GlobalIdActivityViewModel> viewModel;

            if (requestModel.Ascending)
            {
                var result = Repository.GetListQueryable(FullIncludeProperties)
                    .Where(a => a.GlobalId == globalId &&
                                (requestModel.FilterEventDate == default(DateTime) ||
                                 a.ActivityDate.Date == requestModel.FilterEventDate.Date))
                    .OrderBy(o => o.ActivityDate)
                    .Skip(requestModel.PageIndex * requestModel.PageSize).Take(requestModel.PageSize);

                viewModel = Mapper.Map<IEnumerable<GlobalIdActivityViewModel>>(result);
            }
            else
            {
                var result = Repository.GetListQueryable(FullIncludeProperties)
                    .Where(a => a.GlobalId == globalId &&
                                (requestModel.FilterEventDate == default(DateTime) ||
                                 a.ActivityDate.Date == requestModel.FilterEventDate.Date))
                    .OrderByDescending(o => o.ActivityDate)
                    .Skip(requestModel.PageIndex * requestModel.PageSize).Take(requestModel.PageSize);

                viewModel = Mapper.Map<IEnumerable<GlobalIdActivityViewModel>>(result);
            }
            await BindingActor(viewModel);
            return viewModel;
        }
        public async Task<GlobalIdActivityPagingViewModel> GetActivityTimelineAsync(long entityId, string entityType, GetActivityTimelineRequestModel requestModel)
        {
            var storedProcedureName = "spu_GetActivityTimeLineCrossModule";
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
                    },
                    new SqlParameter
                    {
                        ParameterName = "@filterBy",
                        Value = requestModel.FilterBy,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@filterValue",
                        Value = requestModel.FilterValue,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };

            if (requestModel.FromDate.HasValue)
            {
                parameters.Add(new SqlParameter
                {
                    ParameterName = "@filterFromDate",
                    Value = requestModel.FromDate,
                    DbType = DbType.DateTime2,
                    Direction = ParameterDirection.Input
                });
            }

            if (requestModel.ToDate.HasValue)
            {
                parameters.Add(new SqlParameter
                {
                    ParameterName = "@filterToDate",
                    Value = requestModel.ToDate,
                    DbType = DbType.DateTime2,
                    Direction = ParameterDirection.Input
                });
            }

            Func<DbDataReader, GlobalIdActivityPagingViewModel> mapping = (reader) =>
            {
                var mappedData = new GlobalIdActivityPagingViewModel();
                while (reader.Read())
                {
                    mappedData.TotalRecord = (int)reader[0];
                }

                reader.NextResult();

                mappedData.GlobalIdActivities = new List<GlobalIdActivityViewModel>();

                // Map data for activity information
                while (reader.Read())
                {
                    var newRow = new GlobalIdActivityViewModel();
                    newRow.Activity = new ActivityViewModel();
                    object tmpValue;

                    // Must be in order of data reader
                    newRow.Activity.Id = (long)reader[0];
                    newRow.ActivityId = newRow.Activity.Id;

                    tmpValue = reader[1];
                    newRow.Activity.ShipmentId = DBNull.Value == tmpValue ? (long?)null : (long)tmpValue;

                    tmpValue = reader[2];
                    newRow.Activity.ContainerId = DBNull.Value == tmpValue ? (long?)null : (long)tmpValue;

                    tmpValue = reader[3];
                    newRow.Activity.PurchaseOrderId = DBNull.Value == tmpValue ? (long?)null : (long)tmpValue;

                    tmpValue = reader[4];
                    newRow.Activity.POFulfillmentId = DBNull.Value == tmpValue ? (long?)null : (long)tmpValue;

                    tmpValue = reader[5];
                    newRow.Activity.FreightSchedulerId = DBNull.Value == tmpValue ? (long?)null : (long)tmpValue;

                    tmpValue = reader[6];
                    newRow.Activity.ActivityCode = tmpValue.ToString();

                    tmpValue = reader[7];
                    newRow.Activity.ActivityType = tmpValue.ToString();

                    tmpValue = reader[8];
                    newRow.Activity.ActivityLevel = tmpValue.ToString();

                    tmpValue = reader[9];
                    newRow.Activity.ActivityDate = (DateTime)tmpValue;

                    tmpValue = reader[10];
                    newRow.Activity.ActivityDescription = tmpValue.ToString();

                    tmpValue = reader[11];
                    newRow.Activity.Location = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[12];
                    newRow.Activity.Remark = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[13];
                    newRow.POFulfillmentNos = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[14];
                    newRow.ShipmentNos = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[15];
                    newRow.ContainerNos = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[16];
                    newRow.VesselFlight = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[17];
                    newRow.Activity.Resolved = DBNull.Value == tmpValue ? (bool?)null : (bool)tmpValue;

                    tmpValue = reader[18];
                    newRow.Activity.Resolution = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[19];
                    newRow.Activity.ResolutionDate = DBNull.Value == tmpValue ? (DateTime?)null : (DateTime)tmpValue;

                    tmpValue = reader[20];
                    newRow.Activity.CreatedBy = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[21];
                    newRow.Activity.CreatedDate = (DateTime)tmpValue;

                    newRow.Id = (long)reader[22];

                    tmpValue = reader[23];
                    newRow.GlobalId = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[24];
                    newRow.Location = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[25];
                    newRow.Remark = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[26];
                    newRow.ActivityDate = (DateTime)tmpValue;

                    tmpValue = reader[27];
                    newRow.CreatedDate = (DateTime)tmpValue;

                    tmpValue = reader[28];
                    newRow.UpdatedDate = DBNull.Value == tmpValue ? (DateTime?)null : (DateTime)tmpValue;

                    tmpValue = reader[29];
                    newRow.CreatedBy = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    tmpValue = reader[30];
                    newRow.Activity.SortSequence = (long)tmpValue;

                    mappedData.GlobalIdActivities.Add(newRow);
                }

                return mappedData;
            };
            var viewModel = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, parameters.ToArray());
            await BindingActor(viewModel.GlobalIdActivities);
            return viewModel;
        }

        public async Task<int> GetActivityTotalAsync(long entityId, string entityType)
        {
            var storedProcedureName = "spu_GetActivityTimeLineCrossModule";
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

            Func<DbDataReader, GlobalIdActivityPagingViewModel> mapping = (reader) =>
            {
                var mappedData = new GlobalIdActivityPagingViewModel();
                while (reader.Read())
                {
                    mappedData.TotalRecord = (int)reader[0];
                }

                return mappedData;
            };

            var total = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, parameters.ToArray());
            return total?.TotalRecord ?? 0;
        }

        public async Task<IEnumerable<DropDownListItem>> GetFilterValueDropdownAsync(long entityId, string entityType, string filterBy)
        {
            var storedProcedureName = "spu_GetFilterValue_ActivityTimeline";
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
                    },
                    new SqlParameter
                    {
                        ParameterName = "@filterBy",
                        Value = filterBy,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, IEnumerable<DropDownListItem>> mapping = (reader) =>
            {
                var mappedData = new List<DropDownListItem>();
                while (reader.Read())
                {
                    var tmpValue = reader[0].ToString();
                    var item = new DropDownListItem
                    {
                        Text = tmpValue,
                        Value = tmpValue
                    };
                    mappedData.Add(item);
                };

                return mappedData;
            };
            return await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, parameters.ToArray());
        }

        public async Task DeleteActivitiesByShipmentAndConsigmentAsync(long shipmentId, long consignmentId)
        {
            var consignmentGlobalId = CommonHelper.GenerateGlobalId(consignmentId, EntityType.Consignment);
            var shipmentGlobalId = CommonHelper.GenerateGlobalId(shipmentId, EntityType.Shipment);

            var activitiesOfConsignment = await Repository.Query(g => g.GlobalId == consignmentGlobalId).ToListAsync();
            var activityIds = activitiesOfConsignment.Select(x => x.ActivityId);

            var activitiesOfShipnment = await Repository.Query(g => g.GlobalId == shipmentGlobalId && activityIds.Contains(g.ActivityId)).ToListAsync();

            foreach (var activity in activitiesOfConsignment)
            {
                Repository.Remove(activity);
            }

            foreach (var activity in activitiesOfShipnment)
            {
                Repository.Remove(activity);
            }
        }
    }
}
