using System.Threading.Tasks;

namespace Groove.CSFE.IdentityServer.Services.Interfaces
{
    public interface IAccountService
    {
        /// <summary>
        /// To check if current user is admin
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        Task<bool> CheckIsInRoleByEmailAsync(string userName, params int[] roles);
    }
}
