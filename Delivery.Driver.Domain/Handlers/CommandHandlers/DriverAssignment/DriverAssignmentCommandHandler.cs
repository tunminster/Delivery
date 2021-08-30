using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverAssignment
{
    public record DriverAssignmentCommand(DriverAssignmentCreationContract DriverAssignmentCreationContract);
    
    public class DriverAssignmentCommandHandler : ICommandHandler<DriverAssignmentCommand, DriverAssignmentStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverAssignmentCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverAssignmentStatusContract> Handle(DriverAssignmentCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var driverOrder =
                databaseContext.DriverOrders.SingleOrDefault(x =>
                    x.DriverId == command.DriverAssignmentCreationContract.DriverId 
                    && x.OrderId == command.DriverAssignmentCreationContract.OrderId 
                    && x.Status == DriverOrderStatus.None);

            if (driverOrder == null)
            {
                var createDriverOrder = new Database.Entities.DriverOrder
                {
                    DriverId = command.DriverAssignmentCreationContract.DriverId,
                    OrderId = command.DriverAssignmentCreationContract.OrderId,
                    Status = DriverOrderStatus.None
                };

                databaseContext.DriverOrders.Add(createDriverOrder);
                await databaseContext.SaveChangesAsync();
            }

            var driverAssignmentStatusContract = new DriverAssignmentStatusContract
            {
                DateCreated = DateTimeOffset.UtcNow
            };

            return driverAssignmentStatusContract;
        }
    }
}