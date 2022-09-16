using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Converters;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverCreation
{
    public record DriverCreationCommand(DriverCreationContract DriverCreationContract, DriverCreationStatusContract DriverCreationStatusContract);
    
    public class DriverCreationCommandHandler : ICommandHandler<DriverCreationCommand, DriverCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverCreationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        public async Task<DriverCreationStatusContract> HandleAsync(DriverCreationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var driver = DriverContractConverter.ConvertToEntity(command.DriverCreationContract,
                command.DriverCreationStatusContract);

            await databaseContext.Drivers.AddAsync(driver);

            await databaseContext.SaveChangesAsync();

            return command.DriverCreationStatusContract;
        }
    }
}