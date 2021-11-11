using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreUpdate;
using FluentValidation;

namespace Delivery.Store.Domain.Validators
{
    public class StoreUpdateValidator : AbstractValidator<StoreUpdateContract>
    {
        public StoreUpdateValidator()
        {
            RuleFor(x => x.StoreName).NotNull().NotEmpty().WithMessage("Store name must be provided.");
            RuleFor(x => x.AddressLine1).NotNull().NotEmpty().WithMessage("AddressLine1 must be provided.");
            RuleFor(x => x.StoreTypeId).NotNull().NotEmpty().WithMessage("Store type must be provided.");
            RuleFor(x => x.City).NotNull().NotEmpty().WithMessage("City must be provided.");
            RuleFor(x => x.Country).NotNull().NotEmpty().WithMessage("Country must be provided.");
            RuleFor(x => x.PostalCode).NotNull().NotEmpty().WithMessage("PostalCode must be provided.");
            
            RuleForEach(x => x.StoreOpeningHours).SetValidator(new StoreOpeningHourCreationValidator());
        }
    }
}