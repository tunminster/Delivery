using Delivery.Domain.Contracts.V1.RestContracts;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts
{
    public record OrderAdminManagementPagedContract : PagedContract<OrderAdminManagementContract>;
}