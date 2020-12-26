using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;

namespace Delivery.StripePayment.Domain.CommandHandlers.AccountCreation
{
    public class AccountCreationCommand
    {
        public AccountCreationCommand(StripeAccountCreationContract stripeAccountCreationContract)
        {
            StripeAccountCreationContract = stripeAccountCreationContract;
        }
        public StripeAccountCreationContract StripeAccountCreationContract { get; }
    }
}