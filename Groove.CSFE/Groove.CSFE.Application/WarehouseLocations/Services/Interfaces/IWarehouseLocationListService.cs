using Groove.CSFE.Core.Data;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.WarehouseLocations.Services.Interfaces
{
    public interface IWarehouseLocationListService
    {
        Task<DataSourceResult> GetListWarehouseLocationAsync(DataSourceRequest request);
    }
}
