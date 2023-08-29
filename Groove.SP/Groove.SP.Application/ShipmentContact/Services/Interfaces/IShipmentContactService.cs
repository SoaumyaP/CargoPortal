using Groove.SP.Application.Common;
using Groove.SP.Application.ShipmentContact.ViewModels;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.ShipmentContact.Services.Interfaces
{
    public interface IShipmentContactService : IServiceBase<ShipmentContactModel, ShipmentContactViewModel>
    {
        Task<IEnumerable<ShipmentContactViewModel>> GetShipmentContactsByShipmentAsync(long shipmentId);
    }
}
