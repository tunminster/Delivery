using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverReAssignment;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Strategies.DriverAssignmentStrategy
{
    public class AwaitingDeliveryOrderStrategy : DriverReAssignmentStrategy
    {
        public AwaitingDeliveryOrderStrategy(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public override bool AppliesTo(DriverOrderStatus driverOrderStatus) => driverOrderStatus.Equals(DriverOrderStatus.InProgress);
        

        public override async Task<DriverReAssignmentCreationStatusContract> ExecuteAsync(int driverOrderId, int awaitingMinutes)
        {
            await using var databaseContext =
                await PlatformDbContext.CreateAsync(ServiceProvider, ExecutingRequestContextAdapter);

            var driverOrder = await databaseContext.DriverOrders
                .Where(x => x.Id == driverOrderId)
                .Include(x => x.Driver)
                .Include(x => x.Order).FirstAsync();
            
            return new DriverReAssignmentCreationStatusContract
            {
                DriverId = driverOrder.Driver.ExternalId,
                OrderId = driverOrder.Order.ExternalId, AssignedDateTime = DateTimeOffset.Now,
                DriverOrderStatus = driverOrder.Status
            };
        }
    }
}