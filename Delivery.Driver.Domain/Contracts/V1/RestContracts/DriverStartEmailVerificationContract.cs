namespace Delivery.Driver.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Driver email verification
    /// </summary>
    public record DriverStartEmailVerificationContract
    {
        /// <summary>
        ///  Driver's full name
        /// </summary>
        public string FullName { get; init; } = string.Empty;

        /// <summary>
        ///  Email address
        /// </summary>
        public string Email { get; init; } = string.Empty;
    }
}