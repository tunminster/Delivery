// See https://aka.ms/new-console-template for more information

using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Initialization;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Microservices.Hosting.Enums;
using Delivery.Azure.Library.Microservices.Hosting.Extensions;
using Delivery.Azure.Library.Microservices.Hosting.Hosts;
using Delivery.Azure.Library.Microservices.Hosting.Logging;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.CronJobs.Host.ContainerHosts;
using Delivery.CronJobs.Host.Kernel;
using Microsoft.Extensions.Hosting;
using ServiceStack;

namespace Delivery.CronJobs.Host
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await ((ContainerHost) GetHostBuilder(args).Properties[nameof(ContainerHost)]).RunAsync();
            //await Task.FromResult(GetHostBuilder(args));
        }

        private static IExecutingRequestContextAdapter GetExecutingContext(string shardKey)
        {
            IExecutingRequestContextAdapter executingRequestContextAdapter =
                new ExecutingRequestContextAdapter(new ExecutingRequestContext
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    Ring = 0,
                    ShardKey = shardKey,
                    AuthenticatedUser = new AuthenticatedUserContract
                    {
                        Role = "System",
                        ShardKey = shardKey,
                        UserEmail = "system-admin@ragibull.com"

                    }
                });

            return executingRequestContextAdapter;
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
                    builder => new CronJobCompetingConsumerContainerHost(builder));;
                

            return hostBuilder;
        }
    }
}
