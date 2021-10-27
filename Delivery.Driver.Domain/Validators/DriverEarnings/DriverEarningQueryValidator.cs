using System;
using Delivery.Driver.Domain.Contracts.V1.Enums.DriverEarnings;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverEarnings;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators.DriverEarnings
{
    public class DriverEarningQueryValidator : AbstractValidator<DriverEarningQueryContract>
    {
        public DriverEarningQueryValidator()
        {
            RuleFor(x => x.DriverEarningFilter).NotEqual(DriverEarningFilter.None).WithMessage($"{nameof(DriverEarningFilter)} must be provided.");
            RuleFor(x => x.Year).NotEqual(0).WithMessage($"{nameof(DriverEarningQueryContract.Year)} must be provided");
            RuleFor(x => x.Month).NotEqual(0).WithMessage($"{nameof(DriverEarningQueryContract.Month)} must be provided");
        }
    }
}