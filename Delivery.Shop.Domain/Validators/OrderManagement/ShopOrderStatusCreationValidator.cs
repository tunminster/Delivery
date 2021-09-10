using Delivery.Database.Enums;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;
using FluentValidation;

namespace Delivery.Shop.Domain.Validators.OrderManagement
{
    public class ShopOrderStatusCreationValidator : AbstractValidator<ShopOrderStatusCreationContract>
    {
        public ShopOrderStatusCreationValidator()
        {
            RuleFor(x => IsValid(x.OrderStatus)).Must(x => x).WithMessage($"Status must be {nameof(Database.Enums.OrderStatus.Accepted)} or {nameof(Database.Enums.OrderStatus.Rejected)}");
        }

        private static bool IsValid(OrderStatus orderStatus)
        {
            return orderStatus is OrderStatus.Accepted or OrderStatus.Rejected;
        }
    }
}