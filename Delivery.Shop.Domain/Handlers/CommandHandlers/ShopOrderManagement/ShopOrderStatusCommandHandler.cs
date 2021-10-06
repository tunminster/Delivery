using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Constants;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Constants;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopOrderManagement
{
    public record ShopOrderStatusCommand(ShopOrderStatusCreationContract ShopOrderStatusCreationContract);
    
    public class ShopOrderStatusCommandHandler : ICommandHandler<ShopOrderStatusCommand, ShopOrderStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ShopOrderStatusCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ShopOrderStatusContract> Handle(ShopOrderStatusCommand statusCommand)
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
            order.PreparationTime = statusCommand.ShopOrderStatusCreationContract.PreparationTime;
            order.PickupTime = statusCommand.ShopOrderStatusCreationContract.PickupTime ?? DateTimeOffset.UtcNow.AddMinutes(statusCommand.ShopOrderStatusCreationContract.PreparationTime + ShopConstant.DefaultPickupMinutes);
            order.DateUpdated = DateTimeOffset.UtcNow;

            await databaseContext.SaveChangesAsync();
            
            // re-index shop order
            var shopOrderIndexCommand = new ShopOrderIndexCommand(order.ExternalId);
            await new ShopOrderIndexCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(
                shopOrderIndexCommand);

            return new ShopOrderStatusContract
            {
                OrderId = order.ExternalId,
                OrderStatus = statusCommand.ShopOrderStatusCreationContract.OrderStatus,
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };
        }
    }
}