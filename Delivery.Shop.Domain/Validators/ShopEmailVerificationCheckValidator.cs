using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopEmailVerification;
using FluentValidation;

namespace Delivery.Shop.Domain.Validators
{
    public class ShopEmailVerificationCheckValidator : AbstractValidator<ShopEmailVerificationCheckContract>
    {
        public ShopEmailVerificationCheckValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
            RuleFor(x => x.Code).NotEmpty().NotNull().WithMessage("Code must be provided.");
        }
    }
}