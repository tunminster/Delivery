using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;

namespace Delivery.StripePayment.Domain.Handlers.CommandHandlers.PaymentIntent.PaymentIntentConfirmation
{
    public class PaymentIntentConfirmationCommand
    {
        public PaymentIntentConfirmationCommand(StripePaymentCaptureCreationContract stripePaymentCaptureCreationContract)
        {
            StripePaymentCaptureCreationContract = stripePaymentCaptureCreationContract;
        }
        public StripePaymentCaptureCreationContract StripePaymentCaptureCreationContract { get; }
    }
}