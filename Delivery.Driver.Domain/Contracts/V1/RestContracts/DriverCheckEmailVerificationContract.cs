namespace Delivery.Driver.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Driver verification check contract
    /// </summary>
    public record DriverCheckEmailVerificationContract
    {
        /// <summary>
        ///  Email address
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        ///  Verify code
        /// </summary>
        public string Code { get; init; } = string.Empty;
    }
}