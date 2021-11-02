using System;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverEarnings;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators.DriverEarnings
{
    public class DriverEarningDetailsQueryValidator : AbstractValidator<DriverEarningDetailsQueryContract>
    {
        public DriverEarningDetailsQueryValidator()
        {
            RuleFor(x => x.StartDate).GreaterThan(DateTimeOffset.UtcNow.AddYears(-1)).WithMessage($"{nameof(DriverEarningDetailsQueryContract.StartDate)} must be in this year.");
            RuleFor(x => x.EndDate).LessThan(DateTimeOffset.UtcNow.AddYears(1)).WithMessage($"{nameof(DriverEarningDetailsQueryContract.EndDate)} must be in this year.");
            RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage(
                $"{nameof(DriverEarningDetailsQueryContract.EndDate)} must be greater than {nameof(DriverEarningDetailsQueryContract.StartDate)} in this year.");
        }
    }
}