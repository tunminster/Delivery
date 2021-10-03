using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverProfile
{
    public record DriverServiceAreaUpdateCommand(DriverServiceAreaUpdateContract DriverServiceAreaUpdateContract);
    
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
            
            driver.ServiceArea = command.DriverServiceAreaUpdateContract.ServiceArea;
            driver.Latitude = command.DriverServiceAreaUpdateContract.Latitude;
            driver.Longitude = command.DriverServiceAreaUpdateContract.Longitude;
            driver.Radius = command.DriverServiceAreaUpdateContract.Radius;

            await databaseContext.SaveChangesAsync();
        }
    }
}