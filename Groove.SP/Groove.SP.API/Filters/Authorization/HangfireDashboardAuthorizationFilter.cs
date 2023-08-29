using Hangfire.Dashboard;
using Microsoft.AspNetCore.Hosting;

namespace Groove.SP.API.Filters.Authorization
{
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private IWebHostEnvironment _environment;

        public HangfireDashboardAuthorizationFilter(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}
