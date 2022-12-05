using Delivery.Managements.Domain.Contracts.V1.RestContracts.Coupon;
using FluentValidation;

namespace Delivery.Managements.Domain.Validators.Coupon;

public class CouponManagementCreationValidator : AbstractValidator<CouponManagementCreationContract>
{
    public CouponManagementCreationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().NotNull().WithMessage($"{nameof(CouponManagementCreationContract.Name)} must be provided.");
    }
}