using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopLogin;
using FluentValidation;

namespace Delivery.Shop.Domain.Validators
{
    public class ShopLoginValidator : AbstractValidator<ShopLoginContract>
    {
        public ShopLoginValidator()
        {
            RuleFor(x => x.Username).NotEmpty().NotNull().WithMessage("Username must be provided.");
            RuleFor(x => x.Password).NotEmpty().NotNull().WithMessage("Password must be provided.");
        }
    }
}