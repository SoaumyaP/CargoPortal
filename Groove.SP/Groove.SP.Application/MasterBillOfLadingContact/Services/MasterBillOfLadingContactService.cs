using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.MasterBillOfLadingContact.Services.Interfaces;
using Groove.SP.Application.MasterBillOfLadingContact.ViewModels;
using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Groove.SP.Application.MasterBillOfLadingContact.Services
{
    public class MasterBillOfLadingContactService : ServiceBase<MasterBillOfLadingContactModel, MasterBillOfLadingContactViewModel>, IMasterBillOfLadingContactService
    {
        public MasterBillOfLadingContactService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        { }

        public async Task<IEnumerable<MasterBillOfLadingContactViewModel>> GetContactsByMasterBOLIdAsync(long masterBOLId)
        {
            var result = await Repository.Query(s => s.MasterBillOfLadingId == masterBOLId).ToListAsync();

            return Mapper.Map<IEnumerable<MasterBillOfLadingContactViewModel>>(result);
        }
    }
}
