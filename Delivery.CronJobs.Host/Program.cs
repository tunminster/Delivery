// See https://aka.ms/new-console-template for more information

using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Initialization;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Microservices.Hosting.Extensions;
using Delivery.Azure.Library.Microservices.Hosting.Logging;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.CronJobs.Host.Kernel;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverTimerRejection;
using Microsoft.Extensions.Hosting;

namespace Delivery.CronJobs.Host
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = GetHostBuilder(args).Build();
            await  hostBuilder.RunAsync();
            
            await new DriverTimerRejectionCommandHandler(hostBuilder.Services, GetExecutingContext("Raus"))
                .Handle(new DriverTimerRejectionCommand(GetExecutingContext("Raus").GetShard().Key));
            
            await new DriverTimerRejectionCommandHandler(hostBuilder.Services, GetExecutingContext("Rauk"))
                .Handle(new DriverTimerRejectionCommand(GetExecutingContext("Rauk").GetShard().Key));

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

                });
                

            return hostBuilder;
        }
    }
}
