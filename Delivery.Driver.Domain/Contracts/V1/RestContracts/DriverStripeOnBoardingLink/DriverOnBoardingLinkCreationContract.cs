namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverStripeOnBoardingLink
{
    /// <summary>
    ///  The contract that used to create on boarding link 
    /// </summary>
    public record DriverOnBoardingLinkCreationContract
    {
        /// <summary>
        ///  Driver email
        /// </summary>
        public string EmailAddress { get; init; } = string.Empty;
        
    }
}