using Groove.SP.Application.Common;
using Groove.SP.Application.POFulfillmentShortshipOrder.ViewModels;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillmentShortshipOrder.Services.Interfaces
{
    public interface IPOFulfillmentShortshipOrderService : IServiceBase<POFulfillmentShortshipOrderModel, POFulfillmentShortshipOrderViewModel>
    {
        Task ReadOrUnreadAsync(long id, POFulfillmentShortshipOrderViewModel viewModel);
        Task<int> CountUnreadAsync(bool isInternal, string affiliates,long? orgId);
    }
}
