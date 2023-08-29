using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Vessels.ViewModels;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Data;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Vessels.Services.Interfaces
{
    public interface IVesselService : IServiceBase<VesselModel, VesselViewModel>
    {
        Task<IEnumerable<DropDownListItem<string>>> SearchRealActiveByNameAsync(string name);
        Task<IEnumerable<VesselViewModel>> GetRealActiveListAsync();
        Task<IEnumerable<VesselViewModel>> GetActiveListAsync();
        Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, string affiliates = "", long? organizationId = 0);
        Task<VesselViewModel> UpdateStatusAsync(long id, VesselStatus status, string userName);
    }
}
