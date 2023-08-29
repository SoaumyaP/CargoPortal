using IdentityModel;
using Microsoft.AspNetCore.SignalR;

namespace Groove.SP.Infrastructure.SignalR.HubConfigs
{
    public class UserNameBasedUserIdProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(JwtClaimTypes.PreferredUserName)?.Value!;
        }
    }
}