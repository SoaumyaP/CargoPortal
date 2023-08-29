using Groove.SP.Application.BillOfLading.Services.Interfaces;
using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.BillOfLading.Services
{
    public class BillOfLadingItineraryService : ServiceBase<BillOfLadingItineraryModel, BillOfLadingItineraryViewModel>, IBillOfLadingItineraryService
    {
        public BillOfLadingItineraryService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        {
        }
    }
}
