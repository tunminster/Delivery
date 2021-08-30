using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopEmailVerification;
using FluentValidation;

namespace Delivery.Shop.Domain.Validators
{
    public class ShopEmailVerificationValidator : AbstractValidator<ShopEmailVerificationContract>
    {
        public ShopEmailVerificationValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().NotNull().WithMessage("Shop owner name must be provided.");
            RuleFor(x => x.EmailAddress).NotEmpty().NotNull().WithMessage("Email address must be provided.");
        }
    }
}