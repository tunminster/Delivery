using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopResetPasswordVerification;
using FluentValidation;

namespace Delivery.Shop.Domain.Validators
{
    public class ShopResetPasswordRequestValidator : AbstractValidator<ShopResetPasswordRequestContract>
    {
        public ShopResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
        }
    }
}