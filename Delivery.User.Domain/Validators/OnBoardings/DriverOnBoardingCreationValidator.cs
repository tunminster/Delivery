using Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings;
using Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings.Driver;
using FluentValidation;

namespace Delivery.User.Domain.Validators.OnBoardings
{
    public class DriverOnBoardingCreationValidator : AbstractValidator<DriverOnBoardingCreationContract>
    {
        public DriverOnBoardingCreationValidator()
        {
            RuleFor(x => x.FirstName).NotNull().NotEmpty().WithMessage("First name must be provided.");
            RuleFor(x => x.LastName).NotNull().NotEmpty().WithMessage("Last name must be provided.");
            RuleFor(x => x.Email).NotNull().NotEmpty().WithMessage("Email must be provided.");
            RuleFor(x => x.JobTitle).NotNull().NotEmpty().WithMessage("Job title must be provided.");
            RuleFor(x => x.DateOfBirth).NotNull().NotEmpty().WithMessage("Dob must be provided.");
            RuleFor(x => x.SocialSecurityNumber).NotNull().NotEmpty().WithMessage("Social security number must be provided.");
            
            RuleFor(x => x.Address.AddressLine1).NotNull().NotEmpty().WithMessage("AddressLine1 must be provided.");
            RuleFor(x => x.Address.City).NotNull().NotEmpty().WithMessage("City must be provided.");
            RuleFor(x => x.Address.PostalCode).NotNull().NotEmpty().WithMessage("Postal code must be provided.");
            
            RuleFor(x => x.BankAccount.AccountNumber).NotNull().NotEmpty().WithMessage("Account number must be provided.");
            RuleFor(x => x.BankAccount.RoutingNumber).NotNull().NotEmpty().WithMessage("Routing number must be provided.");
        }
    }
}