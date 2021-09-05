using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopApproval;
using FluentValidation;

namespace Delivery.Shop.Domain.Validators
{
    public class ShopUserApprovalValidator : AbstractValidator<ShopUserApprovalContract>
    {
        public ShopUserApprovalValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
        }
    }
}