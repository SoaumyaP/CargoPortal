using GGroove.CSFE.Application;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Warehouses.ViewModels;
using Groove.CSFE.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Warehouses.Services.Interfaces
{
    public interface IWarehouseService : IServiceBase<WarehouseModel, WarehouseViewModel>
    {
        /// <summary>
        /// Get warehouse dropdown datasource.
        /// <br/> Label: WarehouseName
        /// <br/> Value: WarehouseCode
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        Task<IEnumerable<DropDownModel<string>>> GetDropdownAsync(string searchTerm = null);
    }
}
