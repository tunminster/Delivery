using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.StripePayments;

namespace Delivery.StripePayment.Domain.CommandHandlers.StripePaymentCreation
{
    public class StripePaymentCreationCommand
    {
        public StripePaymentCreationCommand(StripePaymentCreationContract stripePaymentCreationContract)
        {
            StripePaymentCreationContract = stripePaymentCreationContract;
        }
        
        public StripePaymentCreationContract StripePaymentCreationContract { get; }
    }
}