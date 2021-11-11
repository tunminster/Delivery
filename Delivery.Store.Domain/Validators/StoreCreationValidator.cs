using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;
using FluentValidation;

namespace Delivery.Store.Domain.Validators
{
    public class StoreCreationValidator : AbstractValidator<StoreCreationContract>
    {
        public StoreCreationValidator()
        {
            RuleFor(x => x.StoreName).NotNull().NotEmpty().WithMessage("Store name must be provided.");
            RuleFor(x => x.AddressLine1).NotNull().NotEmpty().WithMessage("AddressLine1 must be provided.");
            RuleFor(x => x.StoreTypeId).NotNull().NotEmpty().WithMessage("Store type must be provided.");
            RuleFor(x => x.City).NotNull().NotEmpty().WithMessage("City must be provided.");
            RuleFor(x => x.Country).NotNull().NotEmpty().WithMessage("Country must be provided.");
            RuleFor(x => x.PostalCode).NotNull().NotEmpty().WithMessage("PostalCode must be provided.");

            RuleFor(x => x.StoreUser.EmailAddress).NotNull().NotEmpty()
                .WithMessage("Store user email must be provided.")
                .EmailAddress().WithMessage("A valid email is required");

            RuleForEach(x => x.StoreOpeningHours).SetValidator(new StoreOpeningHourCreationValidator());
            
            RuleFor(x => x.StoreUser.Password).MinimumLength(6).WithMessage("Password must be at least 6 characters");
            RuleFor(x => x.StoreUser.ConfirmPassword).NotEmpty().NotNull().WithMessage("Confirm password must be provided");
        }
    }
}