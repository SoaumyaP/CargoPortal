using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Organizations.ViewModels;
using Groove.CSFE.Application.WarehouseLocations.ViewModels;
using Groove.CSFE.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.WarehouseLocations.Services.Interfaces
{
    public interface IWarehouseLocationService : IServiceBase<WarehouseLocationModel, WarehouseLocationViewModel>
    {
        Task<IEnumerable<WarehouseLocationViewModel>> GetByOrganizationAsync(long organizationId);
        Task<IEnumerable<WarehouseLocationDropDownViewModel>> GetDropdownAsync(string searchTerm = null);
        Task<IEnumerable<OrganizationReferenceDataViewModel>> GetCustomersByWarehouseLocationIdAsync(long warehouseLocationId);
        Task<WarehouseLocationViewModel> CreateWarehouseLocationAsync(WarehouseLocationViewModel viewModel, string userName);
        Task<WarehouseLocationViewModel> UpdateWarehouseLocationAsync(WarehouseLocationViewModel viewModel, string userName);
    }
}
