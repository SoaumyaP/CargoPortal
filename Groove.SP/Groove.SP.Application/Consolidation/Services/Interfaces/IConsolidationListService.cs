using Groove.SP.Application.Common;
using Groove.SP.Application.Consolidation.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.Consolidation.Services.Interfaces
{
    public interface IConsolidationListService : IServiceBase<ConsolidationModel, ConsolidationListViewModel>
    {
        /// <summary>
        /// Return list of Consolidation by searched parameters in kendoGrid
        /// </summary>
        /// <param name="request"></param>
        /// <param name="isInternal"></param>
        /// <param name="affiliates"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, string affiliates = "", long? organizationId = 0);
    }
}