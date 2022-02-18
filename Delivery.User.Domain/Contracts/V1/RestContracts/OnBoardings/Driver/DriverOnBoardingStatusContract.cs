namespace Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings.Driver
{
    /// <summary>
    ///  Driver on boarding status contract
    /// </summary>
    public record DriverOnBoardingStatusContract
    {
        /// <summary>
        ///  Display message
        /// </summary>
        /// <example>{{message}}</example>
        public string Message { get; init; } = string.Empty;
    }
}