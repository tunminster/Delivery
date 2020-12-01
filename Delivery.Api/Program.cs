using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Api.Models;
using Delivery.Azure.Library.Microservices.Hosting.Extensions;
using Delivery.Azure.Library.Microservices.Hosting.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Delivery.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("secrets/appsettings.secret.json", optional: true)
                .Build();
            // AzureLogConfig azureLogConfig = new AzureLogConfig();
            // config.GetSection("AzureLogConfig").Bind(azureLogConfig);
            //
            // Log.Logger = new LoggerConfiguration()
            //     .WriteTo.AzureAnalytics(workspaceId: azureLogConfig.WorkspaceId,
            //                             authenticationId: azureLogConfig.PrimaryKey)
            //     .CreateLogger();


            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
           var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigurePlatformEnvironment(args)
                .ConfigurePlatformLogging()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureAppConfiguration((HostingAbstractionsHostExtensions, config) =>
                {
                    config.AddJsonFile("secrets/appsettings.secret.json", true, true);
                });

               

           return hostBuilder;
        }
    }
}
