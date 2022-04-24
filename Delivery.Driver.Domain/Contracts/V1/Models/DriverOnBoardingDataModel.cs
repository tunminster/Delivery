namespace Delivery.Driver.Domain.Contracts.V1.Models
{
    public record DriverOnBoardingDataModel
    {
        /// <summary>
        ///  Driver name
        /// </summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>
        ///  Subject
        /// </summary>
        public string Subject { get; init; } = string.Empty;

        /// <summary>
        ///  On boarding link
        /// </summary>
        public string OnBoardingLink { get; init; } = string.Empty;
    }
}