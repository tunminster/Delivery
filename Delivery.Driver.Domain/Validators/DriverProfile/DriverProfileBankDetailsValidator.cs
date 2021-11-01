using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators.DriverProfile
{
    public class DriverProfileBankDetailsValidator : AbstractValidator<DriverProfileBankDetailsContract>
    {
        public DriverProfileBankDetailsValidator()
        {
            RuleFor(x => x.DriverId).NotEmpty().NotNull().WithMessage("Driver id must be provided.");
            RuleFor(x => x.BankName).NotEmpty().NotNull().WithMessage("Bank name must be provided.");
            RuleFor(x => x.AccountNumber).NotEmpty().NotNull().WithMessage("Account number must be provided.");
            RuleFor(x => x.RoutingNumber).NotEmpty().NotNull().WithMessage("Routing number must be provided.");
        }
    }
}