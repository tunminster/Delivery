using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.StripePayments;

namespace Delivery.StripePayment.Domain.Handlers.CommandHandlers.StripePaymentCreation
{
    public class StripePaymentCreationCommand
    {
        public StripePaymentCreationCommand(StripePaymentCreationContract stripePaymentCreationContract, StripePaymentCreationStatusContract stripePaymentCreationStatusContract)
        {
            StripePaymentCreationContract = stripePaymentCreationContract;
            StripePaymentCreationStatusContract = stripePaymentCreationStatusContract;
        }
        
        public StripePaymentCreationContract StripePaymentCreationContract { get; }
        
        public StripePaymentCreationStatusContract StripePaymentCreationStatusContract { get; }
    }
}