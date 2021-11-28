using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Contracts.V1;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.StripePayment.Domain.Handlers.CommandHandlers.CouponPayments
{
    public record CouponPaymentCommand(CouponPaymentCreationContract CouponPaymentCreationContract);
    public class CouponPaymentCommandHandler : ICommandHandler<CouponPaymentCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public CouponPaymentCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> Handle(CouponPaymentCommand command)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;
            var shardMetadataManager = serviceProvider.GetRequiredService<IShardMetadataManager>();
            var shardInformation = shardMetadataManager.GetShardInformation<ShardInformation>().FirstOrDefault(x =>
                x.Key!.ToLower() == executingRequestContextAdapter.GetShard().Key.ToLower());
            
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var transferService = new TransferService();
            
            // Transfer promotion amount to shop owner
            if (!string.IsNullOrEmpty(command.CouponPaymentCreationContract.ShopOwnerConnectedAccount))
            {
                var order = databaseContext.Orders.Single(x =>
                    x.ExternalId == command.CouponPaymentCreationContract.OrderId);

                if (string.IsNullOrEmpty(order.CouponCode))
                {
                    return new StatusContract { Status = false, DateCreated = DateTimeOffset.UtcNow };
                }

                var couponCode = databaseContext.CouponCodes.SingleOrDefault(x => x.PromotionCode == order.CouponCode);

                if (couponCode == null)
                {
                    return new StatusContract { Status = false, DateCreated = DateTimeOffset.UtcNow };
                }
                
                var discountAmount = couponCode.DiscountAmount;
                var driverTransferOptions = new TransferCreateOptions
                {
                    Amount = discountAmount,
                    Currency = shardInformation?.Currency != null ? shardInformation.Currency.ToLower() : "usd",
                    Destination = command.CouponPaymentCreationContract.ShopOwnerConnectedAccount,
                    TransferGroup = command.CouponPaymentCreationContract.OrderId
                };
                await transferService.CreateAsync(driverTransferOptions);
                
                order.CouponDiscountPaid = discountAmount;
                await databaseContext.SaveChangesAsync();
            }
            
            return new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };
        }
    }
}