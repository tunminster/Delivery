namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverStripeOnBoardingLink
{
    /// <summary>
    ///  Driver on boarding status
    /// </summary>
    public record DriverOnBoardingLinkStatusContract
    {
        /// <summary>
        ///  Status
        /// </summary>
        public bool Status { get; init; }

        /// <summary>
        ///  On boarding link 
        /// </summary>
        public string OnBoardingLink { get; init; } = string.Empty;
    }
}
