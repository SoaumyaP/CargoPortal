using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Application.CruiseOrders.Services.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Application.Interfaces.Repositories;
using Newtonsoft.Json;
using Groove.SP.Application.ApplicationBackgroundJob.Services;
using Groove.SP.Application.Cruise.CruiseOrders.BackgroundJobs;

namespace Groove.SP.Application.CruiseOrders.Services
{
    public class CruiseOrderItemService : ServiceBase<CruiseOrderItemModel, CruiseOrderItemViewModel>, ICruiseOrderItemService
    {
        private readonly IRepository<CruiseOrderItemModel> _cruiseOrderItemRepository;
        private readonly IQueuedBackgroundJobs _queuedBackgroundJobs;
        private readonly IDataQuery _dataQuery;

        #region Constructor
        public CruiseOrderItemService(IUnitOfWorkProvider unitOfWorkProvider,
            IRepository<CruiseOrderItemModel> cruiseOrderItemRepository,
            IQueuedBackgroundJobs queuedBackgroundJobs,
            IDataQuery dataQuery
            ) : base(unitOfWorkProvider)
        {
            _cruiseOrderItemRepository = cruiseOrderItemRepository;
            _queuedBackgroundJobs = queuedBackgroundJobs;
            _dataQuery = dataQuery;
        }
        #endregion

        public async Task<CruiseOrderItemViewModel> CreateCruiseOrderItemAsync(CruiseOrderItemViewModel viewModel, string userName)
        {
            viewModel.Audit(userName);
            viewModel.ValidateAndThrow();

            CruiseOrderItemModel model = Mapper.Map<CruiseOrderItemModel>(viewModel);

            var error = await ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }

            await Repository.AddAsync(model);
            await UnitOfWork.SaveChangesAsync();

            OnEntityCreated(model);

            var jsonNewData = JsonConvert.SerializeObject(model, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });

            // Call spu to write log
            // [cruise].[spu_DeleteCruiseOrderItemViaUI]
            // @cruiseOrderItemId BIGINT,
            // @jsonCurrentData NVARCHAR(MAX),
            // @jsonNewData NVARCHAR(MAX),
            // @updatedBy NVARCHAR(512)
            var sql = @"[cruise].[spu_WriteLogCruiseOrderItem] @p0, @p1, @p2, @p3";
            var parameters = new object[]
            {
                    model.Id,
                    null,
                    $"[{jsonNewData}]",
                    userName
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

            // Sync to POLineItems
            _queuedBackgroundJobs.Enqueue<POLineItemSyncJob>(j => j.ExecuteAsync(model.OrderId));

            viewModel = Mapper.Map<CruiseOrderItemViewModel>(model);
            return viewModel;
        }

        public async Task<IEnumerable<CruiseOrderItemViewModel>> ReviseCruiseOrderItemAsync(long cruiseOrderItemId, ReviseCruiseOrderItemViewModel model, string userName)
        {

            if (model.SelectedItemPOLines == null || !model.SelectedItemPOLines.Any())
            {
                throw new AppException("Can not update cruise order items as selected items are invalid!");
            }

            var updatingCruiseOrderItems = _cruiseOrderItemRepository.Query(x => x.OrderId == model.OrderId && model.SelectedItemPOLines.Contains(x.POLine)).ToList();

            if (updatingCruiseOrderItems == null || !updatingCruiseOrderItems.Any())
            {
                throw new AppException("Can not update cruise order items as not found!");
            }

            var jsonCurrentDataList = new List<Tuple<long, string>>();

            for (int i = 0; i < updatingCruiseOrderItems.Count; i++)
            {
                var item = updatingCruiseOrderItems[i];

                // Store current data to write log later
                var jsonCurrentData = JsonConvert.SerializeObject(item, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
                jsonCurrentDataList.Add(Tuple.Create(item.Id, jsonCurrentData));

                item = Mapper.Map<ReviseCruiseOrderItemViewModel, CruiseOrderItemModel>(model, item);
                item.Audit(userName);

            }

            _cruiseOrderItemRepository.UpdateRange(updatingCruiseOrderItems.ToArray());
            await UnitOfWork.SaveChangesAsync();


            // Loop in updated items, then write logs
            foreach (var item in jsonCurrentDataList)
            {
                var newData = updatingCruiseOrderItems.FirstOrDefault(x => x.Id == item.Item1);
                var jsonNewData = JsonConvert.SerializeObject(newData, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });

                // Call spu to write log
                // [cruise].[spu_DeleteCruiseOrderItemViaUI]
                // @cruiseOrderItemId BIGINT,
                // @jsonCurrentData NVARCHAR(MAX),
                // @jsonNewData NVARCHAR(MAX),
                // @updatedBy NVARCHAR(512)
                var sql = @"[cruise].[spu_WriteLogCruiseOrderItem] @p0, @p1, @p2, @p3";
                var parameters = new object[]
                {
                    item.Item1,
                    $"[{item.Item2}]",
                    $"[{jsonNewData}]",
                    userName
                };
                _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());
            }

            // need to call  to get latest information of updated cruise order items, including Shipment
            var updatedCruiseOrderItems = _cruiseOrderItemRepository.QueryAsNoTracking(x => x.OrderId == model.OrderId, includes: x => x.Include(y => y.Shipment)).ToList();

            // sync to POLineItems
            _queuedBackgroundJobs.Enqueue<POLineItemSyncJob>(j => j.ExecuteAsync(model.OrderId));

            return Mapper.Map<List<CruiseOrderItemViewModel>>(updatedCruiseOrderItems);
        }

        public async Task<bool> DeleteCruiseOrderItemAsync(long cruiseOrderItemId, IdentityInfo currentUser)
        {
            var cruiseOrderItem = await _cruiseOrderItemRepository.GetAsync(p => p.Id == cruiseOrderItemId);
            if (cruiseOrderItem == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {cruiseOrderItemId} not found!");
            }

            // Need to check if internal or external user must belong to same organization as who copied
            if (currentUser.IsInternal || (currentUser.OrganizationId == cruiseOrderItem.OriginalOrganizationId))
            {
                var jsonCurrentData = JsonConvert.SerializeObject(cruiseOrderItem, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });

                _cruiseOrderItemRepository.Remove(cruiseOrderItem);
                await UnitOfWork.SaveChangesAsync();

                // Call spu to write log
                // [cruise].[spu_DeleteCruiseOrderItemViaUI]
                // @cruiseOrderItemId BIGINT,
                // @jsonCurrentData NVARCHAR(MAX),
                // @jsonNewData NVARCHAR(MAX),
                // @updatedBy NVARCHAR(512)
                var sql = @"[cruise].[spu_WriteLogCruiseOrderItem] @p0, @p1, @p2, @p3";
                var parameters = new object[]
                {
                    cruiseOrderItemId,
                    $"[{jsonCurrentData}]",
                    null,
                    currentUser.Username
                };
                _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

                // Sync to POLineItems
                _queuedBackgroundJobs.Enqueue<POLineItemSyncJob>(j => j.ExecuteAsync(cruiseOrderItem.OrderId));

                return true;
            }
            throw new ApplicationException($"Object with the id {cruiseOrderItemId} could not be deleted!");
        }
    }
}
