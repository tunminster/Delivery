﻿using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Initialization;
using Delivery.Azure.Library.Microservices.Hosting.Enums;
using Delivery.Azure.Library.Microservices.Hosting.Extensions;
using Delivery.Azure.Library.Microservices.Hosting.Hosts;
using Delivery.Azure.Library.Microservices.Hosting.Logging;
using Delivery.Drivers.Host.ContainerHosts;
using Delivery.Drivers.Host.Kernel;
using Microsoft.Extensions.Hosting;

namespace Delivery.Drivers.Host
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await ((ContainerHost) GetHostBuilder(args).Properties[nameof(ContainerHost)]).RunAsync();
        }
        
        public static IHostBuilder GetHostBuilder(string[] args)
        {
            var hostBuilder = Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder(args)
                .ConfigurePlatformEnvironment(args)
                .ConfigurePlatformLogging()
                .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                {
                    configurationBuilder
                        .UseSimulationBasePath(hostBuilderContext)
                        .UsePlatformConfiguration(hostBuilderContext.HostingEnvironment)
                        .Build(hostBuilderContext);
                })
                .ConfigureServices((hostBuilderContext, serviceCollection) =>
                {
                    serviceCollection
                        .AddApplicationServices(hostBuilderContext.Configuration);

                })
                .ConfigurePlatformHosting(HostTypes.MessagingHost,
                    builder => new DriversCompetingConsumerContainerHost(builder));

            return hostBuilder;
        }
    }
}

