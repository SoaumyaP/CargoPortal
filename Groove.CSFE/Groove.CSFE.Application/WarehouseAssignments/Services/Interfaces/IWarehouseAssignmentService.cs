using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.WarehouseAssignments.ViewModels;
using Groove.CSFE.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.WarehouseAssignments.Services.Interfaces
{
    public interface IWarehouseAssignmentService : IServiceBase<WarehouseAssignmentModel, WarehouseAssignmentViewModel>
    {
        /// <summary>
        /// To get list of warehouse assignments by organization id.
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<IEnumerable<WarehouseAssignmentViewModel>> GetByOrgIdAsync(long organizationId);
    }
}
