using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrderUpdate;
using Delivery.Order.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripeOrderUpdate
{
    public class OrderUpdateCommandHandler : ICommandHandler<OrderUpdateCommand, StripeOrderUpdateStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public OrderUpdateCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StripeOrderUpdateStatusContract> Handle(OrderUpdateCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var order = await databaseContext.Orders.FirstOrDefaultAsync(x =>
                x.ExternalId == command.StripeOrderUpdateContract.OrderId);

            order.PaymentIntentId = command.StripeOrderUpdateContract.PaymentIntentId;
            order.PaymentStatus = command.StripeOrderUpdateContract.PaymentStatus;
            order.Status = command.StripeOrderUpdateContract.OrderStatus;

            await databaseContext.SaveChangesAsync();

            var stripeOrderUpdateStatusContract = new StripeOrderUpdateStatusContract
            {
                OrderId = order.ExternalId,
                UpdatedDateTime = DateTimeOffset.UtcNow,
                PaymentStatusEnum = string.Equals(command.StripeOrderUpdateContract.PaymentStatus, PaymentStatusEnum.Success.ToString(), StringComparison.CurrentCultureIgnoreCase) ? PaymentStatusEnum.Success : PaymentStatusEnum.Failed 
            };

            return stripeOrderUpdateStatusContract;

        }
    }
}