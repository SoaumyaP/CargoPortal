using Groove.SP.Application.BillOfLadingConsignment.Services.Interfaces;
using Groove.SP.Application.BillOfLadingConsignment.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.BillOfLadingConsignment.Services
{
    public class BillOfLadingConsignmentService : ServiceBase<BillOfLadingConsignmentModel, BillOfLadingConsignmentViewModel>, IBillOfLadingConsignmentService
    {
        public BillOfLadingConsignmentService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        { }
    }
}
