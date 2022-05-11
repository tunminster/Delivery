using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderRefund;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopOrderRefund
{
    public record ShopOrderRefundCommand(ShopOrderRefundCreationContract ShopOrderRefundCreationContract);

    public class ShopOrderRefundCommandHandler : ICommandHandler<ShopOrderRefundCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ShopOrderRefundCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> HandleAsync(ShopOrderRefundCommand command)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;
            
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var order = await databaseContext.Orders.SingleOrDefaultAsync(x => x.ExternalId == command.ShopOrderRefundCreationContract.OrderId) 
                         ?? throw new InvalidOperationException($"Expected an order by id: {command.ShopOrderRefundCreationContract.OrderId}");

            var stripePayment = await databaseContext.StripePayments.SingleAsync(x => x.OrderId == order.Id);

            var refundCreateOptions = new RefundCreateOptions
            {
                PaymentIntent = stripePayment.StripePaymentIntentId,
                Amount = order.TotalAmount,
                Reason = command.ShopOrderRefundCreationContract.Reason
            };

            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(refundCreateOptions);

            // Persist refund result
            var stripePaymentRefund = new StripePaymentRefund
            {
                OrderId = order.Id,
                Status = refund.Status,
                StripePaymentIntentId = stripePayment.StripePaymentIntentId,
                TotalAmount = order.TotalAmount
            };

            databaseContext.StripePaymentRefunds.Add(stripePaymentRefund);
            await databaseContext.SaveChangesAsync();

            return new StatusContract {Status = true, DateCreated = DateTimeOffset.UtcNow};
        }
    }
}

