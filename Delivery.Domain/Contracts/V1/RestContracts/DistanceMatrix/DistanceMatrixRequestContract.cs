namespace Delivery.Domain.Contracts.V1.RestContracts.DistanceMatrix
{
    /// <summary>
    ///  Distance matrix request contract
    /// </summary>
    public record DistanceMatrixRequestContract
    {
        /// <summary>
        ///  Source latitude
        /// </summary>
        /// <example>{{sourceLatitude}}</example>
        public double SourceLatitude { get; init; }
        
        /// <summary>
        ///  Source longitude
        /// </summary>
        /// <example>{{sourceLongitude}}</example>
        public double SourceLongitude { get; init; }
        
        /// <summary>
        ///  Destination latitude
        /// </summary>
        /// <example>{{destinationLatitude}}</example>
        public double DestinationLatitude { get; init; }
        
        /// <summary>
        ///  Destination longitude
        /// </summary>
        /// <example>{{destinationLongitude}}</example>
        public double DestinationLongitude { get; init; }
    }
}