using Groove.SP.Application.MobileApplication.ViewModels;
using System.Threading.Tasks;

namespace Groove.SP.Application.MobileApplication.Services.Interfaces
{
    public interface IMobileApplicationService
    {
        /// <summary>
        /// To check for mobile application update from provided version
        /// </summary>
        /// <param name="currentVersion">Current mobile application version</param>
        /// <returns></returns>
        public Task<UpdateCheckerMobileModel> CheckForUpdateAsync(string currentVersion);

        /// <summary>
        /// To check for any update for today. Applied for web GUI Dashboard
        /// </summary>
        /// <returns></returns>
        public Task<MobileTodayUpdateViewModel> CheckForTodayUpdatesAsync();
    }
}
