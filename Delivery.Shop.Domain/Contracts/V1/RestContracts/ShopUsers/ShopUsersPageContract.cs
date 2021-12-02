using Delivery.Domain.Contracts.V1.RestContracts;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopUsers
{
    public record ShopUsersPageContract : PagedContract<ShopUserContract>;
}