using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrderUpdate;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripeOrderUpdate
{
    public class OrderStatusUpdateCommandHandler : ICommandHandler<OrderStatusUpdateCommand, StripeUpdateOrderStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public OrderStatusUpdateCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StripeUpdateOrderStatusContract> HandleAsync(OrderStatusUpdateCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var order = await databaseContext.Orders.FirstOrDefaultAsync(x =>
                x.ExternalId == command.StripeUpdateOrderContract.OrderId);

            order.Status = command.StripeUpdateOrderContract.OrderStatus;
            
            await databaseContext.SaveChangesAsync();

            var stripeUpdateOrderStatusContract = new StripeUpdateOrderStatusContract
            {
                OrderId = command.StripeUpdateOrderContract.OrderId,
                UpdatedDate = DateTimeOffset.UtcNow
            };
            return stripeUpdateOrderStatusContract;
        }
    }
}