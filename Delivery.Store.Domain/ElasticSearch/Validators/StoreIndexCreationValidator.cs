using Delivery.Store.Domain.ElasticSearch.Contracts.V1.RestContracts.StoreIndexing;
using FluentValidation;

namespace Delivery.Store.Domain.ElasticSearch.Validators
{
    public class StoreIndexCreationValidator : AbstractValidator<StoreIndexCreationContract>
    {
        public StoreIndexCreationValidator()
        {
            RuleFor(x => x.StoreId).NotNull().NotEmpty().WithMessage("Store id name must be provided.");
        }
    }
}