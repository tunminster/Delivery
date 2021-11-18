using System.Collections.Generic;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts
{
    public record StorePagedContract : PagedContract<StoreContract>;
}