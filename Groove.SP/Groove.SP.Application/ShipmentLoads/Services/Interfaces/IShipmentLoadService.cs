using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.ShipmentLoads.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.ShipmentLoads.Services.Interfaces
{
    public interface IShipmentLoadService : IServiceBase<ShipmentLoadModel, ShipmentLoadViewModel>
    {
        Task<ShipmentLoadViewModel> GetAsync(long id);
    }
}