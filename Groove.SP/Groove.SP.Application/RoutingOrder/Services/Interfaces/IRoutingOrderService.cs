using Groove.SP.Application.Common;
using Groove.SP.Application.RoutingOrder.ViewModels;
using Groove.SP.Core.Entities;
using System.IO;
using System.Threading.Tasks;

namespace Groove.SP.Application.RoutingOrder.Services.Interfaces
{
    public interface IRoutingOrderService : IServiceBase<RoutingOrderModel, RoutingOrderViewModel>
    {
        Task<RoutingOrderViewModel> GetByIdAsync(long routingOrderId, bool isInternal, string affiliates);
        Task<ImportRoutingOrderResultViewModel> ImportXMLAsync(byte[] content, string fileName, string username);
    }
}