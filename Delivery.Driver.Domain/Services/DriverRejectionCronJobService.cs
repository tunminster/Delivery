using System;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Services;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverTimerRejection;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Services
{
    public class DriverRejectionCronJobService : CronJobService
    {
        private readonly IServiceProvider serviceProvider;
        public DriverRejectionCronJobService(IScheduleConfig<DriverRejectionCronJobService> config, IServiceProvider serviceProvider) : base(config.CronExpression, config.TimeZoneInfo)
        {
            this.serviceProvider = serviceProvider;
        }
        
        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var executingContextUs = GetExecutingContext("Raus");
            
            await new DriverTimerRejectionCommandHandler(serviceProvider, executingContextUs)
                .HandleAsync(new DriverTimerRejectionCommand(executingContextUs.GetShard().Key));
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
                        UserEmail = $"{nameof(DriverRejectionCronJobService).ToLowerInvariant()}@ragibull.com"
                    }
                });

            return executingRequestContextAdapter;
        }
    }
}