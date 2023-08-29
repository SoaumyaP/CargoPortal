using Groove.CSFE.Core.Data;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.EventCodes.Services.Interfaces
{
    public interface IEventCodeListService
    {
        Task<DataSourceResult> SearchAsync(DataSourceRequest request);
    }
}