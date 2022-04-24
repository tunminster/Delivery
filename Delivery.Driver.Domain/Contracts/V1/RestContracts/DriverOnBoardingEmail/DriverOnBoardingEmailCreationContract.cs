namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOnBoardingEmail
{
    /// <summary>
    ///  Driver on boarding email creation contract
    /// </summary>
    public record DriverOnBoardingEmailCreationContract
    {
        /// <summary>
        ///  Email address
        /// </summary>
        /// <example>{{email}}</example>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        ///  Driver name
        /// </summary>
        /// <example>{{name}}</example>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>
        ///  On boarding link
        /// </summary>
        /// <example>{{onBoardingLink}}</example>
        public string OnBoardingLink { get; init; } = string.Empty;
    }
}