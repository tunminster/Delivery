using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverProfile
{
    public record DriverServiceAreaUpdateCommand(DriverServiceAreaContract DriverServiceAreaContract);
    
    public class DriverServiceAreaUpdateCommandHandler : ICommandHandler<DriverServiceAreaUpdateCommand>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverServiceAreaUpdateCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task Handle(DriverServiceAreaUpdateCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var driver = databaseContext.Drivers.FirstOrDefault(x =>
                x.EmailAddress == executingRequestContextAdapter.GetAuthenticatedUser().UserEmail) ?? throw new InvalidOperationException($"Expected a driver by user email: {executingRequestContextAdapter.GetAuthenticatedUser().UserEmail}");
            
            driver.ServiceArea = command.DriverServiceAreaContract.ServiceArea;
            driver.Latitude = command.DriverServiceAreaContract.Latitude;
            driver.Longitude = command.DriverServiceAreaContract.Longitude;
            driver.Radius = command.DriverServiceAreaContract.Radius;

            await databaseContext.SaveChangesAsync();
        }
    }
}