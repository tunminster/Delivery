using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreImageCreations;
using Microsoft.AspNetCore.Http;

namespace Delivery.Store.Domain.Handlers.CommandHandlers.StoreImageCreation
{
    public record StoreImageCreationCommand(StoreImageCreationContract StoreImageCreationContract);
}