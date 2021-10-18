namespace Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings.Driver
{
    /// <summary>
    ///  Driver on boarding status contract
    /// </summary>
    public record DriverOnBoardingStatusContract
    {
        /// <summary>
        ///  Account number
        /// </summary>
        /// <example>{{accountNumber}}</example>
        public string AccountNumber { get; init; } = string.Empty;
    }
}