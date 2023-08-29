using GGroove.CSFE.Application;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Application.Warehouses.Services.Interfaces;
using Groove.CSFE.Application.Warehouses.ViewModels;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Warehouses.Services
{
    public class WarehouseService : ServiceBase<WarehouseModel, WarehouseViewModel>, IWarehouseService
    {
        public WarehouseService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        {
        }

        public async Task<IEnumerable<DropDownModel<string>>> GetDropdownAsync(string searchTerm = null)
        {
            var query = Repository.QueryAsNoTracking(orderBy: x => x.OrderBy(y => y.WarehouseName));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x => x.WarehouseName.Contains(searchTerm));
            }

            var terminals = await query.ToListAsync();

            if (!terminals.Any())
            {
                return Enumerable.Empty<DropDownModel<string>>();
            }

            return terminals.Select(x => new DropDownModel<string>
            {
                Label = x.WarehouseName,
                Value = x.WarehouseCode
            });
        }
    }
}