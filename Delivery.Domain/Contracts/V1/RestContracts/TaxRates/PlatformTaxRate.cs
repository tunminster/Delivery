using System.Collections.Generic;

namespace Delivery.Domain.Contracts.V1.RestContracts.TaxRates
{
    public record PlatformTaxRate
    {
        public List<TaxRateContract> TaxRates { get; init; }
    }
}