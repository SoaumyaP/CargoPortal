using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.BillOfLadingContact.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.BillOfLadingContact.Services.Interfaces
{
    public interface IBillOfLadingContactService : IServiceBase<BillOfLadingContactModel, BillOfLadingContactViewModel>
    {
        Task<IEnumerable<BillOfLadingContactViewModel>> GetBOLContactsByBOLAsync(long billOfLadingId);
    }
}
