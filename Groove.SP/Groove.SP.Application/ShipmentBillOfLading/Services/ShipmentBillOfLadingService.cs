using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.ShipmentBillOfLading.Services.Interfaces;
using Groove.SP.Application.ShipmentBillOfLading.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.ShipmentBillOfLading.Services
{
    public class ShipmentBillOfLadingService : ServiceBase<ShipmentBillOfLadingModel, ShipmentBillOfLadingViewModel>, IShipmentBillOfLadingService
    {
        public ShipmentBillOfLadingService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        {
        }
    }
}
