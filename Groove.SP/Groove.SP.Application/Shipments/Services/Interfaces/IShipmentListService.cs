using Groove.SP.Core.Data;
using System.Threading.Tasks;


namespace Groove.SP.Application.Shipments.Services.Interfaces
{
    public interface IShipmentListService
    {
        Task<DataSourceResult> GetListByFreightSchedulerAsync(DataSourceRequest request, long freightSchedulerId);

        Task<DataSourceResult> GetListShipmentAsync(DataSourceRequest request, bool isInternal, string affiliates = "", string referenceNo = "", string statisticKey = "", string statisticFilter = "");
    }
}