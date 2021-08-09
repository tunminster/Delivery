using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverActive;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverActive
{
    public record DriverActiveCommand(DriverActiveCreationContract DriverActiveCreationContract);
    
    public class DriverActiveCommandHandler : ICommandHandler<DriverActiveCommand, DriverActiveStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverActiveCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverActiveStatusContract> Handle(DriverActiveCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var driver =
                databaseContext.Drivers.SingleOrDefault(x =>
                    x.EmailAddress == command.DriverActiveCreationContract.Username);

            if (driver == null)
            {
                throw new InvalidOperationException(
                    $"Expected to be found a driver. Instead found {driver.ConvertToJson()}");
            }

            driver.IsActive = command.DriverActiveCreationContract.IsActive;
            
            await databaseContext.SaveChangesAsync();

            return new DriverActiveStatusContract
                {IsActive = command.DriverActiveCreationContract.IsActive, DateCreated = DateTimeOffset.UtcNow};
        }
    }
}