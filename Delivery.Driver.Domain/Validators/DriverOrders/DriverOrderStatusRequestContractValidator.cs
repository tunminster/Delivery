using System;
using Delivery.Database.Enums;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators.DriverOrders
{
    public class DriverOrderStatusRequestContractValidator : AbstractValidator<DriverOrderStatusRequestContract>
    {
        public DriverOrderStatusRequestContractValidator()
        {
            RuleFor(x => x.FromDate).NotEmpty()
                .GreaterThan(x => DateTimeOffset.UtcNow.AddYears(-1))
                .WithMessage("From date must be valid");
            RuleFor(x => x.DriverOrderStatus)
                .NotEqual(DriverOrderStatus.None)
                .WithMessage("Driver order status must be provided");
            RuleFor(x => x.DriverOrderStatus)
                .NotEqual(DriverOrderStatus.Accepted)
                .WithMessage("Driver order status must be valid");
            RuleFor(x => x.DriverOrderStatus)
                .NotEqual(DriverOrderStatus.Rejected)
                .WithMessage("Driver order status must be valid");
        }
    }
}