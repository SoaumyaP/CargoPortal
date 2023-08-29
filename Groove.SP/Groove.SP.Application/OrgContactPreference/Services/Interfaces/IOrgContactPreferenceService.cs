using Groove.SP.Application.Common;
using Groove.SP.Application.OrgContactPreference.ViewModels;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.OrgContactPreference.Services.Interfaces
{
    public interface IOrgContactPreferenceService : IServiceBase<OrgContactPreferenceModel, OrgContactPreferenceViewModel>
    {
        Task<IEnumerable<OrgContactPreferenceViewModel>> GetAsNoTrackingAsync(long organizationId);
        Task InsertOrUpdateRangeAsync(IEnumerable<OrgContactPreferenceViewModel> viewModels, long organizationId, string userName);
    }
}
