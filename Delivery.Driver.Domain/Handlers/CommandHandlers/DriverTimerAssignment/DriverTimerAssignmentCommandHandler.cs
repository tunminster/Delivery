using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverTimerAssignment
{
    public record DriverTimerAssignmentCommand(string ShardKey);

    public class DriverTimerAssignmentCommandHandler : ICommandHandler<DriverTimerAssignmentCommand, StatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;

        public DriverTimerAssignmentCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<StatusContract> Handle(DriverTimerAssignmentCommand command)
        {
            await using var databaseContext =
                await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var driverResponseThreshold = serviceProvider.GetRequiredService<IConfigurationProvider>()
                .GetSettingOrDefault<string>("DriverAssignmentThreshold", "3");

            throw new System.NotImplementedException();
        }
    }
}