using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Api.Models;
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
            // var config = new ConfigurationBuilder()
            //     .AddJsonFile("secrets/appsettings.secret.json", optional: true)
            //     .Build();
            // AzureLogConfig azureLogConfig = new AzureLogConfig(); 
            // config.GetSection("AzureLogConfig").Bind(azureLogConfig);   

            // Log.Logger = new LoggerConfiguration()
            //     .WriteTo.AzureAnalytics(workspaceId: azureLogConfig.WorkspaceId, 
            //                             authenticationId: azureLogConfig.PrimaryKey)
            //     .CreateLogger();

            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((HostingAbstractionsHostExtensions, config) =>
                {
                    config.AddJsonFile("secrets/appsettings.secret.json", true, true);
                })
                
                .ConfigureLogging(logging =>
                {
                    // clear default logging providers
                    logging.ClearProviders();

                    // add built-in providers manually, as needed 
                    logging.AddConsole();
                    logging.AddDebug();
                    //logging.AddEventLog();
                    //logging.AddEventSourceLogger();
                    
                    //logging.AddTraceSource(sourceSwitchName);
                });
                //.UseSerilog();
    }
}
