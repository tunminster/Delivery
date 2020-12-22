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
                Amount = 2000,
                Currency = "gbp",
                ApplicationFeeAmount = 123
            };

            var requestOptions = new RequestOptions {StripeAccount = "{{CONNECTED_STRIPE_ACCOUNT_ID}}"};

            var paymentIntent = await service.CreateAsync(createOptions, requestOptions);

            var paymentIntentCreationStatusContract =
                new PaymentIntentCreationStatusContract(paymentIntent.ClientSecret);

            return paymentIntentCreationStatusContract;

        }
    }
}