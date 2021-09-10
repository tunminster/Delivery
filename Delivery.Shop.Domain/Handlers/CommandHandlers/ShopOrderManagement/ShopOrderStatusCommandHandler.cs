using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopOrderManagement
{
    public record ShopOrderStatusCommand(ShopOrderStatusCreationContract ShopOrderStatusCreationContract);
    
    public class ShopOrderStatusCommandHandler : ICommandHandler<ShopOrderStatusCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ShopOrderStatusCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> Handle(ShopOrderStatusCommand statusCommand)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var order =
                await databaseContext.Orders.FirstOrDefaultAsync(x =>
                    x.ExternalId == statusCommand.ShopOrderStatusCreationContract.OrderId);
            
            if (order == null)
            {
                throw new InvalidOperationException(
                    $"Empty order returns for order id: {statusCommand.ShopOrderStatusCreationContract.OrderId}");
            }

            order.Status = statusCommand.ShopOrderStatusCreationContract.OrderStatus;

            await databaseContext.SaveChangesAsync();

            return new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };
        }
    }
}