using System.Threading.Tasks;
using Groove.SP.Application.BuyerCompliance.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;


namespace Groove.SP.Application.BuyerCompliance.Services.Interfaces
{
    public interface IBuyerComplianceListService : IServiceBase<BuyerComplianceModel, BuyerComplianceListViewModel>
    {
        Task<DataSourceResult> ListAsync(DataSourceRequest request);
    }
}
