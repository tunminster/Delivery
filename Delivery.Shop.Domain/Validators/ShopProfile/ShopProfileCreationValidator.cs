using System.Linq;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopProfile;
using FluentValidation;

namespace Delivery.Shop.Domain.Validators.ShopProfile
{
    public class ShopProfileCreationValidator : AbstractValidator<ShopProfileCreationContract>
    {
        public ShopProfileCreationValidator()
        {
            RuleFor(x => x.StoreTypeId).NotEmpty().NotNull().WithMessage("Store type id must be provided.");
            RuleFor(x => x.AddressLine1).NotEmpty().NotNull().WithMessage("AddressLine1 must be provided.");
            RuleFor(x => x.City).NotEmpty().NotNull().WithMessage("City must be provided.");
            RuleFor(x => x.County).NotEmpty().NotNull().WithMessage("County must be provided.");
            RuleFor(x => x.ZipCode).NotEmpty().NotNull().WithMessage("ZipCode must be provided.");
            RuleFor(x => x.StoreOpeningHours.Count).NotNull().GreaterThanOrEqualTo(7)
                .WithMessage("Store opening hours must be defined for 7 days");
            RuleFor(x => x.StoreOpeningHours.Count(x => !string.IsNullOrEmpty(x.TimeZone))).Equal(7)
                .WithMessage("Timezone must t be defined.");
        }
    }
}