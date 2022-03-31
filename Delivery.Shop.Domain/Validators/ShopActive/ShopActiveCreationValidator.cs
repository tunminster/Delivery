using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopActive;
using FluentValidation;

namespace Delivery.Shop.Domain.Validators.ShopActive;

public class ShopActiveCreationValidator : AbstractValidator<ShopActiveCreationContract>
{
    public ShopActiveCreationValidator()
    {
        RuleFor(x => x.ShopUserName).NotEmpty().WithMessage($"{nameof(ShopActiveCreationContract.ShopUserName)} must be provided.");
    }
}