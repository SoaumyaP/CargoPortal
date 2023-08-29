using Groove.SP.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Groove.SP.Application.Tests
{
    public class Startup
    {
        public string _appConnectionString { set; get; }
        public IConfigurationRoot Configuration { set; get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var dataConnections = new AppDbConnections
            {
                CsPortalDb = $"Data Source=.;Initial Catalog=ShipmentPortalUTDatabase;Integrated Security=true;MultipleActiveResultSets=true;",
                SecondaryCsPortalDb = $"Data Source=.;Initial Catalog=ShipmentPortalUTDatabase;Integrated Security=true;MultipleActiveResultSets=true;"
            };

            services.AddSingleton(dataConnections);
        }
    }
}
