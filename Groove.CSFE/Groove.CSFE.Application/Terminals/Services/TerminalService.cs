using GGroove.CSFE.Application;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Application.Terminals.Services.Interfaces;
using Groove.CSFE.Application.Terminals.ViewModels;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Terminals.Services
{
    public class TerminalService : ServiceBase<TerminalModel, TerminalViewModel>, ITerminalService
    {
        public TerminalService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        {
        }

        public async Task<IEnumerable<DropDownModel<string>>> GetDropdownAsync(string searchTerm = null)
        {
            var query = Repository.QueryAsNoTracking(orderBy: x => x.OrderBy(y => y.TerminalName));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x => x.TerminalName.Contains(searchTerm));
            }

            var terminals = await query.ToListAsync();

            if(!terminals.Any())
            {
                return Enumerable.Empty<DropDownModel<string>>();
            }

            return terminals.Select(x => new DropDownModel<string>
            {
                Label = x.TerminalName,
                Value = x.TerminalCode
            });
        }
    }
}