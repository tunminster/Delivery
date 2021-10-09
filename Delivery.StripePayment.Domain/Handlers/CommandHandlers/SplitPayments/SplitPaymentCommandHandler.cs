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
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.SplitPayments;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.StripePayment.Domain.Handlers.CommandHandlers.SplitPayments
{
    public record SplitPaymentCommand(SplitPaymentCreationContract SplitPaymentCreationContract);
    
    public class SplitPaymentCommandHandler :  ICommandHandler<SplitPaymentCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public SplitPaymentCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> Handle(SplitPaymentCommand command)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;
            var shardMetadataManager = serviceProvider.GetRequiredService<IShardMetadataManager>();
            var shardInformation = shardMetadataManager.GetShardInformation<ShardInformation>().FirstOrDefault(x =>
                x.Key!.ToLower() == executingRequestContextAdapter.GetShard().Key.ToLower());
            
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            
            var transferService = new TransferService();

            if (!string.IsNullOrEmpty(command.SplitPaymentCreationContract.StoreOwnerConnectedAccountId))
            {
                var order = databaseContext.Orders.Single(x =>
                    x.ExternalId == command.SplitPaymentCreationContract.OrderId);
                
                var storeOwnerPaymentTotalAmount = 
                    (order.SubTotal + order.TaxFees) -
                    order.BusinessServiceFees;
                // Transfer payment to store connected account
                var shopOwnerTransferOptions = new TransferCreateOptions
                {
                    Amount = storeOwnerPaymentTotalAmount,
                    Currency = shardInformation?.Currency != null ? shardInformation.Currency.ToLower() : "usd",
                    Destination = command.SplitPaymentCreationContract.StoreOwnerConnectedAccountId,
                    TransferGroup = command.SplitPaymentCreationContract.OrderId
                };
                
                var shopOwnerTransferResult = await transferService.CreateAsync(shopOwnerTransferOptions);
                
                order.StoreOwnerPaymentAmount = storeOwnerPaymentTotalAmount;
                order.ShopOwnerTransferredId = shopOwnerTransferResult.Id;
                await databaseContext.SaveChangesAsync();
            }
            
            // Transfer payment to driver
            if (!string.IsNullOrEmpty(command.SplitPaymentCreationContract.DriverConnectedAccountId))
            {
                var order = databaseContext.Orders.Single(x =>
                    x.ExternalId == command.SplitPaymentCreationContract.OrderId);
                var driverPaymentFee = order.DeliveryFees;
                var driverTransferOptions = new TransferCreateOptions
                {
                    Amount = driverPaymentFee,
                    Currency = shardInformation?.Currency != null ? shardInformation.Currency.ToLower() : "usd",
                    Destination = command.SplitPaymentCreationContract.DriverConnectedAccountId,
                    TransferGroup = command.SplitPaymentCreationContract.OrderId
                };
                var driverTransferResult = await transferService.CreateAsync(driverTransferOptions);
                
                order.DriverTransferredId = driverTransferResult.Id;
                await databaseContext.SaveChangesAsync();
            }
            
            return new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };
        }
    }
}