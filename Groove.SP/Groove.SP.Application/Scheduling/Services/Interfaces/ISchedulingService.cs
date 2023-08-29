using Groove.SP.Application.Common;
using Groove.SP.Application.Scheduling.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System.Threading.Tasks;

namespace Groove.SP.Application.Scheduling.Services.Interfaces
{
    public interface ISchedulingService : IServiceBase<SchedulingModel, SchedulingViewModel>
    {
        /// <summary>
        /// To create new scheduling
        /// <br/>
        /// There are two steps:
        /// <list type="number">
        /// <item>Send request to Telerik to create new scheduled task</item>
        /// <item>Add new data on CS Port which links to the scheduled task</item>
        /// </list>
        /// </summary>
        /// <param name="data">Data to create new scheduling</param>
        /// <param name="currentUser">Current user information</param>
        /// <returns></returns>
        Task<SchedulingViewModel> CreateSchedulingAsync(SchedulingViewModel data, IdentityInfo currentUser);

        Task UpdateSchedulingAsync(SchedulingViewModel data, bool isInternal, long organizationId, string userName);

        Task<SchedulingViewModel> GetSchedulingAsync(long schedulingId, bool isInternal, long organizationId);

        Task UpdateSchedulingStatusAsync(long schedulingId, SchedulingStatus newStatus, bool isInternal, long organizationId, string userName);

        Task DeleteSchedulingAsync(long schedulingId, bool isInternal, long organizationId);

        Task UploadFtpAsync(string telerikSchedulingId, string telerikDocumentId);
    }
}
