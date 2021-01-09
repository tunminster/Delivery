using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;

namespace Delivery.StripePayment.Domain.Services.ApplicationServices.StripeCapturePayment
{
    public class StripeCapturePaymentServiceResult
    {
        public StripeCapturePaymentServiceResult(
            StripePaymentCaptureCreationStatusContract stripePaymentCaptureCreationStatusContract)
        {
            StripePaymentCaptureCreationStatusContract = stripePaymentCaptureCreationStatusContract;
        }
        
        public StripePaymentCaptureCreationStatusContract StripePaymentCaptureCreationStatusContract { get;  }
    }
}