using Groove.SP.Application.Common;
using Groove.SP.Application.POFulfillmentCargoReceive.ViewModels;
using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillmentCargoReceive.Services.Interfaces
{
    public interface IPOFulfillmentCargoReceiveService : IServiceBase<POFulfillmentCargoReceiveModel, POFulfillmentCargoReceiveViewModel>
    {
        Task<POFulfillmentCargoReceiveViewModel> FirstByPOFulfillmentIdAsync(long poffId);
    }
}