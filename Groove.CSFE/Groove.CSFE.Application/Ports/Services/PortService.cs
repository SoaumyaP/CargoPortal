using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Application.Ports.ViewModels;
using Groove.CSFE.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Ports.Services
{
    public class PortService : ServiceBase<PortModel, PortViewModel>, IPortService
    {
        public PortService(IUnitOfWorkProvider unitOfWorkProvider)
           : base(unitOfWorkProvider)
        { }

        public IEnumerable<PortViewModel> GetAllPorts(string code)
        {
            var model = this.Repository.GetListQueryable(null);

            if(!string.IsNullOrEmpty(code))
            {
                model = model.Where(p => p.AirportCode.Equals(code) || p.SeaportCode.Equals(code));
            }

            return Mapper.Map<IEnumerable<PortViewModel>>(model);
        }
    }
}
