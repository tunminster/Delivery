using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Contracts.V1;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.CommandHandlers;
using Delivery.Order.Domain.Contracts.V1.ModelContracts.Stripe;
using Microsoft.ApplicationInsights.DataContracts;
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
        
        public async Task<PaymentIntentCreationStatusContract> HandleAsync(PaymentIntentCreationCommand command)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
            var shardMetadataManager = serviceProvider.GetRequiredService<IShardMetadataManager>();
            var shardInformation = shardMetadataManager.GetShardInformation<ShardInformation>().FirstOrDefault(x =>
                x.Key!.ToLower() == executingRequestContextAdapter.GetShard().Key.ToLower());

            StripeConfiguration.ApiKey = stripeApiKey;
            
            var paymentIntentService = new PaymentIntentService();
            
            var createOptions = CreatePaymentIntentCreateOptions(false, command, shardInformation);
            
            var requestOptions = new RequestOptions {StripeAccount = command.PaymentIntentCreationContract.StoreConnectedStripeAccountId};
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(PaymentIntent)} created options: {createOptions.ConvertToJson()}", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
            
            var paymentIntent = await paymentIntentService.CreateAsync(createOptions, requestOptions);
            //var paymentIntent = await paymentIntentService.CreateAsync(createOptions);
            
            var paymentIntentCreationStatusContract =
                new PaymentIntentCreationStatusContract(paymentIntent.Id, paymentIntent.ClientSecret, command.PaymentIntentCreationContract.OrderId, string.Empty);

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
                    Currency = shardInformation?.Currency != null ? shardInformation.Currency.ToLower() : "usd",
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