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
            RuleFor(x => x.DateCreatedFrom).GreaterThan(DateTimeOffset.Now.AddYears(-3)).WithMessage($"{nameof(DriverEarningQueryContract.DateCreatedFrom)} must be provided and greater than last 3 years.");
        }
    }
}