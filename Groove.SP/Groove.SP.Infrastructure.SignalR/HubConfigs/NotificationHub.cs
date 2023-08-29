using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Groove.SP.Infrastructure.SignalR.HubConfigs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public NotificationHub()
        {
        }

		public override async Task OnConnectedAsync()
		{
            if (long.TryParse(Context.User.FindFirstValue("org_id"), out long organizationId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"ORG{organizationId}");
            }

            await base.OnConnectedAsync();
		}
	}
}