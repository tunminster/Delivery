using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.StripePayment.Domain.Validators
{
    public class StripePaymentCaptureCreationValidator : AbstractValidator<StripePaymentCaptureCreationContract>
    {
        public StripePaymentCaptureCreationValidator()
        {
            RuleFor(x => x.StripePaymentIntentId).NotNull().NotEmpty()
                .WithMessage("Stipe payment intent id must be provided.");

            RuleFor(x => x.StripePaymentMethodId).NotNull().NotEmpty()
                .WithMessage("Stripe payment method id must be provided.");

            RuleFor(x => x.StripeFingerPrint).NotNull().NotEmpty().WithMessage("Stripe finger-print must be provided");
        }
    }
}