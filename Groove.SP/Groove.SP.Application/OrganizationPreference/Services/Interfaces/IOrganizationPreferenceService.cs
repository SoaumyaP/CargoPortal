using Groove.SP.Application.Common;
using Groove.SP.Application.OrganizationPreference.ViewModels;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.OrganizationPreference.Services.Interfaces
{
    public interface IOrganizationPreferenceService : IServiceBase<OrganizationPreferenceModel, OrganizationPreferenceViewModel>
    {
        Task<OrganizationPreferenceViewModel> GetAsNoTrackingAsync(long organizationId, string productCode);
        Task<IEnumerable<OrganizationPreferenceViewModel>> GetAsNoTrackingAsync(long organizationId, IEnumerable<string> productCode);
        Task<IEnumerable<OrganizationPreferenceViewModel>> GetAsNoTrackingAsync(long organizationId);
        /// <summary>
        /// To insert (new) or update (existing) organization preference and <em>NOT</em> delete existing data
        /// </summary>
        /// <param name="viewModels">List of data needed to proceed</param>
        /// <param name="organizationId">Organization Id to be stored</param>
        /// <param name="userName">User name (email) to be stored</param>
        /// <returns></returns>
        Task InsertOrUpdateRangeAsync(IEnumerable<OrganizationPreferenceViewModel> viewModels, long organizationId, string userName);

    }
}
