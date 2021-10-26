using Delivery.Shop.Domain.Contracts.V1.Enums;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopPaymentHistory;
using FluentValidation;

namespace Delivery.Shop.Domain.Validators.ShopPaymentHistory
{
    public class ShopPaymentHistoryValidator : AbstractValidator<ShopPaymentHistoryQueryContract>
    {
        public ShopPaymentHistoryValidator()
        {
            RuleFor(x => x.ShopPaymentHistoryFilter).NotEqual(ShopPaymentHistoryFilter.None).WithMessage($"{nameof(ShopPaymentHistoryFilter)} must be provided.");
            RuleFor(x => x.Year).NotEqual(0).WithMessage($"{nameof(ShopPaymentHistoryQueryContract.Year)} must be provided");
            RuleFor(x => x.Month).NotEqual(0).WithMessage($"{nameof(ShopPaymentHistoryQueryContract.Month)} must be provided");
        }
    }
}