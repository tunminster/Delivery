using Delivery.Shop.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Shop.Domain.Validators
{
    public class ShopCreationValidator : AbstractValidator<ShopCreationContract>
    {
        public ShopCreationValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().NotNull().WithMessage("Full name must be provided.");
            RuleFor(x => x.EmailAddress).NotEmpty().NotNull().WithMessage("Email address must be provided.");
            RuleFor(x => x.PhoneNumber).NotEmpty().NotNull().WithMessage("Phone number must be provided.");
            RuleFor(x => x.Password).NotEmpty().NotNull().WithMessage("Password must be valid.");
            RuleFor(x => x.Password).MinimumLength(6).WithMessage("Password must be at least 6 characters");
            RuleFor(x => x.ConfirmPassword).NotEmpty().NotNull().WithMessage("Confirm password must be provided");
            RuleFor(x => x.Password).Equal(x => x.ConfirmPassword).WithMessage("Passwords do not match.");
        }
    }
}