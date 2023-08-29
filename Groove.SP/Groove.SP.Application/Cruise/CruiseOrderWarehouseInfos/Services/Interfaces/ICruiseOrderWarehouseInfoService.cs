using Groove.SP.Application.Common;
using Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.ViewModels;
using Groove.SP.Core.Entities.Cruise;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.Services.Interfaces
{
    public interface ICruiseOrderWarehouseInfoService : IServiceBase<CruiseOrderWarehouseInfoModel, CruiseOrderWarehouseInfoViewModel>
    {
        Task<CruiseOrderWarehouseInfoViewModel> GetByCruiseOrderItemIdAsync(long cruiseOrderItemId);
    }
}
