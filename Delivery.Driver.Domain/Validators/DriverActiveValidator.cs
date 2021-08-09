using System;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverActive;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators
{
    public class DriverActiveValidator : AbstractValidator<DriverActiveCreationContract>
    {
        public DriverActiveValidator(string username)
        {
            RuleFor(x => x.Username).NotEmpty().NotNull().WithMessage("Username must be provided.");
            RuleFor(x => x.Username).Must(x => string.Equals(x, username, StringComparison.CurrentCultureIgnoreCase)).WithMessage("Username must be authenticated.");
        }
    }
}