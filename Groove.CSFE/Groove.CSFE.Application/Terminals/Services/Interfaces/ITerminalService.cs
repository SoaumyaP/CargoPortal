using GGroove.CSFE.Application;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Terminals.ViewModels;
using Groove.CSFE.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Terminals.Services.Interfaces
{
    public interface ITerminalService : IServiceBase<TerminalModel, TerminalViewModel>
    {
        /// <summary>
        /// Get terminal dropdown datasource.
        /// <br/> Label: TerminalName
        /// <br/> Value: TerminalCode
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        Task<IEnumerable<DropDownModel<string>>> GetDropdownAsync(string searchTerm = null);
    }
}
