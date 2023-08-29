using System.Threading.Tasks;
using Groove.CSFE.Application.AlternativeLocations.Services.Interfaces;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Application.Locations.Services.Interfaces;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Application.AlternativeLocations.ViewModels;

namespace Groove.CSFE.Application.AlternativeLocations.Services
{
    public class AlternativeLocationService : ServiceBase<AlternativeLocationModel, AlternativeLocationViewModel>, IAlternativeLocationService
    {
        public AlternativeLocationService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        { }
    }
}
