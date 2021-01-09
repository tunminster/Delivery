using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.StripePayments;

namespace Delivery.StripePayment.Domain.Services.ApplicationServices.StripeCapturePayment
{
    public class StripeCapturePaymentServiceRequest
    {
        public StripeCapturePaymentServiceRequest(StripePaymentCaptureCreationContract stripePaymentCaptureCreationContract)
        {
            StripePaymentCaptureCreationContract = stripePaymentCaptureCreationContract;
        }
        public StripePaymentCaptureCreationContract StripePaymentCaptureCreationContract { get; set; }
    }
}