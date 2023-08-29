using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.Users.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Hangfire;

namespace Groove.SP.Application.Users.Services.Interfaces
{
    public interface IUserProfileService: IServiceBase<UserProfileModel, UserProfileViewModel>
    {
        /// <summary>
        /// To get user profile by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<UserProfileViewModel> GetAsync(long id);

        /// <summary>
        /// To get user profile by user name (email address)
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<UserProfileViewModel> GetAsync(string userName);

        Task<IEnumerable<DropDownListItem<long>>> GetSelectionAsync(string searchEmail = "");

        /// <summary>
        /// To get current user information. It will work on user role switch mode and normal mode
        /// </summary>
        /// <param name="curentUser"></param>
        /// <returns></returns>
        Task<UserProfileViewModel> GetAsync(IdentityInfo curentUser);

        Task<IEnumerable<UserProfileViewModel>> GetUsersByOrganizationIdAsync(long id);

        Task<UserProfileViewModel> GetByUsernameAsync(string username);

        Task<UserProfileViewModel> CreateUserAsync(UserProfileViewModel model, bool isInternal);

        Task<UserProfileViewModel> CreateExternalUserAsync(UserProfileViewModel model);

        Task<bool> UpdateStatusUsersAsync(long organizationId, UserStatus status, string username);

        Task<bool> CheckExistsUser(string email);

        Task<UserProfileViewModel> UpdateCurrentUserAsync(UserProfileViewModel viewModel, string username);

        Task<UserProfileViewModel> UpdateAsync(long id, UserProfileViewModel viewModel, string username);

        Task<UserProfileViewModel> UpdateUserStatusAsync(long id, UpdateUserStatusViewModel viewModel, string username);

        Task<bool> UpdateOrganizationAsync(long organizationId, string organizationName, OrganizationType organizationType, string username);

        Task UpdateLastSignInDateAsync(string username);

        Task SyncUserTracking(string username, IEnumerable<UserAuditLogViewModel> viewModels);

        Task<DataSourceResult> TraceUserByEmailSearchingAsync(DataSourceRequest request, string email);

        Task<long> TraceUserByEmailTotalCountAsync(string email);

        Task ActivateUserAsync(string username);

        /// <summary>
        /// To remove <em><b>external</b></em> user profile completely from the system and Azure B2C users.
        /// </summary>
        /// <param name="id">User profile id</param>
        /// <returns></returns>
        Task RemoveExternalUserCompletelyAsync(long id, IdentityInfo currentUser);

        /// <summary>
        /// Job on validate importing user via Excel
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
        /// <param name="userName"></param>
        /// <param name="importDataProgressId"></param>
        /// <returns></returns>
        [JobDisplayName("Validate Users from {1} by {2} on progress {3}")]
        Task ValidateExcelImportAsync(byte[] file, string fileName, string userName, long importDataProgressId);

        /// <summary>
        /// Import users via Excel, an importing result will be mailed to the user when the progress has been completed.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
        /// <param name="userName"></param>
        /// <param name="name"></param>
        /// <param name="importDataProgressId"></param>
        /// <returns></returns>
        [JobDisplayName("Import Users from {1} by {2} on progress {5}")]
        Task ImportExcelSilentAsync(byte[] file, string fileName, string userName, string email, string name, long importDataProgressId);

        Task SendActivationEmail(long id);
    }
}
