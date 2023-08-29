using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.MasterBillOfLadingContact.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.MasterBillOfLadingContact.Services.Interfaces
{
    public interface IMasterBillOfLadingContactService : IServiceBase<MasterBillOfLadingContactModel, MasterBillOfLadingContactViewModel>
    {
        Task<IEnumerable<MasterBillOfLadingContactViewModel>> GetContactsByMasterBOLIdAsync(long masterBOLId);
    }
}
