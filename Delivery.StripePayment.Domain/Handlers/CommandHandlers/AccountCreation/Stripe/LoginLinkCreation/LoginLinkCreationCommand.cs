using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;

namespace Delivery.StripePayment.Domain.CommandHandlers.AccountCreation.Stripe.LoginLinkCreation
{
    public class LoginLinkCreationCommand
    {
        public LoginLinkCreationCommand(StripeLoginLinkCreationContract stripeLoginLinkCreationContract)
        {
            StripeLoginLinkCreationContract = stripeLoginLinkCreationContract;
        }
        public StripeLoginLinkCreationContract StripeLoginLinkCreationContract { get; }
    }
}