using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverAssignment
{
    public record DriverOrderActionCommand(DriverOrderActionContract DriverOrderActionContract);
    
    public class DriverOrderActionCommandHandler : ICommandHandler<DriverOrderActionCommand, StatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverOrderActionCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> Handle(DriverOrderActionCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var driver = await databaseContext.Drivers.SingleAsync(x =>
                x.EmailAddress == executingRequestContextAdapter.GetAuthenticatedUser().UserEmail);

            var order = await databaseContext.Orders.SingleOrDefaultAsync(x =>
                x.ExternalId == command.DriverOrderActionContract.OrderId) ?? throw new InvalidOperationException($"Expected order by order id: {command.DriverOrderActionContract.OrderId}.");

            var driverOrder =
                await databaseContext.DriverOrders.SingleOrDefaultAsync(x =>
                    x.OrderId == order.Id && x.DriverId == driver.Id);

            if (driverOrder == null)
            {
                return new StatusContract
                {
                    Status = false,
                    DateCreated = DateTimeOffset.UtcNow
                };
            }
            
            driverOrder.Status = command.DriverOrderActionContract.DriverOrderStatus;
            driverOrder.Reason = command.DriverOrderActionContract.Reason;

            await databaseContext.SaveChangesAsync();

            return new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };
            
        }
    }
}