using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Order.Domain.CommandHandlers.Stripe.StripeOrderCreation;
using Delivery.Order.Domain.CommandHandlers.Stripe.StripeOrderTotalAmountCreation;
using Delivery.Order.Domain.CommandHandlers.Stripe.StripePaymentIntent;
using Delivery.Order.Domain.Contracts.ModelContracts.Stripe;
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;
using Delivery.Order.Domain.Contracts.V1.MessageContracts;
using Delivery.Order.Domain.Factories;
using Delivery.Order.Domain.Handlers.MessageHandlers;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Order.Domain.Services.Applications
{
    public class PaymentOrderService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public PaymentOrderService(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<PaymentIntentCreationStatusContract> ExecuteStripePaymentIntentWorkflow(PaymentOrderServiceRequest paymentOrderServiceRequest)
        {
            var orderCreateStatusContract = new OrderCreationStatus
            {
                CurrencyCode = paymentOrderServiceRequest.CurrencyCode
            };

            var stripeOrderTotalAmountCreationCommand =
                new StripeOrderTotalAmountCreationCommand(paymentOrderServiceRequest.StripeOrderCreationContract,
                    orderCreateStatusContract);
            var productIds = string.Join(",",
                paymentOrderServiceRequest.StripeOrderCreationContract.OrderItems.Select(x => x.ProductId));
            var cacheKey =
                $"Database-{executingRequestContextAdapter.GetShard().Key}-products-{productIds}-default-includes-{executingRequestContextAdapter.GetAuthenticatedUser().UserEmail}";

            var orderCreationStatus =
                await new StripeOrderTotalAmountCreationCommandHandler(serviceProvider, executingRequestContextAdapter,
                    cacheKey).Handle(stripeOrderTotalAmountCreationCommand);
            
            await PublishOrderCreationMessageAsync(paymentOrderServiceRequest.StripeOrderCreationContract,
                orderCreationStatus);
            
            var paymentIntentCreationContract = new PaymentIntentCreationContract
            {
                PaymentMethod = "card",
                Amount = orderCreationStatus.TotalAmount,
                ApplicationFeeAmount = ApplicationFeeGenerator.GeneratorFees(orderCreationStatus.TotalAmount),
                ConnectedStripeAccountId = "acct_1I1KVwRMcyGaqHir",
                OrderId = orderCreationStatus.OrderId,
                Currency = "gbp"
            };

            var paymentIntentCreationCommand = new PaymentIntentCreationCommand(paymentIntentCreationContract);
            var paymentIntentCreationStatusContract =
                await new PaymentIntentCreationCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(paymentIntentCreationCommand);

            return paymentIntentCreationStatusContract;

        }

        private async Task PublishOrderCreationMessageAsync(StripeOrderCreationContract stripeOrderCreationContract,
            OrderCreationStatus orderCreationStatus)
        {
            var orderCreationMessage = new OrderCreationMessage
            {
                PayloadIn = stripeOrderCreationContract,
                PayloadOut = orderCreationStatus,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };

            await new OrderCreationMessagePublisher(serviceProvider).PublishAsync(orderCreationMessage);
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(PublishOrderCreationMessageAsync)} published order creation message", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
        }
    }
}