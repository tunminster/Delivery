using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments;
using FluentValidation;

namespace Delivery.StripePayment.Domain.Validators
{
    public class CouponCodeConfirmationValidator : AbstractValidator<CouponCodeConfirmationQueryContract>
    {
        public CouponCodeConfirmationValidator()
        {
            RuleFor(x => x.CouponCode).NotEmpty().WithMessage("Coupon code must be provided.");
        }
    }
}