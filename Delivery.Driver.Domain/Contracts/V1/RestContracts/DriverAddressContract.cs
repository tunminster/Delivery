namespace Delivery.Driver.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Driver address contract
    /// </summary>
    public record DriverAddressContract
    {
        /// <summary>
        ///  Address line 1
        /// </summary>
        public string AddressLine1 { get; init; } = string.Empty;
        
        /// <summary>
        ///  Address line 2
        /// <example>{{addressLine2}}</example>
        /// </summary>
        public string AddressLine2 { get; init; } = string.Empty;
        
        /// <summary>
        ///  City
        /// <example>{{city}}</example>
        /// </summary>
        public string City { get; init; } = string.Empty;
        
        /// <summary>
        ///  County
        /// <example>{{county}}</example>
        /// </summary>
        public string County { get; init; } = string.Empty;
        
        /// <summary>
        ///  Country
        /// <example>{{country}}</example>
        /// </summary>
        public string Country { get; init; } = string.Empty;
    }
    
    
}

