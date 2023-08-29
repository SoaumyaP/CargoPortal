using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Groove.SP.Application.CargoDetail.Services.Interfaces;
using Groove.SP.Application.Common;
using Groove.SP.Application.Consolidation.Services.Interfaces;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.ShipmentLoadDetails.Services.Interfaces;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Groove.SP.Application.ShipmentLoadDetails.Services
{
    public class ShipmentLoadDetailService : ServiceBase<ShipmentLoadDetailModel, ShipmentLoadDetailViewModel>, IShipmentLoadDetailService
    {
        private readonly AppConfig _appConfig;
        private readonly IDataQuery _dataQuery;
        private readonly ICargoDetailService _cargoDetailService;
        private readonly IConsolidationService _consolidationService;
        public ShipmentLoadDetailService(IUnitOfWorkProvider unitOfWorkProvider,
            IOptions<AppConfig> appConfig, ICargoDetailService cargoDetailService,
            IConsolidationService consolidationService,
            IDataQuery dataQuery)
            : base(unitOfWorkProvider)
        {
            _appConfig = appConfig.Value;
            _cargoDetailService = cargoDetailService;
            _consolidationService = consolidationService;
            _dataQuery = dataQuery;
        }
        
        public async Task<ShipmentLoadDetailViewModel> GetAsync(long id)
        {
            var model = await Repository.GetAsync(x => x.Id == id);
            return Mapper.Map<ShipmentLoadDetailViewModel>(model);
        }

        public async Task<DataSourceResult> GetListByConsolidationAsync(DataSourceRequest request, long consolidationId, bool isInternal, long organizationId)
        {
            IQueryable<CargoLoadDetailQueryModel> query;
            string sql = @"
                        SELECT 
	                        sld.Id,
	                        sld.[Sequence],
	                        sld.ShipmentId,
	                        s.ShipmentNo,
	                        cd.OrderId,
	                        po.PONumber,
	                        cd.ItemId,
	                        poi.ProductCode,
	                        cd.[Description] AS CargoDescription,
	                        sld.Unit,
	                        sld.UnitUOM,
	                        sld.Package,
	                        sld.PackageUOM,
	                        sld.Volume,
	                        sld.VolumeUOM,
	                        sld.GrossWeight,
	                        sld.GrossWeightUOM,
                            cd.NetWeight,
							cd.NetWeightUOM,
                            CASE
                                WHEN t.LoadedUnitQty is null
                                    THEN cd.Unit
                                ELSE (cd.Unit - t.LoadedUnitQty)
                            END AS BalanceUnitQty,
	                        CASE
                                WHEN t.LoadedPackageQty is null
                                    THEN cd.Package
                                ELSE (cd.Package - t.LoadedPackageQty)
                            END AS BalancePackageQty,
	                        CASE
                                WHEN t.LoadedVolume is null
                                    THEN cd.Volume
                                ELSE (cd.Volume - t.LoadedVolume)
                            END AS BalanceVolume,
	                        CASE
                                WHEN t.LoadedGrossWeight is null
                                    THEN cd.GrossWeight
                                ELSE (cd.GrossWeight - t.LoadedGrossWeight)
                            END AS BalanceGrossWeight
                        FROM ShipmentLoadDetails sld (NOLOCK)
                        INNER JOIN Shipments s (NOLOCK) ON sld.ShipmentId = s.id
                        INNER JOIN CargoDetails cd (NOLOCK) ON sld.CargoDetailId = cd.Id
                        INNER JOIN PurchaseOrders po (NOLOCK) ON cd.OrderId = po.Id
                        INNER JOIN POLineItems poi (NOLOCK) ON cd.ItemId = poi.Id AND poi.PurchaseOrderId = po.Id
                        OUTER APPLY (
	                        SELECT SUM(sld.Unit) AS LoadedUnitQty, SUM(sld.Package) AS LoadedPackageQty, SUM(sld.Volume) AS LoadedVolume, SUM(sld.GrossWeight) AS LoadedGrossWeight
	                        FROM ShipmentLoadDetails sld (NOLOCK)
	                        WHERE sld.CargoDetailId = cd.id AND sld.ConsolidationId != {0}
                        ) AS t
                        WHERE sld.ConsolidationId = {0}";
            query = _dataQuery.GetQueryable<CargoLoadDetailQueryModel>(sql, consolidationId);
            return await query.ProjectTo<CargoLoadDetailViewModel>(Mapper.ConfigurationProvider)
                .ToDataSourceResultAsync(request);
        }

        public async Task<IEnumerable<ShipmentLoadDetailListViewModel>> GetShipmentLoadDetailByContainer(long containerId, bool isInternal, string affiliates)
        {
            var listOfAffiliates = new List<long>();
            IEnumerable<ShipmentLoadDetailModel> shipmentLoadDetailModels = null;

            if (isInternal)
            {
                shipmentLoadDetailModels = await Repository.Query(s => s.ContainerId == containerId,
                                                    n => n.OrderBy(x => x.Sequence),
                                                    n => n.Include(x => x.Shipment).ThenInclude(x => x.ShipmentBillOfLadings).ThenInclude(x => x.BillOfLading)).ToListAsync();
            }
            else
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }

                shipmentLoadDetailModels = await Repository.Query(s => s.ContainerId == containerId &&
                                                s.Shipment.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId)),
                                                n => n.OrderBy(x => x.Sequence),
                                                n => n.Include(x => x.Shipment).ThenInclude(x => x.ShipmentBillOfLadings).ThenInclude(x => x.BillOfLading)).ToListAsync();
            }
            
            var shipmentLoadDetailViewModels = Mapper.Map<IEnumerable<ShipmentLoadDetailListViewModel>>(shipmentLoadDetailModels);
            // Get Cargo detail listing info
            if (shipmentLoadDetailViewModels != null)
            {
                var cargoDetailsByContainer = await _cargoDetailService.GetCargoDetailsByContainerAsync(containerId);
                foreach (var shipmentLoadDetail in shipmentLoadDetailViewModels)
                {
                    shipmentLoadDetail.CargoDetail = cargoDetailsByContainer?.FirstOrDefault(x => x.Id == shipmentLoadDetail.CargoDetailId);
                }
            }
            return shipmentLoadDetailViewModels;
        }

        public async Task<IEnumerable<ShipmentLoadDetailModel>> UpdateRangeByConsolidationAsync(long consolidationId, IEnumerable<UpdateShipmentLoadDetailViewModel> viewModel, string username)
        {
            UnitOfWork.BeginTransaction();
            var models = await Repository.Query(x => x.ConsolidationId == consolidationId).ToListAsync();

            if (models == null)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }
            var updateShipmentLoadDetails = new List<ShipmentLoadDetailModel>();
            var deleteShipmentLoadDetails = new List<ShipmentLoadDetailModel>();
            foreach (var itemModel in models)
            {
                var itemViewModel = viewModel.Where(x => x.Id == itemModel.Id).FirstOrDefault();
                if (itemViewModel == null)
                {
                    deleteShipmentLoadDetails.Add(itemModel);
                }
                else
                {
                    Mapper.Map(itemViewModel, itemModel);
                    itemModel.Audit(username);
                    updateShipmentLoadDetails.Add(itemModel);
                }
            }

            Repository.UpdateRange(updateShipmentLoadDetails.ToArray());
            Repository.RemoveRange(deleteShipmentLoadDetails.ToArray());
            await UnitOfWork.SaveChangesAsync();

            await _consolidationService.UpdateConsolidationTotalAmountAsync(consolidationId);
            await UnitOfWork.SaveChangesAsync();

            UnitOfWork.CommitTransaction();

            var result = Mapper.Map<List<ShipmentLoadDetailModel>>(models);
            return result;
        }
    }
}
