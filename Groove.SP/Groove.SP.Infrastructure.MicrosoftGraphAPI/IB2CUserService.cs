using System.Threading.Tasks;

namespace Groove.SP.Infrastructure.MicrosoftGraphAPI
{
    public interface IB2CUserService
    {
        Task<dynamic> GetUserBySignInEmailAsync(string userEmailAddress);
        Task CreateUserAsync(
            string displayName,
            string email,
            string title,
            string department,
            string companyName);

        /// <summary>
        /// To remove user from Azure B2C
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns></returns>
        Task RemoveUserAsync(string email);
    }
}
