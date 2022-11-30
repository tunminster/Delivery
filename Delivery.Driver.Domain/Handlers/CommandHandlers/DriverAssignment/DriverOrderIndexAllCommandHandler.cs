using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Driver.Domain.Constants;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverAssignment
{
    public record DriverOrderIndexAllCommand(DateTimeOffset DateIndexed);
    
    public class DriverOrderIndexAllCommandHandler : ICommandHandler<DriverOrderIndexAllCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverOrderIndexAllCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task HandleAsync(DriverOrderIndexAllCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var driverOrders = await databaseContext.DriverOrders
                .Where(x => x.Status == DriverOrderStatus.Complete && x.InsertionDateTime > DateTimeOffset.UtcNow.AddYears(-DriverConstant.MaximumYear))
                .Include(x => x.Driver)
                .Include(x => x.Order)
                .ToListAsync();

            foreach (var item in driverOrders)
            {
                await new DriverOrderIndexCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleAsync(new DriverOrderIndexCommand(new DriverOrderIndexCreationContract
                    {
                        DriverId = item.Driver.ExternalId,
                        OrderId = item.Order.ExternalId
                    }));
            }
        }
    }
}