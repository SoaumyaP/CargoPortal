using System.IO;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace Groove.CSFE.IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
              .Enrich.WithProperty("SP-Identity", "Web App Logging")
              .MinimumLevel.Warning()
              .WriteTo.File("Logs\\Log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
              .CreateLogger();

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                /*
                    Must use Out-of-process hosting model with Kestrel (configurate in Groove.CSFE.IdentityServer.csproj)
                    Because with In-process hosting model, xref:System.IO.Directory.GetCurrentDirectory returns the worker directory
                        of the process started by IIS rather than the app's directory (for example, C:\Windows\System32\inetsrv for w3wp.exe)
                    Reference: https://github.com/aspnet/Docs/blob/master/aspnetcore/host-and-deploy/aspnet-core-module.md#in-process-hosting-model

                    If want to use In-process, implement CurrentDirectoryHelpers and call SetCurrentDirectory before xref:System.IO.Directory.GetCurrentDirectory
                    Source: https://github.com/aspnet/Docs/blob/master/aspnetcore/host-and-deploy/aspnet-core-module/samples_snapshot/2.x/CurrentDirectoryHelpers.cs
                */
                .UseKestrel(options => options.AddServerHeader = false)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .UseSerilog();
    }
}
