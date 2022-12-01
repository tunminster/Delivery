using Delivery.Store.Domain.ElasticSearch.Contracts.V1.RestContracts.StoreRemove;
using FluentValidation;

namespace Delivery.Store.Domain.ElasticSearch.Validators
{
    public class StoreIndexRemoveValidator : AbstractValidator<StoreDeletionContract>
    {
        public StoreIndexRemoveValidator()
        {
            RuleFor(x => x.StoreId).NotNull().NotEmpty().WithMessage("Store id name must be provided.");
        }
    }
}