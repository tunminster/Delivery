using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators
{
    public class DriverCreationValidator : AbstractValidator<DriverCreationContract>
    {
        public DriverCreationValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().NotNull().WithMessage("Driver full name must be provided.");
            RuleFor(x => x.EmailAddress).NotEmpty().NotNull().WithMessage("Email address must be provided.");
            RuleFor(x => x.BankName).NotEmpty().NotNull().WithMessage("Bank name must be provided.");
        }
    }
}