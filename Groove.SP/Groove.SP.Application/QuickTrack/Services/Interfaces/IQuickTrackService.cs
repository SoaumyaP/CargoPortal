using Groove.SP.Application.QuickTrack.ViewModels;
using System.Threading.Tasks;

namespace Groove.SP.Application.QuickTrack.Services.Interfaces
{
    public interface IQuickTrackService
    {
        /// <summary>
        /// Based on the search term to determine the quick track option which the user is searching for.
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="isInternal"></param>
        /// <param name="organizationId"></param>
        /// <param name="affiliates"></param>
        /// <param name="supplierCustomerRelationships"></param>
        /// <returns></returns>
        Task<QuickTrackOptionResult> GetQuickTrackOptionAsync(
            string searchTerm,
            bool isInternal,
            long organizationId = 0,
            string affiliates = "",
            string supplierCustomerRelationships = "");
    }
}