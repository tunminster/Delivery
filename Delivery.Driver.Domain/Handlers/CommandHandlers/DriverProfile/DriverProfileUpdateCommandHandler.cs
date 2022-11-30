using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverProfile
{
    public record DriverProfileUpdateCommand(DriverProfileUpdateContract DriverProfileUpdateContract, string ImageUri);
    public class DriverProfileUpdateCommandHandler : ICommandHandler<DriverProfileUpdateCommand>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverProfileUpdateCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task HandleAsync(DriverProfileUpdateCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected an authenticated user");
            var driver = await databaseContext.Drivers.SingleOrDefaultAsync(x =>
                x.ExternalId == command.DriverProfileUpdateContract.DriverId && x.EmailAddress == userEmail);

            driver.ImageUri = command.ImageUri;

            await databaseContext.SaveChangesAsync();
        }
    }
}