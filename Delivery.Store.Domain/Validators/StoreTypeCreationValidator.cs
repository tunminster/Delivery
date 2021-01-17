using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreTypeCreations;
using FluentValidation;

namespace Delivery.Store.Domain.Validators
{
    public class StoreTypeCreationValidator : AbstractValidator<StoreTypeCreationContract>
    {
        public StoreTypeCreationValidator()
        {
            RuleFor(x => x.StoreTypeName).NotNull().NotEmpty().WithMessage("Store type name must be provided.");
        }
    }
}