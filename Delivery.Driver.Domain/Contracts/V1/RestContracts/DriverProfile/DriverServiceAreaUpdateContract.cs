namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile
{
    /// <summary>
    ///  Driver service area update contract
    /// </summary>
    public record DriverServiceAreaUpdateContract
    {
        /// <summary>
        ///  Driver's service area that he/she intends to work.
        /// <example>{{serviceArea}}</example>
        /// </summary>
        public string ServiceArea { get; init; } = string.Empty;
        
        /// <summary>
        ///  Radius that would cover to give delivery service
        /// <example>{{radius}}</example>
        /// </summary>
        public int Radius { get; init; }
        
        /// <summary>
        ///  Service area latitude that driver wants to work
        /// <example>{{latitude}}</example>
        /// </summary>
        public double Latitude { get; set; }
        
        /// <summary>
        ///  Service area longitude that driver wants to work
        /// <example>{{longitude}}</example>
        /// </summary>
        public double Longitude { get; set; }
    }
}