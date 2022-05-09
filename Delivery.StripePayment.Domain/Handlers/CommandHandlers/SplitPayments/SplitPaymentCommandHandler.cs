using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Contracts.V1;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.SplitPayments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.StripePayment.Domain.Handlers.CommandHandlers.SplitPayments
{
    public record SplitPaymentCommand(SplitPaymentCreationContract SplitPaymentCreationContract);
    
    public class SplitPaymentCommandHandler : ICommandHandler<SplitPaymentCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public SplitPaymentCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> HandleAsync(SplitPaymentCommand command)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;
            var shardMetadataManager = serviceProvider.GetRequiredService<IShardMetadataManager>();
            var shardInformation = shardMetadataManager.GetShardInformation<ShardInformation>().FirstOrDefault(x =>
                x.Key!.ToLower() == executingRequestContextAdapter.GetShard().Key.ToLower());
            
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            
            var transferService = new TransferService();

            var order = await databaseContext.Orders.SingleAsync(x =>
                x.ExternalId == command.SplitPaymentCreationContract.OrderId);
            
            // Transfer payment to driver
            if (!string.IsNullOrEmpty(command.SplitPaymentCreationContract.DriverConnectedAccountId))
            {
                var driverPaymentFee = order.DeliveryFees + order.DeliveryTips;
                var driverTransferOptions = new TransferCreateOptions
                {
                    Amount = driverPaymentFee,
                    Currency = shardInformation?.Currency != null ? shardInformation.Currency.ToLower() : "usd",
                    Destination = command.SplitPaymentCreationContract.DriverConnectedAccountId,
                    TransferGroup = command.SplitPaymentCreationContract.OrderId
                };
                var driverTransferResult = await transferService.CreateAsync(driverTransferOptions);
                
                order.DriverTransferredId = driverTransferResult.Id;

                var driver = databaseContext.Drivers.FirstOrDefault(x =>
                    x.PaymentAccountId == command.SplitPaymentCreationContract.DriverConnectedAccountId);

                if (driver != null)
                {
                    var driverPayment = new DriverPayment
                    {
                        TotalPaymentAmount = driverPaymentFee ?? 0,
                        DriverId = driver.Id
                    };
                    databaseContext.Add(driverPayment);
                }
                
                await databaseContext.SaveChangesAsync();
            }
            
            //Transfer promotion discount to store owner account
            var promoCode = order.CouponCode;
            if (!string.IsNullOrEmpty(promoCode) && !string.IsNullOrEmpty(order.PaymentAccountNumber))
            {
                var couponCode = await databaseContext.CouponCodes
                    .FirstOrDefaultAsync(x => x.PromotionCode == promoCode);

                if (couponCode != null)
                {
                    var storeOwnerTransferOptions = new TransferCreateOptions
                    {
                        Amount = couponCode.DiscountAmount,
                        Currency = shardInformation?.Currency != null ? shardInformation.Currency.ToLower() : "usd",
                        Destination = order.PaymentAccountNumber,
                        TransferGroup = command.SplitPaymentCreationContract.OrderId
                    };
                    
                    var storeOwnerTransferResult = await transferService.CreateAsync(storeOwnerTransferOptions);
                    order.ShopOwnerTransferredId = storeOwnerTransferResult.Id;
                    order.StoreOwnerPaymentAmount = couponCode.DiscountAmount;
                }
                
                await databaseContext.SaveChangesAsync();
                
            }


            return new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };
        }
    }
}