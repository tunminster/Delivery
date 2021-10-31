namespace Delivery.Domain.Contracts.V1.RestContracts.TaxRates
{
    /// <summary>
    ///  This contract contains tax rate
    /// </summary>
    public record TaxRateContract
    {
        /// <summary>
        ///  State name
        /// </summary>
        /// <example>{{stateName}}</example>
        public string StateName { get; init; } = string.Empty;

        /// <summary>
        ///  State code
        /// </summary>
        /// <example>{{stateCode}}</example>
        public string StateCode { get; init; } = string.Empty;
        
        /// <summary>
        ///  Tax rate
        /// </summary>
        /// <example>{{taxRate}}</example>
        public decimal TaxRate { get; init; }
    }
}