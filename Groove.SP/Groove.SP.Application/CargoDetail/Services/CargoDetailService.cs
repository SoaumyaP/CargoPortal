using Groove.SP.Application.CargoDetail.Services.Interfaces;
using Groove.SP.Application.CargoDetail.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.CargoDetail.Services
{
    public class CargoDetailService : ServiceBase<CargoDetailModel, CargoDetailViewModel>, ICargoDetailService
    {
        private readonly IPOFulfillmentAllocatedOrderRepository _poffAllocatedOrderRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IRepository<CruiseOrderModel> _cruiseOrderRepository;
        private readonly IDataQuery _dataQuery;

        public CargoDetailService(IUnitOfWorkProvider unitOfWorkProvider,
            IPOFulfillmentAllocatedOrderRepository poffAllocatedOrderRepository,
            IPurchaseOrderRepository purchaseOrderRepository,
            IRepository<CruiseOrderModel> cruiseOrderRepository,
            IDataQuery dataQuery
            )
            : base(unitOfWorkProvider)
        {
            _poffAllocatedOrderRepository = poffAllocatedOrderRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _cruiseOrderRepository = cruiseOrderRepository;
            _dataQuery = dataQuery;
        }

        public async Task<IEnumerable<CargoDetailListViewModel>> GetCargoDetailsByShipmentAsync(long shipmentId)
        {
            var storedProcedureName = "spu_GetCargoDetailList_ByShipmentId";
            List<SqlParameter> filterParameter;

            filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@ShipmentId",
                        Value = shipmentId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, IEnumerable<CargoDetailListViewModel>> mapping = (reader) =>
            {
                // Default value
                var mappedData = new List<CargoDetailListViewModel>();

                while (reader.Read())
                {
                    // Must be in order of data reader
                    var id = reader[0];
                    var shipmentMarks = reader[1];
                    var description = reader[2];
                    var unit = reader[3];
                    var unitUOM = reader[4];
                    var package = reader[5];
                    var packageUOM = reader[6];
                    var volume = reader[7];
                    var volumeUOM = reader[8];
                    var grossWeight = reader[9];
                    var grossWeightUOM = reader[10];
                    var commodity = reader[11];
                    var hsCode = reader[12];
                    var purchaseOrderId = reader[13];
                    var customerPONumber = reader[14];
                    var poLineItemId = reader[15];
                    var productCode = reader[16];
                    var lineOrder = reader[17];
                    var orderType = reader[18];

                    var newRow = new CargoDetailListViewModel() {
                        Id = (long)id,
                        ShippingMarks = shipmentMarks?.ToString(),
                        Description = description?.ToString(),
                        Unit = (decimal?)unit,
                        UnitUOM = unitUOM?.ToString(),
                        Package = (decimal?)package,
                        PackageUOM = packageUOM?.ToString(),
                        Volume = (decimal?)volume,
                        VolumeUOM = volumeUOM?.ToString(),
                        GrossWeight = (decimal?)grossWeight,
                        GrossWeightUOM = grossWeightUOM?.ToString(),
                        Commodity = commodity?.ToString(),
                        HSCode = hsCode?.ToString(),
                        PurchaseOrderId = purchaseOrderId != DBNull.Value ? (long)purchaseOrderId : (long?)null,
                        CustomerPONumber = customerPONumber?.ToString(),
                        POLineItemId = poLineItemId != DBNull.Value ? (long)poLineItemId : (long?)null,
                        ProductCode = productCode?.ToString(),
                        LineOrder = lineOrder != DBNull.Value ? (int)lineOrder : (int?)null,
                        OrderType = (int)orderType
                    };

                    mappedData.Add(newRow);
                }
                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }

        public async Task<IEnumerable<CargoDetailListViewModel>> GetCargoDetailsByContainerAsync(long containerId)
        {
            var storedProcedureName = "spu_GetCargoDetailList_ByContainerId";
            List<SqlParameter> filterParameter;

            filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@ContainerId",
                        Value = containerId,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, IEnumerable<CargoDetailListViewModel>> mapping = (reader) =>
            {
                // Default value
                var mappedData = new List<CargoDetailListViewModel>();

                while (reader.Read())
                {
                    // Must be in order of data reader
                    var id = reader[0];
                    var shipmentMarks = reader[1];
                    var description = reader[2];
                    var package = reader[3];
                    var packageUOM = reader[4];
                    var volume = reader[5];
                    var volumeUOM = reader[6];
                    var grossWeight = reader[7];
                    var grossWeightUOM = reader[8];
                    var commodity = reader[9];
                    var hsCode = reader[10];
                    var purchaseOrderId = reader[11];
                    var customerPONumber = reader[12];
                    var poLineItemId = reader[13];
                    var productCode = reader[14];
                    var lineOrder = reader[15];
                    var orderType = reader[16];

                    var newRow = new CargoDetailListViewModel()
                    {
                        Id = (long)id,
                        ShippingMarks = shipmentMarks?.ToString(),
                        Description = description?.ToString(),
                        Package = (decimal?)package,
                        PackageUOM = packageUOM?.ToString(),
                        Volume = (decimal?)volume,
                        VolumeUOM = volumeUOM?.ToString(),
                        GrossWeight = (decimal?)grossWeight,
                        GrossWeightUOM = grossWeightUOM?.ToString(),
                        Commodity = commodity?.ToString(),
                        HSCode = hsCode?.ToString(),
                        PurchaseOrderId = purchaseOrderId != DBNull.Value ? (long)purchaseOrderId : (long?)null,
                        CustomerPONumber = customerPONumber?.ToString(),
                        POLineItemId = poLineItemId != DBNull.Value ? (long)poLineItemId : (long?)null,
                        ProductCode = productCode?.ToString(),
                        LineOrder = lineOrder != DBNull.Value ? (int)lineOrder : (int?)null,
                        OrderType = (int)orderType
                    };

                    mappedData.Add(newRow);
                }
                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }

        public override async Task<CargoDetailViewModel> CreateAsync(CargoDetailViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.ProductNumber))
            {
                // data format from ediSON:
                // PurchaseOrders.PONumber~POLineItems.ProductCode~POLineItems.LineOrder~POLineItems.ScheduleLineNo
                //var productNumberParts = viewModel.ProductNumber.Split('~');

                // In case order type is null or Freight
                if (string.IsNullOrEmpty(viewModel.OrderType) || viewModel.OrderType.Equals(OrderType.Freight.ToString(), System.StringComparison.InvariantCultureIgnoreCase))
                {
                    //Notes: No need to fulfill OrderId as there is a trigger 'trg_CargoDetails' will take care on insert/update event

                    // Link poff allocated order to shipment
                    var poffAllocatedOrder = _poffAllocatedOrderRepository.QueryAsNoTracking(x => x.ProductNumber.Equals(viewModel.ProductNumber)).FirstOrDefault();
                    if (poffAllocatedOrder != null)
                    {
                        poffAllocatedOrder.ShipmentId = viewModel.ShipmentId;
                        await UnitOfWork.SaveChangesAsync();
                    }
                }
                else if (viewModel.OrderType.Equals(OrderType.Cruise.ToString(), System.StringComparison.InvariantCultureIgnoreCase))
                {
                    //Notes: No need to  fulfill shipment id to cruise order item as there is a trigger 'trg_CargoDetails' will take care on insert/update event
                    //Notes: No need to fulfill OrderId/ItemId as there is a trigger 'trg_CargoDetails' will take care on insert/update event
                }
            }

            return await base.CreateAsync(viewModel);
        }

        public override async Task<CargoDetailViewModel> UpdateAsync(CargoDetailViewModel viewModel, params object[] keys)
        {
            // Map cargo detail to poff allocated order
            if (!string.IsNullOrEmpty(viewModel.ProductNumber))
            {
                // data format from ediSON:
                // PurchaseOrders.PONumber~POLineItems.ProductCode~POLineItems.LineOrder~POLineItems.ScheduleLineNo
                var poffAllocatedOrder = await _poffAllocatedOrderRepository.GetAsync(x => x.ProductNumber.Equals(viewModel.ProductNumber));

                if (poffAllocatedOrder != null)
                {
                    poffAllocatedOrder.ShipmentId = viewModel.ShipmentId;
                    await UnitOfWork.SaveChangesAsync();
                }
            }

            return await base.UpdateAsync(viewModel, keys);
        }

        public async Task<IEnumerable<CargoDetailLoadViewModel>> GetUnloadCargoDetailsByConsolidationAsync(long consolidationId)
        {
            var storedProcedureName = "spu_GetUnloadCargoDetailList_ByConsolidationId";
            List<SqlParameter> filterParameter;

            filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@consolidationId",
                        Value = consolidationId,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, IEnumerable<CargoDetailLoadViewModel>> mapping = (reader) =>
            {
                // Default value
                var mappedData = new List<CargoDetailLoadViewModel>();

                while (reader.Read())
                {
                    // Must be in order of data reader
                    var id = reader[0];
                    var shipmentId = reader[1];
                    var orderId = reader[2];
                    var itemId = reader[3];
                    var consignmentId = reader[4];
                    var containerId = reader[5];
                    var shipmentLoadId = reader[6];
                    var shipmentNo = reader[7];
                    var poNumber = reader[8];
                    var productCode = reader[9];
                    var unitUOM = reader[10];
                    var packageUOM = reader[11];
                    var volumeUOM = reader[12];
                    var grossWeightUOM = reader[13];
                    var netWeightUOM = reader[14];
                    var unit = reader[15];
                    var package = reader[16];
                    var volume = reader[17];
                    var grossWeight = reader[18];
                    var netWeight = reader[19];

                    var newRow = new CargoDetailLoadViewModel()
                    {
                        Id = (long)id,
                        ShipmentId = (long)shipmentId,
                        ShipmentNo = shipmentNo.ToString(),
                        OrderId = (long)orderId,
                        PONumber = poNumber.ToString(),
                        ItemId = (long)itemId,
                        ProductCode = productCode.ToString(),
                        Unit = (decimal)unit,
                        UnitUOM = unitUOM?.ToString(),
                        Package = (decimal)package,
                        PackageUOM = packageUOM?.ToString(),
                        Volume = (decimal)volume,
                        VolumeUOM = volumeUOM?.ToString(),
                        GrossWeight = (decimal)grossWeight,
                        GrossWeightUOM = grossWeightUOM?.ToString(),
                        NetWeight = netWeight != DBNull.Value ? (decimal)netWeight : (decimal?)null,
                        NetWeightUOM = netWeightUOM?.ToString(),
                        ConsignmentId = consignmentId != DBNull.Value ? (long)consignmentId : (long?)null,
                        ContainerId = containerId != DBNull.Value ? (long)containerId : (long?)null,
                        ShipmentLoadId = (long)shipmentLoadId
                    };

                    mappedData.Add(newRow);
                }
                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }
    }
}
