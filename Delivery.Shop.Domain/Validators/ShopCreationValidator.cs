using System.Linq;
using Delivery.Shop.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Shop.Domain.Validators
{
    public class ShopCreationValidator : AbstractValidator<ShopCreationContract>
    {
        public ShopCreationValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().NotNull().WithMessage("Full name must be provided.");
            RuleFor(x => x.EmailAddress).NotEmpty().NotNull().WithMessage("Email address must be provided.")
                .EmailAddress().WithMessage("A valid email is required");
            RuleFor(x => x.PhoneNumber).NotEmpty().NotNull().WithMessage("Phone number must be provided.");
            RuleFor(x => x.Password).NotEmpty().NotNull().WithMessage("Password must be valid.");
            RuleFor(x => x.Password).MinimumLength(6).WithMessage("Password must be at least 6 characters");
            RuleFor(x => x.ConfirmPassword).NotEmpty().NotNull().WithMessage("Confirm password must be provided");
            RuleFor(x => x.Password).Equal(x => x.ConfirmPassword).WithMessage("Passwords do not match.");
            RuleFor(x => x.StoreTypeId).NotEmpty().NotNull().WithMessage("Store type id must be provided.");
            RuleFor(x => x.StoreOpeningHours.Count).NotNull().GreaterThanOrEqualTo(7)
                .WithMessage("Store opening hours must be defined for 7 days");
            RuleFor(x => x.StoreOpeningHours.Count(x => !string.IsNullOrEmpty(x.TimeZone))).Equal(7)
                .WithMessage("Timezone must t be defined.");
        }
    }
}