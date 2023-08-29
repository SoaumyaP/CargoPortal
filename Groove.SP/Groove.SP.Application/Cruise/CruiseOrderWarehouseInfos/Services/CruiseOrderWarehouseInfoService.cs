using System.Linq;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.Services.Interfaces;
using Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.ViewModels;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Entities.Cruise;

namespace Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.Services
{
    public class CruiseOrderWarehouseInfoService : ServiceBase<CruiseOrderWarehouseInfoModel, CruiseOrderWarehouseInfoViewModel>, ICruiseOrderWarehouseInfoService
    {
        private readonly IRepository<CruiseOrderWarehouseInfoModel> _warehouseRepository;
        private readonly IRepository<CruiseOrderItemModel> _cruiseOrderItemRepository;


        public CruiseOrderWarehouseInfoService(IUnitOfWorkProvider unitOfWorkProvider, IRepository<CruiseOrderWarehouseInfoModel> warehouseRepository)
            : base(unitOfWorkProvider)
        {
            _warehouseRepository = warehouseRepository;
            _cruiseOrderItemRepository = UnitOfWork.GetRepository<CruiseOrderItemModel>();
        }

        public async Task<CruiseOrderWarehouseInfoViewModel> GetByCruiseOrderItemIdAsync(long cruiseOrderItemId)
        {
            var warehouseModel = await _warehouseRepository.GetAsNoTrackingAsync(c => c.CruiseOrderItemId == cruiseOrderItemId);
            var warehouseViewModel = Mapper.Map<CruiseOrderWarehouseInfoModel, CruiseOrderWarehouseInfoViewModel>(warehouseModel);

            return warehouseViewModel; 
        }

        public override Task<CruiseOrderWarehouseInfoViewModel> CreateAsync(CruiseOrderWarehouseInfoViewModel viewModel)
        {
            // Link warehouse to cruise order item by cruise order id and po line
            var cruiseOrderItem = _cruiseOrderItemRepository.QueryAsNoTracking(filter: x => x.OrderId == viewModel.CruiseOrderId && x.POLine == viewModel.POLine).FirstOrDefault();
            if(cruiseOrderItem == null)
            {
                throw new AppException("Can not import as cruise order item not found!");
            }

            viewModel.CruiseOrderItemId = cruiseOrderItem.Id;
            viewModel.FieldStatus[nameof(CruiseOrderWarehouseInfoModel.CruiseOrderItemId)] = Core.Models.FieldDeserializationStatus.HasValue;

            return base.CreateAsync(viewModel);
        }
    }
}
