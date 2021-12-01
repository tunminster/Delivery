using Delivery.Domain.Contracts.V1.RestContracts;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts
{
    public record DriversPageContract : PagedContract<DriverContract>;
}