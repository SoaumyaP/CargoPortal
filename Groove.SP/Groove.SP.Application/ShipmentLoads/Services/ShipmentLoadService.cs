using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.ShipmentLoads.Services.Interfaces;
using Groove.SP.Application.ShipmentLoads.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.ShipmentLoads.Services
{
    public class ShipmentLoadService : ServiceBase<ShipmentLoadModel, ShipmentLoadViewModel>, IShipmentLoadService
    {
        public ShipmentLoadService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        {
        }
        
        public async Task<ShipmentLoadViewModel> GetAsync(long id)
        {
            var model = await Repository.GetAsync(x => x.Id == id);
            return Mapper.Map<ShipmentLoadViewModel>(model);
        }
    }
}
