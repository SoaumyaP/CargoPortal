using System.Threading.Tasks;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Application.AlternativeLocations.ViewModels;

namespace Groove.CSFE.Application.AlternativeLocations.Services.Interfaces
{
    public interface IAlternativeLocationService : IServiceBase<AlternativeLocationModel, AlternativeLocationViewModel>
    {
    }
}
