using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators.DriverProfile
{
    public class DriverServiceAreaValidator : AbstractValidator<DriverServiceAreaUpdateContract>
    {
        public DriverServiceAreaValidator()
        {
            RuleFor(x => x.ServiceArea).NotEmpty().NotNull().WithMessage("Service area must be provided.");
            RuleFor(x => x.Latitude).NotEmpty().NotNull().WithMessage("Latitude must be provided.");
            RuleFor(x => x.Longitude).NotEmpty().NotNull().WithMessage("Longitude must be provided.");
            RuleFor(x => x.Radius).NotEmpty().NotNull().WithMessage("Radius must be provided.");
        }
    }
}