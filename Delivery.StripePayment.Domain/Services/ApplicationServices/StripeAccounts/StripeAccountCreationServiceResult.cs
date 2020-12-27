using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;

namespace Delivery.StripePayment.Domain.Services.ApplicationServices.StripeAccounts
{
    public class StripeAccountCreationServiceResult
    {
        public StripeAccountCreationServiceResult(StripeAccountCreationStatusContract stripeAccountCreationStatusContract)
        {
            StripeAccountCreationStatusContract = stripeAccountCreationStatusContract;
        }
        public StripeAccountCreationStatusContract StripeAccountCreationStatusContract { get; }
    }
}