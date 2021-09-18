using System;
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
            RuleFor(x => IsValid(x.PickupTime)).Must(x => x)
                .WithMessage($"Pickup time should be greater than today datetime");
        }

        private static bool IsValid(OrderStatus orderStatus)
        {
            return orderStatus is OrderStatus.Accepted or OrderStatus.Rejected or OrderStatus.Preparing or OrderStatus.DeliveryOnWay or OrderStatus.Ready;
        }

        private static bool IsValid(DateTimeOffset? pickupTime)
        {
            if (pickupTime == null)
            {
                return true;
            }

            return pickupTime > DateTimeOffset.UtcNow;
        }
    }
}