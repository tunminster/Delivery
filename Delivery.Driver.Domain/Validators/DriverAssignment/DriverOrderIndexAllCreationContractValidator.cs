using System;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators.DriverAssignment
{
    public class DriverOrderIndexAllCreationContractValidator : AbstractValidator<DriverOrderIndexAllCreationContract>
    {
        public DriverOrderIndexAllCreationContractValidator()
        {
            RuleFor(x => x.CreateDate).GreaterThan(DateTimeOffset.UtcNow.AddYears(-1)).WithMessage($"Created date must be greater than {DateTimeOffset.UtcNow.AddYears(-1)}.");
        }
    }
}