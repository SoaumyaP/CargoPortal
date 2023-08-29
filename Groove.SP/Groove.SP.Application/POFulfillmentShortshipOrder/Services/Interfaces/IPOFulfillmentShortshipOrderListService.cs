using Groove.SP.Core.Data;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillmentShortshipOrder.Services.Interfaces
{
    public interface IPOFulfillmentShortshipOrderListService
    {
        Task<DataSourceResult> SearchListAsync(DataSourceRequest request, bool isInternal, string affiliates = "", long? organizationId = 0, string userRole = "");
    }
}