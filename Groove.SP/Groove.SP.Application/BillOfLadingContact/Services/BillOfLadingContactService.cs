using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.BillOfLadingContact.Services.Interfaces;
using Groove.SP.Application.BillOfLadingContact.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Groove.SP.Application.BillOfLadingContact.Services
{
    public class BillOfLadingContactService : ServiceBase<BillOfLadingContactModel, BillOfLadingContactViewModel>, IBillOfLadingContactService
    {
        public BillOfLadingContactService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        { }

        public async Task<IEnumerable<BillOfLadingContactViewModel>> GetBOLContactsByBOLAsync(long billOfLadingId)
        {
            var result = await Repository.Query(s => s.BillOfLadingId == billOfLadingId).ToListAsync();

            return Mapper.Map<IEnumerable<BillOfLadingContactViewModel>>(result);
        }
    }
}
