using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Contracts.V1;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Domain.CommandHandlers;
using Delivery.Order.Domain.Contracts.ModelContracts.Stripe;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripePaymentIntent
{
    public class PaymentIntentCreationCommandHandler : ICommandHandler<PaymentIntentCreationCommand, PaymentIntentCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public PaymentIntentCreationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<PaymentIntentCreationStatusContract> Handle(PaymentIntentCreationCommand command)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
            var shardMetadataManager = serviceProvider.GetRequiredService<IShardMetadataManager>();
            var shardInformation = shardMetadataManager.GetShardInformation<ShardInformation>().FirstOrDefault(x =>
                x.Key!.ToLower() == executingRequestContextAdapter.GetShard().Key.ToLower());
            
            StripeConfiguration.ApiKey = stripeApiKey;
            
            var paymentIntentService = new PaymentIntentService();


            var createOptions = CreatePaymentIntentCreateOptions(true, command, shardInformation);
            
            var requestOptions = new RequestOptions {StripeAccount = command.PaymentIntentCreationContract.StoreConnectedStripeAccountId};
            //var paymentIntent = await paymentIntentService.CreateAsync(createOptions, requestOptions);
            var paymentIntent = await paymentIntentService.CreateAsync(createOptions);

            var storeOwnerAmount =
                (command.PaymentIntentCreationContract.Subtotal + command.PaymentIntentCreationContract.TaxFeeAmount) -
                command.PaymentIntentCreationContract.BusinessFeeAmount;

            var driverFeeAmount = command.PaymentIntentCreationContract.DeliveryFeeAmount;
            
            // Transfer payment to store connected account
            var shopOwnerTransferOptions = new TransferCreateOptions
            {
                Amount = storeOwnerAmount,
                Currency = shardInformation?.Currency != null ? shardInformation.Currency.ToLower() : "usd",
                Destination = command.PaymentIntentCreationContract.StoreConnectedStripeAccountId,
                TransferGroup = command.PaymentIntentCreationContract.OrderId
            };

            var transferService = new TransferService();
            var transfer = await transferService.CreateAsync(shopOwnerTransferOptions);

            // Transfer payment to driver
            // var secondTransferOptions = new TransferCreateOptions
            // {
            //     Amount = driverFeeAmount,
            //     Currency = shardInformation?.Currency != null ? shardInformation.Currency.ToLower() : "usd",
            //     Destination = command.PaymentIntentCreationContract.DriverConnectedStripeAccountId,
            //     TransferGroup = command.PaymentIntentCreationContract.OrderId
            // };
            // var secondTransfer = await transferService.CreateAsync(secondTransferOptions);
            
            
            var paymentIntentCreationStatusContract =
                new PaymentIntentCreationStatusContract(paymentIntent.Id, paymentIntent.ClientSecret, command.PaymentIntentCreationContract.OrderId, transfer.Id);

            return paymentIntentCreationStatusContract;

        }

        private static PaymentIntentCreateOptions CreatePaymentIntentCreateOptions(bool transferGroup, PaymentIntentCreationCommand command, ShardInformation shardInformation)
        {
            if (transferGroup)
            {
                return new PaymentIntentCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                    {
                        "card",
                    },
                    Amount = command.PaymentIntentCreationContract.Amount,
                    Currency = shardInformation?.Currency != null ? shardInformation.Currency.ToLower() : "gbp",
                    ApplicationFeeAmount = command.PaymentIntentCreationContract.ApplicationFeeAmount,
                    TransferGroup = command.PaymentIntentCreationContract.OrderId,
                    Metadata = new Dictionary<string, string>
                    {
                        { "order_id", command.PaymentIntentCreationContract.OrderId },
                    }
                };
            }
            
            return new PaymentIntentCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                Amount = command.PaymentIntentCreationContract.Amount,
                Currency = shardInformation?.Currency != null ? shardInformation.Currency.ToLower() : "gbp",
                ApplicationFeeAmount = command.PaymentIntentCreationContract.ApplicationFeeAmount,
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", command.PaymentIntentCreationContract.OrderId },
                }
            };
            
        }
    }
}