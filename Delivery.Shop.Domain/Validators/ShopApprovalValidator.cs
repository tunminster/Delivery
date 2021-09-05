using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopApproval;
using FluentValidation;

namespace Delivery.Shop.Domain.Validators
{
    public class ShopApprovalValidator : AbstractValidator<ShopApprovalContract>
    {
        public ShopApprovalValidator()
        {
            RuleFor(x => x.ShopId).NotEmpty().NotNull().WithMessage("Shop id must be provided.");
        }
    }
}