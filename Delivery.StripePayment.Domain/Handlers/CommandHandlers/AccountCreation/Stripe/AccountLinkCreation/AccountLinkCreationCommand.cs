using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;

namespace Delivery.StripePayment.Domain.CommandHandlers.AccountCreation.Stripe.AccountLinkCreation
{
    public class AccountLinkCreationCommand
    {
        public AccountLinkCreationCommand(StripeAccountLinkCreationContract stripeAccountLinkCreationContract)
        {
            StripeAccountLinkCreationContract = stripeAccountLinkCreationContract;
        }
        
        public StripeAccountLinkCreationContract StripeAccountLinkCreationContract { get; }
    }
}