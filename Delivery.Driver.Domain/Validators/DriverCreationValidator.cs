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
            RuleFor(x => x.Password).NotEmpty().NotNull().WithMessage("Password must be valid.");
            RuleFor(x => x.Password).MinimumLength(6).WithMessage("Password must be at least 6 characters");
            RuleFor(x => x.Radius).NotNull().GreaterThanOrEqualTo(5)
                .WithMessage("Radius area must be covered 5 miles minimum.")
                .LessThanOrEqualTo(20).WithMessage("Radius area must be covered maximum 20 miles.");
            RuleFor(x => x.ServiceArea).NotEmpty().NotNull().WithMessage("Must provide valid service area");
            RuleFor(x => x.Latitude).NotEqual(0).WithMessage("Must provide valid latitude.");
            RuleFor(x => x.Longitude).NotEqual(0).WithMessage("Must provide valid longitude.");

        }
    }
}