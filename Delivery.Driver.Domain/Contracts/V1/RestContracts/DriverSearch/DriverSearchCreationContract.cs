namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverSearch
{
    /// <summary>
    ///  Driver search creation contract
    /// </summary>
    public record DriverSearchCreationContract
    {
        /// <summary>
        ///  Service area latitude
        /// </summary>
        public double Latitude { get; init; }
        
        /// <summary>
        ///  Service area longitude
        /// </summary>
        public double Longitude { get; init; }

        /// <summary>
        ///  Distance 
        /// </summary>
        public string Distance { get; init; } = "10";
        
        /// <summary>
        ///  Current page
        /// </summary>
        public int Page { get; init; }
        
        /// <summary>
        ///  Page size
        /// </summary>
        public int PageSize { get; init; }
    }
}