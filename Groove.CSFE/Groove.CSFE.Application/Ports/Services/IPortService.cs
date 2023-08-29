using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Ports.ViewModels;
using Groove.CSFE.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Ports.Services
{
    public interface IPortService : IServiceBase<PortModel, PortViewModel>
    {
        IEnumerable<PortViewModel> GetAllPorts(string code);
    }
}
