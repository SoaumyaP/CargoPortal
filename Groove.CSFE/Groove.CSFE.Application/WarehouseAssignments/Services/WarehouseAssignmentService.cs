using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Application.WarehouseAssignments.Services.Interfaces;
using Groove.CSFE.Application.WarehouseAssignments.ViewModels;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.WarehouseAssignments.Services
{
    public class WarehouseAssignmentService : ServiceBase<WarehouseAssignmentModel, WarehouseAssignmentViewModel>, IWarehouseAssignmentService
    {
        public WarehouseAssignmentService(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider)
        {

        }

        public async Task<IEnumerable<WarehouseAssignmentViewModel>> GetByOrgIdAsync(long organizationId)
        {
            var models = await Repository.QueryAsNoTracking(x => x.OrganizationId == organizationId, y => y.OrderBy(z => z.CreatedDate), y => y.Include(z => z.WarehouseLocation).ThenInclude(z => z.Location).ThenInclude(z => z.Country)).ToListAsync();

            return Mapper.Map<IEnumerable<WarehouseAssignmentViewModel>>(models);
        }
    }
}
