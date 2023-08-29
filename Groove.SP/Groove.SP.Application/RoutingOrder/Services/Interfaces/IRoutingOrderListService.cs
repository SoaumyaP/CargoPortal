using Groove.SP.Core.Data;
using System.Threading.Tasks;

namespace Groove.SP.Application.RoutingOrder.Services.Interfaces
{
    public interface IRoutingOrderListService
    {
        Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, string affiliates, string customerRelationships, long? organizationId);
    }
}
