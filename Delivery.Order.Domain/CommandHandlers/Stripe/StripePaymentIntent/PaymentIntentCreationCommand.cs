using Delivery.Order.Domain.Contracts.ModelContracts.Stripe;

namespace Delivery.Order.Domain.CommandHandlers.Stripe.StripePaymentIntent
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