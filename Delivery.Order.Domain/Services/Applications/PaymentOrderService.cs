using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Order.Domain.CommandHandlers.Stripe.StripeOrderCreation;
using Delivery.Order.Domain.CommandHandlers.Stripe.StripePaymentIntent;
using Delivery.Order.Domain.Contracts.ModelContracts.Stripe;
using Delivery.Order.Domain.Factories;

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
            var orderCreationCommand = new OrderCreationCommand(paymentOrderServiceRequest.StripeOrderCreationContract,
                paymentOrderServiceRequest.CurrencyCode);

            var orderCreationStatus =
                await new OrderCreationCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(
                    orderCreationCommand);

            
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
    }
}