using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverApproval;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverApproval
{
    public record DriverApprovalCommand(DriverApprovalContract DriverApprovalContract);
    public class DriverApprovalCommandHandler : ICommandHandler<DriverApprovalCommand, DriverApprovalStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverApprovalCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverApprovalStatusContract> HandleAsync(DriverApprovalCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var driver = await databaseContext.Drivers.FirstOrDefaultAsync(x => x.EmailAddress == command.DriverApprovalContract.Username);

            if (driver != null)
            {
                driver.Approved = command.DriverApprovalContract.Approve;

                await databaseContext.SaveChangesAsync();
            }

            var driverApprovalStatusContract = new DriverApprovalStatusContract
            {
                DateCreated = DateTimeOffset.UtcNow
            };

            return driverApprovalStatusContract;
        }
    }
}