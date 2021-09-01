using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopResetPasswordVerification;
using FluentValidation;

namespace Delivery.Shop.Domain.Validators
{
    public class ShopResetPasswordVerificationCheckValidator : AbstractValidator<ShopResetPasswordCreationContract>
    {
        public ShopResetPasswordVerificationCheckValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
            RuleFor(x => x.Code).NotEmpty().NotNull().WithMessage("Code must be provided.");
            RuleFor(x => x.Password).NotEmpty().NotNull().WithMessage("Password must be provided");
            RuleFor(x => x.ConfirmPassword).NotEmpty().NotNull().WithMessage("Confirm password must be provided");
            RuleFor(x => x.Password).Equal(x => x.ConfirmPassword).WithMessage("Passwords do not match.");
            RuleFor(x => x.Password).MinimumLength(6).WithMessage("Password should be at least 6 characters");
        }
    }
}