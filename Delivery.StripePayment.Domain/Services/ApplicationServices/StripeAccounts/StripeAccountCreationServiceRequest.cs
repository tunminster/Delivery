using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;

namespace Delivery.StripePayment.Domain.Services.ApplicationServices.StripeAccounts
{
    public class StripeAccountCreationServiceRequest
    {
        public StripeAccountCreationServiceRequest(StripeAccountCreationContract stripeAccountCreationContract)
        {
            StripeAccountCreationContract = stripeAccountCreationContract;
        }
        public StripeAccountCreationContract StripeAccountCreationContract { get;  }
    }
}