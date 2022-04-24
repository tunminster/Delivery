namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOnBoardingEmail
{
    /// <summary>
    ///  Driver on boarding email contract
    /// </summary>
    public record DriverOnBoardingEmailContract
    {
        /// <summary>
        ///  Driver name
        /// </summary>
        /// <example>{{name}}</example>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        ///  Message
        /// </summary>
        public string Message { get; init; } = string.Empty;
    }
}