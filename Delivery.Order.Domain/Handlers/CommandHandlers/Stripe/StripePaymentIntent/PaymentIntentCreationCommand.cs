using Delivery.Order.Domain.Contracts.V1.ModelContracts.Stripe;

namespace Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripePaymentIntent
{
    public class PaymentIntentCreationCommand
    {
        public PaymentIntentCreationCommand(PaymentIntentCreationContract paymentIntentCreationContract)
        {
            PaymentIntentCreationContract = paymentIntentCreationContract;
        }
        
        public PaymentIntentCreationContract PaymentIntentCreationContract { get; }
    }
}