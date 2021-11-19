using Delivery.Domain.Contracts.V1.RestContracts;

namespace Delivery.Order.Domain.Contracts.RestContracts
{
    public record OrderPagedContract : PagedContract<OrderManagementContract>;
}