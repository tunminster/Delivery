using Delivery.Product.Domain.Contracts.V1.ModelContracts;
using Delivery.Product.Domain.Contracts.V1.RestContracts;
using Microsoft.AspNetCore.Http;

namespace Delivery.Product.Domain.Handlers.CommandHandlers
{
    public record ProductUpdateCommand(ProductUpdateContract ProductUpdateContract);
}