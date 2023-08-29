using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.PurchaseOrderContact.Services.Interfaces;
using Groove.SP.Application.PurchaseOrderContact.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Infrastructure.CSFE;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.PurchaseOrderContact.Services
{
    public class PurchaseOrderContactService : ServiceBase<PurchaseOrderContactModel, PurchaseOrderContactViewModel>, IPurchaseOrderContactService
    {
        public PurchaseOrderContactService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        {
        }
    }
}
