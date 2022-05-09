using System;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Services;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverTimerAssignment;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverTimerRejection;

namespace Delivery.Driver.Domain.Services
{
    public class DriverAssignmentCronJobService : CronJobService
    {
        private readonly IServiceProvider serviceProvider;

        public DriverAssignmentCronJobService(IScheduleConfig<DriverRejectionCronJobService> config, IServiceProvider serviceProvider) : base(config.CronExpression,
            config.TimeZoneInfo)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var executingContextUs = GetExecutingContext("Raus");

            await new DriverTimerAssignmentCommandHandler(serviceProvider, executingContextUs)
                .HandleAsync(new DriverTimerAssignmentCommand(executingContextUs.GetShard().Key));
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
                        UserEmail = $"{nameof(DriverAssignmentCronJobService).ToLowerInvariant()}@ragibull.com"

                    }
                });

            return executingRequestContextAdapter;
        }
    }
}