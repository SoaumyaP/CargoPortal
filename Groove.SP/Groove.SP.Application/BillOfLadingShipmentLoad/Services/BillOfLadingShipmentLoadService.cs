using Groove.SP.Application.BillOfLadingShipmentLoad.Services.Interfaces;
using Groove.SP.Application.BillOfLadingShipmentLoad.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.BillOfLadingShipmentLoad.Services
{
    public class BillOfLadingShipmentLoadService : ServiceBase<BillOfLadingShipmentLoadModel, BillOfLadingShipmentLoadViewModel>, IBillOfLadingShipmentLoadService
    {
        public BillOfLadingShipmentLoadService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        {
        }
    }
}
