using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders;
using FluentValidation;

namespace Delivery.Shop.Domain.Validators.OrderManagement
{
    public class ShopOrderStatusQueryValidator : AbstractValidator<ShopOrderStatusQueryContract>
    {
        public ShopOrderStatusQueryValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty(). NotNull().WithMessage(" Order id must be provided.");
        }
    }
}