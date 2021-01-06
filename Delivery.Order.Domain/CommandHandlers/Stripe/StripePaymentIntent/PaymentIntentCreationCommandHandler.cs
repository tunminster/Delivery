using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Order.Domain.Contracts.ModelContracts.Stripe;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.Order.Domain.CommandHandlers.Stripe.StripePaymentIntent
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
            var stripeApiKey = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSetting<string>("Stripe-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;
            
            var service = new PaymentIntentService();
            
            
            var createOptions = new PaymentIntentCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                Amount = command.PaymentIntentCreationContract.Amount,
                Currency = "gbp",
                //ApplicationFeeAmount = command.PaymentIntentCreationContract.ApplicationFeeAmount,
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", command.PaymentIntentCreationContract.OrderId },
                }
            };

            //var requestOptions = new RequestOptions {StripeAccount = command.PaymentIntentCreationContract.ConnectedStripeAccountId};

            //var paymentIntent = await service.CreateAsync(createOptions, requestOptions);
            var paymentIntent = await service.CreateAsync(createOptions);
            
            var paymentIntentCreationStatusContract =
                new PaymentIntentCreationStatusContract(paymentIntent.Id, paymentIntent.ClientSecret);

            return paymentIntentCreationStatusContract;

        }
    }
}