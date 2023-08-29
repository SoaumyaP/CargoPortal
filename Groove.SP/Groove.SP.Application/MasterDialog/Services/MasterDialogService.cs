using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.MasterDialog.Services.Interfaces;
using Groove.SP.Application.MasterDialog.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.MasterDialog.Services
{
    public class MasterDialogService : ServiceBase<MasterDialogModel, MasterDialogViewModel>, IMasterDialogService
    {
        private readonly IDataQuery _dataQuery;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IRepository<GlobalIdMasterDialogModel> _globalIdMasterDialogRepository;

        public MasterDialogService(
            IUnitOfWorkProvider unitOfWorkProvider,
            IDataQuery dataQuery,
            ICSFEApiClient csfeApiClient,
            IRepository<GlobalIdMasterDialogModel> globalIdMasterDialogRepository)
            : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
            _csfeApiClient = csfeApiClient;
            _globalIdMasterDialogRepository = globalIdMasterDialogRepository;
        }

        public async Task<MasterDialogViewModel> CreateAsync(MasterDialogViewModel viewModel, string userName)
        {
            viewModel.ValidateAndThrow(false);
            var model = Mapper.Map<MasterDialogModel>(viewModel);

            var selectedItems = JsonConvert.DeserializeObject<SelectedMasterDialogItem[]>(viewModel.SelectedItems);
            model.SelectedItems = JsonConvert.SerializeObject(selectedItems.ToArray());

            var customerPOs = await SearchListOfPurchaseOrdersAsync(model.DisplayOn, model.FilterCriteria, model.FilterValue, "", 0, Int32.MaxValue, POListReturnType.PARENT_LEVEL);

            // List of globalIds will be linked to this message.
            var globalIds = customerPOs.Select(x => x.Value.Split("&")[0]).Distinct();

            // The record will be saved in GlobalIdMasterDialogs.
            foreach (var globalId in globalIds)
            {
                var globalIdMasterDialog = new GlobalIdMasterDialogModel()
                {
                    GlobalId = globalId
                };
                // List of selected item numbers for the dialog
                var dialogItemNumbers = selectedItems?.Where(x => x.Value.Split("&")[0] == globalId && !string.IsNullOrEmpty(x.DialogItemNumber))
                    .Select(x => x.DialogItemNumber);

                if (dialogItemNumbers?.Count() > 0)
                {
                    var entityType = globalId.Split("_")[0];
                    switch (entityType)
                    {
                        case EntityType.CustomerPO:
                            globalIdMasterDialog.ExtendedData = JsonConvert.SerializeObject(dialogItemNumbers.ToArray());
                            break;
                        case EntityType.POFullfillment:
                            globalIdMasterDialog.ExtendedData = JsonConvert.SerializeObject(dialogItemNumbers.Select(x => new { value = x }));
                            break;
                        case EntityType.Shipment:
                            globalIdMasterDialog.ExtendedData = JsonConvert.SerializeObject(dialogItemNumbers.Select(x => new { value = x }));
                            break;
                        default:
                            break;
                    }
                }

                globalIdMasterDialog.Audit(userName);
                model.GlobalIdMasterDialogs.Add(globalIdMasterDialog);
            }

            model.Audit(userName);
            await Repository.AddAsync(model);
            await UnitOfWork.SaveChangesAsync();
            return Mapper.Map<MasterDialogViewModel>(model);
        }

        public async Task<MasterDialogViewModel> UpdateAsync(long id, MasterDialogViewModel viewModel, string userName, bool isInternal, long organizationId)
        {
            UnitOfWork.BeginTransaction();

            viewModel.ValidateAndThrow(true);
            var modelNeedToUpdate = await Repository.FindAsync(id);

            if (modelNeedToUpdate == null || (!isInternal && modelNeedToUpdate.OrganizationId != organizationId))
            {
                throw new AppEntityNotFoundException($"Object with the id {id} not found!");
            }

            // Do Not update Owner/OrganizationId after master dialog created
            var owner = modelNeedToUpdate.Owner;
            var oldOrganizationId = modelNeedToUpdate.OrganizationId;

            Mapper.Map(viewModel, modelNeedToUpdate);

            modelNeedToUpdate.Owner = owner;
            modelNeedToUpdate.OrganizationId = oldOrganizationId;

            var selectedItems = JsonConvert.DeserializeObject<SelectedMasterDialogItem[]>(modelNeedToUpdate.SelectedItems);
            modelNeedToUpdate.SelectedItems = JsonConvert.SerializeObject(selectedItems.ToArray());

            modelNeedToUpdate.Audit(userName);
            Repository.Update(modelNeedToUpdate);
            await UnitOfWork.SaveChangesAsync();

            #region update GlobalIdMasterDialogs

            /** Remove current linked globalIdMasterDialogs */
            var globalIdMasterDialogs = await _globalIdMasterDialogRepository.Query(x => x.MasterDialogId == id).ToListAsync();
            _globalIdMasterDialogRepository.RemoveRange(globalIdMasterDialogs?.ToArray());
            await UnitOfWork.SaveChangesAsync();

            List<GlobalIdMasterDialogModel> newGlobalIdMasterDialogs = new List<GlobalIdMasterDialogModel>();
            var customerPOs = await SearchListOfPurchaseOrdersAsync(modelNeedToUpdate.DisplayOn, modelNeedToUpdate.FilterCriteria, modelNeedToUpdate.FilterValue, "", 0, Int32.MaxValue, POListReturnType.PARENT_LEVEL);

            /** List of globalIds will be linked to this message. */
            var globalIds = customerPOs.Select(x => x.Value.Split("&")[0]).Distinct();

            /** The record will be saved in GlobalIdMasterDialogs. */
            foreach (var globalId in globalIds)
            {
                var globalIdMasterDialogModel = new GlobalIdMasterDialogModel()
                {
                    GlobalId = globalId,
                    MasterDialogId = id
                };
                /** List of selected item numbers for the dialog */
                var dialogItemNumbers = selectedItems?.Where(x => x.Value.Split("&")[0] == globalId && !string.IsNullOrEmpty(x.DialogItemNumber))
                    .Select(x => x.DialogItemNumber);

                if (dialogItemNumbers?.Count() > 0)
                {
                    var entityType = globalId.Split("_")[0];
                    switch (entityType)
                    {
                        case EntityType.CustomerPO:
                            globalIdMasterDialogModel.ExtendedData = JsonConvert.SerializeObject(dialogItemNumbers.ToArray());
                            break;
                        case EntityType.POFullfillment:
                            globalIdMasterDialogModel.ExtendedData = JsonConvert.SerializeObject(dialogItemNumbers.Select(x => new { value = x }));
                            break;
                        case EntityType.Shipment:
                            globalIdMasterDialogModel.ExtendedData = JsonConvert.SerializeObject(dialogItemNumbers.Select(x => new { value = x }));
                            break;
                        default:
                            break;
                    }
                }

                globalIdMasterDialogModel.Audit(userName);
                newGlobalIdMasterDialogs.Add(globalIdMasterDialogModel);
            }

            await _globalIdMasterDialogRepository.AddRangeAsync(newGlobalIdMasterDialogs?.ToArray());
            await UnitOfWork.SaveChangesAsync();
            #endregion

            UnitOfWork.CommitTransaction();

            return Mapper.Map<MasterDialogViewModel>(modelNeedToUpdate);

        }

        public async Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, string affiliates = "", long? organizationId = 0)
        {
            IQueryable<MasterDialogQueryModel> query;
            string sql;

            if (!isInternal)
            {
                sql =
                    @"SELECT Id
	                    ,CreatedDate
                        ,CAST(CreatedDate AS DATE) AS CreatedDateOnly
                        ,[Owner]
                        ,Category
                        ,DisplayOn
                        ,FilterCriteria
                        ,FilterValue
                        ,[Message]
                    FROM MasterDialogs md
                    WHERE md.OrganizationId = {0}
                    ";
            }
            else
            {
                sql =
                    @"SELECT Id
	                    ,CreatedDate
                        ,CAST(CreatedDate AS DATE) AS CreatedDateOnly
                        ,[Owner]
                        ,Category
                        ,DisplayOn
                        ,FilterCriteria
                        ,FilterValue
                        ,[Message]
                    FROM MasterDialogs md
                    ";
            }

            query = _dataQuery.GetQueryable<MasterDialogQueryModel>(sql, organizationId);

            /**Custom filtering data*/
            if (request.Filters != null)
            {
                foreach (var filter in request.Filters)
                {
                    var descriptor = filter as FilterDescriptor;
                    FilterOperator[] appliedOperators = { FilterOperator.IsEqualTo, FilterOperator.IsNotEqualTo, FilterOperator.IsLessThanOrEqualTo, FilterOperator.IsGreaterThan };
                    if (descriptor != null && descriptor.Member == "createdDate" && appliedOperators.Contains(descriptor.Operator))
                    {
                        descriptor.Member = "createdDateOnly";
                    }
                    else if (filter is CompositeFilterDescriptor)
                    {
                        ModifyFilters(((CompositeFilterDescriptor)filter).FilterDescriptors);
                    }
                }
            }
            return await query.ToDataSourceResultAsync(request);
        }
        public async Task<IEnumerable<DropDownListItem<string>>> SearchNumberByFilterCriteriaAsync(string filterCriteria, string filterValue, bool isInternal, long? organizationId = 0)
        {
            var storedProcedureName = "spu_GetFilterValueSelectionList_MasterDialog";
            List<SqlParameter> filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@filterCriteria",
                        Value = filterCriteria,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@filterValue",
                        Value = filterValue,
                        DbType = DbType.String,
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
                    }
                };

            Func<DbDataReader, IEnumerable<DropDownListItem<string>>> mapping = (reader) =>
            {
                // Default value
                var mappedData = new List<DropDownListItem<string>>();
                while (reader.Read())
                {
                    var tmpNumber = reader[0];
                    var dataRow = new DropDownListItem<string>
                    {
                        Value = (string)tmpNumber,
                        Text = (string)tmpNumber
                    };
                    mappedData.Add(dataRow);
                }
                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }

        public async Task<IEnumerable<MasterDialogPOListItemViewModel>> SearchListOfPurchaseOrdersAsync(string messageShownOn, string filterCriteria, string filterValue, string searchTerm, int skip, int take, string returnType = POListReturnType.CHILD_LEVEL)
        {
            var storedProcedureName = "spu_GetListOfPurchaseOrders_OnMasterDialog";
            List<SqlParameter> filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@MessageShownOn",
                        Value = messageShownOn,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@FilterCriteria",
                        Value = filterCriteria,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@FilterValues",
                        Value = filterValue,
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
                    },
                    new SqlParameter
                    {
                        ParameterName = "@ReturnType",
                        Value = returnType,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, IEnumerable<MasterDialogPOListItemViewModel>> mapping = (reader) =>
            {
                // Default value
                var mappedData = new List<MasterDialogPOListItemViewModel>();
                while (reader.Read())
                {
                    var value = reader[0];
                    var text = reader[1];
                    var parentId = reader[2];
                    var dialogItemNumber = reader[3];
                    var totalRecords = reader[4];


                    var dataRow = new MasterDialogPOListItemViewModel
                    {
                        Value = value != DBNull.Value ? (string)value : string.Empty,
                        Text = text != DBNull.Value ? (string)text : string.Empty,
                        ParentId = parentId != DBNull.Value ? (string)parentId : string.Empty,
                        DialogItemNumber = dialogItemNumber != DBNull.Value ? (string)dialogItemNumber : string.Empty,
                        RecordCount = (long)totalRecords,
                        IsChecked = false,
                        IsDisabled = false
                    };
                    mappedData.Add(dataRow);
                }
                return mappedData;
            };
            var tmpData = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());


            // to transfer to hierarchy parent-child

            var result = tmpData.Where(x => string.IsNullOrEmpty(x.ParentId)).ToList();
            foreach (var parent in result)
            {
                var children = tmpData.Where(x => x.ParentId == parent.Value && !string.IsNullOrEmpty(x.ParentId)).OrderBy(x => x.Text).ToList();
                if (children.Count() > 0)
                {
                    parent.ChildrenItems = new List<MasterDialogPOListItemViewModel>(children);
                }
            }

            return result;
        }

        public async Task<IEnumerable<MasterDialogPOListItemViewModel>> SearchListOfPurchaseOrdersByMasterDialogIdAsync(long masterDialogId, string searchTerm, int skip, int take)
        {
            var storedProcedureName = "spu_GetListOfPurchaseOrders_ByMasterDialogId";
            List<SqlParameter> filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@MasterDialogId",
                        Value = masterDialogId,
                        DbType = DbType.Int64,
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

            Func<DbDataReader, IEnumerable<MasterDialogPOListItemViewModel>> mapping = (reader) =>
            {
                // Default value
                var mappedData = new List<MasterDialogPOListItemViewModel>();
                while (reader.Read())
                {
                    var value = reader[0];
                    var text = reader[1];
                    var parentId = reader[2];
                    var dialogItemNumber = reader[3];
                    var totalRecords = reader[4];


                    var dataRow = new MasterDialogPOListItemViewModel
                    {
                        Value = value != DBNull.Value ? (string)value : string.Empty,
                        Text = text != DBNull.Value ? (string)text : string.Empty,
                        ParentId = parentId != DBNull.Value ? (string)parentId : string.Empty,
                        DialogItemNumber = dialogItemNumber != DBNull.Value ? (string)dialogItemNumber : string.Empty,
                        RecordCount = (long)totalRecords,
                        IsChecked = false,
                        IsDisabled = false
                    };
                    mappedData.Add(dataRow);
                }
                return mappedData;
            };
            var tmpData = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());


            // to transfer to hierarchy parent-child

            var result = tmpData.Where(x => string.IsNullOrEmpty(x.ParentId)).ToList();
            foreach (var parent in result)
            {
                var children = tmpData.Where(x => x.ParentId == parent.Value && !string.IsNullOrEmpty(x.ParentId)).OrderBy(x=>x.Text).ToList();
                if (children.Count() > 0)
                {
                    children.Each(x => x.ParentId = parent.Value);
                    parent.ChildrenItems = new List<MasterDialogPOListItemViewModel>(children);
                }
            }

            return result;
        }

        public async Task<MasterDialogViewModel> GetAsync(long id, bool isInternal, long organizationId)
        {
            var query = Repository.QueryAsNoTracking(x => x.Id == id);
            if (!isInternal)
            {
                query = query.Where(x => x.OrganizationId == organizationId);
            }
            var model = await query.FirstOrDefaultAsync();
            return model != null ? Mapper.Map<MasterDialogViewModel>(model) : null;
        }

        public async Task<bool> DeleteByKeysAsync(long id, bool isInternal, long organizationId)
        {
            var model = await Repository.GetAsNoTrackingAsync(x => x.Id == id);

            if (model == null || (!isInternal && model.OrganizationId != organizationId))
            {
                throw new AppEntityNotFoundException($"Object with the id {id} not found!");
            }

            var isDeleted = this.Repository.RemoveByKeys(id);

            await this.UnitOfWork.SaveChangesAsync();

            return isDeleted;
        }
    }

    public class SelectedMasterDialogItem
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("dialogItemNumber")]
        public string DialogItemNumber { get; set; }

        [JsonProperty("parentId")]
        public string ParentId { get; set; }
    }
}
